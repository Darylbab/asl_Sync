using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using asl_SyncLibrary;
using MySql.Data.MySqlClient;


namespace SportsRental
{
    public partial class SportsRental : Form
    {
        private const string TaskName = "SportsRental";
        private string SRConnectionString = @"Data Source=altajava\srsql;Initial Catalog=SR.DB;Integrated Security=True";

        private const string SRGroupNo = "1901000003";

        private string tRabattP;
        private string tRabattVerkaufP;
        private string tZIMMER;

        private DataRow TaskHistory;
        private DataRow TaskStatus;
        private DataRow TaskTemplate;

        private static Asgard ASG = new Asgard();
        private Axess_DCI4CRM Axess = new Axess_DCI4CRM();
        private BUY_ALTA_COM BUY = new BUY_ALTA_COM();
        private CommonFunctions CF = new CommonFunctions();
        private DataWarehouse DW = new DataWarehouse();

        int rentaldiscount = 0;
        int retaildiscount = 0;

        public SportsRental()
        {
            InitializeComponent();
            TaskTemplate = ASG.GetTaskTemplate(TaskName);
            if (TaskTemplate["Name"].ToString() != TaskName)
            {
                // no template for this task, just run
                txtLastRun.Text = "No Template";
                txtNextRun.Text = "No Template";
                txtLastRunResults.Text = "Unknown";
            }
            else
            {
                TaskHistory = ASG.GetTaskHistory(TaskName);
                ASG.SetTaskStatus(TaskName);
                TaskStatus = ASG.GetTaskStatus(TaskName);
                if (TaskHistory != null)
                {
                    txtLastRun.Text = TaskHistory.Field<DateTime>("StartTime").ToString(Mirror.AxessDateTimeFormat);
                }
                else
                {
                    txtLastRun.Text = string.Empty;
                }
                txtNextRun.Text = TaskHistory.Field<DateTime>("StartTime").AddMinutes(TaskTemplate.Field<Int32>("period")).ToString(Mirror.AxessDateTimeFormat);
            }
        }

        private void BtnRunNow_Click(object sender, EventArgs e)
        {
            txtLastRun.Enabled = false;
            txtLastRunResults.Enabled = false;
            txtNextRun.Enabled = false;
            BtnRunNow.Enabled = false;
            Cursor tCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            SPCards2Kunde();
            Cursor.Current = tCursor;
            txtLastRun.Enabled = true;
            txtLastRunResults.Enabled = true;
            txtNextRun.Enabled = true;
            BtnRunNow.Enabled = true;
        }

