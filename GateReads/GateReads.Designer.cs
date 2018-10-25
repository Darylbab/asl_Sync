namespace GateReads
{
    partial class SkiVisits
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
            this.LblStartTime = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.SSLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.SSLStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.LblGateReads = new System.Windows.Forms.Label();
            this.PBGateReads = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // LblStartTime
            // 
            this.LblStartTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.LblStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblStartTime.Location = new System.Drawing.Point(0, 0);
            this.LblStartTime.Name = "LblStartTime";
            this.LblStartTime.Size = new System.Drawing.Size(263, 16);
            this.LblStartTime.TabIndex = 0;
            this.LblStartTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SSLabel,
            this.SSLStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 66);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(263, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // SSLabel
            // 
            this.SSLabel.Name = "SSLabel";
            this.SSLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // SSLStatus
            // 
            this.SSLStatus.Name = "SSLStatus";
            this.SSLStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // LblGateReads
            // 
            this.LblGateReads.AutoSize = true;
            this.LblGateReads.Location = new System.Drawing.Point(7, 43);
            this.LblGateReads.Name = "LblGateReads";
            this.LblGateReads.Size = new System.Drawing.Size(64, 13);
            this.LblGateReads.TabIndex = 2;
            this.LblGateReads.Text = "Gate Reads";
            // 
            // PBGateReads
            // 
            this.PBGateReads.Location = new System.Drawing.Point(77, 33);
            this.PBGateReads.Name = "PBGateReads";
            this.PBGateReads.Size = new System.Drawing.Size(174, 23);
            this.PBGateReads.Step = 1;
            this.PBGateReads.TabIndex = 3;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // SkiVisits
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(263, 88);
            this.Controls.Add(this.PBGateReads);
            this.Controls.Add(this.LblGateReads);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.LblStartTime);
            this.Name = "SkiVisits";
            this.Text = "Gate Reads / Ski Visits";
            this.Shown += new System.EventHandler(this.Form_Shown);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LblStartTime;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel SSLabel;
        private System.Windows.Forms.Label LblGateReads;
        private System.Windows.Forms.ProgressBar PBGateReads;
        private System.Windows.Forms.ToolStripStatusLabel SSLStatus;
        private System.Windows.Forms.Timer timer1;
    }
}

