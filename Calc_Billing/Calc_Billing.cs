using System;
using System.Data;
using System.Windows.Forms;
using asl_SyncLibrary;
using System.Data.OleDb;

namespace Calc_Billing
{
    public partial class Calc_Billing : Form
    {
        private bool inCalc = false;
        const bool POSSumFP = false;
        const string ARTransTable = "ARTrans";  //use test for testing.

        private BUY_ALTA_COM BUY = new BUY_ALTA_COM();
        private CommonFunctions CF = new CommonFunctions();
        private DataWarehouse DW = new DataWarehouse();
        OleDbConnection VFPConn = new OleDbConnection("Provider=VFPOLEDB.1");

        private const string mdatadir = @"g:\fpdata\altaax\";

        //private string merprg;
        private DateTime mxstart = DateTime.Today.AddDays(-1);
        private string sxStart = DateTime.Today.AddDays(-1).ToString(Mirror.AxessDateFormat);
        private DateTime mxend = DateTime.Today.AddDays(-1);
        private DateTime mdate;
        string mtranstype = "";
        string marcustid = string.Empty;
        string marname = string.Empty;
        string mdesc = string.Empty;
        decimal mdebitamt = 0;
        decimal mcreditamt = 0;
        string mauthcode = string.Empty;
        private string mfname = string.Empty;
        private string mlname = string.Empty;
        private string mbilldate;
        string mserialkey = string.Empty;
        int mticktype = 0;
        int mperstype = 0;
        int mtoken = 0;
        //private bool mchgflag = false;
        //private bool mlodgeccbill  = false;
        //private bool msbchg = false;
        private string msbcode = string.Empty;
        //private Int32 srhandlet = 0;
        //private string mupdate;
        //private DateTime mstart;
        //private bool mpreview = true;
        //private string mtestemail = "darylb@alta.com";
        //private bool memailflag = false;
        //private string MSPTOT;
        //private string MCCTOT;
        //private string MTSTOT;
        //private string MLDGTOT;
        //private string martot;
        //private bool memailtest;
        //private string mend;
        //private string mxday1;
        //private string mxday2;
        //private string mskeytext;
        //private string mtempdir = @"c:\acctg\temp\";
        //private string mspsales;
        //private string mtsales;
        //private string mccsales;
        //private string mb2bsales;
        //private string mtotsales;
        //private bool mcalcacc = false;
        private bool mcalcexp = false;
        //private bool mtest = false;
        //private string mcount;
        //private string mctotal;
        //private string mexcount;
        //private string mapcount;
        //private string msalesdetcount;
        //private bool mrefundflag;
        //private string mtickexpbeg;
        //private string mtickexpend;
        //private string mbcode;
        private string mkey;
        //private string mtkey;
        //private string mfampckref;
        private DateTime mcystart;
        //private string mfound_order;
        //private string mfound_serial;
        //private string mmissing_webid;
        //private string msql;
        //private string msql2;
        //private string mwhere;
        private string mmediaid;
        private string mcardacct;
        private string mauthnet_custid;

        //private DataTable ARTrans;
        //private DataTable AXCust;
        //private DataTable PersonType;
        //private DataTable ARCust;
        //private DataTable PromptDef;
        private DataTable SalesData;
        private DataTable PmtData;
        private DataRow SPCards;

        public Calc_Billing()
        {
            InitializeComponent();
            DataRow FormLocation = CF.GetFormLocation("CalcBilling");
            int StopReason = -1;
            if (CF.RowHasData(FormLocation))
            {
                StartPosition = FormStartPosition.Manual;
                Location = new System.Drawing.Point(FormLocation.Field<int>("left"), FormLocation.Field<int>("top"));
                Height = FormLocation.Field<int>("height");
                Width = FormLocation.Field<int>("width");
                StopReason = FormLocation.Field<int>("stopReason");
            }
            CF.SaveFormData("CalcBilling", Location.Y, Location.X, Height, Width, DateTime.Now, null, StopReason);
        }

        private void BtnExit_Click(object sender, EventArgs e) => Application.Exit();

        private void DtpFromDate_ValueChanged(object sender, EventArgs e)
        {
            if ((dtpToDate.Value < dtpFromDate.Value) && !inCalc)
            {
                inCalc = true;
                dtpToDate.Value = dtpFromDate.Value;
                inCalc = false;
            }
        }

        private void DtpToDate_ValueChanged(object sender, EventArgs e)
        {
            if ((dtpToDate.Value > dtpFromDate.Value) && !inCalc)
            {
                inCalc = true;
                dtpFromDate.Value = dtpToDate.Value;
                inCalc = false;
            }
        }

        private void Calc_Billing_Shown(object sender, EventArgs e)
        {
            mcystart = new DateTime(2018, 4, 15); // Convert.ToDateTime(CF.GetSQLField(DW.dwConn, $"SELECT start_date FROM {DW.ActiveDatabase}.season_dates ORDER BY season DESC LIMIT 1"));
            dtpFromDate.Value = DateTime.Today.AddDays(-1);
            dtpToDate.Value = dtpFromDate.Value;
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            mxstart = dtpFromDate.Value;
            //mxend = dtpToDate.Value;
            for (DateTime a = mxstart; a <= dtpToDate.Value; a = a.AddDays(1))
            {
                sxStart = a.ToString(Mirror.AxessDateFormat);
                Main();    
            }
        }

