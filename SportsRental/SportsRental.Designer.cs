namespace SportsRental
{
    partial class SportsRental
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
            this.lblLastRun = new System.Windows.Forms.Label();
            this.txtLastRun = new System.Windows.Forms.TextBox();
            this.txtNextRun = new System.Windows.Forms.TextBox();
            this.lblNextRun = new System.Windows.Forms.Label();
            this.txtLastRunResults = new System.Windows.Forms.TextBox();
            this.BtnRunNow = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.sslFunction = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblLastRun
            // 
            this.lblLastRun.AutoSize = true;
            this.lblLastRun.Location = new System.Drawing.Point(20, 17);
            this.lblLastRun.Name = "lblLastRun";
            this.lblLastRun.Size = new System.Drawing.Size(53, 13);
            this.lblLastRun.TabIndex = 0;
            this.lblLastRun.Text = "Last Run:";
            // 
            // txtLastRun
            // 
            this.txtLastRun.Location = new System.Drawing.Point(79, 10);
            this.txtLastRun.Name = "txtLastRun";
            this.txtLastRun.Size = new System.Drawing.Size(238, 20);
            this.txtLastRun.TabIndex = 1;
            // 
            // txtNextRun
            // 
            this.txtNextRun.Location = new System.Drawing.Point(79, 36);
            this.txtNextRun.Name = "txtNextRun";
            this.txtNextRun.Size = new System.Drawing.Size(238, 20);
            this.txtNextRun.TabIndex = 3;
            // 
            // lblNextRun
            // 
            this.lblNextRun.AutoSize = true;
            this.lblNextRun.Location = new System.Drawing.Point(20, 43);
            this.lblNextRun.Name = "lblNextRun";
            this.lblNextRun.Size = new System.Drawing.Size(55, 13);
            this.lblNextRun.TabIndex = 2;
            this.lblNextRun.Text = "Next Run:";
            // 
            // txtLastRunResults
            // 
            this.txtLastRunResults.Location = new System.Drawing.Point(320, 10);
            this.txtLastRunResults.Name = "txtLastRunResults";
            this.txtLastRunResults.Size = new System.Drawing.Size(100, 20);
            this.txtLastRunResults.TabIndex = 4;
            // 
            // BtnRunNow
            // 
            this.BtnRunNow.Location = new System.Drawing.Point(23, 75);
            this.BtnRunNow.Name = "BtnRunNow";
            this.BtnRunNow.Size = new System.Drawing.Size(100, 23);
            this.BtnRunNow.TabIndex = 5;
            this.BtnRunNow.Text = "&Run Now";
            this.BtnRunNow.UseVisualStyleBackColor = true;
            this.BtnRunNow.Click += new System.EventHandler(this.BtnRunNow_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sslFunction,
            this.ssProgressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 111);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(427, 22);
            this.statusStrip1.TabIndex = 6;
            // 
            // sslFunction
            // 
            this.sslFunction.AutoSize = false;
            this.sslFunction.Name = "sslFunction";
            this.sslFunction.Size = new System.Drawing.Size(300, 17);
            // 
            // ssProgressBar
            // 
            this.ssProgressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.ssProgressBar.AutoSize = false;
            this.ssProgressBar.Name = "ssProgressBar";
            this.ssProgressBar.Size = new System.Drawing.Size(100, 16);
            this.ssProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // SportsRental
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 133);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.BtnRunNow);
            this.Controls.Add(this.txtLastRunResults);
            this.Controls.Add(this.txtNextRun);
            this.Controls.Add(this.lblNextRun);
            this.Controls.Add(this.txtLastRun);
            this.Controls.Add(this.lblLastRun);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "SportsRental";
            this.Text = "Sports Rental";
            this.Shown += new System.EventHandler(this.SportsRental_Shown);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblLastRun;
        private System.Windows.Forms.TextBox txtLastRun;
        private System.Windows.Forms.TextBox txtNextRun;
        private System.Windows.Forms.Label lblNextRun;
        private System.Windows.Forms.TextBox txtLastRunResults;
        private System.Windows.Forms.Button BtnRunNow;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel sslFunction;
        private System.Windows.Forms.ToolStripProgressBar ssProgressBar;
    }
}

