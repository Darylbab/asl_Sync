using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using System.Data.OleDb;
using asl_SyncLibrary;
using System.Diagnostics;
using Microsoft.Office.Interop.Excel;

namespace AltaSum
{
    public partial class AltaSum : Form
    {
        //"saledate", "ytdweek", "dofweek", "tixsales", "tixsalesq", "rtixsales", "rtixsalesq", "convuse", "convuseq", "expuse", "expuseq", "websales", "websalesq", "mcsales", "mcsalesq", "expsales", "expsalesq", "powsales", "powsalesq", "spsales", "spsalesq"20, "sssales", "sssalesq", "sspsales", "sspsalesq", "ssasales", "ssasalesq", "ssksales", "ssksalesq", "ssosales", "ssosalesq"30, "catsales", "catsalesq", "rcsales", "rcsalesq", "ossales", "ossalesq", "giftsales", "giftsalesq", "weborders", "webordersq"40, "tcash", "tar", "tweb", "tvchr", "tgift", "tcard", "alfssales", "alfsqty", "alfsshort", "watscafe", "watscg", "watsbrew", "watsshort", "albsales", "albqty", "albshort", "skisbase", "skiswats", "skisalfs", "skisdemo"60, "skisweb", "skisski", "skisretail", "skisrental", "skisfood", "skisgc", "skisserv", "skisshort", "tickvisit", "powvisit", "expvisit", "sbvisit", "ccvisit", "mcvisit", "slvisit", "ngvisit", "s3visit", "spvisit", "empspvisit", "goldvisit"80, "skivisits", "newloadqty", "reloadqty", "webloadqty", "powcrqty", "powcramt", "passreads", "glupdate", "bflag" 
        //"tixsales", "tixsalesq", "rtixsales", "rtixsalesq", "convuse", "convuseq", "expuse", "expuseq", "websales", "websalesq", "mcsales", "mcsalesq", "expsales", "expsalesq", "powsales", "powsalesq", "spsales", "spsalesq", "sssales", "sssalesq", "sspsales", "sspsalesq", "ssasales", "ssasalesq", "ssksales", "ssksalesq", "ssosales", "ssosalesq", "catsales", "catsalesq", "rcsales", "rcsalesq", "ossales", "ossalesq", "giftsales", "giftsalesq", "weborders", "webordersq", "tcash", "tar", "tweb", "tvchr", "tgift", "tcard", "alfssales", "alfsqty", "alfssshort", "watscafe", "watscg", "watsbrew", "watsshort", "albsales", "albqty", "albshort", "skisbase", "skiswats", "skisalfs", "skisdemo", "skisweb", "skisski", "skisretail", "skisrental", "skisfood", "skisgc", "skisserv", "skisshort", "tickvisit", "powvisit", "expvisit", "sbvisit", "ccvisit", "mcvisit", "slvisit", "ngvisit", "s3visit", "spvisit", "empspvisit", "goldvisit", "skivisitis", "newloadqty", "reloadqty", "webloadqty", "powcrqty", "powcramt", "passreads"
        public string[] locs = new string[] { "saledate", "ytdweek", "dofweek", "tixsales", "tixsalesq", "rtixsales", "rtixsalesq", "convuse", "convuseq", "expuse", "expuseq", "websales", "websalesq", "mcsales", "mcsalesq", "expsales", "expsalesq", "powsales", "powsalesq", "spsales", "spsalesq", "sssales", "sssalesq", "sspsales", "sspsalesq", "ssasales", "ssasalesq", "ssksales", "ssksalesq", "ssosales", "ssosalesq", "catsales", "catsalesq", "rcsales", "rcsalesq", "ossales", "ossalesq", "giftsales", "giftsalesq", "weborders", "webordersq", "tcash", "tar", "tweb", "tvchr", "tgift", "tcard", "alfssales", "alfsqty", "alfsshort", "watscafe", "watscg", "watsbrew", "watsshort", "albsales", "albqty", "albshort", "skisbase", "skiswats", "skisalfs", "skisdemo", "skisweb", "skisski", "skisretail", "skisrental", "skisfood", "skisgc", "skisserv", "skisshort", "tickvisit", "powvisit", "expvisit", "sbvisit", "ccvisit", "mcvisit", "slvisit", "ngvisit", "s3visit", "spvisit", "empspvisit", "goldvisit", "skivisits", "newloadqty", "reloadqty", "webloadqty", "powcrqty", "powcramt", "passreads", "glupdate", "bflag" };

        //private AltaSkiLifts asl = new AltaSkiLifts();
        private Mirror AM = new Mirror();
        private CommonFunctions CF = new CommonFunctions();
        private BUY_ALTA_COM BUY = new BUY_ALTA_COM();
        private DataWarehouse DW = new DataWarehouse();
        OleDbConnection VFPConn = new OleDbConnection("Provider=VFPOLEDB.1");