        public bool SPCards2Kunde()
        {
            //  New cards and changes
            string tQuery = $"SELECT * FROM {DW.ActiveDatabase}.spcards AS A WHERE tgroup <> 'CC' AND lastname<>'TEST' AND firstname<>'TEST' AND LENGTH(mediaid) = 16 AND concat(lastname , firstname) <> '' AND (SELECT COUNT(*)FROM {DW.ActiveDatabase}.spcards WHERE mediaid=A.mediaid AND cardstatus = 'A') = 1 ORDER BY mediaid, nlfdzaehle";
            //tQuery = $"SELECT* FROM { DW.ActiveDatabase}.spcards WHERE serialkey IN  ('14-71314-1','18-66436-2')";
            DataSet tDS = CF.LoadDataSet(DW.dwConn, tQuery, new DataSet(), "spcards");
            if (tDS.Tables.Contains("spcards"))
            {
                foreach (DataRow tRow in tDS.Tables["spcards"].Rows)
                {
                    ASG.SetTaskStatus("SportsRental", "SPCards2Kunde", "", "", "", tDS.Tables["spcards"].Rows.IndexOf(tRow), tDS.Tables["spcards"].Rows.Count);
                    string tSBCode = tRow["mediaid"].ToString().Trim();
                    bool PassActive = (tRow["cardstatus"].ToString().Trim().ToUpper() == "A");
                    tZIMMER = ((tRow["chrgflag"].ToString().Trim() == "Y") && (tRow["tokenkey"].ToString().Trim() != string.Empty)) ? "Yes" : "No";

                    string tLastname = CF.EscapeChar(tRow["lastname"].ToString().Trim().ToUpper());
                    if (tLastname.Length > 30) tLastname = tLastname.Substring(0, 30);
                    string tFirstname = CF.EscapeChar(tRow["firstname"].ToString().Trim().ToUpper());
                    if (tFirstname.Length > 30) tLastname = tFirstname.Substring(0, 30);
                    string tStreet = CF.EscapeChar(tRow["street"].ToString().Trim().ToUpper());
                    if (tStreet.Length > 30) tStreet = tStreet.Substring(0, 30);
                    string tState = CF.EscapeChar(tRow["state"].ToString().Trim().ToUpper());
                    if (tState.Length > 3) tState = tState.Substring(0, 3);
                    string tZip = CF.EscapeChar(tRow["zip"].ToString().Trim().ToUpper());
                    if (tZip.Length > 10) tZip = tZip.Substring(0, 10);
                    string tDOB = Convert.ToDateTime(tRow["dob"].ToString()).ToString(Mirror.AxessDateFormat);
                    if (Convert.ToDateTime(tDOB).Year < 1916)
                        tDOB = CF.defaultDOB.ToString(Mirror.AxessDateFormat);

                    string tSQL = string.Empty;
                    string tWhere = $"WHERE name='{tLastname}' AND vorname='{tFirstname}' AND geburtsdatum='{tDOB}'";
                    SetDiscounts(tRow["disctype"].ToString().Trim());
                    tDS = CF.LoadDataSet(new SqlConnection(SRConnectionString), $"SELECT ZIMMER, RabattP, RabattVerkaufP, Gruppe_ID, v_kunde_id FROM Kunde {tWhere}", tDS, "Zimmer");
                    if (tDS.Tables["Zimmer"].Rows.Count != 0)
                    {
                        bool isModified = tZIMMER.ToUpper() != tDS.Tables["Zimmer"].Rows[0].Field<string>("ZIMMER").ToUpper();
                        isModified |= tRabattP != tDS.Tables["Zimmer"].Rows[0].Field<Int16>("RabattP").ToString();
                        isModified |= tRabattVerkaufP != tDS.Tables["Zimmer"].Rows[0].Field<Int16>("RabattVerkaufP").ToString();
                        isModified |= SRGroupNo != tDS.Tables["Zimmer"].Rows[0].Field<Int64>("Gruppe_ID").ToString();

                        if (isModified)
                        {
                            tSQL = "UPDATE [sr.db].[dbo].[Kunde] SET bemerkung=";
                            if (!PassActive)
                            {
                                tSQL += $"'NON CHARGE', RabattP=0, RabattVerkaufP=0, Gruppe_ID=0, ZIMMER='No', v_kunde_id='{tSBCode}' ";
                            }
                            else
                            {
                                tSQL += $"'', RabattP={tRabattP}, RabattVerkaufP={tRabattVerkaufP}, name='{tLastname}', vorname='{tFirstname}', Geburtsdatum='{tDOB}', Gruppe_ID={SRGroupNo} , ZIMMER='{tZIMMER}', v_kunde_id='{tSBCode}' ";
                            }
                            tSQL += tWhere;
                        }
                    }
                    else
                    {
                        string tInvoice = (Convert.ToInt64(CF.GetSQLField(new SqlConnection(SRConnectionString), "SELECT TOP 1 Kunde_ID FROM [sr.db].[dbo].[Kunde] WHERE Kunde_ID < 999000000000000000 ORDER BY Kunde_ID DESC")) + 1).ToString();
                        string dateinsertval = DateTime.Now.ToString(Mirror.AxessDateTimeFormat);
                        if (PassActive)
                        {
                            tSQL = $"INSERT INTO [sr.db].[dbo].[Kunde] (AenderungsDatum, Datum, Geburtsdatum, Gruppe_ID, Kunde_ID, Name, RabattP, RabattVerkaufP, Telefon, V_Kunde_ID, Vorname, Notiz, Strasse, Staat, postleitzahl, ZIMMER) ";
                            tSQL += $" VALUES ('{dateinsertval}','{dateinsertval}','{tDOB}', '{SRGroupNo}', '{tInvoice}','{tLastname}', {tRabattP}, {tRabattVerkaufP},'8013591078','{tSBCode}','{tFirstname}','{tRow["wtp32"].ToString().Trim()}', '{tStreet}', '{tState}', '{tZip}', '{tZIMMER}')";
                        }
                    }
                    if (tSQL != string.Empty)
                    {
                        CF.ExecuteSQL(new SqlConnection(SRConnectionString), tSQL);
                    }
                    //now clean up blocked cards.
                }
            }
            if (tDS.Tables.Contains("Zimmer"))
            {
                tDS.Tables.Remove("Zimmer");
            }

            tQuery = $"SELECT mediaid FROM {DW.ActiveDatabase}.spcards AS A WHERE tgroup <> 'CC' AND lastname<>'TEST' AND firstname<>'TEST' AND LENGTH(mediaid) = 16 AND concat(lastname , firstname) <> '' AND (SELECT COUNT(*)FROM {DW.ActiveDatabase}.spcards WHERE mediaid=A.mediaid AND cardstatus = 'A') = 0 GROUP BY mediaid";
            using (DataTable tDT = CF.LoadTable(DW.dwConn, tQuery, "InactiveCards"))
            {
                if (tDT.Rows.Count != 0)
                {
                    foreach (DataRow tDR in tDT.Rows)
                    {
                        ASG.SetTaskStatus("SportsRental", "Asb_Passholder2Kunde", "", "", "", tDS.Tables["spcards"].Rows.IndexOf(tDR), tDT.Rows.Count);
                        string tWhere = $"V_Kunde_ID ='{tDR[0].ToString().Trim()}'";
                        if (CF.RowExists(new SqlConnection(SRConnectionString), "[sr.db].[dbo].[Kunde]", tWhere))
                        {
                            CF.ExecuteSQL(new SqlConnection(SRConnectionString), $"UPDATE [sr.db].[dbo].[Kunde] SET bemerkung = 'NON CHARGE', RabattP=0, RabattVerkaufP=0, Gruppe_ID=0, ZIMMER='No' WHERE {tWhere}");
                        }
                    }
                }
            }
            //clear expired cards
            tQuery = $"SELECT mediaid FROM {DW.ActiveDatabase}.spcards AS A WHERE cardstatus='X'";
            using (DataTable tDT = CF.LoadTable(DW.dwConn, tQuery, "ExpiredCards"))
            {
                if (tDT.Rows.Count != 0)
                {
                    foreach (DataRow tDR in tDT.Rows)
                    {
                        ASG.SetTaskStatus("SportsRental", "Asb_Passholder2Kunde", "Clear Expired Cards", "", "", tDT.Rows.IndexOf(tDR), tDT.Rows.Count);
                        string tWhere = $"V_Kunde_ID ='{tDR[0].ToString().Trim()}'";
                        if (CF.RowExists(new SqlConnection(SRConnectionString), "[sr.db].[dbo].[Kunde]", tWhere))
                        {
                            CF.ExecuteSQL(new SqlConnection(SRConnectionString), $"UPDATE [sr.db].[dbo].[Kunde] SET bemerkung = 'NON CHARGE', RabattP=0, RabattVerkaufP=0, Gruppe_ID=0, ZIMMER='No' WHERE {tWhere}");
                        }
                    }
                }
            }

            //check for no longer in SPCards.
            //tQuery = $"SELECT V_Kunde_ID FROM [sr.db].[dbo].[Kunde] GROUP BY V_Kunde_ID";
            //using (DataTable tDT = CF.LoadTable(new SqlConnection(SRConnectionString), tQuery, "PreviousCards"))
            //{
            //    if (tDT.Rows.Count != 0)
            //    {
            //        foreach (DataRow tDR in tDT.Rows)
            //        {
            //            StatusText(tDT.Rows.IndexOf(tDR).ToString() + " of " + tDT.Rows.Count.ToString());
            //            string tWhere = $"mediaid ='{tDR[0].ToString().Trim()}' AND cardstatus='A'";
            //            if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.SPCards", tWhere))
            //            {
            //                CF.ExecuteSQL(new SqlConnection(SRConnectionString), $"UPDATE [sr.db].[dbo].[Kunde] SET bemerkung = 'NON CHARGE', RabattP=0, RabattVerkaufP=0, Gruppe_ID=0, ZIMMER='No' WHERE {tWhere}");
            //            }
            //        }
            //    }
            //}

            ASG.SetTaskStatus("", "", "", "", "");
            return true;
        }

