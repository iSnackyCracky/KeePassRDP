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

using KeePass;
using KeePass.Ecas;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.UI;
using KeePass.Util;
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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP
{
    public sealed class KeePassRDPExt : Plugin
    {
        internal static readonly string MstscPath = Environment.ExpandEnvironmentVariables(Util.DefaultMstscPath);

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

        private readonly Lazy<KprCredentialManager<KprCredential>> _credManager;
        private readonly Dictionary<KprMenu.MenuItem, ToolStripItem> _toolbarItems;
        private readonly ToolStripMenuItem _toolStripMenuItem;

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

            switch (Program.Translation.Properties.Iso6391Code)
            {
                case "de":
                    Resources.Resources.Culture = CultureInfo.CreateSpecificCulture("de");
                    break;
                default:
                    Resources.Resources.Culture = CultureInfo.CreateSpecificCulture("en");
                    break;
            }

            _credManager = new Lazy<KprCredentialManager<KprCredential>>(() => new KprCredentialManager<KprCredential>(_config), LazyThreadSafetyMode.ExecutionAndPublication);
            _toolbarItems = new Dictionary<KprMenu.MenuItem, ToolStripItem>();
            _toolStripMenuItem = new ToolStripMenuItem(Util.KeePassRDP);
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

            _host = host;
            _config = new KprConfig(_host.CustomConfig);
            _connectionManager = new Lazy<KprConnectionManager>(() => new KprConnectionManager(_host, _config, _credManager), LazyThreadSafetyMode.ExecutionAndPublication);

            AddToolbarButtons();

            _host.TriggerSystem.RaisingEvent += TriggerSystem_RaisingEvent_AppInitPost;

            GlobalWindowManager.WindowAdded += GlobalWindowManager_WindowAdded;

            _toolStripMenuItem.DropDownOpening += DropDownOpening;
            _host.MainWindow.EntryContextMenu.Opening += ContextMenuOpening;
            (_host.MainWindow.MainMenuStrip.Items["m_menuEntry"] as ToolStripMenuItem).DropDownOpening += EntryMenuOpening;

            _host.MainWindow.UIStateUpdated += UIStateUpdated;
            _host.MainWindow.FormClosing += MainFormClosing;

            if (UpdateUrl.EndsWith(".gz"))
                UpdateCheckEx.SetFileSigKey(UpdateUrl, KprVersion.FileSigKey);

            return true;
        }

        private void MainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_config.KeePassConfirmOnClose ||
                (e != null && e.Cancel))
                return;

            if (_connectionManager != null && _connectionManager.IsValueCreated)
            {
                if (_connectionManager.Value.IsCompleted)
                    _connectionManager.Value.Dispose();
                else
                {
                    _connectionManager.Value.Cancel();

                    if (sender != null)
                    {
                        if (_isWaitingOnCloseSignal != null)
                            _isWaitingOnCloseSignal.Reset();
                        if (!_connectionManager.Value.IsCompleted &&
                                VistaTaskDialog.ShowMessageBoxEx(
                                    string.Format(KprResourceManager.Instance["{0} connection" + (_connectionManager.Value.Count == 1 ? " is" : "s are") + " still open."], _connectionManager.Value.Count),
                                    KprResourceManager.Instance["Continue?"],
                                    Util.KeePassRDP,
                                    VtdIcon.Information,
                                    _host.MainWindow,
                                    KprResourceManager.Instance["&Wait"], 0,
                                    KprResourceManager.Instance["&Quit"], 1) == 0)
                        {
                            if (_isWaitingOnCloseStart == null)
                            {
                                _isWaitingOnCloseSignal = new ManualResetEventSlim(true);
                                _isWaitingOnCloseStart = DateTimeOffset.UtcNow;
                                _host.MainWindow.UseWaitCursor = true;
                                Task.Factory.StartNew(() =>
                                {
                                    try
                                    {
                                        while (_connectionManager != null &&
                                            !_connectionManager.Value.IsCompleted &&
                                            !_connectionManager.Value.Wait(5))
                                        {
                                            if (!_isWaitingOnCloseSignal.IsSet)
                                                _isWaitingOnCloseSignal.Wait();
                                            if (DateTimeOffset.UtcNow - _isWaitingOnCloseStart >= TimeSpan.FromSeconds(15))
                                                _host.MainWindow.Close();
                                        }
                                        _host.MainWindow.Close();
                                    }
                                    catch
                                    {
                                    }
                                }, CancellationToken.None, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                            }
                            else
                            {
                                _isWaitingOnCloseStart = DateTimeOffset.UtcNow;
                                _isWaitingOnCloseSignal.Set();
                            }

                            e.Cancel = true;
                            return;
                        }

                        _host.MainWindow.UseWaitCursor = false;
                    }

                    _connectionManager.Value.Dispose();
                }

                _connectionManager = null;
            }

            if (_isWaitingOnCloseSignal != null)
            {
                if (!_isWaitingOnCloseSignal.IsSet)
                    _isWaitingOnCloseSignal.Set();
                _isWaitingOnCloseSignal.Dispose();
                _isWaitingOnCloseSignal = null;
            }
        }

        public override void Terminate()
        {
            _isWaitingOnCloseStart = null;

            if (_host == null)
                return;

            if (_credManager.IsValueCreated)
                _credManager.Value.Clear();

            _host.MainWindow.FormClosing -= MainFormClosing;
            _host.MainWindow.UIStateUpdated -= UIStateUpdated;

            (_host.MainWindow.MainMenuStrip.Items["m_menuEntry"] as ToolStripMenuItem).DropDownOpening -= EntryMenuOpening;
            _host.MainWindow.EntryContextMenu.Opening -= ContextMenuOpening;
            _toolStripMenuItem.DropDownOpening -= DropDownOpening;

            GlobalWindowManager.WindowAdded -= GlobalWindowManager_WindowAdded;

            try
            {
                _host.TriggerSystem.RaisingEvent -= TriggerSystem_RaisingEvent_AppInitPost;
            }
            catch
            {
                _host.TriggerSystem.RaisingEvent -= TriggerSystem_RaisingEvent;
            }

            foreach (var menu in KprMenu.MenuItemValues)
                _host.MainWindow.RemoveCustomToolBarButton(menu.ToString());

            foreach (var button in _toolbarItems)
                if (button.Key != KprMenu.MenuItem.IgnoreCredentials)
                    button.Value.Image.Dispose();
                else
                    button.Value.Paint -= IgnoreButtonPaint;

            _toolbarItems.Clear();
            _toolStripMenuItem.DropDownItems.Clear();

            //if (_smallIcon.IsValueCreated)
            //    _smallIcon.Value.Dispose();

            MainFormClosing(null, null);

            _config = null;
            _host = null;
        }

        private void GlobalWindowManager_WindowAdded(object sender, GwmWindowEventArgs e)
        {
            if (e.Form is PwEntryForm)
                AddKprTab(e.Form as PwEntryForm);
        }

        private void TriggerSystem_RaisingEvent_AppInitPost(object sender, EcasRaisingEventArgs e)
        {
            if (e.Event.Type.Equals(_appInitPost))
            {
                // Force initialization of KprCredentialManager to clean up left over invalid credentials.
                new Task<bool>(() =>
                {
                    if (!_credManager.IsValueCreated && _credManager.Value.Count > 0)
                        return true;
                    return false;
                }, CancellationToken.None, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness)
                    .Start(TaskScheduler.Default);

                _host.TriggerSystem.RaisingEvent -= TriggerSystem_RaisingEvent_AppInitPost;
                _host.TriggerSystem.RaisingEvent += TriggerSystem_RaisingEvent;

                MoveToolbarButtons();
                ShowHideToolbarItems();

                if (_config.KeePassHotkeysRegisterLast)
                    AssignShortcuts();
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

            if (_toolStripMenuItem.DropDownItems.ContainsKey(strID))
                _toolStripMenuItem.DropDownItems[strID].PerformClick();
        }

        private void UIStateUpdated(object sender, EventArgs e)
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
            var toolStrip = _host.MainWindow.Controls["m_toolMain"] as CustomToolStripEx;

            toolStrip.SuspendLayout();

            if (toolStrip.ImageList == null)
                toolStrip.ImageList = new ImageList
                {
                    ColorDepth = ColorDepth.Depth32Bit,
                    ImageSize = UIUtil.GetSmallIconSize(),
                    TransparentColor = Color.Transparent
                };

            var iconSize = toolStrip.ImageList.ImageSize.Height;
            foreach (var menu in KprMenu.MenuItemValues)
            {
                var menuString = menu.ToString();
                _host.MainWindow.AddCustomToolBarButton(menuString, menuString, KprMenu.GetText(menu));

                var button = toolStrip.Items[toolStrip.Items.Count - 1];
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
                        using (var icon = IconUtil.ExtractIcon(MstscPath, 4, iconSize))
                        {
                            if (icon.Height < iconSize)
                            {
                                using(var bmp = icon.ToBitmap())
                                    toolStrip.ImageList.Images.Add(menuString, GfxUtil.ScaleImage(bmp, iconSize, iconSize));
                            }
                            else
                                toolStrip.ImageList.Images.Add(menuString, icon.ToBitmap());
                        }
                    button.ImageKey = menuString;
                }

                button.Visible = _config.KeePassToolbarItems.HasFlag(menu);

                _toolbarItems.Add(menu, button);
            }

            toolStrip.ResumeLayout(false);
        }

        private void MoveToolbarButtons()
        {
            var toolStrip = _host.MainWindow.Controls["m_toolMain"] as CustomToolStripEx;

            var firstIndex = toolStrip.Items.IndexOf(_toolbarItems[KprMenu.MenuItem.OpenRdpConnection]);
            if (firstIndex > 0 && !(toolStrip.Items[firstIndex - 1] is ToolStripSeparator))
            {
                var lastSeperator = toolStrip.Items.Cast<ToolStripItem>().Last(x => x is ToolStripSeparator);
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

                tabMain.TabPages.Add(new TabPage(Util.KeePassRDP)
                {
                    Name = Util.KeePassRDP
                });

                // Assigning the ImageKey before adding a TabPage to a TabControl fails.
                tabMain.TabPages[Util.KeePassRDP].ImageKey = Util.KeePassRDP;

                var readOnly = pwEntryForm.EditModeEx == PwEditMode.ViewReadOnlyEntry;

                KprEntrySettings peEntrySettings = null;
                TabControlEventHandler tabMainSelected = (s, ee) =>
                {
                    if (ee.TabPage == tabMain.TabPages[Util.KeePassRDP])
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

                    if (tabMain.TabPages[Util.KeePassRDP].Controls.Count > 0)
                        tabMain.TabPages[Util.KeePassRDP].Controls[0].Dispose();

                    using (tabMain.TabPages[Util.KeePassRDP])
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
            var isValid = Util.IsValid(_host, false);
            UIUtil.SetChecked(
                (ToolStripMenuItem)_toolStripMenuItem.DropDownItems[4], //KprMenu.MenuItem.IgnoreCredentials.ToString()],
                !string.IsNullOrEmpty(_toolbarItems[KprMenu.MenuItem.IgnoreCredentials].ImageKey));
                //isValid && /*tsmiIgnoreCredEntry.Enabled &&*/ Util.IsEntryIgnored(_host.MainWindow.GetSelectedEntry(true, true)));
        }

        private void EntryMenuOpening(object sender, EventArgs e)
        {
            _toolStripMenuItem.Owner = (sender as ToolStripMenuItem).DropDown;
            EnableDisableContextMenuItems();
        }

        private void ContextMenuOpening(object sender, CancelEventArgs e)
        {
            _toolStripMenuItem.Owner = sender as ContextMenuStrip;
            EnableDisableContextMenuItems();
        }

        private ToolStripMenuItem CreateToolStripMenuItem(KprMenu.MenuItem menuItem)
        {
            return Util.CreateToolStripMenuItem(menuItem, KprMenu.GetShortcut(menuItem, _config));
        }

        public override ToolStripMenuItem GetMenuItem(PluginMenuType pluginMenuType)
        {
            ToolStripMenuItem tsmi = null;

            switch (pluginMenuType)
            {
                case PluginMenuType.Entry:
                    tsmi = _toolStripMenuItem;

                    // Create entry menu item only once.
                    if (tsmi.Owner == null)
                    {
                        if (tsmi.Image == null)
                            tsmi.Image = SmallIcon;

                        foreach (var menu in KprMenu.MenuItemValues)
                        {
                            if (!tsmi.DropDownItems.ContainsKey(menu.ToString()))
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
                                    // Configure the IgnoreCredentials menu entry.
                                    case KprMenu.MenuItem.IgnoreCredentials:
                                        tsmiItem.Click += OnIgnoreCredEntry_Click;
                                        break;
                                    default:
                                        continue;
                                }
                                var tbItem = _toolbarItems[menu];
                                tsmiItem.Image = tbItem.Image;
                                tsmi.DropDownItems.Add(tsmiItem);
                                UIUtil.ConfigureTbButton(tbItem, tbItem.ToolTipText, null, tsmiItem);
                            }
                        }

                        ShowHideContextMenuItems();
                    }
                    break;
                case PluginMenuType.Main:
                    // Create the main menu options item.
                    tsmi = new ToolStripMenuItem(KprMenu.GetText(KprMenu.MenuItem.Options))
                    {
                        Image = SmallIcon
                    };
                    tsmi.Click += OnKprOptions_Click;
                    break;
            }

            return tsmi;
        }

        private void EnableDisableContextMenuItems()
        {
            var mainForm = _host.MainWindow;
            var selectedEntry = mainForm.GetSelectedEntry(true, true);

            if (selectedEntry != null)
            {
                // Enable context menu when at least one valid entry is selected.
                _toolStripMenuItem.Enabled = true;
                _toolStripMenuItem.DropDownItems[0].Enabled = //KprMenu.MenuItem.OpenRdpConnection.ToString()].Enabled =
                    _toolStripMenuItem.DropDownItems[1].Enabled = //KprMenu.MenuItem.OpenRdpConnectionAdmin.ToString()].Enabled =
                    _toolStripMenuItem.DropDownItems[2].Enabled = //KprMenu.MenuItem.OpenRdpConnectionNoCred.ToString()].Enabled =
                    _toolStripMenuItem.DropDownItems[3].Enabled = //KprMenu.MenuItem.OpenRdpConnectionNoCredAdmin.ToString()].Enabled =
                        (mainForm.GetSelectedEntriesCount() > 1 &&
                            mainForm.GetSelectedEntries().Any(entry => !entry.Strings.GetSafe(PwDefs.UrlField).IsEmpty)) ||
                        (mainForm.GetSelectedEntriesCount() == 1 && !selectedEntry.Strings.GetSafe(PwDefs.UrlField).IsEmpty);
                _toolStripMenuItem.DropDownItems[4].Enabled = /*KprMenu.MenuItem.IgnoreCredentials.ToString()].Enabled =*/ /*(mainForm.GetSelectedEntriesCount() > 1 &&
                                mainForm.GetSelectedEntries().Any(entry => !(entry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty &&
                                                                                    entry.Strings.GetSafe(PwDefs.PasswordField).IsEmpty))) ||*/
                    (mainForm.GetSelectedEntriesCount() == 1 &&
                    !(selectedEntry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty &&
                    selectedEntry.Strings.GetSafe(PwDefs.PasswordField).IsEmpty));
            }
            else
            {
                // Disable context menu when no entry is selected.
                _toolStripMenuItem.Enabled = false;
                foreach (ToolStripMenuItem item in _toolStripMenuItem.DropDownItems)
                    item.Enabled = false;
            }
        }

        private void ShowHideContextMenuItems()
        {
            KprMenu.MenuItem menu;
            foreach (ToolStripMenuItem item in _toolStripMenuItem.DropDownItems)
                item.Visible = Enum.TryParse(item.Name ?? string.Empty, out menu) && _config.KeePassContextMenuItems.HasFlag(menu);
        }

        private void EnableDisableToolbarItems()
        {
            var isValid = Util.IsValid(_host, false);
            var mainForm = _host.MainWindow;
            var selectedEntry = mainForm.GetSelectedEntry(true, true);

            _toolbarItems[KprMenu.MenuItem.OpenRdpConnection].Enabled =
                _toolbarItems[KprMenu.MenuItem.OpenRdpConnectionAdmin].Enabled =
                _toolbarItems[KprMenu.MenuItem.OpenRdpConnectionNoCred].Enabled =
                _toolbarItems[KprMenu.MenuItem.OpenRdpConnectionNoCredAdmin].Enabled = isValid && ((mainForm.GetSelectedEntriesCount() > 1 &&
                    mainForm.GetSelectedEntries().Any(entry => !entry.Strings.GetSafe(PwDefs.UrlField).IsEmpty)) ||
                (mainForm.GetSelectedEntriesCount() == 1 && !selectedEntry.Strings.GetSafe(PwDefs.UrlField).IsEmpty));
            _toolbarItems[KprMenu.MenuItem.IgnoreCredentials].Enabled = isValid && (/*(mainForm.GetSelectedEntriesCount() > 1 &&
                                mainForm.GetSelectedEntries().Any(entry => !(entry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty &&
                                                                                    entry.Strings.GetSafe(PwDefs.PasswordField).IsEmpty))) ||*/
                    (mainForm.GetSelectedEntriesCount() == 1 &&
                    !(selectedEntry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty &&
                    selectedEntry.Strings.GetSafe(PwDefs.PasswordField).IsEmpty)));

            var button = _toolbarItems[KprMenu.MenuItem.IgnoreCredentials];
            if (button.Visible)
                button.ImageKey = isValid && Util.IsEntryIgnored(selectedEntry) ? KprMenu.MenuItem.IgnoreCredentials.ToString() : null;
        }

        private void ShowHideToolbarItems()
        {
            foreach (var button in _toolbarItems)
                button.Value.Visible = _config.KeePassToolbarItems.HasFlag(button.Key);

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
                if (_toolbarItems.Keys.Any(x => _config.KeePassToolbarItems.HasFlag(x)))
                    toolStripSeparator.Visible = true;
                else if (toolStripSeparator.Visible)
                    toolStripSeparator.Visible = false;
            }
        }

        private void AssignShortcuts()
        {
            var toolStrips = _config.KeePassHotkeysRegisterLast ?
                typeof(ToolStripManager)
                    .GetProperty("ToolStrips", BindingFlags.NonPublic | BindingFlags.Static)
                    .GetValue(null, null) as IList :
                null;
            var shortcuts = _config.KeePassHotkeysRegisterLast ?
                typeof(ToolStrip)
                    .GetProperty("Shortcuts", BindingFlags.NonPublic | BindingFlags.Instance) :
                null;

            foreach (ToolStripMenuItem tsmi in _toolStripMenuItem.DropDownItems)
            {
                var menu = (KprMenu.MenuItem)Enum.Parse(typeof(KprMenu.MenuItem), tsmi.Name);
                var keyCode = KprMenu.GetShortcut(menu, _config);

                // Silently ignore inacceptable shortcuts.
                keyCode = ToolStripManager.IsValidShortcut(keyCode) ? keyCode : Keys.None;

                if (_config.KeePassHotkeysRegisterLast || keyCode != tsmi.ShortcutKeys)
                {
                    UIUtil.AssignShortcut(tsmi, keyCode);
                    UIUtil.ConfigureTbButton(_toolbarItems[menu], _toolbarItems[menu].ToolTipText, null, tsmi);
                }

                if (_config.KeePassHotkeysRegisterLast && ToolStripManager.IsShortcutDefined(keyCode))
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
            var toolStrips = _config.KeePassHotkeysRegisterLast ?
                typeof(ToolStripManager)
                    .GetProperty("ToolStrips", BindingFlags.NonPublic | BindingFlags.Static)
                    .GetValue(null, null) as IList :
                null;
            var shortcuts = _config.KeePassHotkeysRegisterLast ?
                typeof(ToolStrip)
                    .GetProperty("Shortcuts", BindingFlags.NonPublic | BindingFlags.Instance) :
                null;

            foreach (ToolStripMenuItem tsmi in _toolStripMenuItem.DropDownItems)
            {
                var keyCode = KprMenu.GetShortcut((KprMenu.MenuItem)Enum.Parse(typeof(KprMenu.MenuItem), tsmi.Name), _config);

                if (_config.KeePassHotkeysRegisterLast && ToolStripManager.IsShortcutDefined(keyCode))
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

            foreach (ToolStripMenuItem tsmi in _toolStripMenuItem.DropDownItems)
            {
                var keyCode = KprMenu.GetShortcut((KprMenu.MenuItem)Enum.Parse(typeof(KprMenu.MenuItem), tsmi.Name), _config);

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
            UnassignShortcuts();
            if (UIUtil.ShowDialogAndDestroy(new KprOptionsForm(_config, _toolbarItems)) == DialogResult.OK)
            {
                ShowHideContextMenuItems();
                ShowHideToolbarItems();
                if (_connectionManager.IsValueCreated &&
                    _connectionManager.Value.CredentialPicker.IsValueCreated &&
                    _connectionManager.Value.CredentialPicker.Value.CredentialPickerForm.IsValueCreated)
                    _connectionManager.Value.CredentialPicker.Value.CredentialPickerForm.Value
                        .SetRowHeight(_config.CredPickerLargeRows ? KprCredentialPickerForm.RowHeight.Large : KprCredentialPickerForm.RowHeight.Default);
            }
            AssignShortcuts();
            RemoveOldShortcuts();
        }

        private void OnOpenRDP_Click(object sender, EventArgs e)
        {
            _connectionManager.Value.ConnectRDPtoKeePassEntry(false, true);
        }

        private void OnOpenRDPAdmin_Click(object sender, EventArgs e)
        {
            _connectionManager.Value.ConnectRDPtoKeePassEntry(true, true);
        }

        private void OnOpenRDPNoCred_Click(object sender, EventArgs e)
        {
            _connectionManager.Value.ConnectRDPtoKeePassEntry();
        }

        private void OnOpenRDPNoCredAdmin_Click(object sender, EventArgs e)
        {
            _connectionManager.Value.ConnectRDPtoKeePassEntry(true);
        }

        private void OnIgnoreCredEntry_Click(object sender, EventArgs e)
        {
            if (Util.IsValid(_host))
            {
                var pe = _host.MainWindow.GetSelectedEntry(true, true);
                pe.ToggleKprIgnored();
                _host.MainWindow.UpdateUI(false, null, false, null, false, null, true);
            }
        }
    }
}