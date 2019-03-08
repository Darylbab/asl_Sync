using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Net;
using asl_SyncLibrary;
using System.ComponentModel;

namespace Liftopia
{
    public partial class Liftopia : Form
    {

        private string AltaURL = @"https://www.liftopia.com/x/reports/guests.php?PartnerId=141&ApiKey=6EF2313B4B6D6A2BFFF24BDB83662&v=2";
        private string AltaDataFile = "AltaGuests";
        private string AltaDataPath = @"\\taskman\c$\acctg\winscp\data\AltaGuests.csv";

        private string MTCURL = @"https://extranet.liftopia.com/x/reports/passholders.php?ResortId=801001&ApiKey=F88E77D3D9B18A6F3E8ABC4D42929";
        private string MTCDataFile = "MTCGuests";
        private string MTCDataPath = @"\\taskman\c$\acctg\winscp\data\MTCGuests.csv";

        public DateTime MTCSalesStart = new DateTime(2018, 3, 1);
        private DateTime MTCSalesStartPrevious = new DateTime(2017, 3, 1);
        private DateTime MTCSalesStartNext = new DateTime(2019, 3, 1);

        private Asgard ASG = new Asgard();
        private ASLError myError = new ASLError();
        private CommonFunctions CF = new CommonFunctions();
        private DataWarehouse DW = new DataWarehouse();
        private Mirror AM = new Mirror();
        private BUY_ALTA_COM Buy = new BUY_ALTA_COM();

        private bool Alta = true;
        private bool MTC = true;

        private string CurrentFunction = "";

        public Liftopia(string[] args = null)
        {
            InitializeComponent();
            if (args != null)
            {
                switch (args[0].ToUpper())
                {
                    case "ALTA":
                        MTC = false;
                        break;
                    case "MTC":
                        Alta = false;
                        break;
                }
            }

        }

