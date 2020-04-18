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
            this.label1 = new System.Windows.Forms.Label();
            this.numCredVaultTtl = new System.Windows.Forms.NumericUpDown();
            this.chkCredVaultUseWindows = new System.Windows.Forms.CheckBox();
            this.ttCredentialOptions = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.lstRegExPre = new System.Windows.Forms.ListBox();
            this.txtRegExPre = new System.Windows.Forms.TextBox();
            this.cmdRegExPreAdd = new System.Windows.Forms.Button();
            this.cmdRegExPreRemove = new System.Windows.Forms.Button();
            this.cmdRegExPostRemove = new System.Windows.Forms.Button();
            this.cmdRegExPostAdd = new System.Windows.Forms.Button();
            this.txtRegExPost = new System.Windows.Forms.TextBox();
            this.lstRegExPost = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmdRegExPreReset = new System.Windows.Forms.Button();
            this.cmdRegExPostReset = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numMstscHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMstscWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredPickHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredPickWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredVaultTtl)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(267, 283);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(79, 23);
            this.cmdCancel.TabIndex = 2;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // cmdOk
            // 
            this.cmdOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.Location = new System.Drawing.Point(183, 283);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(79, 23);
            this.cmdOk.TabIndex = 1;
            this.cmdOk.Text = "&Ok";
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(5, 115);
            this.lblHeight.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(61, 13);
            this.lblHeight.TabIndex = 6;
            this.lblHeight.Text = "Height - /h:";
            this.ttMstscParams.SetToolTip(this.lblHeight, "/h:<height>\r\nSpecifies the height of the Remote Desktop window.\r\n0 = not set\r\n");
            // 
            // numMstscHeight
            // 
            this.numMstscHeight.Location = new System.Drawing.Point(70, 113);
            this.numMstscHeight.Margin = new System.Windows.Forms.Padding(2);
            this.numMstscHeight.Name = "numMstscHeight";
            this.numMstscHeight.Size = new System.Drawing.Size(128, 20);
            this.numMstscHeight.TabIndex = 7;
            this.ttMstscParams.SetToolTip(this.numMstscHeight, "/h:<height>\r\nSpecifies the height of the Remote Desktop window.\r\n0 = not set");
            // 
            // lblWidth
            // 
            this.lblWidth.AutoSize = true;
            this.lblWidth.Location = new System.Drawing.Point(5, 91);
            this.lblWidth.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(60, 13);
            this.lblWidth.TabIndex = 4;
            this.lblWidth.Text = "Width - /w:";
            this.ttMstscParams.SetToolTip(this.lblWidth, "/w:<width>\r\nSpecifies the width of the Remote Desktop window.\r\n0 = not set\r\n");
            // 
            // numMstscWidth
            // 
            this.numMstscWidth.Location = new System.Drawing.Point(70, 89);
            this.numMstscWidth.Margin = new System.Windows.Forms.Padding(2);
            this.numMstscWidth.Name = "numMstscWidth";
            this.numMstscWidth.Size = new System.Drawing.Size(128, 20);
            this.numMstscWidth.TabIndex = 5;
            this.ttMstscParams.SetToolTip(this.numMstscWidth, "/w:<width>\r\nSpecifies the width of the Remote Desktop window.\r\n0 = not set");
            // 
            // chkMstscUseMultimon
            // 
            this.chkMstscUseMultimon.AutoSize = true;
            this.chkMstscUseMultimon.Location = new System.Drawing.Point(5, 68);
            this.chkMstscUseMultimon.Margin = new System.Windows.Forms.Padding(2);
            this.chkMstscUseMultimon.Name = "chkMstscUseMultimon";
            this.chkMstscUseMultimon.Size = new System.Drawing.Size(155, 17);
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
            this.chkMstscUseSpan.Location = new System.Drawing.Point(5, 47);
            this.chkMstscUseSpan.Margin = new System.Windows.Forms.Padding(2);
            this.chkMstscUseSpan.Name = "chkMstscUseSpan";
            this.chkMstscUseSpan.Size = new System.Drawing.Size(128, 17);
            this.chkMstscUseSpan.TabIndex = 2;
            this.chkMstscUseSpan.Text = "Use &spanning - /span";
            this.ttMstscParams.SetToolTip(this.chkMstscUseSpan, resources.GetString("chkMstscUseSpan.ToolTip"));
            this.chkMstscUseSpan.UseVisualStyleBackColor = true;
            this.chkMstscUseSpan.CheckedChanged += new System.EventHandler(this.checkMstscSizeEnable);
            // 
            // chkMstscUseAdmin
            // 
            this.chkMstscUseAdmin.AutoSize = true;
            this.chkMstscUseAdmin.Location = new System.Drawing.Point(5, 26);
            this.chkMstscUseAdmin.Margin = new System.Windows.Forms.Padding(2);
            this.chkMstscUseAdmin.Name = "chkMstscUseAdmin";
            this.chkMstscUseAdmin.Size = new System.Drawing.Size(118, 17);
            this.chkMstscUseAdmin.TabIndex = 1;
            this.chkMstscUseAdmin.Text = "Use &admin - /admin";
            this.ttMstscParams.SetToolTip(this.chkMstscUseAdmin, "mstsc.exe /admin\r\nConnects you to the session for administering a remote PC.");
            this.chkMstscUseAdmin.UseVisualStyleBackColor = true;
            // 
            // chkMstscUseFullscreen
            // 
            this.chkMstscUseFullscreen.AutoSize = true;
            this.chkMstscUseFullscreen.Location = new System.Drawing.Point(5, 5);
            this.chkMstscUseFullscreen.Margin = new System.Windows.Forms.Padding(2);
            this.chkMstscUseFullscreen.Name = "chkMstscUseFullscreen";
            this.chkMstscUseFullscreen.Size = new System.Drawing.Size(110, 17);
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
            this.chkCredPickRememberSize.Location = new System.Drawing.Point(5, 26);
            this.chkCredPickRememberSize.Margin = new System.Windows.Forms.Padding(2);
            this.chkCredPickRememberSize.Name = "chkCredPickRememberSize";
            this.chkCredPickRememberSize.Size = new System.Drawing.Size(153, 17);
            this.chkCredPickRememberSize.TabIndex = 1;
            this.chkCredPickRememberSize.Text = "Remember CredPicker size";
            this.ttGeneralOptions.SetToolTip(this.chkCredPickRememberSize, "Always remembers the last CredPicker window size.\r\nIf you can alwas set the size " +
        "using the values below.");
            this.chkCredPickRememberSize.UseVisualStyleBackColor = true;
            // 
            // txtCredPickHeight
            // 
            this.txtCredPickHeight.AutoSize = true;
            this.txtCredPickHeight.Location = new System.Drawing.Point(5, 73);
            this.txtCredPickHeight.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.txtCredPickHeight.Name = "txtCredPickHeight";
            this.txtCredPickHeight.Size = new System.Drawing.Size(94, 13);
            this.txtCredPickHeight.TabIndex = 4;
            this.txtCredPickHeight.Text = "CredPicker height:";
            // 
            // numCredPickHeight
            // 
            this.numCredPickHeight.Location = new System.Drawing.Point(103, 71);
            this.numCredPickHeight.Margin = new System.Windows.Forms.Padding(2);
            this.numCredPickHeight.Name = "numCredPickHeight";
            this.numCredPickHeight.Size = new System.Drawing.Size(91, 20);
            this.numCredPickHeight.TabIndex = 5;
            // 
            // txtCredPickWidth
            // 
            this.txtCredPickWidth.AutoSize = true;
            this.txtCredPickWidth.Location = new System.Drawing.Point(5, 49);
            this.txtCredPickWidth.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.txtCredPickWidth.Name = "txtCredPickWidth";
            this.txtCredPickWidth.Size = new System.Drawing.Size(90, 13);
            this.txtCredPickWidth.TabIndex = 2;
            this.txtCredPickWidth.Text = "CredPicker width:";
            // 
            // numCredPickWidth
            // 
            this.numCredPickWidth.Location = new System.Drawing.Point(103, 47);
            this.numCredPickWidth.Margin = new System.Windows.Forms.Padding(2);
            this.numCredPickWidth.Name = "numCredPickWidth";
            this.numCredPickWidth.Size = new System.Drawing.Size(91, 20);
            this.numCredPickWidth.TabIndex = 3;
            // 
            // chkKeepassShowResolvedReferences
            // 
            this.chkKeepassShowResolvedReferences.AutoSize = true;
            this.chkKeepassShowResolvedReferences.Location = new System.Drawing.Point(5, 5);
            this.chkKeepassShowResolvedReferences.Margin = new System.Windows.Forms.Padding(2);
            this.chkKeepassShowResolvedReferences.Name = "chkKeepassShowResolvedReferences";
            this.chkKeepassShowResolvedReferences.Size = new System.Drawing.Size(154, 17);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 28);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Credential TTL:";
            this.ttCredentialOptions.SetToolTip(this.label1, "Specify duration the credentials reside in the Windows Vault (in seconds).");
            // 
            // numCredVaultTtl
            // 
            this.numCredVaultTtl.Location = new System.Drawing.Point(89, 26);
            this.numCredVaultTtl.Margin = new System.Windows.Forms.Padding(2);
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
            this.numCredVaultTtl.Size = new System.Drawing.Size(91, 20);
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
            this.chkCredVaultUseWindows.Location = new System.Drawing.Point(5, 5);
            this.chkCredVaultUseWindows.Margin = new System.Windows.Forms.Padding(2);
            this.chkCredVaultUseWindows.Name = "chkCredVaultUseWindows";
            this.chkCredVaultUseWindows.Size = new System.Drawing.Size(166, 17);
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
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(356, 277);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.cmdRegExPostReset);
            this.tabPage1.Controls.Add(this.cmdRegExPreReset);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.cmdRegExPostRemove);
            this.tabPage1.Controls.Add(this.cmdRegExPostAdd);
            this.tabPage1.Controls.Add(this.txtRegExPost);
            this.tabPage1.Controls.Add(this.lstRegExPost);
            this.tabPage1.Controls.Add(this.cmdRegExPreRemove);
            this.tabPage1.Controls.Add(this.cmdRegExPreAdd);
            this.tabPage1.Controls.Add(this.txtRegExPre);
            this.tabPage1.Controls.Add(this.lstRegExPre);
            this.tabPage1.Controls.Add(this.chkCredPickRememberSize);
            this.tabPage1.Controls.Add(this.txtCredPickHeight);
            this.tabPage1.Controls.Add(this.chkKeepassShowResolvedReferences);
            this.tabPage1.Controls.Add(this.numCredPickHeight);
            this.tabPage1.Controls.Add(this.numCredPickWidth);
            this.tabPage1.Controls.Add(this.txtCredPickWidth);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(348, 251);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "CredPicker options";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lblHeight);
            this.tabPage2.Controls.Add(this.chkMstscUseFullscreen);
            this.tabPage2.Controls.Add(this.numMstscHeight);
            this.tabPage2.Controls.Add(this.chkMstscUseAdmin);
            this.tabPage2.Controls.Add(this.lblWidth);
            this.tabPage2.Controls.Add(this.chkMstscUseSpan);
            this.tabPage2.Controls.Add(this.numMstscWidth);
            this.tabPage2.Controls.Add(this.chkMstscUseMultimon);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(348, 251);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Mstsc options";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.chkCredVaultUseWindows);
            this.tabPage3.Controls.Add(this.numCredVaultTtl);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(350, 261);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Credential options";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // lstRegExPre
            // 
            this.lstRegExPre.FormattingEnabled = true;
            this.lstRegExPre.Location = new System.Drawing.Point(8, 150);
            this.lstRegExPre.Name = "lstRegExPre";
            this.lstRegExPre.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstRegExPre.Size = new System.Drawing.Size(120, 95);
            this.lstRegExPre.TabIndex = 9;
            this.lstRegExPre.SelectedIndexChanged += new System.EventHandler(this.lstRegExPre_SelectedIndexChanged);
            // 
            // txtRegExPre
            // 
            this.txtRegExPre.Location = new System.Drawing.Point(8, 124);
            this.txtRegExPre.Name = "txtRegExPre";
            this.txtRegExPre.Size = new System.Drawing.Size(120, 20);
            this.txtRegExPre.TabIndex = 7;
            // 
            // cmdRegExPreAdd
            // 
            this.cmdRegExPreAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRegExPreAdd.Location = new System.Drawing.Point(134, 122);
            this.cmdRegExPreAdd.Name = "cmdRegExPreAdd";
            this.cmdRegExPreAdd.Size = new System.Drawing.Size(29, 23);
            this.cmdRegExPreAdd.TabIndex = 8;
            this.cmdRegExPreAdd.Text = "＋";
            this.cmdRegExPreAdd.UseVisualStyleBackColor = true;
            this.cmdRegExPreAdd.Click += new System.EventHandler(this.cmdRegExPreAdd_Click);
            // 
            // cmdRegExPreRemove
            // 
            this.cmdRegExPreRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRegExPreRemove.Enabled = false;
            this.cmdRegExPreRemove.Location = new System.Drawing.Point(134, 150);
            this.cmdRegExPreRemove.Name = "cmdRegExPreRemove";
            this.cmdRegExPreRemove.Size = new System.Drawing.Size(29, 23);
            this.cmdRegExPreRemove.TabIndex = 10;
            this.cmdRegExPreRemove.Text = "－";
            this.cmdRegExPreRemove.UseVisualStyleBackColor = true;
            this.cmdRegExPreRemove.Click += new System.EventHandler(this.cmdRegExPreRemove_Click);
            // 
            // cmdRegExPostRemove
            // 
            this.cmdRegExPostRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRegExPostRemove.Enabled = false;
            this.cmdRegExPostRemove.Location = new System.Drawing.Point(304, 150);
            this.cmdRegExPostRemove.Name = "cmdRegExPostRemove";
            this.cmdRegExPostRemove.Size = new System.Drawing.Size(29, 23);
            this.cmdRegExPostRemove.TabIndex = 16;
            this.cmdRegExPostRemove.Text = "－";
            this.cmdRegExPostRemove.UseVisualStyleBackColor = true;
            this.cmdRegExPostRemove.Click += new System.EventHandler(this.cmdRegExPostRemove_Click);
            // 
            // cmdRegExPostAdd
            // 
            this.cmdRegExPostAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRegExPostAdd.Location = new System.Drawing.Point(304, 122);
            this.cmdRegExPostAdd.Name = "cmdRegExPostAdd";
            this.cmdRegExPostAdd.Size = new System.Drawing.Size(29, 23);
            this.cmdRegExPostAdd.TabIndex = 14;
            this.cmdRegExPostAdd.Text = "＋";
            this.cmdRegExPostAdd.UseVisualStyleBackColor = true;
            this.cmdRegExPostAdd.Click += new System.EventHandler(this.cmdRegExPostAdd_Click);
            // 
            // txtRegExPost
            // 
            this.txtRegExPost.Location = new System.Drawing.Point(178, 124);
            this.txtRegExPost.Name = "txtRegExPost";
            this.txtRegExPost.Size = new System.Drawing.Size(120, 20);
            this.txtRegExPost.TabIndex = 13;
            // 
            // lstRegExPost
            // 
            this.lstRegExPost.FormattingEnabled = true;
            this.lstRegExPost.Location = new System.Drawing.Point(178, 150);
            this.lstRegExPost.Name = "lstRegExPost";
            this.lstRegExPost.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstRegExPost.Size = new System.Drawing.Size(120, 95);
            this.lstRegExPost.TabIndex = 15;
            this.lstRegExPost.SelectedIndexChanged += new System.EventHandler(this.lstRegExPost_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "RegEx Prefix";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(175, 108);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "RegEx Postfix";
            // 
            // cmdRegExPreReset
            // 
            this.cmdRegExPreReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRegExPreReset.Font = new System.Drawing.Font("Segoe UI Emoji", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRegExPreReset.Location = new System.Drawing.Point(134, 222);
            this.cmdRegExPreReset.Name = "cmdRegExPreReset";
            this.cmdRegExPreReset.Size = new System.Drawing.Size(29, 23);
            this.cmdRegExPreReset.TabIndex = 11;
            this.cmdRegExPreReset.Text = "↺";
            this.cmdRegExPreReset.UseVisualStyleBackColor = true;
            this.cmdRegExPreReset.Click += new System.EventHandler(this.cmdRegExPreReset_Click);
            // 
            // cmdRegExPostReset
            // 
            this.cmdRegExPostReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRegExPostReset.Font = new System.Drawing.Font("Segoe UI Emoji", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRegExPostReset.Location = new System.Drawing.Point(304, 222);
            this.cmdRegExPostReset.Name = "cmdRegExPostReset";
            this.cmdRegExPostReset.Size = new System.Drawing.Size(29, 23);
            this.cmdRegExPostReset.TabIndex = 17;
            this.cmdRegExPostReset.Text = "↺";
            this.cmdRegExPostReset.UseVisualStyleBackColor = true;
            this.cmdRegExPostReset.Click += new System.EventHandler(this.cmdRegExPostReset_Click);
            // 
            // KPROptionsForm
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(356, 314);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2);
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
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numCredVaultTtl;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
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
    }
}