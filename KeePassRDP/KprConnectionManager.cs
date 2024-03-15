/*
 *  Copyright (C) 2018 - 2024 iSnackyCracky, NETertainer
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
using KeePassLib.Security;
using KeePassRDP.Commands;
using KeePassRDP.Extensions;
using KeePassRDP.Generator;
using KeePassRDP.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
            public CancellationToken CancellationToken { get {  return _cancellationTokenSource.Token; } }
            public bool IsCancellationRequested { get { return _cancellationTokenSource.IsCancellationRequested; } }

            private readonly CancellationTokenSource _cancellationTokenSource;

            public TaskWithCancellationToken(Action<object> action, object state, CancellationTokenSource cancellationTokenSource) : base(
                action,
                state,
                cancellationTokenSource.Token,
                TaskCreationOptions.AttachedToParent |
                TaskCreationOptions.PreferFairness |
                TaskCreationOptions.LongRunning)
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
                return ContinueWith(action, CancellationToken.None, TaskContinuationOptions.PreferFairness, TaskScheduler.Default);
            }

            public new void Dispose()
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
                    base.Dispose();
            }
        }

        public int Count { get { return _tasks.Count; } }

        public bool IsCompleted
        {
            get
            {
                return !_tasks.Values.Any(x =>
                {
                    try
                    {
                        return !x.IsCompleted;
                    }
                    catch
                    {
                        return true;
                    }
                });
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

        public void Cancel()
        {
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
                catch
                {
                }
        }

        public void Dispose()
        {
            _tasks.Clear();

            if (_credentialPicker.IsValueCreated)
                _credentialPicker.Value.Dispose();
        }

        public void ConnectRDPtoKeePassEntry(bool tmpMstscUseAdmin = false, bool tmpUseCreds = false)
        {
            if (Util.IsValid(_host))
            {
                var mainForm = _host.MainWindow;
                var selectedEntries = mainForm.GetSelectedEntries();

                var parentGroups = selectedEntries
                    .Where(entry => !entry.Strings.GetSafe(PwDefs.UrlField).IsEmpty)
                    .GroupBy(entry => entry.ParentGroup.Uuid, EqualityComparer<PwUuid>.Default);

                if (!_config.KeePassConnectToAll)
                    parentGroups = parentGroups.Skip(parentGroups.Count() - 1);

                var totalCount = parentGroups.Aggregate(0, (a, b) => a + b.Count());

                var postfix = string.Format(KprResourceManager.Instance["{0} entr" + (totalCount == 1 ? "y" : "ies") + " of {1} selected."], totalCount, selectedEntries.Length);
                var connectingTo = string.Format(" {0} ", KprResourceManager.Instance["connecting to"]);
                var skipped = string.Format(" {0}", KprResourceManager.Instance["skipped"]);

                mainForm.SetStatusEx(string.Format("{0}{1}{2}", Util.KeePassRDP, connectingTo, postfix));

                foreach (var parentGroup in parentGroups)
                {
                    var entries = parentGroup.AsEnumerable();
                    var count = entries.Count();

                    if (count == 0)
                        continue;

                    if (!_config.KeePassConnectToAll && count > 1)
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
                                if (entrySettings.UseCredpicker && (Util.InRdpSubgroup(entry, _config.CredPickerCustomGroup) || entrySettings.CpGroupUUIDs.Any()))
                                {
                                    credPick.AddEntry(entry, entrySettings, _config.CredPickerCustomGroup);
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
                                credEntry = credPick.GetCredentialEntry(_config.CredPickerIncludeSelected, _config.KeePassShowResolvedReferences);
                            }
                            catch (OperationCanceledException)
                            {
                                continue;
                            }
                            finally
                            {
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
                                credEntry = credEntry.GetResolvedReferencesEntry(new SprContext(credEntry, mainForm.ActiveDatabase, _config.KeePassSprCompileFlags)
                                {
                                    ForcePlainTextPasswords = true // true is default, PwDefs.PasswordField is replaced with PwDefs.HiddenPassword during SprEngine.Compile otherwise.
                                });
                            }
                        }
                    }

                    using (var credEntrySettings = unresolvedCredEntry == null ? KprEntrySettings.Empty : unresolvedCredEntry.GetKprSettings(true) ?? KprEntrySettings.Empty)
                        foreach (var connPwEntry in entries)
                        {
                            var ctx = new SprContext(connPwEntry, mainForm.ActiveDatabase, _config.KeePassSprCompileFlags)
                            {
                                ForcePlainTextPasswords = true
                            };

                            var host = SprEngine.Compile(connPwEntry.Strings.ReadSafe(PwDefs.UrlField), ctx);

                            if (string.IsNullOrEmpty(host))
                                continue;

                            var port = string.Empty;
                            Uri uri = null;

                            // Try to parse entry URL as URI.
                            if (!Uri.TryCreate(host, UriKind.Absolute, out uri) ||
                                (uri.HostNameType == UriHostNameType.Unknown && !UriParser.IsKnownScheme(uri.Scheme)))
                            {
                                try
                                {
                                    // Second try to parse entry URL as URI with UriBuilder.
                                    uri = new UriBuilder(host).Uri;
                                    if (uri.HostNameType == UriHostNameType.Unknown && !UriParser.IsKnownScheme(uri.Scheme))
                                        // Third try to parse entry URL as URI with fallback scheme.
                                        if (!Uri.TryCreate(string.Format("rdp://{0}", host), UriKind.Absolute, out uri) || uri.HostNameType == UriHostNameType.Unknown)
                                            uri = null;
                                }
                                catch (UriFormatException)
                                {
                                    uri = null;
                                }
                            }

                            if (uri != null && uri.HostNameType != UriHostNameType.Unknown)
                            {
                                host = uri.Host;
                                if (!uri.IsDefaultPort && uri.Port != Util.DefaultRdpPort)
                                    port = string.Format(":{0}", uri.Port);
                            }
                            else
                            {
                                VistaTaskDialog.ShowMessageBoxEx(
                                    string.Format(KprResourceManager.Instance["The URL/target '{0}' of the selected entry could not be parsed."], host),
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

                                if (string.IsNullOrWhiteSpace(host))
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
                                        host = entrySettings.RdpFile.FullAddress;
                                        if (string.IsNullOrWhiteSpace(port) && entrySettings.RdpFile.ServerPort > 0)
                                            port = string.Format(":{0}", entrySettings.RdpFile.ServerPort);
                                    }
                                }

                                var retryOnce = entrySettings.RetryOnce;
                                var forceLocalUser = entrySettings.ForceLocalUser;
                                var forceUpn = entrySettings.ForceUpn;

                                // Connect to RDP using credentials from KeePass, skipping entries with no credentials.
                                if (tmpUseCreds)
                                {
                                    // Use result from KprCredentialPicker or fallback to credentials from selected entry.
                                    var shownInPicker = entrySettings.UseCredpicker &&
                                        (Util.InRdpSubgroup(connPwEntry, _config.CredPickerCustomGroup) || entrySettings.CpGroupUUIDs.Any());
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
                                        username = username.ForceLocalUser(host);

                                    if (forceUpn)
                                        username = username.ForceUPN();

                                    var password = tmpEntry.Strings.GetSafe(PwDefs.PasswordField);
                                    // Do not connect to entry if password is empty.
                                    /*if (password.IsEmpty)
                                        continue;*/

                                    var securePassword = password.AsSecureString();

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
                                        string.Format("TERMSRV/{0}", host.ToUpperInvariant()),
                                        _config.CredVaultUseWindows ?
                                            NativeCredentials.CRED_TYPE.DOMAIN_PASSWORD :
                                            NativeCredentials.CRED_TYPE.GENERIC,
                                        _config.CredVaultTtl);

                                    //username = username.Remove(0, username.Length);
                                    //password = password.Remove(0, password.Length);

                                    // Add KprCredential to KprCredentialManager.
                                    //_credManager.Value.Add(cred);

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
                                    retryOnce = false;

                                TaskWithCancellationToken oldTask = null;
                                try
                                {
                                    if ((tmpUseCreds || _config.KeePassAlwaysConfirm) && _tasks.TryGetValue(taskUuid, out oldTask) && !oldTask.IsCompleted)
                                        if (VistaTaskDialog.ShowMessageBoxEx(
                                            tmpUseCreds ?
                                                string.Format(KprResourceManager.Instance["Already connected with the same credentials to URL/target '{0}'."], host) :
                                                string.Format(KprResourceManager.Instance["Already connected to URL/target '{0}'."], host),
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
                                }
                                catch (ObjectDisposedException)
                                {
                                }

                                var title = string.Empty;
                                if (_config.MstscReplaceTitle)
                                    title = SprEngine.Compile(connPwEntry.Strings.ReadSafe(PwDefs.TitleField), ctx);

                                TaskWithCancellationToken newTask = null;
                                _tasks[taskUuid] = newTask = new TaskWithCancellationToken(thisTaskUuid =>
                                {
                                    RdpFile rdpFile = null;

                                    try
                                    {
                                        var waitForRdpFile = false;

                                        var commandString = (!string.IsNullOrWhiteSpace(_config.MstscExecutable) ? _config.MstscExecutable : typeof(MstscCommand).Name).Split(new[] { ':' }, 2);
                                        var commandType = Type.GetType(string.Format("{0}.{1}", typeof(Command).Namespace, commandString.FirstOrDefault()));
                                        var icommand = Activator.CreateInstance(commandType, new[] { commandString.Skip(1).FirstOrDefault() }) as ICommand;

                                        if (icommand == null)
                                            throw new NullReferenceException("Command");

                                        if (icommand is IMstscCommand)
                                        {
                                            var command = icommand as IMstscCommand;

                                            command.HostPort = host + port;

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
                                                if (!string.IsNullOrWhiteSpace(host))
                                                    rdpFile.FullAddress = host;
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
                                        }

                                        var argumentsBuilder = new StringBuilder(icommand.ToString());

                                        foreach (var argument in entrySettings.MstscParameters)
                                        {
                                            argumentsBuilder.Append(SprEngine.Compile(argument, ctx));
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
                                                Util.RemoveHintFromRegistry(host);
                                        }

                                        // Start RDP / mstsc.exe.
                                        var rdpProcess = new ProcessStartInfo
                                        {
                                            WindowStyle = ProcessWindowStyle.Normal,
                                            FileName = icommand.Executable,
                                            Arguments = argumentsBuilder.ToString().TrimEnd(),
                                            ErrorDialog = true,
                                            ErrorDialogParentHandle = Form.ActiveForm.Handle,
                                            LoadUserProfile = false,
                                            WorkingDirectory = Path.GetTempPath() // Environment.ExpandEnvironmentVariables("%TEMP%")
                                        };

                                        if (_config.MstscReplaceTitle)
                                            title = string.Format("{0} - {1}", string.IsNullOrWhiteSpace(title) ? host : title, Util.KeePassRDP);

                                        // Add KprCredential to KprCredentialManager.
                                        if (cred != null)
                                            _credManager.Value.Add(cred);
                                        if (gatewayCred != null)
                                            _credManager.Value.Add(gatewayCred);

                                        using (var process = Process.Start(rdpProcess))
                                        {
                                            var inc = _config.CredVaultTtl > 0 && _config.CredVaultAdaptiveTtl && cred != null ?
                                                TimeSpan.FromSeconds(Math.Ceiling(Math.Max(1, _config.CredVaultTtl / 2f))) : TimeSpan.Zero;

                                            var ttl = (int)Math.Max(1000, inc.TotalMilliseconds);

                                            if (process.WaitForInputIdle(ttl) && !process.HasExited)
                                                // Wait a limited time for mstsc.exe window, otherwise assume something went wrong.
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
                                                                return process.MainWindowHandle != IntPtr.Zero;
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

                                                        if (!string.IsNullOrEmpty(title))
                                                        {
                                                            var oldTitle = new char[NativeMethods.GetWindowTextLength(process.MainWindowHandle) + 1];
                                                            NativeMethods.GetWindowText(process.MainWindowHandle, oldTitle, oldTitle.Length);
                                                            if (!new string(oldTitle).TrimEnd(char.MinValue).Equals(title, StringComparison.OrdinalIgnoreCase))
                                                                NativeMethods.SetWindowText(process.MainWindowHandle, title);
                                                        }

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
                                                                        var element = AutomationElement.FromHandle(lastPopup);

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

                                                                                    waitForRdpFile = false;
                                                                                }
                                                                            }

                                                                            continue;
                                                                        }
                                                                        else
                                                                            waitForRdpFile = false;
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        if (pbHandle != IntPtr.Zero)
                                                        {
                                                            var ms = (int)TimeSpan.FromMilliseconds(750).TotalMilliseconds;
                                                            do
                                                            {
                                                                if (process.HasExited || process.WaitForExit(ms))
                                                                    break;

                                                                process.Refresh();

                                                                if (process.HasExited || process.MainWindowHandle == IntPtr.Zero)
                                                                    break;

                                                                var lastPopup = NativeMethods.GetLastActivePopup(process.MainWindowHandle);

                                                                // Continue when popup is open.
                                                                if (lastPopup != process.MainWindowHandle)
                                                                {
                                                                    var element = AutomationElement.FromHandle(lastPopup);

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
                                                                else //if (NativeMethods.FindWindowEx(process.MainWindowHandle, IntPtr.Zero, "BBarWindowClass", "BBar") != IntPtr.Zero)
                                                                {
                                                                    // Break when progress bar is gone.
                                                                    pbHandle = process.MainWindowHandle == IntPtr.Zero ?
                                                                        IntPtr.Zero :
                                                                        NativeMethods.FindWindowEx(process.MainWindowHandle, IntPtr.Zero, "msctls_progress32", null);
                                                                    if (pbHandle == IntPtr.Zero)
                                                                        break;
                                                                }

                                                                // Keep incrementing TTL as necessary.
                                                                if (cred != null)
                                                                    cred.IncreaseTTL(inc);
                                                                if (gatewayCred != null)
                                                                    gatewayCred.IncreaseTTL(inc);
                                                            } while (!process.HasExited);
                                                        }
                                                        else if (spins == 1)
                                                        {
                                                            process.Refresh();

                                                            if ((process.HasExited && _config.CredVaultRemoveOnExit) ||
                                                                (process.MainWindowHandle != IntPtr.Zero && _config.CredVaultTtl > 0 && _config.CredVaultAdaptiveTtl))
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
                                                        }
                                                    }
                                                    else
                                                        break;
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

                                                        // Check if the window is still alive from time to time.
                                                        while (!process.HasExited) // !process.WaitForExit(timeout))
                                                        {
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

                                                            var lastPopup = NativeMethods.GetLastActivePopup(process.MainWindowHandle);
                                                            if (lastPopup != process.MainWindowHandle)
                                                            {
                                                                if (tmpUseCreds)
                                                                {
                                                                    var element = AutomationElement.FromHandle(lastPopup);

                                                                    // Enter credentials dialog.
                                                                    if (element.Current.ControlType == ControlType.Window &&
                                                                        element.Current.ClassName == "Credential Dialog Xaml Host")
                                                                    {
                                                                        timeout = timeoutInc;
                                                                        nextTimeout = timeout;

                                                                        var textBox = element.FindFirst(
                                                                            TreeScope.Subtree,
                                                                            new PropertyCondition(AutomationElement.AutomationIdProperty, "StaticTextField_0"));

                                                                        if (textBox != null)
                                                                        {
                                                                            var options = element.FindFirst(
                                                                                TreeScope.Subtree,
                                                                                new PropertyCondition(AutomationElement.AutomationIdProperty, "ChooseAnotherOption"));

                                                                            if (options != null)
                                                                            {
                                                                                var ip = (InvokePattern)options.GetCurrentPattern(InvokePattern.Pattern);

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
                                                                                        var last = items.OfType<AutomationElement>().LastOrDefault();

                                                                                        if (last != null)
                                                                                        {
                                                                                            var sp = (SelectionItemPattern)last.GetCurrentPattern(SelectionItemPattern.Pattern);
                                                                                            sp.Select();
                                                                                        }
                                                                                    }
                                                                                }

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

                                                                        var checkbox = element.FindFirst(
                                                                            TreeScope.Subtree,
                                                                            new PropertyCondition(AutomationElement.ClassNameProperty, "CheckBox"));

                                                                        if (checkbox != null)
                                                                        {
                                                                            var tp = (TogglePattern)checkbox.GetCurrentPattern(TogglePattern.Pattern);
                                                                            if (tp.Current.ToggleState == ToggleState.On)
                                                                                tp.Toggle();
                                                                        }

                                                                        if (retryOnce)
                                                                        {
                                                                            retryOnce = false;

                                                                            if (retryUsername != null)
                                                                            {
                                                                                if (textBox == null)
                                                                                    textBox = element.FindFirst(
                                                                                        TreeScope.Subtree,
                                                                                        new PropertyCondition(AutomationElement.AutomationIdProperty, "EditField_1"));

                                                                                if (textBox != null)
                                                                                {
                                                                                    var vp = (ValuePattern)textBox.GetCurrentPattern(ValuePattern.Pattern);
                                                                                    if (string.IsNullOrEmpty(vp.Current.Value))
                                                                                    {
                                                                                        if (retryCredEntry != null)
                                                                                        {
                                                                                            // KeePass.Util.EntryUtil.FillPlaceholders
                                                                                            var tmpEntry = retryCredEntry.GetResolvedReferencesEntry(new SprContext(retryCredEntry, mainForm.ActiveDatabase, _config.KeePassSprCompileFlags)
                                                                                            {
                                                                                                ForcePlainTextPasswords = true
                                                                                            });
                                                                                            retryUsername = tmpEntry.Strings.GetSafe(PwDefs.UserNameField);
                                                                                            if (forceLocalUser)
                                                                                                retryUsername = retryUsername.ForceLocalUser(host);
                                                                                            if (forceUpn)
                                                                                                retryUsername = retryUsername.ForceUPN();
                                                                                        }
                                                                                        var usernameChars = retryUsername.ReadChars();
                                                                                        var usernameString = new string(usernameChars);
                                                                                        vp.SetValue(usernameString);
                                                                                        MemoryUtil.SecureZeroMemory(usernameString);
                                                                                        MemoryUtil.SecureZeroMemory(usernameChars);
                                                                                    }
                                                                                }
                                                                            }

                                                                            if (retryPassword != null)
                                                                            {
                                                                                var pwBox = element.FindFirst(
                                                                                    TreeScope.Subtree,
                                                                                    new PropertyCondition(AutomationElement.AutomationIdProperty, "PasswordField_2"));

                                                                                if (pwBox != null)
                                                                                {
                                                                                    var vp = (ValuePattern)pwBox.GetCurrentPattern(ValuePattern.Pattern);
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
                                                                                }

                                                                                var button = element.FindFirst(
                                                                                    TreeScope.Children,
                                                                                    new PropertyCondition(AutomationElement.AutomationIdProperty, "OkButton"));

                                                                                if (button != null)
                                                                                {
                                                                                    var ip = (InvokePattern)button.GetCurrentPattern(InvokePattern.Pattern);
                                                                                    ip.Invoke();

                                                                                    if (retryPassword.Length > 0)
                                                                                    {
                                                                                        retryPassword.Dispose();
                                                                                        retryPassword = null;
                                                                                    }

                                                                                    if (retryCredEntry != null)
                                                                                        retryCredEntry = null;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    else
                                                                        retryOnce = retryUsername != null;
                                                                }
                                                            }
                                                            else
                                                                retryOnce = retryUsername != null;

                                                            if (process.HasExited || process.WaitForExit(timeout))
                                                                continue;

                                                            process.Refresh();

                                                            // Assume something went wrong when threads get stuck.
                                                            /*var allThreads = process.Threads.Cast<ProcessThread>();
                                                            if (!allThreads.Any(thread => thread.ThreadState != System.Diagnostics.ThreadState.Wait) &&
                                                                 allThreads.Any(thread => thread.WaitReason != ThreadWaitReason.Suspended))
                                                            {
                                                                if (!process.HasExited)
                                                                    process.Kill();
                                                                break;
                                                            }*/

                                                            // Assume something went wrong when there is no window anymore.
                                                            if (process.HasExited || process.MainWindowHandle == IntPtr.Zero)
                                                            {
                                                                if (!process.HasExited && !process.WaitForExit(timeout) && !process.HasExited)
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
                                                    }
                                                }
                                            }
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
                                                Util.RemoveHintFromRegistry(host);
                                        }

                                        var localTaskUuid = (string)thisTaskUuid;
                                        TaskWithCancellationToken localOldTask = null;
                                        if (_tasks.TryRemove(localTaskUuid, out localOldTask) && localOldTask != newTask && !localOldTask.IsCompleted)
                                            _tasks.TryAdd(localTaskUuid, localOldTask);

                                        newTask.ContinueWith(t =>
                                        {
                                            try
                                            {
                                                t.Wait();
                                            }
                                            catch (Exception ex)
                                            {
                                                VistaTaskDialog.ShowMessageBoxEx(
                                                    string.Format(
                                                        "{0}{1}{2}",
                                                        host,
                                                        Environment.NewLine,
                                                        ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message) ?
                                                            ex.InnerException.Message :
                                                            !string.IsNullOrWhiteSpace(ex.Message) ?
                                                                ex.Message :
                                                                ex.InnerException != null ?
                                                                    ex.InnerException.GetType().Name :
                                                                    ex.GetType().Name),
                                                    null,
                                                    string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Error),
                                                    VtdIcon.Error,
                                                    _host.MainWindow,
                                                    null, 0, null, 0);
                                            }
                                            finally
                                            {
                                                newTask.Dispose();
                                            }
                                        });
                                    }
                                }, taskUuid, new CancellationTokenSource());
                            }
                        }
                }

                mainForm.SetStatusEx(null);
            }
        }

        [DllImport("KeePassRDP.unmanaged.dll", EntryPoint = "KprDoDefaultAction", SetLastError = false)]
        private static extern int KprDoDefaultAction([In] IntPtr parent);

        /*[DllImport("KeePassRDP.unmanaged.dll", EntryPoint = "KprDoDefaultAction", SetLastError = false)]
        private static extern int KprDoDefaultAction([In] IntPtr parent, [In] string automationId);*/
    }
}