namespace KeePassRDP.Generator
{
    partial class KprRdpFileForm
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
            this.kprImageList = new KeePassRDP.KprImageList(this.components);
            this.imageList1 = ((System.Windows.Forms.ImageList)(this.kprImageList));
            this.tblRdpFileForm = new System.Windows.Forms.TableLayoutPanel();
            this.tblCommandButtons = new System.Windows.Forms.TableLayoutPanel();
            this.flpCommandButtonsLeft = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdImport = new System.Windows.Forms.Button();
            this.cmdExport = new System.Windows.Forms.Button();
            this.flpCommandButtonsRight = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdOk = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.tcRdpFileSettings = new System.Windows.Forms.TabControl();
            this.ttGeneral = new KeePassRDP.KprToolTip(this.components);
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.tblRdpFileForm.SuspendLayout();
            this.tblCommandButtons.SuspendLayout();
            this.flpCommandButtonsLeft.SuspendLayout();
            this.flpCommandButtonsRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblRdpFileForm
            // 
            this.tblRdpFileForm.ColumnCount = 1;
            this.tblRdpFileForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblRdpFileForm.Controls.Add(this.tblCommandButtons, 0, 1);
            this.tblRdpFileForm.Controls.Add(this.tcRdpFileSettings, 0, 0);
            this.tblRdpFileForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblRdpFileForm.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tblRdpFileForm.Location = new System.Drawing.Point(0, 0);
            this.tblRdpFileForm.Margin = new System.Windows.Forms.Padding(0);
            this.tblRdpFileForm.Name = "tblRdpFileForm";
            this.tblRdpFileForm.Padding = new System.Windows.Forms.Padding(3);
            this.tblRdpFileForm.RowCount = 2;
            this.tblRdpFileForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblRdpFileForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tblRdpFileForm.Size = new System.Drawing.Size(484, 411);
            this.tblRdpFileForm.TabIndex = 0;
            // 
            // tblCommandButtons
            // 
            this.tblCommandButtons.ColumnCount = 2;
            this.tblCommandButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblCommandButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblCommandButtons.Controls.Add(this.flpCommandButtonsLeft, 0, 0);
            this.tblCommandButtons.Controls.Add(this.flpCommandButtonsRight, 1, 0);
            this.tblCommandButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblCommandButtons.Location = new System.Drawing.Point(3, 383);
            this.tblCommandButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tblCommandButtons.Name = "tblCommandButtons";
            this.tblCommandButtons.RowCount = 1;
            this.tblCommandButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblCommandButtons.Size = new System.Drawing.Size(478, 25);
            this.tblCommandButtons.TabIndex = 3;
            // 
            // flpCommandButtonsLeft
            // 
            this.flpCommandButtonsLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.flpCommandButtonsLeft.AutoSize = true;
            this.flpCommandButtonsLeft.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpCommandButtonsLeft.Controls.Add(this.cmdImport);
            this.flpCommandButtonsLeft.Controls.Add(this.cmdExport);
            this.flpCommandButtonsLeft.Location = new System.Drawing.Point(0, 2);
            this.flpCommandButtonsLeft.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.flpCommandButtonsLeft.Name = "flpCommandButtonsLeft";
            this.flpCommandButtonsLeft.Size = new System.Drawing.Size(145, 23);
            this.flpCommandButtonsLeft.TabIndex = 3;
            // 
            // cmdImport
            // 
            this.cmdImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdImport.AutoSize = true;
            this.cmdImport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdImport.ImageKey = "OpenFile";
            this.cmdImport.ImageList = this.imageList1;
            this.cmdImport.Location = new System.Drawing.Point(0, 0);
            this.cmdImport.Margin = new System.Windows.Forms.Padding(0);
            this.cmdImport.Name = "cmdImport";
            this.cmdImport.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdImport.Size = new System.Drawing.Size(73, 23);
            this.cmdImport.TabIndex = 0;
            this.cmdImport.Text = "&Import";
            this.cmdImport.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cmdImport.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ttGeneral.SetToolTip(this.cmdImport, "Import from .rdp file");
            this.cmdImport.UseVisualStyleBackColor = true;
            this.cmdImport.Click += new System.EventHandler(this.cmdImport_Click);
            // 
            // cmdExport
            // 
            this.cmdExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdExport.AutoSize = true;
            this.cmdExport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdExport.ImageKey = "FileDestination";
            this.cmdExport.ImageList = this.imageList1;
            this.cmdExport.Location = new System.Drawing.Point(73, 0);
            this.cmdExport.Margin = new System.Windows.Forms.Padding(0);
            this.cmdExport.Name = "cmdExport";
            this.cmdExport.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdExport.Size = new System.Drawing.Size(72, 23);
            this.cmdExport.TabIndex = 1;
            this.cmdExport.Text = "&Export";
            this.cmdExport.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cmdExport.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ttGeneral.SetToolTip(this.cmdExport, "Export to .rdp file");
            this.cmdExport.UseVisualStyleBackColor = true;
            this.cmdExport.Click += new System.EventHandler(this.cmdExport_Click);
            // 
            // flpCommandButtonsRight
            // 
            this.flpCommandButtonsRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flpCommandButtonsRight.AutoSize = true;
            this.flpCommandButtonsRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpCommandButtonsRight.Controls.Add(this.cmdOk);
            this.flpCommandButtonsRight.Controls.Add(this.cmdCancel);
            this.flpCommandButtonsRight.Location = new System.Drawing.Point(341, 2);
            this.flpCommandButtonsRight.Margin = new System.Windows.Forms.Padding(0, 2, 2, 0);
            this.flpCommandButtonsRight.Name = "flpCommandButtonsRight";
            this.flpCommandButtonsRight.Size = new System.Drawing.Size(135, 23);
            this.flpCommandButtonsRight.TabIndex = 2;
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
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdOk.Size = new System.Drawing.Size(62, 23);
            this.cmdOk.TabIndex = 0;
            this.cmdOk.Text = "&Save";
            this.cmdOk.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cmdOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ttGeneral.SetToolTip(this.cmdOk, "Save changes");
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.ImageKey = "Cancel";
            this.cmdCancel.ImageList = this.imageList1;
            this.cmdCancel.Location = new System.Drawing.Point(62, 0);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(0);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdCancel.Size = new System.Drawing.Size(73, 23);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cmdCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ttGeneral.SetToolTip(this.cmdCancel, "Discard changes");
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // tcRdpFileSettings
            // 
            this.tcRdpFileSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcRdpFileSettings.Location = new System.Drawing.Point(3, 3);
            this.tcRdpFileSettings.Margin = new System.Windows.Forms.Padding(0);
            this.tcRdpFileSettings.Name = "tcRdpFileSettings";
            this.tcRdpFileSettings.SelectedIndex = 0;
            this.tcRdpFileSettings.Size = new System.Drawing.Size(478, 380);
            this.tcRdpFileSettings.TabIndex = 0;
            this.tcRdpFileSettings.Selected += new System.Windows.Forms.TabControlEventHandler(this.tcRdpFileSettings_Selected);
            // 
            // ttGeneral
            // 
            this.ttGeneral.AutoPopDelay = 10000;
            this.ttGeneral.InitialDelay = 500;
            this.ttGeneral.ReshowDelay = 100;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // KprRdpFileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(484, 411);
            this.Controls.Add(this.tblRdpFileForm);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KprRdpFileForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "KeePassRDP | RDP-File";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.KprRdpFileForm_Closing);
            this.Load += new System.EventHandler(this.KprRdpFileForm_Load);
            this.ResizeBegin += new System.EventHandler(this.KprRdpFileForm_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.KprRdpFileForm_ResizeEnd);
            this.tblRdpFileForm.ResumeLayout(false);
            this.tblCommandButtons.ResumeLayout(false);
            this.tblCommandButtons.PerformLayout();
            this.flpCommandButtonsLeft.ResumeLayout(false);
            this.flpCommandButtonsLeft.PerformLayout();
            this.flpCommandButtonsRight.ResumeLayout(false);
            this.flpCommandButtonsRight.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tblRdpFileForm;
        private System.Windows.Forms.ImageList imageList1;
        private KprImageList kprImageList;
        private System.Windows.Forms.TableLayoutPanel tblCommandButtons;
        private System.Windows.Forms.FlowLayoutPanel flpCommandButtonsRight;
        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.FlowLayoutPanel flpCommandButtonsLeft;
        private System.Windows.Forms.Button cmdImport;
        private KprToolTip ttGeneral;
        private System.Windows.Forms.Button cmdExport;
        private System.Windows.Forms.TabControl tcRdpFileSettings;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}