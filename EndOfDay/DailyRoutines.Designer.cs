namespace DailyRoutines
{
    partial class frmDailyRoutines
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
            this.cbCalcIKONCharges = new System.Windows.Forms.CheckBox();
            this.btnRunUpdates = new System.Windows.Forms.Button();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.cbSetSPCardStatus = new System.Windows.Forms.CheckBox();
            this.cbUpdateUses = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cbCalcIKONCharges
            // 
            this.cbCalcIKONCharges.AutoSize = true;
            this.cbCalcIKONCharges.Location = new System.Drawing.Point(12, 21);
            this.cbCalcIKONCharges.Name = "cbCalcIKONCharges";
            this.cbCalcIKONCharges.Size = new System.Drawing.Size(141, 17);
            this.cbCalcIKONCharges.TabIndex = 0;
            this.cbCalcIKONCharges.Text = "Calculate IKON Charges";
            this.cbCalcIKONCharges.UseVisualStyleBackColor = true;
            // 
            // btnRunUpdates
            // 
            this.btnRunUpdates.Location = new System.Drawing.Point(216, 227);
            this.btnRunUpdates.Name = "btnRunUpdates";
            this.btnRunUpdates.Size = new System.Drawing.Size(85, 23);
            this.btnRunUpdates.TabIndex = 1;
            this.btnRunUpdates.Text = "&Run Updates";
            this.btnRunUpdates.UseVisualStyleBackColor = true;
            this.btnRunUpdates.Click += new System.EventHandler(this.btnRunUpdates_Click);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(159, 16);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(200, 20);
            this.dateTimePicker1.TabIndex = 2;
            // 
            // cbSetSPCardStatus
            // 
            this.cbSetSPCardStatus.AutoSize = true;
            this.cbSetSPCardStatus.Checked = true;
            this.cbSetSPCardStatus.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSetSPCardStatus.Location = new System.Drawing.Point(12, 44);
            this.cbSetSPCardStatus.Name = "cbSetSPCardStatus";
            this.cbSetSPCardStatus.Size = new System.Drawing.Size(114, 17);
            this.cbSetSPCardStatus.TabIndex = 3;
            this.cbSetSPCardStatus.Text = "Set SPCard Status";
            this.cbSetSPCardStatus.UseVisualStyleBackColor = true;
            // 
            // cbUpdateUses
            // 
            this.cbUpdateUses.AutoSize = true;
            this.cbUpdateUses.Checked = true;
            this.cbUpdateUses.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbUpdateUses.Location = new System.Drawing.Point(12, 67);
            this.cbUpdateUses.Name = "cbUpdateUses";
            this.cbUpdateUses.Size = new System.Drawing.Size(88, 17);
            this.cbUpdateUses.TabIndex = 4;
            this.cbUpdateUses.Text = "Update Uses";
            this.cbUpdateUses.UseVisualStyleBackColor = true;
            // 
            // frmDailyRoutines
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(974, 632);
            this.Controls.Add(this.cbUpdateUses);
            this.Controls.Add(this.cbSetSPCardStatus);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.btnRunUpdates);
            this.Controls.Add(this.cbCalcIKONCharges);
            this.Name = "frmDailyRoutines";
            this.Text = "Dailly Routines";
            this.Shown += new System.EventHandler(this.frmDailyRoutines_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbCalcIKONCharges;
        private System.Windows.Forms.Button btnRunUpdates;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.CheckBox cbSetSPCardStatus;
        private System.Windows.Forms.CheckBox cbUpdateUses;
    }
}

