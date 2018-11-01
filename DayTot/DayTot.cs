using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using asl_SyncLibrary;

namespace DayTot
{
    public partial class DayTot : Form
    {
        private ASLEmail Email = new ASLEmail();
        private CommonFunctions CF = new CommonFunctions();
        private DataWarehouse DW = new DataWarehouse();

        private bool SkiShopFP = true;
        private const string mdatadir = @"g:\fpdata\altaax\";

        internal OleDbConnection VFPConn = new OleDbConnection("Provider=VFPOLEDB.1");

        public Int64 mUnMatched = 0;
        public bool mEmailTest = false;
        public DateTime mStart = DateTime.Now.AddDays(-4);
        public DateTime mEnd = DateTime.Now.AddDays(-1);


        public DayTot()
        {
            InitializeComponent();
        }

        private void File_Cleanup()
        {
            //pack all files (salesdata, possum, pmtdata, skivisits
        }

        private void Update_Uses()
        {
            string mSQLCmd = $"SELECT serialkey FROM {DW.ActiveDatabase}.salesdata WHERE saledate='{mStart.ToString(Mirror.AxessDateFormat)}'";
            DataTable TblSalesData = CF.LoadTable(DW.dwConn, mSQLCmd, "SalesData");
            DataColumn[] keys = new DataColumn[1];
            keys[0] = TblSalesData.Columns["serialkey"];
            TblSalesData.PrimaryKey = keys;
            mSQLCmd = $"SELECT serialkey FROM {DW.ActiveDatabase}.spcards";
            DataTable TblSPCards = CF.LoadTable(DW.dwConn, mSQLCmd, "SPCards");
            keys = new DataColumn[1];
            keys[0] = TblSPCards.Columns["serialkey"];
            TblSPCards.PrimaryKey = keys;
            mSQLCmd = $"SELECT serialkey, COUNT(*) AS count FROM {DW.ActiveDatabase}.skivisits WHERE readdate > '2017/11/19' GROUP BY serialkey";
            using (DataTable TblUseCount = CF.LoadTable(DW.dwConn, mSQLCmd, "UseCount"))
            {
                if (TblUseCount != null)
                {
                    foreach (DataRow UseCountRow in TblUseCount.Rows)
                    {
                        string MSKey = UseCountRow["serialkey"].ToString().Trim();
                        string Cnt = UseCountRow["count"].ToString();
                        DataRow SaleRow = TblSalesData.Rows.Find(MSKey);
                        if (SaleRow != null)
                        {
                            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET uses={Cnt} WHERE serialkey='{MSKey}'");
                        }
                        DataRow SPCardRow = TblSPCards.Rows.Find(MSKey);
                        if (SPCardRow != null)
                        {
                            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards SET uses={Cnt} WHERE serialkey='{MSKey}'");
                        }
                    }
                }
            }
        }

