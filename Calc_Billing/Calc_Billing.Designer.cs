namespace Calc_Billing
{
    partial class Calc_Billing
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpFromDate = new System.Windows.Forms.DateTimePicker();
            this.dtpToDate = new System.Windows.Forms.DateTimePicker();
            this.chkAddCharges = new System.Windows.Forms.CheckBox();
            this.chkUseBilling = new System.Windows.Forms.CheckBox();
            this.chkLoadARTrans = new System.Windows.Forms.CheckBox();
            this.chkDisplayReport = new System.Windows.Forms.CheckBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(358, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "CALCULATE CARD USE BILLING && ADD POS ARS && PASS CHARGES";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Date Range to Process:";
            // 
            // dtpFromDate
            // 
            this.dtpFromDate.Location = new System.Drawing.Point(134, 49);
            this.dtpFromDate.Name = "dtpFromDate";
            this.dtpFromDate.Size = new System.Drawing.Size(200, 20);
            this.dtpFromDate.TabIndex = 2;
            this.dtpFromDate.ValueChanged += new System.EventHandler(this.DtpFromDate_ValueChanged);
            // 
            // dtpToDate
            // 
            this.dtpToDate.Location = new System.Drawing.Point(135, 75);
            this.dtpToDate.Name = "dtpToDate";
            this.dtpToDate.Size = new System.Drawing.Size(200, 20);
            this.dtpToDate.TabIndex = 3;
            this.dtpToDate.ValueChanged += new System.EventHandler(this.DtpToDate_ValueChanged);
            // 
            // chkAddCharges
            // 
            this.chkAddCharges.AutoSize = true;
            this.chkAddCharges.Checked = true;
            this.chkAddCharges.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAddCharges.Location = new System.Drawing.Point(12, 103);
            this.chkAddCharges.Name = "chkAddCharges";
            this.chkAddCharges.Size = new System.Drawing.Size(170, 17);
            this.chkAddCharges.TabIndex = 4;
            this.chkAddCharges.Text = "&Add POS ARs && Pass Charges";
            this.chkAddCharges.UseVisualStyleBackColor = true;
            // 
            // chkUseBilling
            // 
            this.chkUseBilling.AutoSize = true;
            this.chkUseBilling.Checked = true;
            this.chkUseBilling.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseBilling.Location = new System.Drawing.Point(12, 126);
            this.chkUseBilling.Name = "chkUseBilling";
            this.chkUseBilling.Size = new System.Drawing.Size(147, 17);
            this.chkUseBilling.TabIndex = 5;
            this.chkUseBilling.Text = "&Calculate Card Use Billing";
            this.chkUseBilling.UseVisualStyleBackColor = true;
            // 
            // chkLoadARTrans
            // 
            this.chkLoadARTrans.AutoSize = true;
            this.chkLoadARTrans.Checked = true;
            this.chkLoadARTrans.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLoadARTrans.Location = new System.Drawing.Point(12, 149);
            this.chkLoadARTrans.Name = "chkLoadARTrans";
            this.chkLoadARTrans.Size = new System.Drawing.Size(223, 17);
            this.chkLoadARTrans.TabIndex = 6;
            this.chkLoadARTrans.Text = "&Load Salesdata && Salespmt from ARTrans";
            this.chkLoadARTrans.UseVisualStyleBackColor = true;
            // 
            // chkDisplayReport
            // 
            this.chkDisplayReport.AutoSize = true;
            this.chkDisplayReport.Checked = true;
            this.chkDisplayReport.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDisplayReport.Location = new System.Drawing.Point(12, 172);
            this.chkDisplayReport.Name = "chkDisplayReport";
            this.chkDisplayReport.Size = new System.Drawing.Size(175, 17);
            this.chkDisplayReport.TabIndex = 7;
            this.chkDisplayReport.Text = "&Display Report After Calculation";
            this.chkDisplayReport.UseVisualStyleBackColor = true;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(11, 195);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 8;
            this.btnRun.Text = "&Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.BtnRun_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(107, 195);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 9;
            this.btnExit.Text = "E&xit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 232);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(358, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsStatusLabel1
            // 
            this.tsStatusLabel1.Name = "tsStatusLabel1";
            this.tsStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // Calc_Billing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 254);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.chkDisplayReport);
            this.Controls.Add(this.chkLoadARTrans);
            this.Controls.Add(this.chkUseBilling);
            this.Controls.Add(this.chkAddCharges);
            this.Controls.Add(this.dtpToDate);
            this.Controls.Add(this.dtpFromDate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Calc_Billing";
            this.Text = "Calc Billing";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Calc_Billing_FormClosed);
            this.Shown += new System.EventHandler(this.Calc_Billing_Shown);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpFromDate;
        private System.Windows.Forms.DateTimePicker dtpToDate;
        private System.Windows.Forms.CheckBox chkAddCharges;
        private System.Windows.Forms.CheckBox chkUseBilling;
        private System.Windows.Forms.CheckBox chkLoadARTrans;
        private System.Windows.Forms.CheckBox chkDisplayReport;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsStatusLabel1;
    }
}

