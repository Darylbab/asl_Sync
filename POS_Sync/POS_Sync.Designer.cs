namespace POS_Sync
{
    partial class POS_Sync
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
            this.label10 = new System.Windows.Forms.Label();
            this.cmdPOSSync = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.dtpPOSSync = new System.Windows.Forms.DateTimePicker();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.SSLStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.PBStatus = new System.Windows.Forms.ToolStripProgressBar();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.POSListView = new System.Windows.Forms.ListView();
            this.POSNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ComputerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Located = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LblStartTime = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(5, 18);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(30, 13);
            this.label10.TabIndex = 12;
            this.label10.Text = "Date";
            // 
            // cmdPOSSync
            // 
            this.cmdPOSSync.Location = new System.Drawing.Point(41, 512);
            this.cmdPOSSync.Name = "cmdPOSSync";
            this.cmdPOSSync.Size = new System.Drawing.Size(274, 28);
            this.cmdPOSSync.TabIndex = 11;
            this.cmdPOSSync.Text = "Sync";
            this.cmdPOSSync.UseVisualStyleBackColor = true;
            this.cmdPOSSync.Click += new System.EventHandler(this.CmdPOSSync_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 47);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "POS";
            // 
            // dtpPOSSync
            // 
            this.dtpPOSSync.Location = new System.Drawing.Point(41, 12);
            this.dtpPOSSync.Name = "dtpPOSSync";
            this.dtpPOSSync.Size = new System.Drawing.Size(202, 20);
            this.dtpPOSSync.TabIndex = 8;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SSLStatus,
            this.PBStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 543);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(332, 22);
            this.statusStrip1.TabIndex = 13;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // SSLStatus
            // 
            this.SSLStatus.AutoSize = false;
            this.SSLStatus.Name = "SSLStatus";
            this.SSLStatus.Size = new System.Drawing.Size(210, 17);
            this.SSLStatus.Text = "Updating POS 13                                  ";
            this.SSLStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PBStatus
            // 
            this.PBStatus.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.PBStatus.Name = "PBStatus";
            this.PBStatus.Size = new System.Drawing.Size(100, 16);
            this.PBStatus.Visible = false;
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(41, 474);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(142, 23);
            this.btnSelectAll.TabIndex = 15;
            this.btnSelectAll.Text = "Selec&t All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.BtnSelectAll_Click);
            // 
            // btnClearAll
            // 
            this.btnClearAll.Location = new System.Drawing.Point(189, 474);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(126, 23);
            this.btnClearAll.TabIndex = 16;
            this.btnClearAll.Text = "Clea&r All";
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.BtnClearAll_Click);
            // 
            // POSListView
            // 
            this.POSListView.AutoArrange = false;
            this.POSListView.CheckBoxes = true;
            this.POSListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.POSNo,
            this.ComputerName,
            this.Located});
            this.POSListView.FullRowSelect = true;
            this.POSListView.GridLines = true;
            this.POSListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.POSListView.Location = new System.Drawing.Point(41, 47);
            this.POSListView.Name = "POSListView";
            this.POSListView.ShowGroups = false;
            this.POSListView.Size = new System.Drawing.Size(274, 421);
            this.POSListView.TabIndex = 17;
            this.POSListView.UseCompatibleStateImageBehavior = false;
            this.POSListView.View = System.Windows.Forms.View.Details;
            // 
            // POSNo
            // 
            this.POSNo.Text = "POS No.";
            this.POSNo.Width = 40;
            // 
            // ComputerName
            // 
            this.ComputerName.Text = "Computer Name";
            this.ComputerName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ComputerName.Width = 80;
            // 
            // Located
            // 
            this.Located.Text = "Location";
            this.Located.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Located.Width = 120;
            // 
            // LblStartTime
            // 
            this.LblStartTime.AutoSize = true;
            this.LblStartTime.Location = new System.Drawing.Point(270, 28);
            this.LblStartTime.Name = "LblStartTime";
            this.LblStartTime.Size = new System.Drawing.Size(0, 13);
            this.LblStartTime.TabIndex = 18;
            this.LblStartTime.Visible = false;
            // 
            // timer1
            // 
            this.timer1.Interval = 900000;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // POS_Sync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(332, 565);
            this.Controls.Add(this.LblStartTime);
            this.Controls.Add(this.POSListView);
            this.Controls.Add(this.btnClearAll);
            this.Controls.Add(this.btnSelectAll);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.cmdPOSSync);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.dtpPOSSync);
            this.MaximizeBox = false;
            this.Name = "POS_Sync";
            this.Text = "POS Sync";
            this.Load += new System.EventHandler(this.POS_Sync_Load);
            this.Shown += new System.EventHandler(this.POS_Sync_Shown);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button cmdPOSSync;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.DateTimePicker dtpPOSSync;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.ListView POSListView;
        private System.Windows.Forms.ColumnHeader POSNo;
        private System.Windows.Forms.ColumnHeader ComputerName;
        private System.Windows.Forms.ColumnHeader Located;
        private System.Windows.Forms.Label LblStartTime;
        private System.Windows.Forms.ToolStripStatusLabel SSLStatus;
        private System.Windows.Forms.ToolStripProgressBar PBStatus;
        private System.Windows.Forms.Timer timer1;
    }
}