        //private DataRow SeasonInfo;
        private DataRow cday;
        private DataRow pday;
        private System.Data.DataTable tblAltaData = new System.Data.DataTable();
        //private DataRow tblAltaSum;
        private DataRow cytd;
        private DataRow cweek;
        private DataRow pytd;
        private DataRow pweek;
        private System.Data.DataTable grid1 = new System.Data.DataTable();
        private DataRow grid2;
        private System.Data.DataTable grid3 = new System.Data.DataTable();
        private System.Data.DataTable grid4 = new System.Data.DataTable();

        private DateTime mcystart;
        private DateTime mcyend;
        private string mcydescrip;
        //private Int32 mweek;
        private DateTime mcyday = new DateTime(1999,12,30);
        private Int32 mdow;
        //private string mpydescrip;
        //private DateTime mfdate = new DateTime(1999,12,30);
        //private DateTime mldate = new DateTime(1999,12,30);
        //private DateTime mrdate = new DateTime(1999,12,30);
        //private Int32 mvalue;
        private DateTime mpystart;
        private DateTime mpyend;
        private string mpydescrip;
        private DateTime mpyday;
        //private string mxkey;
        private DateTime mcywstart = new DateTime(1999,12,30);
        private DateTime mpywstart = new DateTime(1999,12,30);
        private DateTime mdate = DateTime.Now.AddDays(-1);
        private static string mdrive = @"G:\";
        private static string mdir = $"{mdrive}FPDATA";
        //private static bool mautoflag = false;
        //private static string mdatadir = @"G:\FPDATA\altaax\";
        //private static string mlodgedir = @"G:\FPDATA\altaax\lodges\";
        //private static string mutadir = @"G:\FPDATA\altaax\uta\";
        //private static string memaildir = @"G:\FPDATA\altaax\email\";
        //private static string msharedir = @"G:\FPDATA\shared\";
        //private static string msharedcode = @"G:\Source\FPCode\shared\";
        //private static string mskishopdir = @"G:\FPDATA\skishop\";
        //private static string minfodir = @"G:\FPDATA\shared\altainfo\";
        //private static string mbeansdir = @"G:\FPDATA\beans\";
        //private static string mtempdir = @"c:\fpdata\temp\";
        //private static string mfbdir = @"G:\FPDATA\fb\";

        public AltaSum()
        {
            InitializeComponent();
            Init();
        }

        private bool tInitialized = false;
        public bool Initialized
        {
            get
            {
                return tInitialized;
            }
        }


        private void Init()
        {
            if (!tInitialized)
            {
                gvSkierVisits.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomRight;
                gvSalesDetail.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomRight;
                gvSales.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomRight;
                gvTodaysSkierVisits.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.BottomRight;
                SetTodaysSkierVisits();
            }
            return;
        }
        

        private void Load_Day()
        {
            mdate = dateTimePicker1.Value;
            Setup_Dates();
            SumDay();
            Load_Data();
            Load_Grids();
        }

        private decimal GetChange(decimal CurVal, decimal PreVal)
        {
            if (CurVal * PreVal == 0)
            {
                return 0;
            }
            return Convert.ToDecimal(Convert.ToInt32(((CurVal / PreVal) - 1) * 10000)) / 100;
        }

        private int Checkval(int aValue)
        {
            if (aValue > 999)
            {
                aValue = 999;
            }
            if (aValue < -999)
            {
                aValue = -999;
            }
            return aValue;
        }

