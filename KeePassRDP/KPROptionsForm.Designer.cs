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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KPROptionsForm));
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOk = new System.Windows.Forms.Button();
            this.grpMstscParams = new System.Windows.Forms.GroupBox();
            this.lblHeight = new System.Windows.Forms.Label();
            this.numMstscHeight = new System.Windows.Forms.NumericUpDown();
            this.lblWidth = new System.Windows.Forms.Label();
            this.numMstscWidth = new System.Windows.Forms.NumericUpDown();
            this.chkMstscUseMultimon = new System.Windows.Forms.CheckBox();
            this.chkMstscUseSpan = new System.Windows.Forms.CheckBox();
            this.chkMstscUseAdmin = new System.Windows.Forms.CheckBox();
            this.chkMstscUseFullscreen = new System.Windows.Forms.CheckBox();
            this.ttMstscParams = new System.Windows.Forms.ToolTip(this.components);
            this.grpGeneral = new System.Windows.Forms.GroupBox();
            this.chkCredPickRememberSize = new System.Windows.Forms.CheckBox();
            this.txtCredPickHeight = new System.Windows.Forms.Label();
            this.chkKeepassShowResolvedReferences = new System.Windows.Forms.CheckBox();
            this.numCredPickHeight = new System.Windows.Forms.NumericUpDown();
            this.txtCredPickWidth = new System.Windows.Forms.Label();
            this.numCredPickWidth = new System.Windows.Forms.NumericUpDown();
            this.ttGeneralOptions = new System.Windows.Forms.ToolTip(this.components);
            this.grpCredentials = new System.Windows.Forms.GroupBox();
            this.chkCredVaultUseWindows = new System.Windows.Forms.CheckBox();
            this.ttCredentialOptions = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.numCredVaultTtl = new System.Windows.Forms.NumericUpDown();
            this.grpMstscParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMstscHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMstscWidth)).BeginInit();
            this.grpGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCredPickHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredPickWidth)).BeginInit();
            this.grpCredentials.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCredVaultTtl)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(492, 285);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(119, 35);
            this.cmdCancel.TabIndex = 4;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // cmdOk
            // 
            this.cmdOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.Location = new System.Drawing.Point(366, 285);
            this.cmdOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(119, 35);
            this.cmdOk.TabIndex = 3;
            this.cmdOk.Text = "&Ok";
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // grpMstscParams
            // 
            this.grpMstscParams.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.grpMstscParams.Controls.Add(this.lblHeight);
            this.grpMstscParams.Controls.Add(this.numMstscHeight);
            this.grpMstscParams.Controls.Add(this.lblWidth);
            this.grpMstscParams.Controls.Add(this.numMstscWidth);
            this.grpMstscParams.Controls.Add(this.chkMstscUseMultimon);
            this.grpMstscParams.Controls.Add(this.chkMstscUseSpan);
            this.grpMstscParams.Controls.Add(this.chkMstscUseAdmin);
            this.grpMstscParams.Controls.Add(this.chkMstscUseFullscreen);
            this.grpMstscParams.Location = new System.Drawing.Point(318, 12);
            this.grpMstscParams.Name = "grpMstscParams";
            this.grpMstscParams.Size = new System.Drawing.Size(296, 220);
            this.grpMstscParams.TabIndex = 1;
            this.grpMstscParams.TabStop = false;
            this.grpMstscParams.Text = "mstsc.exe parameters";
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(6, 179);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(86, 20);
            this.lblHeight.TabIndex = 6;
            this.lblHeight.Text = "Height - /h:";
            this.ttMstscParams.SetToolTip(this.lblHeight, "/h:<height>\r\nSpecifies the height of the Remote Desktop window.\r\n0 = not set\r\n");
            // 
            // numMstscHeight
            // 
            this.numMstscHeight.Location = new System.Drawing.Point(98, 177);
            this.numMstscHeight.Name = "numMstscHeight";
            this.numMstscHeight.Size = new System.Drawing.Size(192, 26);
            this.numMstscHeight.TabIndex = 7;
            this.ttMstscParams.SetToolTip(this.numMstscHeight, "/h:<height>\r\nSpecifies the height of the Remote Desktop window.\r\n0 = not set");
            // 
            // lblWidth
            // 
            this.lblWidth.AutoSize = true;
            this.lblWidth.Location = new System.Drawing.Point(6, 147);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(82, 20);
            this.lblWidth.TabIndex = 4;
            this.lblWidth.Text = "Width - /w:";
            this.ttMstscParams.SetToolTip(this.lblWidth, "/w:<width>\r\nSpecifies the width of the Remote Desktop window.\r\n0 = not set\r\n");
            // 
            // numMstscWidth
            // 
            this.numMstscWidth.Location = new System.Drawing.Point(98, 145);
            this.numMstscWidth.Name = "numMstscWidth";
            this.numMstscWidth.Size = new System.Drawing.Size(192, 26);
            this.numMstscWidth.TabIndex = 5;
            this.ttMstscParams.SetToolTip(this.numMstscWidth, "/w:<width>\r\nSpecifies the width of the Remote Desktop window.\r\n0 = not set");
            // 
            // chkMstscUseMultimon
            // 
            this.chkMstscUseMultimon.AutoSize = true;
            this.chkMstscUseMultimon.Location = new System.Drawing.Point(6, 115);
            this.chkMstscUseMultimon.Name = "chkMstscUseMultimon";
            this.chkMstscUseMultimon.Size = new System.Drawing.Size(229, 24);
            this.chkMstscUseMultimon.TabIndex = 3;
            this.chkMstscUseMultimon.Text = "Use all &monitors - /multimon";
            this.ttMstscParams.SetToolTip(this.chkMstscUseMultimon, "mstsc.exe /multimon\r\nConfigures the Remote Desktop Services session monitor layou" +
        "t\r\nto be identical to the current client-side configuration.");
            this.chkMstscUseMultimon.UseVisualStyleBackColor = true;
            this.chkMstscUseMultimon.CheckedChanged += new System.EventHandler(this.checkMstscSizeEnable);
            // 
            // chkMstscUseSpan
            // 
            this.chkMstscUseSpan.AutoSize = true;
            this.chkMstscUseSpan.Location = new System.Drawing.Point(6, 85);
            this.chkMstscUseSpan.Name = "chkMstscUseSpan";
            this.chkMstscUseSpan.Size = new System.Drawing.Size(185, 24);
            this.chkMstscUseSpan.TabIndex = 2;
            this.chkMstscUseSpan.Text = "Use &spanning - /span";
            this.ttMstscParams.SetToolTip(this.chkMstscUseSpan, resources.GetString("chkMstscUseSpan.ToolTip"));
            this.chkMstscUseSpan.UseVisualStyleBackColor = true;
            this.chkMstscUseSpan.CheckedChanged += new System.EventHandler(this.checkMstscSizeEnable);
            // 
            // chkMstscUseAdmin
            // 
            this.chkMstscUseAdmin.AutoSize = true;
            this.chkMstscUseAdmin.Location = new System.Drawing.Point(6, 55);
            this.chkMstscUseAdmin.Name = "chkMstscUseAdmin";
            this.chkMstscUseAdmin.Size = new System.Drawing.Size(171, 24);
            this.chkMstscUseAdmin.TabIndex = 1;
            this.chkMstscUseAdmin.Text = "Use &admin - /admin";
            this.ttMstscParams.SetToolTip(this.chkMstscUseAdmin, "mstsc.exe /admin\r\nConnects you to the session for administering a remote PC.");
            this.chkMstscUseAdmin.UseVisualStyleBackColor = true;
            // 
            // chkMstscUseFullscreen
            // 
            this.chkMstscUseFullscreen.AutoSize = true;
            this.chkMstscUseFullscreen.Location = new System.Drawing.Point(6, 25);
            this.chkMstscUseFullscreen.Name = "chkMstscUseFullscreen";
            this.chkMstscUseFullscreen.Size = new System.Drawing.Size(158, 24);
            this.chkMstscUseFullscreen.TabIndex = 0;
            this.chkMstscUseFullscreen.Text = "Use &fullscreen - /f";
            this.ttMstscParams.SetToolTip(this.chkMstscUseFullscreen, "mstsc.exe /f\r\nStarts Remote Desktop Connection in full-screen mode.");
            this.chkMstscUseFullscreen.UseVisualStyleBackColor = true;
            this.chkMstscUseFullscreen.CheckedChanged += new System.EventHandler(this.checkMstscSizeEnable);
            // 
            // ttMstscParams
            // 
            this.ttMstscParams.AutoPopDelay = 10000;
            this.ttMstscParams.InitialDelay = 500;
            this.ttMstscParams.ReshowDelay = 100;
            this.ttMstscParams.ToolTipTitle = "mstsc.exe parameter";
            // 
            // grpGeneral
            // 
            this.grpGeneral.Controls.Add(this.chkCredPickRememberSize);
            this.grpGeneral.Controls.Add(this.txtCredPickHeight);
            this.grpGeneral.Controls.Add(this.chkKeepassShowResolvedReferences);
            this.grpGeneral.Controls.Add(this.numCredPickHeight);
            this.grpGeneral.Controls.Add(this.txtCredPickWidth);
            this.grpGeneral.Controls.Add(this.numCredPickWidth);
            this.grpGeneral.Location = new System.Drawing.Point(12, 12);
            this.grpGeneral.Name = "grpGeneral";
            this.grpGeneral.Size = new System.Drawing.Size(296, 152);
            this.grpGeneral.TabIndex = 0;
            this.grpGeneral.TabStop = false;
            this.grpGeneral.Text = "General options";
            // 
            // chkCredPickRememberSize
            // 
            this.chkCredPickRememberSize.AutoSize = true;
            this.chkCredPickRememberSize.Location = new System.Drawing.Point(6, 55);
            this.chkCredPickRememberSize.Name = "chkCredPickRememberSize";
            this.chkCredPickRememberSize.Size = new System.Drawing.Size(227, 24);
            this.chkCredPickRememberSize.TabIndex = 1;
            this.chkCredPickRememberSize.Text = "Remember CredPicker size";
            this.ttGeneralOptions.SetToolTip(this.chkCredPickRememberSize, "Always remembers the last CredPicker window size.\r\nIf you can alwas set the size " +
        "using the values below.");
            this.chkCredPickRememberSize.UseVisualStyleBackColor = true;
            // 
            // txtCredPickHeight
            // 
            this.txtCredPickHeight.AutoSize = true;
            this.txtCredPickHeight.Location = new System.Drawing.Point(6, 118);
            this.txtCredPickHeight.Name = "txtCredPickHeight";
            this.txtCredPickHeight.Size = new System.Drawing.Size(138, 20);
            this.txtCredPickHeight.TabIndex = 4;
            this.txtCredPickHeight.Text = "CredPicker height:";
            // 
            // chkKeepassShowResolvedReferences
            // 
            this.chkKeepassShowResolvedReferences.AutoSize = true;
            this.chkKeepassShowResolvedReferences.Location = new System.Drawing.Point(6, 25);
            this.chkKeepassShowResolvedReferences.Name = "chkKeepassShowResolvedReferences";
            this.chkKeepassShowResolvedReferences.Size = new System.Drawing.Size(225, 24);
            this.chkKeepassShowResolvedReferences.TabIndex = 0;
            this.chkKeepassShowResolvedReferences.Text = "Show resolved References";
            this.ttGeneralOptions.SetToolTip(this.chkKeepassShowResolvedReferences, "Enable or disable visibility of resolved References.\r\nThis only affects the CredP" +
        "icker.\r\nReferences get always resolved for the RDP connection data.");
            this.chkKeepassShowResolvedReferences.UseVisualStyleBackColor = true;
            // 
            // numCredPickHeight
            // 
            this.numCredPickHeight.Location = new System.Drawing.Point(143, 116);
            this.numCredPickHeight.Name = "numCredPickHeight";
            this.numCredPickHeight.Size = new System.Drawing.Size(137, 26);
            this.numCredPickHeight.TabIndex = 5;
            // 
            // txtCredPickWidth
            // 
            this.txtCredPickWidth.AutoSize = true;
            this.txtCredPickWidth.Location = new System.Drawing.Point(6, 86);
            this.txtCredPickWidth.Name = "txtCredPickWidth";
            this.txtCredPickWidth.Size = new System.Drawing.Size(131, 20);
            this.txtCredPickWidth.TabIndex = 2;
            this.txtCredPickWidth.Text = "CredPicker width:";
            // 
            // numCredPickWidth
            // 
            this.numCredPickWidth.Location = new System.Drawing.Point(143, 84);
            this.numCredPickWidth.Name = "numCredPickWidth";
            this.numCredPickWidth.Size = new System.Drawing.Size(137, 26);
            this.numCredPickWidth.TabIndex = 3;
            // 
            // ttGeneralOptions
            // 
            this.ttGeneralOptions.AutoPopDelay = 10000;
            this.ttGeneralOptions.InitialDelay = 500;
            this.ttGeneralOptions.ReshowDelay = 100;
            this.ttGeneralOptions.ToolTipTitle = "General options";
            // 
            // grpCredentials
            // 
            this.grpCredentials.Controls.Add(this.label1);
            this.grpCredentials.Controls.Add(this.numCredVaultTtl);
            this.grpCredentials.Controls.Add(this.chkCredVaultUseWindows);
            this.grpCredentials.Location = new System.Drawing.Point(12, 169);
            this.grpCredentials.Name = "grpCredentials";
            this.grpCredentials.Size = new System.Drawing.Size(296, 96);
            this.grpCredentials.TabIndex = 2;
            this.grpCredentials.TabStop = false;
            this.grpCredentials.Text = "Credential options";
            // 
            // chkCredVaultUseWindows
            // 
            this.chkCredVaultUseWindows.AutoSize = true;
            this.chkCredVaultUseWindows.Location = new System.Drawing.Point(6, 25);
            this.chkCredVaultUseWindows.Name = "chkCredVaultUseWindows";
            this.chkCredVaultUseWindows.Size = new System.Drawing.Size(244, 24);
            this.chkCredVaultUseWindows.TabIndex = 0;
            this.chkCredVaultUseWindows.Text = "Store as Windows credentials";
            this.ttCredentialOptions.SetToolTip(this.chkCredVaultUseWindows, resources.GetString("chkCredVaultUseWindows.ToolTip"));
            this.chkCredVaultUseWindows.UseVisualStyleBackColor = true;
            // 
            // ttCredentialOptions
            // 
            this.ttCredentialOptions.AutoPopDelay = 10000;
            this.ttCredentialOptions.InitialDelay = 500;
            this.ttCredentialOptions.ReshowDelay = 100;
            this.ttCredentialOptions.ToolTipTitle = "Credential options";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Credential TTL:";
            this.ttCredentialOptions.SetToolTip(this.label1, "Specify duration the credentials reside in the Windows Vault (in seconds).");
            // 
            // numCredVaultTtl
            // 
            this.numCredVaultTtl.Location = new System.Drawing.Point(143, 55);
            this.numCredVaultTtl.Maximum = new decimal(new int[] {
            86400,
            0,
            0,
            0});
            this.numCredVaultTtl.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCredVaultTtl.Name = "numCredVaultTtl";
            this.numCredVaultTtl.Size = new System.Drawing.Size(137, 26);
            this.numCredVaultTtl.TabIndex = 2;
            this.ttCredentialOptions.SetToolTip(this.numCredVaultTtl, "Specify duration the credentials reside in the Windows Vault (in seconds).");
            this.numCredVaultTtl.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // KPROptionsForm
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(626, 333);
            this.Controls.Add(this.grpCredentials);
            this.Controls.Add(this.grpGeneral);
            this.Controls.Add(this.grpMstscParams);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "KPROptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "KeePassRDP Options";
            this.Load += new System.EventHandler(this.KPROptionsForm_Load);
            this.grpMstscParams.ResumeLayout(false);
            this.grpMstscParams.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMstscHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMstscWidth)).EndInit();
            this.grpGeneral.ResumeLayout(false);
            this.grpGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCredPickHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredPickWidth)).EndInit();
            this.grpCredentials.ResumeLayout(false);
            this.grpCredentials.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCredVaultTtl)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.GroupBox grpMstscParams;
        private System.Windows.Forms.CheckBox chkMstscUseFullscreen;
        private System.Windows.Forms.CheckBox chkMstscUseMultimon;
        private System.Windows.Forms.CheckBox chkMstscUseSpan;
        private System.Windows.Forms.CheckBox chkMstscUseAdmin;
        private System.Windows.Forms.ToolTip ttMstscParams;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.NumericUpDown numMstscHeight;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.NumericUpDown numMstscWidth;
        private System.Windows.Forms.GroupBox grpGeneral;
        private System.Windows.Forms.CheckBox chkKeepassShowResolvedReferences;
        private System.Windows.Forms.ToolTip ttGeneralOptions;
        private System.Windows.Forms.GroupBox grpCredentials;
        private System.Windows.Forms.CheckBox chkCredVaultUseWindows;
        private System.Windows.Forms.ToolTip ttCredentialOptions;
        private System.Windows.Forms.Label txtCredPickHeight;
        private System.Windows.Forms.NumericUpDown numCredPickHeight;
        private System.Windows.Forms.Label txtCredPickWidth;
        private System.Windows.Forms.NumericUpDown numCredPickWidth;
        private System.Windows.Forms.CheckBox chkCredPickRememberSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numCredVaultTtl;
    }
}