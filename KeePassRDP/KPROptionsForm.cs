/*
 *  Copyright (C) 2018-2020 iSnackyCracky
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
 *  along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KeePassRDP
{
    public partial class KPROptionsForm : Form
    {
        private readonly KprConfig _config;
        private static int _AltValue = (int)Keys.Alt;
        private static int _CtrlValue = (int)Keys.Control;
        private static int _ShiftValue = (int)Keys.Shift;

        public KPROptionsForm(KprConfig config)
        {
            _config = config;
            InitializeComponent();
        }

        private void checkMstscSizeEnable(object sender, EventArgs e)
        {
            bool enable = true;
            if (chkMstscUseFullscreen.Checked || chkMstscUseMultimon.Checked || chkMstscUseSpan.Checked) { enable = false; }

            lblWidth.Enabled = enable;
            numMstscWidth.Enabled = enable;
            lblHeight.Enabled = enable;
            numMstscHeight.Enabled = enable;
        }

        private void KPROptionsForm_Load(object sender, EventArgs e)
        {
            // set width and height fields maximum to (primary) screen resolution
            numMstscWidth.Maximum = Screen.PrimaryScreen.Bounds.Width;
            numMstscHeight.Maximum = Screen.PrimaryScreen.Bounds.Height;
            numCredPickWidth.Maximum = Screen.PrimaryScreen.Bounds.Width;
            numCredPickHeight.Maximum = Screen.PrimaryScreen.Bounds.Height;

            // set form elements to match previously saved options
            chkKeepassShowResolvedReferences.Checked = _config.KeePassShowResolvedReferences;
            chkCredPickRememberSize.Checked = _config.CredPickerRememberSize;
            if (!string.IsNullOrWhiteSpace(_config.CredPickerRegExPre)) { lstRegExPre.Items.AddRange(_config.CredPickerRegExPre.Split('|')); }
            if (!string.IsNullOrWhiteSpace(_config.CredPickerRegExPost)) { lstRegExPost.Items.AddRange(_config.CredPickerRegExPost.Split('|')); }
            chkCredVaultUseWindows.Checked = _config.CredVaultUseWindows;
            chkMstscUseFullscreen.Checked = _config.MstscUseFullscreen;
            chkMstscUseAdmin.Checked = _config.MstscUseAdmin;
            chkMstscUseSpan.Checked = _config.MstscUseSpan;
            chkMstscUseMultimon.Checked = _config.MstscUseMultimon;
            var mstscWidth = Convert.ToDecimal(_config.MstscWidth);
            var mstscHeight = Convert.ToDecimal(_config.MstscHeight);
            var credPickWidth = Convert.ToDecimal(_config.CredPickerWidth);
            var credPickHeight = Convert.ToDecimal(_config.CredPickerHeight);
            var credVaultTtl = Convert.ToDecimal(_config.CredVaultTtl);


            var openRdpShortcut = Convert.ToInt32(_config.ShortcutOpenRdpConnection);
            if (openRdpShortcut - _AltValue > 0) { openRdpShortcut -= _AltValue; chkOpenRdpAlt.Checked = true; }
            if (openRdpShortcut - _CtrlValue > 0) { openRdpShortcut -= _CtrlValue; chkOpenRdpCtrl.Checked = true; }
            if (openRdpShortcut - _ShiftValue > 0) { openRdpShortcut -= _ShiftValue; chkOpenRdpShift.Checked = true; }
            txtOpenRdpKey.Text = ((Keys)openRdpShortcut).ToString();
            
            var openRdpAdminShortcut = Convert.ToInt32(_config.ShortcutOpenRdpConnectionAdmin);
            if (openRdpAdminShortcut - _AltValue > 0) { openRdpAdminShortcut -= _AltValue; chkOpenRdpAdminAlt.Checked = true; }
            if (openRdpAdminShortcut - _CtrlValue > 0) { openRdpAdminShortcut -= _CtrlValue; chkOpenRdpAdminCtrl.Checked = true; }
            if (openRdpAdminShortcut - _ShiftValue > 0) { openRdpAdminShortcut -= _ShiftValue; chkOpenRdpAdminShift.Checked = true; }
            txtOpenRdpAdminKey.Text = ((Keys)openRdpAdminShortcut).ToString();

            var openRdpNoCredShortcut = Convert.ToInt32(_config.ShortcutOpenRdpConnectionNoCred);
            if (openRdpNoCredShortcut - _AltValue > 0) { openRdpNoCredShortcut -= _AltValue; chkOpenRdpNoCredAlt.Checked = true; }
            if (openRdpNoCredShortcut - _CtrlValue > 0) { openRdpNoCredShortcut -= _CtrlValue; chkOpenRdpNoCredCtrl.Checked = true; }
            if (openRdpNoCredShortcut - _ShiftValue > 0) { openRdpNoCredShortcut -= _ShiftValue; chkOpenRdpNoCredShift.Checked = true; }
            txtOpenRdpNoCredKey.Text = ((Keys)openRdpNoCredShortcut).ToString();

            var openRdpNoCredAdminShortcut = Convert.ToInt32(_config.ShortcutOpenRdpConnectionNoCredAdmin);
            if (openRdpNoCredAdminShortcut - _AltValue > 0) { openRdpNoCredAdminShortcut -= _AltValue; chkOpenRdpNoCredAdminAlt.Checked = true; }
            if (openRdpNoCredAdminShortcut - _CtrlValue > 0) { openRdpNoCredAdminShortcut -= _CtrlValue; chkOpenRdpNoCredAdminCtrl.Checked = true; }
            if (openRdpNoCredAdminShortcut - _ShiftValue > 0) { openRdpNoCredAdminShortcut -= _ShiftValue; chkOpenRdpNoCredAdminShift.Checked = true; }
            txtOpenRdpNoCredAdminKey.Text = ((Keys)openRdpNoCredAdminShortcut).ToString();



            // if previously saved width or height now exceed maximum (primary screen resolution) reset to max
            if (mstscWidth > numMstscWidth.Maximum)
            {
                numMstscWidth.Value = numMstscWidth.Maximum;
                _config.MstscWidth = Convert.ToUInt64(numMstscWidth.Value);
            }
            else { numMstscWidth.Value = mstscWidth; }

            if (mstscHeight > numMstscHeight.Maximum)
            {
                numMstscHeight.Value = numMstscHeight.Maximum;
                _config.MstscHeight = Convert.ToUInt64(numMstscHeight.Value);
            }
            else { numMstscHeight.Value = mstscHeight; }

            if (credPickWidth > numCredPickWidth.Maximum)
            {
                numCredPickWidth.Value = numCredPickWidth.Maximum;
                _config.CredPickerWidth = Convert.ToUInt64(numCredPickWidth.Value);
            }
            else { numCredPickWidth.Value = credPickWidth; }
            if (credPickHeight > numCredPickHeight.Maximum)
            {
                numCredPickHeight.Value = numCredPickHeight.Maximum;
                _config.CredPickerHeight = Convert.ToUInt64(numCredPickHeight.Value);
            }
            else { numCredPickHeight.Value = credPickHeight; }

            if (credVaultTtl > numCredVaultTtl.Maximum)
            {
                numCredVaultTtl.Value = numCredVaultTtl.Maximum;
                _config.CredVaultTtl = Convert.ToUInt64(numCredVaultTtl.Value);
            }
            else { numCredVaultTtl.Value = credVaultTtl; }

            checkMstscSizeEnable(null, EventArgs.Empty);
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            // save configuration
            _config.KeePassShowResolvedReferences = chkKeepassShowResolvedReferences.Checked;
            _config.CredPickerRememberSize = chkCredPickRememberSize.Checked;
            _config.CredPickerWidth = Convert.ToUInt64(numCredPickWidth.Value);
            _config.CredPickerHeight = Convert.ToUInt64(numCredPickHeight.Value);
            _config.CredVaultUseWindows = chkCredVaultUseWindows.Checked;
            _config.CredVaultTtl = Convert.ToUInt64(numCredVaultTtl.Value);
            _config.MstscUseFullscreen = chkMstscUseFullscreen.Checked;
            _config.MstscUseAdmin = chkMstscUseAdmin.Checked;
            _config.MstscUseSpan = chkMstscUseSpan.Checked;
            _config.MstscUseMultimon = chkMstscUseMultimon.Checked;
            _config.MstscWidth = Convert.ToUInt64(numMstscWidth.Value);
            _config.MstscHeight = Convert.ToUInt64(numMstscHeight.Value);

            var regExPre = new List<string>();
            foreach (var item in lstRegExPre.Items) { regExPre.Add(item.ToString()); }
            var regExPost = new List<string>();
            foreach (var item in lstRegExPost.Items) { regExPost.Add(item.ToString()); }

            _config.CredPickerRegExPre = string.Join("|", regExPre);
            _config.CredPickerRegExPost = string.Join("|", regExPost);

            var openRdpShortcut = Util.ConvertStringToKeys(txtOpenRdpKey.Text);
            if (chkOpenRdpAlt.Checked) { openRdpShortcut += _AltValue; }
            if (chkOpenRdpCtrl.Checked) { openRdpShortcut += _CtrlValue; }
            if (chkOpenRdpShift.Checked) { openRdpShortcut += _ShiftValue; }
            _config.ShortcutOpenRdpConnection = Convert.ToUInt64(openRdpShortcut);

            var openRdpAdminShortcut = Util.ConvertStringToKeys(txtOpenRdpAdminKey.Text);
            if (chkOpenRdpAdminAlt.Checked) { openRdpAdminShortcut += _AltValue; }
            if (chkOpenRdpAdminCtrl.Checked) { openRdpAdminShortcut += _CtrlValue; }
            if (chkOpenRdpAdminShift.Checked) { openRdpAdminShortcut += _ShiftValue; }
            _config.ShortcutOpenRdpConnectionAdmin = Convert.ToUInt64(openRdpAdminShortcut);

            var openRdpNoCredShortcut = Util.ConvertStringToKeys(txtOpenRdpNoCredKey.Text);
            if (chkOpenRdpNoCredAlt.Checked) { openRdpNoCredShortcut += _AltValue; }
            if (chkOpenRdpNoCredCtrl.Checked) { openRdpNoCredShortcut += _CtrlValue; }
            if (chkOpenRdpNoCredShift.Checked) { openRdpNoCredShortcut += _ShiftValue; }
            _config.ShortcutOpenRdpConnectionNoCred = Convert.ToUInt64(openRdpNoCredShortcut);

            var openRdpNoCredAdminShortcut = Util.ConvertStringToKeys(txtOpenRdpNoCredAdminKey.Text);
            if (chkOpenRdpNoCredAdminAlt.Checked) { openRdpNoCredAdminShortcut += _AltValue; }
            if (chkOpenRdpNoCredAdminCtrl.Checked) { openRdpNoCredAdminShortcut += _CtrlValue; }
            if (chkOpenRdpNoCredAdminShift.Checked) { openRdpNoCredAdminShortcut += _ShiftValue; }
            _config.ShortcutOpenRdpConnectionNoCredAdmin = Convert.ToUInt64(openRdpNoCredAdminShortcut);
        }

        private void cmdRegExPreAdd_Click(object sender, EventArgs e)
        {
            lstRegExPre.Items.Add(txtRegExPre.Text);
            txtRegExPre.Text = string.Empty;
        }
        private void cmdRegExPostAdd_Click(object sender, EventArgs e)
        {
            lstRegExPost.Items.Add(txtRegExPost.Text);
            txtRegExPost.Text = string.Empty;
        }

        private void cmdRegExPreRemove_Click(object sender, EventArgs e) { for (int i = lstRegExPre.SelectedItems.Count - 1; i >= 0; i--) { lstRegExPre.Items.Remove(lstRegExPre.SelectedItems[i]); } }
        private void cmdRegExPostRemove_Click(object sender, EventArgs e) { for (int i = lstRegExPost.SelectedItems.Count - 1; i >= 0; i--) { lstRegExPost.Items.Remove(lstRegExPost.SelectedItems[i]); } }

        private void lstRegExPre_SelectedIndexChanged(object sender, EventArgs e) { cmdRegExPreRemove.Enabled = lstRegExPre.SelectedItems.Count > 0; }
        private void lstRegExPost_SelectedIndexChanged(object sender, EventArgs e) { cmdRegExPostRemove.Enabled = lstRegExPost.SelectedItems.Count > 0; }

        private void cmdRegExPreReset_Click(object sender, EventArgs e)
        {
            lstRegExPre.Items.Clear();
            lstRegExPre.Items.AddRange(Util.DefaultCredPickRegExPre.Split('|'));
        }
        private void cmdRegExPostReset_Click(object sender, EventArgs e)
        {
            lstRegExPost.Items.Clear();
            lstRegExPost.Items.AddRange(Util.DefaultCredPickRegExPost.Split('|'));
        }

    }
}
