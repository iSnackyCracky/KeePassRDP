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

using KeePass;
using KeePass.UI;
using KeePassRDP.Resources;
using KeePassRDP.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP
{
    public partial class KprOptionsForm : Form
    {
        private readonly KprConfig _config;
        private readonly Font _font;
        private readonly Font _pwFont;
        private readonly Lazy<Image> _largeIcon;
        private readonly Lazy<Image> _largestIcon;
        private readonly Timer _txtCredPickerCustomGroupTooltipTimer;
        private readonly Lazy<Size> _txtCredPickerCustomGroupCursorSize;

        private decimal? _oldNumValue;
        private Point? _lastTooltipMousePosition;
        private NativeCredentials.Credential[] _credentials;

        private bool _tabIntegrationInitialized;
        private bool _tabPickerInitialized;
        private bool _tabExecutableInitialized;
        private bool _tabVaultInitialized;
        private bool _tabAboutInitialized;

        public KprOptionsForm(KprConfig config, IDictionary<KprMenu.MenuItem, ToolStripItem> toolbarItems)
        {
            _config = config;

            _largeIcon = new Lazy<Image>(() =>
            {
                using (var icon = IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, DpiUtil.ScaleIntY(48)))
                    return icon.ToBitmap();
            }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _largestIcon = new Lazy<Image>(() =>
            {
                using (var icon = IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, 256))
                    return icon.ToBitmap();
            }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _txtCredPickerCustomGroupTooltipTimer = new Timer
            {
                Interval = 500,
                Enabled = false,
            };

            _txtCredPickerCustomGroupCursorSize = new Lazy<Size>(() =>
            {
                Size size;// = Util.GetIconSize(Cursor, Cursor.Handle);
                if (CursorUtil.GetIconSize(out size))
                    return size;
                return Size.Empty;
            }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _oldNumValue = null;
            _lastTooltipMousePosition = null;
            _credentials = null;

            _tabIntegrationInitialized =
                _tabPickerInitialized =
                _tabExecutableInitialized =
                _tabVaultInitialized =
                _tabAboutInitialized = false;

            InitializeComponent();

            SuspendLayout();

            switch (Resources.Resources.Culture.TwoLetterISOLanguageName)
            {
                case "de":
                    MinimumSize = new Size(MinimumSize.Width + 50, MinimumSize.Height);
                    break;
            }

            Util.SetDoubleBuffered(tcKprOptionsForm);
            Util.SetDoubleBuffered(tabIntegration);
            Util.SetDoubleBuffered(tabPicker);
            Util.SetDoubleBuffered(tabExecutable);
            Util.SetDoubleBuffered(tabVault);
            Util.SetDoubleBuffered(tabAbout);
            Util.SetDoubleBuffered(tblKprOptionsForm);
            Util.SetDoubleBuffered(tblIntegration);
            Util.SetDoubleBuffered(tblKeyboardSettings);
            Util.SetDoubleBuffered(tblKeePassContextMenuItems);
            Util.SetDoubleBuffered(tblKeePassToolbarItems);
            Util.SetDoubleBuffered(tblVisibilitySettings);
            Util.SetDoubleBuffered(tblPicker);
            Util.SetDoubleBuffered(tblVault);
            Util.SetDoubleBuffered(tblAbout);
            Util.SetDoubleBuffered(lstKeePassContextMenuItems);
            Util.SetDoubleBuffered(lstKeePassContextMenuItemsAvailable);
            Util.SetDoubleBuffered(lstKeePassToolbarItems);
            Util.SetDoubleBuffered(lstKeePassToolbarItemsAvailable);
            Util.SetDoubleBuffered(lstRegExPre);
            Util.SetDoubleBuffered(lstRegExPost);
            Util.SetDoubleBuffered(lvVault);

            btnOpenRdpShortcutIcon.Image = toolbarItems[KprMenu.MenuItem.OpenRdpConnection].Image;
            btnOpenRdpAdminShortcutIcon.Image = toolbarItems[KprMenu.MenuItem.OpenRdpConnectionAdmin].Image;
            btnOpenRdpNoCredShortcutIcon.Image = toolbarItems[KprMenu.MenuItem.OpenRdpConnectionNoCred].Image;
            btnOpenRdpNoCredAdminShortcut.Image = toolbarItems[KprMenu.MenuItem.OpenRdpConnectionNoCredAdmin].Image;

            Icon = IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, UIUtil.GetIconSize().Height);

            if (Program.Config.UI.StandardFont != null)
            {
                // Never dispose cached KeePass font.
                var sf = Program.Config.UI.StandardFont.ToFont();
                _font = new Font(sf.FontFamily, lvVault.Font.Size);
            }
            else
                _font = lvVault.Font;

            if (Program.Config.UI.PasswordFont != null)
            {
                // Never dispose cached KeePass font.
                var sf = Program.Config.UI.PasswordFont.ToFont();
                _pwFont = new Font(sf.FontFamily, lvVault.Font.Size);
            }
            else
                _pwFont = _font;

            lstKeePassContextMenuItems.Tag = cmdKeePassContextMenuItemsRemove;
            lstKeePassContextMenuItemsAvailable.Tag = cmdKeePassContextMenuItemsAdd;
            lstKeePassToolbarItems.Tag = cmdKeePassToolbarItemsRemove;
            lstKeePassToolbarItemsAvailable.Tag = cmdKeePassToolbarItemsAdd;
            lstRegExPre.Tag = cmdRegExPreRemove;
            lstRegExPost.Tag = cmdRegExPostRemove;
            txtRegExPre.Tag = lstRegExPre;
            txtRegExPost.Tag = lstRegExPost;

            KprResourceManager.Instance.TranslateMany(
                this,
                tabIntegration,
                tabPicker,
                tabExecutable,
                tabVault,
                tabAbout,
                cmdCancel
            );

            ResumeLayout(false);
        }

        public new void Dispose()
        {
            if (_pwFont != _font)
                _pwFont.Dispose();
            if (lvVault.Font != _font)
                _font.Dispose();
            Icon.Dispose();
            if (_largeIcon.IsValueCreated)
                _largeIcon.Value.Dispose();
            if (_largestIcon.IsValueCreated)
                _largestIcon.Value.Dispose();
            base.Dispose();
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

        private void KprOptionsForm_Load(object sender, EventArgs e)
        {
            GlobalWindowManager.AddWindow(this);

            UseWaitCursor = true;
            SuspendLayout();

            tcKprOptionsForm_Selected(null, new TabControlEventArgs(tabIntegration, 0, TabControlAction.Selected));

            BannerFactory.CreateBannerEx(
                this,
                pbKprOptionsForm,
                _largeIcon.Value,
                Util.KeePassRDP,
                KprResourceManager.Instance["Here you can configure all KeePassRDP related options."],
                true);

            ResumeLayout(false);
            UseWaitCursor = false;

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

            CleanCredentials();

            MessageFilter.ListBoxMouseWheelHandler.Enable(false);
            MessageFilter.FormClickHandler.Enable(false);
        }

        private void KprOptionsForm_ResizeBegin(object sender, EventArgs e)
        {
            foreach(TabPage page in tcKprOptionsForm.TabPages)
            {
                if (page == tcKprOptionsForm.SelectedTab)
                {
                    if (page == tabIntegration)
                    {
                        tblKeyboardSettings.SuspendLayout();
                        tblVisibilitySettings.SuspendLayout();
                    }
                    continue;
                }
                page.SuspendLayout();
                page.Controls.Clear();
            }
        }

        private void KprOptionsForm_ResizeEnd(object sender, EventArgs e)
        {
            foreach (TabPage page in tcKprOptionsForm.TabPages)
            {
                if (page == tabIntegration)
                {
                    if (!page.Controls.Contains(tblIntegration))
                        page.Controls.Add(tblIntegration);
                    tblKeyboardSettings.ResumeLayout(page == tcKprOptionsForm.SelectedTab);
                    tblVisibilitySettings.ResumeLayout(page == tcKprOptionsForm.SelectedTab);
                }
                else if (page == tabPicker)
                {
                    if (!page.Controls.Contains(tblPicker))
                        page.Controls.Add(tblPicker);
                }
                else if (page == tabExecutable)
                {
                    if (!page.Controls.Contains(grpMstscParameters))
                        page.Controls.Add(grpMstscParameters);
                    if (!page.Controls.Contains(grpMstscAutomation))
                        page.Controls.Add(grpMstscAutomation);
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
                page.ResumeLayout(false);
            }
        }

        private void KprOptionsForm_SizeChanged(object sender, EventArgs e)
        {
            if (UseWaitCursor || pbKprOptionsForm.Image == null || pbKprOptionsForm.Image.Width == 0)
                return;

            var oldWidth = pbKprOptionsForm.Image.Width;
            BannerFactory.UpdateBanner(
                this,
                pbKprOptionsForm,
                _largeIcon.Value,
                Util.KeePassRDP,
                KprResourceManager.Instance["Here you can configure all KeePassRDP related options."],
                ref oldWidth);

            if (tcKprOptionsForm.SelectedTab == tabIntegration)
            {
                tblKeyboardSettings.ResumeLayout();
                tblKeyboardSettings.SuspendLayout();
                tblVisibilitySettings.ResumeLayout();
                tblVisibilitySettings.SuspendLayout();
            }
        }

        private void tcKprOptionsForm_Selected(object sender, TabControlEventArgs e)
        {
            if (_tabVaultInitialized)
                CleanCredentials();

            // Set width and height fields maximum to (primary) screen resolution.

            if (e.TabPage == tabIntegration)
            {
                if (!_tabIntegrationInitialized)
                {
                    _tabIntegrationInitialized = true;

                    tabIntegration.UseWaitCursor = true;
                    tabIntegration.SuspendLayout();

                    KprOptionsForm_ResizeBegin(null, EventArgs.Empty);

                    var invoke = BeginInvoke(new Action(() =>
                    {
                        KprResourceManager.Instance.TranslateMany(
                            grpCredentialOptions,
                            grpEntryOptions,
                            grpHotkeyOptions,
                            lblCredVaultTtl,
                            lblKeyboardSettings,
                            lblShortcut,
                            lblOpenRdpShortcut,
                            lblOpenRdpAdminShortcut,
                            lblOpenRdpNoCredShortcut,
                            lblOpenRdpNoCredAdminShortcut,
                            lblVisibilitySettings,
                            lblKeePassContextMenuItems,
                            lblKeePassToolbarItems,
                            chkCredVaultUseWindows,
                            chkCredVaultOverwriteExisting,
                            chkCredVaultRemoveOnExit,
                            chkCredVaultAdaptiveTtl,
                            chkKeePassConnectToAll,
                            chkKeePassAlwaysConfirm,
                            chkKeePassContextMenuOnScreen,
                            chkKeePassHotkeysRegisterLast
                        );

                        // Set form elements to match previously saved options.
                        chkCredVaultUseWindows.Checked = _config.CredVaultUseWindows;
                        chkCredVaultOverwriteExisting.Checked = _config.CredVaultOverwriteExisting;
                        chkCredVaultRemoveOnExit.Checked = _config.CredVaultRemoveOnExit;
                        chkCredVaultAdaptiveTtl.Checked = _config.CredVaultAdaptiveTtl;

                        numCredVaultTtl.Value = Math.Max(Math.Min(_config.CredVaultTtl, numCredVaultTtl.Maximum), numCredVaultTtl.Minimum);
                        _config.CredVaultTtl = (int)numCredVaultTtl.Value;

                        chkKeePassConnectToAll.Checked = _config.KeePassConnectToAll;
                        chkKeePassAlwaysConfirm.Checked = _config.KeePassAlwaysConfirm;
                        chkKeePassContextMenuOnScreen.Checked = _config.KeePassContextMenuOnScreen;
                        chkKeePassHotkeysRegisterLast.Checked = _config.KeePassHotkeysRegisterLast;

                        txtOpenRdpKey.Hotkey = _config.ShortcutOpenRdpConnection;
                        txtOpenRdpAdminKey.Hotkey = _config.ShortcutOpenRdpConnectionAdmin;
                        txtOpenRdpNoCredKey.Hotkey = _config.ShortcutOpenRdpConnectionNoCred;
                        txtOpenRdpNoCredAdminKey.Hotkey = _config.ShortcutOpenRdpConnectionNoCredAdmin;

                        var keePassContextMenuItems = new BindingList<KeyValuePair<KprMenu.MenuItem, string>>();
                        var keePassToolbarItems = new BindingList<KeyValuePair<KprMenu.MenuItem, string>>();
                        var keePassContextMenuItemsAvailable = new BindingList<KeyValuePair<KprMenu.MenuItem, string>>();
                        var keePassToolbarItemsAvailable = new BindingList<KeyValuePair<KprMenu.MenuItem, string>>();

                        foreach (var menu in KprMenu.MenuItemValues)
                        {
                            var text = KprMenu.GetText(menu);

                            if (_config.KeePassContextMenuItems.HasFlag(menu))
                                keePassContextMenuItems.Add(new KeyValuePair<KprMenu.MenuItem, string>(menu, text));
                            else
                                keePassContextMenuItemsAvailable.Add(new KeyValuePair<KprMenu.MenuItem, string>(menu, text));

                            if (_config.KeePassToolbarItems.HasFlag(menu))
                                keePassToolbarItems.Add(new KeyValuePair<KprMenu.MenuItem, string>(menu, text));
                            else
                                keePassToolbarItemsAvailable.Add(new KeyValuePair<KprMenu.MenuItem, string>(menu, text));
                        }

                        lstKeePassContextMenuItems.DataSource = keePassContextMenuItems;
                        lstKeePassContextMenuItemsAvailable.DataSource = keePassContextMenuItemsAvailable;
                        lstKeePassToolbarItems.DataSource = keePassToolbarItems;
                        lstKeePassToolbarItemsAvailable.DataSource = keePassToolbarItemsAvailable;

                        tabIntegration.ResumeLayout(false);
                        tabIntegration.UseWaitCursor = false;

                        KprOptionsForm_ResizeEnd(null, EventArgs.Empty);
                    }));

                    if (!invoke.IsCompleted)
                        Task.Factory.FromAsync(
                            invoke,
                            endinvoke => EndInvoke(endinvoke),
                            TaskCreationOptions.AttachedToParent,
                            TaskScheduler.Default);
                    else
                        EndInvoke(invoke);
                }
                else
                {
                    if (tabIntegration.VerticalScroll.Visible)
                        tabIntegration.AutoScrollPosition = new Point(0, 0);
                }
            }
            else if (e.TabPage == tabPicker)
            {
                if (!_tabPickerInitialized)
                {
                    _tabPickerInitialized = true;

                    tabPicker.UseWaitCursor = true;
                    tabPicker.SuspendLayout();

                    KprResourceManager.Instance.TranslateMany(
                        grpCredPickerGeneralOptions,
                        grpCredPickerEntryOptions,
                        grpCredPickerTriggerOptions,
                        lblCredPickWidth,
                        lblCredPickHeight,
                        lblCredPickerCustomGroup,
                        lblRegexPrefixes,
                        lblRegexPostfixes,
                        chkCredPickRememberSize,
                        chkCredPickRememberSortOrder,
                        chkKeepassShowResolvedReferences,
                        chkCredPickShowInGroups,
                        chkCredPickIncludeSelected,
                        chkCredPickLargeRows
                    );

                    UIUtil.SetCueBanner(txtCredPickerCustomGroup, Util.DefaultTriggerGroup);

                    // Set form elements to match previously saved options.
                    chkKeepassShowResolvedReferences.Checked = _config.KeePassShowResolvedReferences;

                    chkCredPickRememberSize.Checked = _config.CredPickerRememberSize;
                    chkCredPickRememberSortOrder.Checked = _config.CredPickerRememberSortOrder;
                    txtCredPickerCustomGroup.Text = string.IsNullOrWhiteSpace(_config.CredPickerCustomGroup) || _config.CredPickerCustomGroup == Util.DefaultTriggerGroup ?
                        string.Empty :
                        _config.CredPickerCustomGroup;
                    chkCredPickShowInGroups.Checked = _config.CredPickerShowInGroups;
                    chkCredPickIncludeSelected.Checked = _config.CredPickerIncludeSelected;
                    chkCredPickLargeRows.Checked = _config.CredPickerLargeRows;

                    numCredPickWidth.Maximum = Screen.PrimaryScreen.Bounds.Width;
                    numCredPickWidth.Value = Math.Max(Math.Min(_config.CredPickerWidth, numCredPickWidth.Maximum), numCredPickWidth.Minimum);
                    _config.CredPickerWidth = (int)numCredPickWidth.Value;

                    numCredPickHeight.Maximum = Screen.PrimaryScreen.Bounds.Height;
                    numCredPickHeight.Value = Math.Max(Math.Min(_config.CredPickerHeight, numCredPickHeight.Maximum), numCredPickHeight.Minimum);
                    _config.CredPickerHeight = (int)numCredPickHeight.Value;

                    if (!string.IsNullOrWhiteSpace(_config.CredPickerRegExPre))
                        lstRegExPre.DataSource = new BindingList<string>(_config.CredPickerRegExPre.Split('|').ToList());
                    else
                        lstRegExPre.DataSource = new BindingList<string>();
                    if (!string.IsNullOrWhiteSpace(_config.CredPickerRegExPost))
                        lstRegExPost.DataSource = new BindingList<string>(_config.CredPickerRegExPost.Split('|').ToList());
                    else
                        lstRegExPost.DataSource = new BindingList<string>();

                    tabPicker.ResumeLayout(false);
                    tabPicker.UseWaitCursor = false;
                }
            }
            else if (e.TabPage == tabExecutable)
            {
                if (!_tabExecutableInitialized)
                {
                    _tabExecutableInitialized = true;

                    tabExecutable.UseWaitCursor = true;
                    tabExecutable.SuspendLayout();

                    KprResourceManager.Instance.TranslateMany(
                        grpMstscParameters,
                        grpMstscAutomation,
                        lblWidth,
                        lblHeight,
                        chkMstscReplaceTitle,
                        chkMstscConfirmCertificate
                    );

                    // Set form elements to match previously saved options.
                    chkMstscUseFullscreen.Checked = _config.MstscUseFullscreen;
                    chkMstscUsePublic.Checked = _config.MstscUsePublic;
                    chkMstscUseAdmin.Checked = _config.MstscUseAdmin;
                    chkMstscUseRestrictedAdmin.Checked = _config.MstscUseRestrictedAdmin;
                    chkMstscUseRemoteGuard.Checked = _config.MstscUseRemoteGuard;
                    chkMstscUseSpan.Checked = _config.MstscUseSpan;
                    chkMstscUseMultimon.Checked = _config.MstscUseMultimon;
                    chkMstscReplaceTitle.Checked = _config.MstscReplaceTitle;
                    chkMstscConfirmCertificate.Checked = _config.MstscConfirmCertificate;

                    numMstscWidth.Maximum = Screen.PrimaryScreen.Bounds.Width;
                    numMstscWidth.Value = Math.Max(Math.Min(_config.MstscWidth, numMstscWidth.Maximum), numMstscWidth.Minimum);
                    _config.MstscWidth = (int)numMstscWidth.Value;

                    numMstscHeight.Maximum = Screen.PrimaryScreen.Bounds.Height;
                    numMstscHeight.Value = Math.Max(Math.Min(_config.MstscHeight, numMstscHeight.Maximum), numMstscHeight.Minimum);
                    _config.MstscHeight = (int)numMstscHeight.Value;

                    chkMstsc_CheckedChanged(null, EventArgs.Empty);

                    grpMstscParameters.Enabled = File.Exists(KeePassRDPExt.MstscPath);

                    tabExecutable.ResumeLayout(false);
                    tabExecutable.UseWaitCursor = false;
                }
            }
            else if (e.TabPage == tabVault)
            {
                if (!_tabVaultInitialized)
                {
                    _tabVaultInitialized = true;

                    tabVault.UseWaitCursor = true;
                    tabVault.SuspendLayout();
                    lvVault.SuspendLayout();

                    lvVault.Font = new Font(_font.FontFamily, _font.Size * 2f);
                    lvVault.Columns.AddRange(new[]
                    {
                        new ColumnHeader { Text = "TargetName" },
                        new ColumnHeader { Text = "TargetAlias" },
                        new ColumnHeader { Text = "UserName" },
                        new ColumnHeader { Text = "CredentialBlob" },
                        new ColumnHeader { Text = "Type" },
                        new ColumnHeader { Text = "Persist" },
                        new ColumnHeader { Text = "Flags" },
                        new ColumnHeader { Text = "Comment" },
                        new ColumnHeader { Text = "LastWritten" },
                        new ColumnHeader { Text = "Attributes" }
                    });
                    lvVault.Invalidated += lvVault_Invalidated;

                    KprResourceManager.Instance.TranslateMany(
                        chkSavedCredsShowAll
                    );

                    tabVault.ResumeLayout(false);
                    tabVault.UseWaitCursor = false;
                    lvVault.ResumeLayout(false);
                }

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

                    lblVersionText.Text = KprVersion.Version + " (" + Resources.Resources.Culture.TwoLetterISOLanguageName + ")";
                    lblRevisionText.Text = KprVersion.Revision;

                    if (pbAbout.Image == null || pbAbout.Image.Width == 0)
                        pbAbout.Image = _largestIcon.Value;

                    tblAbout.RowStyles[0] = new RowStyle(SizeType.Absolute, (int)Math.Max(DpiUtil.ScaleIntY(256), tblAbout.RowStyles[0].Height / DpiUtil.FactorY));
                    tblAbout.Location = new Point(tabAbout.Width / 2 - tblAbout.Width / 2, tabAbout.Height / 2 - tblAbout.Height / 2);

                    tabAbout.ResumeLayout(false);
                    tabAbout.UseWaitCursor = false;
                    tblAbout.ResumeLayout(false);
                }
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
                _config.KeePassContextMenuOnScreen = chkKeePassContextMenuOnScreen.Checked;
                _config.KeePassHotkeysRegisterLast = chkKeePassHotkeysRegisterLast.Checked;

                _config.ShortcutOpenRdpConnection = txtOpenRdpKey.Hotkey;
                _config.ShortcutOpenRdpConnectionAdmin = txtOpenRdpAdminKey.Hotkey;
                _config.ShortcutOpenRdpConnectionNoCred = txtOpenRdpNoCredKey.Hotkey;
                _config.ShortcutOpenRdpConnectionNoCredAdmin = txtOpenRdpNoCredAdminKey.Hotkey;

                _config.KeePassContextMenuItems = (lstKeePassContextMenuItems.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>).Aggregate(KprMenu.MenuItem.Empty, (a, b) => a |= b.Key);
                _config.KeePassToolbarItems = (lstKeePassToolbarItems.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>).Aggregate(KprMenu.MenuItem.Empty, (a, b) => a |= b.Key);
            }

            if (_tabPickerInitialized)
            {
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

                if (lstRegExPre.DataSource != null)
                    _config.CredPickerRegExPre = string.Join("|", (lstRegExPre.DataSource as BindingList<string>).Where(x => !string.IsNullOrEmpty(x)));
                if (lstRegExPost.DataSource != null)
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
                _config.MstscConfirmCertificate = chkMstscConfirmCertificate.Checked;
            }
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
            if (ActiveControl == sender && e.KeyCode == Keys.Enter && e.Modifiers == Keys.None)
                ActiveControl = null;
            if (ActiveControl == sender && e.KeyCode == Keys.Escape && e.Modifiers == Keys.None)
            {
                var numericUpDown = sender as NumericUpDown;
                numericUpDown.ValueChanged -= num_ValueChanged;
                numericUpDown.Value = numericUpDown.Minimum;
                numericUpDown.ValueChanged += num_ValueChanged;
                numericUpDown.Value = _oldNumValue.Value;
                ActiveControl = null;
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
            if (!_tabAboutInitialized || tabAbout.UseWaitCursor)
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
            KeePass.Util.WinUtil.OpenUrl(Util.WebsiteUrl, null);
        }

        private void llLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            KeePass.Util.WinUtil.OpenUrl(Util.LicenseUrl, null);
        }

        private void ResetActiveControl(Control control)
        {
            if (control == null || ActiveControl == control)
                ActiveControl = null;
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