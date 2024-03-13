namespace KeePassRDP
{
    partial class KprEntrySettingsTab
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
            System.Windows.Forms.Panel pnlMstscParameters;
            System.Windows.Forms.Panel pnlCpRegExPatterns;
            System.Windows.Forms.Panel pnlCpGroupUUIDs;
            System.Windows.Forms.Panel pnlCpExcludedGroupUUIDs;
            KeePassRDP.KprToolTip ttGeneral;
            this.lblMstscParameters = new System.Windows.Forms.Label();
            this.cmdMstscParametersReset = new System.Windows.Forms.Button();
            this.kprImageList = new KeePassRDP.KprImageList(this.components);
            this.imageList1 = ((System.Windows.Forms.ImageList)(this.kprImageList));
            this.lstMstscParameters = new System.Windows.Forms.ListBox();
            this.txtMstscParameters = new System.Windows.Forms.TextBox();
            this.cmdMstscParametersRemove = new System.Windows.Forms.Button();
            this.cmdMstscParametersAdd = new System.Windows.Forms.Button();
            this.lblCpRegExPatterns = new System.Windows.Forms.Label();
            this.cmdCpRegExPatternsReset = new System.Windows.Forms.Button();
            this.lstCpRegExPatterns = new System.Windows.Forms.ListBox();
            this.txtCpRegExPatterns = new System.Windows.Forms.TextBox();
            this.cmdCpRegExPatternsRemove = new System.Windows.Forms.Button();
            this.cmdCpRegExPatternsAdd = new System.Windows.Forms.Button();
            this.lblCpGroupUUIDs = new System.Windows.Forms.Label();
            this.lstCpGroupUUIDs = new System.Windows.Forms.ListBox();
            this.txtCpGroupUUIDs = new System.Windows.Forms.TextBox();
            this.cmdCpGroupUUIDsAdd = new System.Windows.Forms.Button();
            this.cmdCpGroupUUIDsRemove = new System.Windows.Forms.Button();
            this.cmdCpGroupUUIDsReset = new System.Windows.Forms.Button();
            this.lblCpExcludedGroupUUIDs = new System.Windows.Forms.Label();
            this.cmdCpExcludedGroupUUIDsReset = new System.Windows.Forms.Button();
            this.lstCpExcludedGroupUUIDs = new System.Windows.Forms.ListBox();
            this.txtCpExcludedGroupUUIDs = new System.Windows.Forms.TextBox();
            this.cmdCpExcludedGroupUUIDsRemove = new System.Windows.Forms.Button();
            this.cmdCpExcludedGroupUUIDsAdd = new System.Windows.Forms.Button();
            this.cbIgnore = new System.Windows.Forms.CheckBox();
            this.cbUseCredpicker = new System.Windows.Forms.CheckBox();
            this.cbCpRecurseGroups = new System.Windows.Forms.CheckBox();
            this.btnMore = new System.Windows.Forms.Button();
            this.btnRdpFile = new System.Windows.Forms.Button();
            this.tblKprEntrySettingsTab = new System.Windows.Forms.TableLayoutPanel();
            this.flpCheckboxes = new System.Windows.Forms.FlowLayoutPanel();
            this.ttGroupPicker = new KeePassRDP.KprToolTip(this.components);
            this.cmsMore = new System.Windows.Forms.ContextMenuStrip(this.components);
            pnlMstscParameters = new System.Windows.Forms.Panel();
            pnlCpRegExPatterns = new System.Windows.Forms.Panel();
            pnlCpGroupUUIDs = new System.Windows.Forms.Panel();
            pnlCpExcludedGroupUUIDs = new System.Windows.Forms.Panel();
            ttGeneral = new KeePassRDP.KprToolTip(this.components);
            pnlMstscParameters.SuspendLayout();
            pnlCpRegExPatterns.SuspendLayout();
            pnlCpGroupUUIDs.SuspendLayout();
            pnlCpExcludedGroupUUIDs.SuspendLayout();
            this.tblKprEntrySettingsTab.SuspendLayout();
            this.flpCheckboxes.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMstscParameters
            // 
            pnlMstscParameters.AutoSize = true;
            pnlMstscParameters.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            pnlMstscParameters.Controls.Add(this.lblMstscParameters);
            pnlMstscParameters.Controls.Add(this.cmdMstscParametersReset);
            pnlMstscParameters.Controls.Add(this.lstMstscParameters);
            pnlMstscParameters.Controls.Add(this.txtMstscParameters);
            pnlMstscParameters.Controls.Add(this.cmdMstscParametersRemove);
            pnlMstscParameters.Controls.Add(this.cmdMstscParametersAdd);
            pnlMstscParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlMstscParameters.Location = new System.Drawing.Point(250, 194);
            pnlMstscParameters.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            pnlMstscParameters.Name = "pnlMstscParameters";
            pnlMstscParameters.Size = new System.Drawing.Size(247, 153);
            pnlMstscParameters.TabIndex = 4;
            // 
            // lblMstscParameters
            // 
            this.lblMstscParameters.AutoSize = true;
            this.lblMstscParameters.Location = new System.Drawing.Point(4, 0);
            this.lblMstscParameters.Name = "lblMstscParameters";
            this.lblMstscParameters.Size = new System.Drawing.Size(175, 13);
            this.lblMstscParameters.TabIndex = 0;
            this.lblMstscParameters.Text = "Additional mstsc.exe parameters:";
            // 
            // cmdMstscParametersReset
            // 
            this.cmdMstscParametersReset.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdMstscParametersReset.AutoSize = true;
            this.cmdMstscParametersReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdMstscParametersReset.Enabled = false;
            this.cmdMstscParametersReset.ImageKey = "Refresh";
            this.cmdMstscParametersReset.ImageList = this.imageList1;
            this.cmdMstscParametersReset.Location = new System.Drawing.Point(221, 126);
            this.cmdMstscParametersReset.Name = "cmdMstscParametersReset";
            this.cmdMstscParametersReset.Padding = new System.Windows.Forms.Padding(1);
            this.cmdMstscParametersReset.Size = new System.Drawing.Size(24, 24);
            this.cmdMstscParametersReset.TabIndex = 5;
            ttGeneral.SetToolTip(this.cmdMstscParametersReset, "Reset mstsc.exe parameters to default");
            this.cmdMstscParametersReset.UseVisualStyleBackColor = true;
            this.cmdMstscParametersReset.Click += new System.EventHandler(this.cmdReset_Click);
            // 
            // lstMstscParameters
            // 
            this.lstMstscParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lstMstscParameters.FormattingEnabled = true;
            this.lstMstscParameters.HorizontalScrollbar = true;
            this.lstMstscParameters.IntegralHeight = false;
            this.lstMstscParameters.Location = new System.Drawing.Point(6, 45);
            this.lstMstscParameters.Name = "lstMstscParameters";
            this.lstMstscParameters.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstMstscParameters.Size = new System.Drawing.Size(211, 104);
            this.lstMstscParameters.TabIndex = 3;
            this.lstMstscParameters.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lst_KeyDown);
            // 
            // txtMstscParameters
            // 
            this.txtMstscParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMstscParameters.Location = new System.Drawing.Point(6, 19);
            this.txtMstscParameters.Name = "txtMstscParameters";
            this.txtMstscParameters.Size = new System.Drawing.Size(211, 22);
            this.txtMstscParameters.TabIndex = 1;
            this.txtMstscParameters.TextChanged += new System.EventHandler(this.txtMstscParameters_TextChanged);
            this.txtMstscParameters.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMstscParameters_KeyDown);
            this.txtMstscParameters.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.txt_PreviewKeyDown);
            // 
            // cmdMstscParametersRemove
            // 
            this.cmdMstscParametersRemove.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdMstscParametersRemove.AutoSize = true;
            this.cmdMstscParametersRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdMstscParametersRemove.Enabled = false;
            this.cmdMstscParametersRemove.ImageKey = "Remove";
            this.cmdMstscParametersRemove.ImageList = this.imageList1;
            this.cmdMstscParametersRemove.Location = new System.Drawing.Point(221, 44);
            this.cmdMstscParametersRemove.Name = "cmdMstscParametersRemove";
            this.cmdMstscParametersRemove.Padding = new System.Windows.Forms.Padding(1);
            this.cmdMstscParametersRemove.Size = new System.Drawing.Size(24, 24);
            this.cmdMstscParametersRemove.TabIndex = 4;
            ttGeneral.SetToolTip(this.cmdMstscParametersRemove, "Remove selected mstsc.exe parameter(s)");
            this.cmdMstscParametersRemove.UseVisualStyleBackColor = true;
            this.cmdMstscParametersRemove.Click += new System.EventHandler(this.cmdRemove_Click);
            // 
            // cmdMstscParametersAdd
            // 
            this.cmdMstscParametersAdd.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdMstscParametersAdd.AutoSize = true;
            this.cmdMstscParametersAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdMstscParametersAdd.Enabled = false;
            this.cmdMstscParametersAdd.ImageKey = "Add";
            this.cmdMstscParametersAdd.ImageList = this.imageList1;
            this.cmdMstscParametersAdd.Location = new System.Drawing.Point(221, 18);
            this.cmdMstscParametersAdd.Name = "cmdMstscParametersAdd";
            this.cmdMstscParametersAdd.Padding = new System.Windows.Forms.Padding(1);
            this.cmdMstscParametersAdd.Size = new System.Drawing.Size(24, 24);
            this.cmdMstscParametersAdd.TabIndex = 2;
            ttGeneral.SetToolTip(this.cmdMstscParametersAdd, "Add mstsc.exe parameter");
            this.cmdMstscParametersAdd.UseVisualStyleBackColor = true;
            this.cmdMstscParametersAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // pnlCpRegExPatterns
            // 
            pnlCpRegExPatterns.AutoSize = true;
            pnlCpRegExPatterns.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            pnlCpRegExPatterns.Controls.Add(this.lblCpRegExPatterns);
            pnlCpRegExPatterns.Controls.Add(this.cmdCpRegExPatternsReset);
            pnlCpRegExPatterns.Controls.Add(this.lstCpRegExPatterns);
            pnlCpRegExPatterns.Controls.Add(this.txtCpRegExPatterns);
            pnlCpRegExPatterns.Controls.Add(this.cmdCpRegExPatternsRemove);
            pnlCpRegExPatterns.Controls.Add(this.cmdCpRegExPatternsAdd);
            pnlCpRegExPatterns.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlCpRegExPatterns.Location = new System.Drawing.Point(3, 194);
            pnlCpRegExPatterns.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            pnlCpRegExPatterns.Name = "pnlCpRegExPatterns";
            pnlCpRegExPatterns.Size = new System.Drawing.Size(247, 153);
            pnlCpRegExPatterns.TabIndex = 3;
            // 
            // lblCpRegExPatterns
            // 
            this.lblCpRegExPatterns.AutoSize = true;
            this.lblCpRegExPatterns.Location = new System.Drawing.Point(4, 0);
            this.lblCpRegExPatterns.Name = "lblCpRegExPatterns";
            this.lblCpRegExPatterns.Size = new System.Drawing.Size(144, 13);
            this.lblCpRegExPatterns.TabIndex = 0;
            this.lblCpRegExPatterns.Text = "Additional RegEx patterns:";
            // 
            // cmdCpRegExPatternsReset
            // 
            this.cmdCpRegExPatternsReset.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCpRegExPatternsReset.AutoSize = true;
            this.cmdCpRegExPatternsReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCpRegExPatternsReset.Enabled = false;
            this.cmdCpRegExPatternsReset.ImageKey = "Refresh";
            this.cmdCpRegExPatternsReset.ImageList = this.imageList1;
            this.cmdCpRegExPatternsReset.Location = new System.Drawing.Point(221, 126);
            this.cmdCpRegExPatternsReset.Name = "cmdCpRegExPatternsReset";
            this.cmdCpRegExPatternsReset.Padding = new System.Windows.Forms.Padding(1);
            this.cmdCpRegExPatternsReset.Size = new System.Drawing.Size(24, 24);
            this.cmdCpRegExPatternsReset.TabIndex = 5;
            ttGeneral.SetToolTip(this.cmdCpRegExPatternsReset, "Reset RegEx patterns to default");
            this.cmdCpRegExPatternsReset.UseVisualStyleBackColor = true;
            this.cmdCpRegExPatternsReset.Click += new System.EventHandler(this.cmdReset_Click);
            // 
            // lstCpRegExPatterns
            // 
            this.lstCpRegExPatterns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCpRegExPatterns.FormattingEnabled = true;
            this.lstCpRegExPatterns.HorizontalScrollbar = true;
            this.lstCpRegExPatterns.IntegralHeight = false;
            this.lstCpRegExPatterns.Location = new System.Drawing.Point(6, 45);
            this.lstCpRegExPatterns.Name = "lstCpRegExPatterns";
            this.lstCpRegExPatterns.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstCpRegExPatterns.Size = new System.Drawing.Size(211, 104);
            this.lstCpRegExPatterns.TabIndex = 3;
            this.lstCpRegExPatterns.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lst_KeyDown);
            // 
            // txtCpRegExPatterns
            // 
            this.txtCpRegExPatterns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCpRegExPatterns.Location = new System.Drawing.Point(6, 19);
            this.txtCpRegExPatterns.Name = "txtCpRegExPatterns";
            this.txtCpRegExPatterns.Size = new System.Drawing.Size(211, 22);
            this.txtCpRegExPatterns.TabIndex = 1;
            this.txtCpRegExPatterns.TextChanged += new System.EventHandler(this.txtCpRegExPatterns_TextChanged);
            this.txtCpRegExPatterns.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCpRegExPatterns_KeyDown);
            this.txtCpRegExPatterns.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.txt_PreviewKeyDown);
            // 
            // cmdCpRegExPatternsRemove
            // 
            this.cmdCpRegExPatternsRemove.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCpRegExPatternsRemove.AutoSize = true;
            this.cmdCpRegExPatternsRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCpRegExPatternsRemove.Enabled = false;
            this.cmdCpRegExPatternsRemove.ImageKey = "Remove";
            this.cmdCpRegExPatternsRemove.ImageList = this.imageList1;
            this.cmdCpRegExPatternsRemove.Location = new System.Drawing.Point(221, 44);
            this.cmdCpRegExPatternsRemove.Name = "cmdCpRegExPatternsRemove";
            this.cmdCpRegExPatternsRemove.Padding = new System.Windows.Forms.Padding(1);
            this.cmdCpRegExPatternsRemove.Size = new System.Drawing.Size(24, 24);
            this.cmdCpRegExPatternsRemove.TabIndex = 4;
            ttGeneral.SetToolTip(this.cmdCpRegExPatternsRemove, "Remove selected RegEx pattern(s)");
            this.cmdCpRegExPatternsRemove.UseVisualStyleBackColor = true;
            this.cmdCpRegExPatternsRemove.Click += new System.EventHandler(this.cmdRemove_Click);
            // 
            // cmdCpRegExPatternsAdd
            // 
            this.cmdCpRegExPatternsAdd.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCpRegExPatternsAdd.AutoSize = true;
            this.cmdCpRegExPatternsAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCpRegExPatternsAdd.Enabled = false;
            this.cmdCpRegExPatternsAdd.ImageKey = "Add";
            this.cmdCpRegExPatternsAdd.ImageList = this.imageList1;
            this.cmdCpRegExPatternsAdd.Location = new System.Drawing.Point(221, 18);
            this.cmdCpRegExPatternsAdd.Name = "cmdCpRegExPatternsAdd";
            this.cmdCpRegExPatternsAdd.Padding = new System.Windows.Forms.Padding(1);
            this.cmdCpRegExPatternsAdd.Size = new System.Drawing.Size(24, 24);
            this.cmdCpRegExPatternsAdd.TabIndex = 2;
            ttGeneral.SetToolTip(this.cmdCpRegExPatternsAdd, "Add RegEx pattern");
            this.cmdCpRegExPatternsAdd.UseVisualStyleBackColor = true;
            this.cmdCpRegExPatternsAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // pnlCpGroupUUIDs
            // 
            pnlCpGroupUUIDs.AutoSize = true;
            pnlCpGroupUUIDs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            pnlCpGroupUUIDs.Controls.Add(this.lblCpGroupUUIDs);
            pnlCpGroupUUIDs.Controls.Add(this.lstCpGroupUUIDs);
            pnlCpGroupUUIDs.Controls.Add(this.txtCpGroupUUIDs);
            pnlCpGroupUUIDs.Controls.Add(this.cmdCpGroupUUIDsAdd);
            pnlCpGroupUUIDs.Controls.Add(this.cmdCpGroupUUIDsRemove);
            pnlCpGroupUUIDs.Controls.Add(this.cmdCpGroupUUIDsReset);
            pnlCpGroupUUIDs.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlCpGroupUUIDs.Location = new System.Drawing.Point(3, 35);
            pnlCpGroupUUIDs.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            pnlCpGroupUUIDs.Name = "pnlCpGroupUUIDs";
            pnlCpGroupUUIDs.Size = new System.Drawing.Size(247, 153);
            pnlCpGroupUUIDs.TabIndex = 1;
            // 
            // lblCpGroupUUIDs
            // 
            this.lblCpGroupUUIDs.AutoSize = true;
            this.lblCpGroupUUIDs.Location = new System.Drawing.Point(4, 0);
            this.lblCpGroupUUIDs.Name = "lblCpGroupUUIDs";
            this.lblCpGroupUUIDs.Size = new System.Drawing.Size(125, 13);
            this.lblCpGroupUUIDs.TabIndex = 0;
            this.lblCpGroupUUIDs.Text = "Included group UUIDs:";
            // 
            // lstCpGroupUUIDs
            // 
            this.lstCpGroupUUIDs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCpGroupUUIDs.FormattingEnabled = true;
            this.lstCpGroupUUIDs.HorizontalScrollbar = true;
            this.lstCpGroupUUIDs.IntegralHeight = false;
            this.lstCpGroupUUIDs.Location = new System.Drawing.Point(6, 45);
            this.lstCpGroupUUIDs.Name = "lstCpGroupUUIDs";
            this.lstCpGroupUUIDs.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstCpGroupUUIDs.Size = new System.Drawing.Size(211, 104);
            this.lstCpGroupUUIDs.TabIndex = 3;
            this.lstCpGroupUUIDs.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lst_KeyDown);
            // 
            // txtCpGroupUUIDs
            // 
            this.txtCpGroupUUIDs.AcceptsReturn = true;
            this.txtCpGroupUUIDs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCpGroupUUIDs.Location = new System.Drawing.Point(6, 19);
            this.txtCpGroupUUIDs.Name = "txtCpGroupUUIDs";
            this.txtCpGroupUUIDs.Size = new System.Drawing.Size(211, 22);
            this.txtCpGroupUUIDs.TabIndex = 1;
            this.txtCpGroupUUIDs.TextChanged += new System.EventHandler(this.txtCpGroupUUIDs_TextChanged);
            this.txtCpGroupUUIDs.Enter += new System.EventHandler(this.txtGroupUUIDs_Enter);
            this.txtCpGroupUUIDs.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCpGroupUUIDs_KeyDown);
            this.txtCpGroupUUIDs.Leave += new System.EventHandler(this.txtGroupUUIDs_Leave);
            this.txtCpGroupUUIDs.MouseEnter += new System.EventHandler(this.txtGroupUUIDs_MouseEnter);
            this.txtCpGroupUUIDs.MouseLeave += new System.EventHandler(this.txtGroupUUIDs_MouseLeave);
            this.txtCpGroupUUIDs.MouseMove += new System.Windows.Forms.MouseEventHandler(this.txtGroupUUIDs_MouseMove);
            this.txtCpGroupUUIDs.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.txt_PreviewKeyDown);
            // 
            // cmdCpGroupUUIDsAdd
            // 
            this.cmdCpGroupUUIDsAdd.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCpGroupUUIDsAdd.AutoSize = true;
            this.cmdCpGroupUUIDsAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCpGroupUUIDsAdd.Enabled = false;
            this.cmdCpGroupUUIDsAdd.ImageKey = "Add";
            this.cmdCpGroupUUIDsAdd.ImageList = this.imageList1;
            this.cmdCpGroupUUIDsAdd.Location = new System.Drawing.Point(221, 18);
            this.cmdCpGroupUUIDsAdd.Name = "cmdCpGroupUUIDsAdd";
            this.cmdCpGroupUUIDsAdd.Padding = new System.Windows.Forms.Padding(1);
            this.cmdCpGroupUUIDsAdd.Size = new System.Drawing.Size(24, 24);
            this.cmdCpGroupUUIDsAdd.TabIndex = 2;
            ttGeneral.SetToolTip(this.cmdCpGroupUUIDsAdd, "Add included group");
            this.cmdCpGroupUUIDsAdd.UseVisualStyleBackColor = true;
            this.cmdCpGroupUUIDsAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // cmdCpGroupUUIDsRemove
            // 
            this.cmdCpGroupUUIDsRemove.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCpGroupUUIDsRemove.AutoSize = true;
            this.cmdCpGroupUUIDsRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCpGroupUUIDsRemove.Enabled = false;
            this.cmdCpGroupUUIDsRemove.ImageKey = "Remove";
            this.cmdCpGroupUUIDsRemove.ImageList = this.imageList1;
            this.cmdCpGroupUUIDsRemove.Location = new System.Drawing.Point(221, 44);
            this.cmdCpGroupUUIDsRemove.Name = "cmdCpGroupUUIDsRemove";
            this.cmdCpGroupUUIDsRemove.Padding = new System.Windows.Forms.Padding(1);
            this.cmdCpGroupUUIDsRemove.Size = new System.Drawing.Size(24, 24);
            this.cmdCpGroupUUIDsRemove.TabIndex = 4;
            ttGeneral.SetToolTip(this.cmdCpGroupUUIDsRemove, "Remove selected included group(s)");
            this.cmdCpGroupUUIDsRemove.UseVisualStyleBackColor = true;
            this.cmdCpGroupUUIDsRemove.Click += new System.EventHandler(this.cmdRemove_Click);
            // 
            // cmdCpGroupUUIDsReset
            // 
            this.cmdCpGroupUUIDsReset.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCpGroupUUIDsReset.AutoSize = true;
            this.cmdCpGroupUUIDsReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCpGroupUUIDsReset.Enabled = false;
            this.cmdCpGroupUUIDsReset.ImageKey = "Refresh";
            this.cmdCpGroupUUIDsReset.ImageList = this.imageList1;
            this.cmdCpGroupUUIDsReset.Location = new System.Drawing.Point(221, 126);
            this.cmdCpGroupUUIDsReset.Name = "cmdCpGroupUUIDsReset";
            this.cmdCpGroupUUIDsReset.Padding = new System.Windows.Forms.Padding(1);
            this.cmdCpGroupUUIDsReset.Size = new System.Drawing.Size(24, 24);
            this.cmdCpGroupUUIDsReset.TabIndex = 5;
            ttGeneral.SetToolTip(this.cmdCpGroupUUIDsReset, "Reset included groups to default");
            this.cmdCpGroupUUIDsReset.UseVisualStyleBackColor = true;
            this.cmdCpGroupUUIDsReset.Click += new System.EventHandler(this.cmdReset_Click);
            // 
            // pnlCpExcludedGroupUUIDs
            // 
            pnlCpExcludedGroupUUIDs.AutoSize = true;
            pnlCpExcludedGroupUUIDs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            pnlCpExcludedGroupUUIDs.Controls.Add(this.lblCpExcludedGroupUUIDs);
            pnlCpExcludedGroupUUIDs.Controls.Add(this.cmdCpExcludedGroupUUIDsReset);
            pnlCpExcludedGroupUUIDs.Controls.Add(this.lstCpExcludedGroupUUIDs);
            pnlCpExcludedGroupUUIDs.Controls.Add(this.txtCpExcludedGroupUUIDs);
            pnlCpExcludedGroupUUIDs.Controls.Add(this.cmdCpExcludedGroupUUIDsRemove);
            pnlCpExcludedGroupUUIDs.Controls.Add(this.cmdCpExcludedGroupUUIDsAdd);
            pnlCpExcludedGroupUUIDs.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlCpExcludedGroupUUIDs.Location = new System.Drawing.Point(250, 35);
            pnlCpExcludedGroupUUIDs.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            pnlCpExcludedGroupUUIDs.Name = "pnlCpExcludedGroupUUIDs";
            pnlCpExcludedGroupUUIDs.Size = new System.Drawing.Size(247, 153);
            pnlCpExcludedGroupUUIDs.TabIndex = 2;
            // 
            // lblCpExcludedGroupUUIDs
            // 
            this.lblCpExcludedGroupUUIDs.AutoSize = true;
            this.lblCpExcludedGroupUUIDs.Location = new System.Drawing.Point(4, 0);
            this.lblCpExcludedGroupUUIDs.Name = "lblCpExcludedGroupUUIDs";
            this.lblCpExcludedGroupUUIDs.Size = new System.Drawing.Size(126, 13);
            this.lblCpExcludedGroupUUIDs.TabIndex = 0;
            this.lblCpExcludedGroupUUIDs.Text = "Excluded group UUIDs:";
            // 
            // cmdCpExcludedGroupUUIDsReset
            // 
            this.cmdCpExcludedGroupUUIDsReset.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCpExcludedGroupUUIDsReset.AutoSize = true;
            this.cmdCpExcludedGroupUUIDsReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCpExcludedGroupUUIDsReset.Enabled = false;
            this.cmdCpExcludedGroupUUIDsReset.ImageKey = "Refresh";
            this.cmdCpExcludedGroupUUIDsReset.ImageList = this.imageList1;
            this.cmdCpExcludedGroupUUIDsReset.Location = new System.Drawing.Point(221, 126);
            this.cmdCpExcludedGroupUUIDsReset.Name = "cmdCpExcludedGroupUUIDsReset";
            this.cmdCpExcludedGroupUUIDsReset.Padding = new System.Windows.Forms.Padding(1);
            this.cmdCpExcludedGroupUUIDsReset.Size = new System.Drawing.Size(24, 24);
            this.cmdCpExcludedGroupUUIDsReset.TabIndex = 5;
            ttGeneral.SetToolTip(this.cmdCpExcludedGroupUUIDsReset, "Reset excluded groups to default");
            this.cmdCpExcludedGroupUUIDsReset.UseVisualStyleBackColor = true;
            this.cmdCpExcludedGroupUUIDsReset.Click += new System.EventHandler(this.cmdReset_Click);
            // 
            // lstCpExcludedGroupUUIDs
            // 
            this.lstCpExcludedGroupUUIDs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCpExcludedGroupUUIDs.FormattingEnabled = true;
            this.lstCpExcludedGroupUUIDs.HorizontalScrollbar = true;
            this.lstCpExcludedGroupUUIDs.IntegralHeight = false;
            this.lstCpExcludedGroupUUIDs.Location = new System.Drawing.Point(6, 45);
            this.lstCpExcludedGroupUUIDs.Name = "lstCpExcludedGroupUUIDs";
            this.lstCpExcludedGroupUUIDs.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstCpExcludedGroupUUIDs.Size = new System.Drawing.Size(211, 104);
            this.lstCpExcludedGroupUUIDs.TabIndex = 3;
            this.lstCpExcludedGroupUUIDs.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lst_KeyDown);
            // 
            // txtCpExcludedGroupUUIDs
            // 
            this.txtCpExcludedGroupUUIDs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCpExcludedGroupUUIDs.Location = new System.Drawing.Point(6, 19);
            this.txtCpExcludedGroupUUIDs.Name = "txtCpExcludedGroupUUIDs";
            this.txtCpExcludedGroupUUIDs.Size = new System.Drawing.Size(211, 22);
            this.txtCpExcludedGroupUUIDs.TabIndex = 1;
            this.txtCpExcludedGroupUUIDs.TextChanged += new System.EventHandler(this.txtCpExcludedGroupUUIDs_TextChanged);
            this.txtCpExcludedGroupUUIDs.Enter += new System.EventHandler(this.txtGroupUUIDs_Enter);
            this.txtCpExcludedGroupUUIDs.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCpExcludedGroupUUIDs_KeyDown);
            this.txtCpExcludedGroupUUIDs.Leave += new System.EventHandler(this.txtGroupUUIDs_Leave);
            this.txtCpExcludedGroupUUIDs.MouseEnter += new System.EventHandler(this.txtGroupUUIDs_MouseEnter);
            this.txtCpExcludedGroupUUIDs.MouseLeave += new System.EventHandler(this.txtGroupUUIDs_MouseLeave);
            this.txtCpExcludedGroupUUIDs.MouseMove += new System.Windows.Forms.MouseEventHandler(this.txtGroupUUIDs_MouseMove);
            this.txtCpExcludedGroupUUIDs.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.txt_PreviewKeyDown);
            // 
            // cmdCpExcludedGroupUUIDsRemove
            // 
            this.cmdCpExcludedGroupUUIDsRemove.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCpExcludedGroupUUIDsRemove.AutoSize = true;
            this.cmdCpExcludedGroupUUIDsRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCpExcludedGroupUUIDsRemove.Enabled = false;
            this.cmdCpExcludedGroupUUIDsRemove.ImageKey = "Remove";
            this.cmdCpExcludedGroupUUIDsRemove.ImageList = this.imageList1;
            this.cmdCpExcludedGroupUUIDsRemove.Location = new System.Drawing.Point(221, 44);
            this.cmdCpExcludedGroupUUIDsRemove.Name = "cmdCpExcludedGroupUUIDsRemove";
            this.cmdCpExcludedGroupUUIDsRemove.Padding = new System.Windows.Forms.Padding(1);
            this.cmdCpExcludedGroupUUIDsRemove.Size = new System.Drawing.Size(24, 24);
            this.cmdCpExcludedGroupUUIDsRemove.TabIndex = 4;
            ttGeneral.SetToolTip(this.cmdCpExcludedGroupUUIDsRemove, "Remove selected excluded group(s)");
            this.cmdCpExcludedGroupUUIDsRemove.UseVisualStyleBackColor = true;
            this.cmdCpExcludedGroupUUIDsRemove.Click += new System.EventHandler(this.cmdRemove_Click);
            // 
            // cmdCpExcludedGroupUUIDsAdd
            // 
            this.cmdCpExcludedGroupUUIDsAdd.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCpExcludedGroupUUIDsAdd.AutoSize = true;
            this.cmdCpExcludedGroupUUIDsAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCpExcludedGroupUUIDsAdd.Enabled = false;
            this.cmdCpExcludedGroupUUIDsAdd.ImageKey = "Add";
            this.cmdCpExcludedGroupUUIDsAdd.ImageList = this.imageList1;
            this.cmdCpExcludedGroupUUIDsAdd.Location = new System.Drawing.Point(221, 18);
            this.cmdCpExcludedGroupUUIDsAdd.Name = "cmdCpExcludedGroupUUIDsAdd";
            this.cmdCpExcludedGroupUUIDsAdd.Padding = new System.Windows.Forms.Padding(1);
            this.cmdCpExcludedGroupUUIDsAdd.Size = new System.Drawing.Size(24, 24);
            this.cmdCpExcludedGroupUUIDsAdd.TabIndex = 2;
            ttGeneral.SetToolTip(this.cmdCpExcludedGroupUUIDsAdd, "Add excluded group");
            this.cmdCpExcludedGroupUUIDsAdd.UseVisualStyleBackColor = true;
            this.cmdCpExcludedGroupUUIDsAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // ttGeneral
            // 
            ttGeneral.AutoPopDelay = 10000;
            ttGeneral.InitialDelay = 500;
            ttGeneral.ReshowDelay = 100;
            // 
            // cbIgnore
            // 
            this.cbIgnore.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbIgnore.AutoSize = true;
            this.cbIgnore.Location = new System.Drawing.Point(0, 4);
            this.cbIgnore.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.cbIgnore.Name = "cbIgnore";
            this.cbIgnore.Size = new System.Drawing.Size(60, 17);
            this.cbIgnore.TabIndex = 0;
            this.cbIgnore.Text = "Ignore";
            ttGeneral.SetToolTip(this.cbIgnore, "Always hide entry from credential picker if checked.");
            this.cbIgnore.UseVisualStyleBackColor = true;
            this.cbIgnore.CheckedChanged += new System.EventHandler(this.cbIgnore_CheckedChanged);
            // 
            // cbUseCredpicker
            // 
            this.cbUseCredpicker.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbUseCredpicker.AutoSize = true;
            this.cbUseCredpicker.Location = new System.Drawing.Point(61, 4);
            this.cbUseCredpicker.Margin = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.cbUseCredpicker.Name = "cbUseCredpicker";
            this.cbUseCredpicker.Size = new System.Drawing.Size(133, 17);
            this.cbUseCredpicker.TabIndex = 1;
            this.cbUseCredpicker.Text = "Use credential picker";
            ttGeneral.SetToolTip(this.cbUseCredpicker, "Never open credential picker for entry if unchecked.");
            this.cbUseCredpicker.UseVisualStyleBackColor = true;
            this.cbUseCredpicker.CheckedChanged += new System.EventHandler(this.cbUseCredpicker_CheckedChanged);
            // 
            // cbCpRecurseGroups
            // 
            this.cbCpRecurseGroups.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbCpRecurseGroups.AutoSize = true;
            this.cbCpRecurseGroups.Location = new System.Drawing.Point(195, 4);
            this.cbCpRecurseGroups.Margin = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.cbCpRecurseGroups.Name = "cbCpRecurseGroups";
            this.cbCpRecurseGroups.Size = new System.Drawing.Size(145, 17);
            this.cbCpRecurseGroups.TabIndex = 3;
            this.cbCpRecurseGroups.Text = "Recursive group search";
            ttGeneral.SetToolTip(this.cbCpRecurseGroups, "Enable recursive searching through all childs of entrys parent group.");
            this.cbCpRecurseGroups.UseVisualStyleBackColor = true;
            this.cbCpRecurseGroups.CheckedChanged += new System.EventHandler(this.cbCpRecurseGroups_CheckedChanged);
            // 
            // btnMore
            // 
            this.btnMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMore.FlatAppearance.BorderSize = 0;
            this.btnMore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMore.ImageKey = "Expander";
            this.btnMore.ImageList = this.imageList1;
            this.btnMore.Location = new System.Drawing.Point(472, 6);
            this.btnMore.Name = "btnMore";
            this.btnMore.Padding = new System.Windows.Forms.Padding(0, 1, 1, 2);
            this.btnMore.Size = new System.Drawing.Size(22, 18);
            this.btnMore.TabIndex = 5;
            ttGeneral.SetToolTip(this.btnMore, "Show more settings");
            this.btnMore.UseVisualStyleBackColor = true;
            this.btnMore.Click += new System.EventHandler(this.btnMore_Click);
            // 
            // btnRdpFile
            // 
            this.btnRdpFile.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnRdpFile.AutoSize = true;
            this.btnRdpFile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRdpFile.BackColor = System.Drawing.Color.Gainsboro;
            this.btnRdpFile.FlatAppearance.BorderSize = 0;
            this.btnRdpFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRdpFile.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRdpFile.ImageKey = "RemoteDesktop";
            this.btnRdpFile.ImageList = this.imageList1;
            this.btnRdpFile.Location = new System.Drawing.Point(340, 0);
            this.btnRdpFile.Margin = new System.Windows.Forms.Padding(0, 0, 1, 0);
            this.btnRdpFile.Name = "btnRdpFile";
            this.btnRdpFile.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.btnRdpFile.Size = new System.Drawing.Size(22, 23);
            this.btnRdpFile.TabIndex = 4;
            ttGeneral.SetToolTip(this.btnRdpFile, "Configure extended settings via .rdp file.");
            this.btnRdpFile.UseVisualStyleBackColor = false;
            this.btnRdpFile.Click += new System.EventHandler(this.btnRdpFile_Click);
            // 
            // tblKprEntrySettingsTab
            // 
            this.tblKprEntrySettingsTab.ColumnCount = 2;
            this.tblKprEntrySettingsTab.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblKprEntrySettingsTab.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblKprEntrySettingsTab.Controls.Add(pnlMstscParameters, 1, 2);
            this.tblKprEntrySettingsTab.Controls.Add(pnlCpRegExPatterns, 0, 2);
            this.tblKprEntrySettingsTab.Controls.Add(this.flpCheckboxes, 0, 0);
            this.tblKprEntrySettingsTab.Controls.Add(pnlCpGroupUUIDs, 0, 1);
            this.tblKprEntrySettingsTab.Controls.Add(pnlCpExcludedGroupUUIDs, 1, 1);
            this.tblKprEntrySettingsTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblKprEntrySettingsTab.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tblKprEntrySettingsTab.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tblKprEntrySettingsTab.Location = new System.Drawing.Point(0, 0);
            this.tblKprEntrySettingsTab.Name = "tblKprEntrySettingsTab";
            this.tblKprEntrySettingsTab.RowCount = 3;
            this.tblKprEntrySettingsTab.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblKprEntrySettingsTab.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblKprEntrySettingsTab.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblKprEntrySettingsTab.Size = new System.Drawing.Size(500, 350);
            this.tblKprEntrySettingsTab.TabIndex = 0;
            // 
            // flpCheckboxes
            // 
            this.flpCheckboxes.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.flpCheckboxes.AutoSize = true;
            this.flpCheckboxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tblKprEntrySettingsTab.SetColumnSpan(this.flpCheckboxes, 2);
            this.flpCheckboxes.Controls.Add(this.cbIgnore);
            this.flpCheckboxes.Controls.Add(this.cbUseCredpicker);
            this.flpCheckboxes.Controls.Add(this.cbCpRecurseGroups);
            this.flpCheckboxes.Controls.Add(this.btnRdpFile);
            this.flpCheckboxes.Location = new System.Drawing.Point(68, 6);
            this.flpCheckboxes.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.flpCheckboxes.Name = "flpCheckboxes";
            this.flpCheckboxes.Size = new System.Drawing.Size(363, 23);
            this.flpCheckboxes.TabIndex = 0;
            // 
            // ttGroupPicker
            // 
            this.ttGroupPicker.AutoPopDelay = 10000;
            this.ttGroupPicker.InitialDelay = 500;
            this.ttGroupPicker.ReshowDelay = 100;
            // 
            // cmsMore
            // 
            this.cmsMore.AutoClose = false;
            this.cmsMore.DropShadowEnabled = false;
            this.cmsMore.Name = "cmsMore";
            this.cmsMore.ShowImageMargin = false;
            this.cmsMore.Size = new System.Drawing.Size(36, 4);
            // 
            // KprEntrySettingsTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.btnMore);
            this.Controls.Add(this.tblKprEntrySettingsTab);
            this.Name = "KprEntrySettingsTab";
            this.Size = new System.Drawing.Size(500, 350);
            this.Load += new System.EventHandler(this.KprEntrySettingsTab_Load);
            pnlMstscParameters.ResumeLayout(false);
            pnlMstscParameters.PerformLayout();
            pnlCpRegExPatterns.ResumeLayout(false);
            pnlCpRegExPatterns.PerformLayout();
            pnlCpGroupUUIDs.ResumeLayout(false);
            pnlCpGroupUUIDs.PerformLayout();
            pnlCpExcludedGroupUUIDs.ResumeLayout(false);
            pnlCpExcludedGroupUUIDs.PerformLayout();
            this.tblKprEntrySettingsTab.ResumeLayout(false);
            this.tblKprEntrySettingsTab.PerformLayout();
            this.flpCheckboxes.ResumeLayout(false);
            this.flpCheckboxes.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox cbIgnore;
        private System.Windows.Forms.CheckBox cbUseCredpicker;
        private System.Windows.Forms.CheckBox cbCpRecurseGroups;
        private System.Windows.Forms.TableLayoutPanel tblKprEntrySettingsTab;
        private System.Windows.Forms.FlowLayoutPanel flpCheckboxes;
        private System.Windows.Forms.Button cmdCpRegExPatternsReset;
        private System.Windows.Forms.ListBox lstCpRegExPatterns;
        private System.Windows.Forms.TextBox txtCpRegExPatterns;
        private System.Windows.Forms.Button cmdCpRegExPatternsRemove;
        private System.Windows.Forms.Button cmdCpRegExPatternsAdd;
        private System.Windows.Forms.ListBox lstCpGroupUUIDs;
        private System.Windows.Forms.TextBox txtCpGroupUUIDs;
        private System.Windows.Forms.Button cmdCpGroupUUIDsAdd;
        private System.Windows.Forms.Button cmdCpGroupUUIDsRemove;
        private System.Windows.Forms.Button cmdCpGroupUUIDsReset;
        private System.Windows.Forms.Button cmdCpExcludedGroupUUIDsReset;
        private System.Windows.Forms.ListBox lstCpExcludedGroupUUIDs;
        private System.Windows.Forms.TextBox txtCpExcludedGroupUUIDs;
        private System.Windows.Forms.Button cmdCpExcludedGroupUUIDsRemove;
        private System.Windows.Forms.Button cmdCpExcludedGroupUUIDsAdd;
        private System.Windows.Forms.Button cmdMstscParametersReset;
        private System.Windows.Forms.ListBox lstMstscParameters;
        private System.Windows.Forms.TextBox txtMstscParameters;
        private System.Windows.Forms.Button cmdMstscParametersRemove;
        private System.Windows.Forms.Button cmdMstscParametersAdd;
        private KprToolTip ttGroupPicker;
        private System.Windows.Forms.Label lblMstscParameters;
        private System.Windows.Forms.Label lblCpRegExPatterns;
        private System.Windows.Forms.Label lblCpGroupUUIDs;
        private System.Windows.Forms.Label lblCpExcludedGroupUUIDs;
        private System.Windows.Forms.ContextMenuStrip cmsMore;
        private System.Windows.Forms.Button btnMore;
        private System.Windows.Forms.Button btnRdpFile;
        private System.Windows.Forms.ImageList imageList1;
        private KprImageList kprImageList;
    }
}