        private void SetDiscounts(string aDiscType)
        {
            switch (aDiscType)
            {
                case "E":
                    tRabattP = "30";
                    tRabattVerkaufP = "30";
                    break;
                case "C":
                    tRabattP = "20";
                    tRabattVerkaufP = "20";
                    break;
                default:
                    tRabattP = "20";
                    tRabattVerkaufP = "0";
                    break;
            }
        }


        // *** OCCASIONALLY USED FUNCTIONS  ***
        //    Usually these are run manually

        public bool TestDB()    //tested good
        {
            DataSet tDS = CF.LoadDataSet(new SqlConnection(SRConnectionString), "SELECT zimmer FROM [sr.db].[dbo].[kunde] WHERE name = 'X MGMT'", new DataSet(),  "kunde");
            if (tDS.Tables.Contains("kunde"))
            {
                tDS.Tables.Remove("kunde");
                return true;
            }
            return false;
        }

        public bool ResetForNewYear()   //tested good
        {
            return (CF.ExecuteSQL(new SqlConnection(SRConnectionString), "UPDATE [sr.db].[dbo].[kunde] SET zimmer='No', rabattp=0, rabattverkaufp=0 WHERE name != 'X MGMT'"));
        }


        // *** DEVELOPMENT AND COPIED FROM FOXPRO BUT NOT USED FUNCTIONS ***
        //                          good luck!!!

