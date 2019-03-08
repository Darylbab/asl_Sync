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
            this.components = new System.ComponentModel.Container();
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
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.pbLoadFromWeb = new System.Windows.Forms.ProgressBar();
            this.cbLoadFromWeb = new System.Windows.Forms.CheckBox();
            this.lblLoadFromWeb = new System.Windows.Forms.Label();
            this.lblUpdateMediaIDs = new System.Windows.Forms.Label();
            this.lblUpdateTokenXRef = new System.Windows.Forms.Label();
            this.lblUpdateSalesData = new System.Windows.Forms.Label();
            this.lblSyncTokenFiles = new System.Windows.Forms.Label();
            this.pbSyncTokenFiles = new System.Windows.Forms.ProgressBar();
            this.cbSyncTokenFiles = new System.Windows.Forms.CheckBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // CbUpdateMediaID
            // 
            this.CbUpdateMediaID.Checked = true;
            this.CbUpdateMediaID.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CbUpdateMediaID.Location = new System.Drawing.Point(18, 80);
            this.CbUpdateMediaID.Name = "CbUpdateMediaID";
            this.CbUpdateMediaID.Size = new System.Drawing.Size(119, 24);
            this.CbUpdateMediaID.TabIndex = 0;
            this.CbUpdateMediaID.Text = "Update MediaIDs";
            this.CbUpdateMediaID.UseVisualStyleBackColor = true;
            // 
            // PbUpdateMediaID
            // 
            this.PbUpdateMediaID.Location = new System.Drawing.Point(143, 80);
            this.PbUpdateMediaID.Name = "PbUpdateMediaID";
            this.PbUpdateMediaID.Size = new System.Drawing.Size(186, 23);
            this.PbUpdateMediaID.TabIndex = 1;
            // 
            // PbUpdateTokenXRef
            // 
            this.PbUpdateTokenXRef.Location = new System.Drawing.Point(143, 140);
            this.PbUpdateTokenXRef.Name = "PbUpdateTokenXRef";
            this.PbUpdateTokenXRef.Size = new System.Drawing.Size(186, 23);
            this.PbUpdateTokenXRef.TabIndex = 3;
            // 
            // CbUpdateTokenXRef
            // 
            this.CbUpdateTokenXRef.Checked = true;
            this.CbUpdateTokenXRef.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CbUpdateTokenXRef.Location = new System.Drawing.Point(18, 140);
            this.CbUpdateTokenXRef.Name = "CbUpdateTokenXRef";
            this.CbUpdateTokenXRef.Size = new System.Drawing.Size(119, 24);
            this.CbUpdateTokenXRef.TabIndex = 2;
            this.CbUpdateTokenXRef.Text = "Update TokenXRef";
            this.CbUpdateTokenXRef.UseVisualStyleBackColor = true;
            // 
            // PbUpdateSalesData
            // 
            this.PbUpdateSalesData.Location = new System.Drawing.Point(143, 169);
            this.PbUpdateSalesData.Name = "PbUpdateSalesData";
            this.PbUpdateSalesData.Size = new System.Drawing.Size(186, 23);
            this.PbUpdateSalesData.TabIndex = 5;
            // 
            // CbUpdateSalesData
            // 
            this.CbUpdateSalesData.Location = new System.Drawing.Point(18, 169);
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
            this.LblStartTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.LblStartTime.Location = new System.Drawing.Point(0, 0);
            this.LblStartTime.Name = "LblStartTime";
            this.LblStartTime.Size = new System.Drawing.Size(341, 39);
            this.LblStartTime.TabIndex = 6;
            this.LblStartTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SslStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 252);
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
            this.BtnRunUpdates.Location = new System.Drawing.Point(143, 209);
            this.BtnRunUpdates.Name = "BtnRunUpdates";
            this.BtnRunUpdates.Size = new System.Drawing.Size(186, 36);
            this.BtnRunUpdates.TabIndex = 8;
            this.BtnRunUpdates.Text = "&Run Updates";
            this.BtnRunUpdates.UseVisualStyleBackColor = true;
            this.BtnRunUpdates.Click += new System.EventHandler(this.BtnRunUpdates_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // pbLoadFromWeb
            // 
            this.pbLoadFromWeb.Location = new System.Drawing.Point(143, 51);
            this.pbLoadFromWeb.Name = "pbLoadFromWeb";
            this.pbLoadFromWeb.Size = new System.Drawing.Size(186, 23);
            this.pbLoadFromWeb.TabIndex = 10;
            // 
            // cbLoadFromWeb
            // 
            this.cbLoadFromWeb.Checked = true;
            this.cbLoadFromWeb.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbLoadFromWeb.Location = new System.Drawing.Point(18, 51);
            this.cbLoadFromWeb.Name = "cbLoadFromWeb";
            this.cbLoadFromWeb.Size = new System.Drawing.Size(119, 24);
            this.cbLoadFromWeb.TabIndex = 9;
            this.cbLoadFromWeb.Text = "Load From eStore";
            this.cbLoadFromWeb.UseVisualStyleBackColor = true;
            // 
            // lblLoadFromWeb
            // 
            this.lblLoadFromWeb.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLoadFromWeb.Location = new System.Drawing.Point(188, 54);
            this.lblLoadFromWeb.Name = "lblLoadFromWeb";
            this.lblLoadFromWeb.Size = new System.Drawing.Size(100, 16);
            this.lblLoadFromWeb.TabIndex = 11;
            this.lblLoadFromWeb.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLoadFromWeb.Visible = false;
            // 
            // lblUpdateMediaIDs
            // 
            this.lblUpdateMediaIDs.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUpdateMediaIDs.Location = new System.Drawing.Point(188, 85);
            this.lblUpdateMediaIDs.Name = "lblUpdateMediaIDs";
            this.lblUpdateMediaIDs.Size = new System.Drawing.Size(100, 16);
            this.lblUpdateMediaIDs.TabIndex = 12;
            this.lblUpdateMediaIDs.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUpdateMediaIDs.Visible = false;
            // 
            // lblUpdateTokenXRef
            // 
            this.lblUpdateTokenXRef.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUpdateTokenXRef.Location = new System.Drawing.Point(188, 145);
            this.lblUpdateTokenXRef.Name = "lblUpdateTokenXRef";
            this.lblUpdateTokenXRef.Size = new System.Drawing.Size(100, 16);
            this.lblUpdateTokenXRef.TabIndex = 13;
            this.lblUpdateTokenXRef.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUpdateTokenXRef.Visible = false;
            // 
            // lblUpdateSalesData
            // 
            this.lblUpdateSalesData.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUpdateSalesData.Location = new System.Drawing.Point(188, 174);
            this.lblUpdateSalesData.Name = "lblUpdateSalesData";
            this.lblUpdateSalesData.Size = new System.Drawing.Size(100, 16);
            this.lblUpdateSalesData.TabIndex = 14;
            this.lblUpdateSalesData.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUpdateSalesData.Visible = false;
            // 
            // lblSyncTokenFiles
            // 
            this.lblSyncTokenFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSyncTokenFiles.Location = new System.Drawing.Point(188, 116);
            this.lblSyncTokenFiles.Name = "lblSyncTokenFiles";
            this.lblSyncTokenFiles.Size = new System.Drawing.Size(100, 16);
            this.lblSyncTokenFiles.TabIndex = 20;
            this.lblSyncTokenFiles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSyncTokenFiles.Visible = false;
            // 
            // pbSyncTokenFiles
            // 
            this.pbSyncTokenFiles.Location = new System.Drawing.Point(143, 111);
            this.pbSyncTokenFiles.Name = "pbSyncTokenFiles";
            this.pbSyncTokenFiles.Size = new System.Drawing.Size(186, 23);
            this.pbSyncTokenFiles.TabIndex = 19;
            // 
            // cbSyncTokenFiles
            // 
            this.cbSyncTokenFiles.Location = new System.Drawing.Point(18, 111);
            this.cbSyncTokenFiles.Name = "cbSyncTokenFiles";
            this.cbSyncTokenFiles.Size = new System.Drawing.Size(119, 24);
            this.cbSyncTokenFiles.TabIndex = 18;
            this.cbSyncTokenFiles.Text = "Mirror Token Files";
            this.cbSyncTokenFiles.UseVisualStyleBackColor = true;
            // 
            // TokenXRefSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 274);
            this.Controls.Add(this.lblSyncTokenFiles);
            this.Controls.Add(this.pbSyncTokenFiles);
            this.Controls.Add(this.cbSyncTokenFiles);
            this.Controls.Add(this.lblUpdateSalesData);
            this.Controls.Add(this.lblUpdateTokenXRef);
            this.Controls.Add(this.lblUpdateMediaIDs);
            this.Controls.Add(this.lblLoadFromWeb);
            this.Controls.Add(this.pbLoadFromWeb);
            this.Controls.Add(this.cbLoadFromWeb);
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
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TokenXRefSync_FormClosed);
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
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ProgressBar pbLoadFromWeb;
        private System.Windows.Forms.CheckBox cbLoadFromWeb;
        private System.Windows.Forms.Label lblLoadFromWeb;
        private System.Windows.Forms.Label lblUpdateMediaIDs;
        private System.Windows.Forms.Label lblUpdateTokenXRef;
        private System.Windows.Forms.Label lblUpdateSalesData;
        private System.Windows.Forms.Label lblSyncTokenFiles;
        private System.Windows.Forms.ProgressBar pbSyncTokenFiles;
        private System.Windows.Forms.CheckBox cbSyncTokenFiles;
    }
}

