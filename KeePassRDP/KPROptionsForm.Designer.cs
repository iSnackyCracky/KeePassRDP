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
            this.lblHeight = new System.Windows.Forms.Label();
            this.numMstscHeight = new System.Windows.Forms.NumericUpDown();
            this.lblWidth = new System.Windows.Forms.Label();
            this.numMstscWidth = new System.Windows.Forms.NumericUpDown();
            this.chkMstscUseMultimon = new System.Windows.Forms.CheckBox();
            this.chkMstscUseSpan = new System.Windows.Forms.CheckBox();
            this.chkMstscUseAdmin = new System.Windows.Forms.CheckBox();
            this.chkMstscUseFullscreen = new System.Windows.Forms.CheckBox();
            this.ttMstscParams = new System.Windows.Forms.ToolTip(this.components);
            this.chkCredPickRememberSize = new System.Windows.Forms.CheckBox();
            this.txtCredPickHeight = new System.Windows.Forms.Label();
            this.numCredPickHeight = new System.Windows.Forms.NumericUpDown();
            this.txtCredPickWidth = new System.Windows.Forms.Label();
            this.numCredPickWidth = new System.Windows.Forms.NumericUpDown();
            this.chkKeepassShowResolvedReferences = new System.Windows.Forms.CheckBox();
            this.ttGeneralOptions = new System.Windows.Forms.ToolTip(this.components);
            this.lblCredVaultTtl = new System.Windows.Forms.Label();
            this.numCredVaultTtl = new System.Windows.Forms.NumericUpDown();
            this.chkCredVaultUseWindows = new System.Windows.Forms.CheckBox();
            this.ttCredentialOptions = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblOpenRdpNoCredAdminShortcut = new System.Windows.Forms.Label();
            this.chkOpenRdpNoCredAdminCtrl = new System.Windows.Forms.CheckBox();
            this.chkOpenRdpNoCredAdminAlt = new System.Windows.Forms.CheckBox();
            this.chkOpenRdpNoCredAdminShift = new System.Windows.Forms.CheckBox();
            this.txtOpenRdpNoCredAdminKey = new System.Windows.Forms.TextBox();
            this.txtOpenRdpNoCredKey = new System.Windows.Forms.TextBox();
            this.txtOpenRdpAdminKey = new System.Windows.Forms.TextBox();
            this.txtOpenRdpKey = new System.Windows.Forms.TextBox();
            this.chkOpenRdpNoCredShift = new System.Windows.Forms.CheckBox();
            this.chkOpenRdpNoCredAlt = new System.Windows.Forms.CheckBox();
            this.chkOpenRdpNoCredCtrl = new System.Windows.Forms.CheckBox();
            this.lblOpenRdpNoCredShortcut = new System.Windows.Forms.Label();
            this.lblOpenRdpAdminShortcut = new System.Windows.Forms.Label();
            this.chkOpenRdpAdminCtrl = new System.Windows.Forms.CheckBox();
            this.chkOpenRdpAdminAlt = new System.Windows.Forms.CheckBox();
            this.chkOpenRdpAdminShift = new System.Windows.Forms.CheckBox();
            this.chkOpenRdpCtrl = new System.Windows.Forms.CheckBox();
            this.chkOpenRdpAlt = new System.Windows.Forms.CheckBox();
            this.chkOpenRdpShift = new System.Windows.Forms.CheckBox();
            this.lblOpenRdpShortcut = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.grpCredentialOptions = new System.Windows.Forms.GroupBox();
            this.tabCredPicker = new System.Windows.Forms.TabPage();
            this.cmdRegExPostReset = new System.Windows.Forms.Button();
            this.cmdRegExPreReset = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmdRegExPostRemove = new System.Windows.Forms.Button();
            this.cmdRegExPostAdd = new System.Windows.Forms.Button();
            this.txtRegExPost = new System.Windows.Forms.TextBox();
            this.lstRegExPost = new System.Windows.Forms.ListBox();
            this.cmdRegExPreRemove = new System.Windows.Forms.Button();
            this.cmdRegExPreAdd = new System.Windows.Forms.Button();
            this.txtRegExPre = new System.Windows.Forms.TextBox();
            this.lstRegExPre = new System.Windows.Forms.ListBox();
            this.tabMstsc = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.numMstscHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMstscWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredPickHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredPickWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredVaultTtl)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.grpCredentialOptions.SuspendLayout();
            this.tabCredPicker.SuspendLayout();
            this.tabMstsc.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(400, 435);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(118, 35);
            this.cmdCancel.TabIndex = 2;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // cmdOk
            // 
            this.cmdOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.Location = new System.Drawing.Point(274, 435);
            this.cmdOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(118, 35);
            this.cmdOk.TabIndex = 1;
            this.cmdOk.Text = "&Ok";
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(8, 177);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(86, 20);
            this.lblHeight.TabIndex = 6;
            this.lblHeight.Text = "Height - /h:";
            this.ttMstscParams.SetToolTip(this.lblHeight, "/h:<height>\r\nSpecifies the height of the Remote Desktop window.\r\n0 = not set\r\n");
            // 
            // numMstscHeight
            // 
            this.numMstscHeight.Location = new System.Drawing.Point(105, 174);
            this.numMstscHeight.Name = "numMstscHeight";
            this.numMstscHeight.Size = new System.Drawing.Size(192, 26);
            this.numMstscHeight.TabIndex = 7;
            this.ttMstscParams.SetToolTip(this.numMstscHeight, "/h:<height>\r\nSpecifies the height of the Remote Desktop window.\r\n0 = not set");
            // 
            // lblWidth
            // 
            this.lblWidth.AutoSize = true;
            this.lblWidth.Location = new System.Drawing.Point(8, 140);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(82, 20);
            this.lblWidth.TabIndex = 4;
            this.lblWidth.Text = "Width - /w:";
            this.ttMstscParams.SetToolTip(this.lblWidth, "/w:<width>\r\nSpecifies the width of the Remote Desktop window.\r\n0 = not set\r\n");
            // 
            // numMstscWidth
            // 
            this.numMstscWidth.Location = new System.Drawing.Point(105, 137);
            this.numMstscWidth.Name = "numMstscWidth";
            this.numMstscWidth.Size = new System.Drawing.Size(192, 26);
            this.numMstscWidth.TabIndex = 5;
            this.ttMstscParams.SetToolTip(this.numMstscWidth, "/w:<width>\r\nSpecifies the width of the Remote Desktop window.\r\n0 = not set");
            // 
            // chkMstscUseMultimon
            // 
            this.chkMstscUseMultimon.AutoSize = true;
            this.chkMstscUseMultimon.Location = new System.Drawing.Point(8, 105);
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
            this.chkMstscUseSpan.Location = new System.Drawing.Point(8, 72);
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
            this.chkMstscUseAdmin.Location = new System.Drawing.Point(8, 40);
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
            this.chkMstscUseFullscreen.Location = new System.Drawing.Point(8, 8);
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
            // chkCredPickRememberSize
            // 
            this.chkCredPickRememberSize.AutoSize = true;
            this.chkCredPickRememberSize.Location = new System.Drawing.Point(8, 40);
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
            this.txtCredPickHeight.Location = new System.Drawing.Point(8, 112);
            this.txtCredPickHeight.Name = "txtCredPickHeight";
            this.txtCredPickHeight.Size = new System.Drawing.Size(138, 20);
            this.txtCredPickHeight.TabIndex = 4;
            this.txtCredPickHeight.Text = "CredPicker height:";
            // 
            // numCredPickHeight
            // 
            this.numCredPickHeight.Location = new System.Drawing.Point(154, 109);
            this.numCredPickHeight.Name = "numCredPickHeight";
            this.numCredPickHeight.Size = new System.Drawing.Size(136, 26);
            this.numCredPickHeight.TabIndex = 5;
            // 
            // txtCredPickWidth
            // 
            this.txtCredPickWidth.AutoSize = true;
            this.txtCredPickWidth.Location = new System.Drawing.Point(8, 75);
            this.txtCredPickWidth.Name = "txtCredPickWidth";
            this.txtCredPickWidth.Size = new System.Drawing.Size(131, 20);
            this.txtCredPickWidth.TabIndex = 2;
            this.txtCredPickWidth.Text = "CredPicker width:";
            // 
            // numCredPickWidth
            // 
            this.numCredPickWidth.Location = new System.Drawing.Point(154, 72);
            this.numCredPickWidth.Name = "numCredPickWidth";
            this.numCredPickWidth.Size = new System.Drawing.Size(136, 26);
            this.numCredPickWidth.TabIndex = 3;
            // 
            // chkKeepassShowResolvedReferences
            // 
            this.chkKeepassShowResolvedReferences.AutoSize = true;
            this.chkKeepassShowResolvedReferences.Location = new System.Drawing.Point(8, 8);
            this.chkKeepassShowResolvedReferences.Name = "chkKeepassShowResolvedReferences";
            this.chkKeepassShowResolvedReferences.Size = new System.Drawing.Size(225, 24);
            this.chkKeepassShowResolvedReferences.TabIndex = 0;
            this.chkKeepassShowResolvedReferences.Text = "Show resolved References";
            this.ttGeneralOptions.SetToolTip(this.chkKeepassShowResolvedReferences, "Enable or disable visibility of resolved References.\r\nThis only affects the CredP" +
        "icker.\r\nReferences get always resolved for the RDP connection data.");
            this.chkKeepassShowResolvedReferences.UseVisualStyleBackColor = true;
            // 
            // ttGeneralOptions
            // 
            this.ttGeneralOptions.AutoPopDelay = 10000;
            this.ttGeneralOptions.InitialDelay = 500;
            this.ttGeneralOptions.ReshowDelay = 100;
            this.ttGeneralOptions.ToolTipTitle = "General options";
            // 
            // lblCredVaultTtl
            // 
            this.lblCredVaultTtl.AutoSize = true;
            this.lblCredVaultTtl.Location = new System.Drawing.Point(6, 60);
            this.lblCredVaultTtl.Name = "lblCredVaultTtl";
            this.lblCredVaultTtl.Size = new System.Drawing.Size(116, 20);
            this.lblCredVaultTtl.TabIndex = 1;
            this.lblCredVaultTtl.Text = "Credential TTL:";
            this.ttCredentialOptions.SetToolTip(this.lblCredVaultTtl, "Specify duration the credentials reside in the Windows Vault (in seconds).");
            // 
            // numCredVaultTtl
            // 
            this.numCredVaultTtl.Location = new System.Drawing.Point(132, 57);
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
            this.numCredVaultTtl.Size = new System.Drawing.Size(136, 26);
            this.numCredVaultTtl.TabIndex = 2;
            this.ttCredentialOptions.SetToolTip(this.numCredVaultTtl, "Specify duration the credentials reside in the Windows Vault (in seconds).");
            this.numCredVaultTtl.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
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
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabCredPicker);
            this.tabControl1.Controls.Add(this.tabMstsc);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(534, 426);
            this.tabControl1.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.groupBox1);
            this.tabGeneral.Controls.Add(this.grpCredentialOptions);
            this.tabGeneral.Location = new System.Drawing.Point(4, 29);
            this.tabGeneral.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabGeneral.Size = new System.Drawing.Size(526, 393);
            this.tabGeneral.TabIndex = 2;
            this.tabGeneral.Text = "General options";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(7, 114);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(512, 232);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Keyboard shortcuts";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 62.42171F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5261F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5261F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5261F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.lblOpenRdpNoCredAdminShortcut, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.chkOpenRdpNoCredAdminCtrl, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.chkOpenRdpNoCredAdminAlt, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.chkOpenRdpNoCredAdminShift, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.txtOpenRdpNoCredAdminKey, 4, 4);
            this.tableLayoutPanel1.Controls.Add(this.txtOpenRdpNoCredKey, 4, 3);
            this.tableLayoutPanel1.Controls.Add(this.txtOpenRdpAdminKey, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.txtOpenRdpKey, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkOpenRdpNoCredShift, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.chkOpenRdpNoCredAlt, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.chkOpenRdpNoCredCtrl, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblOpenRdpNoCredShortcut, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblOpenRdpAdminShortcut, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkOpenRdpAdminCtrl, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkOpenRdpAdminAlt, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkOpenRdpAdminShift, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkOpenRdpCtrl, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkOpenRdpAlt, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkOpenRdpShift, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblOpenRdpShortcut, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label9, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label10, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label11, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.label12, 4, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 22);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(506, 207);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // lblOpenRdpNoCredAdminShortcut
            // 
            this.lblOpenRdpNoCredAdminShortcut.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblOpenRdpNoCredAdminShortcut.AutoSize = true;
            this.lblOpenRdpNoCredAdminShortcut.Location = new System.Drawing.Point(3, 162);
            this.lblOpenRdpNoCredAdminShortcut.Name = "lblOpenRdpNoCredAdminShortcut";
            this.lblOpenRdpNoCredAdminShortcut.Size = new System.Drawing.Size(230, 40);
            this.lblOpenRdpNoCredAdminShortcut.TabIndex = 19;
            this.lblOpenRdpNoCredAdminShortcut.Text = "Open RDP Connection without Credentials (/admin)";
            // 
            // chkOpenRdpNoCredAdminCtrl
            // 
            this.chkOpenRdpNoCredAdminCtrl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkOpenRdpNoCredAdminCtrl.AutoSize = true;
            this.chkOpenRdpNoCredAdminCtrl.Location = new System.Drawing.Point(263, 172);
            this.chkOpenRdpNoCredAdminCtrl.Name = "chkOpenRdpNoCredAdminCtrl";
            this.chkOpenRdpNoCredAdminCtrl.Size = new System.Drawing.Size(22, 21);
            this.chkOpenRdpNoCredAdminCtrl.TabIndex = 20;
            this.chkOpenRdpNoCredAdminCtrl.UseVisualStyleBackColor = true;
            // 
            // chkOpenRdpNoCredAdminAlt
            // 
            this.chkOpenRdpNoCredAdminAlt.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkOpenRdpNoCredAdminAlt.AutoSize = true;
            this.chkOpenRdpNoCredAdminAlt.Location = new System.Drawing.Point(313, 172);
            this.chkOpenRdpNoCredAdminAlt.Name = "chkOpenRdpNoCredAdminAlt";
            this.chkOpenRdpNoCredAdminAlt.Size = new System.Drawing.Size(22, 21);
            this.chkOpenRdpNoCredAdminAlt.TabIndex = 21;
            this.chkOpenRdpNoCredAdminAlt.UseVisualStyleBackColor = true;
            // 
            // chkOpenRdpNoCredAdminShift
            // 
            this.chkOpenRdpNoCredAdminShift.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkOpenRdpNoCredAdminShift.AutoSize = true;
            this.chkOpenRdpNoCredAdminShift.Location = new System.Drawing.Point(363, 172);
            this.chkOpenRdpNoCredAdminShift.Name = "chkOpenRdpNoCredAdminShift";
            this.chkOpenRdpNoCredAdminShift.Size = new System.Drawing.Size(22, 21);
            this.chkOpenRdpNoCredAdminShift.TabIndex = 22;
            this.chkOpenRdpNoCredAdminShift.UseVisualStyleBackColor = true;
            // 
            // txtOpenRdpNoCredAdminKey
            // 
            this.txtOpenRdpNoCredAdminKey.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtOpenRdpNoCredAdminKey.Location = new System.Drawing.Point(402, 169);
            this.txtOpenRdpNoCredAdminKey.Name = "txtOpenRdpNoCredAdminKey";
            this.txtOpenRdpNoCredAdminKey.Size = new System.Drawing.Size(100, 26);
            this.txtOpenRdpNoCredAdminKey.TabIndex = 23;
            // 
            // txtOpenRdpNoCredKey
            // 
            this.txtOpenRdpNoCredKey.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtOpenRdpNoCredKey.Location = new System.Drawing.Point(402, 122);
            this.txtOpenRdpNoCredKey.Name = "txtOpenRdpNoCredKey";
            this.txtOpenRdpNoCredKey.Size = new System.Drawing.Size(100, 26);
            this.txtOpenRdpNoCredKey.TabIndex = 18;
            // 
            // txtOpenRdpAdminKey
            // 
            this.txtOpenRdpAdminKey.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtOpenRdpAdminKey.Location = new System.Drawing.Point(402, 76);
            this.txtOpenRdpAdminKey.Name = "txtOpenRdpAdminKey";
            this.txtOpenRdpAdminKey.Size = new System.Drawing.Size(100, 26);
            this.txtOpenRdpAdminKey.TabIndex = 13;
            // 
            // txtOpenRdpKey
            // 
            this.txtOpenRdpKey.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtOpenRdpKey.Location = new System.Drawing.Point(402, 30);
            this.txtOpenRdpKey.Name = "txtOpenRdpKey";
            this.txtOpenRdpKey.Size = new System.Drawing.Size(100, 26);
            this.txtOpenRdpKey.TabIndex = 8;
            // 
            // chkOpenRdpNoCredShift
            // 
            this.chkOpenRdpNoCredShift.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkOpenRdpNoCredShift.AutoSize = true;
            this.chkOpenRdpNoCredShift.Location = new System.Drawing.Point(363, 124);
            this.chkOpenRdpNoCredShift.Name = "chkOpenRdpNoCredShift";
            this.chkOpenRdpNoCredShift.Size = new System.Drawing.Size(22, 21);
            this.chkOpenRdpNoCredShift.TabIndex = 17;
            this.chkOpenRdpNoCredShift.UseVisualStyleBackColor = true;
            // 
            // chkOpenRdpNoCredAlt
            // 
            this.chkOpenRdpNoCredAlt.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkOpenRdpNoCredAlt.AutoSize = true;
            this.chkOpenRdpNoCredAlt.Location = new System.Drawing.Point(313, 124);
            this.chkOpenRdpNoCredAlt.Name = "chkOpenRdpNoCredAlt";
            this.chkOpenRdpNoCredAlt.Size = new System.Drawing.Size(22, 21);
            this.chkOpenRdpNoCredAlt.TabIndex = 16;
            this.chkOpenRdpNoCredAlt.UseVisualStyleBackColor = true;
            // 
            // chkOpenRdpNoCredCtrl
            // 
            this.chkOpenRdpNoCredCtrl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkOpenRdpNoCredCtrl.AutoSize = true;
            this.chkOpenRdpNoCredCtrl.Location = new System.Drawing.Point(263, 124);
            this.chkOpenRdpNoCredCtrl.Name = "chkOpenRdpNoCredCtrl";
            this.chkOpenRdpNoCredCtrl.Size = new System.Drawing.Size(22, 21);
            this.chkOpenRdpNoCredCtrl.TabIndex = 15;
            this.chkOpenRdpNoCredCtrl.UseVisualStyleBackColor = true;
            // 
            // lblOpenRdpNoCredShortcut
            // 
            this.lblOpenRdpNoCredShortcut.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblOpenRdpNoCredShortcut.AutoSize = true;
            this.lblOpenRdpNoCredShortcut.Location = new System.Drawing.Point(3, 115);
            this.lblOpenRdpNoCredShortcut.Name = "lblOpenRdpNoCredShortcut";
            this.lblOpenRdpNoCredShortcut.Size = new System.Drawing.Size(230, 40);
            this.lblOpenRdpNoCredShortcut.TabIndex = 14;
            this.lblOpenRdpNoCredShortcut.Text = "Open RDP Connection without credentials";
            // 
            // lblOpenRdpAdminShortcut
            // 
            this.lblOpenRdpAdminShortcut.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblOpenRdpAdminShortcut.AutoSize = true;
            this.lblOpenRdpAdminShortcut.Location = new System.Drawing.Point(3, 79);
            this.lblOpenRdpAdminShortcut.Name = "lblOpenRdpAdminShortcut";
            this.lblOpenRdpAdminShortcut.Size = new System.Drawing.Size(232, 20);
            this.lblOpenRdpAdminShortcut.TabIndex = 9;
            this.lblOpenRdpAdminShortcut.Text = "Open RDP Connection (/admin)";
            // 
            // chkOpenRdpAdminCtrl
            // 
            this.chkOpenRdpAdminCtrl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkOpenRdpAdminCtrl.AutoSize = true;
            this.chkOpenRdpAdminCtrl.Location = new System.Drawing.Point(263, 78);
            this.chkOpenRdpAdminCtrl.Name = "chkOpenRdpAdminCtrl";
            this.chkOpenRdpAdminCtrl.Size = new System.Drawing.Size(22, 21);
            this.chkOpenRdpAdminCtrl.TabIndex = 10;
            this.chkOpenRdpAdminCtrl.UseVisualStyleBackColor = true;
            // 
            // chkOpenRdpAdminAlt
            // 
            this.chkOpenRdpAdminAlt.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkOpenRdpAdminAlt.AutoSize = true;
            this.chkOpenRdpAdminAlt.Location = new System.Drawing.Point(313, 78);
            this.chkOpenRdpAdminAlt.Name = "chkOpenRdpAdminAlt";
            this.chkOpenRdpAdminAlt.Size = new System.Drawing.Size(22, 21);
            this.chkOpenRdpAdminAlt.TabIndex = 11;
            this.chkOpenRdpAdminAlt.UseVisualStyleBackColor = true;
            // 
            // chkOpenRdpAdminShift
            // 
            this.chkOpenRdpAdminShift.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkOpenRdpAdminShift.AutoSize = true;
            this.chkOpenRdpAdminShift.Location = new System.Drawing.Point(363, 78);
            this.chkOpenRdpAdminShift.Name = "chkOpenRdpAdminShift";
            this.chkOpenRdpAdminShift.Size = new System.Drawing.Size(22, 21);
            this.chkOpenRdpAdminShift.TabIndex = 12;
            this.chkOpenRdpAdminShift.UseVisualStyleBackColor = true;
            // 
            // chkOpenRdpCtrl
            // 
            this.chkOpenRdpCtrl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkOpenRdpCtrl.AutoSize = true;
            this.chkOpenRdpCtrl.Location = new System.Drawing.Point(263, 32);
            this.chkOpenRdpCtrl.Name = "chkOpenRdpCtrl";
            this.chkOpenRdpCtrl.Size = new System.Drawing.Size(22, 21);
            this.chkOpenRdpCtrl.TabIndex = 5;
            this.chkOpenRdpCtrl.UseVisualStyleBackColor = true;
            // 
            // chkOpenRdpAlt
            // 
            this.chkOpenRdpAlt.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkOpenRdpAlt.AutoSize = true;
            this.chkOpenRdpAlt.Location = new System.Drawing.Point(313, 32);
            this.chkOpenRdpAlt.Name = "chkOpenRdpAlt";
            this.chkOpenRdpAlt.Size = new System.Drawing.Size(22, 21);
            this.chkOpenRdpAlt.TabIndex = 6;
            this.chkOpenRdpAlt.UseVisualStyleBackColor = true;
            // 
            // chkOpenRdpShift
            // 
            this.chkOpenRdpShift.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkOpenRdpShift.AutoSize = true;
            this.chkOpenRdpShift.Location = new System.Drawing.Point(363, 32);
            this.chkOpenRdpShift.Name = "chkOpenRdpShift";
            this.chkOpenRdpShift.Size = new System.Drawing.Size(22, 21);
            this.chkOpenRdpShift.TabIndex = 7;
            this.chkOpenRdpShift.UseVisualStyleBackColor = true;
            // 
            // lblOpenRdpShortcut
            // 
            this.lblOpenRdpShortcut.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblOpenRdpShortcut.AutoSize = true;
            this.lblOpenRdpShortcut.Location = new System.Drawing.Point(3, 33);
            this.lblOpenRdpShortcut.Name = "lblOpenRdpShortcut";
            this.lblOpenRdpShortcut.Size = new System.Drawing.Size(171, 20);
            this.lblOpenRdpShortcut.TabIndex = 4;
            this.lblOpenRdpShortcut.Text = "Open RDP Connection";
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(257, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(33, 20);
            this.label9.TabIndex = 0;
            this.label9.Text = "Ctrl";
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(310, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(28, 20);
            this.label10.TabIndex = 1;
            this.label10.Text = "Alt";
            // 
            // label11
            // 
            this.label11.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(353, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(42, 20);
            this.label11.TabIndex = 2;
            this.label11.Text = "Shift";
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(402, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(35, 20);
            this.label12.TabIndex = 3;
            this.label12.Text = "Key";
            // 
            // grpCredentialOptions
            // 
            this.grpCredentialOptions.Controls.Add(this.chkCredVaultUseWindows);
            this.grpCredentialOptions.Controls.Add(this.lblCredVaultTtl);
            this.grpCredentialOptions.Controls.Add(this.numCredVaultTtl);
            this.grpCredentialOptions.Location = new System.Drawing.Point(7, 8);
            this.grpCredentialOptions.Name = "grpCredentialOptions";
            this.grpCredentialOptions.Size = new System.Drawing.Size(282, 100);
            this.grpCredentialOptions.TabIndex = 0;
            this.grpCredentialOptions.TabStop = false;
            this.grpCredentialOptions.Text = "Credential options";
            // 
            // tabCredPicker
            // 
            this.tabCredPicker.Controls.Add(this.cmdRegExPostReset);
            this.tabCredPicker.Controls.Add(this.cmdRegExPreReset);
            this.tabCredPicker.Controls.Add(this.label3);
            this.tabCredPicker.Controls.Add(this.label2);
            this.tabCredPicker.Controls.Add(this.cmdRegExPostRemove);
            this.tabCredPicker.Controls.Add(this.cmdRegExPostAdd);
            this.tabCredPicker.Controls.Add(this.txtRegExPost);
            this.tabCredPicker.Controls.Add(this.lstRegExPost);
            this.tabCredPicker.Controls.Add(this.cmdRegExPreRemove);
            this.tabCredPicker.Controls.Add(this.cmdRegExPreAdd);
            this.tabCredPicker.Controls.Add(this.txtRegExPre);
            this.tabCredPicker.Controls.Add(this.lstRegExPre);
            this.tabCredPicker.Controls.Add(this.chkCredPickRememberSize);
            this.tabCredPicker.Controls.Add(this.txtCredPickHeight);
            this.tabCredPicker.Controls.Add(this.chkKeepassShowResolvedReferences);
            this.tabCredPicker.Controls.Add(this.numCredPickHeight);
            this.tabCredPicker.Controls.Add(this.numCredPickWidth);
            this.tabCredPicker.Controls.Add(this.txtCredPickWidth);
            this.tabCredPicker.Location = new System.Drawing.Point(4, 29);
            this.tabCredPicker.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabCredPicker.Name = "tabCredPicker";
            this.tabCredPicker.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabCredPicker.Size = new System.Drawing.Size(526, 393);
            this.tabCredPicker.TabIndex = 0;
            this.tabCredPicker.Text = "CredPicker options";
            this.tabCredPicker.UseVisualStyleBackColor = true;
            // 
            // cmdRegExPostReset
            // 
            this.cmdRegExPostReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRegExPostReset.Font = new System.Drawing.Font("Segoe UI Emoji", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRegExPostReset.Location = new System.Drawing.Point(456, 342);
            this.cmdRegExPostReset.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdRegExPostReset.Name = "cmdRegExPostReset";
            this.cmdRegExPostReset.Size = new System.Drawing.Size(44, 35);
            this.cmdRegExPostReset.TabIndex = 17;
            this.cmdRegExPostReset.Text = "↺";
            this.cmdRegExPostReset.UseVisualStyleBackColor = true;
            this.cmdRegExPostReset.Click += new System.EventHandler(this.cmdRegExPostReset_Click);
            // 
            // cmdRegExPreReset
            // 
            this.cmdRegExPreReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRegExPreReset.Font = new System.Drawing.Font("Segoe UI Emoji", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRegExPreReset.Location = new System.Drawing.Point(201, 342);
            this.cmdRegExPreReset.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdRegExPreReset.Name = "cmdRegExPreReset";
            this.cmdRegExPreReset.Size = new System.Drawing.Size(44, 35);
            this.cmdRegExPreReset.TabIndex = 11;
            this.cmdRegExPreReset.Text = "↺";
            this.cmdRegExPreReset.UseVisualStyleBackColor = true;
            this.cmdRegExPreReset.Click += new System.EventHandler(this.cmdRegExPreReset_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(262, 166);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(129, 20);
            this.label3.TabIndex = 12;
            this.label3.Text = "RegEx Postfixes:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 166);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 20);
            this.label2.TabIndex = 6;
            this.label2.Text = "RegEx Prefixes:";
            // 
            // cmdRegExPostRemove
            // 
            this.cmdRegExPostRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRegExPostRemove.Enabled = false;
            this.cmdRegExPostRemove.Location = new System.Drawing.Point(456, 231);
            this.cmdRegExPostRemove.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdRegExPostRemove.Name = "cmdRegExPostRemove";
            this.cmdRegExPostRemove.Size = new System.Drawing.Size(44, 35);
            this.cmdRegExPostRemove.TabIndex = 16;
            this.cmdRegExPostRemove.Text = "－";
            this.cmdRegExPostRemove.UseVisualStyleBackColor = true;
            this.cmdRegExPostRemove.Click += new System.EventHandler(this.cmdRegExPostRemove_Click);
            // 
            // cmdRegExPostAdd
            // 
            this.cmdRegExPostAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRegExPostAdd.Location = new System.Drawing.Point(456, 188);
            this.cmdRegExPostAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdRegExPostAdd.Name = "cmdRegExPostAdd";
            this.cmdRegExPostAdd.Size = new System.Drawing.Size(44, 35);
            this.cmdRegExPostAdd.TabIndex = 14;
            this.cmdRegExPostAdd.Text = "＋";
            this.cmdRegExPostAdd.UseVisualStyleBackColor = true;
            this.cmdRegExPostAdd.Click += new System.EventHandler(this.cmdRegExPostAdd_Click);
            // 
            // txtRegExPost
            // 
            this.txtRegExPost.Location = new System.Drawing.Point(267, 191);
            this.txtRegExPost.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtRegExPost.Name = "txtRegExPost";
            this.txtRegExPost.Size = new System.Drawing.Size(178, 26);
            this.txtRegExPost.TabIndex = 13;
            // 
            // lstRegExPost
            // 
            this.lstRegExPost.FormattingEnabled = true;
            this.lstRegExPost.ItemHeight = 20;
            this.lstRegExPost.Location = new System.Drawing.Point(267, 231);
            this.lstRegExPost.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lstRegExPost.Name = "lstRegExPost";
            this.lstRegExPost.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstRegExPost.Size = new System.Drawing.Size(178, 144);
            this.lstRegExPost.TabIndex = 15;
            this.lstRegExPost.SelectedIndexChanged += new System.EventHandler(this.lstRegExPost_SelectedIndexChanged);
            // 
            // cmdRegExPreRemove
            // 
            this.cmdRegExPreRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRegExPreRemove.Enabled = false;
            this.cmdRegExPreRemove.Location = new System.Drawing.Point(201, 231);
            this.cmdRegExPreRemove.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdRegExPreRemove.Name = "cmdRegExPreRemove";
            this.cmdRegExPreRemove.Size = new System.Drawing.Size(44, 35);
            this.cmdRegExPreRemove.TabIndex = 10;
            this.cmdRegExPreRemove.Text = "－";
            this.cmdRegExPreRemove.UseVisualStyleBackColor = true;
            this.cmdRegExPreRemove.Click += new System.EventHandler(this.cmdRegExPreRemove_Click);
            // 
            // cmdRegExPreAdd
            // 
            this.cmdRegExPreAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRegExPreAdd.Location = new System.Drawing.Point(201, 188);
            this.cmdRegExPreAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdRegExPreAdd.Name = "cmdRegExPreAdd";
            this.cmdRegExPreAdd.Size = new System.Drawing.Size(44, 35);
            this.cmdRegExPreAdd.TabIndex = 8;
            this.cmdRegExPreAdd.Text = "＋";
            this.cmdRegExPreAdd.UseVisualStyleBackColor = true;
            this.cmdRegExPreAdd.Click += new System.EventHandler(this.cmdRegExPreAdd_Click);
            // 
            // txtRegExPre
            // 
            this.txtRegExPre.Location = new System.Drawing.Point(12, 191);
            this.txtRegExPre.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtRegExPre.Name = "txtRegExPre";
            this.txtRegExPre.Size = new System.Drawing.Size(178, 26);
            this.txtRegExPre.TabIndex = 7;
            // 
            // lstRegExPre
            // 
            this.lstRegExPre.FormattingEnabled = true;
            this.lstRegExPre.ItemHeight = 20;
            this.lstRegExPre.Location = new System.Drawing.Point(12, 231);
            this.lstRegExPre.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lstRegExPre.Name = "lstRegExPre";
            this.lstRegExPre.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstRegExPre.Size = new System.Drawing.Size(178, 144);
            this.lstRegExPre.TabIndex = 9;
            this.lstRegExPre.SelectedIndexChanged += new System.EventHandler(this.lstRegExPre_SelectedIndexChanged);
            // 
            // tabMstsc
            // 
            this.tabMstsc.Controls.Add(this.lblHeight);
            this.tabMstsc.Controls.Add(this.chkMstscUseFullscreen);
            this.tabMstsc.Controls.Add(this.numMstscHeight);
            this.tabMstsc.Controls.Add(this.chkMstscUseAdmin);
            this.tabMstsc.Controls.Add(this.lblWidth);
            this.tabMstsc.Controls.Add(this.chkMstscUseSpan);
            this.tabMstsc.Controls.Add(this.numMstscWidth);
            this.tabMstsc.Controls.Add(this.chkMstscUseMultimon);
            this.tabMstsc.Location = new System.Drawing.Point(4, 29);
            this.tabMstsc.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabMstsc.Name = "tabMstsc";
            this.tabMstsc.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabMstsc.Size = new System.Drawing.Size(526, 393);
            this.tabMstsc.TabIndex = 1;
            this.tabMstsc.Text = "Mstsc options";
            this.tabMstsc.UseVisualStyleBackColor = true;
            // 
            // KPROptionsForm
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(534, 483);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "KPROptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "KeePassRDP Options";
            this.Load += new System.EventHandler(this.KPROptionsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numMstscHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMstscWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredPickHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredPickWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredVaultTtl)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.grpCredentialOptions.ResumeLayout(false);
            this.grpCredentialOptions.PerformLayout();
            this.tabCredPicker.ResumeLayout(false);
            this.tabCredPicker.PerformLayout();
            this.tabMstsc.ResumeLayout(false);
            this.tabMstsc.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.CheckBox chkMstscUseFullscreen;
        private System.Windows.Forms.CheckBox chkMstscUseMultimon;
        private System.Windows.Forms.CheckBox chkMstscUseSpan;
        private System.Windows.Forms.CheckBox chkMstscUseAdmin;
        private System.Windows.Forms.ToolTip ttMstscParams;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.NumericUpDown numMstscHeight;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.NumericUpDown numMstscWidth;
        private System.Windows.Forms.CheckBox chkKeepassShowResolvedReferences;
        private System.Windows.Forms.ToolTip ttGeneralOptions;
        private System.Windows.Forms.CheckBox chkCredVaultUseWindows;
        private System.Windows.Forms.ToolTip ttCredentialOptions;
        private System.Windows.Forms.Label txtCredPickHeight;
        private System.Windows.Forms.NumericUpDown numCredPickHeight;
        private System.Windows.Forms.Label txtCredPickWidth;
        private System.Windows.Forms.NumericUpDown numCredPickWidth;
        private System.Windows.Forms.CheckBox chkCredPickRememberSize;
        private System.Windows.Forms.Label lblCredVaultTtl;
        private System.Windows.Forms.NumericUpDown numCredVaultTtl;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabCredPicker;
        private System.Windows.Forms.TabPage tabMstsc;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button cmdRegExPostRemove;
        private System.Windows.Forms.Button cmdRegExPostAdd;
        private System.Windows.Forms.TextBox txtRegExPost;
        private System.Windows.Forms.ListBox lstRegExPost;
        private System.Windows.Forms.Button cmdRegExPreRemove;
        private System.Windows.Forms.Button cmdRegExPreAdd;
        private System.Windows.Forms.TextBox txtRegExPre;
        private System.Windows.Forms.ListBox lstRegExPre;
        private System.Windows.Forms.Button cmdRegExPostReset;
        private System.Windows.Forms.Button cmdRegExPreReset;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblOpenRdpNoCredAdminShortcut;
        private System.Windows.Forms.CheckBox chkOpenRdpNoCredAdminCtrl;
        private System.Windows.Forms.CheckBox chkOpenRdpNoCredAdminAlt;
        private System.Windows.Forms.CheckBox chkOpenRdpNoCredAdminShift;
        private System.Windows.Forms.TextBox txtOpenRdpNoCredAdminKey;
        private System.Windows.Forms.TextBox txtOpenRdpNoCredKey;
        private System.Windows.Forms.TextBox txtOpenRdpAdminKey;
        private System.Windows.Forms.TextBox txtOpenRdpKey;
        private System.Windows.Forms.CheckBox chkOpenRdpNoCredShift;
        private System.Windows.Forms.CheckBox chkOpenRdpNoCredAlt;
        private System.Windows.Forms.CheckBox chkOpenRdpNoCredCtrl;
        private System.Windows.Forms.Label lblOpenRdpNoCredShortcut;
        private System.Windows.Forms.Label lblOpenRdpAdminShortcut;
        private System.Windows.Forms.CheckBox chkOpenRdpAdminCtrl;
        private System.Windows.Forms.CheckBox chkOpenRdpAdminAlt;
        private System.Windows.Forms.CheckBox chkOpenRdpAdminShift;
        private System.Windows.Forms.CheckBox chkOpenRdpCtrl;
        private System.Windows.Forms.CheckBox chkOpenRdpAlt;
        private System.Windows.Forms.CheckBox chkOpenRdpShift;
        private System.Windows.Forms.Label lblOpenRdpShortcut;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox grpCredentialOptions;
    }
}