        public void GetDiscountValues()
        {
            rentaldiscount = 0;
            retaildiscount = 0;
            string myQuery = $"SELECT rentaldiscountpercentage, discountpercentage FROM alta_shared.skishopdiscount WHERE activediscount = 1 AND startdate <= '{DateTime.Now.ToString(Mirror.AxessDateFormat)}' AND enddate >= '{DateTime.Now.ToString(Mirror.AxessDateFormat)}' ORDER BY idskishopdiscount DESC LIMIT 1";
            DataSet DS = CF.LoadDataSet(DW.dwConn, myQuery, new DataSet(), "SRDiscounts");
            if (DS.Tables["SRDiscounts"].Rows.Count != 0)
            {
                DataRow tRow = DS.Tables["SRDiscounts"].Rows[0];
                rentaldiscount = Convert.ToInt32(tRow["rentaldiscountpercentage"].ToString());
                retaildiscount = Convert.ToInt32(tRow["discountpercentage"].ToString());
            }
        }

        public void Kunde2CSV()
        {
            DataSet tDS = CF.LoadDataSet(new SqlConnection(SRConnectionString), "SELECT * FROM Kunde", new DataSet(), "Kunde");
            CF.Table2CSV(tDS.Tables["Kunde"], @"C:\Users\darylb\Documents\SportRental\sr.csv", true, false);
        }

