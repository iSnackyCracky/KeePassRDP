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
            get { return chkMstscFullscreen.Checked; }
            set { chkMstscFullscreen.Checked = value; }
        }

        public KPROptionsForm()
        {
            InitializeComponent();
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {

        }
    }
}