        private void Load_Data()
        {
            CF.ExecuteSQL(DW.dwConn, "UPDATE applications.altadata SET cday=0, pday=0, cweek=0, pweek=0, cytd=0,pytd=0,cdayq=0, pdayq=0, cweekq=0, pweekq=0, cytdq=0, pytdq=0, daypcnt=0, weekpcnt=0, ytdpcnt=0");
            System.Data.DataTable altadata = CF.LoadTable(DW.dwConn, "SELECT * FROM applications.altadata ORDER BY sorder", "altadata");
            foreach (DataRow tRow in altadata.Rows)
            {
                string tQuery = String.Empty;
                if (tRow.Field<int>("aloc") != 0)
                {
                    string aField = locs[tRow.Field<int>("aloc")];
                    if (cday.Field<decimal?>(aField).HasValue)
                    {
                        tQuery += $"cday = {cday.Field<Decimal>(aField).ToString()}";
                        if (pday.Field<decimal?>(aField).HasValue)
                        {
                            tQuery += $"{((tQuery != string.Empty) ? ", " : "")}pday = {pday.Field<Decimal>(aField).ToString()}, daypcnt={GetChange(cday.Field<Decimal>(aField), pday.Field<Decimal>(aField)).ToString()}";
                        }
                    }
                    if (cweek.Field<Decimal?>(aField).HasValue)
                    {
                        tQuery += $"{((tQuery != string.Empty) ? ", ": "")}cweek = {cweek.Field<Decimal>(aField).ToString()}";
                        if (pweek.Field<Decimal?>(aField).HasValue)
                        {
                            tQuery += $"{((tQuery != string.Empty) ? ", " : "")}pweek = {pweek.Field<Decimal>(aField).ToString()}, weekpcnt={GetChange(cweek.Field<Decimal>(aField), pweek.Field<Decimal>(aField)).ToString()}";
                        }
                    }
                    if (cytd.Field<Decimal?>(aField).HasValue)
                    {
                        tQuery += $"{((tQuery != string.Empty) ? ", " : "")}cytd = {cytd.Field<Decimal>(aField).ToString()}";
                        if (pytd.Field<Decimal?>(aField).HasValue)
                        {
                            tQuery += $"{((tQuery != string.Empty) ? ", " : "")}pytd = {pytd.Field<Decimal>(aField).ToString()}, ytdpcnt={GetChange(cytd.Field<Decimal>(aField), pytd.Field<Decimal>(aField)).ToString()}";
                        }
                    }
                }
                if (tRow.Field<int>("qloc") != 0)
                {
                    string qField = locs[tRow.Field<int>("qloc")];
                    if (cday.Field<decimal?>(qField).HasValue)
                    {
                        tQuery += $"{((tQuery != string.Empty) ? ", " : "")}cdayq = {cday.Field<decimal>(qField).ToString()}";
                        if (pday.Field<decimal?>(qField).HasValue)
                        {
                            tQuery += $"{((tQuery != string.Empty) ? ", " : "")}pdayq = {pday.Field<decimal>(qField).ToString()}";
                        }
                    }
                    if (cweek.Field<decimal?>(qField).HasValue)
                    {
                        tQuery += $"{((tQuery != string.Empty) ? ", " : "")}cweekq = {cweek.Field<decimal>(qField).ToString()}";
                        if (pweek.Field<decimal?>(qField).HasValue)
                        {
                            tQuery += $"{((tQuery != string.Empty) ? ", " : "")}pweekq = {pweek.Field<decimal>(qField).ToString()}";
                        }
                    }
                    if (cytd.Field<decimal?>(qField).HasValue)
                    {
                        tQuery += $"{((tQuery != string.Empty) ? ", " : "")}cytdq = {cytd.Field<decimal>(qField).ToString()}";
                        if (pytd.Field<decimal?>(qField).HasValue)
                        {
                            tQuery += $"{((tQuery != string.Empty) ? ", " : "")}pytdq = {pytd.Field<decimal>(qField).ToString()}";
                        }
                    }
                }
                if (tQuery != string.Empty)
                {
                    CF.ExecuteSQL(DW.dwConn, $"UPDATE applications.altadata SET {tQuery} WHERE sorder={tRow.Field<decimal>("sorder")}");
                }
            }

            altadata = CF.LoadTable(DW.dwConn, "SELECT * FROM applications.altadata  WHERE stype = 'T' AND rtype <> 'P' ORDER BY sorder", "altadata");
            foreach (DataRow tRow in altadata.Rows)
            {
                int Ordinal = Convert.ToInt32(tRow.Field<decimal>("Sorder"));
                string tQuery = "SELECT SUM(cday) AS cday, SUM(Cdayq) AS cdayq, SUM(Pday) AS pday, SUM(Pdayq) AS pdayq, SUM(cweek) AS cweek, SUM(cweekq) AS cweekq, SUM(pweek) AS pweek, SUM(pweekq) AS pweekq, SUM(cytd) AS cytd, SUM(cytdq) AS cytdq, SUM(pytd) AS pytd, SUM(pytdq) AS pytdq FROM applications.altadata WHERE ";
                if (tRow["rtype"].ToString() == "X")
                {
                    tQuery += "stype = 'T'";
                }
                else
                {
                    tQuery += $"rtype = '{tRow["rtype"].ToString()}' AND stype <> 'T'";
                }
                DataRow sRow = CF.LoadDataRow(DW.dwConn, tQuery);
                if (sRow["cday"].ToString() != string.Empty)
                {
                    Int32 tcday = Convert.ToInt32(sRow.Field<decimal>("cday"));
                    Int32 tpday = Convert.ToInt32(sRow.Field<decimal>("pday"));
                    Int32 tcweek = Convert.ToInt32(sRow.Field<decimal>("cweek"));
                    Int32 tpweek = Convert.ToInt32(sRow.Field<decimal>("pweek"));
                    Int32 tcytd = Convert.ToInt32(sRow.Field<decimal>("cytd"));
                    Int32 tpytd = Convert.ToInt32(sRow.Field<decimal>("pytd"));
                    tQuery = $"UPDATE applications.altadata SET cday={tcday.ToString()}, cdayq={sRow["cdayq"].ToString()}, pday={tpday.ToString()}, pdayq={sRow["pdayq"].ToString()}, cweek={tcweek.ToString()}, cweekq={sRow["cweekq"].ToString()}, pweek={tpweek.ToString()}, pweekq={sRow["pweekq"].ToString()}, cytd={tcytd.ToString()}, cytdq={sRow["cytdq"].ToString()}, pytd={tpytd.ToString()}, pytdq={sRow["pytdq"].ToString()}";
                    if (tcday != 0 && tpday != 0)
                    {
                        tQuery += $", daypcnt={GetChange(tcday, tpday).ToString()}";
                    }
                    if (tcweek != 0 && tpweek != 0)
                    {
                        tQuery += $", weekpcnt={GetChange(tcweek, tpweek).ToString()}";
                    }
                    if (tcytd != 0 && tpytd != 0)
                    {
                        tQuery += $", weekpcnt={GetChange(tcytd, tpytd).ToString()}";
                    }
                    CF.ExecuteSQL(DW.dwConn, $"{tQuery} WHERE sorder={Ordinal.ToString()}");
                }
            }
        }

