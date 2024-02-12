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
            this.flpRdpFileSettings = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdImport = new System.Windows.Forms.Button();
            this.flpCommandButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdOk = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.ttGeneral = new KeePassRDP.KprToolTip(this.components);
            this.tblRdpFileForm.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flpCommandButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblRdpFileForm
            // 
            this.tblRdpFileForm.ColumnCount = 1;
            this.tblRdpFileForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblRdpFileForm.Controls.Add(this.flpRdpFileSettings, 0, 0);
            this.tblRdpFileForm.Controls.Add(this.tableLayoutPanel1, 0, 1);
            this.tblRdpFileForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblRdpFileForm.Location = new System.Drawing.Point(0, 0);
            this.tblRdpFileForm.Margin = new System.Windows.Forms.Padding(0);
            this.tblRdpFileForm.Name = "tblRdpFileForm";
            this.tblRdpFileForm.Padding = new System.Windows.Forms.Padding(3);
            this.tblRdpFileForm.RowCount = 2;
            this.tblRdpFileForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblRdpFileForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblRdpFileForm.Size = new System.Drawing.Size(484, 411);
            this.tblRdpFileForm.TabIndex = 0;
            // 
            // flpRdpFileSettings
            // 
            this.flpRdpFileSettings.AutoScroll = true;
            this.flpRdpFileSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpRdpFileSettings.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpRdpFileSettings.Location = new System.Drawing.Point(3, 3);
            this.flpRdpFileSettings.Margin = new System.Windows.Forms.Padding(0);
            this.flpRdpFileSettings.Name = "flpRdpFileSettings";
            this.flpRdpFileSettings.Size = new System.Drawing.Size(478, 380);
            this.flpRdpFileSettings.TabIndex = 2;
            this.flpRdpFileSettings.WrapContents = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flpCommandButtons, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 383);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(478, 25);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.cmdImport);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 2);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(73, 23);
            this.flowLayoutPanel1.TabIndex = 3;
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
            // flpCommandButtons
            // 
            this.flpCommandButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flpCommandButtons.AutoSize = true;
            this.flpCommandButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpCommandButtons.Controls.Add(this.cmdOk);
            this.flpCommandButtons.Controls.Add(this.cmdCancel);
            this.flpCommandButtons.Location = new System.Drawing.Point(343, 2);
            this.flpCommandButtons.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.flpCommandButtons.Name = "flpCommandButtons";
            this.flpCommandButtons.Size = new System.Drawing.Size(135, 23);
            this.flpCommandButtons.TabIndex = 2;
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
            // ttGeneral
            // 
            this.ttGeneral.AutoPopDelay = 10000;
            this.ttGeneral.InitialDelay = 500;
            this.ttGeneral.ReshowDelay = 100;
            // 
            // KprRdpFileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(484, 411);
            this.Controls.Add(this.tblRdpFileForm);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 450);
            this.Name = "KprRdpFileForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "KeePassRDP | RDP-File";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.KprRdpFileForm_Closing);
            this.Load += new System.EventHandler(this.KprRdpFileForm_Load);
            this.tblRdpFileForm.ResumeLayout(false);
            this.tblRdpFileForm.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flpCommandButtons.ResumeLayout(false);
            this.flpCommandButtons.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tblRdpFileForm;
        private System.Windows.Forms.FlowLayoutPanel flpRdpFileSettings;
        private System.Windows.Forms.ImageList imageList1;
        private KprImageList kprImageList;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flpCommandButtons;
        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button cmdImport;
        private KprToolTip ttGeneral;
    }
}