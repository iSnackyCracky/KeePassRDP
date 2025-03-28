namespace KeePassRDP
{
    partial class KprSettingsControl
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Panel pnlKeePassToolbarItemsCmds;
            System.Windows.Forms.Panel pnlKeePassContextMenuItemsCmds;
            this.cmdKeePassToolbarItemsReset = new System.Windows.Forms.Button();
            this.kprImageList = new KeePassRDP.KprImageList(this.components);
            this.imageList1 = ((System.Windows.Forms.ImageList)(this.kprImageList));
            this.cmdKeePassToolbarItemsAdd = new System.Windows.Forms.Button();
            this.cmdKeePassToolbarItemsRemove = new System.Windows.Forms.Button();
            this.cmdKeePassContextMenuItemsReset = new System.Windows.Forms.Button();
            this.cmdKeePassContextMenuItemsAdd = new System.Windows.Forms.Button();
            this.cmdKeePassContextMenuItemsRemove = new System.Windows.Forms.Button();
            this.tblKeyboardSettings = new KeePassRDP.KprSettingsControl.KprKeyboardSettingsTableLayoutPanel();
            this.lblKeyboardSettings = new System.Windows.Forms.Label();
            this.lblKeyboardShortcut = new System.Windows.Forms.Label();
            this.tblVisibilitySettings = new System.Windows.Forms.TableLayoutPanel();
            this.lblVisibilitySettings = new System.Windows.Forms.Label();
            this.tblKeePassToolbarItems = new System.Windows.Forms.TableLayoutPanel();
            this.lstKeePassToolbarItemsAvailable = new System.Windows.Forms.ListBox();
            this.lstKeePassToolbarItems = new System.Windows.Forms.ListBox();
            this.lblKeePassContextMenuItems = new System.Windows.Forms.Label();
            this.lblKeePassToolbarItems = new System.Windows.Forms.Label();
            this.tblKeePassContextMenuItems = new System.Windows.Forms.TableLayoutPanel();
            this.lstKeePassContextMenuItemsAvailable = new System.Windows.Forms.ListBox();
            this.lstKeePassContextMenuItems = new System.Windows.Forms.ListBox();
            this.ttGeneric = new KeePassRDP.KprToolTip(this.components);
            pnlKeePassToolbarItemsCmds = new System.Windows.Forms.Panel();
            pnlKeePassContextMenuItemsCmds = new System.Windows.Forms.Panel();
            pnlKeePassToolbarItemsCmds.SuspendLayout();
            pnlKeePassContextMenuItemsCmds.SuspendLayout();
            this.tblKeyboardSettings.SuspendLayout();
            this.tblVisibilitySettings.SuspendLayout();
            this.tblKeePassToolbarItems.SuspendLayout();
            this.tblKeePassContextMenuItems.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlKeePassToolbarItemsCmds
            // 
            pnlKeePassToolbarItemsCmds.Anchor = System.Windows.Forms.AnchorStyles.None;
            pnlKeePassToolbarItemsCmds.AutoSize = true;
            pnlKeePassToolbarItemsCmds.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            pnlKeePassToolbarItemsCmds.Controls.Add(this.cmdKeePassToolbarItemsReset);
            pnlKeePassToolbarItemsCmds.Controls.Add(this.cmdKeePassToolbarItemsAdd);
            pnlKeePassToolbarItemsCmds.Controls.Add(this.cmdKeePassToolbarItemsRemove);
            pnlKeePassToolbarItemsCmds.Location = new System.Drawing.Point(125, 8);
            pnlKeePassToolbarItemsCmds.Margin = new System.Windows.Forms.Padding(0);
            pnlKeePassToolbarItemsCmds.Name = "pnlKeePassToolbarItemsCmds";
            pnlKeePassToolbarItemsCmds.Size = new System.Drawing.Size(24, 76);
            pnlKeePassToolbarItemsCmds.TabIndex = 0;
            // 
            // cmdKeePassToolbarItemsReset
            // 
            this.cmdKeePassToolbarItemsReset.AutoSize = true;
            this.cmdKeePassToolbarItemsReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdKeePassToolbarItemsReset.ImageKey = "Refresh";
            this.cmdKeePassToolbarItemsReset.ImageList = this.imageList1;
            this.cmdKeePassToolbarItemsReset.Location = new System.Drawing.Point(0, 52);
            this.cmdKeePassToolbarItemsReset.Margin = new System.Windows.Forms.Padding(0);
            this.cmdKeePassToolbarItemsReset.Name = "cmdKeePassToolbarItemsReset";
            this.cmdKeePassToolbarItemsReset.Padding = new System.Windows.Forms.Padding(1);
            this.cmdKeePassToolbarItemsReset.Size = new System.Drawing.Size(24, 24);
            this.cmdKeePassToolbarItemsReset.TabIndex = 2;
            this.ttGeneric.SetToolTip(this.cmdKeePassToolbarItemsReset, "Reset toolbar items to default");
            this.cmdKeePassToolbarItemsReset.UseVisualStyleBackColor = true;
            this.cmdKeePassToolbarItemsReset.Click += new System.EventHandler(this.cmdKeePassToolbarItemsReset_Click);
            // 
            // cmdKeePassToolbarItemsAdd
            // 
            this.cmdKeePassToolbarItemsAdd.AutoSize = true;
            this.cmdKeePassToolbarItemsAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdKeePassToolbarItemsAdd.Enabled = false;
            this.cmdKeePassToolbarItemsAdd.ImageKey = "Add";
            this.cmdKeePassToolbarItemsAdd.ImageList = this.imageList1;
            this.cmdKeePassToolbarItemsAdd.Location = new System.Drawing.Point(0, 0);
            this.cmdKeePassToolbarItemsAdd.Margin = new System.Windows.Forms.Padding(0);
            this.cmdKeePassToolbarItemsAdd.Name = "cmdKeePassToolbarItemsAdd";
            this.cmdKeePassToolbarItemsAdd.Padding = new System.Windows.Forms.Padding(1);
            this.cmdKeePassToolbarItemsAdd.Size = new System.Drawing.Size(24, 24);
            this.cmdKeePassToolbarItemsAdd.TabIndex = 0;
            this.ttGeneric.SetToolTip(this.cmdKeePassToolbarItemsAdd, "Add selected toolbar item(s)");
            this.cmdKeePassToolbarItemsAdd.UseVisualStyleBackColor = true;
            this.cmdKeePassToolbarItemsAdd.Click += new System.EventHandler(this.cmdKeePassToolbarItemsAdd_Click);
            // 
            // cmdKeePassToolbarItemsRemove
            // 
            this.cmdKeePassToolbarItemsRemove.AutoSize = true;
            this.cmdKeePassToolbarItemsRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdKeePassToolbarItemsRemove.Enabled = false;
            this.cmdKeePassToolbarItemsRemove.ImageKey = "Remove";
            this.cmdKeePassToolbarItemsRemove.ImageList = this.imageList1;
            this.cmdKeePassToolbarItemsRemove.Location = new System.Drawing.Point(0, 26);
            this.cmdKeePassToolbarItemsRemove.Margin = new System.Windows.Forms.Padding(0);
            this.cmdKeePassToolbarItemsRemove.Name = "cmdKeePassToolbarItemsRemove";
            this.cmdKeePassToolbarItemsRemove.Padding = new System.Windows.Forms.Padding(1);
            this.cmdKeePassToolbarItemsRemove.Size = new System.Drawing.Size(24, 24);
            this.cmdKeePassToolbarItemsRemove.TabIndex = 1;
            this.ttGeneric.SetToolTip(this.cmdKeePassToolbarItemsRemove, "Remove selected toolbar item(s)");
            this.cmdKeePassToolbarItemsRemove.UseVisualStyleBackColor = true;
            this.cmdKeePassToolbarItemsRemove.Click += new System.EventHandler(this.cmdKeePassToolbarItemsRemove_Click);
            // 
            // pnlKeePassContextMenuItemsCmds
            // 
            pnlKeePassContextMenuItemsCmds.Anchor = System.Windows.Forms.AnchorStyles.None;
            pnlKeePassContextMenuItemsCmds.AutoSize = true;
            pnlKeePassContextMenuItemsCmds.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            pnlKeePassContextMenuItemsCmds.Controls.Add(this.cmdKeePassContextMenuItemsReset);
            pnlKeePassContextMenuItemsCmds.Controls.Add(this.cmdKeePassContextMenuItemsAdd);
            pnlKeePassContextMenuItemsCmds.Controls.Add(this.cmdKeePassContextMenuItemsRemove);
            pnlKeePassContextMenuItemsCmds.Location = new System.Drawing.Point(125, 8);
            pnlKeePassContextMenuItemsCmds.Margin = new System.Windows.Forms.Padding(0);
            pnlKeePassContextMenuItemsCmds.Name = "pnlKeePassContextMenuItemsCmds";
            pnlKeePassContextMenuItemsCmds.Size = new System.Drawing.Size(24, 76);
            pnlKeePassContextMenuItemsCmds.TabIndex = 0;
            // 
            // cmdKeePassContextMenuItemsReset
            // 
            this.cmdKeePassContextMenuItemsReset.AutoSize = true;
            this.cmdKeePassContextMenuItemsReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdKeePassContextMenuItemsReset.ImageKey = "Refresh";
            this.cmdKeePassContextMenuItemsReset.ImageList = this.imageList1;
            this.cmdKeePassContextMenuItemsReset.Location = new System.Drawing.Point(0, 52);
            this.cmdKeePassContextMenuItemsReset.Margin = new System.Windows.Forms.Padding(0);
            this.cmdKeePassContextMenuItemsReset.Name = "cmdKeePassContextMenuItemsReset";
            this.cmdKeePassContextMenuItemsReset.Padding = new System.Windows.Forms.Padding(1);
            this.cmdKeePassContextMenuItemsReset.Size = new System.Drawing.Size(24, 24);
            this.cmdKeePassContextMenuItemsReset.TabIndex = 2;
            this.ttGeneric.SetToolTip(this.cmdKeePassContextMenuItemsReset, "Reset context menu items to default");
            this.cmdKeePassContextMenuItemsReset.UseVisualStyleBackColor = true;
            this.cmdKeePassContextMenuItemsReset.Click += new System.EventHandler(this.cmdKeePassContextMenuItemsReset_Click);
            // 
            // cmdKeePassContextMenuItemsAdd
            // 
            this.cmdKeePassContextMenuItemsAdd.AutoSize = true;
            this.cmdKeePassContextMenuItemsAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdKeePassContextMenuItemsAdd.Enabled = false;
            this.cmdKeePassContextMenuItemsAdd.ImageKey = "Add";
            this.cmdKeePassContextMenuItemsAdd.ImageList = this.imageList1;
            this.cmdKeePassContextMenuItemsAdd.Location = new System.Drawing.Point(0, 0);
            this.cmdKeePassContextMenuItemsAdd.Margin = new System.Windows.Forms.Padding(0);
            this.cmdKeePassContextMenuItemsAdd.Name = "cmdKeePassContextMenuItemsAdd";
            this.cmdKeePassContextMenuItemsAdd.Padding = new System.Windows.Forms.Padding(1);
            this.cmdKeePassContextMenuItemsAdd.Size = new System.Drawing.Size(24, 24);
            this.cmdKeePassContextMenuItemsAdd.TabIndex = 0;
            this.ttGeneric.SetToolTip(this.cmdKeePassContextMenuItemsAdd, "Add selected context menu item(s)");
            this.cmdKeePassContextMenuItemsAdd.UseVisualStyleBackColor = true;
            this.cmdKeePassContextMenuItemsAdd.Click += new System.EventHandler(this.cmdKeePassContextMenuItemsAdd_Click);
            // 
            // cmdKeePassContextMenuItemsRemove
            // 
            this.cmdKeePassContextMenuItemsRemove.AutoSize = true;
            this.cmdKeePassContextMenuItemsRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdKeePassContextMenuItemsRemove.Enabled = false;
            this.cmdKeePassContextMenuItemsRemove.ImageKey = "Remove";
            this.cmdKeePassContextMenuItemsRemove.ImageList = this.imageList1;
            this.cmdKeePassContextMenuItemsRemove.Location = new System.Drawing.Point(0, 26);
            this.cmdKeePassContextMenuItemsRemove.Margin = new System.Windows.Forms.Padding(0);
            this.cmdKeePassContextMenuItemsRemove.Name = "cmdKeePassContextMenuItemsRemove";
            this.cmdKeePassContextMenuItemsRemove.Padding = new System.Windows.Forms.Padding(1);
            this.cmdKeePassContextMenuItemsRemove.Size = new System.Drawing.Size(24, 24);
            this.cmdKeePassContextMenuItemsRemove.TabIndex = 1;
            this.ttGeneric.SetToolTip(this.cmdKeePassContextMenuItemsRemove, "Remove selected context menu item(s)");
            this.cmdKeePassContextMenuItemsRemove.UseVisualStyleBackColor = true;
            this.cmdKeePassContextMenuItemsRemove.Click += new System.EventHandler(this.cmdKeePassContextMenuItemsRemove_Click);
            // 
            // tblKeyboardSettings
            // 
            this.tblKeyboardSettings.AutoSize = true;
            this.tblKeyboardSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tblKeyboardSettings.ColumnCount = 5;
            this.tblKeyboardSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57F));
            this.tblKeyboardSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblKeyboardSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblKeyboardSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 43F));
            this.tblKeyboardSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblKeyboardSettings.Controls.Add(this.lblKeyboardSettings, 0, 0);
            this.tblKeyboardSettings.Controls.Add(this.lblKeyboardShortcut, 3, 0);
            this.tblKeyboardSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.tblKeyboardSettings.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tblKeyboardSettings.Location = new System.Drawing.Point(0, 0);
            this.tblKeyboardSettings.Name = "tblKeyboardSettings";
            this.tblKeyboardSettings.RowCount = 9;
            this.tblKeyboardSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblKeyboardSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblKeyboardSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblKeyboardSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblKeyboardSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblKeyboardSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblKeyboardSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblKeyboardSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblKeyboardSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.tblKeyboardSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblKeyboardSettings.Size = new System.Drawing.Size(550, 221);
            this.tblKeyboardSettings.TabIndex = 0;
            this.tblKeyboardSettings.Visible = false;
            // 
            // lblKeyboardSettings
            // 
            this.lblKeyboardSettings.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKeyboardSettings.AutoSize = true;
            this.lblKeyboardSettings.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblKeyboardSettings.Location = new System.Drawing.Point(3, 3);
            this.lblKeyboardSettings.Margin = new System.Windows.Forms.Padding(3);
            this.lblKeyboardSettings.Name = "lblKeyboardSettings";
            this.lblKeyboardSettings.Size = new System.Drawing.Size(101, 13);
            this.lblKeyboardSettings.TabIndex = 37;
            this.lblKeyboardSettings.Text = "Keyboard settings";
            this.lblKeyboardSettings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKeyboardShortcut
            // 
            this.lblKeyboardShortcut.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKeyboardShortcut.AutoSize = true;
            this.lblKeyboardShortcut.Location = new System.Drawing.Point(373, 3);
            this.lblKeyboardShortcut.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.lblKeyboardShortcut.Name = "lblKeyboardShortcut";
            this.lblKeyboardShortcut.Size = new System.Drawing.Size(51, 13);
            this.lblKeyboardShortcut.TabIndex = 38;
            this.lblKeyboardShortcut.Text = "Shortcut";
            this.lblKeyboardShortcut.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ttGeneric.SetToolTip(this.lblKeyboardShortcut, "Press delete key to clear the hotkey.");
            // 
            // tblVisibilitySettings
            // 
            this.tblVisibilitySettings.AutoSize = true;
            this.tblVisibilitySettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tblVisibilitySettings.ColumnCount = 2;
            this.tblVisibilitySettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblVisibilitySettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblVisibilitySettings.Controls.Add(this.lblVisibilitySettings, 0, 0);
            this.tblVisibilitySettings.Controls.Add(this.tblKeePassToolbarItems, 1, 2);
            this.tblVisibilitySettings.Controls.Add(this.lblKeePassContextMenuItems, 0, 1);
            this.tblVisibilitySettings.Controls.Add(this.lblKeePassToolbarItems, 1, 1);
            this.tblVisibilitySettings.Controls.Add(this.tblKeePassContextMenuItems, 0, 2);
            this.tblVisibilitySettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblVisibilitySettings.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tblVisibilitySettings.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tblVisibilitySettings.Location = new System.Drawing.Point(0, 221);
            this.tblVisibilitySettings.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.tblVisibilitySettings.Name = "tblVisibilitySettings";
            this.tblVisibilitySettings.RowCount = 4;
            this.tblVisibilitySettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblVisibilitySettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblVisibilitySettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblVisibilitySettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.tblVisibilitySettings.Size = new System.Drawing.Size(550, 129);
            this.tblVisibilitySettings.TabIndex = 3;
            this.tblVisibilitySettings.Visible = false;
            // 
            // lblVisibilitySettings
            // 
            this.lblVisibilitySettings.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblVisibilitySettings.AutoSize = true;
            this.lblVisibilitySettings.BackColor = System.Drawing.Color.Transparent;
            this.tblVisibilitySettings.SetColumnSpan(this.lblVisibilitySettings, 2);
            this.lblVisibilitySettings.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVisibilitySettings.Location = new System.Drawing.Point(227, 3);
            this.lblVisibilitySettings.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.lblVisibilitySettings.Name = "lblVisibilitySettings";
            this.lblVisibilitySettings.Size = new System.Drawing.Size(95, 13);
            this.lblVisibilitySettings.TabIndex = 5;
            this.lblVisibilitySettings.Text = "Visibility settings";
            // 
            // tblKeePassToolbarItems
            // 
            this.tblKeePassToolbarItems.AutoSize = true;
            this.tblKeePassToolbarItems.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tblKeePassToolbarItems.ColumnCount = 3;
            this.tblKeePassToolbarItems.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblKeePassToolbarItems.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblKeePassToolbarItems.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblKeePassToolbarItems.Controls.Add(this.lstKeePassToolbarItemsAvailable, 0, 0);
            this.tblKeePassToolbarItems.Controls.Add(this.lstKeePassToolbarItems, 2, 0);
            this.tblKeePassToolbarItems.Controls.Add(pnlKeePassToolbarItemsCmds, 1, 0);
            this.tblKeePassToolbarItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblKeePassToolbarItems.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tblKeePassToolbarItems.Location = new System.Drawing.Point(275, 35);
            this.tblKeePassToolbarItems.Margin = new System.Windows.Forms.Padding(0);
            this.tblKeePassToolbarItems.MaximumSize = new System.Drawing.Size(0, 200);
            this.tblKeePassToolbarItems.MinimumSize = new System.Drawing.Size(0, 85);
            this.tblKeePassToolbarItems.Name = "tblKeePassToolbarItems";
            this.tblKeePassToolbarItems.RowCount = 2;
            this.tblKeePassToolbarItems.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblKeePassToolbarItems.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.tblKeePassToolbarItems.Size = new System.Drawing.Size(275, 93);
            this.tblKeePassToolbarItems.TabIndex = 4;
            // 
            // lstKeePassToolbarItemsAvailable
            // 
            this.lstKeePassToolbarItemsAvailable.DisplayMember = "Value";
            this.lstKeePassToolbarItemsAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstKeePassToolbarItemsAvailable.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.lstKeePassToolbarItemsAvailable.FormattingEnabled = true;
            this.lstKeePassToolbarItemsAvailable.IntegralHeight = false;
            this.lstKeePassToolbarItemsAvailable.Location = new System.Drawing.Point(3, 3);
            this.lstKeePassToolbarItemsAvailable.Name = "lstKeePassToolbarItemsAvailable";
            this.lstKeePassToolbarItemsAvailable.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstKeePassToolbarItemsAvailable.Size = new System.Drawing.Size(119, 86);
            this.lstKeePassToolbarItemsAvailable.TabIndex = 0;
            this.lstKeePassToolbarItemsAvailable.ValueMember = "Key";
            this.lstKeePassToolbarItemsAvailable.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lst_DrawItem);
            this.lstKeePassToolbarItemsAvailable.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.lst_MeasureItem);
            this.lstKeePassToolbarItemsAvailable.SelectedIndexChanged += new System.EventHandler(this.lstKeePassToolbarItemsAvailable_SelectedIndexChanged);
            this.lstKeePassToolbarItemsAvailable.SizeChanged += new System.EventHandler(this.lst_SizeChanged);
            this.lstKeePassToolbarItemsAvailable.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lst_KeyDownEnter);
            this.lstKeePassToolbarItemsAvailable.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.lst_PreviewKeyDown);
            // 
            // lstKeePassToolbarItems
            // 
            this.lstKeePassToolbarItems.DisplayMember = "Value";
            this.lstKeePassToolbarItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstKeePassToolbarItems.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.lstKeePassToolbarItems.FormattingEnabled = true;
            this.lstKeePassToolbarItems.IntegralHeight = false;
            this.lstKeePassToolbarItems.Location = new System.Drawing.Point(152, 3);
            this.lstKeePassToolbarItems.Name = "lstKeePassToolbarItems";
            this.lstKeePassToolbarItems.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstKeePassToolbarItems.Size = new System.Drawing.Size(120, 86);
            this.lstKeePassToolbarItems.TabIndex = 1;
            this.lstKeePassToolbarItems.ValueMember = "Key";
            this.lstKeePassToolbarItems.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lst_DrawItem);
            this.lstKeePassToolbarItems.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.lst_MeasureItem);
            this.lstKeePassToolbarItems.SelectedIndexChanged += new System.EventHandler(this.lstKeePassToolbarItems_SelectedIndexChanged);
            this.lstKeePassToolbarItems.SizeChanged += new System.EventHandler(this.lst_SizeChanged);
            this.lstKeePassToolbarItems.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lst_KeyDownDelete);
            // 
            // lblKeePassContextMenuItems
            // 
            this.lblKeePassContextMenuItems.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblKeePassContextMenuItems.AutoSize = true;
            this.lblKeePassContextMenuItems.BackColor = System.Drawing.Color.Transparent;
            this.lblKeePassContextMenuItems.Location = new System.Drawing.Point(98, 19);
            this.lblKeePassContextMenuItems.Margin = new System.Windows.Forms.Padding(3);
            this.lblKeePassContextMenuItems.Name = "lblKeePassContextMenuItems";
            this.lblKeePassContextMenuItems.Size = new System.Drawing.Size(79, 13);
            this.lblKeePassContextMenuItems.TabIndex = 1;
            this.lblKeePassContextMenuItems.Text = "Context menu";
            // 
            // lblKeePassToolbarItems
            // 
            this.lblKeePassToolbarItems.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblKeePassToolbarItems.AutoSize = true;
            this.lblKeePassToolbarItems.BackColor = System.Drawing.Color.Transparent;
            this.lblKeePassToolbarItems.Location = new System.Drawing.Point(389, 19);
            this.lblKeePassToolbarItems.Margin = new System.Windows.Forms.Padding(2, 3, 3, 3);
            this.lblKeePassToolbarItems.Name = "lblKeePassToolbarItems";
            this.lblKeePassToolbarItems.Size = new System.Drawing.Size(46, 13);
            this.lblKeePassToolbarItems.TabIndex = 2;
            this.lblKeePassToolbarItems.Text = "Toolbar";
            // 
            // tblKeePassContextMenuItems
            // 
            this.tblKeePassContextMenuItems.AutoSize = true;
            this.tblKeePassContextMenuItems.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tblKeePassContextMenuItems.ColumnCount = 3;
            this.tblKeePassContextMenuItems.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblKeePassContextMenuItems.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblKeePassContextMenuItems.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblKeePassContextMenuItems.Controls.Add(this.lstKeePassContextMenuItemsAvailable, 0, 0);
            this.tblKeePassContextMenuItems.Controls.Add(this.lstKeePassContextMenuItems, 2, 0);
            this.tblKeePassContextMenuItems.Controls.Add(pnlKeePassContextMenuItemsCmds, 1, 0);
            this.tblKeePassContextMenuItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblKeePassContextMenuItems.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tblKeePassContextMenuItems.Location = new System.Drawing.Point(0, 35);
            this.tblKeePassContextMenuItems.Margin = new System.Windows.Forms.Padding(0);
            this.tblKeePassContextMenuItems.MaximumSize = new System.Drawing.Size(0, 200);
            this.tblKeePassContextMenuItems.MinimumSize = new System.Drawing.Size(0, 85);
            this.tblKeePassContextMenuItems.Name = "tblKeePassContextMenuItems";
            this.tblKeePassContextMenuItems.RowCount = 2;
            this.tblKeePassContextMenuItems.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblKeePassContextMenuItems.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.tblKeePassContextMenuItems.Size = new System.Drawing.Size(275, 93);
            this.tblKeePassContextMenuItems.TabIndex = 3;
            // 
            // lstKeePassContextMenuItemsAvailable
            // 
            this.lstKeePassContextMenuItemsAvailable.DisplayMember = "Value";
            this.lstKeePassContextMenuItemsAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstKeePassContextMenuItemsAvailable.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.lstKeePassContextMenuItemsAvailable.FormattingEnabled = true;
            this.lstKeePassContextMenuItemsAvailable.IntegralHeight = false;
            this.lstKeePassContextMenuItemsAvailable.Location = new System.Drawing.Point(3, 3);
            this.lstKeePassContextMenuItemsAvailable.Name = "lstKeePassContextMenuItemsAvailable";
            this.lstKeePassContextMenuItemsAvailable.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstKeePassContextMenuItemsAvailable.Size = new System.Drawing.Size(119, 86);
            this.lstKeePassContextMenuItemsAvailable.TabIndex = 0;
            this.lstKeePassContextMenuItemsAvailable.ValueMember = "Key";
            this.lstKeePassContextMenuItemsAvailable.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lst_DrawItem);
            this.lstKeePassContextMenuItemsAvailable.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.lst_MeasureItem);
            this.lstKeePassContextMenuItemsAvailable.SelectedIndexChanged += new System.EventHandler(this.lstKeePassContextMenuItemsAvailable_SelectedIndexChanged);
            this.lstKeePassContextMenuItemsAvailable.SizeChanged += new System.EventHandler(this.lst_SizeChanged);
            this.lstKeePassContextMenuItemsAvailable.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lst_KeyDownEnter);
            this.lstKeePassContextMenuItemsAvailable.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.lst_PreviewKeyDown);
            // 
            // lstKeePassContextMenuItems
            // 
            this.lstKeePassContextMenuItems.DisplayMember = "Value";
            this.lstKeePassContextMenuItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstKeePassContextMenuItems.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.lstKeePassContextMenuItems.FormattingEnabled = true;
            this.lstKeePassContextMenuItems.IntegralHeight = false;
            this.lstKeePassContextMenuItems.Location = new System.Drawing.Point(152, 3);
            this.lstKeePassContextMenuItems.Name = "lstKeePassContextMenuItems";
            this.lstKeePassContextMenuItems.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstKeePassContextMenuItems.Size = new System.Drawing.Size(120, 86);
            this.lstKeePassContextMenuItems.TabIndex = 1;
            this.lstKeePassContextMenuItems.ValueMember = "Key";
            this.lstKeePassContextMenuItems.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lst_DrawItem);
            this.lstKeePassContextMenuItems.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.lst_MeasureItem);
            this.lstKeePassContextMenuItems.SelectedIndexChanged += new System.EventHandler(this.lstKeePassContextMenuItems_SelectedIndexChanged);
            this.lstKeePassContextMenuItems.SizeChanged += new System.EventHandler(this.lst_SizeChanged);
            this.lstKeePassContextMenuItems.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lst_KeyDownDelete);
            // 
            // ttGeneric
            // 
            this.ttGeneric.AutoPopDelay = 10000;
            this.ttGeneric.InitialDelay = 500;
            this.ttGeneric.ReshowDelay = 100;
            // 
            // KprSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tblVisibilitySettings);
            this.Controls.Add(this.tblKeyboardSettings);
            this.DoubleBuffered = true;
            this.Name = "KprSettingsControl";
            this.Size = new System.Drawing.Size(550, 350);
            pnlKeePassToolbarItemsCmds.ResumeLayout(false);
            pnlKeePassToolbarItemsCmds.PerformLayout();
            pnlKeePassContextMenuItemsCmds.ResumeLayout(false);
            pnlKeePassContextMenuItemsCmds.PerformLayout();
            this.tblKeyboardSettings.ResumeLayout(false);
            this.tblKeyboardSettings.PerformLayout();
            this.tblVisibilitySettings.ResumeLayout(false);
            this.tblVisibilitySettings.PerformLayout();
            this.tblKeePassToolbarItems.ResumeLayout(false);
            this.tblKeePassToolbarItems.PerformLayout();
            this.tblKeePassContextMenuItems.ResumeLayout(false);
            this.tblKeePassContextMenuItems.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private KprKeyboardSettingsTableLayoutPanel tblKeyboardSettings;
        private System.Windows.Forms.TableLayoutPanel tblVisibilitySettings;
        private System.Windows.Forms.TableLayoutPanel tblKeePassToolbarItems;
        private System.Windows.Forms.ListBox lstKeePassToolbarItemsAvailable;
        private System.Windows.Forms.ListBox lstKeePassToolbarItems;
        private System.Windows.Forms.Button cmdKeePassToolbarItemsReset;
        private System.Windows.Forms.Button cmdKeePassToolbarItemsAdd;
        private System.Windows.Forms.Button cmdKeePassToolbarItemsRemove;
        private System.Windows.Forms.Label lblKeePassContextMenuItems;
        private System.Windows.Forms.Label lblKeePassToolbarItems;
        private System.Windows.Forms.TableLayoutPanel tblKeePassContextMenuItems;
        private System.Windows.Forms.ListBox lstKeePassContextMenuItemsAvailable;
        private System.Windows.Forms.ListBox lstKeePassContextMenuItems;
        private System.Windows.Forms.Button cmdKeePassContextMenuItemsReset;
        private System.Windows.Forms.Button cmdKeePassContextMenuItemsAdd;
        private System.Windows.Forms.Button cmdKeePassContextMenuItemsRemove;
        private System.Windows.Forms.Label lblVisibilitySettings;
        private System.Windows.Forms.ImageList imageList1;
        private KprImageList kprImageList;
        private System.Windows.Forms.Label lblKeyboardSettings;
        private System.Windows.Forms.Label lblKeyboardShortcut;
        private KprToolTip ttGeneric;
    }
}