        private void Load_Grids()
        {
            gvSkierVisits.Columns["CYDay1"].HeaderText = mcyday.ToString("MM/dd/yyyy");
            gvSkierVisits.Columns["PYDay1"].HeaderText = mpyday.ToString("MM/dd/yyyy");
            grid1 = CF.LoadTable(DW.dwConn, $"SELECT descrip, cday, pday, daypcnt, SPACE(3) as b1, cweek, pweek, weekpcnt, SPACE(3) as b2, cytd, pytd, ytdpcnt FROM applications.altadata WHERE rtype = 'V' ORDER BY SOrder", "grid1");
            gvSkierVisits.DataSource = grid1;
            gvSkierVisits.Refresh();
            grid2 = CF.LoadDataRow(DW.dwConn, $"SELECT cday, pday, daypcnt, SPACE(3) as b1, cweek, pweek, weekpcnt, SPACE(3) as b2, cytd, pytd, ytdpcnt FROM applications.altadata WHERE rtype = 'E'  ORDER BY SOrder");
            textBox7.Text = grid2["cday"].ToString();
            textBox8.Text = grid2["pday"].ToString();
            textBox9.Text = grid2["daypcnt"].ToString();
            textBox11.Text = grid2["cweek"].ToString();
            textBox12.Text = grid2["pweek"].ToString();
            textBox14.Text = grid2["weekpcnt"].ToString();
            textBox16.Text = grid2["cytd"].ToString();
            textBox15.Text = grid2["pytd"].ToString();
            textBox17.Text = grid2["ytdpcnt"].ToString();
            grid3 = CF.LoadTable(DW.dwConn, $"SELECT descrip, cday, pday, daypcnt, SPACE(3) as b1, cweek, pweek, weekpcnt, SPACE(3) as b2, cytd, pytd, ytdpcnt, rtype FROM applications.altadata WHERE stype = 'T' ORDER BY SOrder", "grid3");
            gvSales.Columns["CDaySales"].HeaderText = mcyday.ToString("MM/dd/yyyy");
            gvSales.Columns["PDaySales"].HeaderText = mpyday.ToString("MM/dd/yyyy");
            gvSales.DataSource = grid3;
            gvSales.Refresh();
        }

        public void Setup_Dates()
        {
            mcyday = mdate.Date;
            int curYear = mcyday.Year;
            if (mcyday.Month < 6)
            {
                curYear--;
            }
            //int curXMasWeek = Convert.ToInt32(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(new DateTime(mcyday.Year, 12, 25), CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday));
            //int preXMasWeek = Convert.ToInt32(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(new DateTime(mcyday.Year - 1, 12, 25), CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday));
            mdow = Convert.ToInt32(mcyday.DayOfWeek);    //set Monday to first day of week, Monday = 1 and Sunday = 7
            if (mdow == 0)
            {
                mdow = 7;
            }
            //mweek = Convert.ToInt32(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(mcyday, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday));
            mpyday = mcyday.AddYears(-1);
            int pdow = Convert.ToInt32(mpyday.DayOfWeek);
            if (pdow == 0)
            {
                pdow = 7;
            }
            int z = mdow - pdow;
            if (z < 1)
            {
                pdow = pdow - 7;
                z = mdow - pdow;
            }
            mpyday = mpyday.AddDays(z);
            //int pweek = Convert.ToInt32(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(mpyday, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday));
            //mpyday = mcyday.AddYears(-1).AddDays((7 * (mweek - pweek)) + (mdow - pdow));
            mcywstart = mcyday.AddDays(1 - mdow);
            mcystart = new DateTime(curYear, 6, 1);
            mcyend = mcystart.AddYears(1).AddDays(-1);
            mcydescrip = $"{mcystart.Year}-{mcyend.Year}";
            //txtSeason.Text = mcydescrip.Replace("-20","-");
            mpywstart = mpyday.AddDays(1 - mdow);
            mpystart = mcystart.AddYears(-1);
            mpyend = mcyend.AddYears(-1);
            mpydescrip = $"{mpystart.Year}-{mpyend.Year}"; 
        }

