namespace OtherSkiAreas
{
    partial class OtherSkiAreas
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
            this.cbUpdateOtherSkiAreas = new System.Windows.Forms.CheckBox();
            this.pbUpdateOtherSkiAreas = new System.Windows.Forms.ProgressBar();
            this.cbUpdateASBShared = new System.Windows.Forms.CheckBox();
            this.pbUpdateASBShared = new System.Windows.Forms.ProgressBar();
            this.btnRunSelectedUpdates = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // LblStartTime
            // 
            this.LblStartTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.LblStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblStartTime.Location = new System.Drawing.Point(0, 0);
            this.LblStartTime.Name = "LblStartTime";
            this.LblStartTime.Size = new System.Drawing.Size(284, 23);
            this.LblStartTime.TabIndex = 0;
            this.LblStartTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 156);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(284, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // cbUpdateOtherSkiAreas
            // 
            this.cbUpdateOtherSkiAreas.AutoSize = true;
            this.cbUpdateOtherSkiAreas.Location = new System.Drawing.Point(12, 51);
            this.cbUpdateOtherSkiAreas.Name = "cbUpdateOtherSkiAreas";
            this.cbUpdateOtherSkiAreas.Size = new System.Drawing.Size(138, 17);
            this.cbUpdateOtherSkiAreas.TabIndex = 2;
            this.cbUpdateOtherSkiAreas.Text = "Update &Other Ski Areas";
            this.cbUpdateOtherSkiAreas.UseVisualStyleBackColor = true;
            // 
            // pbUpdateOtherSkiAreas
            // 
            this.pbUpdateOtherSkiAreas.Location = new System.Drawing.Point(154, 50);
            this.pbUpdateOtherSkiAreas.Name = "pbUpdateOtherSkiAreas";
            this.pbUpdateOtherSkiAreas.Size = new System.Drawing.Size(118, 23);
            this.pbUpdateOtherSkiAreas.TabIndex = 3;
            // 
            // cbUpdateASBShared
            // 
            this.cbUpdateASBShared.AutoSize = true;
            this.cbUpdateASBShared.Location = new System.Drawing.Point(12, 84);
            this.cbUpdateASBShared.Name = "cbUpdateASBShared";
            this.cbUpdateASBShared.Size = new System.Drawing.Size(122, 17);
            this.cbUpdateASBShared.TabIndex = 4;
            this.cbUpdateASBShared.Text = "Update &ASB Shared";
            this.cbUpdateASBShared.UseVisualStyleBackColor = true;
            // 
            // pbUpdateASBShared
            // 
            this.pbUpdateASBShared.Location = new System.Drawing.Point(154, 78);
            this.pbUpdateASBShared.Name = "pbUpdateASBShared";
            this.pbUpdateASBShared.Size = new System.Drawing.Size(118, 23);
            this.pbUpdateASBShared.TabIndex = 5;
            // 
            // btnRunSelectedUpdates
            // 
            this.btnRunSelectedUpdates.Location = new System.Drawing.Point(62, 118);
            this.btnRunSelectedUpdates.Name = "btnRunSelectedUpdates";
            this.btnRunSelectedUpdates.Size = new System.Drawing.Size(150, 31);
            this.btnRunSelectedUpdates.TabIndex = 6;
            this.btnRunSelectedUpdates.Text = "&Run Selected Updates";
            this.btnRunSelectedUpdates.UseVisualStyleBackColor = true;
            this.btnRunSelectedUpdates.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(118, 17);
            this.lblStatus.Text = "toolStripStatusLabel1";
            // 
            // OtherSkiAreas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 178);
            this.Controls.Add(this.btnRunSelectedUpdates);
            this.Controls.Add(this.pbUpdateASBShared);
            this.Controls.Add(this.cbUpdateASBShared);
            this.Controls.Add(this.pbUpdateOtherSkiAreas);
            this.Controls.Add(this.cbUpdateOtherSkiAreas);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.LblStartTime);
            this.Name = "OtherSkiAreas";
            this.Text = "Other Ski Areas";
            this.Shown += new System.EventHandler(this.OtherSkiAreas_Shown);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LblStartTime;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.CheckBox cbUpdateOtherSkiAreas;
        private System.Windows.Forms.ProgressBar pbUpdateOtherSkiAreas;
        private System.Windows.Forms.CheckBox cbUpdateASBShared;
        private System.Windows.Forms.ProgressBar pbUpdateASBShared;
        private System.Windows.Forms.Button btnRunSelectedUpdates;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}

