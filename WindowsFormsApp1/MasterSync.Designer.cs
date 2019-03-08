namespace WindowsFormsApp1
{
    partial class MasterSync
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
            this.MasterSyncStatusStrip = new System.Windows.Forms.StatusStrip();
            this.tslStatusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.tslProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.MasterSyncStatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // MasterSyncStatusStrip
            // 
            this.MasterSyncStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslStatusText,
            this.tslProgressBar});
            this.MasterSyncStatusStrip.Location = new System.Drawing.Point(0, 239);
            this.MasterSyncStatusStrip.Name = "MasterSyncStatusStrip";
            this.MasterSyncStatusStrip.Size = new System.Drawing.Size(566, 22);
            this.MasterSyncStatusStrip.TabIndex = 0;
            this.MasterSyncStatusStrip.Text = "statusStrip1";
            // 
            // tslStatusText
            // 
            this.tslStatusText.AutoSize = false;
            this.tslStatusText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tslStatusText.Name = "tslStatusText";
            this.tslStatusText.Size = new System.Drawing.Size(700, 17);
            this.tslStatusText.Spring = true;
            this.tslStatusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tslProgressBar
            // 
            this.tslProgressBar.AutoSize = false;
            this.tslProgressBar.Name = "tslProgressBar";
            this.tslProgressBar.Size = new System.Drawing.Size(150, 16);
            this.tslProgressBar.Step = 1;
            this.tslProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // MasterSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 261);
            this.Controls.Add(this.MasterSyncStatusStrip);
            this.Name = "MasterSync";
            this.Text = "Master Sync";
            this.Load += new System.EventHandler(this.MasterSync_Load);
            this.MasterSyncStatusStrip.ResumeLayout(false);
            this.MasterSyncStatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip MasterSyncStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tslStatusText;
        private System.Windows.Forms.ToolStripProgressBar tslProgressBar;
    }
}