        public void SumDay()
        {
            //SUM FOR BETWEEN(saledate, mcystart, mcyday) TO ARRAY cytd
            //SUM FOR BETWEEN(saledate, mcywstart, mcyday)TO ARRAY cweek
            //SUM FOR saledate = mcyday TO ARRAY cday
            //SUM FOR BETWEEN(saledate, mpystart, mpyday) TO ARRAY pytd
            //SUM FOR BETWEEN(saledate, mpywstart, mpyday)TO ARRAY pweek
            //SUM FOR saledate = mpyday TO ARRAY pday
            string SQLQuery = $"SELECT SUM(tixsales) AS tixsales, SUM(tixsalesq) as tixsalesq, SUM(rtixsales) as rtixsales, SUM(rtixsalesq) as rtixsalesq, SUM(convuse) AS convuse, SUM(convuseq) as convuseq, SUM(expuse) AS expuse, SUM(expuseq) AS expuseq, SUM(websales) AS websales, SUM(websalesq) AS websalesq, SUM(mcsales) AS mcsales, SUM(mcsalesq) AS mcsalesq, SUM(expsales) AS expsales, SUM(expsalesq) AS expsalesq, SUM(powsales) AS powsales, SUM(powsalesq) AS powsalesq, SUM(spsales) AS spsales, SUM(spsalesq) AS spsalesq, SUM(sssales) AS sssales, SUM(sssalesq) AS sssalesq, SUM(sspsales) AS sspsales, SUM(sspsalesq) AS sspsalesq, SUM(ssasales) AS ssasales, SUM(ssasalesq) AS ssasalesq, SUM(ssksales) AS ssksales, SUM(ssksalesq) AS ssksalesq, SUM(ssosales) AS ssosales, SUM(ssosalesq) AS ssosalesq, SUM(catsales) AS catsales, SUM(catsalesq) AS catsalesq, SUM(rcsales) AS rcsales, SUM(rcsalesq) AS rcsalesq, SUM(ossales) AS ossales, SUM(ossalesq) AS ossalesq, SUM(giftsales) AS giftsales, SUM(giftsalesq) AS giftsalesq, SUM(weborders) AS weborders, SUM(webordersq) AS webordersq, SUM(tcash) as tcash, SUM(tar) AS tar, SUM(tweb) AS tweb, SUM(tvchr) AS tvchr, SUM(tgift) AS tgift, SUM(tcard) AS tcard, SUM(alfssales) AS alfssales, SUM(alfsqty) AS alfsqty, SUM(alfsshort) AS alfssshort, SUM(watscafe) AS watscafe, SUM(watscg) AS watscg, SUM(watsbrew) AS watsbrew, SUM(watsshort) AS watsshort, SUM(albsales) AS albsales, SUM(albqty) AS albqty, SUM(albshort) AS albshort, SUM(skisbase) AS skisbase, SUM(skiswats) AS skiswats, SUM(skisalfs) AS skisalfs, SUM(skisdemo) AS skisdemo, SUM(skisweb) AS skisweb, SUM(skisski) AS skisski, SUM(skisretail) AS skisretail, SUM(skisrental) AS skisrental, SUM(skisfood) AS skisfood, SUM(skisgc) AS skisgc, SUM(skisserv) AS skisserv, SUM(skisshort) AS skisshort, SUM(tickvisit) As tickvisit, SUM(powvisit) AS powvisit, SUM(expvisit) AS expvisit, SUM(sbvisit) AS sbvisit, SUM(ccvisit) AS ccvisit, SUM(mcvisit) AS mcvisit, SUM(slvisit) AS slvisit, SUM(ngvisit) AS ngvisit, SUM(s3visit) AS s3visit, SUM(spvisit) AS spvisit, SUM(empspvisit) AS empspvisit, SUM(goldvisit) AS goldvisit, SUM(skivisits) AS skivisits, SUM(newloadqty) AS newloadqty, SUM(reloadqty) AS reloadqty, SUM(webloadqty) AS webloadqty, SUM(powcrqty) AS powcrqty, SUM(powcramt) AS powcramt, SUM(passreads) AS passreads FROM {DW.ActiveDatabase}.altasum";
            cytd = CF.LoadDataRow(DW.dwConn, $"{SQLQuery} WHERE saledate >= '{mcystart.ToString(Mirror.AxessDateFormat)}' AND saledate <= '{mcyday.ToString(Mirror.AxessDateFormat)}'");
            cweek = CF.LoadDataRow(DW.dwConn, $"{SQLQuery} WHERE saledate >= '{mcywstart.ToString(Mirror.AxessDateFormat)}' AND saledate <= '{mcyday.ToString(Mirror.AxessDateFormat)}'");
            cday = CF.LoadDataRow(DW.dwConn, $"{SQLQuery} WHERE saledate = '{mcyday.ToString(Mirror.AxessDateFormat)}'");
            pytd = CF.LoadDataRow(DW.dwConn, $"{SQLQuery} WHERE saledate >= '{mpystart.ToString(Mirror.AxessDateFormat)}' AND saledate <= '{mpyday.ToString(Mirror.AxessDateFormat)}'");
            pweek = CF.LoadDataRow(DW.dwConn, $"{SQLQuery} WHERE saledate >= '{mpywstart.ToString(Mirror.AxessDateFormat)}' AND saledate <= '{mpyday.ToString(Mirror.AxessDateFormat)}'");
            pday = CF.LoadDataRow(DW.dwConn, $"{SQLQuery} WHERE saledate = '{mpyday.ToString(Mirror.AxessDateFormat)}'");
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (gvSales.CurrentRow != null)
            {
                grid4 = CF.LoadTable(DW.dwConn, $"SELECT descrip, cdayq, cday, pdayq, pday, daypcnt, cweekq, cweek, pweekq, pweek, weekpcnt, cytdq, cytd, pytdq, pytd, ytdpcnt FROM applications.altadata WHERE rtype = '{gvSales.CurrentRow.Cells["rtype"].Value.ToString()}' ORDER BY SOrder", "duh");
                label10.Text = mdate.ToString("MMM dd, yyyy");
                label11.Text = mpyday.ToString("MMM dd, yyyy");
                gvSalesDetail.DataSource = grid4;
                gvSalesDetail.Refresh();
            }
        }

        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            Load_Day();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void SkierVisitsAutoRefresh_CheckedChanged(object sender, EventArgs e)
        {
            btnSkierVisitsRefresh.Visible = !SkierVisitsAutoRefresh.Checked;
        }

