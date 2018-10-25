using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using asl_SyncLibrary;

namespace chrgupdate
{
    public partial class ChrgUpdate : Form
    {
        CommonFunctions CF = new CommonFunctions();
        DataWarehouse DW = new DataWarehouse();
        Asgard AG =new Asgard();

        OleDbConnection VFPConn = new OleDbConnection("Provider=VFPOLEDB.1");

        DataTable SysCfg = new DataTable();
        DataTable SPCards = new DataTable();
        //DataTable DDFDCust = new DataTable();
        //DataTable FDCustX = new DataTable();
        //DataTable ARCustX = new DataTable();
        //DataTable CDDARCust = new DataTable();
        //DataTable DDFDScn = new DataTable();
        //DataTable DDFDIntr = new DataTable();
        //DataTable DDFDHis = new DataTable();
        //DataTable DDFDPhone = new DataTable();
        //DataTable RecFilDT = new DataTable();
        //DataTable RecHis = new DataTable();

        DateTime msdate = DateTime.Now;
        DateTime mdate = DateTime.Now;


        enum Restaurants : int
        {
            Albion,
            Alfs,
            Watsons
        }
        enum RestField : int
        {
            Office,
            Place,
            BusName,
            IStore,
            DDDir,
            CFBDir
        }
        string[,] Restaurant = new string[3, 6];

        bool AutoRun = false;



        //string mdrive = @"G:\";
        //string mdir = @"G:\FPDATA\";
        //string mcodedir = @"G:\Source\FPCode\FB\";
        //string msharedcode = @"G:\Source\FPCode\shared\";
        string mdatadir = @"G:\FPDATA\FB\";
        //string msharedir = @"G:\FPDATA\shared\";
        //string mbeansdir = @"G:\FPDATA\beans\";
        //string TempDir = @"c:\temp\";
        string mdigdindir = string.Empty; //   Digital Dining DATA folder for active restaurant.       
        //string DigDiningCodeDir = @"P:\_admin\ACCTG2\F&B\Digital Dining\";

        bool malbconnectflag = false;
        bool malfconnectflag = false;
        bool mwatconnectflag = false;
        DateTime medate = DateTime.Now;
        DateTime myrstart;
        string mcfbdir = string.Empty;
        Int32 mistore = 0;
        //Int32 mkey2 = 0;
        string mfname = string.Empty;
        string mbar = string.Empty;
        Int32 mkey = 0;
        string mplace = string.Empty;
        string moffice = string.Empty;
        string lbusname = string.Empty;
        string srhandle = string.Empty;
        Color mgo = Color.Green;
        Color mstop = Color.Red;

        public ChrgUpdate()
        {
            InitializeComponent();
            String[] args = Environment.GetCommandLineArgs();

            if (args != null)
            {
                AutoRun = (args[0].ToUpper() == "AUTORUN");
            }
            myrstart = CF.SeasonStart;
            
        }

        private void ChrgUpdate_Shown(object sender, EventArgs e)
        {
            DateTime StartTime = DateTime.Now;
            Check_Connection();
            //if (AutoRun)
            {
                CF.ExecuteSQL(Asgard.AsgardCon, $"INSERT sync.task_history (name, id, start_time, exit_time, exit_code, runtime, passed_params) VALUES ('ChrgUpdate', 0, '{StartTime.ToString(Mirror.AxessDateTimeFormat)}', null, 0, 0, 'AUTORUN')");
                RunDD();
                DateTime ExitTime = DateTime.Now;
                CF.ExecuteSQL(Asgard.AsgardCon, $"UPDATE sync.task_history SET exit_time='{ExitTime.ToString(Mirror.AxessDateTimeFormat)}', runtime = {(ExitTime - StartTime).TotalSeconds.ToString()} WHERE name='ChrgUpdate' AND start_time='{StartTime.ToString(Mirror.AxessDateTimeFormat)}'");
                Application.Exit();
            }
        }