        private void ValidateRow()
        {
            //mySQL
            int tDayOfWeek = Convert.ToInt32(mStart.DayOfWeek);
            if (tDayOfWeek == 0)
            {
                tDayOfWeek = 7;
            }
            if (!CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.altasum", $"saledate='{mStart.ToString(Mirror.AxessDateFormat)}'"))
            {
                CF.ExecuteSQL(DW.dwConn, $"INSERT INTO {DW.ActiveDatabase}.altasum (saledate, ytdweek, dofweek) VALUES ('{mStart.ToString(Mirror.AxessDateFormat)}',{CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(mStart, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday).ToString()},{tDayOfWeek.ToString()})");
            }
            //VFP
            VFPConn.ConnectionString = @"provider=vfpoledb.1;data source = G:\FPData\altaax\altasum_x.dbf";
            if (CF.RowCount(VFPConn, "altasum_x", $"saledate={CF.SetFoxProDate(mStart)}") == 0 )
            //if (!CF.RowExists(VFPConn, "altasum", $"saledate={CF.SetFoxProDate(mStart)}"))
            {
                VFPConn.ConnectionString = @"provider=vfpoledb.1;data source = G:\FPData\altaax\altasum_x.dbf";
                string Query = $"INSERT INTO altasum_x (saledate, ytdweek, dofweek,tixsales,tixsalesq,rtixsales,rtixsalesq,convuse,convuseq,expuse,expuseq,websales," +
                    $"websalesq,mcsales,mcsalesq,expsales,expsalesq,powsales,powsalesq,spsales,spsalesq,sssales,sssalesq,sspsales,sspsalesq,ssasales,ssasalesq,ssksales," +
                    $"ssksalesq,ssosales,ssosalesq,catsales,catsalesq,rcsales,rcsalesq,osales,osalesq,giftsales,giftsalesq,weborders,webordersq,tcash,tar,tweb,tvchr," +
                    $"tgift,tcard,alfssales,alfsqty,alfsshort,watscafe,watscg,watsbrew,watsshort,albsales,albqty,albshort,skisbase,skiswats,skisalfs,skisdemo,skisweb," +
                    $"skisski,skisretail,skisrental,skisfood,skisgc,skisserv,skisshort,tickvisit,powvisit,expvisit,sbvisit,ccvisit,mcvisit,slvisit,ngvisit,s3visit,spvisit," +
                    $"empspvisit,goldvisit,skivisits,newloadqty,reloadqty,webloadqty,powcrqty,powcramt,passreads,glupdate,bflag) " +
                    $"VALUES ({CF.SetFoxProDate(mStart)},{CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(mStart, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday).ToString()},{tDayOfWeek.ToString()}," +
                    $"0.00,0,0.00,0,0.00,0,0.00,0,0.00,0,0.00,0,0.00,0,0.00,0,0,0,0.00,0,0.00,0,0.00,0,0.00,0,0.00,0,0.00,0,0.00,0,0.00,0,0.00,0,0.00,0,0.00,0.00,0.00,0.00,0.00," +
                    $"0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0)";
                CF.ExecuteSQL(VFPConn, Query);
            }
        }

            private void Sum_Ski_Visits()
        {
            string[,] sv = new string[14, 2] { { "TS", "0" }, { "PC", "0" }, { "EX", "0" }, { "SB", "0" }, { "MC", "0" }, { "CC", "0" }, { "SL", "0" }, { "NG", "0" }, { "SP", "0" }, { "S3", "0" }, { "EM", "0" }, { "SO", "0" }, { "SR", "0" }, { "OS", "0" } };
            long TotalRides = 0;
            if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.skivisits", $"readdate='{mStart.ToString(Mirror.AxessDateFormat)}' AND altaflag='Y'"))
            {
                string SkiVisitsQuery = $"SELECT COUNT(*) FROM {DW.ActiveDatabase}.skivisits WHERE readdate='{mStart.ToString(Mirror.AxessDateFormat)}' AND altaflag='Y'";
                for (int i = 0; i < 11; i++)
                {
                    Int32 RideCount = Convert.ToInt32(CF.GetSQLField(DW.dwConn, $"{SkiVisitsQuery} AND tgroup='{sv[i, 0]}'"));
                    TotalRides += RideCount;
                    sv[i, 1] = RideCount.ToString();
                }
                //long SBOnlyRides = CF.RowCount(DW.dwConn, $"{DW.ActiveDatabase}.skivisits", $"readdate='{mStart.ToString(Mirror.AxessDateFormat)}' AND tgroup='MC' AND sbflag='Y' AND altaflag='N'");
                //TotalRides -= SBOnlyRides;
                //sv[4, 1] = (Convert.ToInt32(sv[4, 1]) -  SBOnlyRides).ToString();
                sv[11, 1] = CF.GetSQLField(DW.dwConn, $"SELECT COUNT(*) AS passes FROM {DW.ActiveDatabase}.skivisits WHERE readdate='{mStart.ToString(Mirror.AxessDateFormat)}' AND passtoalta<>0");
                ValidateRow();
                //first do mySQL
                string tQuery = $"UPDATE {DW.ActiveDatabase}.altasum SET " +
                    $"tickvisit = {sv[0, 1]}, " +      // TS  
                    $"powvisit = {sv[1, 1]}, " +       // PC
                    $"expvisit = {sv[2, 1]}, " +       // EX
                    $"sbvisit = {sv[3, 1]}, " +        // SB
                    $"mcvisit = {sv[4, 1]}, " +        // MC
                    $"ccvisit = {sv[5, 1]}, " +        // CC
                    $"slvisit = {sv[6, 1]}, " +        // SL
                    $"ngvisit = {sv[7, 1]}, " +        // NG
                    $"spvisit = {sv[8, 1]}, " +        // SP
                    $"s3visit = {sv[9, 1]}, " +        // S3
                    $"empspvisit = {sv[10, 1]}, " +    // EM
                    $"passreads = {sv[11, 1]}, " +	 // sugarloaf pass reads
                    $"skivisits = {(Convert.ToDecimal(sv[0, 1]) + Convert.ToDecimal(sv[1, 1]) + Convert.ToDecimal(sv[2, 1]) + Convert.ToDecimal(sv[3, 1]) + Convert.ToDecimal(sv[4, 1]) + Convert.ToDecimal(sv[5, 1]) + Convert.ToDecimal(sv[6, 1]) + Convert.ToDecimal(sv[7, 1]) + Convert.ToDecimal(sv[8, 1]) + Convert.ToDecimal(sv[9, 1]) + Convert.ToDecimal(sv[10, 1])).ToString()} " +
                    $"WHERE saledate = ";
                CF.ExecuteSQL(DW.dwConn, $"{tQuery}'{mStart.ToString(Mirror.AxessDateFormat)}'");
                //then do FoxPro
                tQuery = tQuery.Replace($"{DW.ActiveDatabase}.", string.Empty);
                VFPConn.ConnectionString = @"provider=vfpoledb.1;data source = G:\FPData\altaax\altasum_x.dbf";
                CF.ExecuteSQL(VFPConn, $"{tQuery.Replace("altasum","altasum_x")}{CF.SetFoxProDate(mStart)}");

                Int32 X = Convert.ToInt32(CF.GetSQLField(DW.dwConn, $"{SkiVisitsQuery} AND nticktype<1"));
                Int32 Y = Convert.ToInt32(CF.GetSQLField(DW.dwConn, $"{SkiVisitsQuery} AND nticktype<1 AND nkassanr>=50 AND nkassanr<=55"));
                Int32 Z = Convert.ToInt32(CF.GetSQLField(DW.dwConn, $"{SkiVisitsQuery} AND nticktype<1 AND nkassanr<=30 AND nkassanr NOT IN (20, 21, 22)"));
                if (Z + Y > 0)
                {
                    string MText = $"Some passes read at the gates on {mStart.ToString(Mirror.AxessDateFormat)} - No matching sales in Axess.\r\n\n";
                    if (Z > 0)
                    {
                        MText += $"Unmatched from Alta Lodges ({Z.ToString()})./r/n/n";
                    }
                    if (Y > 0)
                    {
                        MText += $"Unmatched from Alta Ticket Offices ({Y.ToString()})./r/n/n";
                    }
                    DataTable TblPOEReads = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.skivisits WHERE readdate='{mStart.ToString(Mirror.AxessDateFormat)}' AND nticktype<1 AND ((nkassanr<=30 AND nkassanr NOT IN (20, 21, 22)) OR (nkassanr>=50 AND nkassanr<=55))", "POEReads");
                    if (TblPOEReads != null)
                    {
                        MText += $"<table width=100%><tr><th>POS</th><th>Serial#</th><th>Uni</th><th>T Group</th><th>Ticket Type</th><th>Ticket Description</th><th>Person Type</th><th>Person Description</th><th>Web ID</th><th>Rides</th></tr> ";
                        foreach (DataRow RPOE in TblPOEReads.Rows)
                        {
                            MText += $"<tr><td>{RPOE["nkassanr"].ToString()}</td><td>{RPOE["nseriennr"].ToString()}</td><td>{RPOE["nunicodenr"].ToString()}</td><td>{RPOE["tgroup"].ToString()}</td><td>{RPOE["nticktype"].ToString()}</td><td>{RPOE["tickdesc"].ToString()}</td><td>{RPOE["nperstype"].ToString()}</td><td>{RPOE["persdesc"].ToString()}</td><td>{RPOE["Rides"].ToString()}</td></tr>";
                        }
                        MText += "</table>";
                        //mail tblpoereads to pete and daryl
                        Email.IsHTML = true;
                        Email.ToAddress = "pete@alta.com;daryl@alta.com;accounting@alta.com";
                        Email.Subject = "UNMATCHED READS";
                        Email.FromAddress = "AltaReports@alta.com";
                        Email.Body = MText;
                        Email.CreateEmail();
                        //Email.SendEmail();
                    }
                }
            }
        }

        private void Update_ARSub()
        {
            //no longer used
        }

        private void Sum_SkiShop()
        {
            decimal mwats = 0;
            decimal mbase = 0;
            decimal malfs = 0;
            decimal MSHORT = 0;
            decimal mretail = 0;
            decimal mservice = 0;
            decimal mfood = 0;
            decimal mrental = 0;
            decimal mski = 0;
            decimal mweb = 0;
            decimal mgift = 0;
            decimal mdemo = 0;

            DataTable TblSkiShop;

            if (SkiShopFP) //using FoxPro
            {
                VFPConn.ConnectionString = $@"provider=vfpoledb.1;data source = G:\FPData\SkiShop\srdaysum.dbf";
                TblSkiShop = CF.LoadTable(VFPConn, $"SELECT * FROM srdaysum WHERE sdate={CF.SetFoxProDate(mStart)}", "srdaysum");
            }
            else //using MySQL
            {
                TblSkiShop = CF.LoadTable(DW.dwConn, $"SELECT * FROM srdaysum", "srdaysum");
            }
            if (TblSkiShop != null)
            {
                foreach (DataRow RowSkiShop in TblSkiShop.Rows)
                {
                    decimal tVal = Convert.ToDecimal(RowSkiShop["srretail"].ToString()) + Convert.ToDecimal(RowSkiShop["srservice"].ToString()) + Convert.ToDecimal(RowSkiShop["srfood"].ToString()) + Convert.ToDecimal(RowSkiShop["srrental"].ToString()) + Convert.ToDecimal(RowSkiShop["srski"].ToString()) + Convert.ToDecimal(RowSkiShop["srgiftsale"].ToString()) - Convert.ToDecimal(RowSkiShop["webtot"].ToString());
                    switch (RowSkiShop["loc"].ToString().Trim().ToUpper())
                    {
                        case "A":
                            mbase += tVal;
                            break;
                        case "B":
                            mwats += tVal;
                            break;
                        case "C":
                            mdemo += tVal;
                            break;
                        case "D":
                            malfs += tVal;
                            break;
                    }
                    mgift += Convert.ToDecimal(RowSkiShop["webagc"].ToString());
                    mweb += Convert.ToDecimal(RowSkiShop["webtot"].ToString());
                    mservice += Convert.ToDecimal(RowSkiShop["srservice"].ToString());
                    mfood += Convert.ToDecimal(RowSkiShop["srfood"].ToString());
                    mrental += Convert.ToDecimal(RowSkiShop["srrental"].ToString());
                    mretail += Convert.ToDecimal(RowSkiShop["srretail"].ToString());
                    mski += Convert.ToDecimal(RowSkiShop["srski"].ToString());
                    MSHORT += Convert.ToDecimal(RowSkiShop["overshort"].ToString());
                    ValidateRow();
                    CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.altasum SET skisalfs={malfs.ToString()}, skiswats={mwats.ToString()}, skisbase={mbase.ToString()}, skisdemo={mdemo.ToString()}, skisweb={mweb.ToString()}, skisserv={mservice.ToString()}, skisrental={mrental.ToString()}, skisski={mski.ToString()}, skisretail={mretail.ToString()}, skisfood={mfood.ToString()}, skisgc={mgift.ToString()}, skisshort={MSHORT.ToString()} WHERE saledate='{mStart.ToString(Mirror.AxessDateFormat)}'");
                    VFPConn.ConnectionString = @"provider=vfpoledb.1;data source = G:\FPData\altaax\altasum_x.dbf";
                    CF.ExecuteSQL(VFPConn, $"UPDATE altasum_x SET skisalfs={malfs.ToString()}, skiswats={mwats.ToString()}, skisbase={mbase.ToString()}, skisdemo={mdemo.ToString()}, skisweb={mweb.ToString()}, skisserv={mservice.ToString()}, skisrental={mrental.ToString()}, skisski={mski.ToString()}, skisretail={mretail.ToString()}, skisfood={mfood.ToString()}, skisgc={mgift.ToString()}, skisshort={MSHORT.ToString()} WHERE saledate={CF.SetFoxProDate(mStart)}");
                }
            }

        }

        private void Sum_Sales()
        {
            decimal msales = 0;
            decimal mpmts = 0;
            decimal mmtncc = 140;
            decimal mmtn1day = 70;
            decimal mtixsales = 0;
            Int32 mtixsalesq = 0;
            decimal mconvuse = 0;
            Int32 mconvuseq = 0;
            decimal mrtixsales = 0;
            Int32 mrtixsalesq = 0;
            decimal mwebsales = 0;
            Int32 mwebsalesq = 0;
            //decimal msbsales = 0;
            //Int32 msbsalesq = 0;
            decimal mspsales = 0;
            Int32 mspsalesq = 0;
            decimal mpowsales = 0;
            Int32 mpowsalesq = 0;
            decimal mmcsales = 0;
            Int32 mmcsalesq = 0;
            decimal mexpuse = 0;
            Int32 mexpuseq = 0;
            decimal msspsales = 0;
            Int32 msspsalesq = 0;
            decimal mssasales = 0;
            Int32 mssasalesq = 0;
            //decimal mexpsales = 0;
            //Int32 mexpsalesq = 0;
            decimal mssksales = 0;
            Int32 mssksalesq = 0;
            decimal mssosales = 0;
            Int32 mssosalesq = 0;
            decimal mgiftsales = 0;
            Int32 mgiftsalesq = 0;
            decimal mosales = 0;
            Int32 mosalesq = 0;
            decimal mcatsales = 0;
            Int32 mcatsalesq = 0;
            decimal mrcsales = 0;
            Int32 mrcsalesq = 0;
            //decimal mvctscamt = 0;
            //Int32 mvctscqty = 0;
            decimal mpowcreditamt = 0;
            Int32 mpowcreditqty = 0;


            string Query = $"SELECT saledate, tgroup, nkassanr, nticktype, nperstype, nartnr, SUM(tfactor) AS qty, SUM(tariff) as sales from {DW.ActiveDatabase}.salesdata where saledate = '{mStart.ToString(Mirror.AxessDateFormat)}' group by saledate, nkassanr, tgroup, nticktype, nperstype, nartnr";
            using (DataTable TblSalesTot = CF.LoadTable(DW.dwConn, Query, "salestot"))
            {
                foreach (DataRow RowSale in TblSalesTot.Rows)
                {
                    decimal tsales = Convert.ToDecimal(RowSale["sales"].ToString());
                    Int32 tqty = (RowSale["qty"].ToString() == string.Empty? 0 : Convert.ToInt32(RowSale["qty"].ToString()));
                    Int32 tticktype = (RowSale["nticktype"].ToString() == string.Empty ? 0 : Convert.ToInt32(RowSale["nticktype"].ToString()));
                    Int32 tperstype = (RowSale["nperstype"].ToString() == string.Empty ? 0 : Convert.ToInt32(RowSale["nperstype"].ToString()));
                    Int32 tkassanr = (RowSale["nkassanr"].ToString() == string.Empty ? 0 : Convert.ToInt32(RowSale["nkassanr"].ToString()));
                    string tgroup = RowSale["tgroup"].ToString().Trim();
                    //string toffice = RowSale["office"].ToString().Trim();
                    msales += tsales;
                    if (tticktype == 5000)
                    {
                        mpowcreditqty += tqty;
                        mpowcreditamt += tsales;
                    }
                    decimal Famt = tsales;
                    switch (tticktype)
                    {
                        case 2025:
                            Famt = tqty * mmtncc;
                            break;
                        case 170:
                            Famt = tqty * mmtn1day;
                            break;
                    }
                    if (tkassanr >= 50 && tkassanr <= 55 && tgroup == "TS") //lodge ticket sales
                    {
                        mrtixsales += Famt;
                        mrtixsalesq += tqty;
                    }
                    else if (tticktype == 2025)                              //MTC CC issued at POS
                    {
                        mmcsales += Famt;
                        mmcsalesq += tqty;
                    }
                    else if (tticktype == 170)                              //1 day free day issued at POS
                    {
                        mmcsales += Famt;
                        mmcsalesq += tqty;
                    }
                    else if (tkassanr == 33 && tgroup == "MC")               //50% days calculated base on use
                    {
                        mmcsales += Famt;
                        mmcsalesq += tqty;
                    }
                    else if (tticktype == 1 && tperstype == 101)             //MTC adult 50% days issued at POS
                    {
                        mmcsales += Famt;
                        mmcsalesq += tqty;
                    }
                    else if (tticktype == 1 && tperstype == 102)             //MTC child 50% days issued at POS
                    {
                        mmcsales += Famt;
                        mmcsalesq += tqty;
                    }
                    else if (tkassanr == 33 && tgroup == "CC")               //conveneince card use
                    {
                        mconvuse += Famt;
                        mconvuseq += tqty;
                    }
                    else if (tkassanr == 33 && tgroup == "EX")                //gold & copper card use
                    {
                        mexpuse += Famt;
                        mexpuseq += tqty;
                    }
                    else if (tgroup == "PC")
                    {
                        mpowsales += Famt;
                        mpowsalesq += tqty;
                    }
                    else if (tkassanr == 20 && tgroup == "TS")               //web reloads
                    {
                        mwebsales += Famt;
                        mwebsalesq += tqty;
                    }
                    //else if (toffice == "SB" && tgroup == "TS")              //snowbird ticket sales
                    //{
                    //    msbsales += Famt;
                    //    msbsalesq += tqty;
                    //}
                    else if (tgroup == "TS")      //all other TS sales
                    {
                        mtixsales += Famt;
                        mtixsalesq += tqty;
                    }
                    // Passes
                    else if (tgroup == "SP")
                    {
                        mspsales += Famt;
                        mspsalesq += tqty;
                    }
                    // Ski School
                    else if (tgroup == "SSP")
                    {
                        msspsales += Famt;
                        msspsalesq += tqty;
                    }
                    else if (tgroup == "SSA")
                    {
                        mssasales += Famt;
                        mssasalesq += tqty;
                    }
                    else if (tgroup == "SSK")
                    {
                        mssksales += Famt;
                        mssksalesq += tqty;
                    }
                    else if (tgroup == "SSO")
                    {
                        mssosales += Famt;
                        mssosalesq += tqty;
                    }
                    // Other Sales
                    else if (tgroup == "GC")
                    {
                        mgiftsales += Famt;
                        mgiftsalesq += tqty;
                    }
                    else if (tgroup == "CAT")
                    {
                        mcatsales += Famt;
                        mcatsalesq += tqty;
                    }
                    else if (tgroup == "RC")
                    {
                        mrcsales += Famt;
                        mrcsalesq += tqty;
                    }
                    //    OTHERWISE
                    else
                    {
                        if (tgroup != "AR")
                        {
                            mosales += Famt;
                            mosalesq += tqty;
                        }
                    }
                }
            }
            decimal mcash = 0;
            decimal mcard = 0;
            decimal mar = 0;
            decimal mweb = 0;
            decimal mvchr = 0;
            decimal mgift = 0;
            using (DataTable TblPayments = CF.LoadTable(DW.dwConn, $"SELECT saledate, ptype, SUM(pmtamt) AS pmts, COUNT(pmtamt) AS qty FROM {DW.ActiveDatabase}.pmtdata WHERE saledate = '{mStart.ToString(Mirror.AxessDateFormat)}' GROUP BY saledate, ptype", "pmtstot"))
            {
                foreach (DataRow RowPayment in TblPayments.Rows)
                {
                    decimal tamt = Convert.ToDecimal(RowPayment["pmts"].ToString());
                    mpmts += tamt;
                    switch (RowPayment["ptype"].ToString().Trim())
                    {
                        case "$":
                            mcash += tamt;
                            break;
                        case "CC":
                            mcard += tamt;
                            break;
                        case "AR":
                            mar += tamt;
                            break;
                        case "WE":
                            mweb += tamt;
                            break;
                        case "V":
                            mvchr += tamt;
                            break;
                        case "G":
                            mgift += tamt;
                            break;
                    }
                }
            }
            if (!CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.altasum", $"saledate='{mStart.ToString(Mirror.AxessDateFormat)}'"))
            {
                ValidateRow();
            }
            Query = $"UPDATE {DW.ActiveDatabase}.altasum SET " +
                $"tixsales = {mtixsales.ToString()}, " +
                $"tixsalesq = {mtixsalesq.ToString()}, " +
                $"rtixsales={mrtixsales.ToString()}, " +
                $"rtixsalesq={mrtixsalesq.ToString()}, " +
                $"convuse = {mconvuse.ToString()}, " +
                $"convuseq = {mconvuseq.ToString()}, " +
                $"expuse = {mexpuse.ToString()}, " +
                $"expuseq = {mexpuseq.ToString()}, " +
                $"websales={mwebsales.ToString()}, " +
                $"websalesq={mwebsalesq.ToString()}, " +
                $"mcsales = {mmcsales.ToString()}, " +
                $"mcsalesq = {mmcsalesq.ToString()}, " +
                //$"expsales = {mexpsales.ToString()}, " +
                //$"expsalesq = {mexpsalesq.ToString()}, " +
                $"powsales = {mpowsales.ToString()}, " +
                $"powsalesq = {mpowsalesq.ToString()}, " +
                $"spsales={mspsales.ToString()}, " +
                $"spsalesq={mspsalesq.ToString()},  " +
                //$"sssales = {msssales.ToString()}, " +
                //$"sssalesq = {msssalesq.ToString()}, " +
                $"sspsales = {msspsales.ToString()}, " +
                $"sspsalesq = {msspsalesq.ToString()}, " +
                $"ssasales = {mssasales.ToString()}, " +
                $"ssasalesq = {mssasalesq.ToString()}, " +
                $"ssksales = {mssksales.ToString()}, " +
                $"ssksalesq = {mssksalesq.ToString()}, " +
                $"ssosales = {mssosales.ToString()}, " +
                $"ssosalesq = {mssosalesq.ToString()}, " +
                $"catsales = {mcatsales.ToString()}, " +
                $"catsalesq = {mcatsalesq.ToString()}, " +
                $"rcsales = {mrcsales.ToString()}, " +
                $"rcsalesq = {mrcsalesq.ToString()}, " +
                $"ossales = {mosales.ToString()}, " +
                $"ossalesq = {mosalesq.ToString()}, " +
                $"giftsales = {mgiftsales.ToString()}, " +
                $"giftsalesq = {mgiftsalesq.ToString()}, " +
                //$"weborders = {mweborders.ToString()}, " +
                //$"webordersq = {mwebordersq.ToString()}, " +
                $"tcash = {mcash.ToString()}, " +
                $"tar = {mar.ToString()}, " +
                $"tweb = {mweb.ToString()}, " +
                $"tvchr = {mvchr.ToString()}, " +
                $"tgift = {mgift.ToString()}, " +
                $"tcard = {mcard.ToString()}, " +
                $"bflag={(((mpmts == msales) || (Math.Abs(mpmts - msales) <= 5)) ? "0" : "1")} " +
                $"WHERE saledate = ";
            CF.ExecuteSQL(DW.dwConn, Query + $"'{mStart.ToString(Mirror.AxessDateFormat)}'");
            VFPConn.ConnectionString = @"provider=vfpoledb.1;data source = G:\FPData\altaax\altasum_x.dbf";
            CF.ExecuteSQL(VFPConn, Query.Replace($"{DW.ActiveDatabase}.altasum","altasum_x").Replace("ossales","osales") + CF.SetFoxProDate(mStart));
        }

        private void Sum_FoodBev()
        {
            decimal MalfsSALES = 0;
            decimal MalfsSHORT = 0;
            decimal mwatssales = 0;
            decimal mwatsshort = 0;
            decimal mALBsales = 0;
            decimal mALBshort = 0;
            decimal mcolsales = 0;
            //decimal mcolshort = 0;
            decimal mbrewsales = 0;
            //decimal mbrewshort = 0;
            VFPConn.ConnectionString = @"provider=vfpoledb.1;data source = G:\FPData\FB\daytot.dbf";
            using (DataTable TblDayTot = CF.LoadTable(VFPConn, $"SELECT * FROM daytot WHERE sdate={CF.SetFoxProDate(mStart)}", "daytot"))
            {
                if (TblDayTot != null)
                foreach (DataRow RowDayTot in TblDayTot.Rows)
                {
                    decimal SalesTot = Convert.ToDecimal(RowDayTot["salestot"].ToString());
                    decimal ShortTot = Convert.ToDecimal(RowDayTot["cashshort"].ToString()) + Convert.ToDecimal(RowDayTot["cardshort"].ToString());
                    if (RowDayTot["office"].ToString().Trim().ToUpper() == "ALB")
                    {
                        mALBsales += SalesTot;
                        mALBshort += ShortTot;
                    }
                    else
                    {
                        if (RowDayTot["office"].ToString().Trim().ToUpper() == "ALF")
                        {
                            MalfsSALES += SalesTot;
                            MalfsSHORT += ShortTot;
                        }
                        else
                        {
                            if (RowDayTot["office"].ToString().Trim().ToUpper() == "WAT")
                            {
                                if (RowDayTot["drawer"].ToString().Trim() == "6")
                                {
                                    mbrewsales += SalesTot;
                                }
                                else
                                {
                                    if (RowDayTot["drawer"].ToString().ToUpper() == "5")
                                    {
                                        mcolsales += SalesTot;
                                    }
                                    else
                                    {
                                        mwatssales += SalesTot;
                                    }
                                }
                                mwatsshort += ShortTot;
                            }
                        }
                    }
                }
            }
            ValidateRow();
            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.altasum SET alfssales={MalfsSALES.ToString()}, alfsshort={MalfsSHORT.ToString()}, albsales={mALBsales.ToString()}, albshort={mALBshort.ToString()}, watscafe={mwatssales.ToString()}, watscg={mcolsales.ToString()}, watsbrew={mbrewsales.ToString()}, watsshort={mwatsshort.ToString()} WHERE saledate='{mStart.ToString(Mirror.AxessDateFormat)}'");
            VFPConn.ConnectionString = @"provider=vfpoledb.1;data source = G:\FPData\altaax\altasum_x.dbf";
            CF.ExecuteSQL(VFPConn, $"UPDATE altasum_x SET alfssales={MalfsSALES.ToString()}, alfsshort={MalfsSHORT.ToString()}, albsales={mALBsales.ToString()}, albshort={mALBshort.ToString()}, watscafe={mwatssales.ToString()}, watscg={mcolsales.ToString()}, watsbrew={mbrewsales.ToString()}, watsshort={mwatsshort.ToString()} WHERE saledate={CF.SetFoxProDate(mStart)}");
        }

        private void RunDay(DateTime NewDate)
        {
            mStart = NewDate;
            ValidateRow();
            Debug.Print(mStart.ToShortDateString());
            if (checkBox1.Checked)
            {
                Sum_Sales();
                //checkBox1.Checked = false;
                //Application.DoEvents();
            }
            if (checkBox2.Checked)
            {
                Sum_FoodBev();
                //checkBox2.Checked = false;
                //Application.DoEvents();
            }
            if (checkBox3.Checked)
            {
                Sum_SkiShop();
                //checkBox3.Checked = false;
                //Application.DoEvents();
            }
            if (checkBox4.Checked)
            {
                //Update_ARSub();
                //checkBox4.Checked = false;
                //Application.DoEvents();
            }
            if (checkBox5.Checked)
            {
                mUnMatched = 0;
                Sum_Ski_Visits();
                //checkBox5.Checked = false;
                //Application.DoEvents();
            }
            if (checkBox6.Checked)
            {
                if (mStart == mEnd)
                {
                    Update_Uses();
                    //checkBox6.Checked = false;
                    //Application.DoEvents();
                }
            }
            if (checkBox7.Checked)
            {
                if (mStart == mEnd)
                {
                    //File_Cleanup();
                    //checkBox7.Checked = false;
                    //Application.DoEvents();
                }
            }
        }

        private void DayTot_Shown(object sender, EventArgs e)
        {
            for (DateTime tDate = mStart; tDate <= mEnd; tDate = tDate.AddDays(1))
            {
                RunDay(tDate);
            }
            //RunDay(new DateTime(2018, 10, 22));
            Application.Exit();
        }
    }

}
