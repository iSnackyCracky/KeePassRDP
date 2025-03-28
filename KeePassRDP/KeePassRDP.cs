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

using KeePass;
using KeePass.App.Configuration;
using KeePass.Ecas;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Utility;
using KeePassRDP.Extensions;
using KeePassRDP.Resources;
using KeePassRDP.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP
{
    public sealed class KeePassRDPExt : Plugin
    {
        internal static readonly string MstscPath = Environment.ExpandEnvironmentVariables(Util.DefaultMstscPath);

        internal static readonly ManualResetEventSlim Initialized = new ManualResetEventSlim(false);

        private static readonly Lazy<Image> _smallIcon = new Lazy<Image>(() =>
        {
            using (var icon = IconUtil.ExtractIcon(MstscPath, 0, UIUtil.GetSmallIconSize().Height))
                return icon.ToBitmap();
        });

        // KeePass.Ecas.EcasEventIDs.AppInitPost
        private static readonly PwUuid _appInitPost = new PwUuid(new byte[]
        {
            0xD4, 0xCE, 0xCD, 0xB5, 0x4B, 0x98, 0x4F, 0xF2,
            0xA6, 0xA9, 0xE2, 0x55, 0x26, 0x1E, 0xC8, 0xE8
        });

        private static readonly ManualResetEventSlim _toolbarButtonsAdded = new ManualResetEventSlim(false);

        private readonly Lazy<KprCredentialManager<KprCredential>> _credManager;
        private readonly Dictionary<KprMenu.MenuItem, ToolStripItem> _toolbarItems;
        private readonly Lazy<ToolStripMenuItem> _toolStripMenuItem;

        private DateTimeOffset? _isWaitingOnCloseStart;
        private ManualResetEventSlim _isWaitingOnCloseSignal;
        private Lazy<KprConnectionManager> _connectionManager;
        private IPluginHost _host;
        private KprConfig _config;

        public override string UpdateUrl { get { return Util.UpdateUrl; } }
        public override Image SmallIcon { get { return _smallIcon.Value; } }

        public KeePassRDPExt()
        {
            PreloadJit();

            _credManager = new Lazy<KprCredentialManager<KprCredential>>(() => new KprCredentialManager<KprCredential>(_config), LazyThreadSafetyMode.ExecutionAndPublication);
            _toolbarItems = new Dictionary<KprMenu.MenuItem, ToolStripItem>();
            _toolStripMenuItem = new Lazy<ToolStripMenuItem>(() =>
            {
                var tsmi = new ToolStripMenuItem(Util.KeePassRDP)
                {
                    /*DropDown = new ToolStripDropDown
                    {
                        AutoSize = true,
                        Dock = DockStyle.Fill,
                        LayoutStyle = ToolStripLayoutStyle.Table,
                        ShowItemToolTips = false,
                        DropShadowEnabled = true,
                        AutoClose = true,
                        DefaultDropDownDirection = ToolStripDropDownDirection.Default,
                        Margin = Padding.Empty,
                        Padding = new Padding(1, 2, 1, 2)
                    },*/
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    AutoToolTip = false,
                    ShowShortcutKeys = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ImageAlign = ContentAlignment.MiddleLeft,
                    TextImageRelation = TextImageRelation.ImageBeforeText,
                    DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
                };

                tsmi.DropDown.LayoutStyle = ToolStripLayoutStyle.Table;
                var tls = tsmi.DropDown.LayoutSettings as TableLayoutSettings;
                tls.ColumnCount = 1;
                tls.RowCount = 1;
                tls.ColumnStyles.Clear();
                tls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                tls.RowStyles.Clear();
                tls.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tls.GrowStyle = TableLayoutPanelGrowStyle.AddRows;

                return tsmi;
            }, LazyThreadSafetyMode.ExecutionAndPublication);
            _isWaitingOnCloseStart = null;
            _isWaitingOnCloseSignal = null;
            _connectionManager = null;
            _host = null;
            _config = null;
        }

        private static void PreloadJit()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var method in Assembly.GetExecutingAssembly().GetTypes()
                    .Where(type => typeof(Form).IsAssignableFrom(type))
                    .SelectMany(type => type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)))
                {
                    if (method.Attributes.HasFlag(MethodAttributes.Abstract) || method.ContainsGenericParameters)
                        continue;
                    RuntimeHelpers.PrepareMethod(method.MethodHandle);
                }
            }, CancellationToken.None, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
        }

        public override bool Initialize(IPluginHost host)
        {
            Terminate();

            if (host == null)
                return false;

            Task.Factory.StartNew(() =>
            {
                var kprPlgx = Path.Combine(Path.GetDirectoryName(WinUtil.GetExecutable()), "Plugins", "KeePassRDP.plgx");

                try
                {
                    if (File.Exists(kprPlgx) || File.Exists(kprPlgx = Path.Combine(AppConfigSerializer.AppDataDirectory, "Plugins", "KeePassRDP.plgx")))
                    {
                        string baseFileName = null;
                        byte[] guidBytes = null;

                        using (var fs = File.Open(kprPlgx, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var br = new BinaryReader(fs))
                        {
                            br.ReadUInt32();
                            br.ReadUInt32();
                            br.ReadUInt32();

                            var bt = 0;
                            while ((bt = br.ReadUInt16()) != 0) // KeePass.Plugins.PlgxPlugin.PlgxEOF
                            {
                                var bl = br.ReadUInt32();
                                var bb = bl > 0 ? br.ReadBytes((int)bl) : null;
                                if (bb != null)
                                {
                                    switch (bt)
                                    {
                                        case 1: // KeePass.Plugins.PlgxPlugin.PlgxFileUuid
                                            guidBytes = bb;
                                            break;
                                        case 2: // KeePass.Plugins.PlgxPlugin.PlgxBaseFileName
                                            baseFileName = Encoding.UTF8.GetString(bb);
                                            break;
                                    }

                                    if (guidBytes != null && baseFileName != null)
                                        break;
                                }
                            }
                        }
                        if (guidBytes != null && !string.IsNullOrWhiteSpace(baseFileName))
                        {
                            var cacheDir = Path.Combine(PlgxCache.GetCacheDirectory(new PlgxPluginInfo(false, false, false)
                            {
                                BaseFileName = baseFileName,
                                FileUuid = new PwUuid(guidBytes)
                            }, false), "de");

                            if (!Directory.Exists(cacheDir))
                                using (var mrs = new ManualResetEventSlim(false))
                                    for (var i = 0; i < 3; i++)
                                    {
                                        mrs.Wait(TimeSpan.FromSeconds(1));

                                        if (Directory.Exists(cacheDir))
                                            break;
                                    }

                            if (!Directory.Exists(cacheDir))
                                MessageBox.Show(cacheDir);
                        }
                    }
                }
                catch { }
                finally
                {
                    Initialized.Set();
                }
            }, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

            _host = host;
            _config = new KprConfig(_host.CustomConfig);
            _connectionManager = new Lazy<KprConnectionManager>(() => new KprConnectionManager(_host, _config, _credManager), LazyThreadSafetyMode.ExecutionAndPublication);

            var mainForm = _host.MainWindow;

            Task.Factory.FromAsync(mainForm.BeginInvoke(new Action(() =>
            {
                AddToolbarButtons();
                _toolStripMenuItem.Value.DropDownOpening += DropDownOpening;
            })), endInvoke => mainForm.EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

            _host.TriggerSystem.RaisingEvent += TriggerSystem_RaisingEvent_AppInitPost;
            if (!_host.TriggerSystem.Enabled)
            {
                Task.Factory.StartNew(() =>
                {
                    using(var mrs = new ManualResetEventSlim(false))
                    {
                        mrs.Wait(TimeSpan.FromSeconds(1));
                        TriggerSystem_RaisingEvent_AppInitPost(null, new EcasRaisingEventArgs(new EcasEvent() { Type = _appInitPost }, new EcasPropertyDictionary()));
                    }
                }, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }

            GlobalWindowManager.WindowAdded += GlobalWindowManager_WindowAdded;
            GlobalWindowManager.WindowRemoved += GlobalWindowManager_WindowRemoved;

            mainForm.EntryContextMenu.Opening += ContextMenuOpening;
            (mainForm.MainMenuStrip.Items["m_menuEntry"] as ToolStripMenuItem).DropDownOpening += EntryMenuOpening;

            mainForm.UIStateUpdated += MainWindow_UIStateUpdated;
            mainForm.FormClosing += MainWindow_FormClosing;
            mainForm.FormClosed += MainWindow_FormClosed;
            mainForm.Deactivate += MainWindow_Deactivate;
            mainForm.DefaultEntryAction += MainWindow_DefaultEntryAction;
            PwEntry.EntryTouched += PwEntry_EntryTouched;

            if (UpdateUrl.EndsWith(".gz"))
                UpdateCheckEx.SetFileSigKey(UpdateUrl, KprVersion.FileSigKey);

            return true;
        }

        private void PwEntry_EntryTouched(object sender, ObjectTouchedEventArgs e)
        {
            if (e.Modified)
            {
                var pwEntry = e.Object as PwEntry;

                // Reset ignored state when username and password are unset.
                if (Util.IsEntryIgnored(pwEntry) &&
                    pwEntry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty &&
                    pwEntry.Strings.GetSafe(PwDefs.PasswordField).IsEmpty)
                {
                    pwEntry.ToggleKprIgnored(false);
                }
            }
        }

        private void MainWindow_DefaultEntryAction(object sender, CancelEntryEventArgs e)
        {
            if (!_config.KeePassDefaultEntryAction || e.Cancel)
                return;

            if (Control.ModifierKeys != Keys.None)
                return;

            var col = Program.Config.MainWindow.EntryListColumns[e.ColumnId];
            switch (col.Type)
            {
                case AceColumnType.Url:
                    var kprCpcg = _config.CredPickerCustomGroup;
                    var kprCptr = _config.CredPickerTriggerRecursive;
                    using (var entrySettings = e.Entry.GetKprSettings(true) ?? KprEntrySettings.Empty)
                        if (entrySettings.UseCredpicker && Util.InRdpSubgroup(e.Entry, kprCpcg, kprCptr))
                        {
                            var kpScf = _config.KeePassSprCompileFlags;
                            var ctx = new SprContext(e.Entry, _host.Database, kpScf)
                            {
                                ForcePlainTextPasswords = false
                            };

                            var hostname = SprEngine.Compile(e.Entry.Strings.ReadSafe(PwDefs.UrlField), ctx);

                            if (string.IsNullOrWhiteSpace(hostname))
                                return;

                            hostname = hostname.Trim();

                            var uri = Util.ParseURL(hostname);

                            if (uri != null && uri.HostNameType != UriHostNameType.Unknown && uri.IsRdpScheme())
                            {
                                //_toolbarItems[KprMenu.MenuItem.OpenRdpConnection].PerformClick();
                                var tsmi = _toolStripMenuItem.Value;
                                var idx = tsmi.DropDownItems.IndexOfKey(KprMenu.MenuItem.OpenRdpConnection.ToString());
                                if (idx >= 0)
                                {
                                    tsmi.DropDownItems[idx].PerformClick();
                                    e.Cancel = true;
                                }
                            }
                        }
                    break;
            }
        }

        private void MainWindow_FormClosed(object sender, EventArgs e)
        {
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

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_config.KeePassConfirmOnClose || (e != null && e.Cancel))
            {
                if ((e == null || !e.Cancel) && _connectionManager != null && _connectionManager.IsValueCreated)
                    _connectionManager.Value.Dispose();

                if (SecureDesktop.IsValueCreated)
                    SecureDesktop.Instance.Dispose();
                return;
            }

            if (_connectionManager != null && _connectionManager.IsValueCreated)
            {
                if (_connectionManager.Value.IsCompleted)
                {
                    _connectionManager.Value.Dispose();
                    if (SecureDesktop.IsValueCreated)
                        SecureDesktop.Instance.Dispose();
                }
                else
                {
                    if (sender != null)
                    {
                        if (_isWaitingOnCloseSignal != null)
                            _isWaitingOnCloseSignal.Reset();

                        if (_connectionManager != null && !_connectionManager.Value.IsCompleted && (_isWaitingOnCloseStart == null || _isWaitingOnCloseSignal != null))
                        {
                            var firstRun = _isWaitingOnCloseStart == null;
                            if (firstRun)
                            {
                                _host.MainWindow.UseWaitCursor = true;
                                foreach (var control in _host.MainWindow.Controls.OfType<Control>())
                                    control.Enabled = false;

                                _isWaitingOnCloseStart = DateTimeOffset.UtcNow;
                                _isWaitingOnCloseSignal = new ManualResetEventSlim(false);

                                Task.Factory.StartNew(() =>
                                {
                                    try
                                    {
                                        while (_connectionManager != null &&
                                            !_connectionManager.Value.IsCompleted &&
                                            !_connectionManager.Value.Wait(5))
                                        {
                                            if (_isWaitingOnCloseSignal != null && !_isWaitingOnCloseSignal.IsSet && !_isWaitingOnCloseSignal.Wait(TimeSpan.FromSeconds(5)))
                                                continue;
                                            if (DateTimeOffset.UtcNow - _isWaitingOnCloseStart >= TimeSpan.FromSeconds(15))
                                                _host.MainWindow.Close();
                                        }

                                        if (_isWaitingOnCloseSignal != null && _isWaitingOnCloseSignal.IsSet)
                                        {
                                            var lastPopup = NativeMethods.GetLastActivePopup(_host.MainWindow.Handle);
                                            if (lastPopup != IntPtr.Zero)
                                            {
                                                var sb = new char[NativeMethods.GetWindowTextLength(lastPopup) + 1];
                                                if (NativeMethods.GetWindowText(lastPopup, sb, sb.Length) != 0 &&
                                                    new string(sb).TrimEnd(char.MinValue).Equals(Util.KeePassRDP, StringComparison.OrdinalIgnoreCase))
                                                    NativeMethods.SendMessage(lastPopup, NativeMethods.WM_CLOSE, 0, 0);
                                            }
                                        }

                                        if (_connectionManager != null && _connectionManager.Value.IsCompleted)
                                        {
                                            _connectionManager.Value.Dispose();
                                            if (SecureDesktop.IsValueCreated)
                                                SecureDesktop.Instance.Dispose();
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    finally
                                    {
                                        _isWaitingOnCloseStart = null;
                                        _host.MainWindow.Close();
                                    }
                                }, CancellationToken.None, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
                            }
                            else
                                _host.MainWindow.SetStatusEx(
                                    string.Format("{0} {1}",
                                        Util.KeePassRDP,
                                        _connectionManager != null && _connectionManager.Value.Count == 1 ?
                                            KprResourceManager.Instance["waiting for 1 connection..."] :
                                            string.Format(KprResourceManager.Instance["waiting for {0} connections..."], _connectionManager != null ? _connectionManager.Value.Count : 0)));

                            if (_isWaitingOnCloseSignal != null && !_isWaitingOnCloseSignal.IsSet && _connectionManager != null && !_connectionManager.Value.IsCompleted)
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    using (var qicon = SystemIcons.Question)
                                    using (var icon = IconUtil.ExtractIcon(MstscPath, 0, DpiUtil.ScaleIntY(qicon.Size.Height)))
                                    {
                                        var vistaTaskDialog = new VistaTaskDialog
                                        {
                                            CommandLinks = true,
                                            WindowTitle = Util.KeePassRDP,
                                            Content = string.Format(KprResourceManager.Instance["{0} connection" + (_connectionManager != null && _connectionManager.Value.Count == 1 ? " is" : "s are") + " still open."], _connectionManager != null ? _connectionManager.Value.Count : 0),
                                            MainInstruction = KprResourceManager.Instance[KPRes.AskContinue]
                                        };
                                        if (icon != null)
                                            vistaTaskDialog.SetIcon(icon.Handle);
                                        vistaTaskDialog.AddButton(
                                            6,
                                            KprResourceManager.Instance[string.Format("&{0}", KPRes.Wait)],
                                            KprResourceManager.Instance["Wait until all connections are finished and cleaned up."]);
                                        vistaTaskDialog.AddButton(
                                            7,
                                            KprResourceManager.Instance[string.Format("&{0}", KPRes.Exit)],
                                            KprResourceManager.Instance["Close KeePass without waiting for connections to finish."]);
                                        if (SecureDesktop.IsValueCreated && SecureDesktop.Instance.IsAlive && !SecureDesktop.Instance.IsCancellationRequested)
                                            vistaTaskDialog.VerificationText = KprResourceManager.Instance["Ignore windows on secure desktop."];
                                        if (/*VistaTaskDialog.ShowMessageBoxEx(
                                            string.Format(KprResourceManager.Instance["{0} connection" + (_connectionManager.Value.Count == 1 ? " is" : "s are") + " still open."], _connectionManager.Value.Count),
                                            KprResourceManager.Instance[KPRes.AskContinue],
                                            Util.KeePassRDP,
                                            VtdIcon.Information,
                                            _host.MainWindow,
                                            KprResourceManager.Instance[string.Format("&{0}", KPRes.Wait)], 0,
                                            KprResourceManager.Instance[string.Format("&{0}", KPRes.Exit)], 1) == 0*/
                                            vistaTaskDialog.ShowDialog(_host.MainWindow) &&
                                            (vistaTaskDialog.Result == 6 || (!string.IsNullOrEmpty(vistaTaskDialog.VerificationText) && !vistaTaskDialog.ResultVerificationChecked)))
                                        {
                                            if (_isWaitingOnCloseSignal != null)
                                            {
                                                _isWaitingOnCloseStart = DateTimeOffset.UtcNow;
                                                _isWaitingOnCloseSignal.Set();
                                            }

                                            _host.MainWindow.SetStatusEx(
                                                string.Format("{0} {1}",
                                                    Util.KeePassRDP,
                                                    _connectionManager != null && _connectionManager.Value.Count == 1 ?
                                                        KprResourceManager.Instance["waiting for 1 connection..."] :
                                                        string.Format(KprResourceManager.Instance["waiting for {0} connections..."], _connectionManager != null ? _connectionManager.Value.Count : 0)));

                                            if (_connectionManager == null || _connectionManager.Value.IsCompleted)
                                                _host.MainWindow.Close();
                                        }
                                        else if (_isWaitingOnCloseSignal != null)
                                        {
                                            using (_isWaitingOnCloseSignal)
                                                if (!_isWaitingOnCloseSignal.IsSet)
                                                    _isWaitingOnCloseSignal.Set();
                                            _isWaitingOnCloseSignal = null;

                                            if (_connectionManager != null && !_connectionManager.Value.IsCompleted)
                                                _connectionManager.Value.Cancel(false);
                                            _host.MainWindow.Close();
                                        }
                                    }
                                }, CancellationToken.None, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);

                                if (_connectionManager != null && !_connectionManager.Value.IsCompleted)
                                {
                                    e.Cancel = true;
                                    _host.MainWindow.UpdateUI(true, null, true, null, true, null, false);
                                    return;
                                }
                            }
                        }
                    }

                    if (_connectionManager != null && _connectionManager.Value.IsCompleted)
                    {
                        _connectionManager.Value.Dispose();
                        if (SecureDesktop.IsValueCreated)
                            SecureDesktop.Instance.Dispose();
                    }
                }

                _connectionManager = null;
            }

            if (_isWaitingOnCloseSignal != null)
            {
                using (_isWaitingOnCloseSignal)
                    if (!_isWaitingOnCloseSignal.IsSet)
                        _isWaitingOnCloseSignal.Set();
                _isWaitingOnCloseSignal = null;

                var lastPopup = NativeMethods.GetLastActivePopup(_host.MainWindow.Handle);
                if (lastPopup != IntPtr.Zero)
                {
                    var sb = new char[NativeMethods.GetWindowTextLength(lastPopup) + 1];
                    if (NativeMethods.GetWindowText(lastPopup, sb, sb.Length) != 0 &&
                        new string(sb).TrimEnd(char.MinValue).Equals(Util.KeePassRDP, StringComparison.OrdinalIgnoreCase))
                        NativeMethods.SendMessage(lastPopup, NativeMethods.WM_CLOSE, 0, 0);
                }
            }
        }

        public override void Terminate()
        {
            Initialized.Reset();
            _isWaitingOnCloseStart = null;

            if (_host == null)
                return;

            if (_credManager.IsValueCreated)
                _credManager.Value.Clear();

            var mainForm = _host.MainWindow;

            PwEntry.EntryTouched -= PwEntry_EntryTouched;
            mainForm.DefaultEntryAction -= MainWindow_DefaultEntryAction;
            mainForm.Deactivate -= MainWindow_Deactivate;
            mainForm.FormClosing -= MainWindow_FormClosing;
            mainForm.FormClosed -= MainWindow_FormClosed;
            mainForm.UIStateUpdated -= MainWindow_UIStateUpdated;

            (mainForm.MainMenuStrip.Items["m_menuEntry"] as ToolStripMenuItem).DropDownOpening -= EntryMenuOpening;
            mainForm.EntryContextMenu.Opening -= ContextMenuOpening;

            GlobalWindowManager.WindowAdded -= GlobalWindowManager_WindowAdded;
            GlobalWindowManager.WindowRemoved -= GlobalWindowManager_WindowRemoved;

            try
            {
                _host.TriggerSystem.RaisingEvent -= TriggerSystem_RaisingEvent_AppInitPost;
            }
            catch
            {
                _host.TriggerSystem.RaisingEvent -= TriggerSystem_RaisingEvent;
            }

            if (_toolStripMenuItem.IsValueCreated)
            {
                var tsmi = _toolStripMenuItem.Value;
                tsmi.DropDownOpening -= DropDownOpening;
                foreach (ToolStripItem tsmiItem in tsmi.DropDownItems)
                {
                    KprMenu.MenuItem menu;
                    if (Enum.TryParse(tsmiItem.Name, out menu))
                        switch (menu)
                        {
                            case KprMenu.MenuItem.OpenRdpConnection:
                                tsmiItem.Click -= OnOpenRDP_Click;
                                break;
                            case KprMenu.MenuItem.OpenRdpConnectionAdmin:
                                tsmiItem.Click -= OnOpenRDPAdmin_Click;
                                break;
                            case KprMenu.MenuItem.OpenRdpConnectionNoCred:
                                tsmiItem.Click -= OnOpenRDPNoCred_Click;
                                break;
                            case KprMenu.MenuItem.OpenRdpConnectionNoCredAdmin:
                                tsmiItem.Click -= OnOpenRDPNoCredAdmin_Click;
                                break;
                            case KprMenu.MenuItem.ShadowSession:
                                tsmiItem.Click -= OnShadowSession_Click;
                                break;
                            case KprMenu.MenuItem.ShadowSessionNoCred:
                                tsmiItem.Click -= OnShadowSessionNoCred_Click;
                                break;
                            case KprMenu.MenuItem.IgnoreCredentials:
                                tsmiItem.Click -= OnIgnoreCredEntry_Click;
                                break;
                            default:
                                continue;
                        }
                }
                tsmi.DropDownItems.Clear();
            }

            _toolbarButtonsAdded.Reset();
            foreach (var menu in KprMenu.MenuItemValues)
                mainForm.RemoveCustomToolBarButton(menu.ToString());

            foreach (var button in _toolbarItems)
            {
                if (button.Key != KprMenu.MenuItem.IgnoreCredentials)
                    button.Value.Image.Dispose();
                else
                    button.Value.Paint -= IgnoreButtonPaint;
                if (!_host.TriggerSystem.Enabled)
                    button.Value.Click -= OnToolstripItem_Click;
            }
            _toolbarItems.Clear();

            MainWindow_FormClosing(null, null);

            _config = null;
            _host = null;

            KprResourceManager.Instance.ClearCache();
        }

        private void MainWindow_Deactivate(object sender, EventArgs e)
        {
            if (SecureDesktop.IsValueCreated)
                SecureDesktop.Instance.ToolBarBringToFront();
        }

        private void GlobalWindowManager_WindowAdded(object sender, GwmWindowEventArgs e)
        {
            if (e.Form is PwEntryForm)
                AddKprTab(e.Form as PwEntryForm);
        }

        private void GlobalWindowManager_WindowRemoved(object sender, GwmWindowEventArgs e)
        {
            if (e.Form is EcasTriggersForm)
                foreach (var button in _toolbarItems)
                {
                    try
                    {
                        button.Value.Click -= OnToolstripItem_Click;
                    }
                    catch
                    {
                    }

                    if (!_host.TriggerSystem.Enabled)
                        button.Value.Click += OnToolstripItem_Click;
                }
        }

        private void TriggerSystem_RaisingEvent_AppInitPost(object sender, EcasRaisingEventArgs e)
        {
            if (e.Event.Type.Equals(_appInitPost))
            {
                // Force initialization of KprCredentialManager to clean up left over invalid credentials.
                Task.Factory.StartNew(() =>
                {
                    if (!_credManager.IsValueCreated && _credManager.Value.Count > 0)
                        return true;
                    return false;
                }, CancellationToken.None, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);

                if (_host.TriggerSystem.Enabled)
                {
                    _host.TriggerSystem.RaisingEvent -= TriggerSystem_RaisingEvent_AppInitPost;
                    _host.TriggerSystem.RaisingEvent += TriggerSystem_RaisingEvent;
                }

                Task.Factory.FromAsync(_host.MainWindow.BeginInvoke(new Action(() =>
                {
                    if (!_toolbarButtonsAdded.IsSet)
                        if (!_toolbarButtonsAdded.Wait(TimeSpan.FromSeconds(10)))
                            throw new TimeoutException();

                    MoveToolbarButtons();
                    ShowHideToolbarItems();

                    if (_config.KeePassHotkeysRegisterLast)
                        AssignShortcuts();
                })), endInvoke => _host.MainWindow.EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

                SecureDesktop.SetToolStripRenderer(ToolStripManager.Renderer);
            }
        }

        private void TriggerSystem_RaisingEvent(object sender, EcasRaisingEventArgs e)
        {
            if (e.Cancel)
                return;

            var dict = e.Properties;

            if (dict == null)
                return;

            var strID = dict.Get<string>(EcasProperty.CommandID) ?? string.Empty;

            if (string.IsNullOrEmpty(strID))
                return;

            /*if (_toolStripMenuItem.DropDownItems.ContainsKey(strID))
                _toolStripMenuItem.DropDownItems[strID].PerformClick();*/

            if (_toolStripMenuItem.IsValueCreated)
            {
                var tsmi = _toolStripMenuItem.Value;
                var idx = tsmi.DropDownItems.IndexOfKey(strID);
                if (idx >= 0)
                    tsmi.DropDownItems[idx].PerformClick();
            }
        }

        private void MainWindow_UIStateUpdated(object sender, EventArgs e)
        {
            EnableDisableToolbarItems();
            EnableDisableContextMenuItems();
        }

        private void IgnoreButtonPaint(object sender, PaintEventArgs e)
        {
            var isDark = UIUtil.IsDarkTheme;
            var darkColor = isDark ? SystemColors.ControlLight : SystemColors.ControlDark;
            var lightColor = isDark ? SystemColors.ControlDark : SystemColors.ControlLight;
            var btn = sender as ToolStripButton;
            ControlPaint.DrawBorder(
                   e.Graphics,
                   btn.ContentRectangle,
                   btn.Enabled ? darkColor : lightColor,
                   ButtonBorderStyle.Solid);
        }

        private void AddToolbarButtons()
        {
            if (_toolbarButtonsAdded.IsSet)
                return;

            var mainForm = _host.MainWindow;
            var toolStrip = mainForm.Controls["m_toolMain"] as CustomToolStripEx;

            toolStrip.SuspendLayout();

            if (toolStrip.ImageList == null)
                toolStrip.ImageList = new ImageList
                {
                    ColorDepth = ColorDepth.Depth32Bit,
                    ImageSize = UIUtil.GetSmallIconSize(),
                    TransparentColor = Color.Transparent
                };

            var iconSize = toolStrip.ImageList.ImageSize.Height;
            using (var shield = SystemIcons.Shield)
            using (var bmpShield = shield.ToBitmap())
            using (var icon = IconUtil.ExtractIcon(MstscPath, 4, iconSize))
            using (var icon2 = IconUtil.ExtractIcon(MstscPath, 10, iconSize))
                foreach (var menu in KprMenu.MenuItemValues)
                {
                    var menuString = menu.ToString();
                    mainForm.AddCustomToolBarButton(menuString, menuString, menu.GetText());

                    var button = toolStrip.Items[toolStrip.Items.Count - 1];
                    button.Enabled = false;
                    button.Name = menuString;
                    button.DisplayStyle = ToolStripItemDisplayStyle.Image;

                    // Replace custom toolbar button text with image.
                    if (menu == KprMenu.MenuItem.IgnoreCredentials)
                    {
                        if (!toolStrip.ImageList.Images.ContainsKey(menuString) && KprImageList.Instance.Images.ContainsKey("Checkmark"))
                            toolStrip.ImageList.Images.Add(menuString, KprImageList.Instance.Images["Checkmark"]);
                        button.Paint += IgnoreButtonPaint;
                    }
                    else
                    {
                        if (!toolStrip.ImageList.Images.ContainsKey(menuString))
                        {
                            Bitmap bmp;
                            switch (menu)
                            {
                                case KprMenu.MenuItem.OpenRdpConnectionAdmin:
                                case KprMenu.MenuItem.OpenRdpConnectionNoCredAdmin:
                                    var widthScaled = icon.Width * 0.67f;
                                    var heightScaled = icon.Height * 0.67f;
                                    using (var g = Graphics.FromImage(bmp = icon.ToBitmap()))
                                    {
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                        g.SmoothingMode = SmoothingMode.HighQuality;
                                        g.CompositingQuality = CompositingQuality.HighQuality;
                                        g.DrawImage(
                                            bmpShield,
                                            new RectangleF(bmp.Width - widthScaled + 1, bmp.Height - heightScaled, widthScaled, heightScaled),
                                            new RectangleF(0, 0, bmpShield.Width, bmpShield.Height),
                                            GraphicsUnit.Pixel);
                                    }
                                    break;
                                case KprMenu.MenuItem.ShadowSession:
                                case KprMenu.MenuItem.ShadowSessionNoCred:
                                    bmp = icon2.ToBitmap();
                                    break;
                                default:
                                    bmp = icon.ToBitmap();
                                    break;
                            }
                            if (icon.Height < iconSize)
                            {
                                using (bmp)
                                    toolStrip.ImageList.Images.Add(menuString, GfxUtil.ScaleImage(bmp, iconSize, iconSize));
                            }
                            else
                                toolStrip.ImageList.Images.Add(menuString, bmp);
                        }
                        button.ImageKey = menuString;
                    }

                    button.Available = button.Visible = _config.KeePassToolbarItems.HasFlag(menu);
                    if (!_host.TriggerSystem.Enabled)
                        button.Click += OnToolstripItem_Click;

                    _toolbarItems.Add(menu, button);
                }

            toolStrip.ResumeLayout(false);

            _toolbarButtonsAdded.Set();
        }

        private void MoveToolbarButtons()
        {
            var toolStrip = _host.MainWindow.Controls["m_toolMain"] as CustomToolStripEx;

            var firstIndex = toolStrip.Items.IndexOf(_toolbarItems[KprMenu.MenuItem.OpenRdpConnection]);
            if (firstIndex > 0 && !(toolStrip.Items[firstIndex - 1] is ToolStripSeparator))
            {
                var lastSeperator = toolStrip.Items.OfType<ToolStripItem>().Last(x => x is ToolStripSeparator);
                var seperatorIndex = toolStrip.Items.IndexOf(lastSeperator) + 1;

                toolStrip.SuspendLayout();

                foreach (var item in _toolbarItems.Values.Reverse())
                {
                    toolStrip.Items.Remove(item);
                    toolStrip.Items.Insert(seperatorIndex, item);
                }

                if (toolStrip.Items.Count > seperatorIndex + _toolbarItems.Count)
                    toolStrip.Items.Insert(seperatorIndex + _toolbarItems.Count, new ToolStripSeparator());

                toolStrip.ResumeLayout(false);
            }
        }

        private void AddKprTab(PwEntryForm pwEntryForm)
        {
            if (pwEntryForm.Controls.ContainsKey("m_tabMain") &&
                pwEntryForm.Controls["m_tabMain"] is TabControl)
            {
                var tabMain = pwEntryForm.Controls["m_tabMain"] as TabControl;

                if (tabMain.ImageList == null)
                    tabMain.ImageList = new ImageList
                    {
                        ColorDepth = ColorDepth.Depth32Bit,
                        ImageSize = UIUtil.GetSmallIconSize(),
                        TransparentColor = Color.Transparent
                    };

                if (!tabMain.ImageList.Images.ContainsKey(Util.KeePassRDP))
                    tabMain.ImageList.Images.Add(Util.KeePassRDP, SmallIcon);

                var tabPage = new TabPage(Util.KeePassRDP)
                {
                    Name = Util.KeePassRDP
                };
                tabMain.TabPages.Add(tabPage);

                // Assigning the ImageKey before adding a TabPage to a TabControl fails.
                tabPage.ImageKey = Util.KeePassRDP;

                var readOnly = pwEntryForm.EditModeEx == PwEditMode.ViewReadOnlyEntry;

                KprEntrySettings peEntrySettings = null;
                TabControlEventHandler tabMainSelected = (s, ee) =>
                {
                    if (ee.TabPage == tabPage)
                    {
                        if (ee.TabPage.Controls.Count == 0)
                        {
                            peEntrySettings = pwEntryForm.EntryRef.GetKprSettings(readOnly) ?? (readOnly ? KprEntrySettings.Empty : new KprEntrySettings());

                            var tab = new KprEntrySettingsTab(peEntrySettings)
                            {
                                Width = ee.TabPage.Width,
                                Height = ee.TabPage.Height
                            };

                            //var updateEntryMoveMenu = typeof(MainForm).GetMethod("UpdateEntryMoveMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                            //updateEntryMoveMenu.Invoke(m_host.MainWindow, new object[] { tab.DynamicMenu, false });
                            tab.UpdateContextMenu(_host);

                            ee.TabPage.Controls.Add(tab);
                        }

                        MessageFilter.ListBoxMouseWheelHandler.Enable(true);
                        MessageFilter.FormClickHandler.Enable(true);
                    }
                    else
                    {
                        MessageFilter.ListBoxMouseWheelHandler.Enable(false);
                        MessageFilter.FormClickHandler.Enable(false);
                    }
                };

                tabMain.Selected += tabMainSelected;

                if (!readOnly)
                    pwEntryForm.EntrySaving += (s, ee) =>
                    {
                        // Always try to migrate existing settings when saving.
                        if (peEntrySettings == null && pwEntryForm.EntryStrings.Exists(Util.KprEntrySettingsField))
                            peEntrySettings = pwEntryForm.EntryRef.GetKprSettings(readOnly) ?? KprEntrySettings.Empty;

                        if (peEntrySettings != null)
                            peEntrySettings.SaveEntrySettings(pwEntryForm);
                    };

                pwEntryForm.FormClosed += (s, ee) =>
                {
                    tabMain.Selected -= tabMainSelected;

                    if (peEntrySettings != null)
                        peEntrySettings.Dispose();

                    if (tabPage.Controls.Count > 0)
                        tabPage.Controls[0].Dispose();

                    using (tabPage)
                        tabMain.TabPages.RemoveByKey(Util.KeePassRDP);

                    if (tabMain.ImageList != null)
                        using (tabMain.ImageList.Images[Util.KeePassRDP])
                            tabMain.ImageList.Images.RemoveByKey(Util.KeePassRDP);

                    MessageFilter.ListBoxMouseWheelHandler.Enable(false);
                    MessageFilter.FormClickHandler.Enable(false);
                };
            }
        }

        private void DropDownOpening(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null || !menuItem.HasDropDownItems)
                return;

            var dropDownItems = menuItem.DropDownItems.OfType<ToolStripItem>();
            var mainForm = _host.MainWindow;

            Task.Factory.FromAsync(mainForm.BeginInvoke(new Action(() =>
            {
                //var isValid = Util.IsValid(_host, false);
                var lastButton = _toolbarItems[KprMenu.MenuItem.IgnoreCredentials];
                UIUtil.SetChecked(
                    dropDownItems.LastOrDefault(x => x is ToolStripMenuItem) as ToolStripMenuItem, //KprMenu.MenuItem.IgnoreCredentials.ToString()],
                    (lastButton.Visible && !string.IsNullOrEmpty(lastButton.ImageKey)) ||
                    (!lastButton.Visible && mainForm.GetSelectedEntriesCount() == 1 && Util.IsEntryIgnored(mainForm.GetSelectedEntry(true, true))));
                //isValid && /*tsmiIgnoreCredEntry.Enabled &&*/ Util.IsEntryIgnored(mainForm.GetSelectedEntry(true, true)));
            })), endInvoke => mainForm.EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

            var maxWidth = dropDownItems.Where(x => x.Available).Max(x => x.GetPreferredSize(Size.Empty).Width);
            menuItem.DropDown.MaximumSize = new Size(maxWidth, 0);

            if (_config.KeePassContextMenuOnScreen)
            {
                var toolStrip = menuItem.GetCurrentParent();
                var bounds = toolStrip.RectangleToScreen(toolStrip.DisplayRectangle);

                var currentScreen = Screen.FromControl(toolStrip); //Screen.FromPoint(bounds.Location);
                var screenBounds = currentScreen.Bounds;

                menuItem.DropDownDirection =
                    bounds.Left - maxWidth >= screenBounds.Left &&
                    bounds.Right + maxWidth + 5 >= screenBounds.Right ? ToolStripDropDownDirection.Left : ToolStripDropDownDirection.Right;
            }
            else
                menuItem.DropDownDirection = ToolStripDropDownDirection.Default;
        }

        private void EntryMenuOpening(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            var dd = tsmi != null ? tsmi.DropDown : null;
            var kprtsmi = _toolStripMenuItem.Value;
            if (kprtsmi.Owner != dd)
                kprtsmi.Owner = dd;
            if (dd != null)
                EnableDisableContextMenuItems();
        }

        private void ContextMenuOpening(object sender, CancelEventArgs e)
        {
            var cms = sender as ContextMenuStrip;
            var tsmi = _toolStripMenuItem.Value;
            if (tsmi.Owner != cms)
                tsmi.Owner = cms;
            if (cms != null)
                EnableDisableContextMenuItems();
        }

        private ToolStripMenuItem CreateToolStripMenuItem(KprMenu.MenuItem menuItem)
        {
            return Util.CreateToolStripMenuItem(menuItem, _config.GetShortcut(menuItem));
        }

        public override ToolStripMenuItem GetMenuItem(PluginMenuType pluginMenuType)
        {
            ToolStripMenuItem tsmi = null;

            switch (pluginMenuType)
            {
                case PluginMenuType.Entry:
                    tsmi = _toolStripMenuItem.Value;

                    // Create entry menu item only once.
                    if (tsmi.Owner == null)
                    {
                        if (tsmi.Image == null)
                            tsmi.Image = SmallIcon;

                        var mainForm = _host.MainWindow;

                        Task.Factory.FromAsync(mainForm.BeginInvoke(new Action(() =>
                        {
                            if (!_toolbarButtonsAdded.IsSet)
                                if (!_toolbarButtonsAdded.Wait(TimeSpan.FromSeconds(10)))
                                    throw new TimeoutException();

                            tsmi.DropDownItems.Clear();
                            tsmi.DropDownItems.AddRange(KprMenu.MenuItemValues.Select(menu =>
                            {
                                var tsmiItem = CreateToolStripMenuItem(menu);
                                switch (menu)
                                {
                                    // Configure the OpenRdpConnection menu entry.
                                    case KprMenu.MenuItem.OpenRdpConnection:
                                        tsmiItem.Click += OnOpenRDP_Click;
                                        break;
                                    // Configure the OpenRdpConnectionAdmin menu entry.
                                    case KprMenu.MenuItem.OpenRdpConnectionAdmin:
                                        tsmiItem.Click += OnOpenRDPAdmin_Click;
                                        break;
                                    // Configure the OpenRdpConnectionNoCred menu entry.
                                    case KprMenu.MenuItem.OpenRdpConnectionNoCred:
                                        tsmiItem.Click += OnOpenRDPNoCred_Click;
                                        break;
                                    // Configure the OpenRdpConnectionNoCredAdmin menu entry.
                                    case KprMenu.MenuItem.OpenRdpConnectionNoCredAdmin:
                                        tsmiItem.Click += OnOpenRDPNoCredAdmin_Click;
                                        break;
                                    // Configure the ShadowSession menu entry.
                                    case KprMenu.MenuItem.ShadowSession:
                                        tsmiItem.Click += OnShadowSession_Click;
                                        break;
                                    // Configure the ShadowSessionNoCred menu entry.
                                    case KprMenu.MenuItem.ShadowSessionNoCred:
                                        tsmiItem.Click += OnShadowSessionNoCred_Click;
                                        break;
                                    // Configure the IgnoreCredentials menu entry.
                                    case KprMenu.MenuItem.IgnoreCredentials:
                                        tsmiItem.Click += OnIgnoreCredEntry_Click;
                                        break;
                                    default:
                                        break;
                                }

                                ToolStripItem tbItem;
                                if (_toolbarItems.TryGetValue(menu, out tbItem) && tbItem != null)
                                {
                                    tsmiItem.Image = tbItem.Image;
                                    UIUtil.ConfigureTbButton(tbItem, tbItem.ToolTipText, null, tsmiItem);
                                }

                                return tsmiItem;
                            }).ToArray());

                            ShowHideContextMenuItems();
                        })), endInvoke => mainForm.EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    }
                    break;
                case PluginMenuType.Main:
                    // Create the main menu options item.
                    tsmi = new ToolStripMenuItem(KprMenu.MenuItem.Options.GetText())
                    {
                        Image = SmallIcon,
                        Dock = DockStyle.Fill,
                        AutoSize = true,
                        AutoToolTip = false,
                        ShowShortcutKeys = false,
                        TextAlign = ContentAlignment.MiddleLeft,
                        ImageAlign = ContentAlignment.MiddleLeft,
                        TextImageRelation = TextImageRelation.ImageBeforeText,
                        DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
                    };
                    tsmi.Click += OnKprOptions_Click;
                    break;
            }

            return tsmi;
        }

        private void EnableDisableContextMenuItems()
        {
            var mainForm = _host.MainWindow;
            Task.Factory.FromAsync(mainForm.BeginInvoke(new Action(() =>
            {
                var selectedEntry = mainForm.GetSelectedEntry(true, true);
                var tsmi = _toolStripMenuItem.Value;
                var items = tsmi.DropDownItems;

                if (selectedEntry != null)
                {
                    var selectedEntriesCount = mainForm.GetSelectedEntriesCount();

                    // Enable context menu when at least one valid entry is selected.
                    tsmi.Enabled = true;
                    var itemsEnabled =
                            (selectedEntriesCount > 1 &&
                                mainForm.GetSelectedEntries().Any(entry => !entry.Strings.GetSafe(PwDefs.UrlField).IsEmpty)) ||
                            (selectedEntriesCount == 1 && !selectedEntry.Strings.GetSafe(PwDefs.UrlField).IsEmpty);

                    var i = items.Count - 1;
                    var item = items[i];
                    item.Enabled = item.Available && /*(selectedEntriesCount > 1 &&
                        mainForm.GetSelectedEntries().Any(entry => !(entry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty &&
                                                                            entry.Strings.GetSafe(PwDefs.PasswordField).IsEmpty))) ||*/
                        (selectedEntriesCount == 1 &&
                        !(selectedEntry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty &&
                        selectedEntry.Strings.GetSafe(PwDefs.PasswordField).IsEmpty));

                    for (i--; i >= 0; i--)
                    {
                        item = items[i];
                        item.Enabled = item.Available && itemsEnabled;
                    }
                }
                else
                {
                    // Disable context menu when no entry is selected.
                    tsmi.Enabled = false;
                    for (var i = items.Count - 1; i >= 0; i--)
                        items[i].Enabled = false;
                }
            })), endInvoke => mainForm.EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void ShowHideContextMenuItems()
        {
            KprMenu.MenuItem menu;
            var kpCmi = _config.KeePassContextMenuItems;
            var tsmi = _toolStripMenuItem.Value;
            foreach (ToolStripItem item in tsmi.DropDownItems)
                item.Available = item.Visible = Enum.TryParse(item.Name ?? string.Empty, out menu) && kpCmi.HasFlag(menu);
        }

        private void EnableDisableToolbarItems()
        {
            var mainForm = _host.MainWindow;
            Task.Factory.FromAsync(mainForm.BeginInvoke(new Action(() =>
            {
                var isValid = Util.IsValid(_host, false);
                var selectedEntriesCount = isValid ? mainForm.GetSelectedEntriesCount() : 0;
                var selectedEntry = selectedEntriesCount == 1 ? mainForm.GetSelectedEntry(true, true) : null;

                var itemsEnabled = isValid && ((selectedEntriesCount > 1 &&
                        mainForm.GetSelectedEntries().Any(entry => !entry.Strings.GetSafe(PwDefs.UrlField).IsEmpty)) ||
                    (selectedEntriesCount == 1 && !selectedEntry.Strings.GetSafe(PwDefs.UrlField).IsEmpty));

                foreach (var menuItem in KprMenu.MenuItemValues.Take(KprMenu.MenuItemValues.Count - 1))
                {
                    var item = _toolbarItems[menuItem];
                    item.Enabled = item.Available && itemsEnabled;
                }

                var lastButton = _toolbarItems[KprMenu.MenuItem.IgnoreCredentials];
                lastButton.Enabled = lastButton.Available && isValid && (/*(selectedEntriesCount > 1 &&
                                mainForm.GetSelectedEntries().Any(entry => !(entry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty &&
                                                                                    entry.Strings.GetSafe(PwDefs.PasswordField).IsEmpty))) ||*/
                        (selectedEntriesCount == 1 &&
                        !(selectedEntry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty &&
                        selectedEntry.Strings.GetSafe(PwDefs.PasswordField).IsEmpty)));

                if (lastButton.Visible)
                    lastButton.ImageKey = isValid && selectedEntriesCount == 1 && Util.IsEntryIgnored(selectedEntry) ? KprMenu.MenuItem.IgnoreCredentials.ToString() : null;
            })), endInvoke => mainForm.EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void ShowHideToolbarItems()
        {
            var kpTbi = _config.KeePassToolbarItems;
            foreach (var button in _toolbarItems)
                button.Value.Available = button.Value.Visible = kpTbi.HasFlag(button.Key);

            // Try to hide preceeding ToolStripSeparator when no items are visible.
            var toolStrip = _host.MainWindow.Controls["m_toolMain"] as CustomToolStripEx;
            var toolStripSeparator = toolStrip.Items[Math.Max(0, toolStrip.Items.IndexOf(_toolbarItems[KprMenu.MenuItem.OpenRdpConnection]) - 1)] as ToolStripSeparator;
            if (toolStripSeparator == null)
                return;

            var last = _toolbarItems[KprMenu.MenuItem.IgnoreCredentials];
            var nextIdx = toolStrip.Items.IndexOf(last) + 1;
            if (toolStrip.Items[toolStrip.Items.Count - 1] == last ||
                (toolStrip.Items.Count > nextIdx && toolStrip.Items[nextIdx] is ToolStripSeparator))
            {
                if (_toolbarItems.Keys.Any(x => kpTbi.HasFlag(x)))
                    toolStripSeparator.Available = toolStripSeparator.Visible = true;
                else if (toolStripSeparator.Visible)
                    toolStripSeparator.Available = toolStripSeparator.Visible = false;
            }
        }

        private void AssignShortcuts()
        {
            var kpHrl = _config.KeePassHotkeysRegisterLast;
            var toolStrips = kpHrl ?
                typeof(ToolStripManager)
                    .GetProperty("ToolStrips", BindingFlags.NonPublic | BindingFlags.Static)
                    .GetValue(null, null) as IList :
                null;
            var shortcuts = kpHrl ?
                typeof(ToolStrip)
                    .GetProperty("Shortcuts", BindingFlags.NonPublic | BindingFlags.Instance) :
                null;

            foreach (var tsmi in _toolStripMenuItem.Value.DropDownItems.OfType<ToolStripMenuItem>())
            {
                var menu = (KprMenu.MenuItem)Enum.Parse(typeof(KprMenu.MenuItem), tsmi.Name);
                var keyCode = _config.GetShortcut(menu);

                // Silently ignore inacceptable shortcuts.
                keyCode = ToolStripManager.IsValidShortcut(keyCode) ? keyCode : Keys.None;

                if (kpHrl || keyCode != tsmi.ShortcutKeys)
                {
                    var tbItem = _toolbarItems[menu];
                    var tooltipText = !string.IsNullOrWhiteSpace(tsmi.ShortcutKeyDisplayString) ?
                        tbItem.ToolTipText.Replace(string.Format("({0})", tsmi.ShortcutKeyDisplayString), string.Empty).TrimEnd() :
                        tbItem.ToolTipText;
                    UIUtil.AssignShortcut(tsmi, keyCode);
                    UIUtil.ConfigureTbButton(tbItem, tooltipText, null, tsmi);
                }

                if (kpHrl && ToolStripManager.IsShortcutDefined(keyCode))
                {
                    for (var i = 0; i < toolStrips.Count; i++)
                    {
                        var owner = toolStrips[i] as ToolStrip;
                        if (owner == null)
                            continue;
                        var ht = shortcuts.GetValue(owner, null) as Hashtable;
                        if (ht != null && ht.Contains(keyCode) && ht[keyCode] != tsmi)
                            ht[keyCode] = tsmi;
                    }
                }
            }
        }

        private void UnassignShortcuts()
        {
            var kpHrl = _config.KeePassHotkeysRegisterLast;
            var toolStrips = kpHrl ?
                typeof(ToolStripManager)
                    .GetProperty("ToolStrips", BindingFlags.NonPublic | BindingFlags.Static)
                    .GetValue(null, null) as IList :
                null;
            var shortcuts = kpHrl ?
                typeof(ToolStrip)
                    .GetProperty("Shortcuts", BindingFlags.NonPublic | BindingFlags.Instance) :
                null;

            foreach (var tsmi in _toolStripMenuItem.Value.DropDownItems.OfType<ToolStripMenuItem>())
            {
                var keyCode = _config.GetShortcut((KprMenu.MenuItem)Enum.Parse(typeof(KprMenu.MenuItem), tsmi.Name));

                if (kpHrl && ToolStripManager.IsShortcutDefined(keyCode))
                {
                    for (var i = 0; i < toolStrips.Count; i++)
                    {
                        var owner = toolStrips[i] as ToolStrip;
                        if (owner == null)
                            continue;
                        var ht = shortcuts.GetValue(owner, null) as Hashtable;
                        if (ht != null && ht.Contains(keyCode) && ht[keyCode] == tsmi)
                            ht[keyCode] = null;
                    }
                }
            }
        }

        private void RemoveOldShortcuts()
        {
            var toolStrips = typeof(ToolStripManager)
                .GetProperty("ToolStrips", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null, null) as IList;
            var shortcuts = typeof(ToolStrip)
                .GetProperty("Shortcuts", BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var tsmi in _toolStripMenuItem.Value.DropDownItems.OfType<ToolStripMenuItem>())
            {
                var keyCode = _config.GetShortcut((KprMenu.MenuItem)Enum.Parse(typeof(KprMenu.MenuItem), tsmi.Name));

                for (var i = 0; i < toolStrips.Count; i++)
                {
                    var owner = toolStrips[i] as ToolStrip;
                    if (owner == null)
                        continue;
                    var ht = shortcuts.GetValue(owner, null) as Hashtable;
                    if (ht != null && ht.Contains(keyCode) && ht[keyCode] == null)
                        ht.Remove(keyCode);
                }
            }
        }

        private void OnKprOptions_Click(object sender, EventArgs e)
        {
            var result = DialogResult.None;

            using (var mrs = new ManualResetEventSlim(false))
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        result = UIUtil.ShowDialogAndDestroy(new KprOptionsForm(_config, _toolbarItems));
                    }
                    finally
                    {
                        mrs.Set();
                    }
                }, CancellationToken.None, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);

                UnassignShortcuts();
                mrs.Wait();

                if (result == DialogResult.OK)
                {
                    ShowHideContextMenuItems();
                    EnableDisableContextMenuItems();
                    ShowHideToolbarItems();
                    EnableDisableToolbarItems();
                    if (_connectionManager.IsValueCreated &&
                        _connectionManager.Value.CredentialPicker.IsValueCreated &&
                        _connectionManager.Value.CredentialPicker.Value.CredentialPickerForm.IsValueCreated)
                        _connectionManager.Value.CredentialPicker.Value.CredentialPickerForm.Value
                            .SetRowHeight(_config.CredPickerLargeRows ? KprCredentialPickerForm.RowHeight.Large : KprCredentialPickerForm.RowHeight.Default);
                }
            }
            AssignShortcuts();
            RemoveOldShortcuts();
        }

        private void OnToolstripItem_Click(object sender, EventArgs e)
        {
            var button = sender as ToolStripItem;
            var tsmi = _toolStripMenuItem.Value;
            var idx = tsmi.DropDownItems.IndexOfKey(button.Name);
            if (idx >= 0)
                tsmi.DropDownItems[idx].PerformClick();
        }

        private void OnOpenRDP_Click(object sender, EventArgs e)
        {
            var toolStrip = _host.MainWindow.Controls["m_toolMain"] as CustomToolStripEx;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _connectionManager.Value.CredentialPicker.Value.SetIcon(toolStrip.ImageList.Images[KprMenu.MenuItem.OpenRdpConnection.ToString()]);
                    _connectionManager.Value.ConnectRDPtoKeePassEntry(false, true);
                }
                catch (Exception ex)
                {
                    Util.ShowErrorDialog(
                        Util.FormatException(ex),
                        null,
                        string.Format("{0} - {1}", Util.KeePassRDP, KPRes.FatalError),
                        VtdIcon.Error,
                        _host.MainWindow,
                        null, 0, null, 0);
                }
            }, CancellationToken.None, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
        }

        private void OnOpenRDPAdmin_Click(object sender, EventArgs e)
        {
            var toolStrip = _host.MainWindow.Controls["m_toolMain"] as CustomToolStripEx;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _connectionManager.Value.CredentialPicker.Value.SetIcon(toolStrip.ImageList.Images[KprMenu.MenuItem.OpenRdpConnectionAdmin.ToString()]);
                    _connectionManager.Value.ConnectRDPtoKeePassEntry(true, true);
                }
                catch (Exception ex)
                {
                    Util.ShowErrorDialog(
                        Util.FormatException(ex),
                        null,
                        string.Format("{0} - {1}", Util.KeePassRDP, KPRes.FatalError),
                        VtdIcon.Error,
                        _host.MainWindow,
                        null, 0, null, 0);
                }
            }, CancellationToken.None, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
        }

        private void OnOpenRDPNoCred_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _connectionManager.Value.ConnectRDPtoKeePassEntry();
                }
                catch (Exception ex)
                {
                    Util.ShowErrorDialog(
                        Util.FormatException(ex),
                        null,
                        string.Format("{0} - {1}", Util.KeePassRDP, KPRes.FatalError),
                        VtdIcon.Error,
                        _host.MainWindow,
                        null, 0, null, 0);
                }
            }, CancellationToken.None, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
        }

        private void OnOpenRDPNoCredAdmin_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _connectionManager.Value.ConnectRDPtoKeePassEntry(true);
                }
                catch (Exception ex)
                {
                    Util.ShowErrorDialog(
                        Util.FormatException(ex),
                        null,
                        string.Format("{0} - {1}", Util.KeePassRDP, KPRes.FatalError),
                        VtdIcon.Error,
                        _host.MainWindow,
                        null, 0, null, 0);
                }
            }, CancellationToken.None, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
        }

        private void OnShadowSession_Click(object sender, EventArgs e)
        {
            var toolStrip = _host.MainWindow.Controls["m_toolMain"] as CustomToolStripEx;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _connectionManager.Value.CredentialPicker.Value.SetIcon(toolStrip.ImageList.Images[KprMenu.MenuItem.ShadowSession.ToString()]);
                    _connectionManager.Value.ConnectRDPtoKeePassEntry(false, true, true);

                }
                catch (Exception ex)
                {
                    Util.ShowErrorDialog(
                        Util.FormatException(ex),
                        null,
                        string.Format("{0} - {1}", Util.KeePassRDP, KPRes.FatalError),
                        VtdIcon.Error,
                        _host.MainWindow,
                        null, 0, null, 0);
                }
            }, CancellationToken.None, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
        }

        private void OnShadowSessionNoCred_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _connectionManager.Value.ConnectRDPtoKeePassEntry(false, false, true);
                }
                catch (Exception ex)
                {
                    Util.ShowErrorDialog(
                        Util.FormatException(ex),
                        null,
                        string.Format("{0} - {1}", Util.KeePassRDP, KPRes.FatalError),
                        VtdIcon.Error,
                        _host.MainWindow,
                        null, 0, null, 0);
                }
            }, CancellationToken.None, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
        }

        private void OnIgnoreCredEntry_Click(object sender, EventArgs e)
        {
            if (Util.IsValid(_host))
            {
                var pe = _host.MainWindow.GetSelectedEntry(true, true);

                if (pe != null)
                {
                    pe.ToggleKprIgnored();
                    _host.MainWindow.UpdateUI(false, null, false, null, false, null, true);
                }
            }
        }
    }
}