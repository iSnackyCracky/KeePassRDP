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
using KeePass.Resources;
using KeePass.UI;
using KeePassRDP.Commands;
using KeePassRDP.Resources;
using KeePassRDP.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace KeePassRDP
{
    public partial class KprOptionsForm : Form, IGwmWindow
    {
        public bool CanCloseWithoutDataLoss { get { return true; } }

        private readonly KprConfig _realConfig;
        private readonly KprConfig _config;
        private readonly Font _font;
        private readonly Font _pwFont;
        private readonly Lazy<Image> _largeIcon;
        private readonly Lazy<Image> _largestIcon;
        private readonly Timer _tooltipTimer;
        private readonly Lazy<Size> _cursorSize;
        private readonly IDictionary<KprMenu.MenuItem, ToolStripItem> _toolbarItems;

        private decimal? _oldNumValue;
        private Point? _lastTooltipMousePosition;
        private NativeCredentials.Credential[] _credentials;

        private bool _tabIntegrationInitialized;
        private bool _tabPickerInitialized;
        private bool _tabExecutableInitialized;
        private bool _tabVaultInitialized;
        private bool _tabAboutInitialized;
        private int _tblIntegrationMinHeight;

        public KprOptionsForm(KprConfig config, IDictionary<KprMenu.MenuItem, ToolStripItem> toolbarItems)
        {
            _realConfig = config;
            _config = config.Clone();

            _largeIcon = new Lazy<Image>(() =>
            {
                using (var icon = IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, DpiUtil.ScaleIntY(48)))
                    return icon.ToBitmap();
            }, LazyThreadSafetyMode.ExecutionAndPublication);

            _largestIcon = new Lazy<Image>(() =>
            {
                using (var icon = IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, 256))
                    return icon.ToBitmap();
            }, LazyThreadSafetyMode.ExecutionAndPublication);

            _tooltipTimer = new Timer
            {
                Interval = 500,
                Enabled = false
            };

            _cursorSize = new Lazy<Size>(() =>
            {
                Size size;// = Util.GetIconSize(Cursor, Cursor.Handle);
                if (CursorUtil.GetIconSize(out size))
                    return size;
                return Size.Empty;
            }, LazyThreadSafetyMode.ExecutionAndPublication);

            _oldNumValue = null;
            _lastTooltipMousePosition = null;
            _credentials = null;

            _tabIntegrationInitialized =
                _tabPickerInitialized =
                _tabExecutableInitialized =
                _tabVaultInitialized =
                _tabAboutInitialized = false;

            _toolbarItems = toolbarItems;

            InitializeComponent();

            SuspendLayout();

            Util.EnableDoubleBuffered(
                tcKprOptionsForm,
                tabIntegration,
                tabPicker,
                tabExecutable,
                tabVault,
                tabAbout,
                tblKprOptionsForm,
                tblIntegration,
                tblPicker,
                tblExecutable,
                tblMstscExecutable,
                cbMstscExecutable,
                tblVault,
                tblAbout,
                lstRegExPre,
                lstRegExPost,
                lvVault
            );

            Icon = IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, UIUtil.GetSmallIconSize().Height);

            if (Program.Config.UI.StandardFont != null)
            {
                try
                {
                    // Never dispose cached KeePass font.
                    var sf = Program.Config.UI.StandardFont.ToFont();
                    _font = new Font(sf.FontFamily, lvVault.Font.Size, sf.Style, sf.Unit);
                }
                catch (ArgumentException) // Do not fail for broken/unsupported fonts.
                {
                    _font = lvVault.Font;
                }
            }
            else
                _font = lvVault.Font;

            if (Program.Config.UI.PasswordFont != null)
            {
                try
                {
                    // Never dispose cached KeePass font.
                    var sf = Program.Config.UI.PasswordFont.ToFont();
                    _pwFont = new Font(sf.FontFamily, lvVault.Font.Size, sf.Style, sf.Unit);
                }
                catch (ArgumentException) // Do not fail for broken/unsupported fonts.
                {
                    _pwFont = _font;
                }
            }
            else
                _pwFont = _font;

            KprResourceManager.Instance.TranslateMany(
                this,
                tabIntegration,
                tabPicker,
                tabExecutable,
                tabVault,
                tabAbout,
                cmdReset,
                cmdCancel
            );

            txtCredPickerCustomGroup.Tag = KprResourceManager.Instance["Define a custom group name that triggers the credential picker.\r\nDefaults to \"RDP\" if unset."];
            txtMstscSignRdpFiles.Tag = KprResourceManager.Instance["Click here to select a certificate from the user store."];
            txtMstscCustomCommand.Tag = KprResourceManager.Instance["Enter the path for the custom command executable.\r\nTo use built-in logic 'MstscCommand:' or 'FreeRdpCommand:' can be prepended."];

            ResumeLayout(false);
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_pwFont != _font)
                    _pwFont.Dispose();
                if (lvVault.Font != _font)
                    _font.Dispose();
                Icon.Dispose();
                if (_largeIcon.IsValueCreated)
                    _largeIcon.Value.Dispose();
                if (_largestIcon.IsValueCreated)
                    _largestIcon.Value.Dispose();
            }

            base.Dispose(disposing);
        }

        /*protected override void CreateHandle()
        {
            base.CreateHandle();

            var Accent = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_DISABLED
            };
            var AccentStructSize = Marshal.SizeOf(Accent);
            var AccentPtr = Marshal.AllocHGlobal(AccentStructSize);
            Marshal.StructureToPtr(Accent, AccentPtr, false);
            var Data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = AccentStructSize,
                Data = AccentPtr
            };
            var Result = SetWindowCompositionAttribute(Handle, ref Data);
            Marshal.FreeHGlobal(AccentPtr);
        }*/

        private void KprOptionsForm_Shown(object sender, EventArgs e)
        {
            Activate();
            KprOptionsForm_Activated(null, EventArgs.Empty);
        }

        private void KprOptionsForm_Activated(object sender, EventArgs e)
        {
            BringToFront();
        }

        private volatile int _oldWidth = 0;
        private volatile int _oldHeight = 0;
        private volatile bool _isResizing = false;
        private volatile bool _isMoving = false;

        private void KprOptionsForm_Load(object sender, EventArgs e)
        {
            GlobalWindowManager.AddWindow(this);

            UseWaitCursor = true;
            SuspendLayout();

            switch (Resources.Resources.Culture.TwoLetterISOLanguageName)
            {
                case "de":
                    MinimumSize = new Size(MinimumSize.Width + 110, MinimumSize.Height);
                    Size = new Size(Math.Max(MinimumSize.Width, Size.Width), Math.Max(MinimumSize.Height, Size.Height));
                    tblIntegration.MinimumSize = new Size(tblIntegration.MinimumSize.Width, _tblIntegrationMinHeight = Math.Max(tabIntegration.Height, tblIntegration.MinimumSize.Height + 35));
                    ResumeLayout(true);
                    SuspendLayout();
                    break;
            }

            _oldWidth = Width;
            _oldHeight = Height;

            Task.Factory.FromAsync(BeginInvoke(new Action(() =>
            {
                BannerFactory.CreateBannerEx(
                    this,
                    pbKprOptionsForm,
                    _largeIcon.Value,
                    Util.KeePassRDP,
                    KprResourceManager.Instance["Here you can configure all KeePassRDP related options."],
                    true);

                Application.DoEvents();

                if (tcKprOptionsForm.SelectedTab == tabIntegration)
                    tcKprOptionsForm_Selected(null, new TabControlEventArgs(tabIntegration, 0, TabControlAction.Selected));

                ResumeLayout(false);
                UseWaitCursor = false;
                tcKprOptionsForm.Visible = true;
            })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

            MessageFilter.ListBoxMouseWheelHandler.Enable(true);
            MessageFilter.FormClickHandler.Enable(true);
        }

        private void KprOptionsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalWindowManager.RemoveWindow(this);

            if (_tabVaultInitialized)
            {
                lvVault.Invalidated -= lvVault_Invalidated;
                var oldFont = lvVault.Font;
                lvVault.Font = null;
                if (oldFont != Font)
                    oldFont.Dispose();
            }

            if (_tabExecutableInitialized)
            {
                if (_x509Chain.IsValueCreated)
                    _x509Chain.Value.Reset();
            }

            CleanCredentials();

            MessageFilter.ListBoxMouseWheelHandler.Enable(false);
            MessageFilter.FormClickHandler.Enable(false);

            KprResourceManager.Instance.ClearCache();
        }

        private void KprOptionsForm_ResizeBegin(object sender, EventArgs e)
        {
            if (_isResizing || UseWaitCursor)
                return;

            _isResizing = true;

            Task.Factory.FromAsync(BeginInvoke(new Action(() =>
            {
                if (!_isResizing)
                    return;

                var selectedTab = tcKprOptionsForm.SelectedTab;
                foreach (TabPage page in tcKprOptionsForm.TabPages)
                {
                    if (page == selectedTab)
                    {
                        if (page == tabIntegration)
                        {
                            if (!_isMoving)
                            {
                                page.SuspendLayout();
                                tblIntegration.SuspendLayout();
                                flpOptions.SuspendLayout();
                                kprSettingsControl.SuspendLayout();
                            }
                        }
                        continue;
                    }
                    page.SuspendLayout();
                    page.Controls.Clear();
                }
            })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void KprOptionsForm_ResizeEnd(object sender, EventArgs e)
        {
            if (!_isResizing || UseWaitCursor)
                return;

            _isResizing = false;

            Task.Factory.FromAsync(BeginInvoke(new Action(() =>
            {
                if (_isResizing)
                    return;

                var selectedTab = tcKprOptionsForm.SelectedTab;
                foreach (TabPage page in tcKprOptionsForm.TabPages)
                {
                    if (page == tabIntegration)
                    {
                        if (!page.Controls.Contains(tblIntegration))
                            page.Controls.Add(tblIntegration);
                        if (page == selectedTab)
                        {
                            if (!_isMoving)
                            {
                                tblIntegration.ResumeLayout(false);
                                flpOptions.ResumeLayout(false);
                                kprSettingsControl.ResumeLayout(false);
                                page.ResumeLayout(false);
                            }
                        }
                    }
                    else if (page == tabPicker)
                    {
                        if (!page.Controls.Contains(tblPicker))
                            page.Controls.Add(tblPicker);
                    }
                    else if (page == tabExecutable)
                    {
                        if (!page.Controls.Contains(tblExecutable))
                            page.Controls.Add(tblExecutable);
                    }
                    else if (page == tabVault)
                    {
                        if (!page.Controls.Contains(tblVault))
                            page.Controls.Add(tblVault);
                    }
                    else if (page == tabAbout)
                    {
                        if (!page.Controls.Contains(tblAbout))
                            page.Controls.Add(tblAbout);
                    }
                    if (page != selectedTab)
                        page.ResumeLayout(false);
                }
            })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void KprOptionsForm_SizeChanged(object sender, EventArgs e)
        {
            if (UseWaitCursor || pbKprOptionsForm.Image == null || pbKprOptionsForm.Image.Width == 0)
                return;

            var newWidth = Width;
            var exec = _oldWidth != newWidth;
            _oldWidth = newWidth;
            if (exec)
            {
                //Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                {
                    /*if (_oldWidth != newWidth)
                        return;*/

                    var oldWidth = newWidth;
                    BannerFactory.UpdateBanner(
                        this,
                        pbKprOptionsForm,
                        _largeIcon.Value,
                        Util.KeePassRDP,
                        KprResourceManager.Instance["Here you can configure all KeePassRDP related options."],
                        ref oldWidth);

                    Application.DoEvents();
                }//)), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }

            var newHeight = Height;
            exec = exec || _oldHeight != newHeight;
            _oldHeight = newHeight;
            if (exec)
            {
                if (tcKprOptionsForm.SelectedTab == tabIntegration)
                {
                    Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                    {
                        if (_oldHeight != newHeight || _oldWidth != newWidth)
                            return;

                        tblIntegration.ResumeLayout(true);
                        flpOptions.ResumeLayout(true);
                        kprSettingsControl.ResumeLayout(true);
                        tabIntegration.ResumeLayout(true);

                        if (_isResizing)
                        {
                            tabIntegration.SuspendLayout();
                            tblIntegration.SuspendLayout();
                            flpOptions.SuspendLayout();
                            kprSettingsControl.SuspendLayout();
                        }
                    })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                }
            }
        }

        private void tcKprOptionsForm_Selected(object sender, TabControlEventArgs e)
        {
            if (_tabVaultInitialized)
                CleanCredentials();

            kprSettingsControl.Visible = false;

            if (e.TabPage == tabIntegration)
                Init_TabIntegration();
            else if (e.TabPage == tabPicker)
                Init_TabPicker();
            else if (e.TabPage == tabExecutable)
                Init_TabExecutable();
            else if (e.TabPage == tabVault)
            {
                Init_TabVault();
                cmdRefreshCredentials_Click(null, EventArgs.Empty);
            }
            else if (e.TabPage == tabAbout)
            {
                if (!_tabAboutInitialized)
                {
                    _tabAboutInitialized = true;

                    tabAbout.UseWaitCursor = true;
                    tabAbout.SuspendLayout();
                    tblAbout.SuspendLayout();

                    KprResourceManager.Instance.TranslateMany(
                        llWebsite,
                        llLicense
                    );

                    ttGeneric.SetToolTip(llWebsite, Util.WebsiteUrl);
                    ttGeneric.SetToolTip(llLicense, Util.LicenseUrl);

                    lblVersionText.Text = string.Format("{0} ({1})", KprVersion.Version, Resources.Resources.Culture.TwoLetterISOLanguageName);
                    lblRevisionText.Text = KprVersion.Revision;

                    if (pbAbout.Image == null || pbAbout.Image.Width == 0)
                        pbAbout.Image = _largestIcon.Value;

                    tblAbout.RowStyles[0] = new RowStyle(SizeType.Absolute, (int)Math.Max(DpiUtil.ScaleIntY(256), tblAbout.RowStyles[0].Height / DpiUtil.FactorY));
                    tblAbout.Location = new Point(tabAbout.Width / 2 - tblAbout.Width / 2, tabAbout.Height / 2 - tblAbout.Height / 2);

                    tabAbout.ResumeLayout(false);
                    tabAbout.UseWaitCursor = false;
                    tblAbout.ResumeLayout(false);

                    if (!tabAbout.Created)
                        tabAbout.CreateControl();
                }
            }
        }

        private void cmdReset_Click(object sender, EventArgs e)
        {
            if (VistaTaskDialog.ShowMessageBoxEx(
                KprResourceManager.Instance["Really reset all values to default? This action cannot be undone."],
                KprResourceManager.Instance[KPRes.AskContinue],
                string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Warning),
                VtdIcon.Warning,
                this,
                KprResourceManager.Instance[KPRes.YesCmd], 1,
                KprResourceManager.Instance[KPRes.NoCmd], 0) == 1)
            {
                _config.Reset();
                if (_tabIntegrationInitialized)
                    Config_TabIntegration();
                if (_tabPickerInitialized)
                    Config_TabPicker();
                if (_tabExecutableInitialized)
                    Config_TabExecutable();
            }
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            // Save configuration.

            if (_tabIntegrationInitialized)
            {
                _config.CredVaultUseWindows = chkCredVaultUseWindows.Checked;
                _config.CredVaultOverwriteExisting = chkCredVaultOverwriteExisting.Checked;
                _config.CredVaultRemoveOnExit = chkCredVaultRemoveOnExit.Checked;
                _config.CredVaultTtl = (int)numCredVaultTtl.Value;
                _config.CredVaultAdaptiveTtl = chkCredVaultAdaptiveTtl.Checked;

                _config.KeePassConnectToAll = chkKeePassConnectToAll.Checked;
                _config.KeePassAlwaysConfirm = chkKeePassAlwaysConfirm.Checked;
                _config.KeePassDefaultEntryAction = chkKeePassDefaultEntryAction.Checked;
                _config.KeePassContextMenuOnScreen = chkKeePassContextMenuOnScreen.Checked;
                _config.KeePassHotkeysRegisterLast = chkKeePassHotkeysRegisterLast.Checked;
                _config.KeePassConfirmOnClose = chkKeePassConfirmOnClose.Checked;

                /*_config.ShortcutOpenRdpConnection = kprSettingsControl[KprMenu.MenuItem.OpenRdpConnection].Hotkey; // txtOpenRdpKey.Hotkey;
                _config.ShortcutOpenRdpConnectionAdmin = kprSettingsControl[KprMenu.MenuItem.OpenRdpConnectionAdmin].Hotkey; // txtOpenRdpAdminKey.Hotkey;
                _config.ShortcutOpenRdpConnectionNoCred = kprSettingsControl[KprMenu.MenuItem.OpenRdpConnectionNoCred].Hotkey; // txtOpenRdpNoCredKey.Hotkey;
                _config.ShortcutOpenRdpConnectionNoCredAdmin = kprSettingsControl[KprMenu.MenuItem.OpenRdpConnectionNoCredAdmin].Hotkey; //  txtOpenRdpNoCredAdminKey.Hotkey;
                _config.ShortcutIgnoreCredentials = kprSettingsControl[KprMenu.MenuItem.IgnoreCredentials].Hotkey;*/

                var menuItems = KprMenu.MenuItemValues.ToDictionary(x => x, x => kprSettingsControl[x]);
                foreach (var kv in menuItems)
                    _config.SetShortcut(kv.Key, kv.Value.Hotkey);
                _config.KeePassContextMenuItems = menuItems.Aggregate(KprMenu.MenuItem.Empty, (a, b) => a |= b.Value.ContextMenuChecked ? b.Key : KprMenu.MenuItem.Empty); //(lstKeePassContextMenuItems.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>).Aggregate(KprMenu.MenuItem.Empty, (a, b) => a |= b.Key);
                _config.KeePassToolbarItems = menuItems.Aggregate(KprMenu.MenuItem.Empty, (a, b) => a |= b.Value.ToolbarChecked ? b.Key : KprMenu.MenuItem.Empty); //(lstKeePassToolbarItems.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>).Aggregate(KprMenu.MenuItem.Empty, (a, b) => a |= b.Key);
            }

            if (_tabPickerInitialized)
            {
                _config.CredPickerSecureDesktop = chkCredPickSecureDesktop.Checked;
                _config.CredPickerRememberSize = chkCredPickRememberSize.Checked;
                _config.CredPickerWidth = (int)numCredPickWidth.Value;
                _config.CredPickerHeight = (int)numCredPickHeight.Value;
                _config.CredPickerRememberSortOrder = chkCredPickRememberSortOrder.Checked;

                _config.KeePassShowResolvedReferences = chkKeepassShowResolvedReferences.Checked;
                _config.CredPickerShowInGroups = chkCredPickShowInGroups.Checked;
                _config.CredPickerIncludeSelected = chkCredPickIncludeSelected.Checked;
                _config.CredPickerLargeRows = chkCredPickLargeRows.Checked;

                _config.CredPickerCustomGroup = string.IsNullOrWhiteSpace(txtCredPickerCustomGroup.Text) || txtCredPickerCustomGroup.Text == Util.DefaultTriggerGroup ?
                    string.Empty :
                    txtCredPickerCustomGroup.Text;

                _config.CredPickerTriggerRecursive = chkCredPickerTriggerRecursive.Checked;

                if (lstRegExPre.DataSource is BindingList<string>)
                    _config.CredPickerRegExPre = string.Join("|", (lstRegExPre.DataSource as BindingList<string>).Where(x => !string.IsNullOrEmpty(x)));
                if (lstRegExPost.DataSource is BindingList<string>)
                    _config.CredPickerRegExPost = string.Join("|", (lstRegExPost.DataSource as BindingList<string>).Where(x => !string.IsNullOrEmpty(x)));
            }

            if (_tabExecutableInitialized)
            {
                _config.MstscUseFullscreen = chkMstscUseFullscreen.Checked;
                _config.MstscUsePublic = chkMstscUsePublic.Checked;
                _config.MstscUseAdmin = chkMstscUseAdmin.Checked;
                _config.MstscUseRestrictedAdmin = chkMstscUseRestrictedAdmin.Checked;
                _config.MstscUseRemoteGuard = chkMstscUseRemoteGuard.Checked;
                _config.MstscUseSpan = chkMstscUseSpan.Checked;
                _config.MstscUseMultimon = chkMstscUseMultimon.Checked;
                _config.MstscWidth = (int)numMstscWidth.Value;
                _config.MstscHeight = (int)numMstscHeight.Value;
                _config.MstscReplaceTitle = chkMstscReplaceTitle.Checked;
                _config.MstscCleanupRegistry = chkMstscCleanupRegistry.Checked;
                _config.MstscConfirmCertificate = chkMstscConfirmCertificate.Checked;
                _config.MstscHandleCredDialog = chkMstscHandleCredDialog.Checked;

                if (!chkMstscSignRdpFiles.Checked || txtMstscSignRdpFiles.Text == txtMstscSignRdpFiles.Tag as string)
                {
                    txtMstscSignRdpFiles.TextChanged -= txtMstscSignRdpFiles_TextChanged;
                    txtMstscSignRdpFiles.Text = string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(_config.MstscSignRdpFiles) &&
                    (!chkMstscSignRdpFiles.Checked || string.IsNullOrWhiteSpace(txtMstscSignRdpFiles.Text) ||
                    !string.Equals(_config.MstscSignRdpFiles, txtMstscSignRdpFiles.Text, StringComparison.OrdinalIgnoreCase)))
                {
                    var thumbprint = string.Format("{0}00", _config.MstscSignRdpFiles);
                    try
                    {
                        using (var tsc = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Terminal Server Client", true))
                        using (var pbl = tsc.CreateSubKey("PublisherBypassList"))
                            if (pbl.GetValue(thumbprint, 0) as int? == 0xff)
                                pbl.DeleteValue(thumbprint);
                    }
                    catch
                    {
                    }
                }
                if (chkMstscSignRdpFiles.Checked && !string.IsNullOrWhiteSpace(txtMstscSignRdpFiles.Text))
                {
                    /*if (string.IsNullOrWhiteSpace(_config.MstscSignRdpFiles) ||
                        !string.Equals(_config.MstscSignRdpFiles, txtMstscSignRdpFiles.Text, StringComparison.OrdinalIgnoreCase))*/
                    {
                        var thumbprint = string.Format("{0}00", txtMstscSignRdpFiles.Text);
                        try
                        {
                            using (var tsc = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Terminal Server Client", true))
                            using (var pbl = tsc.CreateSubKey("PublisherBypassList"))
                                if (pbl.GetValue(thumbprint, 0) as int? != 0xff)
                                    pbl.SetValue(thumbprint, 0xff, RegistryValueKind.DWord);
                        }
                        catch { }

                        _config.MstscSignRdpFiles = txtMstscSignRdpFiles.Text;
                    }
                }
                else
                    _config.MstscSignRdpFiles = KprConfig.Default.MstscSignRdpFiles;

                // HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy Objects\{B143DA1D-E406-4B17-A27F-BC9BDF1DBE3A}User\SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services
                // TrustedCertThumbprints

                var item = cbMstscExecutable.SelectedItem as string;
                if (!string.IsNullOrWhiteSpace(item) && item != typeof(MstscCommand).Name)
                {
                    if (!string.IsNullOrWhiteSpace(txtMstscCustomCommand.Text))
                        _config.MstscExecutable = string.Format("{0}:{1}", item, txtMstscCustomCommand.Text);
                    else
                        _config.MstscExecutable = item;
                }
                else
                    _config.MstscExecutable = KprConfig.Default.MstscExecutable;

                _config.MstscSecureDesktop = chkMstscSecureDesktop.Checked;
            }

            _realConfig.CopyFrom(_config);
        }

        private void num_Enter(object sender, EventArgs e)
        {
            var numericUpDown = sender as NumericUpDown;
            numericUpDown.ValueChanged += num_ValueChanged;
            _oldNumValue = numericUpDown.Value;
            AcceptButton = CancelButton = null;
        }

        private void num_Leave(object sender, EventArgs e)
        {
            var numericUpDown = sender as NumericUpDown;
            numericUpDown.ValueChanged -= num_ValueChanged;
            _oldNumValue = null;
            AcceptButton = cmdOk;
            CancelButton = cmdCancel;
        }

        private void num_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.None)
                ResetActiveControl(sender as Control);
            else if (e.KeyCode == Keys.Escape && e.Modifiers == Keys.None)
            {
                var numericUpDown = sender as NumericUpDown;
                numericUpDown.ValueChanged -= num_ValueChanged;
                numericUpDown.Value = numericUpDown.Minimum;
                numericUpDown.ValueChanged += num_ValueChanged;
                numericUpDown.Value = _oldNumValue.Value;
                ResetActiveControl(sender as Control);
            }
        }

        private void num_ValueChanged(object sender, EventArgs e)
        {
            var numericUpDown = sender as NumericUpDown;
            _oldNumValue = numericUpDown.Value;
        }

        private void txt_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Modifiers == Keys.None)
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                    case Keys.Escape:
                        var textBox = sender as TextBox;
                        if (string.IsNullOrEmpty(textBox.Text))
                            return;
                        e.IsInputKey = true;
                        break;
                }
        }

        private void lst_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Modifiers == Keys.None)
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        var listBox = sender as ListBox;
                        if (listBox.SelectedIndices.Count == 0)
                            return;
                        e.IsInputKey = true;
                        break;
                }
        }

        private void lst_KeyDownEnter(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.None)
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        var listBox = sender as ListBox;
                        var button = listBox.Tag as Button;
                        button.PerformClick();
                        e.SuppressKeyPress = e.Handled = true;
                        break;
                }
        }

        private void lst_KeyDownDelete(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.None)
                switch (e.KeyCode)
                {
                    case Keys.Delete:
                        var listBox = sender as ListBox;
                        var button = listBox.Tag as Button;
                        button.PerformClick();
                        e.SuppressKeyPress = e.Handled = true;
                        break;
                }
        }

        private void tabAbout_SizeChanged(object sender, EventArgs e)
        {
            if (!_tabAboutInitialized || UseWaitCursor)
                return;

            var rowCount = tblAbout.RowCount;
            if (rowCount < 1)
                return;

            var heights = tblAbout.GetRowHeights();
            if (heights.Length < 5)
                return;

            var autosize = heights[1] + heights[2] + heights[3] + heights[4];
            var height = tabAbout.Height - autosize;
            var h1 = DpiUtil.ScaleIntY(256);
            if (height > h1)
                height = h1;

            if (rowCount > 0 && tblAbout.RowStyles[0].Height != height)
            {
                tblAbout.SuspendLayout();
                tblAbout.RowStyles[0] = new RowStyle(SizeType.Absolute, height);
                tblAbout.ResumeLayout();
            }
        }

        private void tblAbout_SizeChanged(object sender, EventArgs e)
        {
            tblAbout.Location = new Point(tabAbout.Width / 2 - tblAbout.Width / 2, tabAbout.Height / 2 - tblAbout.Height / 2);
        }

        private void llWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                KeePass.Util.WinUtil.OpenUrl(Util.WebsiteUrl, null);
                e.Link.Visited = true;
            }
        }

        private void llLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                KeePass.Util.WinUtil.OpenUrl(Util.LicenseUrl, null);
                e.Link.Visited = true;
            }
        }

        private void ResetActiveControl(Control control)
        {
            if (control == null || ActiveControl == control)
                ActiveControl = null;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_MOVING = 0x0216;
            if (m.Msg == WM_MOVING && !UseWaitCursor)
            {
                _isMoving = true;

                var page = tcKprOptionsForm.SelectedTab;
                if (page != tabIntegration)
                    page.SuspendLayout();

                tabIntegration.SuspendLayout();
                tblIntegration.SuspendLayout();
                flpOptions.SuspendLayout();
                kprSettingsControl.SuspendLayout();

                base.WndProc(ref m);

                _isMoving = false;

                Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                {
                    if (_isMoving)
                        return;

                    tblIntegration.ResumeLayout(false);
                    flpOptions.ResumeLayout(false);
                    kprSettingsControl.ResumeLayout(false);
                    tabIntegration.ResumeLayout(false);

                    if (page != tabIntegration)
                        page.ResumeLayout(false);
                })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }
            else
                base.WndProc(ref m);
        }

        /*[DllImport("User32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        private enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }

        private enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_ENABLE_HOSTBACKDROP = 5,
            ACCENT_INVALID_STATE = 6
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }*/
    }
}