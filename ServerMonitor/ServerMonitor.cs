using System;
using System.Drawing;
using System.Windows.Forms;
using asl_SyncLibrary;

namespace ServerMonitor
{
    public partial class ServerMonitor : Form
    {
        CommonFunctions CF = new CommonFunctions();
        ASLEmail EMail = new ASLEmail();

        Int32 LocalServerCount = 5;
        Int32 WebServicesCount = 5;
        //Int32 RemoteServerCount = 0;
        Int32 TotalServerCount = 10;

        bool ServerStatusChanged = false;
        
        //LOCAL SERVERS
        Asgard ASG = new Asgard(true);
        Server Server_Asgard = new Server("Asgard");
        bool AsgardModified = false;
        BUY_ALTA_COM BUY = new BUY_ALTA_COM();
        Server Server_Buy = new Server("Buy");
        bool BuyModified = false;
        DataWarehouse DW = new DataWarehouse();
        Server Server_DataWarehouse = new Server("DataWarehouse");
        bool DataWarehouseModified = false;
        Mirror AM = new Mirror();
        Server Server_Mirror = new Server("Hilde");
        bool MirrorModified = false;
        Shop AltaShop = new Shop();
        Server Server_Shop = new Server("Shop");
        bool ShopModified = false;
       

        //WEB SERVICES
        Axess_DCI4CRM CRM = new Axess_DCI4CRM();    //Axess CRM Service
        Server Server_CRM = new Server("DCI4CRM");
        bool DCI4CRMModified = false;
        WTPSI SI = new WTPSI();                     //Axess Back Office
        Server Server_SI = new Server("WTPSI");
        bool SIModified = false;
        DCI4WTP WTP = new DCI4WTP();                //AxessWTP sservice
        Server Server_WTP = new Server("DCI4WTP");
        bool WTPModified = false;
        OBI4POS POS = new OBI4POS();              //Axess POS service *** Still has problems ***
        Server Server_POS = new Server("OBI4POS");
        bool POSModified = false;
        POSSync SS = new POSSync();                 //Axess Sync Service
        Server Server_SS = new Server("POSSync");
        bool SSModified = false;



        public ServerMonitor()
        {
            InitializeComponent();
            cboRefreshRate.SelectedIndex = 3;
            timer1.Interval = 1;
            timer1.Enabled = true;
        }