        public bool GetLiftopiaAltaFile()
        {
            CurrentFunction = "GetLiftopiaAltaFile";
            statusStrip1.Items[0].Text = "Preparing Liftopia Alta Import...";
            SetTaskStatus();
            WebRequest request = WebRequest.Create(AltaURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            File.WriteAllText(AltaDataPath, reader.ReadToEnd().Replace("\0", string.Empty));
            response.Close();
            reader.Close();
            statusStrip1.Items[0].Text = "Updating Willcall...";
            SetTaskStatus();
            string tQuery = string.Empty;
            DataSet tDS = CF.CSV2DS(AltaDataPath, AltaDataFile, new DataSet());
            int tRowCount = tDS.Tables[AltaDataFile].Rows.Count;
            pbAlta.Maximum = tRowCount;
            DataTable Willcall = CF.LoadTable(DW.dwConn, $"SELECT res_no, itemstatus, arr_date FROM {DW.ActiveDatabase}.willcall", "Willcall");
            DataColumn[] WillcallKey = new DataColumn[1];
            WillcallKey[0] = Willcall.Columns["res_no"];
            Willcall.PrimaryKey = WillcallKey;
            foreach (DataRow tRow in tDS.Tables[AltaDataFile].Rows)
            {
                tQuery = string.Empty;
                pbAlta.Value = tDS.Tables[AltaDataFile].Rows.IndexOf(tRow) + 1;
                SetTaskStatus("", "", pbAlta);
                if (tRow["guest_first_name"].ToString().Trim() != "ADULT 1" && tRow["guest_last_name"].ToString().Trim() != "ADULT 1 " && tRow["net_rate_revenue"].ToString() != string.Empty)
                {
                    bool isPaid = (tRow["order_status"].ToString().ToUpper() == "PAID");
                    DateTime TripDate = Convert.ToDateTime(tRow["trip_date"].ToString());
                    string TickID = tRow["ticket_id"].ToString();
                    //DataRow oldRow = CF.LoadDataRow(DW.dwConn, $"SELECT itemstatus, arr_date FROM {DW.ActiveDatabase}.willcall WHERE res_no='{TickID}' LIMIT 1");
                    DataRow oldRow = Willcall.Rows.Find(TickID);
                    if (CF.RowHasData(oldRow))
                    {
                        if (!isPaid)
                        {
                            string oldStatus = oldRow["itemstatus"].ToString().Trim();
                            if (oldStatus == "U")
                            {
                                tQuery += "itemstatus='C'";
                            }
                            else
                            {
                                if (oldStatus != "C")
                                {
                                    myError.UpdateErrorLog("ExternalSources", -668, $"Res_No={TickID} is trying to cancel an issued or refunded ticket.", "GetLiftopiaAltaFile");
                                }
                            }
                        }
                        if (oldRow.Field<DateTime>("arr_date") != TripDate)
                        {

                            tQuery += $"{(tQuery.Length != 0 ? ", " : "")}arr_date='{TripDate.ToString(Mirror.AxessDateFormat)}'";
                        }
                        if (tQuery.Length != 0)
                        {
                            tQuery = $"UPDATE {DW.ActiveDatabase}.willcall SET {tQuery} WHERE res_no='{TickID}'";
                        }
                    }
                    else
                    {
                        tDS = CF.LoadDataSet(DW.dwConn, $"SELECT nticktype, nperstype FROM {DW.ActiveDatabase}.tickpersxref WHERE extsource='LIFTOPIA_ALTA' AND extcode='{tRow["product"].ToString().Substring(0, 6)}{tRow["ticket_type"].ToString().Substring(0, 1)}'", tDS, "tickpersxref");
                        if (tDS.Tables.Contains("tickpersxref"))
                        {
                            string nperstypenr = tDS.Tables["tickpersxref"].Rows[0].Field<int>("nperstype").ToString();
                            string nticktypenr = tDS.Tables["tickpersxref"].Rows[0].Field<int>("nticktype").ToString();
                            string nPersType = CF.GetSQLField(AM.MirrorConn, $"SELECT szname FROM axess_cwc.tabperstypdef WHERE nperstypnr='{nperstypenr}'").ToUpper();
                            string nTickType = CF.GetSQLField(AM.MirrorConn, $"SELECT szname FROM axess_cwc.tabkundenkartentypdef WHERE nkundenkartentypnr='{nticktypenr}'").ToUpper();
                            tQuery = $"INSERT INTO {DW.ActiveDatabase}.willcall (res_no, arr_date, tickcode, tickdesc, persdesc, firstname, lastname, qty, uses, resprice, axtotal, itemstatus, company, bookdate, arcustid, arname, purfname, purlname, puremail, purstreet, purcity, purstate, purzip, ordernbr) VALUES ('";
                            tQuery += $"{tRow["ticket_id"].ToString()}','{Convert.ToDateTime(tRow["trip_date"].ToString()).ToString(Mirror.AxessDateFormat)}','NEW','{nTickType}','{nPersType}','";
                            tQuery += $"{CF.EscapeChar(tRow["guest_first_name"].ToString().ToUpper())}','{CF.EscapeChar(tRow["guest_last_name"].ToString().ToUpper())}',1,0,{Convert.ToDouble(tRow["net_rate_revenue"].ToString())},{Convert.ToDouble(tRow["net_rate_revenue"].ToString())},'";
                            tQuery += $"{(isPaid ? "U" : "C")}','LT','{Convert.ToDateTime(tRow["order_date"].ToString()).ToString(Mirror.AxessDateFormat)}',11733,'LIFTOPIA RESERVATIONS','";
                            tQuery += $"{CF.EscapeChar(tRow["purchaser_first_name"].ToString().ToUpper())}','{CF.EscapeChar(tRow["purchaser_last_name"].ToString().ToUpper())}','{tRow["purchaser_email"].ToString()}','{CF.EscapeChar(tRow["purchaser_address"].ToString())}','";
                            tQuery += $"{CF.EscapeChar(tRow["purchaser_city"].ToString().ToUpper())}','{tRow["purchaser_state"].ToString().ToUpper()}','{tRow["purchaser_zip"].ToString()}','{tRow["order_id"].ToString()}')";
                            tDS.Tables.Remove("tickpersxref");
                        }
                    }
                }

                if (tQuery.Length != 0)
                {
                    if (!CF.ExecuteSQL(DW.dwConn, tQuery))
                    {
                        return false;
                    }
                }
            }
            statusStrip1.Items[0].Text = string.Empty;
            return true;
        }

        public bool GetLiftopiaMTCFile(DateTime? StartDate = null, DateTime? EndDate = null)
        {
            CurrentFunction = "GetLiftopiaMTCFile";
            statusStrip1.Items[0].Text = "Preparing Liftopia MTC Import...";
            SetTaskStatus();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            if (StartDate != null)
            {
                MTCURL += @"&orderDateGreaterThan=" + StartDate.Value.AddDays(-1).ToString(Mirror.AxessDateFormat);
            }
            if (EndDate != null)
            {
                if (EndDate >= StartDate)
                {
                    MTCURL += @"&orderDateLessThan=" + EndDate.Value.AddDays(1).ToString(Mirror.AxessDateFormat);
                }
            }
            WebRequest request = WebRequest.Create(MTCURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            File.WriteAllText(MTCDataPath, reader.ReadToEnd().Replace("\0", string.Empty));
            response.Close();
            response.Dispose();
            reader.Close();
            reader.Dispose();
            statusStrip1.Items[0].Text = "Updating MTC_Sales...";
            SetTaskStatus();
            CommonFunctions cf = new CommonFunctions();
            DataWarehouse dw = new DataWarehouse();
            DataSet myDS = cf.CSV2DS(MTCDataPath, MTCDataFile, new DataSet());
            int tRowCount = myDS.Tables[MTCDataFile].Rows.Count;
            pbMTC.Maximum = tRowCount;
            foreach (DataRow myRow in myDS.Tables[MTCDataFile].Rows)
            {
                pbMTC.Value = myDS.Tables[MTCDataFile].Rows.IndexOf(myRow) + 1;
                System.Diagnostics.Debug.Print(pbMTC.Value.ToString() + " of " + myDS.Tables[MTCDataFile].Rows.Count.ToString());
                //if (myRow["Order Id"].ToString() == "11490976")
                //if (pbMTC.Value > 59999  || myRow.Field<string>("pre arrival approved").ToUpper() == "YES" || myRow.Field<string>("order status").ToUpper() == "CANCELLED") // || myRow["estimated arrival date"].ToString() != string.Empty)
                {
                    //SetTaskStatus("", "", pbMTC);
                    string GFN = myRow["guest first name"].ToString().Replace("â€™", "'").Replace("Ã©", "e").Replace("Ã¤", "a").Replace("/", "").Replace(@"&","").Trim();
                    string GLN = myRow["guest last name"].ToString().Replace("â€™", "'").Replace("Ã©", "e").Replace("Ã¤", "a").Replace("/", "").Trim();
                    string GEM = myRow["guest email"].ToString();
                    string GN = GFN + " " + GLN;
                    string dob = myRow["guest birth date"].ToString();
                    bool legalName = (myRow["guest last name"].ToString().ToUpper().Trim() != "INVALID");
                    foreach (char a in GN)
                    {
                        legalName |= ((int)a <= 127);
                    }
                    if (myRow["product name"].ToString().Contains(cf.Season.Replace("-", "/")) && legalName)
                    {
                        //if (myRow.Field<int>("recid") > 572739)
                        {
                            if (Buy.SalesFromMTC(myRow))    //test for row existing and either update or insert 
                            {
                                DataSet tDS = CF.LoadDataSet(AM.MirrorConn, $"SELECT * FROM axess_cwc.tabpersonen WHERE SZNAME = '{CF.EscapeChar(GLN)}' AND dtgeburt = '{dob}' AND nperskassanr <= 40", new DataSet(), "tabPersonen");
                                if (tDS.Tables.Contains("tabPersonen"))
                                {
                                    if (tDS.Tables["tabPersonen"].Rows.Count != 0)
                                    {
                                        int FoundRow = 0;
                                        bool IsFound = (tDS.Tables["tabPersonen"].Rows.Count == 1);
                                        if (!IsFound)
                                        {
                                            foreach (DataRow Personen in tDS.Tables["tabPersonen"].Rows)
                                            {
                                                if (Personen["szvorname"].ToString().ToUpper() == GFN.ToUpper())
                                                {
                                                    FoundRow = tDS.Tables["tabPersonen"].Rows.IndexOf(Personen);
                                                    IsFound = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (IsFound)
                                        {
                                            int nPersNr = tDS.Tables["tabPersonen"].Rows[FoundRow].Field<int>("nPersNr");
                                            int nKassaNr = tDS.Tables["tabPersonen"].Rows[FoundRow].Field<Int16>("nPersKassaNr");
                                            if (nKassaNr <= 40 || (nKassaNr >= 117 && nKassaNr <= 120))
                                            {
                                                cf.ExecuteSQL(Buy.Buy_Alta_ComConn, $"UPDATE asbshared.mtncol_sales SET axess_perskassa={nKassaNr.ToString()}, axess_persnr={nPersNr.ToString()} WHERE mcpnbr = '{myRow.Field<string>("Barcode")}'");
                                            }
                                        }
                                    }
                                }
                                //dw.WillcallFromMTC(myRow);
                            }
                        }
                    }
                }
                }
            return true;
        }

        private void Liftopia_Shown(object sender, EventArgs e)
        {
            CurrentFunction = "Liftopia_Shown";
            SetTaskStatus();
            DateTime StartTime = DateTime.Now;
            lblStartTime.Text = $"{StartTime.ToString("MM-dd-yyyy HH:mm:ss")}";
            if (Alta)
            {
                GetLiftopiaAltaFile();
            }
            if (MTC)
            {
                GetLiftopiaMTCFile(MTCSalesStart,DateTime.Today);
            }
            statusStrip1.Items[0].Text = "COMPLETE";
            SetTaskStatus("", DateTime.Now.ToString(Mirror.AxessDateTimeFormat));
            Application.Exit();
        }

        private void SetTaskStatus(string Description = "", string Extra = "", ProgressBar PB1 = null, ProgressBar PB2 = null)
        {
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
            ASG.SetTaskStatus(Application.ProductName, CurrentFunction, SSLStatus.Text, Description, Extra);
        }


    }
}
