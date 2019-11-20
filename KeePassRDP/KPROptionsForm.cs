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

            // set form elements to match previously saved options
            chkKeepassShowResolvedReferences.Checked = _config.KeePassShowResolvedReferences;
            chkMstscUseFullscreen.Checked = _config.MstscUseFullscreen;
            chkMstscUseAdmin.Checked = _config.MstscUseAdmin;
            chkMstscUseSpan.Checked = _config.MstscUseSpan;
            chkMstscUseMultimon.Checked = _config.MstscUseMultimon;
            decimal mstscWidth = Convert.ToDecimal(_config.MstscWidth);
            decimal mstscHeight = Convert.ToDecimal(_config.MstscHeight);

            // if previously saved width or height now exceed maximum (primary screen resolution) reset to max
            if (mstscWidth > numMstscWidth.Maximum)
            {
                numMstscWidth.Value = numMstscWidth.Maximum;
                _config.MstscWidth = Convert.ToUInt64(numMstscWidth.Maximum);
            } else
            {
                numMstscWidth.Value = mstscWidth;
            }

            if (mstscHeight > numMstscHeight.Maximum)
            {
                numMstscHeight.Value = numMstscHeight.Maximum;
                _config.MstscHeight = Convert.ToUInt64(numMstscHeight.Maximum);
            } else
            {
                numMstscHeight.Value = mstscHeight;
            }

            checkMstscSizeEnable(null, EventArgs.Empty);
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            // save configuration
            _config.KeePassShowResolvedReferences = chkKeepassShowResolvedReferences.Checked;
            _config.MstscUseFullscreen = chkMstscUseFullscreen.Checked;
            _config.MstscUseAdmin = chkMstscUseAdmin.Checked;
            _config.MstscUseSpan = chkMstscUseSpan.Checked;
            _config.MstscUseMultimon = chkMstscUseMultimon.Checked;
            _config.MstscWidth = Convert.ToUInt64(numMstscWidth.Value);
            _config.MstscHeight = Convert.ToUInt64(numMstscHeight.Value);
        }
    }
}