        private bool CheckLocalServers(bool aLocalProgressBarOnly = true)
        {
            bool ReturnValue = false;
            if (aLocalProgressBarOnly)
            {
                toolStripProgressBar1.Maximum = LocalServerCount;
                toolStripProgressBar1.Minimum = 0;
                toolStripProgressBar1.Value = 0;
            }
            toolStripStatusLabel1.Text = "Checking Asgard...";
            Application.DoEvents();
            if (ASG.SetTaskStatus("ServerMonitor", "CheckLocalServers", toolStripStatusLabel1.Text, "", "", toolStripProgressBar1.Value, toolStripProgressBar1.Maximum))
            {
                UpdateStatus(Server_Asgard, ASG.GetServerStatus());
                Server_Asgard.Refresh();
                txtAsgardStatus.Text = Server_Asgard.Status.ToString().Replace('-', ' ');
                txtAsgardStatus.BackColor = StatusColor(Server_Asgard.Status);
                txtAsgardStatus.ForeColor = Color.White;
                txtAsgardLastCheck.Text = Server_Asgard.LastChecked.Value.ToString(Mirror.AxessDateTimeFormat);
                txtAsgardSince.Text = Server_Asgard.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat);
                ReturnValue = (Server_Asgard.LastChecked.Value.AddMinutes(5) < Server_Asgard.LastChecked.Value);
                toolStripStatusLabel1.Text = "Checking Buy...";
                ASG.SetTaskStatus("ServerMonitor", "CheckLocalServers", toolStripStatusLabel1.Text, "", "", ++toolStripProgressBar1.Value, toolStripProgressBar1.Maximum);
                Application.DoEvents();
                UpdateStatus(Server_Buy, BUY.GetServerStatus());
                Server_Buy.Refresh();
                txtBuyStatus.Text = Server_Buy.Status.ToString().Replace('-', ' ');
                txtBuyStatus.BackColor = StatusColor(Server_Buy.Status);
                txtBuyStatus.ForeColor = Color.White;
                txtBuyLastCheck.Text = Server_Buy.LastChecked.Value.ToString(Mirror.AxessDateTimeFormat);
                txtBuySince.Text = Server_Buy.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat);
                ReturnValue |= (Server_Buy.LastChecked.Value.AddMinutes(5) < Server_Buy.LastChecked.Value);
                toolStripStatusLabel1.Text = "Checking DataWarehouse...";
                ASG.SetTaskStatus("ServerMonitor", "CheckLocalServers", toolStripStatusLabel1.Text, "", "", ++toolStripProgressBar1.Value, toolStripProgressBar1.Maximum);
                Application.DoEvents();
                UpdateStatus(Server_DataWarehouse, DW.GetServerStatus());
                Server_DataWarehouse.Refresh();
                txtDataWarehouseStatus.Text = Server_DataWarehouse.Status.ToString().Replace('-', ' ');
                txtDataWarehouseStatus.BackColor = StatusColor(Server_DataWarehouse.Status);
                txtDataWarehouseStatus.ForeColor = Color.White;
                txtDataWarehouseLastCheck.Text = Server_DataWarehouse.LastChecked.Value.ToString(Mirror.AxessDateTimeFormat);
                txtDataWarehouseSince.Text = Server_DataWarehouse.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat);
                ReturnValue |= (Server_DataWarehouse.LastChecked.Value.AddMinutes(5) < Server_DataWarehouse.LastChecked.Value);
                toolStripStatusLabel1.Text = "Checking Hilde...";
                ASG.SetTaskStatus("ServerMonitor", "CheckLocalServers", toolStripStatusLabel1.Text, "", "", ++toolStripProgressBar1.Value, toolStripProgressBar1.Maximum);
                Application.DoEvents();
                UpdateStatus(Server_Mirror, AM.GetServerStatus());
                Server_Mirror.Refresh();
                txtHildeStatus.Text = Server_Mirror.Status.ToString().Replace('-', ' ');
                txtHildeStatus.BackColor = StatusColor(Server_Mirror.Status);
                txtHildeStatus.ForeColor = Color.White;
                txtHildeLastCheck.Text = Server_Mirror.LastChecked.Value.ToString(Mirror.AxessDateTimeFormat);
                txtHildeSince.Text = Server_Mirror.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat);
                ReturnValue |= (Server_Mirror.LastChecked.Value.AddMinutes(5) < Server_Mirror.LastChecked.Value);
                toolStripStatusLabel1.Text = "Checking Shop...";
                ASG.SetTaskStatus("ServerMonitor", "CheckLocalServers", toolStripStatusLabel1.Text, "", "", ++toolStripProgressBar1.Value, toolStripProgressBar1.Maximum);
                Application.DoEvents();
                UpdateStatus(Server_Shop, AltaShop.GetServerStatus());
                Server_Shop.Refresh();
                txtShopStatus.Text = Server_Shop.Status.ToString().Replace('-', ' ');
                txtShopStatus.BackColor = StatusColor(Server_Shop.Status);
                txtShopStatus.ForeColor = Color.White;
                txtShopLastCheck.Text = Server_Shop.LastChecked.Value.ToString(Mirror.AxessDateTimeFormat);
                txtShopSince.Text = Server_Shop.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat);
                ReturnValue |= (Server_Shop.LastChecked.Value.AddMinutes(5) < Server_Shop.LastChecked.Value);
                ASG.SetTaskStatus("ServerMonitor", "CheckLocalServers", "Complete", "", "", ++toolStripProgressBar1.Value, toolStripProgressBar1.Maximum);
            }
            else
            {
                ASG.SetTaskStatus("ServerMonitor", "CheckLocalServers", "Asgard Failed", "", "", LocalServerCount, toolStripProgressBar1.Maximum);
            }
            toolStripStatusLabel1.Text = string.Empty;
            Application.DoEvents();
            return ReturnValue;
        }

        private bool CheckWebServices(bool aWebServiceProgressBarOnly = true)
        {
            bool ReturnValue = false;
            if (aWebServiceProgressBarOnly)
            {
                toolStripProgressBar1.Maximum = WebServicesCount;
                toolStripProgressBar1.Minimum = 0;
                toolStripProgressBar1.Value = 0;
            }
            if (Server_Asgard.Status == Serverstatus.Active)
            {
                //check dci4crm
                toolStripStatusLabel1.Text = "Checking DCI4CRM";
                ASG.SetTaskStatus("ServerMonitor", "CheckWebServices", toolStripStatusLabel1.Text, "", "", toolStripProgressBar1.Value, toolStripProgressBar1.Maximum);
                Application.DoEvents();
                UpdateStatus(Server_CRM, CRM.GetServerStatus(Server_CRM.URL));
                Server_CRM.Refresh();
                txtCRMStatus.Text = Server_CRM.Status.ToString().Replace('-', ' ');
                txtCRMStatus.BackColor = StatusColor(Server_CRM.Status);
                txtCRMStatus.ForeColor = Color.White;
                txtCRMLastCheck.Text = Server_CRM.LastChecked.Value.ToString(Mirror.AxessDateTimeFormat);
                txtCRMSince.Text = Server_CRM.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat);
                ReturnValue = (Server_CRM.LastChecked.Value.AddMinutes(5) < Server_CRM.LastChecked.Value);
                toolStripStatusLabel1.Text = "Checking DCI4WTP...";
                ASG.SetTaskStatus("ServerMonitor", "CheckWebServices", toolStripStatusLabel1.Text, "", "", ++toolStripProgressBar1.Value, toolStripProgressBar1.Maximum);
                Application.DoEvents();
                //check dci4wtp
                UpdateStatus(Server_WTP, WTP.GetServerStatus(Server_WTP.URL));
                Server_WTP.Refresh();
                txtWTPStatus.Text = Server_WTP.Status.ToString().Replace('-', ' ');
                txtWTPStatus.BackColor = StatusColor(Server_WTP.Status);
                txtWTPStatus.ForeColor = Color.White;
                txtWTPLastCheck.Text = Server_WTP.LastChecked.Value.ToString(Mirror.AxessDateTimeFormat);
                txtWTPSince.Text = Server_WTP.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat);
                ReturnValue |= (Server_WTP.LastChecked.Value.AddMinutes(5) < Server_WTP.LastChecked.Value);
                toolStripStatusLabel1.Text = "Checking OBI4POS...";
                ASG.SetTaskStatus("ServerMonitor", "CheckWebServices", toolStripStatusLabel1.Text, "", "", ++toolStripProgressBar1.Value, toolStripProgressBar1.Maximum);
                Application.DoEvents();
                //check obi4pos
                UpdateStatus(Server_POS, POS.GetServerStatus(Server_POS.URL));
                Server_POS.Refresh();
                txtOBIStatus.Text = Server_POS.Status.ToString().Replace('-', ' ');
                txtOBIStatus.BackColor = StatusColor(Server_POS.Status);
                txtOBIStatus.ForeColor = Color.White;
                txtOBILastCheck.Text = Server_POS.LastChecked.Value.ToString(Mirror.AxessDateTimeFormat);
                txtOBISince.Text = Server_POS.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat);
                ReturnValue |= (Server_POS.LastChecked.Value.AddMinutes(5) < Server_POS.LastChecked.Value);
                ASG.SetTaskStatus("ServerMonitor", "CheckWebServices", toolStripStatusLabel1.Text, "", "", ++toolStripProgressBar1.Value, toolStripProgressBar1.Maximum);
                toolStripStatusLabel1.Text = "Checking WTPSI...";
                Application.DoEvents();
                //check wtpsi
                UpdateStatus(Server_SI, SI.GetServerStatus(Server_SI.URL));
                Server_SI.Refresh();
                txtSIStatus.Text = Server_SI.Status.ToString().Replace('-', ' ');
                txtSIStatus.BackColor = StatusColor(Server_SI.Status);
                txtSIStatus.ForeColor = Color.White;
                txtSILastCheck.Text = Server_SI.LastChecked.Value.ToString(Mirror.AxessDateTimeFormat);
                txtSISince.Text = Server_SI.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat);
                toolStripStatusLabel1.Text = "Checking DCI4Sync...";
                ReturnValue |= (Server_SI.LastChecked.Value.AddMinutes(5) < Server_SI.LastChecked.Value);
                ASG.SetTaskStatus("ServerMonitor", "CheckWebServices", toolStripStatusLabel1.Text, "", "", ++toolStripProgressBar1.Value, toolStripProgressBar1.Maximum);
                Application.DoEvents();
                //check POSSync
                UpdateStatus(Server_SS, SS.GetServerStatus(Server_SS.URL));
                Server_SS.Refresh();
                txtSSStatus.Text = Server_SS.Status.ToString().Replace('-', ' ');
                txtSSStatus.BackColor = StatusColor(Server_SS.Status);
                txtSSStatus.ForeColor = Color.White;
                txtSSLastCheck.Text = Server_SS.LastChecked.Value.ToString(Mirror.AxessDateTimeFormat);
                txtSSSince.Text = Server_SS.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat);
                ReturnValue |= (Server_SS.LastChecked.Value.AddMinutes(5) < Server_SS.LastChecked.Value);
                ASG.SetTaskStatus("ServerMonitor", "CheckWebServices", toolStripStatusLabel1.Text, "", "", ++toolStripProgressBar1.Value, toolStripProgressBar1.Maximum);
            }
            else
            {
                toolStripProgressBar1.Value += WebServicesCount;
                ASG.SetTaskStatus("ServerMonitor", "CheckWebServices", "COMPLETE", "", "", toolStripProgressBar1.Maximum, toolStripProgressBar1.Maximum);
            }
            toolStripStatusLabel1.Text = string.Empty;
            Application.DoEvents();
            return ReturnValue;
        }

        private bool UpdateStatus(Server aServer, Serverstatus aNewStatus)
        {
            string CheckTime = DateTime.Now.ToString(Mirror.AxessDateTimeFormat);
            string TQuery = $"UPDATE sync.server SET last_status_check='{CheckTime}'";
            if (aNewStatus != aServer.Status)
            {
                TQuery += $", last_status_change='{CheckTime}', status='{aNewStatus}'";
                switch (aServer.Name)
                {
                    case "Asgard":
                        AsgardModified = true;
                        break;
                    case "Buy":
                        BuyModified = true;
                        break;
                    case "DataWarehouse":
                        DataWarehouseModified = true;
                        break;
                    case "DCI4CRM":
                        DCI4CRMModified = true;
                        break;
                    case "DCI4WTP":
                        WTPModified = true;
                        break;
                    case "Hilde":
                        MirrorModified = true;
                        break;
                    case "OBI4POS":
                        POSModified = true;
                        break;
                    case "POSSync":
                        SSModified = true;
                        break;
                    case "WTPSI":
                        SIModified = true;
                        break;
                    case "Shop":
                        ShopModified = true;
                        break;
                }
                ServerStatusChanged = true;
            }
            TQuery += $" WHERE name='{aServer.Name}'";
            return CF.ExecuteSQL(Asgard.AsgardCon, TQuery);
        }

        private Color StatusColor(Serverstatus aStatus)
        {
            Color ReturnColor = Color.Red;
            switch (aStatus)
            {
                case Serverstatus.Active:
                    ReturnColor = Color.Green;
                    break;
                case Serverstatus.Server_Failed:
                    ReturnColor = Color.Red;
                    break;
                case Serverstatus.Service_Failed:
                    ReturnColor = Color.PaleVioletRed;
                    break;
            }
            return ReturnColor;
        }

        private void UpdateServerStatus()
        {
            //stop timer so a second sync doesn't happen if the first isn't complete
            timer1.Enabled = false;

            //stop user from using controls
            button1.Enabled = false;
            cboRefreshRate.Enabled = false;
            toolStripProgressBar1.Maximum = TotalServerCount;
            toolStripProgressBar1.Minimum = 0;
            toolStripProgressBar1.Value = 0;

            //reset variables
            AsgardModified = false;
            BuyModified = false;
            DataWarehouseModified = false;
            DCI4CRMModified = false;
            WTPModified = false;
            MirrorModified = false;
            POSModified = false;
            ShopModified = false;
            SSModified = false;
            SIModified = false;
            ServerStatusChanged = false;
            bool SendEmail = CheckLocalServers(false);
            //CheckRemoteServers(false);
            SendEmail |= CheckWebServices(false);
            toolStripProgressBar1.Value = 0;
            toolStripStatusLabel1.Text = string.Empty;
            if (ServerStatusChanged)
            {
                //if (SendEmail)
                {
                    ASG.SetTaskStatus("ServerMonitor", "Timer1_Tick", "Sending email...");
                    EMail.IsHTML = true;
                    string TExtraData = "Current server status:\r\n\nLocal Servers\r\n<table>";
                    TExtraData += $"<tr><td>Asgard</td><td>{Server_Asgard.Status}  {(AsgardModified ? "***" : "")}</td><td>{Server_Asgard.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat)}</td></tr>";
                    TExtraData += $"<tr><td>Buy</td><td>{Server_Buy.Status}  {(BuyModified ? "***" : "")}</td><td>{Server_Buy.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat)}</td></tr>";
                    TExtraData += $"<tr><td>DataWhse</td><td>{Server_DataWarehouse.Status}  {(DataWarehouseModified ? "***" : "")}</td><td>{Server_DataWarehouse.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat)}</td></tr>";
                    TExtraData += $"<tr><td>Hilde</td><td>{Server_Mirror.Status}  {(MirrorModified ? "***" : "")}</td><td>{Server_Mirror.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat)}</td></tr>";
                    TExtraData += $"<tr><td>Shop</td><td>{Server_Shop.Status}  {(ShopModified ? "***" : "")}</td><td>{Server_Shop.LastChanged.Value.ToString(Mirror.AxessDateFormat)}</td></tr>";
                    TExtraData += "</table>\n\nAxess Servers\r\n<table>";
                    TExtraData += $"<tr><td>DCI4CRM</td><td>{Server_CRM.Status}  {(DCI4CRMModified ? "***" : "")}</td><td>{Server_CRM.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat)}</td></tr>";
                    TExtraData += $"<tr><td>DCI4WTP</td><td>{Server_WTP.Status}  {(WTPModified ? "***" : "")}</td><td>{Server_WTP.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat)}</td></tr>";
                    TExtraData += $"<tr><td>OBI4POS</td><td>{Server_POS.Status}  {(POSModified ? "***" : "")}</td><td>{Server_POS.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat)}</td></tr>";
                    TExtraData += $"<tr><td>POSSync</td><td>{Server_SS.Status}  {(SSModified ? "***" : "")}</td><td>{Server_SS.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat)}</td></tr>";
                    TExtraData += $"<tr><td>WTPSI</td><td>{Server_SI.Status}  {(SIModified ? "***" : "")}</td><td>{Server_SI.LastChanged.Value.ToString(Mirror.AxessDateTimeFormat)}</td></tr></table>";
                    EMail.SendNotification("SERVER STATUS CHANGE", TExtraData);
                    if (AsgardModified || BuyModified || DataWarehouseModified || MirrorModified)
                    {
                        EMail.SendNotification("SERVER STATUS CHANGE LO", TExtraData);
                    }
                }
            }
            if (cboRefreshRate.SelectedIndex < 0)
            {
                cboRefreshRate.SelectedIndex = 3;
            }
            else
            {
                SetTimer();
                timer1.Enabled = true;
            }
            ASG.SetTaskStatus("ServerMonitor", "", "", "Waiting for timer...", cboRefreshRate.SelectedItem.ToString());
            cboRefreshRate.Enabled = true;
            button1.Enabled = true;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            UpdateServerStatus();
        }

        private void SetTimer()
        {
            if (DateTime.Now.Hour >= 6 && DateTime.Now.Hour <= 18)
            {
                switch (cboRefreshRate.SelectedIndex)
                {
                    case 1: //2 minutes
                        timer1.Interval = 120000;
                        break;
                    case 2: //3 minutes
                        timer1.Interval = 180000;
                        break;
                    case 3: //5 minutes
                        timer1.Interval = 300000;
                        break;
                    case 4: //10 minutes
                        timer1.Interval = 600000;
                        break;
                    case 5: //30 minutes
                        timer1.Interval = 1800000;
                        break;
                    default: //1 minute or anything unexpected
                        timer1.Interval = 60000;
                        break;
                }
            }
            else
            {
                timer1.Interval = 300000;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            UpdateServerStatus();
        }

        private void CboRefreshRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            SetTimer();
            timer1.Enabled = true;
        }
    }
}
