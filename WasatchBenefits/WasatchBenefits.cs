using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using asl_SyncLibrary;

namespace WasatchBenefits
{
    public partial class WABA : Form
    {
        private Asgard ASG = new Asgard();
        private DataWarehouse DW = new DataWarehouse();
        private CommonFunctions CF = new CommonFunctions();

        private string CurrentFunction = string.Empty;

        private string[,] WB_Resorts = new string[,] { { "snowbird", "20" } }; //, { "deer valley", "30" }, { "solitude", "40" } };
        private DateTime WBDateEnd = new DateTime(2018, 5, 31);

        public WABA(string[] args = null)
        {
            InitializeComponent();
        }

        public bool UpdateWasatchBenefit()
        {
            CurrentFunction = "updateWasatchBenefit";
            for (Int32 I = 0; I<=WB_Resorts.GetUpperBound(0); I++)
            {
                string tResortName = WB_Resorts[I,0].ToUpper();
                string tTableName = $"WB_{tResortName}";
                string tResortFilter = WB_Resorts[I, 1];
                string tWBEnd = WBDateEnd.ToString(Mirror.AxessDateFormat);
                string tSeason = CF.SeasonShort;
                CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.clistperm SET okayflag=0 WHERE passcat='WB' AND company='{tResortName}'");
                string tFileName = $"{CF.DataFolder}{tResortName.Replace(" ", "")}.csv";
                File.Copy(tFileName, $"{CF.ITDataFolder}{tResortName.Replace(" ", "")}.csv", true);
                DataSet tDS2 = CF.CSV2DS(tFileName, tTableName, new DataSet());
                if (tDS2.Tables.Contains(tTableName))
                {
                    tDS2.Tables[tTableName].Columns[0].ColumnName = "ResortID";
                    tDS2.Tables[tTableName].Columns[1].ColumnName = "PersKey";
                    tDS2.Tables[tTableName].Columns[2].ColumnName = "FirstName";
                    tDS2.Tables[tTableName].Columns[3].ColumnName = "LastName";
                    tDS2.Tables[tTableName].Columns[4].ColumnName = "City";
                    tDS2.Tables[tTableName].Columns[5].ColumnName = "Age";
                    tDS2.Tables[tTableName].Columns[6].ColumnName = "PassNbr";
                    tDS2.Tables[tTableName].Columns[7].ColumnName = "Barcode";
                    tDS2.Tables[tTableName].Columns[8].ColumnName = "Status";
                    tDS2.Tables[tTableName].Columns.Add("Uses");
                    tDS2.Tables[tTableName].Columns.Add("Resortname");
                    tDS2.Tables[tTableName].Columns.Add("dob");
                    tDS2.Tables[tTableName].Columns.Add("nKartenNr");
                    tDS2.Tables[tTableName].Columns.Add("cStat");
                    tDS2.Tables[tTableName].Columns.Add("PersKassa");
                    tDS2.Tables[tTableName].Columns.Add("PersNr");
                    tDS2.Tables[tTableName].Columns.Add("MtnFlag");
                    tDS2.Tables[tTableName].Columns.Add("WasBenFlag");
                    tDS2.Tables[tTableName].Columns.Add("MediaID");
                    tDS2.Tables[tTableName].Columns.Add("wtp64");
                    tDS2.Tables[tTableName].Columns.Add("CardStatus");
                    foreach (DataRow tRow in tDS2.Tables[tTableName].Rows)
                    {
                        string tFName = tRow["FirstName"].ToString();
                        string tLName = tRow["LastName"].ToString().ToString();
                        if (((tRow["Perskey"] != null) || (tRow["ResortID"].ToString().Trim() != tResortFilter)) && (tFName.Length != 0) && (tLName.Length != 0))
                        {
                            string tPassKey = $"{tRow["ResortID"]}-{tRow["PersKey"]}";
                            if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.clistperm", $"passkey='{tPassKey}'"))    //update or insert?
                            {   //update
                                if (tRow["status"].ToString() != "1")
                                    CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.clistperm SET okayflag=false WHERE company='{tResortName}'", 300);
                            }
                            else
                            {   //insert
                                Int32 tAge = tRow.Field<Int32?>("age") == null ? 0 : tRow.Field<Int32>("age");
                                string tQuery2 = $"INSERT INTO {DW.ActiveDatabase}.clistperm (dexpires,season,lname,fname,category,reason,passkey,passcat,dent,passtype,tickets,barcode,passnbr,tickdesc,media,appby,dept,okayflag,company,maxuses,price,tickpers,uses,extuses) VALUES ('";
                                tQuery2 += $"{tWBEnd}','{tSeason}','{CF.EscapeChar(tRow["LastName"].ToString().ToUpper().Trim())}','{CF.EscapeChar(tRow["FirstName"].ToString().ToUpper().Trim())}','{tResortName}','WASATCH BENEFIT','{tPassKey}','WB','";
                                tQuery2 += $"{DateTime.Now.ToString(Mirror.AxessDateFormat)}','AD',2,'{tRow["barcode"].ToString().Trim()}','{tRow["PassNbr"].ToString().Trim()}','ALTA AREA DAY','ALTA ONE USE','WASATCH BENEFIT','ADMIN',True,'";
                                tQuery2 += $"{tResortName}',2,0,'WB {((tAge <= 12) && (tAge > 0) ? "CHILD" : "ADULT")}',0,0)";
                                CF.ExecuteSQL(DW.dwConn, tQuery2);
                            }
                        }
                    }
                    tDS2.Tables.Remove(tTableName);
                }
            }
            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.clistperm AS a INNER JOIN (SELECT COUNT(*) AS tt, passkey FROM {DW.ActiveDatabase}.clist WHERE tickdesc = 'WASATCH BENEFIT' GROUP BY passkey) b ON a.passkey = b.passkey SET uses = b.tt WHERE a.reason = 'WASATCH BENEFIT'");

            string tQuery = $"SELECT 10 AS resortid, perskey, UPPER(firstname) AS firstname,  UPPER(replace(lastname,',','')) AS lastname, UPPER(replace(city,',','')) AS city, CAST((DATEDIFF(now(), dob) / 365) AS signed) AS age, mediaid, wtp64, 1 AS status FROM {DW.ActiveDatabase}.spcards WHERE wasbenflag = 'Y' AND cardstatus='A' AND testflag=false";
            string tFileName2 = @"\\taskman\c$\acctg\winscp\data\alta.csv";
            DataSet tDS = CF.LoadDataSet(DW.dwConn, tQuery, new DataSet(), "spcards");
            if (tDS.Tables.Contains("spcards"))
            {
                CF.Table2CSV(tDS.Tables["spcards"], tFileName2, true);
                CF.Table2CSV(tDS.Tables["spcards"], CF.ITDataFolder + "alta.csv", true);
            }
            return true;
        }

        private void WasatchBenefits_Shown(object sender, EventArgs e)
        {
            LblStartTime.Text = DateTime.Now.ToString(Mirror.AxessDateTimeFormat);
            UpdateWasatchBenefit();
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
            ASG.SetTaskStatus(Application.ProductName, CurrentFunction, statusStrip1.Items[0].Text, Description, Extra);
        }

    }
}
