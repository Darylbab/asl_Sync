namespace Liftopia
{
    partial class Liftopia
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
            this.lblAltaFile = new System.Windows.Forms.Label();
            this.pbAlta = new System.Windows.Forms.ProgressBar();
            this.pbMTC = new System.Windows.Forms.ProgressBar();
            this.lblMTC = new System.Windows.Forms.Label();
            this.lblStartTime = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.SSLStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblAltaFile
            // 
            this.lblAltaFile.AutoSize = true;
            this.lblAltaFile.Location = new System.Drawing.Point(17, 44);
            this.lblAltaFile.Name = "lblAltaFile";
            this.lblAltaFile.Size = new System.Drawing.Size(44, 13);
            this.lblAltaFile.TabIndex = 0;
            this.lblAltaFile.Text = "Alta File";
            // 
            // pbAlta
            // 
            this.pbAlta.Location = new System.Drawing.Point(82, 44);
            this.pbAlta.Name = "pbAlta";
            this.pbAlta.Size = new System.Drawing.Size(178, 13);
            this.pbAlta.TabIndex = 1;
            // 
            // pbMTC
            // 
            this.pbMTC.Location = new System.Drawing.Point(82, 73);
            this.pbMTC.Name = "pbMTC";
            this.pbMTC.Size = new System.Drawing.Size(178, 13);
            this.pbMTC.TabIndex = 3;
            // 
            // lblMTC
            // 
            this.lblMTC.AutoSize = true;
            this.lblMTC.Location = new System.Drawing.Point(17, 73);
            this.lblMTC.Name = "lblMTC";
            this.lblMTC.Size = new System.Drawing.Size(49, 13);
            this.lblMTC.TabIndex = 2;
            this.lblMTC.Text = "MTC File";
            // 
            // lblStartTime
            // 
            this.lblStartTime.AutoSize = true;
            this.lblStartTime.Location = new System.Drawing.Point(81, 13);
            this.lblStartTime.Name = "lblStartTime";
            this.lblStartTime.Size = new System.Drawing.Size(107, 13);
            this.lblStartTime.TabIndex = 4;
            this.lblStartTime.Text = "01-01-2018 12:15PM";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Start Time:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SSLStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 98);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(272, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // SSLStatus
            // 
            this.SSLStatus.Name = "SSLStatus";
            this.SSLStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // Liftopia
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(272, 120);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblStartTime);
            this.Controls.Add(this.pbMTC);
            this.Controls.Add(this.lblMTC);
            this.Controls.Add(this.pbAlta);
            this.Controls.Add(this.lblAltaFile);
            this.MaximizeBox = false;
            this.Name = "Liftopia";
            this.Text = "Liftopia Sync";
            this.Shown += new System.EventHandler(this.Liftopia_Shown);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblAltaFile;
        private System.Windows.Forms.ProgressBar pbAlta;
        private System.Windows.Forms.ProgressBar pbMTC;
        private System.Windows.Forms.Label lblMTC;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel SSLStatus;
    }
}