        private void Check_Connection()
        {
            VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mdatadir}syscfg.dbf";
            SysCfg = CF.LoadTable(VFPConn, $"SELECT * FROM syscfg", "SysCfg");
            foreach (DataRow DRSysCfg in SysCfg.Rows)
            {
                string DigDinFolder = string.Empty;
                switch (DRSysCfg["office"].ToString())
                {
                    case "ALB":
                        DigDinFolder = DRSysCfg["digdindir"].ToString().Trim().Replace("J:",@"\\dd-albion\digital dining");
                        //DigDinFolder = DRSysCfg["digdindir"].ToString().Trim().Replace("J:", @"C:\digital dining\albion"); //testing only
                        Restaurant[(int)Restaurants.Albion, (int)RestField.Office] = DRSysCfg["office"].ToString().Trim();
                        Restaurant[(int)Restaurants.Albion, (int)RestField.Place] = "G";
                        Restaurant[(int)Restaurants.Albion, (int)RestField.BusName] = DRSysCfg["busname"].ToString().Trim();
                        Restaurant[(int)Restaurants.Albion, (int)RestField.IStore] = DRSysCfg["istore"].ToString().Trim();
                        Restaurant[(int)Restaurants.Albion, (int)RestField.DDDir] = DigDinFolder;
                        Restaurant[(int)Restaurants.Albion, (int)RestField.CFBDir] = @"..\FB\albion\";
                        break;
                    case "ALF":
                        DigDinFolder = DRSysCfg["digdindir"].ToString().Trim().Replace("K:", @"\\dd-alfs\digital dining");
                        //DigDinFolder = DRSysCfg["digdindir"].ToString().Trim().Replace("K:", @"C:\digital dining\alfs");   //testing only
                        Restaurant[(int)Restaurants.Alfs, (int)RestField.Office] = DRSysCfg["office"].ToString().Trim();
                        Restaurant[(int)Restaurants.Alfs, (int)RestField.Place] = "A";
                        Restaurant[(int)Restaurants.Alfs, (int)RestField.BusName] = DRSysCfg["busname"].ToString().Trim();
                        Restaurant[(int)Restaurants.Alfs, (int)RestField.IStore] = DRSysCfg["istore"].ToString().Trim();
                        Restaurant[(int)Restaurants.Alfs, (int)RestField.DDDir] = DigDinFolder;
                        Restaurant[(int)Restaurants.Alfs, (int)RestField.CFBDir] = @"..\FB\alfs\";
                        break;
                    case "WAT":
                        DigDinFolder = DRSysCfg["digdindir"].ToString().Trim().Replace("L:", @"\\dd-watsons\digital dining");
                        //DigDinFolder = DRSysCfg["digdindir"].ToString().Trim().Replace("L:", @"C:\digital dining\watson"); //testing only
                        Restaurant[(int)Restaurants.Watsons, (int)RestField.Office] = DRSysCfg["office"].ToString().Trim();
                        Restaurant[(int)Restaurants.Watsons, (int)RestField.Place] = "W";
                        Restaurant[(int)Restaurants.Watsons, (int)RestField.BusName] = DRSysCfg["busname"].ToString().Trim();
                        Restaurant[(int)Restaurants.Watsons, (int)RestField.IStore] = DRSysCfg["istore"].ToString().Trim();
                        Restaurant[(int)Restaurants.Watsons, (int)RestField.DDDir] = DigDinFolder;
                        Restaurant[(int)Restaurants.Watsons, (int)RestField.CFBDir] = @"..\FB\watson\";
                        break; 
                }
                if (File.Exists($@"{DigDinFolder}acctrn_c.dbf"))
                {
                    switch (DRSysCfg["office"].ToString())
                    {
                        case "ALB":
                            malbconnectflag = true;
                            break;
                        case "ALF":
                            malfconnectflag = true;
                            break;
                        case "WAT":
                            mwatconnectflag = true;
                            break;
                    }
                }
            }
            if (mwatconnectflag)
            {

            }
            CBAlbionGrill.Checked = malbconnectflag;
            CBAlbionGrill.Enabled = malbconnectflag;
            PBAlbionGrill.Enabled = malbconnectflag;
            CBAlfsRestaurant.Checked = false; // malfconnectflag;
            CBAlfsRestaurant.Enabled = false; //malfconnectflag;
            PBAlfsRestaurant.Enabled = malfconnectflag;
            CBWatsonShelter.Checked = false; //mwatchonnectflag;
            CBWatsonShelter.Enabled = false; //mwatchonnectflag;
            PBWatsonShelter.Enabled = false; //mwatchonnectflag;
        }

        // U = Update record
        // N = add new record
        // X = Deactivate record