        public bool Asb_passholders2Kunde()
        {
            DataSet tDS = CF.LoadDataSet(BUY.Buy_Alta_ComConn, "SELECT I FROM asbshared.asb_passholders WHERE issued_by='Snowbird'", new DataSet(), "asb");
            foreach (DataRow tRow in tDS.Tables["asb"].Rows)
            {
                string tPassStatus = tRow["pass_status"].ToString().Trim();
                string tChrgflag = tRow["chrgflag"].ToString();
                string tSBCode = Axess.GetMediaID(tRow["wtp64"].ToString());
                string tTokenkey = "0";
                string tNkartennr = tRow["wtp32"].ToString().Trim();
                string tDisctype = tRow["disctype"].ToString();
                string tPersdesc = tRow["persdesc"].ToString();
                string tNkassanr = String.IsNullOrEmpty(tRow["nkassanr"].ToString().Trim()) ? "0" : tRow["nkassanr"].ToString().Trim();
                string tLastname = CF.EscapeChar(tRow["lastname"].ToString().Trim().ToUpper());
                if (tLastname.Length > 30) tLastname = tLastname.Substring(0, 30);
                string tFirstname = CF.EscapeChar(tRow["firstname"].ToString().Trim().ToUpper());
                if (tFirstname.Length > 30) tLastname = tFirstname.Substring(0, 30);
                string tStreet = CF.EscapeChar(tRow["street"].ToString().Trim().ToUpper());
                if (tStreet.Length > 30) tStreet = tStreet.Substring(0, 30);
                string tState = CF.EscapeChar(tRow["state"].ToString().Trim().ToUpper());
                if (tState.Length > 3) tState = tState.Substring(0, 3);
                string tZip = CF.EscapeChar(tRow["zip"].ToString().Trim().ToUpper());
                if (tZip.Length > 10) tZip = tZip.Substring(0, 10);
                string tDOB = Convert.ToDateTime(tRow["dob"].ToString()).ToString(Mirror.AxessDateFormat);
                if (Convert.ToDateTime(tDOB).Year < 1916) tDOB = "1969-12-31";
                string tSQL = string.Empty;
                bool tZimmer = (Convert.ToInt32(tTokenkey) != '0') && (tChrgflag == "Y") && (tPassStatus == "A");
                tDS = CF.LoadDataSet(new SqlConnection(SRConnectionString), "SELECT ZIMMER FROM Kunde WHERE V_Kunde_ID='" + tSBCode + "'", tDS, "Zimmer");
                if (!CF.RowExists(new SqlConnection(SRConnectionString), "Kunde", $"V_Kunde_ID='{tSBCode}'"))
                {
                    string tInvoice = (Convert.ToInt64(CF.GetSQLField(new SqlConnection(SRConnectionString), "SELECT TOP 1 Kunde_ID FROM [sr.db].[dbo].[Kunde] WHERE Kunde_ID < 999000000000000000 ORDER BY Kunde_ID DESC")) + 1).ToString();
                    string dateinsertval = DateTime.Now.ToString(Mirror.AxessDateTimeFormat);
                    if (tPassStatus == "A")
                    {
                        string insertnameval = (tFirstname + " " + tLastname);
                        string tStaat = tRow["state"].ToString();
                        string sqlval = insertnameval.Replace("'", "''");

                        tSQL = "INSERT INTO[sr.db].[dbo].[Kunde] (AenderungsDatum, Datum, Geburtsdatum, Gruppe_ID, Kunde_ID, Name, RabattP, RabattVerkaufP, Telefon, V_Kunde_ID, Vorname, Notiz, Strasse, Staat, postleitzahl, ZIMMER) VALUES('" + dateinsertval + "','" + dateinsertval + "','" + tDOB + "', '" + SRGroupNo + "', '" + tInvoice + "','" + tLastname + "', 20, 0,'8013591078','" + tSBCode + "','" + tFirstname + "','" + tNkartennr + "', '" + tStreet + "', '" + tState + "', '" + tZip + "', 'No')";
                    }
                }
                CF.ExecuteSQL(new SqlConnection(SRConnectionString), tSQL);
                ASG.SetTaskStatus("SportsRental", "Asb_Passholder2Kunde", "", "", "", tDS.Tables["spcards"].Rows.IndexOf(tRow), tDS.Tables["spcards"].Rows.Count);
            }
            return true;
        }


