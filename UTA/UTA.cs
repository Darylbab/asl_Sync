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

namespace UTA
{
    public partial class UTA : Form
    {
        private const string UTAFolder = @"\\alta.com\namespace\APPS\FPData\extsync\uta";
        private const string UTABulkEmail = "efcbulkimport@rideuta.com";
        private const string EmailFromAddress = "darylb@alta.com";
        private DateTime UTABegin = new DateTime(2018, 12, 1);
        private DateTime UTAEnd = new DateTime(2019, 5, 6);

        private Asgard ASG = new Asgard();
        private CommonFunctions CF = new CommonFunctions();
        private DataWarehouse DW = new DataWarehouse();

        public UTA(string[] args = null)
        {
            InitializeComponent();
            //if (args != null)
            //{
            //    switch (args[0].ToUpper())
            //    {
            //        case "SEND":
            //        break;
            //    case "RESET":
                    //UTASend(false, true);
            //        break;
            //}
            //}
        }

        public bool UTASend(bool aSendAll = false, bool aReset = false)
        {
            //set UTAFlag in SPCards.
            string tQuery = $"UPDATE {DW.ActiveDatabase}.spcards SET S.utaflag = 'N'";
            //tQuery = $"SELECT concat(substr(mediaid,15,2), substr(mediaid,13,2), substr(mediaid,11,2), substr(mediaid,9,2), substr(mediaid,7,2), substr(mediaid,5,2), substr(mediaid,3,2), substr(mediaid,1,2)) AS ChipID, 'DEACTIVATE' as status, '{UTABegin.ToString(CF.FileNameDateFormat)}' AS UTAStartDate,'{UTAEnd.ToString(CF.FileNameDateFormat)}' AS UTAEndDate, nlfdzaehle, nticktype, nperstype, tickdesc, persdesc FROM {DW.ActiveDatabase}.spcards AS S INNER JOIN {DW.ActiveDatabase}.tickettypes AS T ON T.ticktype=S.nticktype WHERE S.saledate > '2018-04-14' AND S.utaflag='Y' AND T.utaflag='N' AND LENGTH(S.MediaID) = 16";
            CF.ExecuteSQL(DW.dwConn, tQuery);
            tQuery = $"UPDATE {DW.ActiveDatabase}.spcards SET utaflag='Y' WHERE nticktype=3000 OR nperstype IN (30071,30072,30073,30074)";
            CF.ExecuteSQL(DW.dwConn, tQuery);

            string tDBName = "utapass";
            string tDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            string tUTAFileName = @"\alta_" + tDate.Replace("-", "").Replace(" ","_").Replace(":","") + ".csv";
            string tMailFileName = UTAFolder + tUTAFileName;
            tQuery = $"SELECT concat(substr(mediaid,15,2), substr(mediaid,13,2), substr(mediaid,11,2), substr(mediaid,9,2), substr(mediaid,7,2), substr(mediaid,5,2), substr(mediaid,3,2), substr(mediaid,1,2)) AS ChipID, ";
            tQuery += aReset ? "'DEACTIVATE'" : "CASE cardstatus WHEN 'A' THEN 'ACTIVATE' ELSE 'DEACTIVATE' END";
            tQuery += $" AS status, '{UTABegin.ToString(CF.FileNameDateFormat)}' AS UTAStartDate,'{UTAEnd.ToString(CF.FileNameDateFormat)}' AS UTAEndDate, nlfdzaehle, nticktype, nperstype, tickdesc, persdesc FROM {DW.ActiveDatabase}.spcards WHERE saledate > '2018-04-14' AND length(mediaid)=16 AND testflag=false AND utaflag='Y' OR ((nticktype=144 AND nperstype BETWEEN 510 AND 550) OR (nperstype IN (30059, 30071, 30072, 30073, 30074))" ;
            if (!(aSendAll || aReset))
            {
                tQuery += " AND utasent=false";
            }
            tQuery += " ORDER BY saledate";
            using (DataSet tDS = CF.LoadDataSet(DW.dwConn, tQuery, new DataSet(), tDBName))
            {
                if (tDS.Tables.Contains(tDBName))
                {
                    //create CSV, UTA does not want a header row.
                    CF.Table2CSV(tDS.Tables[tDBName], tMailFileName, false);        
                    //save a copy to the IT folder
                    CF.Table2CSV(tDS.Tables[tDBName], CF.ITDataFolder + tUTAFileName, false);
                    //only send an email if there is something to send
                    if (tDS.Tables[tDBName].Rows.Count != 0)
                    {
                        //send email with attachment
                        ASLEmail tEmail = new ASLEmail()
                        {
                            BCCAddress = EmailFromAddress,
                            ToAddress = UTABulkEmail,
                            FromAddress = EmailFromAddress,
                            Subject = $"ALTA RFID Passes - {tDate}",
                            Body = $"RFID chip file for Alta Ski Area as of {tDate}."
                        };
                        tEmail.CreateEmail();
                        tEmail.Attach(tMailFileName);
                        tEmail.SendEmail();
                    }
                }
            }
            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards SET utasent=true WHERE utaflag='Y' AND LENGTH(mediaid)=16");
            return true;
        }

        private void UTA_Shown(object sender, EventArgs e)
        {
            UTASend();
            Application.Exit();
        }
    }
}
