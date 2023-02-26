namespace KeePassRDP
{
    partial class KprCredentialPickerForm
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
            this.cmdCancel = new System.Windows.Forms.Button();
            this.kprImageList = new KeePassRDP.KprImageList(this.components);
            this.imageList1 = ((System.Windows.Forms.ImageList)(this.kprImageList));
            this.columnTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnUserName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnNotes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.flpCommandButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdOk = new System.Windows.Forms.Button();
            this.lvEntries = new System.Windows.Forms.ListView();
            this.columnPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tblCredentialPickerForm = new System.Windows.Forms.TableLayoutPanel();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.flpCommandButtons.SuspendLayout();
            this.tblCredentialPickerForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.ImageKey = "Cancel";
            this.cmdCancel.ImageList = this.imageList1;
            this.cmdCancel.Location = new System.Drawing.Point(80, 0);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(0);
            this.cmdCancel.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdCancel.Size = new System.Drawing.Size(80, 23);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cmdCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // columnTitle
            // 
            this.columnTitle.Text = "Title";
            // 
            // columnUserName
            // 
            this.columnUserName.Text = "User Name";
            // 
            // columnNotes
            // 
            this.columnNotes.Text = "Notes";
            // 
            // flpCommandButtons
            // 
            this.flpCommandButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flpCommandButtons.AutoSize = true;
            this.flpCommandButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpCommandButtons.Controls.Add(this.cmdOk);
            this.flpCommandButtons.Controls.Add(this.cmdCancel);
            this.flpCommandButtons.Location = new System.Drawing.Point(971, 435);
            this.flpCommandButtons.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.flpCommandButtons.Name = "flpCommandButtons";
            this.flpCommandButtons.Size = new System.Drawing.Size(160, 23);
            this.flpCommandButtons.TabIndex = 4;
            // 
            // cmdOk
            // 
            this.cmdOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOk.AutoSize = true;
            this.cmdOk.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.ImageKey = "Checkmark";
            this.cmdOk.ImageList = this.imageList1;
            this.cmdOk.Location = new System.Drawing.Point(0, 0);
            this.cmdOk.Margin = new System.Windows.Forms.Padding(0);
            this.cmdOk.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdOk.Size = new System.Drawing.Size(80, 23);
            this.cmdOk.TabIndex = 0;
            this.cmdOk.Text = "&GO";
            this.cmdOk.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cmdOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // lvEntries
            // 
            this.lvEntries.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvEntries.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnPath,
            this.columnTitle,
            this.columnUserName,
            this.columnNotes});
            this.lvEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvEntries.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvEntries.FullRowSelect = true;
            this.lvEntries.HideSelection = false;
            this.lvEntries.Location = new System.Drawing.Point(3, 3);
            this.lvEntries.Margin = new System.Windows.Forms.Padding(0);
            this.lvEntries.MultiSelect = false;
            this.lvEntries.Name = "lvEntries";
            this.lvEntries.OwnerDraw = true;
            this.lvEntries.Size = new System.Drawing.Size(1128, 430);
            this.lvEntries.TabIndex = 0;
            this.lvEntries.UseCompatibleStateImageBehavior = false;
            this.lvEntries.View = System.Windows.Forms.View.Details;
            this.lvEntries.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvEntries_ColumnClick);
            this.lvEntries.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.lvEntries_DrawColumnHeader);
            this.lvEntries.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.lvEntries_DrawItem);
            this.lvEntries.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.lvEntries_DrawSubItem);
            this.lvEntries.ItemActivate += new System.EventHandler(this.lvEntries_ItemActivate);
            this.lvEntries.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvEntries_KeyDown);
            // 
            // columnPath
            // 
            this.columnPath.Text = "Path";
            // 
            // tblCredentialPickerForm
            // 
            this.tblCredentialPickerForm.ColumnCount = 1;
            this.tblCredentialPickerForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblCredentialPickerForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblCredentialPickerForm.Controls.Add(this.lvEntries, 0, 0);
            this.tblCredentialPickerForm.Controls.Add(this.flpCommandButtons, 0, 1);
            this.tblCredentialPickerForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblCredentialPickerForm.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tblCredentialPickerForm.Location = new System.Drawing.Point(0, 0);
            this.tblCredentialPickerForm.Margin = new System.Windows.Forms.Padding(0);
            this.tblCredentialPickerForm.Name = "tblCredentialPickerForm";
            this.tblCredentialPickerForm.Padding = new System.Windows.Forms.Padding(3);
            this.tblCredentialPickerForm.RowCount = 2;
            this.tblCredentialPickerForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblCredentialPickerForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblCredentialPickerForm.Size = new System.Drawing.Size(1134, 461);
            this.tblCredentialPickerForm.TabIndex = 0;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // KprCredentialPickerForm
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(1134, 461);
            this.Controls.Add(this.tblCredentialPickerForm);
            this.DoubleBuffered = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 200);
            this.Name = "KprCredentialPickerForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "KeePassRDP | ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.KprCredentialPickerForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.KprCredentialPickerForm_FormClosed);
            this.Load += new System.EventHandler(this.KprCredentialPickerForm_Load);
            this.flpCommandButtons.ResumeLayout(false);
            this.flpCommandButtons.PerformLayout();
            this.tblCredentialPickerForm.ResumeLayout(false);
            this.tblCredentialPickerForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ListView lvEntries;
        private System.Windows.Forms.TableLayoutPanel tblCredentialPickerForm;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.FlowLayoutPanel flpCommandButtons;
        private System.Windows.Forms.ImageList imageList1;
        private KprImageList kprImageList;
        private System.Windows.Forms.ColumnHeader columnPath;
        private System.Windows.Forms.ColumnHeader columnTitle;
        private System.Windows.Forms.ColumnHeader columnUserName;
        private System.Windows.Forms.ColumnHeader columnNotes;
        private System.Windows.Forms.Button cmdCancel;
    }
}