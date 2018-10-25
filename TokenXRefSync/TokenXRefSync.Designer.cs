namespace TokenXRefSync
{
    partial class TokenXRefSync
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
            this.CbUpdateMediaID = new System.Windows.Forms.CheckBox();
            this.PbUpdateMediaID = new System.Windows.Forms.ProgressBar();
            this.PbUpdateTokenXRef = new System.Windows.Forms.ProgressBar();
            this.CbUpdateTokenXRef = new System.Windows.Forms.CheckBox();
            this.PbUpdateSalesData = new System.Windows.Forms.ProgressBar();
            this.CbUpdateSalesData = new System.Windows.Forms.CheckBox();
            this.LblStartTime = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.SslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.BtnRunUpdates = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // CbUpdateMediaID
            // 
            this.CbUpdateMediaID.Checked = true;
            this.CbUpdateMediaID.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CbUpdateMediaID.Location = new System.Drawing.Point(18, 58);
            this.CbUpdateMediaID.Name = "CbUpdateMediaID";
            this.CbUpdateMediaID.Size = new System.Drawing.Size(119, 24);
            this.CbUpdateMediaID.TabIndex = 0;
            this.CbUpdateMediaID.Text = "Update MediaIDs";
            this.CbUpdateMediaID.UseVisualStyleBackColor = true;
            // 
            // PbUpdateMediaID
            // 
            this.PbUpdateMediaID.Location = new System.Drawing.Point(143, 58);
            this.PbUpdateMediaID.Name = "PbUpdateMediaID";
            this.PbUpdateMediaID.Size = new System.Drawing.Size(186, 23);
            this.PbUpdateMediaID.TabIndex = 1;
            // 
            // PbUpdateTokenXRef
            // 
            this.PbUpdateTokenXRef.Location = new System.Drawing.Point(143, 87);
            this.PbUpdateTokenXRef.Name = "PbUpdateTokenXRef";
            this.PbUpdateTokenXRef.Size = new System.Drawing.Size(186, 23);
            this.PbUpdateTokenXRef.TabIndex = 3;
            // 
            // CbUpdateTokenXRef
            // 
            this.CbUpdateTokenXRef.Checked = true;
            this.CbUpdateTokenXRef.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CbUpdateTokenXRef.Location = new System.Drawing.Point(18, 87);
            this.CbUpdateTokenXRef.Name = "CbUpdateTokenXRef";
            this.CbUpdateTokenXRef.Size = new System.Drawing.Size(119, 24);
            this.CbUpdateTokenXRef.TabIndex = 2;
            this.CbUpdateTokenXRef.Text = "Update TokenXRef";
            this.CbUpdateTokenXRef.UseVisualStyleBackColor = true;
            // 
            // PbUpdateSalesData
            // 
            this.PbUpdateSalesData.Location = new System.Drawing.Point(143, 116);
            this.PbUpdateSalesData.Name = "PbUpdateSalesData";
            this.PbUpdateSalesData.Size = new System.Drawing.Size(186, 23);
            this.PbUpdateSalesData.TabIndex = 5;
            // 
            // CbUpdateSalesData
            // 
            this.CbUpdateSalesData.Checked = true;
            this.CbUpdateSalesData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CbUpdateSalesData.Location = new System.Drawing.Point(18, 116);
            this.CbUpdateSalesData.Name = "CbUpdateSalesData";
            this.CbUpdateSalesData.Size = new System.Drawing.Size(119, 24);
            this.CbUpdateSalesData.TabIndex = 4;
            this.CbUpdateSalesData.Text = "Update SalesData";
            this.CbUpdateSalesData.UseVisualStyleBackColor = true;
            // 
            // LblStartTime
            // 
            this.LblStartTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.LblStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblStartTime.Location = new System.Drawing.Point(0, 0);
            this.LblStartTime.Name = "LblStartTime";
            this.LblStartTime.Size = new System.Drawing.Size(341, 39);
            this.LblStartTime.TabIndex = 6;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SslStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 195);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(341, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // SslStatus
            // 
            this.SslStatus.Name = "SslStatus";
            this.SslStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // BtnRunUpdates
            // 
            this.BtnRunUpdates.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnRunUpdates.Location = new System.Drawing.Point(143, 156);
            this.BtnRunUpdates.Name = "BtnRunUpdates";
            this.BtnRunUpdates.Size = new System.Drawing.Size(186, 36);
            this.BtnRunUpdates.TabIndex = 8;
            this.BtnRunUpdates.Text = "&Run Updates";
            this.BtnRunUpdates.UseVisualStyleBackColor = true;
            this.BtnRunUpdates.Click += new System.EventHandler(this.BtnRunUpdates_Click);
            // 
            // TokenXRefSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 217);
            this.Controls.Add(this.BtnRunUpdates);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.LblStartTime);
            this.Controls.Add(this.PbUpdateSalesData);
            this.Controls.Add(this.CbUpdateSalesData);
            this.Controls.Add(this.PbUpdateTokenXRef);
            this.Controls.Add(this.CbUpdateTokenXRef);
            this.Controls.Add(this.PbUpdateMediaID);
            this.Controls.Add(this.CbUpdateMediaID);
            this.Name = "TokenXRefSync";
            this.Text = "Token XRef Sync";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox CbUpdateMediaID;
        private System.Windows.Forms.ProgressBar PbUpdateMediaID;
        private System.Windows.Forms.ProgressBar PbUpdateTokenXRef;
        private System.Windows.Forms.CheckBox CbUpdateTokenXRef;
        private System.Windows.Forms.ProgressBar PbUpdateSalesData;
        private System.Windows.Forms.CheckBox CbUpdateSalesData;
        private System.Windows.Forms.Label LblStartTime;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel SslStatus;
        private System.Windows.Forms.Button BtnRunUpdates;
    }
}

