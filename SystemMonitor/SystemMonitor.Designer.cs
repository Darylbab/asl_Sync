namespace SystemMonitor
{
    partial class SystemMonitor
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.dgvSyncs = new System.Windows.Forms.DataGridView();
            this.Sync = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LastRun = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Results = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvSources = new System.Windows.Forms.DataGridView();
            this.service_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.last_status_check = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.last_status_change = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSyncs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSources)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 588);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1225, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // dgvSyncs
            // 
            this.dgvSyncs.AllowUserToAddRows = false;
            this.dgvSyncs.AllowUserToDeleteRows = false;
            this.dgvSyncs.AllowUserToOrderColumns = true;
            this.dgvSyncs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSyncs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Sync,
            this.LastRun,
            this.Results});
            this.dgvSyncs.Dock = System.Windows.Forms.DockStyle.Left;
            this.dgvSyncs.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvSyncs.Location = new System.Drawing.Point(0, 0);
            this.dgvSyncs.Name = "dgvSyncs";
            this.dgvSyncs.ReadOnly = true;
            this.dgvSyncs.RowHeadersVisible = false;
            this.dgvSyncs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSyncs.Size = new System.Drawing.Size(291, 588);
            this.dgvSyncs.TabIndex = 1;
            // 
            // Sync
            // 
            this.Sync.DataPropertyName = "name";
            this.Sync.HeaderText = "Sync";
            this.Sync.Name = "Sync";
            this.Sync.ReadOnly = true;
            // 
            // LastRun
            // 
            this.LastRun.DataPropertyName = "start_time";
            this.LastRun.HeaderText = "Last Run";
            this.LastRun.Name = "LastRun";
            this.LastRun.ReadOnly = true;
            // 
            // Results
            // 
            this.Results.DataPropertyName = "exit_code";
            this.Results.HeaderText = "Results";
            this.Results.Name = "Results";
            this.Results.ReadOnly = true;
            // 
            // dgvSources
            // 
            this.dgvSources.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSources.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.service_name,
            this.Status,
            this.last_status_check,
            this.last_status_change});
            this.dgvSources.Location = new System.Drawing.Point(297, 0);
            this.dgvSources.Name = "dgvSources";
            this.dgvSources.RowHeadersVisible = false;
            this.dgvSources.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSources.Size = new System.Drawing.Size(374, 269);
            this.dgvSources.TabIndex = 2;
            // 
            // service_name
            // 
            this.service_name.DataPropertyName = "service_name";
            this.service_name.HeaderText = "Name";
            this.service_name.MinimumWidth = 80;
            this.service_name.Name = "service_name";
            this.service_name.ReadOnly = true;
            this.service_name.Width = 80;
            // 
            // Status
            // 
            this.Status.DataPropertyName = "status";
            this.Status.HeaderText = "Status";
            this.Status.MaxInputLength = 30;
            this.Status.MinimumWidth = 50;
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.Width = 50;
            // 
            // last_status_check
            // 
            this.last_status_check.DataPropertyName = "last_status_check";
            this.last_status_check.HeaderText = "Last Check";
            this.last_status_check.MinimumWidth = 100;
            this.last_status_check.Name = "last_status_check";
            this.last_status_check.ReadOnly = true;
            this.last_status_check.Width = 120;
            // 
            // last_status_change
            // 
            this.last_status_change.DataPropertyName = "last_status_change";
            this.last_status_change.HeaderText = "Last Change";
            this.last_status_change.MinimumWidth = 100;
            this.last_status_change.Name = "last_status_change";
            this.last_status_change.ReadOnly = true;
            this.last_status_change.Width = 120;
            // 
            // SystemMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1225, 610);
            this.Controls.Add(this.dgvSources);
            this.Controls.Add(this.dgvSyncs);
            this.Controls.Add(this.statusStrip1);
            this.Name = "SystemMonitor";
            this.Text = "Alta System Monitor";
            ((System.ComponentModel.ISupportInitialize)(this.dgvSyncs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSources)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.DataGridView dgvSyncs;
        private System.Windows.Forms.DataGridViewTextBoxColumn Sync;
        private System.Windows.Forms.DataGridViewTextBoxColumn LastRun;
        private System.Windows.Forms.DataGridViewTextBoxColumn Results;
        private System.Windows.Forms.DataGridView dgvSources;
        private System.Windows.Forms.DataGridViewTextBoxColumn service_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn last_status_check;
        private System.Windows.Forms.DataGridViewTextBoxColumn last_status_change;
    }
}