        private void DDVerify()
        {
            if (!Directory.Exists(mcfbdir))
            {
                Directory.CreateDirectory(mcfbdir);
            }

            File.Copy($"{mdigdindir}fdcustdt.dbf", $"{mcfbdir}fdcustdt.dbf", true);
            File.Copy($"{mdigdindir}fdcustdt.cdx", $"{mcfbdir}fdcustdt.cdx", true);
            File.Copy($"{mdigdindir}fdcustdt.fpt", $"{mcfbdir}fdcustdt.fpt", true);

            File.Copy($"{mdigdindir}fdocsn_c.dbf", $"{mcfbdir}fdocsn_c.dbf", true);
            File.Copy($"{mdigdindir}fdocsn_c.cdx", $"{mcfbdir}fdocsn_c.cdx", true);

            File.Copy($"{mdigdindir}fdintr_c.dbf", $"{mcfbdir}fdintr_c.dbf", true);
            File.Copy($"{mdigdindir}fdintr_c.cdx", $"{mcfbdir}fdintr_c.cdx", true);

            File.Copy($"{mdigdindir}fdchis_h.dbf", $"{mcfbdir}fdchis_h.dbf", true);
            File.Copy($"{mdigdindir}fdchis_h.cdx", $"{mcfbdir}fdchis_h.cdx", true);

            File.Copy($"{mdigdindir}fdphon_c.dbf", $"{mcfbdir}fdphon_c.dbf", true);
            File.Copy($"{mdigdindir}fdphon_c.cdx", $"{mcfbdir}fdphon_c.cdx", true);

            File.Copy($"{mdigdindir}recfildt.dbf", $"{mcfbdir}recfildt.dbf", true);
            File.Copy($"{mdigdindir}recfildt.cdx", $"{mcfbdir}recfildt.cdx", true);
            File.Copy($"{mdigdindir}recfildt.fpt", $"{mcfbdir}recfildt.fpt", true);

            File.Copy($"{mdigdindir}rechis_h.dbf", $"{mcfbdir}rechis_h.dbf", true);
            File.Copy($"{mdigdindir}rechis_h.cdx", $"{mcfbdir}rechis_h.cdx", true);
            File.Copy($"{mdigdindir}rechis_h.err", $"{mcfbdir}rechis_h.err", true);
            File.Copy($"{mdigdindir}rechis_h.fxp", $"{mcfbdir}rechis_h.fxp", true);


            if (File.Exists($@"C:\asl_Sync\data\FD_AR__C.dbf"))
            {
                File.Copy($@"C:\asl_Sync\data\FD_AR__C.dbf", $"{mcfbdir}FD_AR__C.dbf", true);
                File.Copy($@"C:\asl_Sync\data\FD_AR__C.cdx", $"{mcfbdir}FD_AR__C.cdx", true);
            }
            else
            {
                File.Copy($@"C:\Users\darylb\Documents\Visual Studio 2017\Projects\asl_Sync\chrgupdate\bin\FB\Default\FD_AR__C.dbf", $"{mcfbdir}FD_AR__C.dbf", true);
                File.Copy($@"C:\Users\darylb\Documents\Visual Studio 2017\Projects\asl_Sync\chrgupdate\bin\FB\Default\FD_AR__C.cdx", $"{mcfbdir}FD_AR__C.cdx", true);
            }


            // *** SUMMER ONLY
            DeactivateAll();
            // *** END SUMMER ONLY

            VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdcustdt.dbf";
            int mnewkey = Convert.ToInt32(CF.GetSQLField(VFPConn, "SELECT MAX(key) FROM fdcustdt")) + 1;

            if (PBAlbionGrill != null)
                PBAlbionGrill.Maximum = SPCards.Rows.Count;
            foreach (DataRow SPCardRow in SPCards.Rows)
            {
                if (PBAlbionGrill != null)
                    PBAlbionGrill.Value = SPCards.Rows.IndexOf(SPCardRow) + 1;
                bool isModified = false;
                string CardAcct = SPCardRow["cardacct"].ToString().Trim().ToUpper();       //Chip ID
                string CardStatus = SPCardRow["cardstatus"].ToString().Trim().ToUpper();   //Current Status
                string lfax = SPCardRow.Field<DateTime>("dob").ToString("yyyy/MM/dd");
                string Query = string.Empty;
                bool isExpired = (CardStatus == "X");
                bool isActive = (CardStatus == "A") || isExpired;   //renive isExpired during regular season, this is for summer only!
                bool canCharge = (SPCardRow["chrgflag"].ToString().ToUpper().Trim() == "Y");
                bool empCharge = (SPCardRow["empchrg"].ToString().ToUpper().Trim() == "Y");
                string Fname = CF.EscapeChar(SPCardRow["firstname"].ToString());
                string Lname = CF.EscapeChar(SPCardRow["lastname"].ToString());
                string Where = $"smemberno='{CardAcct.ToString()}'";
                VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdcustdt.dbf";
                if (CF.RowCount(VFPConn, "fdcustdt", Where) > 1)
                {
                    Where += $" AND sfirstname='{Fname}' AND slastname='{Lname}'";
                }
                VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdcustdt.dbf";
                DataRow DRCustRow = CF.LoadDataRow(VFPConn, $"SELECT * FROM fdcustdt WHERE {Where} ORDER BY key DESC TOP 1");                              //Find card in FDCustDT
                if (DRCustRow != null)                                                        //If card found
                {
                    if (!((isActive) ^ (DRCustRow["scity"].ToString().Trim().ToUpper() == "INACTIVE")))   //If SPCard and DDFDCust records do not match active status
                    {
                        if (isActive)
                        {
                            Query = $"sfirstname='{SPCardRow["firstname"].ToString().Trim()}', " +
                                $"slastname='{SPCardRow["lastname"].ToString().Trim()}', " +
                                $"cdesc='{SPCardRow["firstname"].ToString().Trim()} {SPCardRow["lastname"].ToString().Trim()}', " +
                                $"ssortname='{(SPCardRow["lastname"].ToString().Trim() + "      ").Substring(0, 6)}', " +
                                $"scustaddr1='{SPCardRow["street"].ToString().Trim()}', " +
                                $"scustaddr2='', " +
                                $"sfaxno='{lfax}', " +
                                $"scity='{SPCardRow["city"].ToString().Trim()}', " +
                                $"sstate='{SPCardRow["state"].ToString().Trim()}', " +
                                $"szip='{SPCardRow["zip"].ToString().Trim()}', " +
                                $"cemail='', " +
                                $"ktufdeliv=1, " +
                                $"ktufdinin={UpdateDiscount(SPCardRow["disctype"].ToString())}";
                        }
                        else
                        {
                            Query = $"sfirstname='XA {SPCardRow["firstname"].ToString().Trim()}', " +
                                $"slastname='XA {SPCardRow["lastname"].ToString().Trim()}', " +
                                $"cdesc='XA {SPCardRow["firstname"].ToString().Trim()} {SPCardRow["lastname"].ToString().Trim()}', " +
                                $"ssortname='{(SPCardRow["lastname"].ToString().Trim() + new string(' ', 6)).Substring(0, 6).Trim()}', " +
                                $"scustaddr1='____X_____', " +
                                $"scustaddr2='____X_____', " +
                                $"sfaxno='{lfax}', " +
                                $"scity='INACTIVE', " +
                                $"sstate='', " +
                                $"szip='', " +
                                $"cemail='____X_____', " +
                                $"ktufdeliv=0, " +
                                $"ktufdinin=0, " +
                                $"istore='{mistore}', " +
                                $"fvisitor=1";
                        }
                        VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdcustdt.dbf";
                        CF.ExecuteSQL(VFPConn, $"Update fdcustdt SET {Query} WHERE smemberno='{CardAcct}'");
                        isModified = true;
                    }
                }
                else
                {                                                                       //No matching CardAcct in DDFDCust
                    if (isActive)                                                       //If SPCard record is active
                    {
                        isModified = true;
                        VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdphon_c.dbf";
                        CF.ExecuteSQL(VFPConn, $"INSERT INTO fdphon_c (kdfdcust, sareacode, sexchange, sphone_no, sextension) VALUES ({mnewkey++}, '801', '359', '1078', '123')");
                        string CurDate = DateTime.Now.ToString(Mirror.AxessDateTimeFormat);
                        string FPDate = DateTime.Now.ToString("{^yyyy/MM/dd}");
                        VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdcustdt.dbf";
                        CF.ExecuteSQL(VFPConn, $"INSERT INTO fdcustdt (key, cabv, smemberno, sfirstname, slastname, csort, cdesc, ssortname, ktfdplan, ktfddzon, ktmainintr, dlstpurch, dlstcmplt, iptdmnuitm, itotmnuitm, iptdprchno, itotprchno, rptdprchv, rtotprchv, iptdcmplno, itotcmplno, rptdpoints, rtotpoints, ktrcptyp, rptdfdcrd, rtotfdcrd, ftaxexpt, ktufdinin, fchkaccess, scustaddr1, scustaddr2, sfaxno, scity, sstate, szip, cemail, ktufdeliv, istore, fvisitor, scompany, memo, ssortcomp, mposmsg, mdelinst, stitle, sdatepurch, ilstchkno, sdatecmpl, sgender, spaydetail, ffdintrsts, dhqlstprch, dhqlstcmpl, cuniqueid, fvip, cemailchk, fnopaperck, femailman, femailgen, cemailrcpt, idlvmthck, idlvmthrt) " +
                            $"VALUES ({mnewkey.ToString()}, '{SPCardRow["empid"].ToString().Trim()}', '{SPCardRow["cardacct"].ToString().Trim()}', '{SPCardRow["firstname"].ToString().Trim()}', '{SPCardRow["lastname"].ToString().Trim()}', '{(mnewkey + 999999).ToString().Substring(1, 6)}', '{SPCardRow["firstname"].ToString().Trim()}  {SPCardRow["lastname"].ToString().Trim()}', '{(SPCardRow["lastname"].ToString().Trim() + new string(' ', 6)).Substring(0, 6).Trim()}', 2, 1, 1, {FPDate}, {FPDate}, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, '0', {UpdateDiscount(SPCardRow["disctype"].ToString())}, '0', '{SPCardRow["street"].ToString().Trim()}', '', '{lfax}', '{SPCardRow["city"].ToString().Trim()}', '{SPCardRow["state"].ToString().Trim()}', '{SPCardRow["zip"].ToString().Trim()}', '', 1, {mistore}, '1', 'Resort Charge','', '', '', '', '', '', 0, '', '', '', '', " + "{//},{//}, '', '', '', '', '', '', '', 0, 0)");

                        //UpdateFDScn();
                        VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdocsn_c.dbf";
                        if (CF.RowCount(VFPConn,"fdocsn_c", $"kdfdcust={mnewkey.ToString()}") == 0)
                        {
                            for (int x = 1; x < 7; x++)
                            {
                                VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdocsn_c.dbf";
                                CF.ExecuteSQL(VFPConn, $"INSERT INTO fdocsn_c (kdfdcust, ktfdocsn, socsndetal, docsndate, socsndate, iseqnum) VALUES ({mnewkey.ToString()},1,'None'," + "{^1999/12/30},{^1999/12/30}," + $"{x.ToString()})");
                            }
                        }
                        //UpdateFDHis();
                        VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdchis_h.dbf";
                        if (CF.RowCount(VFPConn, $"fdchis_h", $"key={mnewkey.ToString()}") == 0)
                        {
                            for (int x = 1; x < 25; x++)
                            {
                                if (x != 13)
                                {
                                    VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdchis_h.dbf";
                                    CF.ExecuteSQL(VFPConn, $"INSERT INTO fdchis_h (key, ktperiod, dposted, rnoofpurch, rvalue, rcreditin, rcreditout) VALUES({mnewkey.ToString()}, {x.ToString()}, " + "{^1999/12/31},0,0,0,0)");
                                }
                            }
                        }
                        //UpdateFDInt();
                        VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdintr_c.dbf";
                        if (CF.RowCount(VFPConn, "fdintr_c", $"kdfdcust={mnewkey.ToString()}") == 0)
                        {
                            for (int x = 1; x < 7; x++)
                            {
                                VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdintr_c.dbf";
                                CF.ExecuteSQL(VFPConn, $"INSERT INTO fdintr_c (kdfdcust, ktfdintr, fmain, iseqnum) VALUES ({mnewkey.ToString()}, 1, '{(x == 1 ? "1" : "0")}', {x.ToString()})");
                            }
                        }
                    }
                    string rptkey = string.Empty;
                    if (isModified)
                    {
                        if ((canCharge && (Convert.ToInt32(SPCardRow["tokenkey"].ToString()) > 0)) || empCharge)
                        {
                            VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}recfildt.dbf";
                            DataRow DRARCustx = CF.LoadDataRow(VFPConn, $"SELECT * FROM recfildt WHERE saccnum = '{CardAcct}'");
                            if (DRARCustx != null)                                                      //If matching record found
                            {
                                if (!((isActive) ^ (DRARCustx["finactive"].ToString().Trim() == "1")))  //if SPCards cardstatus does not match ARCust status
                                {
                                    rptkey = "U";                                                       //Set record to be updated
                                }
                            }
                            else
                            {                                                                           //No matching ARCust record found
                                if (isActive)
                                {
                                    rptkey = "N";                                                       //Set record to insert into ARCust from SPCards
                                }
                            }

                            if (Fname.Length != 0 && Lname.Length != 0)
                            {
                                //UpdateARCust();   
                                VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}recfildt.dbf";
                                Int32 NewKey = Convert.ToInt32(CF.GetSQLField(VFPConn, "SELECT MAX(saccnum) FROM recfildt")) + 1;
                                if (rptkey == "U")  //updated record
                                {
                                    VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}recfildt.dbf";
                                    if (CF.RowCount(VFPConn, "recfildt", $"saccnum={CardAcct}") != 0)
                                    {
                                        mkey = NewKey - 1;
                                        mnewkey = mkey;
                                        Query = $"SET cabv='{Lname} {Fname}', sposition='{SPCardRow["empid"].ToString()}',";
                                        if (isActive)
                                        {
                                            Query += $"finactive=0, sfstnam='{Fname}',slstnam='{Lname}', sfaxphn='{lfax}', sstradd='{CF.EscapeChar(SPCardRow["street"].ToString())}', scity='{CF.EscapeChar(SPCardRow["city"].ToString())}', sstate='{SPCardRow["state"].ToString()}', szip='{SPCardRow["zip"].ToString()}', shomphn='{SPCardRow["empid"].ToString()}'";
                                        }
                                        else
                                        {
                                            Query += $"finactive=1, sfstnam='XA {Fname}',slstnam='XA {Lname}', sstradd='____X_____', scity='INACTIVE', sstate='', szip='', shomphn=''";
                                        }
                                        CF.ExecuteSQL(VFPConn, $"UPDATE recfildt {Query} WHERE saccnum={CardAcct}");
                                    }
                                }
                                else if (rptkey == "N") //new record
                                {
                                    Query = $"INSERT INTO recfildt (csort, key, cabv, saccnum, sfstnam, slstnam, sposition, sfaxphn, sstradd, scity, sstate, szip, shomphn, cdesc, slimuse, ktacctyp, finactive, memo, sbldnam, rcrdlim, dlstpurc, dlstpymt, ropnbal, rcurbal, r30dbal, r60dbal, r90dbal, mposmsg, istore, semail, semailchk, semailstmt, semailrcpt, idlvmthck, idlvmthrt) VALUES " +
                                        $"('{(mnewkey + 999999).ToString().Substring(1,6)}', {mnewkey.ToString()}, '{Lname}  {Fname}', '{CardAcct}', '{Fname}', '{Lname}', '{SPCardRow["empid"].ToString()}', '{lfax}', '{CF.EscapeChar(SPCardRow["street"].ToString())}', '{CF.EscapeChar(SPCardRow["city"].ToString())}', '{SPCardRow["state"].ToString()}', '{SPCardRow["zip"].ToString()}', '{SPCardRow["empid"].ToString()}', '{Lname}  {Fname}', 'F', 2, '0', '', '', 0," + "{^1899/12/30}, {^1899/12/30}," + $" 0, 0, 0, 0, 0, '', 0, '', '', '', '', 0, 0)";
                                    //Query = "SELECT dlstpurc FROM recfildt";
                                    VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}recfildt.dbf";
                                    //DataTable ttt = CF.LoadTable(VFPConn, Query, "ttt");
                                    CF.ExecuteSQL(VFPConn, Query);
                                    //END UpdateARCust inline code
                                    //UpdateARHist();
                                    VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}rechis_h.dbf";
                                    if (CF.RowCount(VFPConn, "rechis_h", $"key={mnewkey.ToString()}") == 0)
                                    {
                                        for (int x = 1; x < 25; x++)
                                        {
                                            if (x != 13)
                                            {
                                                VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}rechis_h.dbf";
                                                CF.ExecuteSQL(VFPConn, $"INSERT INTO rechis_h (key, ktperiod, dposted, rsales, rpaymts, rcovcnt, ichkcnt) VALUES ({mnewkey.ToString()}, {x.ToString()}, " + "{^1999/12/31},0,0,0,0)");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (PBAlbionGrill != null)
                PBAlbionGrill.Value = 0;
        }

        private void Build_FD_AR_Link()
        {
            VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}recfildt.dbf";
            using (DataTable TempTable = CF.LoadTable(VFPConn, $"SELECT fdcustdt.key as kdfdcust, recfildt.key as kdrecfil FROM recfildt INNER JOIN fdcustdt ON fdcustdt.smemberno = recfildt.saccnum", "temp"))
            {
                if (TempTable != null)
                {
                    if (PBAlfsRestaurant != null)
                        PBAlfsRestaurant.Maximum = TempTable.Rows.Count;
                    foreach (DataRow TempRow in TempTable.Rows)
                    {
                        if (PBAlfsRestaurant != null)
                            PBAlfsRestaurant.Value = TempTable.Rows.IndexOf(TempRow);
                        System.Diagnostics.Debug.Print($"Row {TempTable.Rows.IndexOf(TempRow).ToString()} of {TempTable.Rows.Count.ToString()}");
                        VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}FD_AR__C.dbf";
                        CF.ExecuteSQL(VFPConn, $"INSERT INTO FD_AR__C (kdfdcust, kdrecfil) VALUES ({TempRow["kdfdcust"].ToString()},{TempRow["kdrecfil"].ToString()})");
                    }
                    if (PBAlfsRestaurant != null)
                        PBAlfsRestaurant.Value = 0;
                }
            }
        }

        private void CopyBack()
        {
            string ldatadir = mdatadir;
            switch (moffice)
            {
                case "ALB":
                    ldatadir += @"Albion\";
                    break;
                case "WAT":
                    ldatadir += @"Watson\";
                    break;
                case "ALF":
                    ldatadir += @"Alfs\";
                    break;
            }
            File.Copy($"{mcfbdir}fdcustdt.dbf", $"{ldatadir}fdcustdt.dbf", true);
            File.Copy($"{mcfbdir}fdcustdt.cdx", $"{ldatadir}fdcustdt.cdx", true);
            File.Copy($"{mcfbdir}fdcustdt.fpt", $"{ldatadir}fdcustdt.fpt", true);

            File.Copy($"{mcfbdir}fdocsn_c.dbf", $"{ldatadir}fdocsn_c.dbf", true);
            File.Copy($"{mcfbdir}fdocsn_c.cdx", $"{ldatadir}fdocsn_c.cdx", true);

            File.Copy($"{mcfbdir}fdintr_c.dbf", $"{ldatadir}fdintr_c.dbf", true);
            File.Copy($"{mcfbdir}fdintr_c.cdx", $"{ldatadir}fdintr_c.cdx", true);

            File.Copy($"{mcfbdir}fdchis_h.dbf", $"{ldatadir}fdchis_h.dbf", true);
            File.Copy($"{mcfbdir}fdchis_h.cdx", $"{ldatadir}fdchis_h.cdx", true);

            File.Copy($"{mcfbdir}fdphon_c.dbf", $"{ldatadir}fdphon_c.dbf", true);
            File.Copy($"{mcfbdir}fdphon_c.cdx", $"{ldatadir}fdphon_c.cdx", true);

            File.Copy($"{mcfbdir}recfildt.dbf", $"{ldatadir}recfildt.dbf", true);
            File.Copy($"{mcfbdir}recfildt.cdx", $"{ldatadir}recfildt.cdx", true);
            File.Copy($"{mcfbdir}recfildt.fpt", $"{ldatadir}recfildt.fpt", true);

            File.Copy($"{mcfbdir}rechis_h.dbf", $"{ldatadir}rechis_h.dbf", true);
            File.Copy($"{mcfbdir}rechis_h.cdx", $"{ldatadir}rechis_h.cdx", true);
            File.Copy($"{mcfbdir}rechis_h.err", $"{ldatadir}rechis_h.err", true);
            File.Copy($"{mcfbdir}rechis_h.fxp", $"{ldatadir}rechis_h.fxp", true);

            File.Copy($"{mcfbdir}FD_AR__C.dbf", $"{ldatadir}FD_AR__C.dbf", true);
            File.Copy($"{mcfbdir}FD_AR__C.cdx", $"{ldatadir}FD_AR__C.cdx", true);
        }

        private int UpdateDiscount(string DiscType)
        {
            int RtnVal = 0;
            switch (DiscType)
            {
                case "C":
                    switch (mplace)
                    {
                        case "A":
                            RtnVal = 15;
                            break;
                        case "W":
                            RtnVal = 6;
                            break;
                        case "G":
                            RtnVal = 15;
                            break;
                    }
                    break;
                case "E":
                    switch (mplace)
                    {
                        case "A":
                            RtnVal = 16;
                            break;
                        case "W":
                            RtnVal = 5;
                            break;
                        case "G":
                            RtnVal = 16;
                            break;
                    }
                    break;
                default:
                    switch (mplace)
                    {
                        case "A":
                            RtnVal = 19;
                            break;
                        case "W":
                            RtnVal = 20;
                            break;
                        case "G":
                            RtnVal = 1;
                            break;
                    }
                    break;
            }
            return RtnVal;
        }

        private void DeactivateAll()
        {
            string Query = $"UPDATE fdcustdt SET cdesc='XA ' + sfirstname + ' ' + slastname, ssortname=slastname, scustaddr1='____X_____', scustaddr2='____X_____', scity='INACTIVE', sstate='', szip='', cemail='____X_____', sfirstname='XA '+sfirstname, slastname='XA ' + slastname, ktufdeliv=0, ktufdinin = 0, istore={mistore.ToString()}, fvisitor='1' WHERE scity NOT LIKE 'INACTIVE%'";
            VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdcustdt.dbf";
            CF.ExecuteSQL(VFPConn, Query);
        }

        private void DeactiveAccount(string AcctNo)
        {
            //string Query = "SELECT "
                
                
                
                
                
                
            //    Query = $"sfirstname='XA {SPCardData["firstname"].ToString().Trim()}', " +
            //            $"slastname='XA {SPCardData["lastname"].ToString().Trim()}', " +
            //            $"cdesc='XA {SPCardData["firstname"].ToString().Trim()} {SPCardData["lastname"].ToString().Trim()}', " +
            //            $"ssortname='{(SPCardData["lastname"].ToString().Trim() + new string(' ', 6)).Substring(0, 6).Trim()}', " +
            //            $"scustaddr1='____X_____', " +
            //            $"scustaddr2='____X_____', " +
            //            $"sfaxno='{lfax}', " +
            //            $"scity='INACTIVE', " +
            //            $"sstate='', " +
            //            $"szip='', " +
            //            $"cemail='____X_____', " +
            //            $"ktufdeliv=0, " +
            //            $"ktufdinin=0, " +
            //            $"istore='{mistore}', " +
            //            $"fvisitor=1";
                        //VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = {mcfbdir}fdcustdt.dbf";
                        //CF.ExecuteSQL(VFPConn, $"Update fdcust SET {Query} WHERE smemberno='{AcctNo[""].ToString()}'");

        }

    private void BtnContinue_Click(object sender, EventArgs e)
        {
            RunDD();
        }

private void RunDD()
        {
            BtnContinue.Enabled = false;
            BtnExit.Enabled = false;
            string Employees = string.Empty;
            //Summer season
            //{^" + DateTime.Now.AddDays(-21).Date.ToShortDateString() + "}
            VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = G:\FPData\payroll\shared\empsum.dbf";
            DataTable ActiveEmployees = CF.LoadTable(VFPConn, "SELECT DISTINCT emp_id FROM empsum WHERE (chk_date >= (date() - 21)) OR (emp_id = 'CAN1187')", "EmpSum");
            CF.Table2CSV(ActiveEmployees, @"C:\temp\employees.csv");
            foreach (DataRow ActiveEmployee in ActiveEmployees.Rows)
            {
                Employees = CF.AddToList(Employees, $"'{ActiveEmployee["emp_id"].ToString()}'");
            }
            string Where = $"(empid IN ({Employees}) OR (empid<>'' AND saledate>='{DateTime.Now.AddDays(-21).ToString(Mirror.AxessDateFormat)}' AND cardstatus='A'))";
//regular season  
            //string Where = "(chrgflag = 'Y' OR discflag = 'Y' OR empchrg = 'Y') AND lastname <> '' AND firstname <> '' AND cardacct <> ''";
            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards SET updateflag='', rptkey='' WHERE {Where}");
            SPCards = CF.LoadTable(DW.dwConn, $"SELECT RecID, saledate, Cardacct, CardStatus, EmpID, firstname, lastname, street, city, state, zip, dob, chrgflag, disctype, tokenkey, empchrg, '' AS UpdateFlag, '' AS RptKey FROM {DW.ActiveDatabase}.spcards WHERE {Where} ORDER BY cardacct, nlfdzaehle DESC", "SPCards");
            CF.Table2CSV(SPCards, @"C:\temp\active_employees.csv");
            moffice = string.Empty;
            mplace = string.Empty;
            lbusname = string.Empty;
            mistore = 0;
            mcfbdir = string.Empty;

            if (CBAlbionGrill.Checked)
            {
                moffice = Restaurant[(int)Restaurants.Albion,(int)RestField.Office];
                mplace = Restaurant[(int)Restaurants.Albion, (int)RestField.Place];
                lbusname = Restaurant[(int)Restaurants.Albion, (int)RestField.BusName];
                mistore = Convert.ToInt32(Restaurant[(int)Restaurants.Albion, (int)RestField.IStore]);
                mcfbdir = Restaurant[(int)Restaurants.Albion, (int)RestField.CFBDir];
                mdigdindir = Restaurant[(int)Restaurants.Albion, (int)RestField.DDDir];
                DDVerify();         //sets TSPCards to update or insert new records into FDCust and ARCust tables
                SSLStatus.Text = $"Building FD & A/R customer links... ({lbusname})";
                Build_FD_AR_Link(); //
                SSLStatus.Text = $"Copying file back to server... ({lbusname})";
                CopyBack();
            }
            //if (CBAlfsRestaurant.Checked)
            //{
            //    moffice = Restaurant[(int)Restaurants.Alfs, (int)RestField.Office];
            //    mplace = Restaurant[(int)Restaurants.Alfs, (int)RestField.Place];
            //    lbusname = Restaurant[(int)Restaurants.Alfs, (int)RestField.BusName];
            //    mistore = Convert.ToInt32(Restaurant[(int)Restaurants.Alfs, (int)RestField.IStore]);
            //    mcfbdir = Restaurant[(int)Restaurants.Alfs, (int)RestField.CFBDir];
            //    mdigdindir = Restaurant[(int)Restaurants.Alfs, (int)RestField.DDDir];
            //    DDVerify();
            //    SSLStatus.Text = $"Building FD & A/R customer links... {lbusname}";
            //    Build_FD_AR_Link();
            //    SSLStatus.Text = $"Copying file back to server... {lbusname}";
            //    CopyBack();
            //}
            //if (CBWatsonShelter.Checked)
            //{
            //    moffice = Restaurant[(int)Restaurants.Watsons, (int)RestField.Office];
            //    mplace = Restaurant[(int)Restaurants.Watsons, (int)RestField.Place];
            //    lbusname = Restaurant[(int)Restaurants.Watsons, (int)RestField.BusName];
            //    mistore = Convert.ToInt32(Restaurant[(int)Restaurants.Watsons, (int)RestField.IStore]);
            //    mcfbdir = Restaurant[(int)Restaurants.Watsons, (int)RestField.CFBDir];
            //    mdigdindir = Restaurant[(int)Restaurants.Watsons, (int)RestField.DDDir];
            //    DDVerify();
            //    SSLStatus.Text = $"Building FD & A/R customer links... {lbusname}";
            //    Build_FD_AR_Link();
            //    SSLStatus.Text = $"Copying file back to server... {lbusname}";
            //    CopyBack();
            //}
            //BtnExit.Enabled = true;
            //BtnContinue.Enabled = true;
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


    }
}
