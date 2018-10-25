namespace MorningUpdate
{
    partial class MorningUpdates
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
            this.LblStartTime = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.SSLStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.LblFixMTCWillcallPersNr = new System.Windows.Forms.Label();
            this.PBFixPersNrInWillcall = new System.Windows.Forms.ProgressBar();
            this.PBUpdateSalesDataUsage = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.PBPOSSyncForPreviousDay = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            this.PBUpdateSalesDataCardStatus = new System.Windows.Forms.ProgressBar();
            this.label3 = new System.Windows.Forms.Label();
            this.TSPB = new System.Windows.Forms.ToolStripProgressBar();
            this.PBPOSUpdateStatus = new System.Windows.Forms.ProgressBar();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // LblStartTime
            // 
            this.LblStartTime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LblStartTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.LblStartTime.Location = new System.Drawing.Point(0, 0);
            this.LblStartTime.Name = "LblStartTime";
            this.LblStartTime.Size = new System.Drawing.Size(347, 23);
            this.LblStartTime.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SSLStatus,
            this.TSPB});
            this.statusStrip1.Location = new System.Drawing.Point(0, 187);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(347, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // SSLStatus
            // 
            this.SSLStatus.AutoSize = false;
            this.SSLStatus.Name = "SSLStatus";
            this.SSLStatus.Size = new System.Drawing.Size(200, 17);
            // 
            // LblFixMTCWillcallPersNr
            // 
            this.LblFixMTCWillcallPersNr.AutoSize = true;
            this.LblFixMTCWillcallPersNr.Location = new System.Drawing.Point(6, 37);
            this.LblFixMTCWillcallPersNr.Name = "LblFixMTCWillcallPersNr";
            this.LblFixMTCWillcallPersNr.Size = new System.Drawing.Size(102, 13);
            this.LblFixMTCWillcallPersNr.TabIndex = 2;
            this.LblFixMTCWillcallPersNr.Text = "Fix PersNr in Willcall";
            // 
            // PBFixPersNrInWillcall
            // 
            this.PBFixPersNrInWillcall.Location = new System.Drawing.Point(165, 27);
            this.PBFixPersNrInWillcall.Name = "PBFixPersNrInWillcall";
            this.PBFixPersNrInWillcall.Size = new System.Drawing.Size(170, 23);
            this.PBFixPersNrInWillcall.TabIndex = 3;
            // 
            // PBUpdateSalesDataUsage
            // 
            this.PBUpdateSalesDataUsage.Location = new System.Drawing.Point(165, 56);
            this.PBUpdateSalesDataUsage.Name = "PBUpdateSalesDataUsage";
            this.PBUpdateSalesDataUsage.Size = new System.Drawing.Size(170, 23);
            this.PBUpdateSalesDataUsage.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "UpdateSalesDataUsage";
            // 
            // PBPOSSyncForPreviousDay
            // 
            this.PBPOSSyncForPreviousDay.Location = new System.Drawing.Point(165, 85);
            this.PBPOSSyncForPreviousDay.Name = "PBPOSSyncForPreviousDay";
            this.PBPOSSyncForPreviousDay.Size = new System.Drawing.Size(170, 23);
            this.PBPOSSyncForPreviousDay.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "POS Sync for Previous Day";
            // 
            // PBUpdateSalesDataCardStatus
            // 
            this.PBUpdateSalesDataCardStatus.Location = new System.Drawing.Point(165, 146);
            this.PBUpdateSalesDataCardStatus.Name = "PBUpdateSalesDataCardStatus";
            this.PBUpdateSalesDataCardStatus.Size = new System.Drawing.Size(170, 23);
            this.PBUpdateSalesDataCardStatus.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 156);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(149, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Update SalesData CardStatus";
            // 
            // TSPB
            // 
            this.TSPB.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.TSPB.AutoSize = false;
            this.TSPB.Name = "TSPB";
            this.TSPB.Size = new System.Drawing.Size(100, 16);
            this.TSPB.Visible = false;
            // 
            // PBPOSUpdateStatus
            // 
            this.PBPOSUpdateStatus.Location = new System.Drawing.Point(165, 114);
            this.PBPOSUpdateStatus.Name = "PBPOSUpdateStatus";
            this.PBPOSUpdateStatus.Size = new System.Drawing.Size(170, 23);
            this.PBPOSUpdateStatus.TabIndex = 10;
            // 
            // MorningUpdates
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(347, 209);
            this.Controls.Add(this.PBPOSUpdateStatus);
            this.Controls.Add(this.PBUpdateSalesDataCardStatus);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.PBPOSSyncForPreviousDay);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.PBUpdateSalesDataUsage);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PBFixPersNrInWillcall);
            this.Controls.Add(this.LblFixMTCWillcallPersNr);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.LblStartTime);
            this.Name = "MorningUpdates";
            this.Text = "Morning Updates";
            this.Shown += new System.EventHandler(this.MorningUpdates_Shown);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LblStartTime;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel SSLStatus;
        private System.Windows.Forms.ToolStripProgressBar TSPB;
        private System.Windows.Forms.Label LblFixMTCWillcallPersNr;
        private System.Windows.Forms.ProgressBar PBFixPersNrInWillcall;
        private System.Windows.Forms.ProgressBar PBUpdateSalesDataUsage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar PBPOSSyncForPreviousDay;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar PBUpdateSalesDataCardStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ProgressBar PBPOSUpdateStatus;
    }
}

