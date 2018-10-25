namespace VFP2MySQLSync
{
    partial class VFP2MySQLSync
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
            this.label1 = new System.Windows.Forms.Label();
            this.PBPersonTypes = new System.Windows.Forms.ProgressBar();
            this.PBTicketTypes = new System.Windows.Forms.ProgressBar();
            this.LblTicketTypes = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.SSLStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.PBAXPOS = new System.Windows.Forms.ProgressBar();
            this.LblAXPOS = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // LblStartTime
            // 
            this.LblStartTime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LblStartTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.LblStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblStartTime.Location = new System.Drawing.Point(0, 0);
            this.LblStartTime.Name = "LblStartTime";
            this.LblStartTime.Size = new System.Drawing.Size(284, 23);
            this.LblStartTime.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Person Types";
            // 
            // PBPersonTypes
            // 
            this.PBPersonTypes.Location = new System.Drawing.Point(90, 37);
            this.PBPersonTypes.Name = "PBPersonTypes";
            this.PBPersonTypes.Size = new System.Drawing.Size(182, 23);
            this.PBPersonTypes.TabIndex = 2;
            // 
            // PBTicketTypes
            // 
            this.PBTicketTypes.Location = new System.Drawing.Point(90, 66);
            this.PBTicketTypes.Name = "PBTicketTypes";
            this.PBTicketTypes.Size = new System.Drawing.Size(182, 23);
            this.PBTicketTypes.TabIndex = 4;
            // 
            // LblTicketTypes
            // 
            this.LblTicketTypes.AutoSize = true;
            this.LblTicketTypes.Location = new System.Drawing.Point(16, 67);
            this.LblTicketTypes.Name = "LblTicketTypes";
            this.LblTicketTypes.Size = new System.Drawing.Size(69, 13);
            this.LblTicketTypes.TabIndex = 3;
            this.LblTicketTypes.Text = "Ticket Types";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SSLStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 131);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(284, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // SSLStatus
            // 
            this.SSLStatus.Name = "SSLStatus";
            this.SSLStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // PBAXPOS
            // 
            this.PBAXPOS.Location = new System.Drawing.Point(90, 95);
            this.PBAXPOS.Name = "PBAXPOS";
            this.PBAXPOS.Size = new System.Drawing.Size(182, 23);
            this.PBAXPOS.TabIndex = 7;
            // 
            // LblAXPOS
            // 
            this.LblAXPOS.AutoSize = true;
            this.LblAXPOS.Location = new System.Drawing.Point(16, 96);
            this.LblAXPOS.Name = "LblAXPOS";
            this.LblAXPOS.Size = new System.Drawing.Size(46, 13);
            this.LblAXPOS.TabIndex = 6;
            this.LblAXPOS.Text = "AX POS";
            // 
            // VFP2MySQLSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 153);
            this.Controls.Add(this.PBAXPOS);
            this.Controls.Add(this.LblAXPOS);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.PBTicketTypes);
            this.Controls.Add(this.LblTicketTypes);
            this.Controls.Add(this.PBPersonTypes);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LblStartTime);
            this.Name = "VFP2MySQLSync";
            this.Text = "Visual Fox Pro 2 MySQL Sync";
            this.Shown += new System.EventHandler(this.VFP2MySQLSync_Shown);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LblStartTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar PBPersonTypes;
        private System.Windows.Forms.ProgressBar PBTicketTypes;
        private System.Windows.Forms.Label LblTicketTypes;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel SSLStatus;
        private System.Windows.Forms.ProgressBar PBAXPOS;
        private System.Windows.Forms.Label LblAXPOS;
    }
}

