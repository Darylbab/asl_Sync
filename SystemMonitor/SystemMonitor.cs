using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using asl_SyncLibrary;


namespace SystemMonitor
{
    public partial class SystemMonitor : Form
    {
        Asgard AG = new Asgard();
        CommonFunctions CF = new CommonFunctions();

        DataTable TaskHistory;
        DataTable ServerStatus;

        public SystemMonitor()
        {
            InitializeComponent();
            TaskHistory = CF.LoadTable(Asgard.AsgardCon, "SELECT name, start_time, exit_time, CASE WHEN exit_code=0 THEN 'SUCCESS' WHEN exit_code IS NULL THEN 'RUNNING' WHEN exit_code > 0 THEN CONCAT('ERROR ', exit_code) END AS exit_code, runtime FROM sync.task_history WHERE rec_no IN (SELECT max(rec_no) FROM sync.task_history GROUP BY name)", "TaskHistory");
            dgvSyncs.DataSource = TaskHistory;
            ServerStatus = CF.LoadTable(Asgard.AsgardCon, "SELECT service_name, status, last_status_check, last_status_change FROM sync.server WHERE active=1 ORDER BY service_name", "ServerStatus");
            dgvSources.DataSource = ServerStatus;
        }
    }
}
