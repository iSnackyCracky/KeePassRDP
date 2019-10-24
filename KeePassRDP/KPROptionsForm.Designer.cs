namespace KeePassRDP
{
    partial class KPROptionsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOk = new System.Windows.Forms.Button();
            this.grpMstscSettings = new System.Windows.Forms.GroupBox();
            this.chkMstscFullscreen = new System.Windows.Forms.CheckBox();
            this.grpMstscSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(471, 367);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(112, 35);
            this.cmdCancel.TabIndex = 4;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // cmdOk
            // 
            this.cmdOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.Location = new System.Drawing.Point(350, 367);
            this.cmdOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(112, 35);
            this.cmdOk.TabIndex = 3;
            this.cmdOk.Text = "&Ok";
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // grpMstscSettings
            // 
            this.grpMstscSettings.Controls.Add(this.chkMstscFullscreen);
            this.grpMstscSettings.Location = new System.Drawing.Point(12, 12);
            this.grpMstscSettings.Name = "grpMstscSettings";
            this.grpMstscSettings.Size = new System.Drawing.Size(220, 293);
            this.grpMstscSettings.TabIndex = 5;
            this.grpMstscSettings.TabStop = false;
            this.grpMstscSettings.Text = "mstsc.exe parameters";
            // 
            // chkMstscFullscreen
            // 
            this.chkMstscFullscreen.AutoSize = true;
            this.chkMstscFullscreen.Location = new System.Drawing.Point(6, 25);
            this.chkMstscFullscreen.Name = "chkMstscFullscreen";
            this.chkMstscFullscreen.Size = new System.Drawing.Size(159, 24);
            this.chkMstscFullscreen.TabIndex = 0;
            this.chkMstscFullscreen.Text = "Use fullscreen (/f)";
            this.chkMstscFullscreen.UseVisualStyleBackColor = true;
            // 
            // KPROptionsForm
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(596, 416);
            this.Controls.Add(this.grpMstscSettings);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "KPROptionsForm";
            this.Text = "KPROptionsForm";
            this.grpMstscSettings.ResumeLayout(false);
            this.grpMstscSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.GroupBox grpMstscSettings;
        private System.Windows.Forms.CheckBox chkMstscFullscreen;
    }
}