        private void PopulateSpList()
        {
            /*spcCon.Open();
            OleDbCommand myCommand = new OleDbCommand(@"SELECT * FROM spcards WHERE tgroup = 'SP'", spcCon);
            OleDbDataReader myReader = myCommand.ExecuteReader();
            */
            if (CF.OpenConn(DW.dwConn) == ConnectionState.Open)
            {
                string myQuery = "SELECT * FROM spcards WHERE tgroup <> 'CC'";
                MySqlCommand myCommand = new MySqlCommand(myQuery, DW.dwConn);
                MySqlDataReader myReader = myCommand.ExecuteReader();
                myReader.Read();
                while (myReader.Read())
                {
                    try
                    {
                        RestInfo lineinfo = new RestInfo()
                        {
                            //Nkartennr = myReader["nkartennr"].ToString(),
                            //Bcode = myReader["chipiddec"].ToString(),
                            Chrgflag = myReader["chrgflag"].ToString(),
                            Tokenkey = myReader["tokenkey"].ToString(),
                            Nkartennr = myReader["wtp32"].ToString().Trim(),
                            Bcode = myReader["mediaID"].ToString().Trim(),
                            Disctype = myReader["disctype"].ToString(),
                            Persdesc = myReader["persdesc"].ToString(),
                            Nkassanr = Convert.ToInt32(String.IsNullOrEmpty(myReader["nkassanr"].ToString().Trim()) ? "0" : myReader["nkassanr"].ToString().Trim()),
                            Sbcode = myReader["mediaID"].ToString().Trim()
                        };
                        if (lineinfo.Nkassanr == 22)
                        {
                            string trimmedbarcode = lineinfo.Sbcode.Substring(3);
                            trimmedbarcode = trimmedbarcode.TrimStart('0');
                            lineinfo.Sbcode = "RXX" + trimmedbarcode;
                        }
                        lineinfo.Dob = myReader["dob"].ToString().Trim();
                        lineinfo.Skey = myReader["serialkey"].ToString().Trim();
                        //lineinfo.Passstatus = myReader["cstat"].ToString().Trim();
                        lineinfo.Passstatus = myReader["cardstatus"].ToString().Trim();
                        lineinfo.Firstname = (String.IsNullOrEmpty(myReader["firstname"].ToString().Trim()) ? "" : myReader["firstname"].ToString().Trim());
                        lineinfo.Lastname = (String.IsNullOrEmpty(myReader["lastname"].ToString().Trim()) ? "" : myReader["lastname"].ToString().Trim());
                        /*try
                        {
                            lineinfo.Nperskassa = Convert.ToInt32(myReader["perskassa"].ToString().Trim());
                        }
                        catch
                        {
                            lineinfo.Nperskassa = 0;
                        }
                        try
                        {
                            lineinfo.Npersnr = Convert.ToInt32(myReader["persnr"].ToString().Trim());
                        }
                        catch
                        {
                            lineinfo.Npersnr = 0;
                        }*/
                        //splist.Add(lineinfo);
                    }
                    catch  //(Exception ex)
                    { }
                }
                myReader.Close();
                CF.CloseConn(DW.dwConn);
            }
        }

        //private void removeRestacctRecords()
        //{
        //    raCon.Open();
        //    foreach (restacctinfoclass spinfo in splist)
        //    {
        //        /*OleDbCommand myCommand2 = new OleDbCommand(@"SELECT skey, expflag, discflag, invoice FROM restaccts WHERE skey= '" + spinfo.Skey + "'", raCon);
        //        OleDbDataReader myReader2 = myCommand2.ExecuteReader();
        //        */
        //        CF.openConnection(DWH);
        //        string myQuery = "SELECT serialkey, expdate, discflag FROM spcards WHERE serialkey = '" + spinfo.Skey + "'";
        //        MySqlCommand myCommand = new MySqlCommand(myQuery, DWH);
        //        MySqlDataReader myReader2 = myCommand.ExecuteReader();
        //        myReader2.Read();
        //        try
        //        {
        //            string expflag = myReader2["expdate"].ToString();
        //            string discflag = myReader2["discflag"].ToString();
        //            string skeyval = myReader2["serialkey"].ToString().Trim();
        //            /*string expflag = myReader2["expflag"].ToString();
        //            string discflag = myReader2["discflag"].ToString();
        //            string skeyval = myReader2["skey"].ToString().Trim();
        //            */
        //            //if (expflag == "True" && discflag == "N")
        //            if (Convert.ToDateTime(expflag) < DateTime.Now.Date && discflag == "N")
        //            {
        //                removelist.Add(skeyval);
        //            }
        //            /*else
        //            {

        //            }*/
        //        }
        //        catch
        //        {
        //        }
        //        myReader2.Close();
        //    }
        //    raCon.Close();
        //    foreach (string j in removelist)
        //    {
        //        int keyIndex = splist.FindIndex(s => s.Skey == j);
        //        splist.RemoveAt(keyIndex);
        //    }
        //}

