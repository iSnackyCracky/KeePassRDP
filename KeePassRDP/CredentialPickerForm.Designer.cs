namespace KeePassRDP
{
    partial class CredentialPickerForm
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
            this.olvEntries = new BrightIdeasSoftware.ObjectListView();
            this.olvColUsername = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColPath = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColTitle = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColNotes = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColUidHash = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cmdOk = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.olvEntries)).BeginInit();
            this.SuspendLayout();
            // 
            // olvEntries
            // 
            this.olvEntries.AllColumns.Add(this.olvColUsername);
            this.olvEntries.AllColumns.Add(this.olvColPath);
            this.olvEntries.AllColumns.Add(this.olvColTitle);
            this.olvEntries.AllColumns.Add(this.olvColNotes);
            this.olvEntries.AllColumns.Add(this.olvColUidHash);
            this.olvEntries.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvEntries.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColUsername,
            this.olvColPath,
            this.olvColTitle,
            this.olvColNotes});
            this.olvEntries.FullRowSelect = true;
            this.olvEntries.GridLines = true;
            this.olvEntries.Location = new System.Drawing.Point(12, 12);
            this.olvEntries.MultiSelect = false;
            this.olvEntries.Name = "olvEntries";
            this.olvEntries.SelectAllOnControlA = false;
            this.olvEntries.ShowGroups = false;
            this.olvEntries.Size = new System.Drawing.Size(586, 214);
            this.olvEntries.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.olvEntries.TabIndex = 0;
            this.olvEntries.UseCompatibleStateImageBehavior = false;
            this.olvEntries.View = System.Windows.Forms.View.Details;
            this.olvEntries.ItemActivate += new System.EventHandler(this.olvEntries_ItemActivate);
            // 
            // olvColUsername
            // 
            this.olvColUsername.AspectName = "Username";
            this.olvColUsername.DisplayIndex = 2;
            this.olvColUsername.IsEditable = false;
            this.olvColUsername.Text = "User Name";
            this.olvColUsername.Width = 200;
            // 
            // olvColPath
            // 
            this.olvColPath.AspectName = "Path";
            this.olvColPath.DisplayIndex = 0;
            this.olvColPath.Text = "Path";
            this.olvColPath.Width = 150;
            // 
            // olvColTitle
            // 
            this.olvColTitle.AspectName = "Title";
            this.olvColTitle.DisplayIndex = 1;
            this.olvColTitle.IsEditable = false;
            this.olvColTitle.Text = "Title";
            this.olvColTitle.Width = 90;
            // 
            // olvColNotes
            // 
            this.olvColNotes.AspectName = "Notes";
            this.olvColNotes.FillsFreeSpace = true;
            this.olvColNotes.IsEditable = false;
            this.olvColNotes.Text = "Notes";
            // 
            // olvColUidHash
            // 
            this.olvColUidHash.AspectName = "UidHash";
            this.olvColUidHash.DisplayIndex = 4;
            this.olvColUidHash.IsEditable = false;
            this.olvColUidHash.IsVisible = false;
            this.olvColUidHash.Text = "UUID Hash";
            // 
            // cmdOk
            // 
            this.cmdOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.Location = new System.Drawing.Point(442, 232);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(75, 23);
            this.cmdOk.TabIndex = 1;
            this.cmdOk.Text = "&Ok";
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(523, 232);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 2;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // CredentialPickerForm
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(610, 267);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOk);
            this.Controls.Add(this.olvEntries);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CredentialPickerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select account";
            this.Load += new System.EventHandler(this.CredentialPickerForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.olvEntries)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private BrightIdeasSoftware.ObjectListView olvEntries;
        private BrightIdeasSoftware.OLVColumn olvColTitle;
        private BrightIdeasSoftware.OLVColumn olvColUsername;
        private BrightIdeasSoftware.OLVColumn olvColNotes;
        private BrightIdeasSoftware.OLVColumn olvColUidHash;
        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.Button cmdCancel;
        private BrightIdeasSoftware.OLVColumn olvColPath;
    }
}