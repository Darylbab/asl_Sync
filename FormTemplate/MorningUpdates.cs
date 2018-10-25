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

namespace MorningUpdate
{
    public partial class MorningUpdates : Form
    {
        private Asgard ASG = new Asgard();
        private CommonFunctions CF = new CommonFunctions();
        private DataWarehouse DW = new DataWarehouse();

        private string CurrentFunction = "MorningUpdates";
        private string[] TArgs;
        private DateTime StartTime = DateTime.Now;



        public MorningUpdates(string[] args = null)
        {
            InitializeComponent();
            TArgs = args;
            string StartTimeStr = StartTime.ToString(Mirror.AxessDateTimeFormat);
            LblStartTime.Text = $"Start Time: {StartTimeStr}";
            SSLStatus.Text = "Initializing";
            SetTaskStatus();
        }

        private void MorningUpdates_Shown(object sender, EventArgs e)
        {
            if (TArgs != null)
            {
                //process any passed parameters/arguments.
            }
        }

        private void SetTaskStatus(string Description = "", string Extra = "", ProgressBar PB1 = null, ProgressBar PB2 = null)
        {
            Application.DoEvents();
            if (PB2 != null)
            {
                PB2.Enabled = PB2.Maximum > 0;
                ASG.SetTaskStatus(Application.ProductName, CurrentFunction, statusStrip1.Items[0].Text, Description, Extra, PB1?.Value, PB1?.Maximum, PB2?.Value, PB2?.Maximum);
                PB1.Refresh();
                PB2.Refresh();
                return;
            }
            if (PB1 != null)
            {
                PB1.Enabled = PB1.Maximum > 0;
                ASG.SetTaskStatus(Application.ProductName, CurrentFunction, statusStrip1.Items[0].Text, Description, Extra, PB1?.Value, PB1?.Maximum);
                PB1.Refresh();
                return;
            }
            ASG.SetTaskStatus(Application.ProductName, CurrentFunction, statusStrip1.Items[0].Text, Description, Extra);
        }

    }
}
