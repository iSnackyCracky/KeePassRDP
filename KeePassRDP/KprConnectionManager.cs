/*
 *  Copyright (C) 2018 - 2023 iSnackyCracky, NETertainer
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
using KeePassRDP.Extensions;
using KeePassRDP.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
                TaskCreationOptions.PreferFairness |
                    TaskCreationOptions.AttachedToParent |
                    TaskCreationOptions.LongRunning)
            {
                _cancellationTokenSource = cancellationTokenSource;
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

        public bool IsCompleted { get { return !_tasks.Values.Any(x => !x.IsCompleted); } }

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
                return Task.WaitAll(_tasks.Values.Where(x => !x.IsCompleted).ToArray(), TimeSpan.FromSeconds(seconds));
            }
            catch
            {
                return IsCompleted;
            }
        }

        public void Cancel()
        {
            foreach (var task in _tasks.Values.Where(x => !x.IsCompleted && !x.IsCancellationRequested))
                task.Cancel();
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
                var connectingTo = " " + KprResourceManager.Instance["connecting to"] + " ";
                var skipped = " " + KprResourceManager.Instance["skipped"];

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
                            else
                            {
                                credEntry = Util.IsEntryIgnored(credEntry) ?
                                    null :
                                    credEntry.GetResolvedReferencesEntry(new SprContext(credEntry, mainForm.ActiveDatabase, SprCompileFlags.NonActive)
                                    {
                                        ForcePlainTextPasswords = true // true is default, PwDefs.PasswordField is replaced with PwDefs.HiddenPassword during SprEngine.Compile otherwise.
                                    });
                            }
                        }
                    }

                    foreach (var connPwEntry in entries)
                    {
                        var ctx = new SprContext(connPwEntry, mainForm.ActiveDatabase, SprCompileFlags.NonActive)
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
                                Util.KeePassRDP + " - " + KPRes.Error,
                                VtdIcon.Error,
                                _host.MainWindow,
                                null, 0, null, 0);
                            continue;
                        }

                        var taskUuid = connPwEntry.Uuid.ToHexString();

                        using (var entrySettings = connPwEntry.GetKprSettings(true) ?? KprEntrySettings.Empty)
                        {
                            KprCredential cred = null;

                            // Connect to RDP using credentials from KeePass, skipping entries with no credentials.
                            if (tmpUseCreds)
                            {
                                // Use result from KprCredentialPicker or fallback to credentials from selected entry.
                                var shownInPicker = entrySettings.UseCredpicker &&
                                    (Util.InRdpSubgroup(connPwEntry, _config.CredPickerCustomGroup) || entrySettings.CpGroupUUIDs.Any());
                                var tmpEntry = credEntry != null && shownInPicker ? credEntry :
                                    entrySettings.Ignore || (credEntry != null && !shownInPicker) ? null :
                                        skippedEntries.Contains(connPwEntry) ?
                                        connPwEntry.GetResolvedReferencesEntry(ctx) :
                                        null;

                                if (tmpEntry == null)
                                    continue;

                                taskUuid += "-" + tmpEntry.Uuid.ToHexString();

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

                                var password = tmpEntry.Strings.GetSafe(PwDefs.PasswordField);
                                // Do not connect to entry if password is empty.
                                /*if (password.IsEmpty)
                                    continue;*/

                                if (entrySettings.ForceLocalUser)
                                    username = username.ForceLocalUser(host);

                                // Create new KprCredential.
                                cred = new KprCredential(
                                    username,
                                    password,
                                    host,
                                    _config.CredVaultUseWindows ?
                                        NativeCredentials.CRED_TYPE.DOMAIN_PASSWORD :
                                        NativeCredentials.CRED_TYPE.GENERIC,
                                    _config.CredVaultTtl);

                                //username = username.Remove(0, username.Length);
                                //password = password.Remove(0, password.Length);

                                // Add KprCredential to KprCredentialManager.
                                _credManager.Value.Add(cred);
                            }

                            if ((tmpUseCreds || _config.KeePassAlwaysConfirm) && _tasks.ContainsKey(taskUuid) && !_tasks[taskUuid].IsCompleted)
                                if (VistaTaskDialog.ShowMessageBoxEx(
                                    tmpUseCreds ?
                                        string.Format(KprResourceManager.Instance["Already connected with the same credentials to URL/target '{0}'."], host) :
                                        string.Format(KprResourceManager.Instance["Already connected to URL/target '{0}'."], host),
                                    KprResourceManager.Instance["Continue?"],
                                    Util.KeePassRDP,
                                    VtdIcon.Information,
                                    _host.MainWindow,
                                    KprResourceManager.Instance["&Yes"], 0,
                                    KprResourceManager.Instance["&No"], 1) == 1)
                                    continue;

                            var argumentsBuilder = new StringBuilder();
                            argumentsBuilder.Append("/v:");
                            argumentsBuilder.Append(host);
                            argumentsBuilder.Append(port);

                            if (entrySettings.IncludeDefaultParameters)
                            {
                                if (tmpMstscUseAdmin || _config.MstscUseAdmin)
                                {
                                    argumentsBuilder.Append(" /admin");
                                    if (_config.MstscUseRestrictedAdmin)
                                        argumentsBuilder.Append(" /restrictedAdmin");
                                }
                                if (_config.MstscUsePublic)
                                    argumentsBuilder.Append(" /public");
                                if (_config.MstscUseRemoteGuard)
                                    argumentsBuilder.Append(" /remoteGuard");
                                if (_config.MstscUseFullscreen)
                                    argumentsBuilder.Append(" /f");
                                if (_config.MstscUseSpan)
                                    argumentsBuilder.Append(" /span");
                                if (_config.MstscUseMultimon)
                                    argumentsBuilder.Append(" /multimon");
                                if (_config.MstscWidth > 0)
                                {
                                    argumentsBuilder.Append(" /w:");
                                    argumentsBuilder.Append(_config.MstscWidth);
                                }
                                if (_config.MstscHeight > 0)
                                {
                                    argumentsBuilder.Append(" /h:");
                                    argumentsBuilder.Append(_config.MstscHeight);
                                }
                            }
                            else if (tmpMstscUseAdmin)
                            {
                                argumentsBuilder.Append(" /admin");
                            }

                            foreach (var argument in entrySettings.MstscParameters)
                            {
                                argumentsBuilder.Append(" ");
                                argumentsBuilder.Append(argument);
                            }

                            // Start RDP / mstsc.exe.
                            var rdpProcess = new ProcessStartInfo
                            {
                                WindowStyle = ProcessWindowStyle.Normal,
                                FileName = KeePassRDPExt.MstscPath,
                                Arguments = argumentsBuilder.ToString(),
                                ErrorDialog = true,
                                ErrorDialogParentHandle = Form.ActiveForm.Handle,
                                LoadUserProfile = false,
                                WorkingDirectory = Environment.ExpandEnvironmentVariables("%TEMP%")
                            };

                            var title = string.Empty;
                            if (_config.MstscReplaceTitle)
                            {
                                title = SprEngine.Compile(connPwEntry.Strings.ReadSafe(PwDefs.TitleField), ctx);
                                title = string.Format("{0} - {1}", string.IsNullOrEmpty(title) ? host : title, Util.KeePassRDP);
                            }

                            (_tasks[taskUuid] = new TaskWithCancellationToken(thisTaskUuid =>
                            {
                                try
                                {
                                    using (var process = Process.Start(rdpProcess))
                                    {
                                        var inc = _config.CredVaultTtl > 0 && _config.CredVaultAdaptiveTtl && cred != null ?
                                            TimeSpan.FromSeconds(Math.Ceiling(Math.Max(1, _config.CredVaultTtl / 2f))) : TimeSpan.Zero;

                                        var ttl = (int)Math.Max(1000, inc.TotalMilliseconds);

                                        if (process.WaitForInputIdle(ttl))
                                            // Wait a limited time for mstsc.exe window, otherwise assume something went wrong.
                                            for (var spins = ttl / 250; spins > 0; spins--)
                                                if (process.MainWindowHandle != IntPtr.Zero || !process.WaitForExit(200))
                                                {
                                                    process.Refresh();

                                                    if (process.MainWindowHandle == IntPtr.Zero &&
                                                        !SpinWait.SpinUntil(() =>
                                                        {
                                                            process.Refresh();
                                                            return process.MainWindowHandle != IntPtr.Zero;
                                                        }, 50))
                                                    {
                                                        // Keep incrementing TTL as necessary.
                                                        if (cred != null)
                                                            cred.IncreaseTTL(inc);
                                                        continue;
                                                    }

                                                    var oldTitle = process.MainWindowTitle;

                                                    if (!string.IsNullOrEmpty(title))
                                                        SetWindowText(process.MainWindowHandle, title);

                                                    // Find progress bar.
                                                    var pbHandle = FindWindowEx(process.MainWindowHandle, IntPtr.Zero, "msctls_progress32", null);
                                                    if (pbHandle != IntPtr.Zero)
                                                    {
                                                        var ms = (int)TimeSpan.FromMilliseconds(750).TotalMilliseconds;
                                                        do
                                                        {
                                                            if (process.HasExited || process.WaitForExit(ms))
                                                                break;

                                                            process.Refresh();

                                                            if (process.MainWindowHandle == IntPtr.Zero)
                                                                break;

                                                            var lastPopup = GetLastActivePopup(process.MainWindowHandle);

                                                            // Continue when popup is open.
                                                            if (lastPopup != process.MainWindowHandle)
                                                            {
                                                                /*var sb = new char[GetWindowTextLength(lastPopup) + 1];
                                                                if (GetWindowText(lastPopup, sb, sb.Length) != 0 && new string(sb).Equals(oldTitle, StringComparison.OrdinalIgnoreCase))
                                                                {
                                                                }*/

                                                                var element = AutomationElement.FromHandle(lastPopup);

                                                                // Connection failed error box.
                                                                var button = element.FindFirst(
                                                                    TreeScope.Children,
                                                                    new PropertyCondition(AutomationElement.AutomationIdProperty, "CommandButton_1"));

                                                                if (button != null && button.Current.ClassName == "Button")
                                                                    break;

                                                                if (_config.MstscConfirmCertificate)
                                                                {
                                                                    // Confirm certificate error box.
                                                                    button = element.FindFirst(
                                                                        TreeScope.Children,
                                                                        new PropertyCondition(AutomationElement.AutomationIdProperty, "14004"));

                                                                    if (button != null && button.Current.ClassName == "Button")
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
                                                                            SendMessage(buttonHandle, WM_LBUTTONDOWN, 0, 0);
                                                                            SendMessage(buttonHandle, WM_LBUTTONUP, 0, 0);
                                                                            SendMessage(buttonHandle, BM_CLICK, 0, 0);
                                                                        }
                                                                        continue;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // Break when progress bar is gone.
                                                                //if (FindWindowEx(process.MainWindowHandle, IntPtr.Zero, "BBarWindowClass", "BBar") != IntPtr.Zero)
                                                                if (FindWindowEx(process.MainWindowHandle, IntPtr.Zero, "msctls_progress32", null) == IntPtr.Zero)
                                                                    break;
                                                            }

                                                            // Keep incrementing TTL as necessary.
                                                            if (cred != null)
                                                                cred.IncreaseTTL(inc);
                                                        } while (!process.HasExited);
                                                    }
                                                    else if (spins == 1 && cred != null && (_config.CredVaultRemoveOnExit || (_config.CredVaultTtl > 0 && _config.CredVaultAdaptiveTtl)))
                                                    {
                                                        cred.Dispose();
                                                        cred = null;
                                                    }
                                                }
                                                else
                                                    break;

                                        if (_config.CredVaultTtl > 0 && _config.CredVaultAdaptiveTtl && cred != null)
                                        {
                                            cred.Dispose();
                                            cred = null;
                                        }

                                        if (!process.HasExited)
                                        {
                                            process.Refresh();
                                            if (!string.IsNullOrEmpty(title) && process.MainWindowHandle != IntPtr.Zero)
                                                SetWindowText(process.MainWindowHandle, title);

                                            if (!process.HasExited && !process.WaitForExit(200))
                                            {
                                                process.Refresh();

                                                // Set title twice to try to make sure to catch the right window handle.
                                                if (!string.IsNullOrEmpty(title) && process.MainWindowHandle != IntPtr.Zero)
                                                    SetWindowText(process.MainWindowHandle, title);

                                                // Check if a window is still alive from time to time.
                                                while (!process.WaitForExit(30000))
                                                {
                                                    process.Refresh();

                                                    // Assume something went wrong when there is no window anymore.
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
                                finally
                                {
                                    if (cred != null)
                                    {
                                        if (_config.CredVaultTtl > 0 && _config.CredVaultAdaptiveTtl)
                                            cred.ResetTTL();
                                        if (_config.CredVaultRemoveOnExit)
                                            cred.Dispose();
                                    }
                                }

                                TaskWithCancellationToken oldtask;
                                if (_tasks.TryRemove((string)thisTaskUuid, out oldtask))
                                    oldtask.ContinueWith(t =>
                                    {
                                        try
                                        {
                                            if (!t.IsCompleted)
                                                t.Wait();
                                        }
                                        finally
                                        {
                                            oldtask.Dispose();
                                        }
                                    });
                            }, taskUuid, new CancellationTokenSource())).Start();
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

        [DllImport("User32.dll", EntryPoint = "FindWindowExW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);

        /*[DllImport("User32.dll", EntryPoint = "GetWindowTextLengthW", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("User32.dll", EntryPoint = "GetWindowTextW", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, [Out] char[] lpString, int nMaxCount);*/

        [DllImport("User32.dll", EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern int SetWindowText(IntPtr hWnd, string text);

        [DllImport("User32.dll", EntryPoint = "GetLastActivePopup", SetLastError = true)]
        private static extern IntPtr GetLastActivePopup(IntPtr hWnd);

        private const uint BM_CLICK = 0x00F5;

        private const uint WM_LBUTTONDOWN = 0x0201;

        private const uint WM_LBUTTONUP = 0x0202;

        [DllImport("User32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
    }
}