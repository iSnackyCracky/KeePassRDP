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

using KeePassRDP.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP
{
    public partial class KprOptionsForm
    {
        private void Init_TabIntegration()
        {
            if (!_tabIntegrationInitialized)
            {
                _tabIntegrationInitialized = true;

                kprSettingsControl.UseWaitCursor = true;
                tabIntegration.UseWaitCursor = true;
                tabIntegration.AutoScroll = false;
                tabIntegration.SuspendLayout();
                tblIntegration.SuspendLayout();
                kprSettingsControl.SuspendLayout();

                KprOptionsForm_ResizeBegin(null, EventArgs.Empty);

                Task.Factory.StartNew(() =>
                {
                    var invoke = BeginInvoke(new Action(() =>
                    {
                        KprResourceManager.Instance.TranslateMany(
                            grpCredentialOptions,
                            grpEntryOptions,
                            grpHotkeyOptions,
                            lblCredVaultTtl,
                            chkCredVaultUseWindows,
                            chkCredVaultOverwriteExisting,
                            chkCredVaultRemoveOnExit,
                            chkCredVaultAdaptiveTtl,
                            chkKeePassConnectToAll,
                            chkKeePassAlwaysConfirm,
                            chkKeePassDefaultEntryAction,
                            chkKeePassContextMenuOnScreen,
                            chkKeePassHotkeysRegisterLast,
                            chkKeePassConfirmOnClose
                        );

                        if (Util.CheckCredentialGuard())
                            chkCredVaultUseWindows.ForeColor = Color.Gray;

                        Config_TabIntegration();

                        tblIntegration.ResumeLayout(false);
                        tabIntegration.ResumeLayout(false);
                        tabIntegration.AutoScroll = true;
                        tabIntegration.UseWaitCursor = false;

                        if (!tabIntegration.Created)
                            tabIntegration.CreateControl();
                    }));

                    if (!invoke.IsCompleted)
                        Task.Factory.FromAsync(
                            invoke,
                            endinvoke => EndInvoke(endinvoke),
                            TaskCreationOptions.AttachedToParent,
                            TaskScheduler.Default);
                    else
                        EndInvoke(invoke);
                }, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }
            else
            {
                if (tabIntegration.VerticalScroll.Visible)
                    tabIntegration.AutoScrollPosition = Point.Empty;

                kprSettingsControl.Visible = true;
            }
        }

        private void Config_TabIntegration()
        {
            // Set form elements to match previously saved options.
            chkCredVaultUseWindows.Checked = _config.CredVaultUseWindows;
            chkCredVaultOverwriteExisting.Checked = _config.CredVaultOverwriteExisting;
            chkCredVaultRemoveOnExit.Checked = _config.CredVaultRemoveOnExit;
            chkCredVaultAdaptiveTtl.Checked = _config.CredVaultAdaptiveTtl;

            numCredVaultTtl.Value = Math.Max(Math.Min(_config.CredVaultTtl, numCredVaultTtl.Maximum), numCredVaultTtl.Minimum);
            _config.CredVaultTtl = (int)numCredVaultTtl.Value;

            chkKeePassConnectToAll.Checked = _config.KeePassConnectToAll;
            chkKeePassAlwaysConfirm.Checked = _config.KeePassAlwaysConfirm;
            chkKeePassDefaultEntryAction.Checked = _config.KeePassDefaultEntryAction;
            chkKeePassContextMenuOnScreen.Checked = _config.KeePassContextMenuOnScreen;
            chkKeePassHotkeysRegisterLast.Checked = _config.KeePassHotkeysRegisterLast;
            chkKeePassConfirmOnClose.Checked = _config.KeePassConfirmOnClose;

            Task.Factory.StartNew(() =>
            {
                var items = new Dictionary<KprMenu.MenuItem, KprSettingsControl.MenuItemSettings>();

                foreach (var menuItem in KprMenu.MenuItemValues)
                {
                    Image image = null;
                    ToolStripItem temp = null;
                    if (_toolbarItems.TryGetValue(menuItem, out temp))
                        image = temp.Image;
                    items[menuItem] = new KprSettingsControl.MenuItemSettings
                    {
                        ContextMenuChecked = _config.KeePassContextMenuItems.HasFlag(menuItem),
                        ToolbarChecked = _config.KeePassToolbarItems.HasFlag(menuItem),
                        Hotkey = _config.GetShortcut(menuItem),
                        Image = image
                    };
                }

                if (!kprSettingsControl.ControlsCreated.IsSet)
                    if (!kprSettingsControl.ControlsCreated.Wait(TimeSpan.FromSeconds(5)))
                        throw new TimeoutException();

                Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                {
                    kprSettingsControl.BeginUpdate();
                    foreach (var kv in items)
                        kprSettingsControl[kv.Key] = kv.Value;
                    kprSettingsControl.EndUpdate();
                    kprSettingsControl.ResumeLayout(false);
                    kprSettingsControl.UseWaitCursor = false;
                    kprSettingsControl.Visible = true;

                    KprOptionsForm_ResizeEnd(null, EventArgs.Empty);
                })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void numCredVaultTtl_ValueChanged(object sender, EventArgs e)
        {
            chkCredVaultAdaptiveTtl.Enabled = numCredVaultTtl.Value > 0;
        }

        private void tabIntegration_Resize(object sender, EventArgs e)
        {
            if (!_tabIntegrationInitialized || tabIntegration.UseWaitCursor)
                return;

            var minHeight = Math.Max(tabIntegration.Height, _tblIntegrationMinHeight);
            if (tblIntegration.MinimumSize.Height != minHeight)
            {
                tblIntegration.SuspendLayout();
                tblIntegration.MinimumSize = new Size(0, minHeight);
                tblIntegration.ResumeLayout(true);
            }
        }
    }
}