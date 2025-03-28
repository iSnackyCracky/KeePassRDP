/*
 *  Copyright (C) 2018 - 2025 iSnackyCracky, NETertainer
 *
 *  This file is part of KeePassRDP.
 *
 *  KeePassRDP is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  KeePassRDP is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with KeePassRDP.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Security;
using KeePassLib.Utility;
using KeePassRDP.Commands;
using KeePassRDP.Extensions;
using KeePassRDP.Generator;
using KeePassRDP.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace KeePassRDP
{
    internal class KprConnectionManager : IDisposable
    {
        private class TaskWithCancellationToken : Task
        {
            public bool IsCancellationRequested { get { return _cancellationTokenSource.IsCancellationRequested; } }

            private readonly CancellationTokenSource _cancellationTokenSource;

            public TaskWithCancellationToken(Action<object> action, object state, CancellationTokenSource cancellationTokenSource) : base(
                action,
                state,
                cancellationTokenSource.Token,
                TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness | TaskCreationOptions.LongRunning)
            {
                _cancellationTokenSource = cancellationTokenSource;
                Start();
            }

            public void Cancel()
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                    _cancellationTokenSource.Cancel();
            }

            public new void Start()
            {
                Start(TaskScheduler.Default);
            }

            public new Task ContinueWith(Action<Task> action)
            {
                return ContinueWith(
                    action,
                    _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested ? _cancellationTokenSource.Token : CancellationToken.None,
                    TaskContinuationOptions.PreferFairness,
                    TaskScheduler.Default);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (!IsCompleted)
                    {
                        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                            _cancellationTokenSource.Cancel();
                        try
                        {
                            Wait(TimeSpan.FromSeconds(10));
                        }
                        catch
                        {
                        }
                    }

                    if (_cancellationTokenSource != null)
                        _cancellationTokenSource.Dispose();

                    if (IsCompleted)
                        base.Dispose(disposing);
                }
                else
                    base.Dispose(disposing);
            }
        }

        public int Count { get { return _tasks.Count; } }

        public bool IsCompleted
        {
            get
            {
                try
                {
                    return _tasks.Values.All(x =>
                    {
                        try
                        {
                            return x.IsCompleted;
                        }
                        catch
                        {
                            return true;
                        }
                    });
                }
                catch
                {
                    return true;
                }
            }
        }

        internal Lazy<KprCredentialPicker> CredentialPicker { get { return _credentialPicker; } }

        private readonly IPluginHost _host;
        private readonly KprConfig _config;
        private readonly Lazy<KprCredentialManager<KprCredential>> _credManager;
        private readonly Lazy<KprCredentialPicker> _credentialPicker;
        private readonly ConcurrentDictionary<string, TaskWithCancellationToken> _tasks;

        public KprConnectionManager(IPluginHost host, KprConfig config, Lazy<KprCredentialManager<KprCredential>> credManager)
        {
            _host = host;
            _config = config;
            _credManager = credManager;
            _credentialPicker = new Lazy<KprCredentialPicker>(() => new KprCredentialPicker(_host, _config), LazyThreadSafetyMode.ExecutionAndPublication);
            _tasks = new ConcurrentDictionary<string, TaskWithCancellationToken>(4, 0);
        }

        public bool Wait(int seconds = Timeout.Infinite)
        {
            try
            {
                return Task.WaitAll(_tasks.Values.Where(x =>
                {
                    try
                    {
                        return !x.IsCompleted;
                    }
                    catch
                    {
                        return false;
                    }
                }).ToArray(), TimeSpan.FromSeconds(seconds));
            }
            catch
            {
                return IsCompleted;
            }
        }

        public void Cancel(bool killTasks = true)
        {
            if (killTasks)
                foreach (var task in _tasks.Values.Where(x =>
                {
                    try
                    {
                        return !x.IsCompleted && !x.IsCancellationRequested;
                    }
                    catch
                    {
                        return false;
                    }
                }))
                    try
                    {
                        task.Cancel();
                    }
                    catch { }

            if (SecureDesktop.IsValueCreated)
                SecureDesktop.Instance.Cancel();
        }

        public void Dispose()
        {
            Cancel();
            Wait(5);

            _tasks.Clear();

            if (_credentialPicker.IsValueCreated)
                _credentialPicker.Value.Dispose();

            if (SecureDesktop.IsValueCreated)
            {
                try
                {
                    var hOldDesktop = NativeMethods.GetThreadDesktop(NativeMethods.GetCurrentThreadId());
                    if (!SecureDesktop.IsInput(hOldDesktop))
                        NativeMethods.SwitchDesktop(hOldDesktop);
                }
                catch { }
            }
        }

        public void ConnectRDPtoKeePassEntry(bool tmpMstscUseAdmin = false, bool tmpUseCreds = false, bool shadowSession = false)
        {
            if (!Util.IsValid(_host))
                return;

            var mainForm = _host.MainWindow;
            var selectedEntries = mainForm.GetSelectedEntries();

            var parentGroups = selectedEntries
                .Where(entry => !entry.Strings.GetSafe(PwDefs.UrlField).IsEmpty)
                .GroupBy(entry => entry.ParentGroup.Uuid, EqualityComparer<PwUuid>.Default);

            var kpCta = _config.KeePassConnectToAll;
            if (!kpCta)
                parentGroups = parentGroups.Skip(parentGroups.Count() - 1);

            var totalCount = parentGroups.Aggregate(0, (a, b) => a + b.Count());

            var postfix = string.Format(KprResourceManager.Instance["{0} entr" + (totalCount == 1 ? "y" : "ies") + " of {1} selected."], totalCount, selectedEntries.Length);
            var connectingTo = string.Format(" {0} ", KprResourceManager.Instance["connecting to"]);
            var skipped = string.Format(" {0}", KprResourceManager.Instance["skipped"]);

            mainForm.SetStatusEx(string.Format("{0}{1}{2}", Util.KeePassRDP, connectingTo, postfix));

            var kprCpcg = _config.CredPickerCustomGroup;
            var kprCptr = _config.CredPickerTriggerRecursive;
            var kprCpis = _config.CredPickerIncludeSelected;
            var kpSrr = _config.KeePassShowResolvedReferences;
            var kpScf = _config.KeePassSprCompileFlags;
            var mstscSd = _config.MstscSecureDesktop;

            foreach (var parentGroup in parentGroups)
            {
                var entries = parentGroup.AsEnumerable();
                var count = entries.Count();

                if (count == 0)
                    continue;

                if (!kpCta && count > 1)
                {
                    entries = entries.Skip(count - 1);
                    count = 1;
                }

                if (totalCount > 1)
                    mainForm.SetStatusEx(string.Format("{0}{1}{2} / {3}", Util.KeePassRDP, connectingTo, count, postfix));

                PwEntry credEntry = null;
                PwEntry unresolvedCredEntry = null;

                var skippedEntries = new List<PwEntry>();

                // Connect to RDP using credentials from KeePass, skipping entries with no credentials.
                if (tmpUseCreds)
                {
                    var credPick = _credentialPicker.Value;
                    var usedCount = 0;

                    // Fill KprCredentialPicker with selected entries.
                    foreach (var entry in entries)
                        using (var entrySettings = entry.GetKprSettings(true) ?? KprEntrySettings.Empty)
                            if (entrySettings.UseCredpicker && (Util.InRdpSubgroup(entry, kprCpcg, kprCptr) || entrySettings.CpGroupUUIDs.Any()))
                            {
                                credPick.AddEntry(entry, entrySettings, kprCpcg, kprCptr);
                                usedCount++;
                            }
                            else
                                skippedEntries.Add(entry);

                    if (usedCount > 0)
                    {
                        if (totalCount > 1 && usedCount != count)
                            mainForm.SetStatusEx(string.Format("{0}{1}{2} ({3}{4}) / {5}", Util.KeePassRDP, connectingTo, usedCount, count - usedCount, skipped, postfix));

                        // Get result from KprCredentialPickerForm.
                        try
                        {
                            credEntry = credPick.GetCredentialEntry(kprCpis, kpSrr);
                        }
                        catch (OperationCanceledException)
                        {
                            continue;
                        }
                        finally
                        {
                            if (_config.CredPickerSecureDesktop && SecureDesktop.IsInput())
                                SecureDesktop.Instance.SwitchDesktop(true);
                            credPick.Reset();
                        }

                        if (credEntry == null)
                        {
                            // Skip group if no credential entries where found and no selected entry has at least a username.
                            if (!entries.Any(entry => !entry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty))
                                continue;
                        }
                        else if (Util.IsEntryIgnored(credEntry))
                            credEntry = null;
                        else
                        {
                            unresolvedCredEntry = credEntry;
                            credEntry = credEntry.GetResolvedReferencesEntry(new SprContext(credEntry, mainForm.ActiveDatabase, kpScf)
                            {
                                ForcePlainTextPasswords = true // true is default, PwDefs.PasswordField is replaced with PwDefs.HiddenPassword during SprEngine.Compile otherwise.
                            });
                        }
                    }
                }

                using (var credEntrySettings = unresolvedCredEntry == null ? KprEntrySettings.Empty : unresolvedCredEntry.GetKprSettings(true) ?? KprEntrySettings.Empty)
                    foreach (var connPwEntry in entries)
                    {
                        var ctx = new SprContext(connPwEntry, mainForm.ActiveDatabase, kpScf)
                        {
                            ForcePlainTextPasswords = true
                        };

                        var hostname = SprEngine.Compile(connPwEntry.Strings.ReadSafe(PwDefs.UrlField), ctx);

                        if (string.IsNullOrWhiteSpace(hostname))
                            continue;

                        hostname = hostname.Trim();

                        var port = string.Empty;
                        var uri = Util.ParseURL(hostname);

                        if (uri != null && uri.HostNameType != UriHostNameType.Unknown)
                        {
                            hostname = uri.Host;
                            if (!uri.IsDefaultPort && uri.Port != Util.DefaultRdpPort)
                                port = string.Format(":{0}", uri.Port);
                        }
                        else
                        {
                            VistaTaskDialog.ShowMessageBoxEx(
                                string.Format(KprResourceManager.Instance["The URL/target '{0}' of the selected entry could not be parsed."], hostname),
                                null,
                                string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Error),
                                VtdIcon.Error,
                                _host.MainWindow,
                                null, 0, null, 0);
                            continue;
                        }

                        var taskUuid = connPwEntry.Uuid.ToHexString();

                        using (var connEntrySettings = connPwEntry.GetKprSettings(true) ?? KprEntrySettings.Empty)
                        using (var entrySettings = connEntrySettings.Inherit(credEntrySettings))
                        {
                            KprCredential cred = null;
                            KprCredential gatewayCred = null;

                            PwEntry retryCredEntry = null;
                            ProtectedString retryUsername = null;
                            SecureString retryPassword = null;

                            if (string.IsNullOrWhiteSpace(hostname))
                            {
                                if (entrySettings.RdpFile == null || string.IsNullOrWhiteSpace(entrySettings.RdpFile.FullAddress))
                                {
                                    if (totalCount == 1)
                                        VistaTaskDialog.ShowMessageBoxEx(
                                            KprResourceManager.Instance["Cannot connect to empty hostname."],
                                            null,
                                            Util.KeePassRDP,
                                            VtdIcon.Information,
                                            _host.MainWindow,
                                            null, 0, null, 0);
                                    continue;
                                }
                                else
                                {
                                    hostname = entrySettings.RdpFile.FullAddress;
                                    if (string.IsNullOrWhiteSpace(port) && entrySettings.RdpFile.ServerPort > 0)
                                        port = string.Format(":{0}", entrySettings.RdpFile.ServerPort);
                                }
                            }

                            var retryOnce = entrySettings.RetryOnce;
                            var forceLocalUser = entrySettings.ForceLocalUser;
                            var forceUpn = entrySettings.ForceUpn;
                            KprSessionListForm.KprSessionListResult sessionListResult = null;

                            // Connect to RDP using credentials from KeePass, skipping entries with no credentials.
                            if (tmpUseCreds)
                            {
                                // Use result from KprCredentialPicker or fallback to credentials from selected entry.
                                var shownInPicker = entrySettings.UseCredpicker &&
                                    (Util.InRdpSubgroup(connPwEntry, kprCpcg, kprCptr) || entrySettings.CpGroupUUIDs.Any());
                                var tmpEntry = credEntry != null && shownInPicker ?
                                    credEntry :
                                    entrySettings.Ignore || (credEntry != null && !shownInPicker) ?
                                        null :
                                            skippedEntries.Contains(connPwEntry) ?
                                                connPwEntry.GetResolvedReferencesEntry(ctx) :
                                                null;

                                if (tmpEntry == null)
                                    continue;

                                taskUuid = string.Format("{0}-{1}", taskUuid, tmpEntry.Uuid.ToHexString());

                                var username = tmpEntry.Strings.GetSafe(PwDefs.UserNameField);
                                // Do not connect to entry if username is empty.
                                if (username.IsEmpty)
                                {
                                    if (totalCount == 1)
                                        VistaTaskDialog.ShowMessageBoxEx(
                                            KprResourceManager.Instance["Username is required when connecting with credentials."],
                                            null,
                                            Util.KeePassRDP,
                                            VtdIcon.Information,
                                            _host.MainWindow,
                                            null, 0, null, 0);
                                    continue;
                                }

                                if (forceLocalUser)
                                    username = username.ForceLocalUser(hostname);

                                if (forceUpn)
                                    username = username.ForceUPN();

                                var password = tmpEntry.Strings.GetSafe(PwDefs.PasswordField);
                                // Do not connect to entry if password is empty.
                                /*if (password.IsEmpty)
                                    continue;*/

                                var securePassword = password.AsSecureString();

                                if (shadowSession)
                                {
                                    var tempTitle = SprEngine.Compile(connPwEntry.Strings.ReadSafe(PwDefs.TitleField), ctx);
                                    if (string.IsNullOrWhiteSpace(tempTitle))
                                        tempTitle = hostname;

                                    using (var sessionListForm = new KprSessionListForm(_config)
                                    {
                                        Text = string.Format("{0} | {1}", Util.KeePassRDP, tempTitle.Trim())
                                    })
                                    {
                                        sessionListForm.Computers.Add(hostname);
                                        using (var tempCred = new KprCredential(
                                            username,
                                            securePassword,
                                            hostname,
                                            NativeCredentials.CRED_TYPE.DOMAIN_PASSWORD))
                                        {
                                            _credManager.Value.Add(tempCred);

                                            var result = DialogResult.None;

                                            sessionListForm.StartPosition = FormStartPosition.CenterParent;
                                            sessionListForm.Location = Point.Empty;

                                            if (_config.CredPickerSecureDesktop)
                                            {
                                                using (var mrs = new ManualResetEventSlim(false))
                                                {
                                                    Task.Factory.StartNew(() =>
                                                    {
                                                        //SecureDesktop.Instance.Prepare();

                                                        var primaryScreen = Screen.FromControl(_host.MainWindow);
                                                        sessionListForm.StartPosition = FormStartPosition.CenterScreen;
                                                        sessionListForm.Location = primaryScreen.Bounds.Location;

                                                        SecureDesktop.Instance.Run(_ =>
                                                        {
                                                            try
                                                            {
                                                                result = sessionListForm.ShowDialog();
                                                            }
                                                            catch
                                                            {
                                                                result = DialogResult.None;
                                                                throw;
                                                            }
                                                            finally
                                                            {
                                                                mrs.Set();
                                                            }

                                                            return SecureDesktop.CompletedTask;
                                                        });
                                                    }, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

                                                    try
                                                    {
                                                        if (!mrs.Wait(TimeSpan.FromSeconds(900)))
                                                            throw new TimeoutException();

                                                        SecureDesktop.Instance.Wait(30); // Maybe wait forever?
                                                    }
                                                    catch (TimeoutException)
                                                    {
                                                        sessionListForm.Close();
                                                        result = DialogResult.None;
                                                    }
                                                    catch
                                                    {
                                                        result = DialogResult.None;
                                                        throw;
                                                    }
                                                    finally
                                                    {
                                                        if (SecureDesktop.IsInput())
                                                            SecureDesktop.Instance.SwitchDesktop(true);
                                                    }
                                                }
                                            }
                                            else
                                                result = sessionListForm.ShowDialog(_host.MainWindow);

                                            if (result != DialogResult.OK)
                                                continue;

                                            sessionListResult = sessionListForm.Results.LastOrDefault();
                                        }
                                    }

                                    if (sessionListResult == null || string.IsNullOrEmpty(sessionListResult.Shadow))
                                    {
                                        VistaTaskDialog.ShowMessageBoxEx(
                                            KprResourceManager.Instance["No session to shadow selected from the list."],
                                            null,
                                            string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Error),
                                            VtdIcon.Error,
                                            _host.MainWindow,
                                            null, 0, null, 0);
                                        continue;
                                    }

                                    taskUuid = string.Format("{0}-{1}", taskUuid, sessionListResult.Shadow);
                                }

                                if (entrySettings.RdpFile != null &&
                                    !string.IsNullOrWhiteSpace(entrySettings.RdpFile.Gatewayhostname) &&
                                    entrySettings.RdpFile.Promptcredentialonce)
                                    gatewayCred = new KprCredential(
                                        username,
                                        securePassword,
                                        entrySettings.RdpFile.Gatewayhostname,
                                        _config.CredVaultUseWindows ?
                                            NativeCredentials.CRED_TYPE.DOMAIN_PASSWORD :
                                            NativeCredentials.CRED_TYPE.GENERIC,
                                        _config.CredVaultTtl);

                                // Create new KprCredential.
                                cred = new KprCredential(
                                    username,
                                    securePassword,
                                    string.Format("TERMSRV/{0}", hostname.ToUpperInvariant()),
                                    _config.CredVaultUseWindows ?
                                        NativeCredentials.CRED_TYPE.DOMAIN_PASSWORD :
                                        NativeCredentials.CRED_TYPE.GENERIC,
                                    _config.CredVaultTtl);

                                if (retryOnce)
                                {
                                    var tmpUnresolvedCredEntry = unresolvedCredEntry ?? connPwEntry;
                                    if (tmpUnresolvedCredEntry != null)
                                    {
                                        var uncompiledUsername = tmpUnresolvedCredEntry.Strings.GetSafe(PwDefs.UserNameField);
                                        var uncompiledChars = uncompiledUsername.ReadChars();
                                        var uncompiledString = new string(uncompiledChars);
                                        if (uncompiledString.IndexOf("{TIMEOTP}", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                            uncompiledString.IndexOf("{HMACOTP}", StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            var tmpCredEntry = new PwEntry(false, false)
                                            {
                                                Uuid = credEntry.Uuid
                                            };
                                            tmpCredEntry.Strings.Set(PwDefs.UserNameField, uncompiledUsername.WithProtection(true));
                                            foreach (var kv in tmpUnresolvedCredEntry.Strings.Where(x => !x.Value.IsEmpty && (x.Key.StartsWith("TimeOtp-") || x.Key.StartsWith("HmacOtp-"))))
                                                tmpCredEntry.Strings.Set(kv.Key, kv.Value.WithProtection(true));
                                            retryCredEntry = tmpCredEntry;
                                        }
                                        MemoryUtil.SecureZeroMemory(uncompiledString);
                                        MemoryUtil.SecureZeroMemory(uncompiledChars);
                                    }
                                    retryUsername = username;
                                    retryPassword = securePassword;
                                }
                                else
                                    securePassword.Dispose();
                            }
                            else
                            {
                                if (shadowSession)
                                {

                                }

                                retryOnce = false;
                            }

                            taskUuid = MemUtil.ByteArrayToHexString(CryptoUtil.HashSha256(Encoding.Unicode.GetBytes(taskUuid)));

                            TaskWithCancellationToken oldTask = null;
                            try
                            {
                                if ((tmpUseCreds || _config.KeePassAlwaysConfirm) && _tasks.TryGetValue(taskUuid, out oldTask) && !oldTask.IsCompleted)
                                {
                                    if (_config.CredVaultTtl > 0)
                                    {
                                        if (cred != null)
                                            cred.IncreaseTTL(TimeSpan.MaxValue);
                                        if (gatewayCred != null)
                                            gatewayCred.IncreaseTTL(TimeSpan.MaxValue);
                                    }

                                    if (VistaTaskDialog.ShowMessageBoxEx(
                                        tmpUseCreds ?
                                            string.Format(KprResourceManager.Instance["Already connected with the same credentials to URL/target '{0}'."], hostname) :
                                            string.Format(KprResourceManager.Instance["Already connected to URL/target '{0}'."], hostname),
                                        KprResourceManager.Instance[KPRes.AskContinue],
                                        Util.KeePassRDP,
                                        VtdIcon.Information,
                                        _host.MainWindow,
                                        KprResourceManager.Instance[KPRes.YesCmd], 0,
                                        KprResourceManager.Instance[KPRes.NoCmd], 1) == 1)
                                    {
                                        if (cred != null)
                                        {
                                            cred.Dispose();
                                            cred = null;
                                        }
                                        if (gatewayCred != null)
                                        {
                                            gatewayCred.Dispose();
                                            gatewayCred = null;
                                        }

                                        continue;
                                    }

                                    if (_config.CredVaultTtl > 0)
                                    {
                                        if (cred != null)
                                            cred.ResetValidUntil();
                                        if (gatewayCred != null)
                                            gatewayCred.ResetValidUntil();
                                    }
                                }
                            }
                            catch (NullReferenceException) { }
                            catch (ObjectDisposedException) { }

                            var title = _config.MstscReplaceTitle ? SprEngine.Compile(connPwEntry.Strings.ReadSafe(PwDefs.TitleField), ctx) : string.Empty;

                            var cts = new CancellationTokenSource();
                            TaskWithCancellationToken newTask = null;
                            _tasks[taskUuid] = newTask = new TaskWithCancellationToken(thisTaskUuid =>
                            {
                                var localTaskUuid = (string)thisTaskUuid;
                                RdpFile rdpFile = null;

                                try
                                {
                                    var icommand = Command.CreateInstance(!string.IsNullOrWhiteSpace(_config.MstscExecutable) ? _config.MstscExecutable : typeof(MstscCommand).Name);

                                    if (icommand == null)
                                        throw new NullReferenceException("Command");

                                    var waitForRdpFile = false;

                                    if (icommand is IMstscCommand)
                                    {
                                        var command = icommand as IMstscCommand;

                                        command.HostPort = hostname + port;

                                        if (entrySettings.IncludeDefaultParameters)
                                        {
                                            if (tmpMstscUseAdmin || _config.MstscUseAdmin)
                                            {
                                                command.Admin = true;
                                                if (_config.MstscUseRestrictedAdmin)
                                                    command.RestrictedAdmin = true;
                                            }
                                            if (_config.MstscUsePublic)
                                                command.Public = true;
                                            if (_config.MstscUseRemoteGuard)
                                                command.RemoteGuard = true;
                                            if (_config.MstscUseFullscreen)
                                                command.Fullscreen = true;
                                            if (_config.MstscUseSpan)
                                                command.Span = true;
                                            if (_config.MstscUseMultimon)
                                                command.Multimon = true;
                                            if (_config.MstscWidth > 0)
                                                command.Width = _config.MstscWidth;
                                            if (_config.MstscHeight > 0)
                                                command.Height = _config.MstscHeight;
                                        }
                                        else if (tmpMstscUseAdmin)
                                            command.Admin = true;

                                        if (entrySettings.RdpFile != null)
                                        {
                                            rdpFile = new RdpFile(entrySettings.RdpFile);
                                            if (!string.IsNullOrWhiteSpace(hostname))
                                                rdpFile.FullAddress = hostname;
                                            if (!string.IsNullOrWhiteSpace(port))
                                                rdpFile.ServerPort = int.Parse(port.Substring(1));

                                            var thumbprint = _config.MstscSignRdpFiles;
                                            if (!string.IsNullOrWhiteSpace(thumbprint))
                                            {
                                                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

                                                try
                                                {
                                                    store.Open(OpenFlags.ReadOnly);

                                                    var cert = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false).OfType<X509Certificate2>().FirstOrDefault();

                                                    if (cert != null)
                                                        rdpFile.Sign(cert);

                                                    command.HostPort = null;
                                                }
                                                catch (CryptographicException)
                                                {
                                                }
                                                finally
                                                {
                                                    store.Close();
                                                }
                                            }

                                            command.Filename = rdpFile.ToString();
                                            waitForRdpFile = !string.IsNullOrWhiteSpace(command.Filename);
                                        }

                                        if (sessionListResult != null)
                                        {
                                            command.Shadow = sessionListResult.Shadow;
                                            command.Control = sessionListResult.Control;
                                            command.NoConsentPrompt = sessionListResult.NoConsentPrompt;
                                        }
                                    }

                                    var argumentsBuilder = new StringBuilder(icommand.ToString());

                                    foreach (var argument in entrySettings.MstscParameters
                                        .Select(x => SprEngine.Compile(x, ctx))
                                        .Where(x => !string.IsNullOrWhiteSpace(x))
                                        .Distinct(StringComparer.OrdinalIgnoreCase))
                                    {
                                        argumentsBuilder.Append(argument.Trim());
                                        argumentsBuilder.Append(' ');
                                    }

                                    if (argumentsBuilder.Length == 0)
                                    {
                                        if (cred != null)
                                        {
                                            cred.Dispose();
                                            cred = null;
                                        }
                                        if (gatewayCred != null)
                                        {
                                            gatewayCred.Dispose();
                                            gatewayCred = null;
                                        }

                                        throw new ArgumentOutOfRangeException("Command");
                                    }

                                    if (tmpUseCreds)
                                    {
                                        if (_config.MstscCleanupRegistry)
                                            Util.RemoveHintFromRegistry(hostname);
                                    }

                                    var rdpProcess = new ProcessStartInfo
                                    {
                                        UseShellExecute = false,
                                        WindowStyle = ProcessWindowStyle.Normal,
                                        FileName = icommand.Executable,
                                        Arguments = argumentsBuilder.ToString().TrimEnd(),
                                        ErrorDialog = true,
                                        ErrorDialogParentHandle = _host.MainWindow.Handle,
                                        LoadUserProfile = false,
                                        WorkingDirectory = Path.GetTempPath() // Environment.ExpandEnvironmentVariables("%TEMP%")
                                    };

                                    if (_config.MstscReplaceTitle)
                                        title = string.Format("{0} - {1}", string.IsNullOrWhiteSpace(title) ? hostname : title.Trim(), Util.KeePassRDP);

                                    // Add KprCredential to KprCredentialManager.
                                    if (cred != null)
                                        _credManager.Value.Add(cred);
                                    if (gatewayCred != null)
                                        _credManager.Value.Add(gatewayCred);

                                    // Start RDP / mstsc.exe.
                                    using (var process = new SecureProcess
                                    {
                                        StartInfo = rdpProcess
                                    })
                                    {
                                        if (!process.Start(_config))
                                            throw new InvalidOperationException("Process.Start");

                                        var tcs = new TaskCompletionSource<object>();

                                        cts.Token.Register(() =>
                                        {
                                            try
                                            {
                                                if (!process.HasExited)
                                                    process.Kill();
                                            }
                                            catch { }

                                            if (cred != null)
                                            {
                                                try
                                                {
                                                    cred.Dispose();
                                                    cred = null;
                                                }
                                                catch { }
                                            }
                                            if (gatewayCred != null)
                                            {
                                                try
                                                {
                                                    gatewayCred.Dispose();
                                                    gatewayCred = null;
                                                }
                                                catch { }
                                            }

                                            tcs.TrySetResult(null);

                                            try
                                            {
                                                TaskWithCancellationToken localOldTask = null;
                                                if (_tasks.TryRemove(localTaskUuid, out localOldTask) && localOldTask != newTask && !localOldTask.IsCompleted)
                                                    _tasks.TryAdd(localTaskUuid, localOldTask);
                                            }
                                            catch { }

                                            try
                                            {
                                                if (oldTask != null && !oldTask.IsCompleted)
                                                {
                                                    if (cts.IsCancellationRequested)
                                                        oldTask.Cancel();
                                                    else if (!_tasks.ContainsKey(localTaskUuid))
                                                    {
                                                        if (_tasks.TryAdd(localTaskUuid, oldTask))
                                                            oldTask = null;
                                                    }
                                                }
                                            }
                                            catch { }
                                        });

                                        var isMstscCommand = icommand is MstscCommand;

                                        SecureDesktop.Runner runner = _ => new Task(() =>
                                        {
                                            try
                                            {
                                                var inc = _config.CredVaultTtl > 0 && _config.CredVaultAdaptiveTtl && cred != null ?
                                                    TimeSpan.FromSeconds(Math.Ceiling(Math.Max(1, _config.CredVaultTtl / 2f))) : TimeSpan.Zero;

                                                var ttl = (int)Math.Max(1500, inc.TotalMilliseconds);

                                                // Wait a limited time for mstsc.exe window to appear, otherwise assume something went wrong.
                                                if (!process.WaitForExit(250) &&
                                                    !process.HasExited && (process.WaitForInputIdle(ttl) ||
                                                    (!process.HasExited && process.WaitForInputIdle(ttl))) &&
                                                    !process.HasExited)
                                                    for (var spins = ttl / 250; spins > 0; spins--)
                                                    {
                                                        process.Refresh();
                                                        if (process.HasExited)
                                                            break;

                                                        if (process.MainWindowHandle != IntPtr.Zero || !process.WaitForExit(200))
                                                        {
                                                            process.Refresh();
                                                            if (process.HasExited)
                                                                break;

                                                            if (process.MainWindowHandle == IntPtr.Zero &&
                                                                !SpinWait.SpinUntil(() =>
                                                                {
                                                                    process.Refresh();
                                                                    return process.HasExited || process.MainWindowHandle != IntPtr.Zero;
                                                                }, 50))
                                                            {
                                                                // Keep incrementing TTL as necessary.
                                                                if (spins % 2 == 0)
                                                                {
                                                                    if (cred != null)
                                                                        cred.IncreaseTTL(inc);
                                                                    if (gatewayCred != null)
                                                                        gatewayCred.IncreaseTTL(inc);
                                                                }

                                                                continue;
                                                            }

                                                            process.Refresh();
                                                            if (process.HasExited)
                                                                break;

                                                            if (!string.IsNullOrEmpty(title) && process.MainWindowHandle != IntPtr.Zero)
                                                            {
                                                                var oldTitle = new char[NativeMethods.GetWindowTextLength(process.MainWindowHandle) + 1];
                                                                if (NativeMethods.GetWindowText(process.MainWindowHandle, oldTitle, oldTitle.Length) == 0 ||
                                                                    !string.Equals(new string(oldTitle).TrimEnd(char.MinValue), title, StringComparison.OrdinalIgnoreCase))
                                                                    NativeMethods.SetWindowText(process.MainWindowHandle, title);
                                                            }

                                                            if (isMstscCommand)
                                                            {
                                                                // Find progress bar.
                                                                var pbHandle = process.MainWindowHandle == IntPtr.Zero ?
                                                                    IntPtr.Zero :
                                                                    NativeMethods.FindWindowEx(process.MainWindowHandle, IntPtr.Zero, "msctls_progress32", null);

                                                                if (waitForRdpFile)
                                                                {
                                                                    // Check for (un-)signed .rdp file dialog.
                                                                    var ms = (int)TimeSpan.FromMilliseconds(750).TotalMilliseconds;
                                                                    if (!process.HasExited && !process.WaitForExit(ms))
                                                                    {
                                                                        process.Refresh();
                                                                        if (process.HasExited)
                                                                            break;

                                                                        if (process.MainWindowHandle != IntPtr.Zero)
                                                                        {
                                                                            var lastPopup = NativeMethods.GetLastActivePopup(process.MainWindowHandle);

                                                                            // Continue when popup is open.
                                                                            if (lastPopup != process.MainWindowHandle)
                                                                            {
                                                                                try
                                                                                {
                                                                                    var element = lastPopup == IntPtr.Zero ? null : AutomationElement.FromHandle(lastPopup);

                                                                                    if (element != null)
                                                                                    {
                                                                                        // Break on enter credentials dialog.
                                                                                        if (element.Current.ControlType == ControlType.Window &&
                                                                                            element.Current.ClassName == "Credential Dialog Xaml Host")
                                                                                            break;

                                                                                        // Confirm connection box.
                                                                                        var button = element.FindFirst(
                                                                                        TreeScope.Children,
                                                                                        new PropertyCondition(AutomationElement.AutomationIdProperty, "13498"));

                                                                                        if (button != null && button.Current.ControlType == ControlType.Image)
                                                                                        {
                                                                                            if (_config.MstscConfirmCertificate)
                                                                                            {
                                                                                                button = element.FindFirst(
                                                                                                    TreeScope.Children,
                                                                                                    new PropertyCondition(AutomationElement.AutomationIdProperty, "1"));

                                                                                                if (button != null && button.Current.ControlType == ControlType.Button)
                                                                                                {
                                                                                                    var buttonHandle = new IntPtr(button.Current.NativeWindowHandle);

                                                                                                    // Try LegacyIAccessible.DoDefaultAction() first, fallback to emulating click on button.
                                                                                                    try
                                                                                                    {
                                                                                                        if (KprDoDefaultAction(buttonHandle) != 0)
                                                                                                            throw new Exception();
                                                                                                    }
                                                                                                    catch
                                                                                                    {
                                                                                                        NativeMethods.SendMessage(buttonHandle, NativeMethods.WM_LBUTTONDOWN, 0, 0);
                                                                                                        NativeMethods.SendMessage(buttonHandle, NativeMethods.WM_LBUTTONUP, 0, 0);
                                                                                                        NativeMethods.SendMessage(buttonHandle, NativeMethods.BM_CLICK, 0, 0);
                                                                                                    }
                                                                                                }
                                                                                            }

                                                                                            waitForRdpFile = false;
                                                                                            continue;
                                                                                        }
                                                                                    }
                                                                                }
                                                                                catch (ElementNotAvailableException) { }
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                if (pbHandle != IntPtr.Zero)
                                                                {
                                                                    var ms = (int)TimeSpan.FromMilliseconds(750).TotalMilliseconds;
                                                                    do
                                                                    {
                                                                        if (waitForRdpFile)
                                                                            waitForRdpFile = false;
                                                                        else if (process.HasExited || process.WaitForExit(ms))
                                                                            break;

                                                                        process.Refresh();

                                                                        if (process.HasExited || process.MainWindowHandle == IntPtr.Zero)
                                                                            break;

                                                                        var lastPopup = NativeMethods.GetLastActivePopup(process.MainWindowHandle);

                                                                        // Continue when popup is open.
                                                                        if (lastPopup != process.MainWindowHandle)
                                                                        {
                                                                            try
                                                                            {
                                                                                var element = lastPopup == IntPtr.Zero ? null : AutomationElement.FromHandle(lastPopup);

                                                                                if (element != null)
                                                                                {
                                                                                    // Break on enter credentials dialog.
                                                                                    if (element.Current.ControlType == ControlType.Window &&
                                                                                        element.Current.ClassName == "Credential Dialog Xaml Host")
                                                                                        break;

                                                                                    // Connection failed error box.
                                                                                    var button = element.FindFirst(
                                                                                        TreeScope.Children,
                                                                                        new PropertyCondition(AutomationElement.AutomationIdProperty, "CommandButton_1"));

                                                                                    if (button != null && button.Current.ControlType == ControlType.Button)
                                                                                        break;

                                                                                    if (_config.MstscConfirmCertificate)
                                                                                    {
                                                                                        // Confirm certificate error box.
                                                                                        button = element.FindFirst(
                                                                                            TreeScope.Children,
                                                                                            new PropertyCondition(AutomationElement.AutomationIdProperty, "14004"));

                                                                                        if (button != null && button.Current.ControlType == ControlType.Button)
                                                                                        {
                                                                                            var buttonHandle = new IntPtr(button.Current.NativeWindowHandle);

                                                                                            // Try LegacyIAccessible.DoDefaultAction() first, fallback to emulating click on button.
                                                                                            try
                                                                                            {
                                                                                                if (KprDoDefaultAction(buttonHandle) != 0)
                                                                                                    throw new Exception();
                                                                                            }
                                                                                            catch
                                                                                            {
                                                                                                NativeMethods.SendMessage(buttonHandle, NativeMethods.WM_LBUTTONDOWN, 0, 0);
                                                                                                NativeMethods.SendMessage(buttonHandle, NativeMethods.WM_LBUTTONUP, 0, 0);
                                                                                                NativeMethods.SendMessage(buttonHandle, NativeMethods.BM_CLICK, 0, 0);
                                                                                            }

                                                                                            continue;
                                                                                        }
                                                                                        else
                                                                                            break;
                                                                                    }
                                                                                }
                                                                            }
                                                                            catch (ElementNotAvailableException) { }
                                                                        }
                                                                        else //if (NativeMethods.FindWindowEx(process.MainWindowHandle, IntPtr.Zero, "BBarWindowClass", "BBar") != IntPtr.Zero)
                                                                        {
                                                                            // Break when progress bar is gone.
                                                                            pbHandle = process.MainWindowHandle == IntPtr.Zero ?
                                                                                IntPtr.Zero :
                                                                                NativeMethods.FindWindowEx(process.MainWindowHandle, IntPtr.Zero, "msctls_progress32", null);
                                                                            if (pbHandle == IntPtr.Zero)
                                                                                break;
                                                                        }

                                                                        process.Refresh();
                                                                        if (!process.HasExited)
                                                                        {
                                                                            // Keep incrementing TTL as necessary.
                                                                            if (cred != null)
                                                                                cred.IncreaseTTL(inc);
                                                                            if (gatewayCred != null)
                                                                                gatewayCred.IncreaseTTL(inc);
                                                                        }
                                                                    } while (!process.HasExited);
                                                                }
                                                                else if (spins == 1)
                                                                {
                                                                    process.Refresh();

                                                                    if ((process.HasExited && _config.CredVaultRemoveOnExit) ||
                                                                        (!process.HasExited && process.MainWindowHandle != IntPtr.Zero && _config.CredVaultTtl > 0 && _config.CredVaultAdaptiveTtl))
                                                                    {
                                                                        // Last try to find progress bar.
                                                                        if (!process.HasExited && !process.WaitForExit(250))
                                                                        {
                                                                            process.Refresh();
                                                                            pbHandle = process.MainWindowHandle == IntPtr.Zero ?
                                                                                IntPtr.Zero :
                                                                                NativeMethods.FindWindowEx(process.MainWindowHandle, IntPtr.Zero, "msctls_progress32", null);
                                                                            if (pbHandle != IntPtr.Zero)
                                                                            {
                                                                                spins++;
                                                                                continue;
                                                                            }
                                                                        }

                                                                        if (cred != null)
                                                                        {
                                                                            cred.Dispose();
                                                                            cred = null;
                                                                        }
                                                                        if (gatewayCred != null)
                                                                        {
                                                                            gatewayCred.Dispose();
                                                                            gatewayCred = null;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            process.Refresh();
                                                            if (process.HasExited || process.WaitForExit(250))
                                                                break;
                                                        }
                                                    }

                                                process.Refresh();

                                                if ((process.HasExited || process.MainWindowHandle != IntPtr.Zero) && _config.CredVaultTtl > 0 && _config.CredVaultAdaptiveTtl)
                                                {
                                                    if (cred != null)
                                                    {
                                                        cred.Dispose();
                                                        cred = null;
                                                    }
                                                    if (gatewayCred != null)
                                                    {
                                                        gatewayCred.Dispose();
                                                        gatewayCred = null;
                                                    }
                                                }

                                                if (!process.HasExited && !process.WaitForExit(250))
                                                {
                                                    process.Refresh();
                                                    if (!string.IsNullOrEmpty(title) && process.MainWindowHandle != IntPtr.Zero)
                                                        NativeMethods.SetWindowText(process.MainWindowHandle, title);

                                                    if (!process.HasExited && !process.WaitForExit(250))
                                                    {
                                                        process.Refresh();
                                                        if (!process.HasExited)
                                                        {
                                                            // Set title twice to try to make sure to catch the correct window handle.
                                                            if (!string.IsNullOrEmpty(title) && process.MainWindowHandle != IntPtr.Zero)
                                                                NativeMethods.SetWindowText(process.MainWindowHandle, title);

                                                            // Find progress bar.
                                                            var pbHandle = process.MainWindowHandle == IntPtr.Zero ?
                                                                IntPtr.Zero :
                                                                NativeMethods.FindWindowEx(process.MainWindowHandle, IntPtr.Zero, "msctls_progress32", null);

                                                            var maxTimeout = Math.Max(10000, ttl * 3);
                                                            var timeoutInc = Math.Max(250, Math.Min(1000, process.MainWindowHandle == IntPtr.Zero || pbHandle != IntPtr.Zero ? ttl / 4 : ttl / 2));
                                                            var timeout = timeoutInc;
                                                            var nextTimeout = timeout;
                                                            var setTitle = false;

                                                            // Check if the window is still alive from time to time.
                                                            while (!process.HasExited) // !process.WaitForExit(timeout))
                                                            {
                                                                process.Refresh();
                                                                if (process.HasExited)
                                                                    break;

                                                                // Allow setting the title at later points.
                                                                if (setTitle)
                                                                {
                                                                    setTitle = false;
                                                                    if (!string.IsNullOrEmpty(title) && process.MainWindowHandle != IntPtr.Zero)
                                                                    {
                                                                        var oldTitle = new char[NativeMethods.GetWindowTextLength(process.MainWindowHandle) + 1];
                                                                        if (NativeMethods.GetWindowText(process.MainWindowHandle, oldTitle, oldTitle.Length) == 0 ||
                                                                            !string.Equals(new string(oldTitle).TrimEnd(char.MinValue), title, StringComparison.OrdinalIgnoreCase))
                                                                            NativeMethods.SetWindowText(process.MainWindowHandle, title);
                                                                    }
                                                                }

                                                                if (!process.WaitForInputIdle(timeout))
                                                                {
                                                                    nextTimeout = Math.Min(maxTimeout, timeout + timeoutInc);
                                                                    timeout = nextTimeout;
                                                                    continue;
                                                                }

                                                                process.Refresh();
                                                                if (process.HasExited)
                                                                    break;

                                                                if (process.MainWindowHandle != IntPtr.Zero)
                                                                {
                                                                    if (_config.CredVaultTtl > 0 && _config.CredVaultAdaptiveTtl)
                                                                    {
                                                                        if (cred != null)
                                                                        {
                                                                            cred.Dispose();
                                                                            cred = null;
                                                                        }
                                                                        if (gatewayCred != null)
                                                                        {
                                                                            gatewayCred.Dispose();
                                                                            gatewayCred = null;
                                                                        }
                                                                    }

                                                                    timeout = nextTimeout;
                                                                    nextTimeout = Math.Min(maxTimeout, timeout + timeoutInc);
                                                                }
                                                                else
                                                                {
                                                                    timeout = timeoutInc;
                                                                    nextTimeout = timeout;
                                                                }

                                                                if (isMstscCommand)
                                                                {
                                                                    var lastPopup = NativeMethods.GetLastActivePopup(process.MainWindowHandle);
                                                                    if (lastPopup != process.MainWindowHandle)
                                                                    {
                                                                        var mstscHandleCredDialog = _config.MstscHandleCredDialog;
                                                                        if ((tmpUseCreds && retryOnce) || mstscHandleCredDialog)
                                                                        {
                                                                            try
                                                                            {
                                                                                var element = lastPopup == IntPtr.Zero ? null : AutomationElement.FromHandle(lastPopup);

                                                                                // Enter credentials dialog.
                                                                                if (element != null &&
                                                                                    element.Current.ControlType == ControlType.Window &&
                                                                                    element.Current.ClassName == "Credential Dialog Xaml Host" &&
                                                                                    (retryOnce || mstscHandleCredDialog))
                                                                                {
                                                                                    timeout = timeoutInc;
                                                                                    nextTimeout = timeout;

                                                                                    var textBox = element.FindFirst(
                                                                                        TreeScope.Subtree,
                                                                                        new PropertyCondition(AutomationElement.AutomationIdProperty, "StaticTextField_0"));

                                                                                    if (textBox != null)
                                                                                    {
                                                                                        setTitle = true;

                                                                                        var options = element.FindFirst(
                                                                                            TreeScope.Subtree,
                                                                                            new PropertyCondition(AutomationElement.AutomationIdProperty, "ChooseAnotherOption"));

                                                                                        if (options != null)
                                                                                        {
                                                                                            object pattern;
                                                                                            if (options.TryGetCurrentPattern(InvokePattern.Pattern, out pattern))
                                                                                            {
                                                                                                var ip = (InvokePattern)pattern;

                                                                                                var list = element.FindFirst(
                                                                                                    TreeScope.Subtree,
                                                                                                    new PropertyCondition(AutomationElement.ClassNameProperty, "ListView"));

                                                                                                if (list == null)
                                                                                                {
                                                                                                    ip.Invoke();
                                                                                                    if (process.HasExited || process.WaitForExit(25))
                                                                                                        continue;

                                                                                                    list = element.FindFirst(
                                                                                                        TreeScope.Subtree,
                                                                                                        new PropertyCondition(AutomationElement.ClassNameProperty, "ListView"));

                                                                                                    if (list == null && (process.HasExited || process.WaitForExit(25)))
                                                                                                        continue;
                                                                                                }

                                                                                                if (list == null)
                                                                                                    list = element.FindFirst(
                                                                                                        TreeScope.Subtree,
                                                                                                        new PropertyCondition(AutomationElement.ClassNameProperty, "ListView"));

                                                                                                if (list != null)
                                                                                                {
                                                                                                    var items = list.FindAll(
                                                                                                        TreeScope.Subtree,
                                                                                                        new PropertyCondition(AutomationElement.ClassNameProperty, "ListViewItem"));

                                                                                                    if (items != null)
                                                                                                    {
                                                                                                        var last = items.OfType<AutomationElement>().LastOrDefault(x =>
                                                                                                        {
                                                                                                            try
                                                                                                            {
                                                                                                                var textBlocks = x.FindAll(
                                                                                                                    TreeScope.Subtree,
                                                                                                                    new PropertyCondition(AutomationElement.ClassNameProperty, "TextBlock"));
                                                                                                                if (textBlocks == null || textBlocks.Count != 1)
                                                                                                                    return false;
                                                                                                                if (textBlocks.OfType<AutomationElement>().Any(y => y.Current.Name.Contains("Smartcard")))
                                                                                                                    return false;
                                                                                                            }
                                                                                                            catch
                                                                                                            {
                                                                                                                return false;
                                                                                                            }
                                                                                                            return true;
                                                                                                        });

                                                                                                        if (last != null)
                                                                                                        {
                                                                                                            if (last.TryGetCurrentPattern(SelectionItemPattern.Pattern, out pattern))
                                                                                                            {
                                                                                                                var sp = (SelectionItemPattern)pattern;
                                                                                                                sp.Select();
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }

                                                                                                if (!process.HasExited)
                                                                                                    ip.Invoke();

                                                                                                if (process.HasExited || process.WaitForExit(25))
                                                                                                    continue;

                                                                                                textBox = element.FindFirst(
                                                                                                    TreeScope.Subtree,
                                                                                                    new PropertyCondition(AutomationElement.AutomationIdProperty, "EditField_1"));

                                                                                                if (textBox == null && (process.HasExited || process.WaitForExit(25)))
                                                                                                    continue;
                                                                                            }
                                                                                        }
                                                                                    }

                                                                                    var checkbox = element.FindFirst(
                                                                                        TreeScope.Subtree,
                                                                                        new PropertyCondition(AutomationElement.ClassNameProperty, "CheckBox"));

                                                                                    if (checkbox != null)
                                                                                    {
                                                                                        object pattern;
                                                                                        if (checkbox.TryGetCurrentPattern(TogglePattern.Pattern, out pattern))
                                                                                        {
                                                                                            var tp = (TogglePattern)pattern;
                                                                                            if (tp.Current.ToggleState == ToggleState.On)
                                                                                                tp.Toggle();
                                                                                        }
                                                                                    }

                                                                                    if (retryOnce)
                                                                                    {
                                                                                        if (retryUsername != null)
                                                                                        {
                                                                                            if (textBox == null)
                                                                                                textBox = element.FindFirst(
                                                                                                    TreeScope.Subtree,
                                                                                                    new PropertyCondition(AutomationElement.AutomationIdProperty, "EditField_1"));

                                                                                            if (textBox != null)
                                                                                            {
                                                                                                object pattern;
                                                                                                if (textBox.TryGetCurrentPattern(ValuePattern.Pattern, out pattern))
                                                                                                {
                                                                                                    var vp = (ValuePattern)pattern;
                                                                                                    if (string.IsNullOrEmpty(vp.Current.Value))
                                                                                                    {
                                                                                                        if (retryCredEntry != null)
                                                                                                        {
                                                                                                            // KeePass.Util.EntryUtil.FillPlaceholders
                                                                                                            var tmpEntry = retryCredEntry.GetResolvedReferencesEntry(new SprContext(retryCredEntry, mainForm.ActiveDatabase, kpScf)
                                                                                                            {
                                                                                                                ForcePlainTextPasswords = true
                                                                                                            });
                                                                                                            retryUsername = tmpEntry.Strings.GetSafe(PwDefs.UserNameField);
                                                                                                            if (forceLocalUser)
                                                                                                                retryUsername = retryUsername.ForceLocalUser(hostname);
                                                                                                            if (forceUpn)
                                                                                                                retryUsername = retryUsername.ForceUPN();
                                                                                                        }
                                                                                                        var usernameChars = retryUsername.ReadChars();
                                                                                                        var usernameString = new string(usernameChars);
                                                                                                        vp.SetValue(usernameString);
                                                                                                        MemoryUtil.SecureZeroMemory(usernameString);
                                                                                                        MemoryUtil.SecureZeroMemory(usernameChars);
                                                                                                    }

                                                                                                    if (retryOnce)
                                                                                                        retryOnce = false;
                                                                                                }
                                                                                            }
                                                                                        }

                                                                                        if (retryPassword != null)
                                                                                        {
                                                                                            if (retryPassword.Length > 0)
                                                                                            {
                                                                                                var pwBox = element.FindFirst(
                                                                                                    TreeScope.Subtree,
                                                                                                    new PropertyCondition(AutomationElement.AutomationIdProperty, "PasswordField_2"));

                                                                                                if (pwBox != null)
                                                                                                {
                                                                                                    object pattern;
                                                                                                    if (pwBox.TryGetCurrentPattern(ValuePattern.Pattern, out pattern))
                                                                                                    {
                                                                                                        var vp = (ValuePattern)pattern;
                                                                                                        if (string.IsNullOrEmpty(vp.Current.Value))
                                                                                                        {
                                                                                                            IntPtr valuePtr = IntPtr.Zero;
                                                                                                            try
                                                                                                            {
                                                                                                                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(retryPassword);
                                                                                                                if (valuePtr != IntPtr.Zero)
                                                                                                                {
                                                                                                                    var value = Marshal.PtrToStringUni(valuePtr);
                                                                                                                    vp.SetValue(value);
                                                                                                                    MemoryUtil.SecureZeroMemory(value);
                                                                                                                }
                                                                                                            }
                                                                                                            finally
                                                                                                            {
                                                                                                                if (valuePtr != IntPtr.Zero)
                                                                                                                {
                                                                                                                    MemoryUtil.SafeSecureZeroMemory(valuePtr, retryPassword.Length * 2);
                                                                                                                    Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
                                                                                                                }
                                                                                                            }
                                                                                                        }

                                                                                                        if (retryOnce)
                                                                                                            retryOnce = false;
                                                                                                    }
                                                                                                }
                                                                                            }

                                                                                            var button = element.FindFirst(
                                                                                                TreeScope.Children,
                                                                                                new PropertyCondition(AutomationElement.AutomationIdProperty, "OkButton"));

                                                                                            if (button != null)
                                                                                            {
                                                                                                object pattern;
                                                                                                if (button.TryGetCurrentPattern(InvokePattern.Pattern, out pattern))
                                                                                                {
                                                                                                    var ip = (InvokePattern)pattern;
                                                                                                    ip.Invoke();

                                                                                                    retryPassword.Dispose();
                                                                                                    retryPassword = null;

                                                                                                    if (retryCredEntry != null)
                                                                                                        retryCredEntry = null;

                                                                                                    if (retryOnce)
                                                                                                        retryOnce = false;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                                else
                                                                                    retryOnce = retryUsername != null;
                                                                            }
                                                                            catch (ElementNotAvailableException) { }
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        retryOnce = retryUsername != null;

                                                                        if (!process.HasExited && !string.IsNullOrEmpty(title) && process.MainWindowHandle != IntPtr.Zero)
                                                                        {
                                                                            var oldTitle = new char[NativeMethods.GetWindowTextLength(process.MainWindowHandle) + 1];
                                                                            if (NativeMethods.GetWindowText(process.MainWindowHandle, oldTitle, oldTitle.Length) == 0 ||
                                                                                !new string(oldTitle).TrimEnd(char.MinValue).Equals(title, StringComparison.OrdinalIgnoreCase))
                                                                                NativeMethods.SetWindowText(process.MainWindowHandle, title);
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (retryPassword != null)
                                                                    {
                                                                        retryPassword.Dispose();
                                                                        retryPassword = null;
                                                                    }

                                                                    if (retryCredEntry != null)
                                                                        retryCredEntry = null;
                                                                }

                                                                process.Refresh();

                                                                if (process.HasExited || process.WaitForExit(timeout))
                                                                    continue;

                                                                process.Refresh();

                                                                // Assume something went wrong when threads get stuck.
                                                                /*var allThreads = process.Threads.Cast<ProcessThread>();
                                                                if (!allThreads.Any(thread => thread.ThreadState != ThreadState.Wait) &&
                                                                     allThreads.Any(thread => thread.WaitReason != ThreadWaitReason.Suspended))
                                                                {
                                                                    if (!process.HasExited)
                                                                        process.Kill();
                                                                    break;
                                                                }*/

                                                                try
                                                                {
                                                                    // Assume something went wrong when there is no window anymore.
                                                                    if (process.HasExited || process.MainWindowHandle == IntPtr.Zero)
                                                                    {
                                                                        if (!process.HasExited && !process.WaitForExit(timeout))
                                                                        {
                                                                            process.Refresh();
                                                                            if (process.MainWindowHandle == IntPtr.Zero)
                                                                            {
                                                                                if (!process.HasExited)
                                                                                    process.Kill();
                                                                                break;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                catch { }
                                                            }
                                                        }
                                                    }
                                                }

                                                try
                                                {
                                                    if (mstscSd)
                                                        while (!process.WaitForExit(1000) || (!cts.IsCancellationRequested && !process.HasExited))
                                                            ;

                                                    if (!process.HasExited)
                                                        if (!process.WaitForExit(1000) && cts.IsCancellationRequested && !process.HasExited)
                                                            process.Kill();
                                                }
                                                catch { }
                                            }
                                            finally
                                            {
                                                tcs.TrySetResult(null);
                                                process.Dispose();
                                            }
                                        }, cts.Token, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);

                                        if (mstscSd)
                                            SecureDesktop.Instance.Run(runner);
                                        else
                                            runner().Start(TaskScheduler.Default);

                                        tcs.Task.Wait(cts.Token);
                                    }
                                }
                                finally
                                {
                                    if (cred != null)
                                    {
                                        if (_config.CredVaultTtl > 0 && _config.CredVaultAdaptiveTtl)
                                            cred.ResetTTL();
                                        if (_config.CredVaultRemoveOnExit)
                                            cred.Dispose();
                                    }

                                    if (gatewayCred != null)
                                    {
                                        if (_config.CredVaultTtl > 0 && _config.CredVaultAdaptiveTtl)
                                            gatewayCred.ResetTTL();
                                        if (_config.CredVaultRemoveOnExit)
                                            gatewayCred.Dispose();
                                    }

                                    if (retryPassword != null)
                                        retryPassword.Dispose();

                                    if (rdpFile != null)
                                        rdpFile.Dispose();

                                    if (tmpUseCreds)
                                    {
                                        if (_config.MstscCleanupRegistry)
                                            Util.RemoveHintFromRegistry(hostname);
                                    }

                                    TaskWithCancellationToken localOldTask = null;
                                    try
                                    {
                                        if (_tasks.TryRemove(localTaskUuid, out localOldTask) && localOldTask != newTask && !localOldTask.IsCompleted)
                                            _tasks.TryAdd(localTaskUuid, localOldTask);
                                    }
                                    catch { }

                                    newTask.ContinueWith(t =>
                                    {
                                        try
                                        {
                                            if (_tasks.TryRemove(localTaskUuid, out localOldTask) && localOldTask != newTask && !localOldTask.IsCompleted)
                                                _tasks.TryAdd(localTaskUuid, localOldTask);
                                        }
                                        catch { }

                                        try
                                        {
                                            if (oldTask != null && !oldTask.IsCompleted && !_tasks.ContainsKey(localTaskUuid))
                                                if (_tasks.TryAdd(localTaskUuid, oldTask))
                                                    oldTask = null;
                                        }
                                        catch { }

                                        try
                                        {
                                            t.Wait();
                                        }
                                        catch (Exception ex)
                                        {
                                            Task.Factory.FromAsync(_host.MainWindow.BeginInvoke(new Action(() =>
                                            {
                                                Util.ShowErrorDialog(
                                                    string.Format("{0}{1}{2}", hostname, Environment.NewLine, Util.FormatException(ex)),
                                                    null,
                                                    string.Format("{0} - {1}", Util.KeePassRDP, KPRes.FatalError),
                                                    VtdIcon.Error,
                                                    _host.MainWindow,
                                                    null, 0, null, 0);
                                            })), endInvoke => _host.MainWindow.EndInvoke(endInvoke), TaskCreationOptions.None, TaskScheduler.Default);
                                        }
                                        finally
                                        {
                                            newTask.Dispose();
                                        }
                                    });
                                }

                                try
                                {
                                    if (oldTask != null && !oldTask.IsCompleted)
                                    {
                                        if (cts.IsCancellationRequested)
                                            oldTask.Cancel();
                                        else if (!_tasks.ContainsKey(localTaskUuid) && _tasks.TryAdd(localTaskUuid, oldTask))
                                            oldTask = null;
                                    }
                                }
                                catch { }
                            }, taskUuid, cts);
                        }
                    }
            }

            mainForm.SetStatusEx(null);
        }

        [DllImport("KeePassRDP.unmanaged.dll", EntryPoint = "KprDoDefaultAction", SetLastError = false)]
        private static extern int KprDoDefaultAction([In] IntPtr parent);

        /*[DllImport("KeePassRDP.unmanaged.dll", EntryPoint = "KprDoDefaultAction", SetLastError = false)]
        private static extern int KprDoDefaultAction([In] IntPtr parent, [In] string automationId);*/
    }
}