        private void UpdateSRDatabase()
        {
            //MessageBox.Show(splist.Count.ToString() + " records in the list!");
            //int Ucount = 0;
            //int Icount = 0;

            //foreach (DataRow tRow in DS.Tables["spcards"].Rows)
            //{
            //    //CHECK IF RECORD ALREADY EXISTS
            //    try
            //    {
            //        long reccount = CF.RowCount(srCon, "SELECT Kunde_ID FROM [sr.db].[dbo].[Kunde] WHERE V_Kunde_ID = '" + info.Sbcode.Trim() + "'");

            //        if (reccount > 0)
            //        {
            //            string myUpdateQuery = "";
            //            string myUpdateQuery2 = "";
            //            if (info.Passstatus != "A")
            //            {
            //                myUpdateQuery = "UPDATE [sr.db].[dbo].[Kunde] set bemerkung = 'NON CHARGE', RabattP = 0, RabattVerkaufP = 0, Gruppe_ID = 0  WHERE V_Kunde_id = '" + info.Sbcode.Trim() + "'";
            //            }
            //            else if (info.Disctype == "E" & info.Passstatus == "A")
            //            {
            //                if (info.Persdesc.Contains("Dependent"))
            //                {
            //                    myUpdateQuery = "UPDATE [sr.db].[dbo].[Kunde] set bemerkung = '', RabattP = 30 , RabattVerkaufP = 0 , name='" + info.Lastname + "', vorname='" + info.Firstname + "', Geburtsdatum='" + info.Dob + "', Gruppe_ID = 1901000003 WHERE V_Kunde_id = '" + info.Sbcode.Trim() + "'";
            //                }
            //                else
            //                    myUpdateQuery = "UPDATE [sr.db].[dbo].[Kunde] set bemerkung = '', RabattP = 30 , RabattVerkaufP = 30 , name='" + info.Lastname + "', vorname='" + info.Firstname + "', Geburtsdatum='" + info.Dob + "', Gruppe_ID = 1901000003 WHERE V_Kunde_id = '" + info.Sbcode.Trim() + "'";
            //            }
            //            else if (info.Disctype == "C" & info.Passstatus == "A")
            //            {
            //                myUpdateQuery = "UPDATE [sr.db].[dbo].[Kunde] set bemerkung = '', RabattP = 20 , RabattVerkaufP = 20 ,name='" + info.Lastname + "', vorname='" + info.Firstname + "', Geburtsdatum='" + info.Dob + "', Gruppe_ID = 1901000003 WHERE V_Kunde_id = '" + info.Sbcode.Trim() + "'";
            //            }
            //            else
            //            {
            //                myUpdateQuery = "UPDATE [sr.db].[dbo].[Kunde] set bemerkung = '', RabattP = 20 , RabattVerkaufP = 0 , Geburtsdatum='" + info.Dob + "', name='" + info.Lastname + "', vorname='" + info.Firstname + "', Gruppe_ID = 1901000003 WHERE V_Kunde_id = '" + info.Sbcode.Trim() + "'";
            //            }
            //            if ((Convert.ToInt32(info.Tokenkey) != '0') && (info.Chrgflag.ToString() == "Y") && (info.Passstatus == "A"))
            //            {
            //                myUpdateQuery2 = "UPDATE[sr.db].[dbo].[Kunde] set ZIMMER = 'YES' WHERE V_Kunde_id = '" + info.Sbcode.Trim() + "'";
            //            }
            //            //IF EXISTS, UPDATE RECORD INFORMATION BLOCK/CANCEL STATUS & DISCOUNT PERCENTAGES
            //            CF.OpenConn(srCon);
            //            SqlCommand myUpdateCommand = new SqlCommand(myUpdateQuery, srCon);
            //            myUpdateCommand.ExecuteNonQuery();
            //            if ((Convert.ToInt32(info.Tokenkey) != '0') && (info.Chrgflag.ToString() == "Y") && (info.Passstatus == "A"))
            //            {
            //                SqlCommand myUpdateCommand2 = new SqlCommand(myUpdateQuery2, srCon);
            //                myUpdateCommand2.ExecuteNonQuery();
            //            }
            //            CF.CloseConn(srCon);
            //            Ucount++;
            //        }
            //        else
            //        {
            //            //IF DOESN'T EXIST, INSERT NEW RECORD INFORMATION WITH STATUS & DISCOUNT PERCENTAGES FOR NON-BLOCKED/NON-CANCELLED PASSES
            //            int kundeidval = Convert.ToInt32(CF.GetSQLField(srCon, "SELECT MAX(Kunde_ID) AS maxid FROM [sr.db].[dbo].[Kunde]")) + 1;
            //            info.Invoice = kundeidval.ToString();
            //            string dateinsertval = String.Format("{0:yyyy-MM-dd}", currdate) + " 00:00:00:000";
            //            string myInsertQuery = "";
            //            if (info.Passstatus != "A")
            //            {
            //            }
            //            else
            //            {
            //                try
            //                {
            //                    string insertnameval = (info.Firstname + " " + info.Lastname);
            //                    string sqlval = insertnameval.Replace("'", "''");

            //                    if (info.Disctype == "E")
            //                    {
            //                        if (info.Persdesc.Contains("Dependent"))
            //                        {
            //                            myInsertQuery = "INSERT INTO[sr.db].[dbo].[Kunde] (AenderungsDatum, Datum, Geburtsdatum, Gruppe_ID, Kunde_ID, Name, RabattP, RabattVerkaufP, Telefon, V_Kunde_ID, Vorname, Notiz) VALUES('" + dateinsertval + "','" + dateinsertval + "','" + info.Dob + "', 1901000003, " + info.Invoice + ",'" + info.Lastname + "', 30, 0,'8013591078','" + info.Sbcode + "','" + info.Firstname + "','" + info.Nkartennr + "')";
            //                        }
            //                        else
            //                        {
            //                            myInsertQuery = "INSERT INTO[sr.db].[dbo].[Kunde] (AenderungsDatum, Datum, Geburtsdatum, Gruppe_ID, Kunde_ID, Name, RabattP, RabattVerkaufP, Telefon, V_Kunde_ID, Vorname, Notiz) VALUES('" + dateinsertval + "','" + dateinsertval + "','" + info.Dob + "', 1901000003, " + info.Invoice + ",'" + info.Lastname + "', 30, 30,'8013591078','" + info.Sbcode + "','" + info.Firstname + "','" + info.Nkartennr + "')";
            //                        }

            //                    }
            //                    else if (info.Disctype == "C")
            //                    {
            //                        myInsertQuery = "INSERT INTO[sr.db].[dbo].[Kunde] (AenderungsDatum, Datum, Geburtsdatum, Gruppe_ID, Kunde_ID, Name, RabattP, RabattVerkaufP, Telefon, V_Kunde_ID, Vorname, Notiz) VALUES('" + dateinsertval + "','" + dateinsertval + "','" + info.Dob + "', 1901000003, " + info.Invoice + ",'" + info.Lastname + "', 20, 20,'8013591078','" + info.Sbcode + "','" + info.Firstname + "','" + info.Nkartennr + "')";
            //                    }
            //                    else
            //                    {
            //                        myInsertQuery = "INSERT INTO[sr.db].[dbo].[Kunde] (AenderungsDatum, Datum, Geburtsdatum, Gruppe_ID, Kunde_ID, Name, RabattP, RabattVerkaufP, Telefon, V_Kunde_ID, Vorname, Notiz) VALUES('" + dateinsertval + "','" + dateinsertval + "','" + info.Dob + "', 1901000003, " + info.Invoice + ",'" + info.Lastname + "', 20, 0,'8013591078','" + info.Sbcode + "','" + info.Firstname + "','" + info.Nkartennr + "')";
            //                    }
            //                    //myInsertQuery = "INSERT INTO [sr.db].[dbo].[Kunde] (AenderungsDatum, Datum, Geburtsdatum, Gruppe_ID, Kunde_ID, Name, Notiz, RabattP, RabattVerkaufP, Telefon, V_Kunde_ID, Vorname, Notiz) VALUES ('" + dateinsertval + "','" + dateinsertval + "','" + dateinsertval + "', 1901000003, " + info.Invoice + ",'" + info.Sbcode + "-E','" + sqlval + "'," + rentaldiscount.ToString() + "," + retaildiscount.ToString() + ",'8013591078','" + info.Sbcode + "','" + info.Lastname + "','"+ info.Nkartennr +"')";
            //                    CF.OpenConn(srCon);
            //                    SqlCommand myInsertCommand = new SqlCommand(myInsertQuery, srCon);
            //                    myInsertCommand.ExecuteNonQuery();
            //                    CF.CloseConn(srCon);
            //                    Icount++;
            //                }
            //                catch (Exception ex)
            //                {
            //                    tError.updateErrorLog("SportsRentals.UpdateSrDatabase", -9999, ex.Message, ex.Data.ToString());
            //                    return;
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        tError.updateErrorLog("SportsRentals.UpdateSrDatabase", -9999, ex.Message, ex.Data.ToString());
            //        return;
            //    }

            //}
            //MessageBox.Show(Ucount.ToString() + "<Counts>" + Icount.ToString());
        }


    }
}