        private void SetTodaysSkierVisits()
        {
            dateTimePicker1.Value = DateTime.Today.AddDays(-1);  //set to DateTime.Today for release;
            System.Data.DataTable dtTodaysSkierVisits = CF.LoadTable(DW.dwConn, $"SELECT A.tgroup, A.descrip, COUNT(*) AS Visits FROM applications.tgroups AS A inner join applications.skivisits AS B ON B.tgroup=A.tgroup LEFT JOIN applications.altadata AS C ON C.descrip = A.descrip WHERE B.readdate='{DateTime.Today.ToString(Mirror.AxessDateFormat)}' AND B.ALTAFLAG='Y' GROUP BY A.tgroup ORDER BY C.sorder", "SkiVisits");
            Int32 SkierVisitsTotal = 0;
            foreach (DataRow tRow in dtTodaysSkierVisits.Rows)
            {
                SkierVisitsTotal += Convert.ToInt32(tRow["Visits"].ToString());
            }
            object[] rowVals = new object[3];
            DataRowCollection rowCollection = dtTodaysSkierVisits.Rows;
            rowVals[0] = "Σ";
            rowVals[1] = "  Total Visits:";
            rowVals[2] = SkierVisitsTotal;
            DataRow row = rowCollection.Add(rowVals);
            gvTodaysSkierVisits.DataSource = dtTodaysSkierVisits;
            gvTodaysSkierVisits.Refresh();
        }

        private void BtnSkierVisitsRefresh_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            SetTodaysSkierVisits();
            Cursor = Cursors.Default;
        }

