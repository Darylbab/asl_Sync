namespace GiftCardImport
{
    partial class GiftCardImport
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
            this.cbRootRez = new System.Windows.Forms.CheckBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnRun = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbRootRez
            // 
            this.cbRootRez.AutoSize = true;
            this.cbRootRez.Checked = true;
            this.cbRootRez.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRootRez.Location = new System.Drawing.Point(12, 21);
            this.cbRootRez.Name = "cbRootRez";
            this.cbRootRez.Size = new System.Drawing.Size(68, 17);
            this.cbRootRez.TabIndex = 0;
            this.cbRootRez.Text = "RootRe&z";
            this.cbRootRez.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(86, 15);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(192, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(197, 74);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 2;
            this.btnRun.Text = "&Run";
            this.btnRun.UseVisualStyleBackColor = true;
            // 
            // GiftCardImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 109);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.cbRootRez);
            this.Name = "GiftCardImport";
            this.Text = "Gift Card Imports";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbRootRez;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btnRun;
    }
}