        private void UpdatePossum(DateTime SaleDate, int POS, string Office, decimal ARAmt, decimal TotRec, decimal AXAR, decimal AXTot)
        {
            string sSaleDate = SaleDate.ToString(Mirror.AxessDateFormat);
            if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.possum", $"POS=33 AND saledate='{sSaleDate}'"))
            {
                CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.possum SET office='EXP', aramt={ARAmt.ToString()}, totrec={TotRec.ToString()}, axar={AXAR.ToString()}, axtot={AXTot.ToString()} WHERE POS=33 AND saledate='{sxStart}'");
            }
            else
            {
                CF.ExecuteSQL(DW.dwConn, $"INSERT INTO {DW.ActiveDatabase}.possum (saledate, pos, office, aramt, totrec, axar, axtot) VALUES ('{sSaleDate}', 33, 'EXP', {ARAmt.ToString()}, {TotRec.ToString()}, {AXAR.ToString()}, {AXTot.ToString()})");
            }
            string FPDate = CF.SetFoxProDate(mxstart);
            string FPNullDate = "{^0000-00-00}";
            VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}possum.dbf";
            if (CF.RowCount(VFPConn, $"possum", $"POS=33 AND saledate='{FPDate}'") != 0)
            {
                VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}possum.dbf";
                CF.ExecuteSQL(VFPConn, $"UPDATE possum SET office='EXP', aramt={ARAmt.ToString()}, totrec={TotRec.ToString()}, axar={AXAR.ToString()}, axtot={AXTot.ToString()} WHERE POS=33 AND saledate={FPDate}");
            }
            else
            {
                VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}possum.dbf";
                string Q = $"INSERT INTO possum (saledate, pos, office, cash, cvisa, cmast, cdisc, camex, cgiftuse, cgiftadd, cardcheck, cardtot, gcertqty, gcertamt, vchrqty, vchramt, snchk, arqty, aramt, arguest, acredit, webamt, totrec, axcash, axcard, axvchr, axsnchk, axcredit, axar, axargst, axweb, axgift, axtot, webship, weborders, webreloads, webtickr, webtickn, webskisch, webgift, webrefund, ccbillamt, webfampck, webtot, axorders, axreloads, axwebdisc, trretail, trwillcall, vcwillcall, axsales, notes, cashier1, cashier2, cashier3, cissued, creturn, cused, axused, balflag, saledetot, emflag, ccsettled, crefno, vrefno, arefno, salestot, pmttot, artrantot, poskey, ccbatchnbr) VALUES ({FPDate}, 33, 'EXP', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, {ARAmt.ToString()}, 0, 0, 0, {TotRec.ToString()}, 0, 0, 0, 0, 0, {AXAR.ToString()}, 0, 0, 0, {AXTot.ToString()}, 0, 0, 0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, '','','','',0,0,0,0,0,0, 0,{FPNullDate},'','','',0,0,0,'',0)";
                CF.ExecuteSQL(VFPConn, Q);
            }
        }

        private void Main()
        {
            UpdateExpPrices();
            if (chkAddCharges.Checked)
            {
                tsStatusLabel1.Text = "Loading POS Charges and Pass Charges...";
                //Open_Files();
                PmtData = CF.LoadTable(DW.dwConn, $"SELECT saledate, transkey, nkassanr, arcustid, arname, tfactor, pmtamt, ptype, szprompt1, szprompt2, szprompt3 FROM {DW.ActiveDatabase}.pmtdata WHERE saledate = '{sxStart}' AND ptype IN ('AR','AG','WE') AND (NOT nkassanr IN (20, 33)) AND pmtamt <> 0", "pmtdata");
                //Load_POS_ARs();
                Load_POS_ARs();
                //Build_Lodge_Summary();
                DataTable LodgePmtData = CF.LoadTable(DW.dwConn, $"SELECT nkassanr, transkey, saledate, SUM(pmtamt) AS TotalAmt FROM {DW.ActiveDatabase}.pmtdata WHERE saledate = '{sxStart}' AND nkassanr IN (50,51,52,53,54) GROUP BY nkassanr", "PmtData");
                foreach (DataRow LodgePmtRow in LodgePmtData.Rows)
                {
                    decimal martot = LodgePmtRow.Field<decimal>("TotalAmt");
                    DataRow SaleAmt = CF.LoadDataRow(DW.dwConn, $"SELECT SUM(tariff) AS AXTotal FROM {DW.ActiveDatabase}.salesdata WHERE transkey = '{LodgePmtRow["transkey"].ToString()}'");
                    decimal maxtot = SaleAmt.Field<decimal>("AXTotal");
                    UpdatePossum(LodgePmtRow.Field<DateTime>("saledate"), LodgePmtRow.Field<int>("nkassanr"), "LDG", martot, martot, maxtot, maxtot);
                }
                //ARTrans_TokenUpdate();        //update zero tokenkeys
                CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.{ARTransTable} AS A INNER JOIN {DW.ActiveDatabase}.spcards AS B ON A.serialkey = B.serialkey SET A.tokenkey = B.tokenkey, A.authnet_custid = B.authnet_custid, A.arcustid = '11874', A.arname = 'ASL - PASS CHARGES' WHERE (A.authcode = '') AND ((A.debitamt - A.creditamt) > 0) AND (B.tokenkey > 0)"); 
            }
            tsStatusLabel1.Text = "Calculating Card Charges...";
            mcalcexp = true;
            if (chkUseBilling.Checked)
            {
                Calc_Use_Charges();
            }

            tsStatusLabel1.Text = "Updating ARTrans...";
            Application.DoEvents();
            if (mcalcexp)
            {
                Update_ARTrans();
                Update_ARTrans_TokenKey();
            }
            if (chkLoadARTrans.Checked)
            {
                //Update_Sales_Files();
                //*update salesdata
                //* update pmtdata
                //* update possum

                //*** select from artrans records ***
                string msqlcmd = $"SELECT * FROM {DW.ActiveDatabase}.{ARTransTable} WHERE transdate = '{sxStart}' AND transtype = 'C' AND posnbr = 33 ORDER BY serialkey";
                DataTable artransxt = CF.LoadTable(DW.dwConn, msqlcmd, "artransxt");

                CF.ExecuteSQL(DW.dwConn, $"DELETE FROM {DW.ActiveDatabase}.pmtdata WHERE saledate = '{sxStart}' AND nkassanr = 33");
                CF.ExecuteSQL(DW.dwConn, $"DELETE FROM {DW.ActiveDatabase}.salesdata WHERE saledate = '{sxStart}' AND nkassanr = 33");
                decimal mtot = 0;
                decimal mamt = 0;
                foreach (DataRow ARTransXTRow in artransxt.Rows)
                {
                    statusStrip1.Items[0].Text = $"Updating ARTrans... ({artransxt.Rows.IndexOf(ARTransXTRow).ToString()} of {artransxt.Rows.Count.ToString()})";
                    mamt = ARTransXTRow.Field<decimal>("debitamt") - ARTransXTRow.Field<decimal>("creditamt");
                    mtot += mamt;
                    int mtfactor = (mamt <= 0 ? mtfactor = -1 : 1);
                    string museticktype = (ARTransXTRow.Field<int?>("useticktype").HasValue ? museticktype = ARTransXTRow["useticktype"].ToString() : "0");
                    string musetickdesc = ARTransXTRow["usetickdesc"].ToString();
                    string mmedia = (!string.IsNullOrEmpty(ARTransXTRow.Field<string>("mediaid")) ? mmedia = ARTransXTRow.Field<string>("mediaid") : string.Empty);
                    string MFNAME = CF.EscapeChar(ARTransXTRow["firstname"].ToString());
                    string MLNAME = CF.EscapeChar(ARTransXTRow["lastname"].ToString());
                    //* **insert pmtdata record ***
                    msqlcmd = $"INSERT INTO {DW.ActiveDatabase}.pmtdata (transkey,transpkey,pmtkey,saledate,arcustid,nprojnr,nkassanr,office,ntransnr,nlfdnr,ntransstat,tfactor,pmtamt,ptype,arname,tgroup,fname,lname,pstatus) " +
                        $"VALUES ('{ARTransXTRow["transkey"].ToString()}', '{ARTransXTRow["transkey"].ToString()}', '106-1', '{ARTransXTRow.Field<DateTime>("transdate").ToString(Mirror.AxessDateFormat)}', " +
                        $"'{ARTransXTRow["arcustid"].ToString()}', 705, 33, 'POE', {ARTransXTRow.Field<int>("ntransnr").ToString()}, 0, 3, {mtfactor.ToString()}, " +
                        $"{mamt.ToString()}, 'AR', '{ARTransXTRow["arname"].ToString()}', '{ARTransXTRow["tgroup"].ToString().Trim()}', '{MFNAME}', '{MLNAME}', 'Calculated Use Charge')";
                    CF.ExecuteSQL(DW.dwConn, msqlcmd);
                    //*** insert salesdata record ***
                    msqlcmd = $"INSERT INTO {DW.ActiveDatabase}.salesdata (transkey,serialkey,saledate,ntranstype,ttype,nkassanr,ntransnr,nblocknr,tgroup,tfactor,tariff,fname,lname,persdesc, nperstype, qty,mediaid,nticktype, tickdesc) " +
                        $" VALUES ('{ARTransXTRow["transkey"].ToString()}', '{ARTransXTRow["serialkey"].ToString()}', '{ARTransXTRow.Field<DateTime>("transdate").ToString(Mirror.AxessDateFormat)}', " +
                        $" 0, 'S', 33, {ARTransXTRow.Field<int>("ntransnr").ToString()}, 0, '{ARTransXTRow["tgroup"].ToString().Trim()}', {mtfactor.ToString()}, {mamt.ToString("#0.00")}, '{MFNAME}', " +
                        $"'{MLNAME}', '{ARTransXTRow["persdesc"].ToString()}', {ARTransXTRow.Field<int>("nperstype").ToString()}, 1, '{mmedia}', '{museticktype}', '{musetickdesc}')";
                    CF.ExecuteSQL(DW.dwConn, msqlcmd);
                }
                //* MAKE POSSUM RECORD
                tsStatusLabel1.Text = "Updating POSSUM...";
                Application.DoEvents();
                DataRow SaleAmt = CF.LoadDataRow(DW.dwConn, $"SELECT SUM(tariff) AS AXTotal FROM {DW.ActiveDatabase}.salesdata WHERE saledate = '{sxStart}' AND nkassanr = 33");
                decimal? maxtot = SaleAmt.Field<decimal?>("AXTotal");
                if (mtot != 0)
                {
                    UpdatePossum(mxstart, 33, "EXP", mtot, mtot, maxtot.Value, maxtot.Value);
                }

            }
            if (chkDisplayReport.Checked)
            {
                DataTable ARTrans = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.{ARTransTable} WHERE(arcustid IN ('11874', '') OR (arcustid IS null)) AND (authcode = '' OR authcode IS null) AND ((debitamt + creditamt) > 0) AND (transdate > '{mcystart.ToString(Mirror.AxessDateFormat)}') ORDER BY tokenkey, transdate DESC", "ARTrans");
                //build and print "UNBILLED RESORT CHARGES"   (rcunbilled)
            }
            tsStatusLabel1.Text = "Finished!";
        }

        private void Load_POS_ARs()
        {
            DateTime mtransdate = CF.defaultDOB;
            marname = string.Empty;
            mdesc = string.Empty;
            mdebitamt = 0;
            mcreditamt = 0;
            mauthcode = string.Empty;
            mtoken = 0;
            mserialkey = string.Empty;
            mticktype = 0;
            mperstype = 0;
            //*** load ar charges data***
            foreach (DataRow PmtRowX in PmtData.Rows)
            {
                // Load Salesdata matching pmtrow on transkey
                DataRow SalesRow = CF.LoadDataRow(DW.dwConn, $" SELECT fname, lname, tickdesc, persdesc, serialkey, transkey, nperstype, nticktype, nkassanr, tgroup FROM {DW.ActiveDatabase}.salesdata WHERE transkey = '{PmtRowX["transkey"].ToString()}' LIMIT 1");

                //****************
                //*BUILD MEMORY VARIABLES TO INSERT INTO ARTRANS***
                //****************
                mfname = string.Empty;
                mlname = string.Empty;
                mcardacct = string.Empty;
                mtranstype = string.Empty;
                mtransdate = CF.defaultDOB;
                mdebitamt = 0;
                mcreditamt = 0;
                mauthcode = string.Empty;
                mbilldate = PmtRowX.Field<DateTime>("saledate").ToString(Mirror.AxessDateFormat);
                mdesc = string.Empty;
                mtoken = 0;
                mauthnet_custid = "0";
                mmediaid = string.Empty;
                marname = string.Empty;
                mserialkey = string.Empty;
                mticktype = 0;
                mperstype = 0;
                marcustid = string.Empty;
                mkey = PmtRowX["transkey"].ToString();
                if (!string.IsNullOrEmpty(PmtRowX["arcustid"].ToString()))
                {
                    marcustid = PmtRowX["arcustid"].ToString();
                    marname = PmtRowX["arname"].ToString();
                }
                if (PmtRowX.Field<int>("tfactor") == -1)
                {
                    mcreditamt = (PmtRowX.Field<decimal>("pmtamt") * -1);
                }
                else
                {
                    mdebitamt = PmtRowX.Field<decimal>("pmtamt");
                }
                mbilldate = (PmtRowX.Field<DateTime>("saledate")).ToString(Mirror.AxessDateFormat);
                bool mcont = false;



                //data from SalesData or elsewhere
                mdesc = SalesRow["tickdesc"].ToString();
                if (SalesRow.Field<string>("fname").Length > 0)
                {
                    mfname = SalesRow.Field<string>("fname");
                }
                if (SalesRow.Field<string>("lname").Length > 0)
                {
                    mlname = SalesRow.Field<string>("lname");
                }
                if ((SalesRow.Field<string>("serialkey").Length > 0) && (mlname == string.Empty))
                {
                    SPCards = CF.LoadDataRow(DW.dwConn, $"SELECT firstname, lastname FROM {DW.ActiveDatabase}.spcards WHERE serialkey = '{SalesRow.Field<string>("serialkey")}'");
                    if (SPCards != null)
                    {
                        mfname = SPCards["firstname"].ToString();
                        mlname = SPCards["lastname"].ToString();
                    }
                }

                if (PmtRowX["ptype"].ToString() == "WE") //web sale
                {
                    mkey = SalesRow["transkey"].ToString();
                    mtranstype = "I";
                    mcont = true;
                    mserialkey = SalesRow["serialkey"].ToString();
                    marcustid = "1206";
                    marname = "ESTORE";
                    mauthcode = marcustid;
                    mticktype = 0;
                    if (SalesRow.Field<int?>("nticktype").HasValue)
                    {
                        mticktype = SalesRow.Field<int>("nticktype");
                    }
                    if (SalesRow.Field<int?>("nperstype").HasValue)
                    {
                        mperstype = SalesRow.Field<int>("nperstype");
                    }
                }
                else
                {
                    if ((PmtRowX["ptype"].ToString() == "AG") && (PmtRowX.Field<decimal>("pmtamt") != 0))
                    {
                        switch (PmtRowX["szprompt1"].ToString())
                        {
                            case "LC":

                                mkey = SalesRow["transkey"].ToString();
                                mtranstype = "Z";
                                mcont = true;
                                mfname = PmtRowX["szprompt2"].ToString().Substring(0, (PmtRowX["szprompt2"].ToString() + " ").ToString().IndexOf(' '));
                                mlname = PmtRowX["szprompt2"].ToString().Substring(mfname.Length);
                                if (mlname.Length > 20)
                                {
                                    mlname = mlname.Substring(0, 20);
                                }
                                mlname += "   " + PmtRowX["szprompt3"].ToString();
                                if (mlname.Length > 25)
                                {
                                    mlname = mlname.Substring(0, 25);
                                }
                                mdesc = "Lodge Room Charge  " + SalesRow["tickdesc"].ToString();
                                mauthcode = marcustid;
                                break;

                            case "PC":
                            case "FP":
                                mkey = SalesRow["transkey"].ToString();
                                mtranstype = "Z";
                                mcont = true;
                                mmediaid = PmtRowX["szprompt3"].ToString();
                                mfname = PmtRowX["szprompt2"].ToString().Substring(0, (PmtRowX["szprompt2"].ToString() + " ").ToString().IndexOf(' '));
                                mlname = PmtRowX["szprompt2"].ToString().Substring(mfname.Length);
                                if (mlname.Length > 20)
                                {
                                    mlname = mlname.Substring(0, 20);
                                }
                                if (mlname.Length > 20)
                                {
                                    mlname = mlname.Substring(0, 20);
                                }
                                mauthcode = string.Empty;
                                string mxkey = PmtRowX["szprompt3"].ToString();
                                SPCards = CF.LoadDataRow(DW.dwConn, $"SELECT firstname, lastname, tokenkey, authnet_custid,empid, serialkey, cardacct FROM {DW.ActiveDatabase}.spcards WHERE mediaid = '{mxkey}'");
                                if (SPCards != null)
                                {
                                    if (SPCards["cardacct"].ToString().Trim() != string.Empty)
                                    {
                                        mfname = CF.EscapeChar(SPCards["firstname"].ToString());
                                        mlname = CF.EscapeChar(SPCards["lastname"].ToString());
                                        mtoken = SPCards.Field<Int32>("tokenkey");
                                        mauthnet_custid = SPCards["authnet_custid"].ToString();
                                        if (mtoken != 0)
                                        {
                                            mbilldate = "00/00/0000";
                                        }
                                        if (SPCards["empid"].ToString() != string.Empty && SPCards.Field<int>("tokenkey") == 0)
                                        {
                                            mauthcode = SPCards["empid"].ToString();
                                            marcustid = "11521";
                                            marname = "ASL - EMPLOYEE CHARGES";
                                        }
                                    }
                                }
                                mserialkey = SalesRow["serialkey"].ToString();
                                mcardacct = SPCards["cardacct"].ToString();
                                if (PmtRowX["szprompt1"].ToString() == "FP")
                                {
                                    mdesc = "Forgot Pass Charge";
                                }
                                break;
                            case "RS":
                                mkey = SalesRow["transkey"].ToString();
                                mtranstype = "T";
                                mcont = true;
                                mfname = PmtRowX["szprompt2"].ToString().Substring(0, (PmtRowX["szprompt2"].ToString() + " ").ToString().IndexOf(' '));
                                mlname = PmtRowX["szprompt2"].ToString().Substring(mfname.Length);
                                if (mlname.Length > 20)
                                {
                                    mlname = mlname.Substring(0, 20);
                                }
                                if (mlname.Length > 20)
                                {
                                    mlname = mlname.Substring(0, 20);
                                }
                                mdesc = "Tickets/Ski School Purchase " + mkey;
                                mauthcode = marcustid;
                                break;
                        }
                    }
                    else
                    {
                        if (SalesRow.Field<int?>("nticktype") == 4001)  //AR Payment
                        {
                            mkey = SalesRow["transkey"].ToString();
                            mtranstype = "X";
                            mcont = true;
                            mauthcode = marcustid;
                            if (PmtRowX.Field<int>("tfactor") == -1)
                            {
                                mcreditamt = (PmtRowX.Field<int>("pmtamt") * -1);
                            }
                            else
                            {
                                mdebitamt = PmtRowX.Field<int>("pmtamt");
                            }
                        }
                        else
                        {
                            if (PmtRowX.Field<decimal>("pmtamt") != 0)
                            {
                                mcont = true;
                                mauthcode = marcustid;
                                mserialkey = SalesRow["serialkey"].ToString();
                                mkey = SalesRow["transkey"].ToString();
                                mtranstype = (SalesRow.Field<int>("nkassanr") >= 50 && SalesRow.Field<int>("nkassanr") <= 54) ? "L" : "T";
                                mticktype = SalesRow.Field<int?>("nticktype").HasValue ? SalesRow.Field<int>("nticktype") : 0;
                                mperstype = SalesRow.Field<int?>("nperstype").HasValue ? SalesRow.Field<int>("nperstype") : 0;
                            }
                        }
                    }
                }
                mdesc = CF.EscapeChar(mdesc);
                mfname = CF.EscapeChar(mfname.Replace(",", " "));
                mlname = CF.EscapeChar(mlname.Replace(",", " "));
                //find and fix tokens
                SPCards = CF.LoadDataRow(DW.dwConn, $"SELECT tokenkey, authnet_custid, mediaid, cardacct FROM {DW.ActiveDatabase}.spcards WHERE serialkey = '{mserialkey}' ORDER BY dtupdate DESC");
                if (SPCards != null)
                {
                    if (SPCards["tokenkey"].ToString().Trim() != string.Empty)
                    {
                        mtoken = SPCards.Field<int>("tokenkey");
                        mauthnet_custid = SPCards["authnet_custid"].ToString();
                        if (SPCards["cardacct"].ToString() == string.Empty)
                        {
                            mcardacct = "NULL";
                        }
                        else
                        {
                            mcardacct = $"'{SPCards["cardacct"].ToString()}'";
                        }
                        if (SPCards["mediaid"].ToString() == string.Empty)
                        {
                            mmediaid = "NULL";
                        }
                        else
                        {
                            mmediaid = $"'{SPCards["mediaid"].ToString()}'";
                        }
                    }
                }

                if (mcont)
                {
                    DataRow ARTransRow = CF.LoadDataRow(DW.dwConn, $"SELECT * from {DW.ActiveDatabase}.{ARTransTable} WHERE transtype <> 'C' AND transkey='{mkey}'");
                    if (ARTransRow["arcustid"].ToString().Trim() != string.Empty)
                    {
                        if ((marcustid != ARTransRow["arcustid"].ToString()) || (mtranstype != ARTransRow["transtype"].ToString()) || (mdebitamt != ARTransRow.Field<decimal>("debitamt")) || (mcreditamt != ARTransRow.Field<decimal>("creditamt")) ||  (ARTransRow["authcode"].ToString() == string.Empty  && mtoken != ARTransRow.Field<int>("tokenkey")))
                        {
                            ARTrans_Update(ARTransRow, PmtRowX, SalesRow);
                        }
                    }
                    else
                    {
                        mdate = PmtRowX.Field<DateTime>("saledate");
                        ARTrans_Insert(ARTransRow, PmtRowX, SalesRow);
                    }
                }
            }



            //** add ar payments **

            DataTable SalesData = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.salesdata WHERE nticktype = 4001 AND saledate = '{sxStart}' AND NOT (nkassanr IN (20, 33))", "salesdata");
            foreach (DataRow SalesRow in SalesData.Rows)
            {
                if (SalesRow.Field<decimal>("tariff") != 0)
                {
                    mkey = SalesRow["transkey"].ToString();
                    DataRow PmtRow = CF.LoadDataRow(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.pmtdata WHERE transkey = '{mkey}'");
                    mtranstype = "X";
                    mdebitamt = 0;
                    mcreditamt = 0;
                    mbilldate = SalesRow.Field<DateTime>("saledate").ToString(Mirror.AxessDateFormat);
                    mtransdate = SalesRow.Field<DateTime>("saledate");
                    mdesc = SalesRow["tickdesc"].ToString();
                    mcardacct = string.Empty;
                    mtoken = 0;
                    mauthnet_custid = "0";
                    mmediaid = string.Empty;
                    mserialkey = SalesRow["serialkey"].ToString();
                    mticktype = SalesRow.Field<int>("nticktype");
                    mperstype = SalesRow.Field<int>("nperstype");
                    marcustid = string.Empty;
                    marname = string.Empty;
                    if (PmtRow.Field<string>("arcustid").Trim().Length != 0)
                    {
                        marcustid = PmtRow["arcustid"].ToString();
                        marname = PmtRow["arname"].ToString();
                    }
                    mdesc = SalesRow["tickdesc"].ToString();
                    mauthcode = marcustid;
                    mfname = CF.EscapeChar(SalesRow["fname"].ToString());
                    mlname = CF.EscapeChar(SalesRow["lname"].ToString());
                    if (SalesRow.Field<int>("tfactor") == 1)
                    {
                        mcreditamt = SalesRow.Field<decimal>("tariff");
                    }
                    else
                    {
                        mdebitamt = SalesRow.Field<decimal>("tariff");

                    }

                    DataRow ARTransRow = CF.LoadDataRow(DW.dwConn, $"SELECT arcustid, transtype, debitamt, creditamt, authcode, tokenkey from {DW.ActiveDatabase}.{ARTransTable} WHERE transtype <> 'C' AND transkey='{mkey}'");
                    if (ARTransRow["arcustid"].ToString() != string.Empty)
                    {
                        if ((marcustid != ARTransRow["arcustid"].ToString()) || (mtranstype != ARTransRow["transtype"].ToString()) || (ARTransRow["authcode"].ToString() == string.Empty && mtoken != ARTransRow.Field<int>("tokenkey")))
                        {
                            ARTrans_Update(ARTransRow, PmtRow, SalesRow);
                        }
                    }
                    else
                    {
                        mdate = PmtRow.Field<DateTime>("saledate");
                        ARTrans_Insert(ARTransRow, PmtRow, SalesRow);
                    }
                }
            }
        }

        private void Update_SPCards_Flags()
        {
            DW.UpdateSPCardStatus();
            DW.UpdateSPCardsUses();
            DW.UpdateSalesDataUses();
            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards SET amflag='N', begflag='N', altaflag='N', sbflag='N', useflag='N', lateflag='N', at3flag='N', aramt=0, sbamt=0, usedate=null, uticktype=0, usedesc='' WHERE tgroup IN ('EX','CC','MC')");
            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards AS S INNER JOIN {DW.ActiveDatabase}.skivisits AS V ON V.serialkey=S.serialkey SET S.begflag=V.BEGFLAG, S.sbflag=V.SBFLAG, S.altaflag=V.ALTAFLAG, S.amflag=V.AMFLAG, S.lateflag=V.LATEFLAG, S.at3flag=V.AT3FLAG, S.usedate=V.READDATE WHERE ((S.tgroup='EX' AND S.cardstatus='A') OR (S.tgroup = 'CC' AND S.cardstatus IN ('A','P','X'))) AND V.readdate='{sxStart}'");
            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards AS S INNER JOIN {DW.ActiveDatabase}.skivisits AS V ON V.serialkey=S.serialkey SET S.begflag=V.BEGFLAG, S.sbflag=V.SBFLAG, S.altaflag=V.ALTAFLAG, S.amflag=V.AMFLAG, S.lateflag=V.LATEFLAG, S.at3flag=V.AT3FLAG, S.usedate=V.READDATE WHERE S.tgroup='MC' AND S.nticktype=2025 AND S.cardstatus='A' AND S.mcpnbr<>'' AND V.readdate='{sxStart}'");
            CF.ExecuteSQL(DW.dwConn, $"update {DW.ActiveDatabase}.spcards SET useflag='Y' WHERE altaflag='Y' OR sbflag='Y'");
        }

        private void Update_ARTrans()
        {
            //***GET NEXT TRANSACTION NUMBER***
            Int32 mnextnbr = Convert.ToInt32(CF.GetSQLField(DW.dwConn, $"SELECT ntransnr FROM {DW.ActiveDatabase}.pmtdata WHERE nkassanr=33 ORDER BY ntransnr DESC LIMIT 1")) + 1;
            //reset useflag to "N" if already billed
            string Query = $"UPDATE {DW.ActiveDatabase}.spcards AS S INNER JOIN {DW.ActiveDatabase}.{ARTransTable} AS T ON T.serialkey=S.serialkey SET S.useflag='N' WHERE S.useflag='Y' AND T.posnbr=33 AND T.transdate='{sxStart}' AND T.debitamt > 0 AND T.tokenkey > 0 AND authcode <> ''";
            CF.ExecuteSQL(DW.dwConn, Query);
            //*** IF CHARGES HAVE NOT BEEN BILLED - ***DELETE CHARGES***
            string msqlcmd = $"DELETE FROM {DW.ActiveDatabase}.{ARTransTable} WHERE (authcode = '') AND (TRIM(tgroup) IN ('MC','EX','CC')) AND (transdate = '{sxStart}') AND (transtype = 'C') AND (debitamt > 0)";
            CF.ExecuteSQL(DW.dwConn, msqlcmd);
            //* **insert artrans records ***
            using (DataTable SPCards1 = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.spcards WHERE useflag = 'Y'", "SPCards1"))
            { 
                string moffice = string.Empty;
                int mkassanr = 0;
                decimal martot = 0;
                foreach (DataRow SPCard in SPCards1.Rows)
                {
                    if (SPCard["serialkey"].ToString().Trim() == "51-25019-1")
                        marname = string.Empty;
                    marname = string.Empty;
                    martot = martot + SPCard.Field<decimal>("aramt");
                    mbilldate = "null";
                    mkassanr = 33;
                    mtoken = SPCard.Field<int>("tokenkey");
                    if (SPCard.Field<decimal>("aramt") != 0)
                    {
                        moffice = "POE";
                        string mauthcode = string.Empty;
                        if (SPCard.Field<int>("nkassanr") >= 50 && SPCard.Field<int>("nkassanr") <= 54 && mtoken == 0) // BETWEEN(spcards.nkassanr,50,54) AND mtoken = 0
                        {
                            mauthcode = SPCard["arcustid"].ToString();
                        }
                        string mtranskey = $"33-{mnextnbr.ToString()}";
                        if (SPCard["cardacct"].ToString() == string.Empty)
                        {
                            mcardacct = "NULL";
                        }
                        else
                        {
                            mcardacct = $"'{SPCard["cardacct"].ToString()}'";
                        }
                        if (SPCard["mediaid"].ToString() == string.Empty)
                        {
                            mmediaid = "NULL";
                        }
                        else
                        {
                            mmediaid = $"'{SPCard["mediaid"].ToString()}'";
                        }
                        if (SPCard["arcustid"].ToString() == string.Empty)
                        {
                            marcustid = "11874";
                        }
                        else
                        {
                            marcustid = SPCard["arcustid"].ToString();
                        }
                        string mlastname = CF.EscapeChar(SPCard["lastname"].ToString());
                        string mfirstname = CF.EscapeChar(SPCard["firstname"].ToString());
                        //* **insert artrans record
                        mbilldate = mbilldate.ToUpper();
                        if (mbilldate != "NULL")
                            mbilldate = $"'{mbilldate}'";
                        msqlcmd = $"INSERT {DW.ActiveDatabase}.{ARTransTable} (transdate,posnbr,transkey,transtype,arcustid,arname,transdesc,debitamt,creditamt," +
                            $"authcode,dtupdate,billdate,serialkey,cardacct,mediaid,firstname,lastname,tokenkey,tgroup,nticktype,tickdesc,nperstype,persdesc,useticktype, " +
                            $"usetickdesc, ntransnr, perskey, sbamt) " +
                            $"Values ('{sxStart}',{mkassanr.ToString()},'{mtranskey}','C','{marcustid}','{SPCard["arname"].ToString()}','{SPCard["usedesc"].ToString()}'," +
                            $"{SPCard.Field<decimal>("aramt").ToString("#,##0.00")}, 0,'{mauthcode}','{sxStart}', {mbilldate},'{SPCard["serialkey"].ToString()}'," +
                            $"{mcardacct},{mmediaid},'{mfirstname}','{mlastname}',{mtoken.ToString()},'{SPCard["tgroup"].ToString().Trim()}',{SPCard["nticktype"].ToString()}," +
                            $"'{SPCard["tickdesc"].ToString()}',{SPCard["nperstype"].ToString()},'{SPCard["persdesc"].ToString()}',{SPCard["uticktype"].ToString()}," +
                            $"'{SPCard["usedesc"].ToString()}',{mnextnbr.ToString()},'{SPCard["perskey"].ToString()}',{SPCard.Field<decimal>("sbamt").ToString("#,##0.00")})";
                        CF.ExecuteSQL(DW.dwConn, msqlcmd);
                        mtranskey = $"33-{mnextnbr.ToString()}";
                    }
                    mnextnbr = mnextnbr + 1;
                }

            }
        }

        private void Update_ARTrans_TokenKey()
        {
            //*update previous unbilled tokenkeys
            //Single Query Method
            string Query = $"UPDATE {DW.ActiveDatabase}.{ARTransTable} AS A INNER JOIN {DW.ActiveDatabase}.spcards AS B ON B.serialkey = A.serialkey SET B.tokenkey=A.tokenkey WHERE A.transdate<'2018-11-15' AND A.transtype='C' and A.authcode='' AND A.tokenkey<>B.tokenkey;";
            CF.ExecuteSQL(DW.dwConn, Query);
            //Multiple Query Method
            //DataTable artransxt = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.{ARTransTable} WHERE transdate < '{sxStart}' AND transtype = 'C' AND authcode = ''", "artransxt");
            //foreach (DataRow ARTran in artransxt.Rows)
            //{
            //    mkey = ARTran["serialkey"].ToString();
            //    SPCards = CF.LoadDataRow (DW.dwConn, $"SELECT tokeneky FROM {DW.ActiveDatabase} WHERE serialkey='{mkey}' AND tokeney<>{ARTran["tokenkey"].ToString()} AND pmtprofile<999999900");
            //    if (SPCards != null)
            //    {
            //        if (SPCards["tokenkey"].ToString() != string.Empty)
            //        {
            //            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.{ARTransTable} SET tokenkey='{SPCards["tokenkey"].ToString()}' WHERE recid={ARTran["recid"].ToString()}");
            //        }
            //    }
            //}
        }

        private void Open_files()
        {
            PmtData = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.pmtdata where saledate = '{sxStart}' ORDER BY transkey", "pmtdata");
            SalesData = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.salesdata WHERE saledate = '{sxStart}' ORDER BY serialkey", "salesdata");
            DataTable ARCust = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.arcust ORDER BY ARCustID", "ARCust");
            DataTable TicketTypes = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.tickettypes ORDER BY ticktype", "tickettypes");
            DataTable MCPUses = CF.LoadTable(DW.dwConn, $"SELECT mcpnbr, count(mcpnbr) as daysskied from (SELECT skivisits.mcpnbr, readdate from applications.skivisits WHERE skivisits.mcpnbr LIKE 'MCP%' AND Skivisits.readdate <= '{sxStart}' GROUP BY mcpnbr, readdate ORDER BY mcpnbr) as mcpuses group by mcpnbr ORDER BY mcpnbr", "MCPUses");
            DataTable SkiVisits = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.skivisits WHERE readdate='{sxStart}' AND nkassanr < 60 ORDER BY serialkey", "SkiVisits");
            DataTable SPCards1 = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.spcards WHERE tgroup in ('CC','EX', 'MC') ORDER BY serialkey", "SPCards");
        }

        //  * nzutrnr field in Axess
        //  * 1 - Collins
        //  * 2 - Collins Angle
        //  * 3 - Sunnyside
        //  * 4 - Albion
        //  * 5 - Wildcat
        //  * 6 - Supreme
        //  * 7 - Sugarloaf
        //  * 8 - Sugarpass to Alta
        //  * 9 - Sugarpass to Snowbird
        //  * 61-91 - Snowbird

        private void Calc_Use_Charges()
        {
            Update_SPCards_Flags();
            if (mcalcexp)
            {
                //calc_charges  update express and goldcard files with usage
                DataTable SPCharges = CF.LoadTable(DW.dwConn, $"SELECT recid, saledate, serialkey, mcpnbr, cardstatus, tgroup, nticktype, nperstype, freedays, amflag, lateflag, at3flag, begflag, altaflag, sbflag, useflag, usedate FROM {DW.ActiveDatabase}.spcards WHERE tgroup IN ('CC','EX','MC') AND (saledate >= '{mcystart.ToString(Mirror.AxessDateFormat)}' AND expdate >= '{sxStart}' AND useflag='Y') ORDER BY serialkey", "SPCharges");
                if (SPCharges != null)
                {
                    foreach (DataRow SPChargeRow in SPCharges.Rows)
                    {
                        if (SPChargeRow["serialkey"].ToString().Trim() == "51-25019-1")
                            System.Diagnostics.Debug.Print($"{SPCharges.Rows.IndexOf(SPChargeRow).ToString()} of {SPCharges.Rows.Count.ToString()}");
                        System.Diagnostics.Debug.Print($"{SPCharges.Rows.IndexOf(SPChargeRow).ToString()} of {SPCharges.Rows.Count.ToString()}");
                        statusStrip1.Items[0].Text = $"Calculating Card Charges... ({SPCharges.Rows.IndexOf(SPChargeRow).ToString()} of {SPCharges.Rows.Count.ToString()})";
                        string CardStatus = SPChargeRow["cardstatus"].ToString();
                        bool CardActive = (CardStatus == "A");
                        bool CCOverride = SPChargeRow["tgroup"].ToString().Trim() == "CC" && (CardStatus == "X" || CardStatus == "P");
                        if (CardActive || CCOverride)
                        {
                            mkey = (SPChargeRow.Field<int>("nticktype") + 10000).ToString() + (SPChargeRow.Field<int>("nperstype") + 1000);
                            DataRow ExpressType = CF.LoadDataRow(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.expresstype WHERE ticktype = {SPChargeRow["nticktype"].ToString()} AND perstype = {SPChargeRow["nperstype"].ToString()}");
                            if (CF.RowHasData(ExpressType))
                            {
                                int mticktype = 0;
                                bool SBFlag = SPChargeRow["sbflag"].ToString() == "Y";
                                bool AltaFlag = SPChargeRow["AltaFlag"].ToString() == "Y";
                                if ((CardActive && SPChargeRow["useflag"].ToString() == "Y") || CCOverride)
                                {
                                    DateTime msaledate = SPChargeRow.Field<DateTime>("saledate");
                                    //calc_amt_due
                                    string mski3free = "N";
                                    decimal mmaxamt = 0;
                                    decimal msbupamt = 0;
                                    string msbdesc = string.Empty;
                                    //SELECT spcards
                                    if (ExpressType["dayonly"].ToString() != "0")
                                    {
                                        mticktype = 1; //&& Area Day
                                    }
                                    else
                                    {
                                        if ((SPChargeRow["lateflag"].ToString() == "Y") && !SBFlag)
                                        {
                                            mticktype = 14; //&& Late pass only
                                            if ((SPChargeRow["at3flag"].ToString() == "Y") && SPChargeRow["begflag"].ToString() == "Y")
                                            {
                                                mticktype = 130;
                                                int nticktype = SPChargeRow.Field<int>("nticktype");
                                                if ((nticktype == 2016 || nticktype == 2018 || nticktype == 2019) && mticktype == 130)
                                                {
                                                    mski3free = "Y";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if ((SPChargeRow["begflag"].ToString() == "Y") && !SBFlag)
                                            {
                                                mticktype = 4; //&& Beginner Lifts Only
                                            }
                                            else
                                            {
                                                if ((SPChargeRow["amflag"].ToString() == "N") && !SBFlag)
                                                {
                                                    mticktype = 3; //&& PM Only
                                                }
                                                else
                                                {
                                                    mticktype = 1; //&& Area Day
                                                    if (ExpressType["multiflag"].ToString() == "1")
                                                    {
                                                        //DO check_multiday_use  
                                                        string msqlcmd = $"SELECT readdate, altaflag, amflag, sbflag FROM applications.skivisits WHERE serialkey = '{SPChargeRow["serialkey"].ToString()}' AND readdate <= '{mxstart.ToString(Mirror.AxessDateFormat)}' ORDER BY readdate DESC";
                                                        DataTable CardUse = CF.LoadTable(DW.dwConn, msqlcmd, "CardUse");
                                                        if (CardUse.Rows.Count > 0)
                                                        {
                                                            bool mckflag = true;
                                                            int mxdays = 0;
                                                            DateTime mcurdate = mxstart;
                                                            bool mfreedayused = false;
                                                            foreach (DataRow CardUseRow in CardUse.Rows)
                                                            {
                                                                if (!mckflag)
                                                                {
                                                                    break;
                                                                }
                                                                DateTime CardUseReadDate = CardUseRow.Field<DateTime>("readdate");
                                                                if (CardUseReadDate > (mcurdate.AddDays(-3)))
                                                                {
                                                                    bool DeductDay = CardUseRow["altaflag"].ToString() == "Y" && (CardUseRow["amflag"].ToString() == "Y" || SBFlag);
                                                                    if (CardUseReadDate == (mcurdate.AddDays(-1)))
                                                                    {
                                                                        if (DeductDay)
                                                                        {
                                                                            mxdays += 1;
                                                                        }
                                                                        else
                                                                        {
                                                                            mfreedayused = true;
                                                                        }
                                                                        mcurdate = CardUseReadDate;
                                                                    }
                                                                    else
                                                                    {
                                                                        //****allow free days ****
                                                                        if ((CardUseReadDate == mcurdate.AddDays(-2)) && !mfreedayused)
                                                                        {
                                                                            if (DeductDay)
                                                                            {
                                                                                mxdays = mxdays + 1;
                                                                                mcurdate = CardUseReadDate;
                                                                                mfreedayused = true;
                                                                            }
                                                                            else
                                                                            {
                                                                                mckflag = false;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    mckflag = false;
                                                                }
                                                            }
                                                            if (mxdays != 0)
                                                            {
                                                                mticktype = 33; // && Multiday days 2-3 Pass
                                                                if (mxdays == 1) 
                                                                {
                                                                    mticktype = 32; //Multiday 1st day pass
                                                                }
                                                                if (mxdays == 4 || mxdays == 5)
                                                                {
                                                                    mticktype = 35; // && Multiday days 5-6 Pass
                                                                }
                                                                if (mxdays > 5)
                                                                {
                                                                    mticktype = 37; // && Multiday 7+ days
                                                                }
                                                            }
                                                        }
                                                    }   //end check_multiday_use
                                                }
                                            }
                                        }
                                    }
                                    //** Mountain Collective
                                    bool MTC = SPChargeRow["tgroup"].ToString().Trim().ToUpper() == "MC";
                                    if (MTC)
                                    {
                                        mticktype = 1; //&& Alta
                                    }
                                    DataTable ExpPrices = CF.LoadTable(DW.dwConn, $"SELECT nticktype, tickdesc, aprice, cprice, sba, sbc, pos, 0 AS amtdue, 0 as sbamtdue, aprice2, cprice2 FROM {DW.ActiveDatabase}.expprices", "ExpPrices");
                                    DataRow ExpCfg = CF.LoadDataRow(DW.dwConn, $"SELECT sdate, edate FROM {DW.ActiveDatabase}.expcfg");
                                    //    ***load prices based on date range
                                    string mfield = $"{ExpressType["ratecode"].ToString()}price2";
                                    if (ExpressType["ratecode"].ToString() == string.Empty)
                                        mfield = "cprice2";
                                    string mfield2 = "disc2amt";
                                    if (mxstart < ExpCfg.Field<DateTime>("sdate") || mxstart > ExpCfg.Field<DateTime>("edate"))
                                    {
                                        mfield = mfield.Replace("2", "");
                                        mfield2 = mfield2.Replace("2", "");
                                    }
                                    string msbfield = $"sb{ExpressType["ratecode"]}";
                                    DataRow ExpPrice = CF.LoadDataRow(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.expprices WHERE nticktype={mticktype.ToString()}");
                                    decimal Field1Val = ExpPrice.Field<decimal>(mfield);
                                    decimal Field2Val = ExpressType.Field<decimal>(mfield2);
                                    decimal maxdayrate = Field1Val * (Field2Val / 100);
                                    //*** caculate amount due ***
                                    decimal mamtdue = 0;
                                    if (ExpressType["disctype"].ToString() == "P")
                                    {
                                        mamtdue = maxdayrate;
                                    }
                                    else
                                    {
                                        mamtdue = Field1Val + Field2Val;
                                    }
                                    decimal msbamtdue = ExpPrice.Field<decimal>(msbfield);
                                    //** Mountain Collective
                                    if (MTC)
                                    {
                                        mticktype = 1; //&& Alta
                                        //* **check if free day***
                                        if (SPChargeRow.Field<int>("freedays") > 0)
                                        {
                                            // DO check_card_use
                                            mkey = SPChargeRow["mcpnbr"].ToString();
                                            Int64 DaysSkiedX = CF.RowCount(DW.dwConn, $"{DW.ActiveDatabase}.skivisits", $"mcpnbr='{mkey}' AND readdate <= '{sxStart}'");
                                            //int DaysSkied = Convert.ToInt32(CF.GetSQLField(DW.dwConn, $"SELECT COUNT(*) AS daysskied FROM (SELECT mcpnbr, readdate FROM {DW.ActiveDatabase}.skivisits WHERE mcpnbr mcpnbr='{mkey}' AND readdate <= '{sxStart}' GROUP BY mcpnbr)"));
                                            string FreeDaysStr = CF.GetSQLField(BUY.Buy_Alta_ComConn, $"SELECT avail_days FROM {BUY.ActiveDatabase}.mtncol_sales WHERE mcpnbr='{mkey}'");
                                            Int64 FreeDays = (FreeDaysStr == string.Empty ? SPChargeRow.Field<int>("freedays") : Convert.ToInt64(FreeDaysStr));

                                            if (DaysSkiedX <= FreeDays)
                                            {
                                                mamtdue = 0;
                                            }
                                        }
                                    }
                                    //* deduct reload rate for convenience cards
                                    //if (ExpressType.Field<int>("ticktype") == 30037 && (mticktype < 30 || mticktype > 39))
                                    //{
                                    //    mamtdue = mamtdue - 3;
                                    //}
                                    if (mamtdue < 0)
                                    {
                                        mamtdue = 0;
                                    }
                                    //* ski at three free
                                    if (mski3free == "Y")
                                    {
                                        mamtdue = 0;
                                    }
                                    //*** calcualte alta snowbird upgrades for convenience cards ****
                                    if (SBFlag && !MTC)
                                    {
                                        mticktype = (SBFlag && !AltaFlag) ? 50 : 30014;
                                        if (mxstart >= ExpCfg.Field<DateTime>("sdate") && mxstart <= ExpCfg.Field<DateTime>("edate"))
                                        {
                                            mfield = $"{ExpressType["ratecode"].ToString().ToLower()}price2";
                                            mfield2 = "disc2amt";
                                        }
                                        else
                                        {
                                            mfield = $"{ExpressType["ratecode"].ToString().ToLower()}price";
                                            mfield2 = "discamt";
                                        }
                                        msbfield = "sb" + ExpressType["ratecode"].ToString().ToLower();
                                        maxdayrate = ExpPrice.Field<decimal>(mfield) * (ExpressType.Field<decimal>(mfield2) / 100);
                                        if (mticktype == 30014)
                                        {
                                            mmaxamt = maxdayrate; //&& altasnowbird max
                                            mticktype = 30032; //&& Alta Snowbird Upgrade
                                            ExpPrice = CF.LoadDataRow(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.expprices WHERE nticktype={mticktype}");
                                            if (CF.RowHasData(ExpPrice))
                                            {
                                                msbupamt = ExpPrice.Field<decimal>(mfield) * (ExpressType.Field<decimal>(mfield2) / 100); 
                                                msbdesc = ExpPrice["tickdesc"].ToString();
                                            }
                                            mamtdue = mamtdue + msbupamt;
                                            //if (mamtdue > mmaxamt)
                                            //{
                                            //    mamtdue = mmaxamt;
                                            //}
                                        }
                                        else
                                        {
                                            mamtdue = maxdayrate; //&& snowbird only
                                        }
                                    }
                                    //* copper card deal - spring of 2016 04 / 04 / 2016 - 04 / 08 / 2016 * *********************
                                    if (SPChargeRow.Field<int>("nticktype") == 2018 && (mticktype == 3 || mticktype == 4 || mticktype == 14) && (mxstart >= new DateTime(2016, 04, 04) && mxstart <= new DateTime(2016, 04, 08)))
                                    {
                                        mamtdue = 10;
                                    }
                                    CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.expprices SET amtdue={mamtdue.ToString()}, sbamtdue={msbamtdue.ToString()} WHERE nticktype = {mticktype}");
                                    string Expfield = string.Empty;
                                    if (SBFlag)
                                    {

                                        Expfield = ", sbamt=" + ExpPrice[$"sb{ExpressType["ratecode"].ToString()}"].ToString();
                                    }
                                    CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards SET aramt = {mamtdue.ToString()}, sbamt={msbamtdue.ToString()}, usedesc='{ExpPrice["tickdesc"].ToString().Trim()}', usedate='{sxStart}', uticktype={ExpPrice["nticktype"].ToString()}{Expfield} WHERE recid={SPChargeRow["recid"].ToString()}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ARTrans_Update(DataRow ARTran, DataRow PmtRow, DataRow SaleRow)
        {
            //**mark orginal AR as Adjusted**
            if (CF.ExecuteSQL(DW.dwConn, $"update {DW.ActiveDatabase}.{ARTransTable} set transkey='{ARTran["transkey"].ToString()}-A1', dtupdate='{DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}' WHERE recid={ARTran["recid"].ToString()}"))
            {
                //**insert record for adjustment
                string mtranskey = $"{ARTran["transkey"].ToString()}-A2";
                string mxauthcode = ARTran["authcode"].ToString();
                DateTime? mbilldate = null;
                if (mxauthcode != string.Empty && ARTran.Field<int>("tokenkey") > 0 )
                {
                    mxauthcode = string.Empty;
                }
                else
                {
                    mbilldate = ARTran.Field<DateTime>("billdate");
                }
                string sBillDate = (mbilldate.HasValue ? "null" : $"'{mbilldate.Value.ToString(Mirror.AxessDateFormat)}'");
                if (CF.ExecuteSQL(DW.dwConn, $"INSERT INTO {DW.ActiveDatabase}.{ARTransTable} (transdate,posnbr,transkey,transtype,arcustid,arname,transdesc,debitamt,creditamt," +
                    $"authcode,billdate,serialkey,cardacct,mediaid,firstname,lastname,tokenkey,authnet_custid,tgroup,nticktype,tickdesc,nperstype,persdesc,dtupdate)" +
                    $" Values ('{ARTran.Field<DateTime>("transdate").ToString(Mirror.AxessDateFormat)}',{PmtRow.Field<int>("nkassanr").ToString()},'{mtranskey}'," +
                    $"'{ARTran["transtype"].ToString()}','{ARTran["arcustid"].ToString()}','{ARTran["arname"].ToString()}','{ARTran["transdesc"].ToString()}'," +
                    $"{ARTran.Field<decimal>("creditamt").ToString("0#.00")},{ARTran.Field<decimal>("debitamt").ToString("0#.00")},'{mxauthcode}',{sBillDate}," +
                    $"'{ARTran["serialkey"].ToString()}','{ARTran["cardacct"].ToString()}','{ARTran["mediaid"].ToString()}','{mfname}','{mlname}'," +
                    $"{ARTran.Field<int>("tokenkey").ToString()},{ARTran.Field<int>("authnet_custid").ToString()},'{ARTran["tgroup"].ToString().Trim()}'," +
                    $"{ARTran.Field<int>("nticktype").ToString()},'{CF.EscapeChar(ARTran["tickdesc"].ToString())}',{ARTran.Field<int>("nperstype").ToString()}, " +
                    $"'{ARTran["persdesc"].ToString()}','{DateTime.Now.ToString(Mirror.AxessDateFormat)}')"))
                {
                    //ARTrans_Insert(ARTran, PmtRow, SaleRow);
                }
            }
        }

        private void ARTrans_Insert(DataRow ARTransRow, DataRow PmtRow, DataRow SaleRow)
        {
            string mtickdesc = CF.EscapeChar(SaleRow["tickdesc"].ToString());
            string mpersdesc = CF.EscapeChar(SaleRow["persdesc"].ToString());

            //* *insert artrans record **
            string msqlcmd = $"INSERT INTO {DW.ActiveDatabase}.{ARTransTable} (transdate, posnbr, transkey, transtype, arcustid, arname, transdesc, debitamt, creditamt, " +
                $"authcode, billdate, serialkey, cardacct, mediaid, firstname, lastname, tokenkey, authnet_custid, tgroup, nticktype, tickdesc, " +
                $"nperstype, persdesc, dtupdate) Values('{PmtRow.Field<DateTime>("saledate").ToString(Mirror.AxessDateFormat)}',{PmtRow.Field<int>("nkassanr").ToString()}," +
                $"'{mkey}','{mtranstype}','{marcustid}','{marname}','{CF.EscapeChar(mdesc)}',{mdebitamt.ToString("0#.00")},{mcreditamt.ToString("0#.00")},'{mauthcode}'," +
                $"'{mbilldate}','{mserialkey}','{mcardacct.Replace("'", "")}','{mmediaid.Replace("'","")}','{mfname}','{mlname}',{mtoken.ToString()},{mauthnet_custid}," +
                $"'{SaleRow["tgroup"].ToString().Trim()}',{mticktype},'{mtickdesc}',{mperstype},'{mpersdesc}','{mdate.ToString(Mirror.AxessDateTimeFormat)}')";
            CF.ExecuteSQL(DW.dwConn, msqlcmd);
        }

        private void UpdateExpPrices()
        {
            VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}expresstype.dbf";
            DataTable ExpressType = CF.LoadTable(VFPConn, $"SELECT * FROM expresstype", "expresstype");
            if (ExpressType != null)
            {
                CF.ExecuteSQL(DW.dwConn, $"DELETE FROM {DW.ActiveDatabase}.expresstype");
                foreach (DataRow EV in ExpressType.Rows)
                {
                    CF.ExecuteSQL(DW.dwConn, $"INSERT {DW.ActiveDatabase}.expresstype (ticktype, tickdesc, perstype, persdesc, multiflag, dayonly, sbonly," +
                        $" at3flag, disctype, discamt, disc2amt, ratecode, freedays) VALUES ({EV["ticktype"].ToString().Trim()}, '{EV["tickdesc"].ToString().Trim()}', {EV["perstype"].ToString().Trim()}, '{EV["persdesc"].ToString().Trim()}', " +
                        $"{EV["multiflag"].ToString().Trim()}, {EV["dayonly"].ToString().Trim()}, '{EV["sbonly"].ToString().Trim()}', {EV["at3flag"].ToString().Trim()}, '{EV["disctype"].ToString().Trim()}', " +
                        $"{EV["discamt"].ToString().Trim()}, {EV["disc2amt"].ToString().Trim()},'{EV["ratecode"].ToString().Trim()}', {EV["freedays"].ToString().Trim()})");
                }
            }

            VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}expprices.dbf";
            DataTable ExpPricesFP = CF.LoadTable(VFPConn, $"SELECT * FROM expprices", "expprices");
            if (ExpPricesFP != null)
            {
                CF.ExecuteSQL(DW.dwConn, $"DELETE FROM {DW.ActiveDatabase}.expprices");
                foreach (DataRow EP in ExpPricesFP.Rows)
                {
                    CF.ExecuteSQL(DW.dwConn, $"INSERT {DW.ActiveDatabase}.expprices (nticktype, tickdesc, aprice, cprice, sba, sbc, pos, amtdue, sbamtdue, aprice2, " +
                        $"cprice2) VALUES ({EP["nticktype"].ToString()}, '{EP["tickdesc"].ToString()}', {EP["aprice"].ToString()}, {EP["cprice"].ToString()}, " +
                        $"{EP["sba"].ToString()}, {EP["sbc"].ToString()}, {EP["pos"].ToString()}, {EP["amtdue"].ToString()}, {EP["sbamtdue"].ToString()}, " +
                        $"{EP["aprice2"].ToString()}, {EP["cprice2"].ToString()})");
                }
            }
            VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}expcfg.dbf";
            DataRow ExpCfgFP = CF.LoadDataRow(VFPConn, $"SELECT * FROM expcfg");
            if (ExpCfgFP != null)
            {
                CF.ExecuteSQL(DW.dwConn, $"DELETE FROM {DW.ActiveDatabase}.expcfg");
                CF.ExecuteSQL(DW.dwConn, $"INSERT {DW.ActiveDatabase}.expcfg (sdate, edate) VALUES ('{ExpCfgFP.Field<DateTime>("sdate").ToString(Mirror.AxessDateFormat)}', '{ExpCfgFP.Field<DateTime>("edate").ToString(Mirror.AxessDateFormat)}')");
            }

        }

        private void Calc_Billing_FormClosed(object sender, FormClosedEventArgs e)
        {
            CF.SaveFormData("CalcBilling", Location.Y, Location.X, Height, Width, null, DateTime.Now, (int)e.CloseReason);
        }
    }
}
