using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeePassRDP
{
    public partial class KPROptionsForm : Form
    {
        private readonly KprConfig _config;

        public KPROptionsForm(KprConfig config)
        {
            _config = config;
            InitializeComponent();
        }

        private void checkMstscSizeEnable(object sender, EventArgs e)
        {
            bool enable = true;
            if(chkMstscUseFullscreen.Checked || chkMstscUseMultimon.Checked || chkMstscUseSpan.Checked)
            {
                enable = false;
            }

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
            chkCredVaultUseWindows.Checked = _config.CredVaultUseWindows;
            chkMstscUseFullscreen.Checked = _config.MstscUseFullscreen;
            chkMstscUseAdmin.Checked = _config.MstscUseAdmin;
            chkMstscUseSpan.Checked = _config.MstscUseSpan;
            chkMstscUseMultimon.Checked = _config.MstscUseMultimon;
            decimal mstscWidth = Convert.ToDecimal(_config.MstscWidth);
            decimal mstscHeight = Convert.ToDecimal(_config.MstscHeight);
            decimal credPickWidth = Convert.ToDecimal(_config.CredPickerWidth);
            decimal credPickHeight = Convert.ToDecimal(_config.CredPickerHeight);

            // if previously saved width or height now exceed maximum (primary screen resolution) reset to max
            if (mstscWidth > numMstscWidth.Maximum)
            {
                numMstscWidth.Value = numMstscWidth.Maximum;
                _config.MstscWidth = Convert.ToUInt64(numMstscWidth.Value);
            } else
            {
                numMstscWidth.Value = mstscWidth;
            }

            if (mstscHeight > numMstscHeight.Maximum)
            {
                numMstscHeight.Value = numMstscHeight.Maximum;
                _config.MstscHeight = Convert.ToUInt64(numMstscHeight.Value);
            } else
            {
                numMstscHeight.Value = mstscHeight;
            }

            if (credPickWidth > numCredPickWidth.Maximum)
            {
                numCredPickWidth.Value = numCredPickWidth.Maximum;
                _config.CredPickerWidth = Convert.ToUInt64(numCredPickWidth.Value);
            } else
            {
                numCredPickWidth.Value = credPickWidth;
            }
            if (credPickHeight > numCredPickHeight.Maximum)
            {
                numCredPickHeight.Value = numCredPickHeight.Maximum;
                _config.CredPickerHeight = Convert.ToUInt64(numCredPickHeight.Value);
            } else
            {
                numCredPickHeight.Value = credPickHeight;
            }

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
            _config.MstscUseFullscreen = chkMstscUseFullscreen.Checked;
            _config.MstscUseAdmin = chkMstscUseAdmin.Checked;
            _config.MstscUseSpan = chkMstscUseSpan.Checked;
            _config.MstscUseMultimon = chkMstscUseMultimon.Checked;
            _config.MstscWidth = Convert.ToUInt64(numMstscWidth.Value);
            _config.MstscHeight = Convert.ToUInt64(numMstscHeight.Value);
        }
    }
}
