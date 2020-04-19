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
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.grpCredentialOptions = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBox7 = new System.Windows.Forms.CheckBox();
            this.checkBox8 = new System.Windows.Forms.CheckBox();
            this.checkBox9 = new System.Windows.Forms.CheckBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.checkBox10 = new System.Windows.Forms.CheckBox();
            this.checkBox11 = new System.Windows.Forms.CheckBox();
            this.checkBox12 = new System.Windows.Forms.CheckBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numMstscHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMstscWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredPickHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredPickWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCredVaultTtl)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabCredPicker.SuspendLayout();
            this.tabMstsc.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.grpCredentialOptions.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Credential TTL:";
            this.ttCredentialOptions.SetToolTip(this.label1, "Specify duration the credentials reside in the Windows Vault (in seconds).");
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
            // grpCredentialOptions
            // 
            this.grpCredentialOptions.Controls.Add(this.chkCredVaultUseWindows);
            this.grpCredentialOptions.Controls.Add(this.label1);
            this.grpCredentialOptions.Controls.Add(this.numCredVaultTtl);
            this.grpCredentialOptions.Location = new System.Drawing.Point(7, 8);
            this.grpCredentialOptions.Name = "grpCredentialOptions";
            this.grpCredentialOptions.Size = new System.Drawing.Size(282, 100);
            this.grpCredentialOptions.TabIndex = 0;
            this.grpCredentialOptions.TabStop = false;
            this.grpCredentialOptions.Text = "Credential options";
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
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBox10, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBox11, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBox12, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.textBox4, 4, 4);
            this.tableLayoutPanel1.Controls.Add(this.textBox3, 4, 3);
            this.tableLayoutPanel1.Controls.Add(this.textBox2, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBox1, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBox9, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.checkBox8, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.checkBox7, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBox4, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBox5, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBox6, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBox1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBox2, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBox3, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 1);
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
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(171, 20);
            this.label4.TabIndex = 4;
            this.label4.Text = "Open RDP Connection";
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(263, 32);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(22, 21);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(313, 32);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(22, 21);
            this.checkBox2.TabIndex = 6;
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(363, 32);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(22, 21);
            this.checkBox3.TabIndex = 7;
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBox1.Location = new System.Drawing.Point(402, 30);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 26);
            this.textBox1.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 79);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(232, 20);
            this.label5.TabIndex = 9;
            this.label5.Text = "Open RDP Connection (/admin)";
            // 
            // checkBox4
            // 
            this.checkBox4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(263, 78);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(22, 21);
            this.checkBox4.TabIndex = 10;
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            this.checkBox5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(313, 78);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(22, 21);
            this.checkBox5.TabIndex = 11;
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // checkBox6
            // 
            this.checkBox6.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox6.AutoSize = true;
            this.checkBox6.Location = new System.Drawing.Point(363, 78);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(22, 21);
            this.checkBox6.TabIndex = 12;
            this.checkBox6.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            this.textBox2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBox2.Location = new System.Drawing.Point(402, 76);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 26);
            this.textBox2.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 115);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(230, 40);
            this.label6.TabIndex = 14;
            this.label6.Text = "Open RDP Connection without credentials";
            // 
            // checkBox7
            // 
            this.checkBox7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox7.AutoSize = true;
            this.checkBox7.Location = new System.Drawing.Point(263, 124);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new System.Drawing.Size(22, 21);
            this.checkBox7.TabIndex = 15;
            this.checkBox7.UseVisualStyleBackColor = true;
            // 
            // checkBox8
            // 
            this.checkBox8.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox8.AutoSize = true;
            this.checkBox8.Location = new System.Drawing.Point(313, 124);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new System.Drawing.Size(22, 21);
            this.checkBox8.TabIndex = 16;
            this.checkBox8.UseVisualStyleBackColor = true;
            // 
            // checkBox9
            // 
            this.checkBox9.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox9.AutoSize = true;
            this.checkBox9.Location = new System.Drawing.Point(363, 124);
            this.checkBox9.Name = "checkBox9";
            this.checkBox9.Size = new System.Drawing.Size(22, 21);
            this.checkBox9.TabIndex = 17;
            this.checkBox9.UseVisualStyleBackColor = true;
            // 
            // textBox3
            // 
            this.textBox3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBox3.Location = new System.Drawing.Point(402, 122);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(100, 26);
            this.textBox3.TabIndex = 18;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 162);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(230, 40);
            this.label7.TabIndex = 19;
            this.label7.Text = "Open RDP Connection without Credentials (/admin)";
            // 
            // checkBox10
            // 
            this.checkBox10.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox10.AutoSize = true;
            this.checkBox10.Location = new System.Drawing.Point(263, 172);
            this.checkBox10.Name = "checkBox10";
            this.checkBox10.Size = new System.Drawing.Size(22, 21);
            this.checkBox10.TabIndex = 20;
            this.checkBox10.UseVisualStyleBackColor = true;
            // 
            // checkBox11
            // 
            this.checkBox11.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox11.AutoSize = true;
            this.checkBox11.Location = new System.Drawing.Point(313, 172);
            this.checkBox11.Name = "checkBox11";
            this.checkBox11.Size = new System.Drawing.Size(22, 21);
            this.checkBox11.TabIndex = 21;
            this.checkBox11.UseVisualStyleBackColor = true;
            // 
            // checkBox12
            // 
            this.checkBox12.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox12.AutoSize = true;
            this.checkBox12.Location = new System.Drawing.Point(363, 172);
            this.checkBox12.Name = "checkBox12";
            this.checkBox12.Size = new System.Drawing.Size(22, 21);
            this.checkBox12.TabIndex = 22;
            this.checkBox12.UseVisualStyleBackColor = true;
            // 
            // textBox4
            // 
            this.textBox4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBox4.Location = new System.Drawing.Point(402, 169);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(100, 26);
            this.textBox4.TabIndex = 23;
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
            this.tabCredPicker.ResumeLayout(false);
            this.tabCredPicker.PerformLayout();
            this.tabMstsc.ResumeLayout(false);
            this.tabMstsc.PerformLayout();
            this.tabGeneral.ResumeLayout(false);
            this.grpCredentialOptions.ResumeLayout(false);
            this.grpCredentialOptions.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
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
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox checkBox10;
        private System.Windows.Forms.CheckBox checkBox11;
        private System.Windows.Forms.CheckBox checkBox12;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox checkBox9;
        private System.Windows.Forms.CheckBox checkBox8;
        private System.Windows.Forms.CheckBox checkBox7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox grpCredentialOptions;
    }
}