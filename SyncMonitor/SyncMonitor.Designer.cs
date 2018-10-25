namespace SyncMonitor
{
    partial class SyncMonitor
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.mySqlDataReaderBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.SyncTimer = new System.Windows.Forms.Timer(this.components);
            this.ServerTimer = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.ssLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.gbHilde = new System.Windows.Forms.GroupBox();
            this.txtHildeStatus = new System.Windows.Forms.TextBox();
            this.txtHildeLastRun = new System.Windows.Forms.TextBox();
            this.txtHildeUpSince = new System.Windows.Forms.TextBox();
            this.gbDataWarehouse = new System.Windows.Forms.GroupBox();
            this.txtDWUpSince = new System.Windows.Forms.TextBox();
            this.txtDWLastRun = new System.Windows.Forms.TextBox();
            this.txtDWStatus = new System.Windows.Forms.TextBox();
            this.gbBuy = new System.Windows.Forms.GroupBox();
            this.txtBuyUpSince = new System.Windows.Forms.TextBox();
            this.txtBuyLastRun = new System.Windows.Forms.TextBox();
            this.txtBuyStatus = new System.Windows.Forms.TextBox();
            this.gbAsgard = new System.Windows.Forms.GroupBox();
            this.txtAsgardUpSince = new System.Windows.Forms.TextBox();
            this.txtAsgardLastRun = new System.Windows.Forms.TextBox();
            this.txtAsgardStatus = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mySqlDataReaderBindingSource)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.gbHilde.SuspendLayout();
            this.gbDataWarehouse.SuspendLayout();
            this.gbBuy.SuspendLayout();
            this.gbAsgard.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Top;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(1222, 415);
            this.dataGridView1.TabIndex = 0;
            // 
            // mySqlDataReaderBindingSource
            // 
            this.mySqlDataReaderBindingSource.DataSource = typeof(MySql.Data.MySqlClient.MySqlDataReader);
            // 
            // SyncTimer
            // 
            this.SyncTimer.Enabled = true;
            this.SyncTimer.Interval = 5000;
            this.SyncTimer.Tick += new System.EventHandler(this.SyncTimer_Tick);
            // 
            // ServerTimer
            // 
            this.ServerTimer.Interval = 60000;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ssLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 618);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1222, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // ssLabel1
            // 
            this.ssLabel1.Name = "ssLabel1";
            this.ssLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // gbHilde
            // 
            this.gbHilde.Controls.Add(this.txtHildeUpSince);
            this.gbHilde.Controls.Add(this.txtHildeLastRun);
            this.gbHilde.Controls.Add(this.txtHildeStatus);
            this.gbHilde.Location = new System.Drawing.Point(12, 430);
            this.gbHilde.Name = "gbHilde";
            this.gbHilde.Size = new System.Drawing.Size(469, 43);
            this.gbHilde.TabIndex = 2;
            this.gbHilde.TabStop = false;
            this.gbHilde.Text = "Hilde";
            // 
            // txtHildeStatus
            // 
            this.txtHildeStatus.Location = new System.Drawing.Point(6, 17);
            this.txtHildeStatus.Name = "txtHildeStatus";
            this.txtHildeStatus.Size = new System.Drawing.Size(106, 20);
            this.txtHildeStatus.TabIndex = 0;
            // 
            // txtHildeLastRun
            // 
            this.txtHildeLastRun.Location = new System.Drawing.Point(119, 17);
            this.txtHildeLastRun.Name = "txtHildeLastRun";
            this.txtHildeLastRun.Size = new System.Drawing.Size(171, 20);
            this.txtHildeLastRun.TabIndex = 1;
            // 
            // txtHildeUpSince
            // 
            this.txtHildeUpSince.Location = new System.Drawing.Point(292, 17);
            this.txtHildeUpSince.Name = "txtHildeUpSince";
            this.txtHildeUpSince.Size = new System.Drawing.Size(171, 20);
            this.txtHildeUpSince.TabIndex = 2;
            // 
            // gbDataWarehouse
            // 
            this.gbDataWarehouse.Controls.Add(this.txtDWUpSince);
            this.gbDataWarehouse.Controls.Add(this.txtDWLastRun);
            this.gbDataWarehouse.Controls.Add(this.txtDWStatus);
            this.gbDataWarehouse.Location = new System.Drawing.Point(12, 473);
            this.gbDataWarehouse.Name = "gbDataWarehouse";
            this.gbDataWarehouse.Size = new System.Drawing.Size(469, 43);
            this.gbDataWarehouse.TabIndex = 3;
            this.gbDataWarehouse.TabStop = false;
            this.gbDataWarehouse.Text = "DataWarehouse";
            // 
            // txtDWUpSince
            // 
            this.txtDWUpSince.Location = new System.Drawing.Point(292, 17);
            this.txtDWUpSince.Name = "txtDWUpSince";
            this.txtDWUpSince.Size = new System.Drawing.Size(171, 20);
            this.txtDWUpSince.TabIndex = 2;
            // 
            // txtDWLastRun
            // 
            this.txtDWLastRun.Location = new System.Drawing.Point(119, 17);
            this.txtDWLastRun.Name = "txtDWLastRun";
            this.txtDWLastRun.Size = new System.Drawing.Size(171, 20);
            this.txtDWLastRun.TabIndex = 1;
            // 
            // txtDWStatus
            // 
            this.txtDWStatus.Location = new System.Drawing.Point(6, 17);
            this.txtDWStatus.Name = "txtDWStatus";
            this.txtDWStatus.Size = new System.Drawing.Size(106, 20);
            this.txtDWStatus.TabIndex = 0;
            // 
            // gbBuy
            // 
            this.gbBuy.Controls.Add(this.txtBuyUpSince);
            this.gbBuy.Controls.Add(this.txtBuyLastRun);
            this.gbBuy.Controls.Add(this.txtBuyStatus);
            this.gbBuy.Location = new System.Drawing.Point(14, 516);
            this.gbBuy.Name = "gbBuy";
            this.gbBuy.Size = new System.Drawing.Size(469, 43);
            this.gbBuy.TabIndex = 4;
            this.gbBuy.TabStop = false;
            this.gbBuy.Text = "Buy";
            // 
            // txtBuyUpSince
            // 
            this.txtBuyUpSince.Location = new System.Drawing.Point(292, 17);
            this.txtBuyUpSince.Name = "txtBuyUpSince";
            this.txtBuyUpSince.Size = new System.Drawing.Size(171, 20);
            this.txtBuyUpSince.TabIndex = 2;
            // 
            // txtBuyLastRun
            // 
            this.txtBuyLastRun.Location = new System.Drawing.Point(119, 17);
            this.txtBuyLastRun.Name = "txtBuyLastRun";
            this.txtBuyLastRun.Size = new System.Drawing.Size(171, 20);
            this.txtBuyLastRun.TabIndex = 1;
            // 
            // txtBuyStatus
            // 
            this.txtBuyStatus.Location = new System.Drawing.Point(6, 17);
            this.txtBuyStatus.Name = "txtBuyStatus";
            this.txtBuyStatus.Size = new System.Drawing.Size(106, 20);
            this.txtBuyStatus.TabIndex = 0;
            // 
            // gbAsgard
            // 
            this.gbAsgard.Controls.Add(this.txtAsgardUpSince);
            this.gbAsgard.Controls.Add(this.txtAsgardLastRun);
            this.gbAsgard.Controls.Add(this.txtAsgardStatus);
            this.gbAsgard.Location = new System.Drawing.Point(12, 559);
            this.gbAsgard.Name = "gbAsgard";
            this.gbAsgard.Size = new System.Drawing.Size(469, 43);
            this.gbAsgard.TabIndex = 5;
            this.gbAsgard.TabStop = false;
            this.gbAsgard.Text = "Asgard";
            // 
            // txtAsgardUpSince
            // 
            this.txtAsgardUpSince.Location = new System.Drawing.Point(292, 17);
            this.txtAsgardUpSince.Name = "txtAsgardUpSince";
            this.txtAsgardUpSince.Size = new System.Drawing.Size(171, 20);
            this.txtAsgardUpSince.TabIndex = 2;
            // 
            // txtAsgardLastRun
            // 
            this.txtAsgardLastRun.Location = new System.Drawing.Point(119, 17);
            this.txtAsgardLastRun.Name = "txtAsgardLastRun";
            this.txtAsgardLastRun.Size = new System.Drawing.Size(171, 20);
            this.txtAsgardLastRun.TabIndex = 1;
            // 
            // txtAsgardStatus
            // 
            this.txtAsgardStatus.Location = new System.Drawing.Point(6, 17);
            this.txtAsgardStatus.Name = "txtAsgardStatus";
            this.txtAsgardStatus.Size = new System.Drawing.Size(106, 20);
            this.txtAsgardStatus.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(130, 421);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Last Checked";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(309, 421);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Last Change";
            // 
            // SyncMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1222, 640);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.gbAsgard);
            this.Controls.Add(this.gbBuy);
            this.Controls.Add(this.gbDataWarehouse);
            this.Controls.Add(this.gbHilde);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.dataGridView1);
            this.MaximizeBox = false;
            this.Name = "SyncMonitor";
            this.Text = "Alta Ski Lifts Sync Monitor";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mySqlDataReaderBindingSource)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.gbHilde.ResumeLayout(false);
            this.gbHilde.PerformLayout();
            this.gbDataWarehouse.ResumeLayout(false);
            this.gbDataWarehouse.PerformLayout();
            this.gbBuy.ResumeLayout(false);
            this.gbBuy.PerformLayout();
            this.gbAsgard.ResumeLayout(false);
            this.gbAsgard.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource mySqlDataReaderBindingSource;
        private System.Windows.Forms.Timer SyncTimer;
        private System.Windows.Forms.Timer ServerTimer;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel ssLabel1;
        private System.Windows.Forms.GroupBox gbHilde;
        private System.Windows.Forms.TextBox txtHildeUpSince;
        private System.Windows.Forms.TextBox txtHildeLastRun;
        private System.Windows.Forms.TextBox txtHildeStatus;
        private System.Windows.Forms.GroupBox gbDataWarehouse;
        private System.Windows.Forms.TextBox txtDWUpSince;
        private System.Windows.Forms.TextBox txtDWLastRun;
        private System.Windows.Forms.TextBox txtDWStatus;
        private System.Windows.Forms.GroupBox gbBuy;
        private System.Windows.Forms.TextBox txtBuyUpSince;
        private System.Windows.Forms.TextBox txtBuyLastRun;
        private System.Windows.Forms.TextBox txtBuyStatus;
        private System.Windows.Forms.GroupBox gbAsgard;
        private System.Windows.Forms.TextBox txtAsgardUpSince;
        private System.Windows.Forms.TextBox txtAsgardLastRun;
        private System.Windows.Forms.TextBox txtAsgardStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

