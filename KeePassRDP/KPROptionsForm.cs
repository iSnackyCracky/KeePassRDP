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

        public bool mstscUseFullscreen
        {
            get { return chkMstscUseFullscreen.Checked; }
            set { chkMstscUseFullscreen.Checked = value; }
        }

        public bool mstscUseAdmin
        {
            get { return chkMstscUseAdmin.Checked; }
            set { chkMstscUseAdmin.Checked = value; }
        }

        public bool mstscUseSpan
        {
            get { return chkMstscUseSpan.Checked; }
            set { chkMstscUseSpan.Checked = value; }
        }

        public bool mstscUseMultimon
        {
            get { return chkMstscUseMultimon.Checked; }
            set { chkMstscUseMultimon.Checked = value; }
        }

        public KPROptionsForm()
        {
            InitializeComponent();
        }

        private void checkSizeEnable(object sender, EventArgs e)
        {
            bool enable = true;
            if(chkMstscUseFullscreen.Checked || chkMstscUseMultimon.Checked || chkMstscUseSpan.Checked)
            {
                enable = false;
            }

            lblWidth.Enabled = enable;
            numWidth.Enabled = enable;
            lblHeight.Enabled = enable;
            numHeight.Enabled = enable;
        }

        private void KPROptionsForm_Load(object sender, EventArgs e)
        {
            numWidth.Maximum = Screen.PrimaryScreen.Bounds.Width;
            numHeight.Maximum = Screen.PrimaryScreen.Bounds.Height;
        }
    }
}