        private void TimerSkierVisitsRefresh_Tick(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            SetTodaysSkierVisits();
            Cursor = Cursors.Default;
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            string StatusText = lblStatus.Text;
            lblStatus.Text = "Loading spreadsheet for Alta Summmary Report.";
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            string t = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string r = System.AppDomain.CurrentDomain.BaseDirectory;
            Workbook wb = excel.Workbooks.Open(r + @"\reports\AltaSummary2.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"Alta Summary Report\r\n{mcywstart.ToShortDateString()} - {mcyday.ToShortDateString()}";
            ws.PageSetup.RightHeader = $"Week {Convert.ToInt32(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(mcyday, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday)).ToString()}";
            lblStatus.Text = "Loading data for Alta Summary Report.";
            System.Data.DataTable tDT = CF.LoadTable(DW.dwConn, $"SELECT descrip, cweek, pweek, weekpcnt, cytd, pytd, ytdpcnt, rtype FROM {DW.ActiveDatabase}.altadata WHERE rtype <> '' ORDER BY sorder", "altadata");
            Int32 CurRow = 3;
            Int32 RowSkip = 1;
            if (tDT != null)
            {
                foreach (DataRow tDR in tDT.Rows)
                {
                    if (tDR["descrip"].ToString().StartsWith(" "))
                    {
                        var ulRange = excel.Range[excel.Cells[CurRow - 1, 1], excel.Cells[CurRow - 1, 7]];
                        Borders border = ulRange.Borders;
                        border[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                        ulRange = excel.Range[excel.Cells[CurRow, 1], excel.Cells[CurRow, 7]];
                        ulRange.Font.Bold = true;
                        RowSkip = 2;
                    }
                    lblStatus.Text = $"Setting data row {(CurRow - 2).ToString()} of {tDT.Rows.Count.ToString()}.";
                    string RType = tDR["rtype"].ToString();
                    string CurFormat = (RType == "V" ? "$#0" : "");
                    ws.Cells[CurRow, 1] = tDR["descrip"].ToString();
                    ws.Cells[CurRow, 2] = tDR.Field<int>("cweek").ToString(CurFormat);
                    ws.Cells[CurRow, 3] = tDR.Field<int>("pweek").ToString(CurFormat);
                    ws.Cells[CurRow, 4] = (tDR.Field<decimal>("weekpcnt") / 100).ToString("0#.00%");
                    ws.Cells[CurRow, 5] = tDR.Field<int>("cytd").ToString(CurFormat);
                    ws.Cells[CurRow, 6] = tDR.Field<int>("pytd").ToString(CurFormat);
                    ws.Cells[CurRow, 7] = (tDR.Field<decimal>("ytdpcnt") / 100).ToString("0#.00%");
                    CurRow += RowSkip;
                    RowSkip = 1;
                }
                string CopyName = $"AltaSummaryReport_{DateTime.Now.ToString("yyyy_MM_dd hh_mm")}";
                string FullPathName = $"C:\\temp\\{CopyName}.xlsx";
                lblStatus.Text = $"Saving a copy of {CopyName}.";
                //if (System.IO.File.Exists(FullPathName))
                //{
                //    System.IO.File.Delete(FullPathName);
                //}
                //wb.SaveAs(FullPathName);
                lblStatus.Text = "Printing Alta Summary Report.";
                excel.Visible = true;
                ws.PrintOutEx(Type.Missing, Type.Missing, 1, true);
                excel.Visible = false;
                wb.Close(false);
                lblStatus.Text = StatusText;
                Cursor = Cursors.Default;
            }
        }

        private void BtnDetailSummaryReport_Click(object sender, EventArgs e)
        {
            bool AutoRefresh = SkierVisitsAutoRefresh.Checked;
            timerSkierVisitsRefresh.Enabled = false;
            Cursor = Cursors.WaitCursor;
            string StatusText = lblStatus.Text;
            lblStatus.Text = "Loading spreadsheet for Skier Visits Summmary Report.";
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            string t = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string r = System.AppDomain.CurrentDomain.BaseDirectory;
            Workbook wb = excel.Workbooks.Open(r + @"\reports\SkierVisitsSummary.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"Skier Visits Summary Report\r\n{mdate.ToString("MM/dd/yyyy (dddd)")}";
            lblStatus.Text = "Loading data for Skier Visits Summary Report.";
            ws.Cells[1, 6] = mdate.ToString("MM/dd/yyyy");
            System.Data.DataTable TickSumYear = CF.LoadTable(DW.dwConn, $"SELECT tgroup, tickdesc, persdesc, COUNT(*) as ytickcount, (SELECT COUNT(*) AS dtickcount FROM {DW.ActiveDatabase}.skivisits AS B WHERE readdate = '{mdate.ToString(Mirror.AxessDateFormat)}' AND A.tgroup=B.tgroup AND A.nticktype=B.nticktype AND A.nperstype=B.nperstype) AS dtickcount FROM {DW.ActiveDatabase}.skivisits AS A WHERE readdate <= '{mcyday.ToString(Mirror.AxessDateFormat)}' AND readdate >= '{mcystart.ToString(Mirror.AxessDateFormat)}' GROUP BY tgroup, tickdesc, persdesc ORDER BY tgroup", "ticksumyear");
            Int32 CurRow = 2;
            foreach (System.Data.DataRow tDR in TickSumYear.Rows)
            {
                lblStatus.Text = $"Setting data row {(CurRow - 2).ToString()} of {TickSumYear.Rows.Count.ToString()}.";
                ws.Cells[CurRow, 1] = tDR["tgroup"].ToString();
                ws.Cells[CurRow, 2] = tDR["tickdesc"].ToString();
                ws.Cells[CurRow, 3] = tDR["persdesc"].ToString();
                ws.Cells[CurRow, 4] = tDR["dtickcount"].ToString();
                ws.Cells[CurRow++, 5] = tDR["ytickcount"].ToString();
            }
            string CopyName = $"SkiVisitsSummaryReport_{DateTime.Now.ToString("yyyy_MM_dd hh_mm")}";
            string FullPathName = $"C:\\temp\\{CopyName}.xlsx";
            lblStatus.Text = $"Saving a copy of {CopyName}.";
            //if (System.IO.File.Exists(FullPathName))
            //{
            //    System.IO.File.Delete(FullPathName);
            //}
            //wb.SaveAs(FullPathName);
            lblStatus.Text = "Printing Ski Visits Summary Report.";
            excel.Visible = true;
            ws.PrintOutEx(Type.Missing, Type.Missing, 1, true);
            excel.Visible = false;
            wb.Close(false);
            lblStatus.Text = StatusText;
            Cursor = Cursors.Default;
            SkierVisitsAutoRefresh.Checked = AutoRefresh;
        }

        private void BtnPaulsReport_Click(object sender, EventArgs e)
        {
            bool AutoRefresh = SkierVisitsAutoRefresh.Checked;
            timerSkierVisitsRefresh.Enabled = false;
            Cursor = Cursors.WaitCursor;
            string StatusText = lblStatus.Text;
            lblStatus.Text = "Loading spreadsheet for Paul's Report.";
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            string t = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string r = System.AppDomain.CurrentDomain.BaseDirectory;
            Workbook wb = excel.Workbooks.Open(r + @"\reports\PaulsReport.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.LeftHeader = $"Alta Summary Report\r\n{mcywstart.ToShortDateString()} - {mcyday.ToShortDateString()}";
            lblStatus.Text = "Loading data for Paul's Report.";
            System.Data.DataTable tDT = CF.LoadTable(DW.dwConn, $"SELECT descrip, cweek, pweek, weekpcnt, cytd, pytd, ytdpcnt, rtype FROM {DW.ActiveDatabase}.altadata WHERE rtype <> '' ORDER BY sorder", "altadata");
            Int32 CurRow = 3;
            if (tDT != null)
            {
                foreach (DataRow tDR in tDT.Rows)
                {
                    //if (tDR["descrip"].ToString().StartsWith(" "))
                    //{
                    //    var ulRange = excel.Range[excel.Cells[CurRow - 1, 1], excel.Cells[CurRow - 1, 7]];
                    //    Borders border = ulRange.Borders;
                    //    border[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                    //    ulRange = excel.Range[excel.Cells[CurRow, 1], excel.Cells[CurRow, 7]];
                    //    ulRange.Font.Bold = true;
                    //}
                    string RType = tDR["rtype"].ToString();
                    string CurFormat = (RType == "V" ? "$#0" : "");
                    lblStatus.Text = $"Setting data row {(CurRow - 2).ToString()} of {tDT.Rows.Count.ToString()}.";
                    ws.Cells[CurRow, 1] = tDR["descrip"].ToString();
                    ws.Cells[CurRow, 2] = tDR.Field<int>("cweek").ToString(CurFormat);
                    ws.Cells[CurRow, 3] = tDR.Field<int>("pweek").ToString(CurFormat);
                    ws.Cells[CurRow, 4] = (tDR.Field<decimal>("weekpcnt") / 100).ToString("0#.00%");
                    ws.Cells[CurRow, 5] = tDR.Field<int>("cytd").ToString(CurFormat);
                    ws.Cells[CurRow, 6] = tDR.Field<int>("pytd").ToString(CurFormat);
                    ws.Cells[CurRow++, 7] = (tDR.Field<decimal>("ytdpcnt") / 100).ToString("0#.00%");
                    //ws.Cells[CurRow++, 8] = tDR["comments"].ToString();
                }
                string CopyName = $"PaulsReport_{DateTime.Now.ToString("yyyy_MM_dd hh_mm")}";
                string FullPathName = $"C:\\temp\\{CopyName}.xlsx";
                lblStatus.Text = $"Saving a copy of {CopyName}.";
                if (System.IO.File.Exists(FullPathName))
                {
                    System.IO.File.Delete(FullPathName);
                }
                wb.SaveAs(FullPathName);
                lblStatus.Text = "Printing Paul's Report.";
                excel.Visible = true;
                ws.PrintOutEx(Type.Missing, Type.Missing, 1, true);
                excel.Visible = false;
                wb.Close(false);
                lblStatus.Text = StatusText;
                Cursor = Cursors.Default;
                timerSkierVisitsRefresh.Enabled = AutoRefresh;
            }
        }

        private void BtnDayTot_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            bool AutoRefresh = SkierVisitsAutoRefresh.Checked;
            string oldStatus = lblStatus.Text;
            lblStatus.Text = "Running DayTot...";


            lblStatus.Text = oldStatus;
            SkierVisitsAutoRefresh.Checked = AutoRefresh;
            Cursor = Cursors.Default;
        }

        private void BtnDayDown_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            dateTimePicker1.Value = dateTimePicker1.Value.AddDays(-1);
            Cursor = Cursors.Default;
        }

        private void BtnDayUp_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            dateTimePicker1.Value = dateTimePicker1.Value.AddDays(1);
            Cursor = Cursors.Default;
        }

        private void AltaSum_Load(object sender, EventArgs e)
        {
            if (!"MIKEM,ALO,PETE,DARYLB".Contains(Environment.UserName.ToUpper()))
            {
                System.Windows.Forms.Application.Exit();
            }
        }
    }
}
