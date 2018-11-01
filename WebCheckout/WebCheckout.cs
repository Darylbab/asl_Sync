using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using asl_SyncLibrary;
using Microsoft.Office.Interop.Excel;

namespace WebCheckout
{
    public partial class WebCheckout : Form
    {
        BUY_ALTA_COM BUY = new BUY_ALTA_COM();
        CommonFunctions CF = new CommonFunctions();
        DataWarehouse DW = new DataWarehouse();
        Mirror am = new Mirror();
        Shop AltaShop = new Shop();

        OleDbConnection VFPConn = new OleDbConnection("Provider=VFPOLEDB.1");

        System.Data.DataTable AltaCredit;
        System.Data.DataTable ARTrans;
        System.Data.DataTable ARTransXT;
        System.Data.DataTable AXCust;
        //DataTable AXOrders;
        DataRow AXReloads;
        //DataTable Orders;
        System.Data.DataTable PmtDay;
        DataRow POS20Sum;
        DataRow POS21Sum;
        System.Data.DataTable SalesDay;
        //DataRow WebError;
        System.Data.DataTable WebSales;
        System.Data.DataTable Willcall;

        const string IFormat = "#,##0";
        const string DFormat = "#,##0.00";

        DateTime mStart = DateTime.Today.AddDays(-1);   //assume yesterday
        //DateTime mEnd;
        DateTime mEndDate;
        decimal mVISA = 0;
        decimal mMAST = 0;
        decimal mDisc = 0;
        decimal mAMEX = 0;
        decimal mCardTot = 0;
        decimal mReloadDisc = 0;
        decimal mReloadTot = 0;
        //decimal mSubTot = 0;
        decimal mAXTot = 0;
        //decimal mShipTot = 0;
        //decimal mOrderTot = 0;
        decimal mAltaCredit = 0;
        //decimal madjcardtot = 0;
        //decimal mfamcredits = 0;
        decimal mgiftuse = 0;
        decimal mgiftloads = 0;
        //decimal mgiftsprdcd = 0;
        decimal mwebship = 0;
        //decimal mwebreloads = 0;
        //decimal mweborders = 0;
        //decimal maxorders = 0;
        //decimal maxreloads = 0;
        //decimal mweretail = 0;
        decimal mccbill = 0;
        //decimal mwebretrl = 0;
        //decimal mpassorders = 0;
        decimal mpassreloads = 0;
        decimal mtickn = 0;
        decimal mtickr = 0;
        decimal mskischool = 0;
        //decimal mother = 0;
        //decimal maxpassorders = 0;
        decimal maxpassreloads = 0;
        decimal mwcnew = 0;
        decimal mwcreload = 0;
        decimal mfampck = 0;
        decimal mrcbillamt = 0;
        decimal msalestot = 0;
        //decimal mwebfampck = 0;
        decimal mwebauthnet = 0;
        decimal mwebcoupon = 0;
        decimal mwebgiftuse = 0;
        decimal mwebtotal = 0;
        //decimal mtickexpbeg = 0;
        //decimal mtickexpend = 0;
        decimal maxrecd = 0;
        decimal mwcsent = 0;
        decimal maxcanc = 0;
        decimal mwccredit = 0;
        decimal mwcrefund = 0;
        //long mqty = 0;
        //decimal mprice = 0;
        //decimal mhfeet = 0;
        //decimal mhinches = 0;
        decimal mtrretail = 0;
        //decimal mtrgift = 0;
        //decimal mtrwillcall = 0;
        decimal mvcwillcall = 0;
        decimal mShort = 0;
        //decimal maxsales = 0;
        decimal mwebsales = 0;
        //decimal mwebretail = 0;
        //decimal mwebretailreload = 0;
        decimal mwebrefund = 0;
        decimal mwebgift = 0;
        decimal mTotRec = 0;
        string sStart = string.Empty;
        string sEnd = string.Empty;
        string mLastName = string.Empty;
        string mFirstName = string.Empty;
        //string mOrderID = string.Empty;
        string mWebID = string.Empty;
        string mProduct = string.Empty;
        string mType = string.Empty;
        DateTime mDOB = DateTime.Today;



        public WebCheckout()
        {
            InitializeComponent();
        }

        private void WebCheckout_Shown(object sender, EventArgs e)
        {
            sStart = mStart.ToString(Mirror.AxessDateFormat);
            sEnd = mStart.AddDays(1).ToString(Mirror.AxessDateFormat);

            // files to open 
            // comes from open_files

            //'axblockstr'
            //'tgroups'         DW
            //'salesdatastr'
            //'pmtdatastr'
            //'pospmtstr'
            //'tickettypes'     DW
            //'persontype'      DW
            //'pmttype'         DW
            //'orderdetstr'
            //'axordersstr'
            //'arcust'          DW
            //'ccbill'
            //'salespmtday'
            //'possum'          DW
            //'weberrors'
            //'fampckstr'
            //'temp'
            //'daylightsavings'
            //'arsum'
            LoadData();
        }

        //private void OrdersSum()
        //{
        //    foreach (DataRow Order in AXOrders.Rows)
        //    {
        //        int mticktype = Order.Field<int>("nkundenkartentypnr");
        //        string mtickdesc = Order["descrip"].ToString();
        //        int mperstype = Order.Field<int>("nperstypnr");
        //        string mpersdesc = Order["descrip"].ToString();
        //        decimal mcomp = 0;
        //        decimal mopen = 0;
        //        //decimal moth = 0;
        //        decimal mcompamt = 0;
        //        decimal mopenamt = 0;
        //        //decimal mothamt = 0;
        //        while ((mticktype == Order.Field<int>("nkundenkartentypnr")) && (mperstype == Order.Field<int>("nperstypnr")))
        //        {
        //            if (Order.Field<int>("norderstatusnr") == 1)
        //            {
        //                mopen = mopen + 1;
        //                mopenamt = mopenamt + Order.Field<decimal>("feinzeltarif");
        //            }
        //            else
        //            {
        //                mcomp = mcomp + 1;
        //                mcompamt = mcompamt + Order.Field<decimal>("feinzeltarif");
        //            }
        //        }
        //        CF.ExecuteSQL(DW.dwConn, $"INSERT {DW.ActiveDatabase}.orderdetstr (nticktype, tickdesc, nperstypnr, persdesc, cnt, amt, opencnt, openamt");
        //        //SELECT orders
        //        //APPEND BLANK
        //        //replace ticktypenr WITH mticktype,;
        //        //tickdesc WITH mtickdesc,;
        //        //nperstypnr with mperstype,;
        //        //persdesc WITH mpersdesc,;
        //        //cnt with mopen + mcomp + moth,;
        //        //amt with mopenamt + mcompamt + mothamt,;
        //        //opencnt with mopen,;
        //        //openamt with mopenamt,;
        //        //compcnt with mcomp,;
        //        //compamt with mcompamt,;
        //        //othcnt with moth,;
        //        //othamt with mothamt

        //    }
        //}

        private void BtnEstoreDetail_Click(object sender, EventArgs e)
        {
            DataRow[] WebSaleX= WebSales.Select($"status='APPROVED'");
            Cursor = Cursors.WaitCursor;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            string t = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string r = System.AppDomain.CurrentDomain.BaseDirectory;
            Workbook wb = excel.Workbooks.Open(r + @"\reports\AltaCredit.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"Estore Detail Sales Report/r/n{sStart}";
            int CurRow = 2;
            int TotQty = 0;
            decimal Tot = 0;
            decimal OrderTot = 0;
            decimal CreditAmt = 0;
            decimal CCAmt = 0;
            decimal CCTot = 0;
            string OrderID = string.Empty;
            foreach (DataRow WSX in WebSaleX)
            {
                if (OrderID == string.Empty ||OrderID == WSX["OrderID"].ToString())
                {
                    OrderID = WSX["OrderID"].ToString();
                    ws.Cells[CurRow, 1] = OrderID;
                    ws.Cells[CurRow, 2] = (WSX["WebID"].ToString() != string.Empty ? "R" : "N");
                    ws.Cells[CurRow, 3] = (WSX["WebID"].ToString() != string.Empty ? (WSX["WebID"].ToString() + "       ").Substring(0, 8) : WSX["ItemID"].ToString());
                    ws.Cells[CurRow, 4] = WSX["product_name"].ToString();
                    ws.Cells[CurRow, 5] = WSX["firstname"].ToString() + " " + WSX["lastname"].ToString();
                    ws.Cells[CurRow, 6] = WSX.Field<int>("njournalno").ToString();
                    ws.Cells[CurRow++, 3] = WSX.Field<decimal>("price").ToString("0#.00");
                    ws.Cells[CurRow, 6] = WSX.Field<int>("qty").ToString();
                    decimal MyTotal = WSX.Field<decimal>("total");
                    ws.Cells[CurRow++, 3] = MyTotal.ToString("0#.00");
                    OrderTot += MyTotal;
                    Tot += MyTotal;
                    CreditAmt += WSX.Field<decimal>("credit_amt");
                    CCTot += WSX.Field<decimal>("ccamt");
                    CCAmt += WSX.Field<decimal>("ccamt");
                    TotQty++;
                }
                else
                {
                    ws.Cells[CurRow, 4] = $"/t/t/tCredit Used: {CreditAmt.ToString("0#.00")}";
                    ws.Cells[CurRow, 5] = $"CC Charge {CCAmt.ToString("0#.00")}  {WSX["tran_approval_code"].ToString()}";
                    ws.Cells[CurRow, 7] = "Amount Due:";
                    ws.Cells[CurRow++, 8] = OrderTot.ToString("0#.00");
                    CreditAmt = 0;
                    OrderID = WSX["OrderID"].ToString();
                    OrderTot = 0;
                    CCAmt = 0;

                }
            }
            ws.Cells[CurRow, 4] = $"/t/t/tCredit Used: {mwebcoupon.ToString("0#.00")}";
            ws.Cells[CurRow, 5] = $"CC Charge {mwebauthnet.ToString("0#.00")}";
            ws.Cells[CurRow, 6] = "Totals";
            ws.Cells[CurRow, 7] = TotQty.ToString();
            ws.Cells[CurRow, 8] = Tot.ToString("0#.00");
            excel.Visible = true;
            ws.PrintOutEx(Type.Missing, Type.Missing, 1, true);
            excel.Visible = false;
            wb.Close(false);
            //SET FILTER TO
            //SELECT ccbill
            //SET ORDER TO authcode
            //SET FILTER TO billdate = mstart
            System.Data.DataTable CCBill = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.ccbill WHERE billdate='{sStart}' ORDER BY authcode", "CCBill");
            Cursor = Cursors.WaitCursor;
            excel = new Microsoft.Office.Interop.Excel.Application();
            wb = excel.Workbooks.Open(r + @"\reports\expbill.xlsx");
            ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"Gold & Express Billing Card Use/r/n{sStart}";
            CurRow = 3;
            TotQty = 0;
            Tot = 0;
            OrderTot = 0;
            CreditAmt = 0;
            CCAmt = 0;
            CCTot = 0;
            OrderID = string.Empty;
            foreach (DataRow WSX in WebSaleX)
            {
                if (OrderID == string.Empty || OrderID == WSX["OrderID"].ToString())
                {
                    OrderID = WSX["OrderID"].ToString();
                    ws.Cells[CurRow, 1] = OrderID;
                    ws.Cells[CurRow, 2] = (WSX["WebID"].ToString() != string.Empty ? "R" : "N");
                    ws.Cells[CurRow, 3] = (WSX["WebID"].ToString() != string.Empty ? (WSX["WebID"].ToString() + "       ").Substring(0, 8) : WSX["ItemID"].ToString());
                    ws.Cells[CurRow, 4] = WSX["product_name"].ToString();
                    ws.Cells[CurRow, 5] = WSX["firstname"].ToString() + " " + WSX["lastname"].ToString();
                    ws.Cells[CurRow, 6] = WSX.Field<int>("njournalno").ToString();
                    ws.Cells[CurRow++, 3] = WSX.Field<decimal>("price").ToString("0#.00");
                    ws.Cells[CurRow, 6] = WSX.Field<int>("qty").ToString();
                    decimal MyTotal = WSX.Field<decimal>("total");
                    ws.Cells[CurRow++, 3] = MyTotal.ToString("0#.00");
                    OrderTot += MyTotal;
                    Tot += MyTotal;
                    CreditAmt += WSX.Field<decimal>("credit_amt");
                    CCTot += WSX.Field<decimal>("ccamt");
                    CCAmt += WSX.Field<decimal>("ccamt");
                    TotQty++;
                }
                else
                {
                    ws.Cells[CurRow, 4] = $"/t/t/tCredit Used: {CreditAmt.ToString("0#.00")}";
                    ws.Cells[CurRow, 5] = $"CC Charge {CCAmt.ToString("0#.00")}  {WSX["tran_approval_code"].ToString()}";
                    ws.Cells[CurRow, 7] = "Amount Due:";
                    ws.Cells[CurRow++, 8] = OrderTot.ToString("0#.00");
                    CreditAmt = 0;
                    OrderID = WSX["OrderID"].ToString();
                    OrderTot = 0;
                    CCAmt = 0;

                }
            }
            ws.Cells[CurRow, 4] = $"/t/t/tCredit Used: {mwebcoupon.ToString("0#.00")}";
            ws.Cells[CurRow, 5] = $"CC Charge {mwebauthnet.ToString("0#.00")}";
            ws.Cells[CurRow, 6] = "Totals";
            ws.Cells[CurRow, 7] = TotQty.ToString();
            ws.Cells[CurRow, 8] = Tot.ToString("0#.00");
            excel.Visible = true;
            ws.PrintOutEx(Type.Missing, Type.Missing, 1, true);
            excel.Visible = false;
            wb.Close(false);
            Cursor = Cursors.Default;



            //GO TOP
            //IF !EOF()
            //    REPORT form expbill preview
            //ENDIF
            //SET FILTER TO;
            btnExit.Focus();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void BtnLoadData_Click(object sender, EventArgs e) => LoadData();

        private void LoadData()
        {
            Cursor.Current = Cursors.WaitCursor;
            SetButtonStatus(false);
            SetTextboxStatus(false);
            SetLabelStatus(false);
            StatusText.Enabled = true;
            StatusText.Visible = true;
            btnExit.Enabled = true;
            btnSummaryReport.Enabled = true;
            btnEstoreDetail.Enabled = true;
            StatusText.Text = "Loading Checkout Info";
            System.Windows.Forms.Application.DoEvents();
            AXCust = CF.LoadTable(am.MirrorConn, $"SELECT * FROM {am.ActiveDatabase}.tabaxcust WHERE ((nperskassa < 60) OR (nperskassa between 117 AND 120)) ORDER BY nperskassa, npersnr", "AXCust");
            //FoxPro
            VFPConn.ConnectionString = @"provider=vfpoledb.1;data source = G:\FPData\altaax\possum.dbf";
            POS20Sum = CF.LoadDataRow(VFPConn, $"SELECT cmast, cvisa, cdisc, camex, cardtot, cgiftuse, cgiftadd, totrec, webreloads, weborders, webskisch, webfampck, webtickn, webtickr, webgift, webrefund, ccbillamt, webship, acredit, axtot, axwebdisc FROM possum WHERE saledate={CF.SetFoxProDate(mStart)} AND pos=20");
            VFPConn.ConnectionString = @"provider=vfpoledb.1;data source = G:\FPData\altaax\possum.dbf";
            POS21Sum = CF.LoadDataRow(VFPConn, $"SELECT cmast, cvisa, cdisc, camex, cardtot, cgiftuse, cgiftadd, totrec, webreloads, weborders, webskisch, webfampck, webtickn, webtickr, webgift, webrefund, ccbillamt, webship, acredit, axtot, axwebdisc FROM possum WHERE saledate={CF.SetFoxProDate(mStart)} AND pos=21");
//mySQL
            //POS20Sum = CF.LoadDataRow(DW.dwConn, $"SELECT cmast, cvisa, cdisc, camex, cardtot, cgiftuse, cgiftadd, totrec, webreloads, weborders, webskisch, webfampck, webtickn, webtickr, webgift, webrefund, ccbillamt, webship, acredit, axtot, axwebdisc FROM {DW.ActiveDatabase}.possum WHERE saledate='{mStart.ToString(Mirror.AxessDateFormat)}' AND pos=20");
            //POS21Sum = CF.LoadDataRow(DW.dwConn, $"SELECT cmast, cvisa, cdisc, camex, cardtot, cgiftuse, cgiftadd, totrec, webreloads, weborders, webskisch, webfampck, webtickn, webtickr, webgift, webrefund, ccbillamt, webship, acredit, axtot, axwebdisc FROM {DW.ActiveDatabase}.possum WHERE saledate='{mStart.ToString(Mirror.AxessDateFormat)}' AND pos=21");
            //ClrFields();
            if (POS20Sum != null)
            {
                LoadFields();
            }
            mEndDate = mStart.AddDays(1);
            StatusText.Text = "Loading eStore Data";
            System.Windows.Forms.Application.DoEvents();
            GetWebSales();
            GetCreditUse();
            StatusText.Text = "Loading Alta Credit Info";
            System.Windows.Forms.Application.DoEvents();
            Update_Reloads();
            GetWillcall();
            GetRCBill();
            StatusText.Text = "Loading Axess Info";
            System.Windows.Forms.Application.DoEvents();
            GetAXWebReloads();
            //GetAXOrders();
            StatusText.Text = "Updating Checkout Detail Files";
            System.Windows.Forms.Application.DoEvents();
            bool msalesflag = false;
            if (POS20Sum != null) msalesflag = true;
            if (POS21Sum != null) msalesflag = true;
            if (mccbill != 0) msalesflag = true;
                if (msalesflag)
            {
                if (mCardTot == 0)
                {
                    txtVISA.Enabled = true;
                    txtMastercard.Enabled = true;
                    txtAMEX.Enabled = true;
                    txtDiscover.Enabled = true;
                    SetButtonStatus(false);
                    btnEdit.Enabled = false;
                    btnSave.Enabled = true;
                    btnAbort.Enabled = true;
                    Notification.Visible = true;
                    CalcTot();
                }
                else
                {
                    txtVISA.Enabled = false;
                    txtMastercard.Enabled = false;
                    txtAMEX.Enabled = false;
                    txtDiscover.Enabled = false;
                    SetButtonStatus(false);
                    btnEdit.Enabled = true;
                    btnExit.Enabled = true;
                    btnSave.Enabled = false;
                    btnAbort.Enabled = false;
                    Notification.Visible = false;
                    btnSummaryReport.Enabled = true;
                    btnEstoreDetail.Enabled = true;
                    CalcTot();
                    EnableRpts();
                }
                StatusText.Visible = false;
                SetLabelStatus(true);
                btnExit.Enabled = true;
                btnSummaryReport.Enabled = true;
                btnEstoreDetail.Enabled = true;
                btnPreviousDay.Enabled = true;
                btnNextDay.Enabled = true;
                System.Windows.Forms.Application.DoEvents();
            }
            StatusText.Enabled = false;
            StatusText.Visible = false;
            Cursor.Current = Cursors.Default;
        }

        private void EnableRpts()
        {
            //btnPassCardOrders.Enabled = (mpassorders == 0);
            btnPrepaidsReloads.Enabled = (mpassreloads == 0);
            btnFamilyPackages.Enabled = (mfampck == 0);
            btnTicketReservations.Enabled = (mtickn == 0);
            btnDateSpecificReloads.Enabled = (mtickr == 0);
            btnSkiSchoolSales.Enabled = (mskischool == 0);
            btnGiftCards.Enabled = (mwebgift == 0);
            btnAuthNetRefunds.Enabled = (mwebrefund == 0);
            btnRCBillingTotal.Enabled = (mrcbillamt == 0);
            //btnAxessOrders.Enabled = (maxpassorders == 0);
            btnAxessPrepaidsReloads.Enabled = (maxpassreloads == 0);
            btnWillcallTicketReservations.Enabled = (mwcnew == 0);
            btnWillcallDateSpecificReloads.Enabled = (mwcreload == 0);
            btnCreditUses.Enabled = (mAltaCredit == 0);
            //btnErrorsReport.Enabled = (mpassorders != maxpassorders || mpassreloads != maxpassreloads);
            btnErrorsReport.Enabled = (mpassreloads != maxpassreloads);
            StatusText.Visible = false;
        }

        private void CalcTot()
        {
            // ***GET E - STORE TOTALS***
            //ClrFields();
            foreach (DataRow WebSale in WebSales.Rows)
            {
                string mOrderID = WebSale["orderid"].ToString().Trim();

                decimal mccamt = 0;
                decimal mcredit = 0;
                decimal lgiftuse = 0;
                decimal lwebship = 0;
                while (mOrderID == WebSale["orderid"].ToString().Trim())
                {
                    if (WebSale["status"].ToString().Trim() == "APPROVED" || WebSale["orderid"].ToString().Trim() == "CREDIT_APPLIED")
                    {
                        mwebtotal = mwebtotal + WebSale.Field<decimal>("total");
                        mccamt = WebSale.Field<decimal>("ccamt");
                        mcredit = WebSale.Field<decimal>("credit_amount");
                        lgiftuse = WebSale.Field<decimal>("gift_card_amount");
                        lwebship = WebSale.Field<decimal>("shipping_amount");
                        {
                            //**Add new date specific and new tickets with Alta card to new tickets
                            if ((WebSale["product_type"].ToString() == "new" && WebSale["product_group"].ToString().Substring(0, 4).ToLower() == "date") || WebSale["product_type"].ToString() == "altacard")
                            {
                                mtickn = mtickn + WebSale.Field<decimal>("price");
                            }
                            else
                            {
                                //* *Add new good anytime to new tickets
                                if (WebSale["product_group"].ToString().Substring(0, 4) == "good" && WebSale["product_type"].ToString() == "new")
                                {
                                    mtickn = mtickn + WebSale.Field<decimal>("price");
                                }
                                else
                                {
                                    //* *Add date specific reloads to reload tickets
                                    if (WebSale["product_type"].ToString() == "reload" && WebSale["product_group"].ToString().Substring(0, 4) == "date")
                                    {
                                        mtickr = mtickr + WebSale.Field<decimal>("price");
                                    }
                                    else
                                    {
                                        //* *Add Pre - Paids, Family Packages to Pass Reloads
                                        if ((WebSale["product_type"].ToString() == "reload" && WebSale["product_group"].ToString().Substring(0, 4) != "date") || (WebSale.Field<int>("tickettype") == 123) || (WebSale["webid"].ToString() == "4BAA2F04-003-2C2") || (WebSale["product_group"].ToString().Substring(0, 6) == "season") || (WebSale.Field<int>("tickettype") == 99 && WebSale["product_type"].ToString() == "new"))
                                        {
                                            mpassreloads = mpassreloads + WebSale.Field<decimal>("price");
                                        }
                                        else
                                        {
                                            //* *Add Ski School products to Ski School
                                            if (WebSale["product_group"].ToString().Substring(0, 3) == "ski" && WebSale["webid"].ToString() != "4BAA2F04-003-2C2")
                                            {
                                                mskischool = mskischool + WebSale.Field<decimal>("price");
                                            }
                                            else
                                            {
                                                //* *Add Gift Cards to Web Gift
                                                if (WebSale["product_name"].ToString() == "Alta Gift Card")
                                                {
                                                    mwebgift = mwebgift + WebSale.Field<decimal>("price");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        mccamt = 0;
                        mcredit = 0;
                    }
                }
                mwebcoupon += mcredit;
                txtECoupons.Text = mwebcoupon.ToString(DFormat);
                mwebauthnet += mccamt;
                txtCreditCards.Text = mwebauthnet.ToString(DFormat);
                mwebgiftuse += lgiftuse;
                txtGiftCardUse.Text = mwebgiftuse.ToString(DFormat);
                mwebship = mwebship + lwebship;
            }
            //**** get resort charge/ refund totals***
            //SELECT artrans
            mrcbillamt = 0;
            mwebrefund = 0;
            foreach (DataRow Trans in ARTrans.Rows)
            {
                if ((Trans.Field<DateTime>("billdate") == mStart) && (Trans["authcode"].ToString() == string.Empty))
                {
                    if ((Trans.Field<int>("tokenkey") > 0) && (Trans["arcustid"].ToString() == "11874"))
                    {
                        mrcbillamt += Trans.Field<decimal>("debitamt") - Trans.Field<decimal>("creditamt");
                    }
                    if (Trans["transtype"].ToString() == "R")
                    {
                        mwebrefund += Trans.Field<decimal>("debitamt") - Trans.Field<decimal>("creditamt");
                    }
                }
            }
            //* **GET AXESS TOTALS ***
            //***ORDERS * **
            //maxpassorders = 0;
            //foreach (DataRow Order in AXOrders.Rows)
            //{
            //    if (Order.Field<int>("norderstat") >= 1 && Order.Field<int>("norderstat") <= 5)
            //    {
            //        maxpassorders += Order.Field<decimal>("tariff");
            //    }
            //}
            //txtAxessOrders.Text = mpassorders.ToString(DFormat);

            //*** RELOADS ***
            maxpassreloads = 0;
            maxrecd = 0;
            maxcanc = 0;
            if (SalesDay != null)
            foreach (DataRow SaleDay in SalesDay.Rows)
            {
                decimal tariff = SaleDay.Field<decimal>("tariff");
                if (SaleDay.Field<int>("nkassanr") == 20)
                {
                    maxpassreloads += tariff;
                }
                if (SaleDay["ttype"].ToString() == "S")
                {
                    maxrecd += tariff;
                }
                else
                {
                    maxcanc -= tariff;
                }
            }
            txtReceivedByAxess21.Text = maxrecd.ToString(DFormat);
            txtCancelledByAxess.Text = maxcanc.ToString(DFormat);
            mReloadTot = maxpassreloads;
            txtAxessPrepaidsReloads.Text = mReloadTot.ToString(DFormat);
            //* **GET WILLCALL TICKETS ***
            mwcnew = 0;
            txtWillcallTicketReservations.Text = mwcnew.ToString(DFormat);
            mwcreload = 0;
            mwcsent = 0;
            txtSentFromWillcall.Text = mwcsent.ToString(DFormat);
            mwccredit = 0;
            mwcrefund = 0;
            foreach (DataRow WC in Willcall.Rows)
            {
                if (WC.Field<DateTime>("bookdate") == mStart)
                {
                    if (WC["company"].ToString() == "DT")
                    {
                        string TickCode = WC["tickcode"].ToString().ToUpper().Trim();
                        decimal AXVal = WC.Field<decimal>("axtotal");
                        DateTime CancelDate = WC.Field<DateTime>("canceldate");
                        if (TickCode == "NEW" || TickCode == "ALT")
                        {
                            mwcnew += AXVal;
                        }
                        if (TickCode == "REL")
                        {
                            mwcreload += AXVal;
                        }
                        if (WC.Field<DateTime>("arr_date") == mStart.AddDays(1))
                        {
                            mwcsent += AXVal;
                        }
                        if (CancelDate == mStart)
                        {
                            if (TickCode == "REL")
                            {
                                mwccredit += AXVal;
                            }
                            if (WC["itemstatus"].ToString() == "R")
                            {
                                mwcrefund += AXVal;
                            }
                        }
                    }
                }
                txtBackInWillcall.Text = mwccredit.ToString(DFormat);
                //txtAxessOrders.BackColor = (mpassorders != maxpassorders ? Color.Red : Color.Yellow);
                txtAxessPrepaidsReloads.BackColor = (mpassreloads != maxpassreloads + mReloadDisc ? Color.Red : Color.Yellow);
                txtWillcallTicketReservations.BackColor = (mtickn != mwcnew ? Color.Red : Color.Yellow);
                txtWillcallDateSpecificReloads.BackColor = (mtickr != mwcreload ? Color.Red : Color.Yellow);
                //**** summarize results***
                mCardTot = mVISA + mMAST + mAMEX + mDisc;
                //mwebsales = mpassorders + mpassreloads + mskischool + mwebgift + mtickn + mtickr + mfampck + mreloaddisc + mwebship;
                mwebsales = mpassreloads + mskischool + mwebgift + mtickn + mtickr + mfampck + mReloadDisc + mwebship;
                txtEstoreSales.Text = mwebsales.ToString(DFormat);
                mwebauthnet += mrcbillamt + mwebrefund;
                txtCreditCards.Text = mwebauthnet.ToString(DFormat);
                mAXTot = mwebauthnet + mwebcoupon + mwebgiftuse;
                txtEstoreReceipts.Text = mAXTot.ToString(DFormat);
                mTotRec = mCardTot + mgiftuse + mAltaCredit;
                msalestot = mpassreloads + mskischool + mwebgift + mtickn + mtickr + mfampck + mReloadDisc + mrcbillamt + mwebrefund + mwebship;
                //msalestot = mpassorders + mpassreloads + mskischool + mwebgift + mtickn + mtickr + mfampck + mreloaddisc + mrcbillamt + mwebrefund + mwebship;
                mShort = msalestot - mTotRec;
                if (Convert.ToDouble(Math.Abs(mShort)) < 0.01)
                {
                    mShort = 0;
                }
                txtOverShort.Text = mShort.ToString(DFormat);
                txtAuthNetTotal.BackColor = (Convert.ToDouble(Math.Abs(mCardTot - mwebauthnet)) <= 0.01 ? Color.Yellow : Color.Red);
                txtSalesTotal.BackColor = ((msalestot != mAXTot) || (mTotRec != mAXTot) || (mTotRec != msalestot) ? Color.Red : Color.Yellow);
                txtEstoreReceipts.BackColor = txtSalesTotal.BackColor;
                txtSettledReceipts.BackColor = txtSalesTotal.BackColor;
            }
        }

        private void GetAXWebReloads()
        {
            PmtDay = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.pmtdata WHERE saledate = '{sStart}' AND nkassanr IN (20,21) AND testflag = 0 ORDER BY transkey", "PmtDay");
            SalesDay = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.salesdata WHERE saledate = '{sStart}' AND nkassanr IN (20, 21) AND testflag = 0 ORDER BY transkey", "SalesDay");
        }

        //private void GetAXOrders()
        //{
        //    string Query = $"SELECT A.NPROJNR, A.NFIRMENNR, A.NFIRMENKASSANR as nfirmenkas, A.NORDERLISTNR as norderid, A.NAUFTRAGNR as exorderid, " +
        //      $"A.NORDERSTATUSNR as norderstat, A.NKASSANR, A.NTRANSNR, A.NKUNDENKARTENTYPNR as nticktype, A.NPERSTYPNR, A.NPOOLNR, A.FEINZELTARIF as tariff, A.DTGILTAB as orderdate, " +
        //      $"A.DTGILTAB AS issuedate, A.esdate, A.NORDERLISTNR, A.NKARTENNR, A.szdeliveryname AS szdelname, A.NPERSKASSANR AS nperskassa, A.NPERSNR, A.NEINGEGSORTSTK AS odup, " +
        //      $"concat_ws('-', A.nperskassanr, A.npersnr) AS perskey, '{sStart}' AS issuedate, P.szname AS persdesc, K.szname AS tickdesc, C.lastname AS lname, C.firstname AS fname, C.dob " +
        //      $"FROM {am.ActiveDatabase}.tabaxorderdata AS A " +
        //      $"LEFT JOIN {am.ActiveDatabase}.tabperstypdef AS P ON P.nperstypnr = A.nperstypnr " +
        //      $"LEFT JOIN {am.ActiveDatabase}.tabkundenkartentypdef AS K ON K.NKUNDENKARTENTYPNR = A.NKUNDENKARTENTYPNR " +
        //      $"LEFT JOIN {am.ActiveDatabase}.tabaxcust AS C ON C.nperskassa = A.nperskassanr AND C.npersnr = A.npersnr " +
        //      $"WHERE DTGILTAB = '{sStart}' AND(nperskassanr < 50 OR nperskassanr BETWEEN 110 AND 120) AND npoolnr< 100 AND npoolnr<> 94  AND testflag = '0' " +
        //      $"ORDER BY norderlistnr";
        //    AXOrders = CF.LoadTable(am.MirrorConn, Query, "AXOrders");

        //}

        private void GetRCBill()
        {
            string Where = $"billdate = '{sStart}' AND authcode <> '' AND ((tokenkey <> '' AND arcustid = '11874') or transtype = 'R')";
            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.artrans SET creditamt = 0 WHERE creditamt is null AND {Where}");
            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.artrans SET debitamt = 0 WHERE debitamt is null AND {Where}");
            string tVal = CF.GetSQLField(DW.dwConn, $"SELECT SUM(debitamt-creditamt) FROM {DW.ActiveDatabase}.artrans WHERE transtype <> 'R' AND {Where}");
            mrcbillamt = (tVal == string.Empty ? 0 : Convert.ToDecimal(tVal));
            tVal = CF.GetSQLField(DW.dwConn, $"SELECT SUM(creditamt-debitamt) FROM {DW.ActiveDatabase}.artrans WHERE transtype <> '=' AND {Where}");
            mwebrefund = (tVal == string.Empty ? 0 : Convert.ToDecimal(tVal)); 
            ARTrans = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.artrans WHERE {Where}", "ARTrans");
        }

        private void GetWillcall() => Willcall = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.willcall WHERE tickcode IN ('NEW', 'ALT') OR (tickcode='REL' AND company = 'DT')", "willcall");

        private void Update_Reloads()
        {
            foreach (DataRow WebSale in WebSales.Rows)
            {
                if (WebSale["webid"].ToString() != string.Empty)
                {
                    AXReloads = CF.LoadDataRow(DW.dwConn, $"SELECT saledate, nlfdzaehler, transkey FROM {DW.ActiveDatabase}.salesdata WHERE nkassanr=20 AND nseriennr={WebSale["nserialno"].ToString()} ORDER BY saledate DESC LIMIT 1");
                    if (AXReloads != null)
                    {
                        if (AXReloads.Field<DateTime>("saledate") != WebSale.Field<DateTime>("sale_date"))
                        {
                            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET saledate = '{WebSale.Field<DateTime>("saledate").ToString(Mirror.AxessDateFormat)}' WHERE nlfdzaehler='{WebSale["nlfdzaehler"].ToString()}'");
                            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.pmtdata SET saledate = '{WebSale.Field<DateTime>("saledate").ToString(Mirror.AxessDateFormat)}' WHERE transkey='{WebSale["transkey"].ToString()}'");
                        }
                    }
                }
            }
        }

        private void GetWebSales()
        {
            //string Query = $"SELECT DATE(O.created) as sale_date,  O.id as orderid, I.id as itemid, I.parent_order_item_id as fampckid, I.product_id, " +
            //    $"I.tickettype, I.persontype, I.poolnumber, I.product_type, cast(I.product_name as char(30)) as product_name, I.price, I.qty, I.total, " +
            //    $"SUBSTR(O.status, 1, 15) as status, SUBSTR(I.firstname, 1, 20) as firstname, SUBSTR(I.lastname, 1, 20) as lastname, I.birthdate, I.gender, " +
            //    $"I.feet, I.inches, I.webid, I.NJOURNALNO, I.NPOSNO, I.NPROJECTNO, I.NSERIALNO, I.NTARIFF, I.SZCURRENCY, I.product_sku, I.product_group,  " +
            //    $"I.product_description, I.required_docs_json, I.required_docs_verified,  I.flaggedByEvan, I.attach_credit_card, I.axess_skip_soap, I.NERRORNO, " +
            //    $"I.SZERRORMESSAGE, I.SZVALIDTILL, I.NCUSTOMERID, I.axess_status, I.axess_msg, I.created, I.modified, I.cancelled, O.customer_id, " +
            //    $"O.customerPaymentProfileId, O.invoice, O.email, O.optin, O.has_required_docs, O.phone, SUBSTR(O.billing_firstname, 1, 20) as billing_firstname, " +
            //    $"SUBSTR(O.billing_lastname, 1, 20) as billing_lastname, O.billing_address, O.billing_city, O.billing_state, O.billing_zip, O.billing_country, " +
            //    $"O.ship_to_billing_address, O.shipping_firstname, O.shipping_lastname, O.shipping_address, O.shipping_city, O.shipping_state, O.shipping_zip, " +
            //    $"O.shipping_country, O.subtotal, O.shipping_amount, O.shipping_method, O.tax_amount, O.credit_amount, O.gift_card_amount, O.total as ccamt, " +
            //    $"O.cc_name, O.cc_type, O.cc_number, O.cc_expiration_month, O.cc_expiration_year, O.tran_response_code, O.tran_response_sub_code, " +
            //    $"O.tran_response_reason_code, O.tran_response_reason_text, O.tran_approval_code, O.tran_avs_result_code, O.tran_transaction_id, O.tran_original_id, " +
            //    $"O.tran_amount, O.tran_transaction_type, O.portal_id, O.portal_name,  O.is_test, O.created, O.modified, SPACE(20) as confirmed " +
            //    $"FROM altaweb.alta_orders AS O INNER JOIN altaweb.alta_order_items AS I ON O.id = I.order_id " +
            //    $"WHERE I.created = '{sStart}'";
            string Query = $"SELECT DATE(O.created_at) AS sale_date, O.id AS orderid, I.id AS itemid, I.axess_tickettype AS tickettype, I.product_type, " +
                $"CAST(I.product_name AS CHAR (30)) AS product_name, I.price, I.quantity AS qty, I.total, SUBSTR(O.status, 1, 15) AS status, " +
                $"SUBSTR(I.person_firstname, 1, 20) AS firstname, SUBSTR(I.person_lastname, 1, 20) AS lastname, I.axess_wtp AS webid, T.NJOURNALNO, T.NPOSNO AS NPOSNO, " +
                $"T.NSERIALNO, I.product_group, O.subtotal, O.shipping_amount, O.credit_amount, O.giftcard_amount AS gift_card_amount, O.total AS ccamt, " +
                $"P.transaction_auth_code AS tran_approval_code, SPACE(20) AS confirmed " +
                $"FROM altashop.orders AS O " +
                $"INNER JOIN altashop.order_items AS I ON O.id = I.order_id " +
                $"LEFT JOIN altashop.payments AS P ON P.order_id = O.ID " +
                $"LEFT JOIN altashop.axess_transactions AS T ON T.order_item_id=I.id " +
                $"WHERE I.created_at >= '{sStart}' AND I.created_at < '{mStart.AddDays(1).ToString(Mirror.AxessDateFormat)}'";
            WebSales = CF.LoadTable(AltaShop.Shop_Alta_ComConn, Query, "WebSales");
        }

        private void GetCreditUse() => AltaCredit = CF.LoadTable(BUY.Buy_Alta_ComConn, $"SELECT A.creditkey, A.customerkey, A.ecoupon, " +
            $"A.type, A.season, A.transactionamount AS amtused, A.pos, A.transaction, A.transactiondate, A.dateexpires, A.reason, A.memo, " +
            $"A.acct, A.masterflag, A.active, C.fname, C.lname " +
            $"FROM axessutility.altacredit2 AS A " +
            $"INNER JOIN axessutility.creditcustomers AS C ON A.customerkey=C.customerkey " +
            $"WHERE A.transactiondate='{mStart.ToString(Mirror.AxessDateFormat)}' AND A.pos=20", "AltaCredit");

        private void SetLabelStatus(bool Enable)
        {
            lblAMEX.Enabled = Enable;
            lblAuthNetGiftCardUse.Enabled = Enable;
            lblAuthNetRefunds.Enabled = Enable;
            lblAuthNetTotal.Enabled = Enable;
            lblAxessOrders.Enabled = Enable;
            lblAxessPrepaidsReloads.Enabled = Enable;
            lblBackInWillcall.Enabled = Enable;
            lblCancelledByAxess.Enabled = Enable;
            lblCreditCards.Enabled = Enable;
            lblCreditUses.Enabled = Enable;
            lblDateSpecificReloads.Enabled = Enable;
            lblDiscover.Enabled = Enable;
            lblECoupons.Enabled = Enable;
            lblEstoreReceipts.Enabled = Enable;
            lblEstoreSales.Enabled = Enable;
            lblFamilyPackages.Enabled = Enable;
            lblGiftCards.Enabled = Enable;
            lblGiftCardUse.Enabled = Enable;
            lblMastercard.Enabled = Enable;
            lblOverShort.Enabled = Enable;
            lblPassCardOrders.Enabled = Enable;
            lblPrepaidsReloads.Enabled = Enable;
            lblRCBillingTotal.Enabled = Enable;
            lblReceivedByAxess21.Enabled = Enable;
            lblSalesTotal.Enabled = Enable;
            lblSentFromWillcall.Enabled = Enable;
            lblSettledReceipts.Enabled = Enable;
            lblShipping.Enabled = Enable;
            lblSkiSchoolSales.Enabled = Enable;
            lblTicketReservations.Enabled = Enable;
            lblVISA.Enabled = Enable;
            lblWillcallDateSpecificReloads.Enabled = Enable;
            lblWillcallTicketReservations.Enabled = Enable;
        }

        private void SetTextboxStatus(bool Enable)
        {
            txtAMEX.Enabled = Enable;
            txtAuthNetGiftCardUse.Enabled = Enable; ;
            txtAuthNetRefunds.Enabled = Enable;
            txtAuthNetTotal.Enabled = Enable;
            txtAxessOrders.Enabled = Enable;
            txtAxessPrepaidsReloads.Enabled = Enable;
            txtBackInWillcall.Enabled = Enable;
            txtCancelledByAxess.Enabled = Enable;
            txtCreditCards.Enabled = Enable;
            txtCreditUses.Enabled = Enable;
            txtDateSpecificReloads.Enabled = Enable;
            txtDiscover.Enabled = Enable;
            txtECoupons.Enabled = Enable;
            txtEstoreReceipts.Enabled = Enable;
            txtEstoreSales.Enabled = Enable;
            txtFamilyPackages.Enabled = Enable;
            txtGiftCards.Enabled = Enable;
            txtGiftCardUse.Enabled = Enable;
            txtMastercard.Enabled = Enable;
            txtOverShort.Enabled = Enable;
            txtPassCardOrders.Enabled = Enable;
            txtPrepaidsReloads.Enabled = Enable;
            txtRCBillingTotal.Enabled = Enable;
            txtReceivedByAxess21.Enabled = Enable;
            txtSalesTotal.Enabled = Enable;
            txtSentFromWillcall.Enabled = Enable;
            txtSettledReceipts.Enabled = Enable;
            txtShipping.Enabled = Enable;
            txtSkiSchoolSales.Enabled = Enable;
            txtTicketReservations.Enabled = Enable;
            txtVISA.Enabled = Enable;
            txtWillcallDateSpecificReloads.Enabled = Enable;
            txtWillcallTicketReservations.Enabled = Enable;
        }

        private void SetButtonStatus(bool Enable)
        {
            btnAbort.Enabled = Enable;
            btnAuthNetRefunds.Enabled = Enable;
            btnAxessOrders.Enabled = Enable;
            btnAxessPrepaidsReloads.Enabled = Enable;
            btnCreditUses.Enabled = Enable;
            btnDateSpecificReloads.Enabled = Enable;
            btnEdit.Enabled = Enable;
            btnErrorsReport.Enabled = Enable;
            btnEstoreDetail.Enabled = Enable;
            btnExit.Enabled = Enable;
            btnFamilyPackages.Enabled = Enable;
            btnGiftCards.Enabled = Enable;
            btnLoadData.Enabled = Enable;
            btnPassCardOrders.Enabled = Enable;
            btnPrepaidsReloads.Enabled = Enable;
            btnPreviousDay.Enabled = Enable;
            btnRCBillingTotal.Enabled = Enable;
            btnSave.Enabled = Enable;
            btnSkiSchoolSales.Enabled = Enable;
            btnSummaryReport.Enabled = Enable;
            btnTicketReservations.Enabled = Enable;
            btnWillcallDateSpecificReloads.Enabled = Enable;
            btnWillcallTicketReservations.Enabled = Enable;
        }

        private void ClrFields()
        {
            mMAST = 0;
            txtMastercard.Text = mMAST.ToString(DFormat);
            mVISA = 0;
            txtVISA.Text = mVISA.ToString(DFormat);
            mDisc = 0;
            txtDiscover.Text = mDisc.ToString(DFormat);
            mAMEX = 0;
            txtAMEX.Text = mAMEX.ToString(DFormat);
            mCardTot = 0;
            txtAuthNetTotal.Text = mCardTot.ToString(DFormat);
            mReloadTot = 0;
            txtAxessPrepaidsReloads.Text = mReloadTot.ToString(DFormat);
            //maxsales = 0;
            //maxorders = 0;
            mwebsales = 0;
            txtEstoreSales.Text = mwebsales.ToString(DFormat);
            //mweborders = 0;
            //mwebreloads = 0;
            //mwebretail = 0;
            //mwebretailreload = 0;
            //mShipTot = 0;
            mShort = 0;
            txtOverShort.Text = mShort.ToString(DFormat);
            mwebtotal = 0;
            mTotRec = 0;
            mwebcoupon = 0;
            txtECoupons.Text = mwebcoupon.ToString(DFormat);
            mwebauthnet = 0;
            txtCreditCards.Text = string.Empty;
            //mfamcredits = 0;
            //mgiftsprdcd = 0;
            mgiftloads = 0;
            mgiftuse = 0;
            txtGiftCardUse.Text = mgiftuse.ToString(DFormat);
            //mpassorders = 0;
            //txtPassCardOrders.Text = mpassorders.ToString(DFormat);
            mpassreloads = 0;
            mskischool = 0;
            mwebrefund = 0;
            mwebgift = 0;
            mrcbillamt = 0;
            msalestot = 0;
            mtickn = 0;
            mtickr = 0;
            mAltaCredit = 0;
            txtCreditUses.Text = mAltaCredit.ToString(DFormat);
            //maxpassorders = 0;
            //txtAxessOrders.Text = maxpassorders.ToString(DFormat);
            maxpassreloads = 0;
            mwcnew = 0;
            txtWillcallTicketReservations.Text = mwcnew.ToString(DFormat);
            mwcreload = 0;
            txtWillcallDateSpecificReloads.Text = mwcreload.ToString(DFormat);
            //mOrderID = string.Empty;
            //mqty = 0;
            //mprice = 0;
            mLastName = string.Empty;
            mFirstName = string.Empty;
            mDOB = CF.defaultDOB;
            //mhfeet = 0;
            //mhinches = 0;
            mWebID = string.Empty;
            mProduct = string.Empty;
            mType = string.Empty;
            maxrecd = 0;
            txtReceivedByAxess21.Text = maxrecd.ToString(DFormat);
            mwcsent = 0;
            txtSentFromWillcall.Text = mwcsent.ToString(DFormat);
            maxcanc = 0;
            txtCancelledByAxess.Text = maxcanc.ToString(DFormat);
            mAXTot = 0;
            txtEstoreReceipts.Text = mAXTot.ToString(DFormat);
            mwccredit = 0;
            txtBackInWillcall.Text = mwccredit.ToString(DFormat);
            System.Windows.Forms.Application.DoEvents();
        }

        private void LoadFields()
        {
            mMAST = CF.Null2Val(POS20Sum.Field<decimal?>("cmast"));
            txtMastercard.Text = mMAST.ToString(DFormat);
            mVISA = CF.Null2Val(POS20Sum.Field<decimal?>("cvisa"));
            txtVISA.Text = mVISA.ToString(DFormat);
            mDisc = CF.Null2Val(POS20Sum.Field<decimal?>("cdisc"));
            txtDiscover.Text = mDisc.ToString(DFormat);
            mAMEX = CF.Null2Val(POS20Sum.Field<decimal?>("camex"));
            txtAMEX.Text = mAMEX.ToString(DFormat);
            mCardTot = CF.Null2Val(POS20Sum.Field<decimal?>("cardtot"));
            txtAuthNetTotal.Text = mCardTot.ToString(DFormat);
            mgiftuse = CF.Null2Val(POS20Sum.Field<decimal?>("cgiftuse"));
            txtAuthNetGiftCardUse.Text = mgiftuse.ToString(DFormat);
            mgiftloads = CF.Null2Val(POS20Sum.Field<decimal?>("cgiftadd"));

            mTotRec = CF.Null2Val(POS20Sum.Field<decimal?>("totrec"));
            txtSettledReceipts.Text = mTotRec.ToString(DFormat);
            mpassreloads = CF.Null2Val(POS20Sum.Field<decimal?>("webreloads"));
            txtPrepaidsReloads.Text = mpassreloads.ToString(DFormat);
            //mpassorders = CF.Null2Val(POS20Sum.Field<decimal?>("weborders"));
            //txtPassCardOrders.Text = mpassorders.ToString(DFormat);
            mskischool = CF.Null2Val(POS20Sum.Field<decimal?>("webskisch"));
            txtSkiSchoolSales.Text = mskischool.ToString(DFormat);
            mfampck = CF.Null2Val(POS20Sum.Field<decimal?>("webfampck"));
            txtFamilyPackages.Text = mfampck.ToString(DFormat);
            mtickn = CF.Null2Val(POS20Sum.Field<decimal?>("webtickn"));
            txtTicketReservations.Text = mtickn.ToString(DFormat);
            mtickr = CF.Null2Val(POS20Sum.Field<decimal?>("webtickr"));
            txtDateSpecificReloads.Text = mtickr.ToString(DFormat);
            mwebgift = CF.Null2Val(POS20Sum.Field<decimal?>("webgift"));
            txtGiftCards.Text = mwebgift.ToString(DFormat);
            mwebrefund = CF.Null2Val(POS20Sum.Field<decimal?>("webrefund"));
            txtAuthNetRefunds.Text = mwebrefund.ToString(DFormat);
            mrcbillamt = CF.Null2Val(POS20Sum.Field<decimal?>("ccbillamt"));
            txtRCBillingTotal.Text = mrcbillamt.ToString(DFormat);
            mwebship = CF.Null2Val(POS20Sum.Field<decimal?>("webship"));
            txtShipping.Text = mwebship.ToString(DFormat);
            mAltaCredit = CF.Null2Val(POS20Sum.Field<decimal?>("acredit"));
            txtCreditUses.Text = mAltaCredit.ToString(DFormat);
            msalestot = CF.Null2Val(POS20Sum.Field<decimal?>("axtot"));
            txtSalesTotal.Text = msalestot.ToString(DFormat);
            mReloadDisc = CF.Null2Val(POS20Sum.Field<decimal?>("axwebdisc"));
            //maxsales = 0;
            System.Windows.Forms.Application.DoEvents();
        }

        //private void BtnPreviousDay_Click(object sender, EventArgs e) => dtpWorkDate.Value = dtpWorkDate.Value.AddDays(-1);

        //private void BtnNextDay_Click(object sender, EventArgs e) => dtpWorkDate.Value = dtpWorkDate.Value.AddDays(1);

        private void BtnSummaryReport_Click(object sender, EventArgs e)
        {
            //THISFORM.SAVEFIELDS21

            if (POS21Sum == null)
            {

            }
            SetButtonStatus(true);
            btnSave.Enabled = false;
            btnAbort.Enabled = false;
            SetTextboxStatus(false);
            btnSummaryReport.Enabled = true;
            btnEstoreDetail.Enabled = true;
            btnExit.Enabled = true;
            //btnErrorsReport.Visible = (mpassorders != maxpassorders || mpassreloads != maxpassreloads);
            btnErrorsReport.Visible = (mpassreloads != maxpassreloads);


            //SELECT POSSUM
            //SET FILTER TO SALEDATE = MSTART AND pos = 20
            //GO TOP
            //REPORT FORM estoresum preview
            //SET FILTER TO

            //** print web store error report
            //SELECT websales
            btnExit.Focus();
        }

        private void SaveFields()
        {
            string tQuery = string.Empty;
            if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.possum", $"saledate='{sStart}'"))
            {
                //update record
                tQuery = $"UPDATE {DW.ActiveDatabase}.possum SET cmast={mMAST.ToString()}, cvisa = {mVISA.ToString()}, cdisc = {mDisc.ToString()}, camex = {mAMEX.ToString()}, " +
                    $"cardtot = {mCardTot.ToString()}, cgiftuse = {mgiftuse.ToString()}, cgiftadd = {mgiftloads.ToString()}, cardcheck = {mCardTot.ToString()}, " +
                    $"acredit = {mAltaCredit.ToString()}, webship = {mwebship.ToString()}, webreloads = {mpassreloads.ToString()}, " +
                    $"webSKISCH = {mskischool.ToString()}, webfampck = {mfampck.ToString()}, webgift = {mwebgift.ToString()}, webrefund = {mwebrefund.ToString()}, " +
                    $"ccbillamt = {mrcbillamt.ToString()}, webtickn = {mtickn.ToString()}, webtickr = {mtickr.ToString()}, webtot = {msalestot.ToString()}, " +
                    $"axreloads = {maxpassreloads.ToString()}, axwebdisc = {mReloadDisc.ToString()}, trretail = {mtrretail.ToString()}, " +
                    $"trwillcall ={(mwcreload + mwcnew).ToString()}, vcwillcall = {mvcwillcall.ToString()}, axcard = {mwebauthnet.ToString()}, " +
                    $"axcredit = {mwebcoupon.ToString()}, axgift = {mwebgiftuse.ToString()}, axtot = {mAXTot.ToString()}, axsales = {msalestot.ToString()}, ccsettled = '{sStart}' " +
                    $"WHERE saledate = '{sStart}'";

                //tQuery =$"UPDATE {DW.ActiveDatabase}.possum SET cmast={mMAST.ToString()}, cvisa = {mVISA.ToString()}, cdisc = {mDisc.ToString()}, camex = {mAMEX.ToString()}, " +
                //    $"cardtot = {mCardTot.ToString()}, cgiftuse = {mgiftuse.ToString()}, cgiftadd = {mgiftloads.ToString()}, cardcheck = {mCardTot.ToString()}, " +
                //    $"acredit = {mAltaCredit.ToString()}, webship = {mwebship.ToString()}, weborders = {mpassorders.ToString()}, webreloads = {mpassreloads.ToString()}, " +
                //    $"webSKISCH = {mskischool.ToString()}, webfampck = {mfampck.ToString()}, webgift = {mwebgift.ToString()}, webrefund = {mwebrefund.ToString()}, " +
                //    $"ccbillamt = {mrcbillamt.ToString()}, webtickn = {mtickn.ToString()}, webtickr = {mtickr.ToString()}, webtot = {msalestot.ToString()}, " +
                //    $"axorders = {maxpassorders.ToString()}, axreloads = {maxpassreloads.ToString()}, axwebdisc = {mRelaodDisc.ToString()}, trretail = {mtrretail.ToString()}, " +
                //    $"trwillcall ={(mwcreload + mwcnew).ToString()}, vcwillcall = {mvcwillcall.ToString()}, axcard = {mwebauthnet.ToString()}, " +
                //    $"axcredit = {mwebcoupon.ToString()}, axgift = {mwebgiftuse.ToString()}, axtot = {mAXTot.ToString()}, axsales = {msalestot.ToString()}, ccsettled = '{sStart}' " +
                //    $"WHERE saledate = '{sStart}'";
            }
            else
            {
                //insert record
                tQuery = $"INSERT {DW.ActiveDatabase}.possum (saledate, cmast, cvisa, cdisc, camex, cardtot, cgiftuse, cgiftadd, cardcheck, acredit, webship, weborders, " +
                    $"webreloads, webSKISCH, webfampck, webgift, webrefund, ccbillamt, webtickn, webtickr, webtot, axorders, axreloads, axwebdisc, trretail, " +
                    $"trwillcall, vcwillcall, axcard, axcredit, axgift, axtot, axsales, ccsettled) VALUES ('{sStart}', {mMAST.ToString()}, {mVISA.ToString()}, " +
                    $"{mDisc.ToString()}, {mAMEX.ToString()}, {mCardTot.ToString()}, {mgiftuse.ToString()}, {mgiftloads.ToString()}, {mCardTot.ToString()}, " +
                    $"{mAltaCredit.ToString()}, {mwebship.ToString()}, 0, {mpassreloads.ToString()}, {mskischool.ToString()}, " +
                    $"{mfampck.ToString()}, {mwebgift.ToString()}, {mwebrefund.ToString()}, {mrcbillamt.ToString()}, {mtickn.ToString()}, {mtickr.ToString()}, " +
                    $"{msalestot.ToString()}, 0, {maxpassreloads.ToString()}, {mReloadDisc.ToString()}, {mtrretail.ToString()}, " +
                    $"{(mwcreload + mwcnew).ToString()}, {mvcwillcall.ToString()}, {mwebauthnet.ToString()}, {mwebcoupon.ToString()}, {mwebgiftuse.ToString()}, " +
                    $"{mAXTot.ToString()}, {msalestot.ToString()}, '{sStart}')";
                //tQuery = $"INSERT {DW.ActiveDatabase}.possum (saledate, cmast, cvisa, cdisc, camex, cardtot, cgiftuse, cgiftadd, cardcheck, acredit, webship, weborders, " +
                //    $"webreloads, webSKISCH, webfampck, webgift, webrefund, ccbillamt, webtickn, webtickr, webtot, axorders, axreloads, axwebdisc, trretail, " +
                //    $"trwillcall, vcwillcall, axcard, axcredit, axgift, axtot, axsales, ccsettled) VALUES ('{sStart}', {mMAST.ToString()}, {mVISA.ToString()}, " +
                //    $"{mDisc.ToString()}, {mAMEX.ToString()}, {mCardTot.ToString()}, {mgiftuse.ToString()}, {mgiftloads.ToString()}, {mCardTot.ToString()}, " +
                //    $"{mAltaCredit.ToString()}, {mwebship.ToString()}, {mpassorders.ToString()}, {mpassreloads.ToString()}, {mskischool.ToString()}, " +
                //    $"{mfampck.ToString()}, {mwebgift.ToString()}, {mwebrefund.ToString()}, {mrcbillamt.ToString()}, {mtickn.ToString()}, {mtickr.ToString()}, " +
                //    $"{msalestot.ToString()}, {maxpassorders.ToString()}, {maxpassreloads.ToString()}, {mRelaodDisc.ToString()}, {mtrretail.ToString()}, " +
                //    $"{(mwcreload + mwcnew).ToString()}, {mvcwillcall.ToString()}, {mwebauthnet.ToString()}, {mwebcoupon.ToString()}, {mwebgiftuse.ToString()}, " +
                //    $"{mAXTot.ToString()}, {msalestot.ToString()}, '{sStart}')";
            }
            CF.ExecuteSQL(DW.dwConn, tQuery);
        }

        private void SaveFields21()
        {
            string tQuery = string.Empty;
            if (!CF.RowExists(DW.dwConn, $""))
            {
                tQuery = $"INSERT INTO {DW.ActiveDatabase}.possum (saledate, pos, office, cash, cvisa, cmast, cdisc, camex, cgiftuse, cgiftadd, cardcheck, cardtot, " +
                    $"gcertqty, gcertamt, vchrqty, vchramt, snchk, arqty, aramt, arguest, acredit, webamt, totrec, axcash, axcard, axvchr, axsnchk, axcredit, axar, " +
                    $"axargst, axweb, axgift, axtot, webship, weborders, webreloads, webtickr, webtickn, webskisch, webgift, webrefund, ccbillamt, webfampck, webtot, " +
                    $"axorders, axreloads, axwebdisc, trretail, trwillcall, vcwillcall, axsales, notes, cashier1, cashier2, cashier3, cissued, creturn, cused, axused, " +
                    $"balflag, saledetot, emflag, ccsettled, crefno, vrefno, arefno, salestot, pmttot, artrantot, poskey, ccbatchnbr) VALUES " +
                    $"'{sStart}', 21, 'WEB', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0, 0, {(maxrecd + maxcanc).ToString()}, " +
                    $"0, 0, 0, {(mwcsent + mwccredit).ToString()}, 0, 0, 0, 0, 0, 0, {(mwcsent + mwccredit).ToString()}, 0, {(maxrecd + maxcanc).ToString()}, 0, " +
                    $"0, 0, 0, 0, null, '','', '', 0, 0, 0, 0, 0, 0, 0, null, '', '', '', 0, 0, 0, '', 0)";
            }
            else
            {
                tQuery = $"UPDATE {DW.ActiveDatabase}.possum SET axreloads = {(maxrecd + maxcanc).ToString()}, webtickr={(mwcsent + mwccredit).ToString()}, webtot={(mwcsent + mwccredit).ToString()}, axtot = {(maxrecd + maxcanc).ToString()} WHERE saledate={sStart} AND pos=21";
            }
            CF.ExecuteSQL(DW.dwConn, tQuery);
        }

        private void Update_ARTrans()
        {
            string tQuery = $"SELECT * FROM {DW.ActiveDatabase}.artrans WHERE transdate = '{sStart}' AND posnbr IN (20, 21) ORDER BY transkey";
            ARTransXT = CF.LoadTable(DW.dwConn, tQuery, "ARTransXT");
            //*** update artrans with will call Ticket Sales(both new & reload) ***
            Willcall = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.willcall WHERE bookdate='{sStart}' AND company='DT'", "Willcall");
            foreach (DataRow WillcallRow in Willcall.Rows)
            {
                string mTransKey = $"20-{WillcallRow["res_no"].ToString()}";
                decimal mCreditAmt = 0;
                decimal mDebitAmt = 0;
                if (WillcallRow.Field<decimal>("resprice") > 0)
                {
                    mCreditAmt = WillcallRow.Field<decimal>("resprice");
                }
                else
                {
                    mDebitAmt = WillcallRow.Field<decimal>("resprice");
                }
                string lARID = "1179";
                string lARName = "WEB TICKETS";
                string mFName = CF.EscapeChar(WillcallRow["firstname"].ToString());
                string mLName = CF.EscapeChar(WillcallRow["lastname"].ToString());
                if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.artrans", $"transdate = '{sStart}' AND posnbr IN (20, 21) AND transkey='{mTransKey}'"))
                {
                    //*** insert WC transactions into AR trans***
                    tQuery = $"INSERT INTO {DW.ActiveDatabase}.artrans (transdate, posnbr, transkey,transtype,arcustid,arname,firstname, lastname, transdesc, " +
                        $"debitamt, creditamt, authcode, billdate, resno, dtupdate) " +
                        $"Values ('{sStart}', 20, '{mTransKey}', 'T', {lARID}, '{lARName}', '{mFName}', '{mLName}', " +
                        $"'{WillcallRow["tickdesc"].ToString()}-{WillcallRow["persdesc"].ToString()}', {mDebitAmt.ToString("0#.00")}, {mCreditAmt.ToString("0#.00")}, " +
                        $"'{sStart}', '{WillcallRow["res_no"].ToString()}','{DateTime.Now.ToString(Mirror.AxessDateFormat)}')";
                    CF.ExecuteSQL(DW.dwConn, tQuery);
                }
            }
            //*** update artrans with authnet billings ***
            if (mrcbillamt != 0)
            {
                decimal mcreditamt = 0;
                decimal mdebitamt = 0;
                if (mrcbillamt > 0)
                {
                    mcreditamt = mrcbillamt;
                }
                else
                {
                    mdebitamt = mrcbillamt;
                }
                string mTransKey = $"20-{sStart}-RC";
                string tRecID = CF.GetSQLField(DW.dwConn, $"SELECT recid FROM {DW.ActiveDatabase}.artrans WHERE transdate = '{sStart}' AND posnbr IN (20, 21) AND transkey='{mTransKey}' LIMIT 1");
                if (tRecID != null)
                {
                    tQuery = $"UPDATE {DW.ActiveDatabase}.artrans SET debitamt={mdebitamt.ToString("0#.00")}, creditamt={mcreditamt.ToString("0#.00")}, dtupdate='{DateTime.Now.ToString(Mirror.AxessDateFormat)}' WHERE recid='{tRecID}'";
                }
                else
                {
                    //*** insert RESORT CHARGE RECEIPTS into AR trans ***
                    tQuery = $"INSERT INTO {DW.ActiveDatabase}.artrans (transdate, posnbr, transkey, transtype, arcustid, arname, transdesc, debitamt, creditamt, " +
                        $"authcode, billdate, dtupdate) " +
                        $"Values ('{sStart}', 20,'{mTransKey}', 'E', '11874', 'ASL - PASS CHARGES', 'AUTHNET BILLING', {mdebitamt.ToString("0#.00")}, {mcreditamt.ToString("0#.00")}, '11874', '{sStart}', '{DateTime.Now.ToString(Mirror.AxessDateFormat)}')";
                }
                CF.ExecuteSQL(DW.dwConn, tQuery);
            }
            //*** update artrans with gift cards ***
            foreach (DataRow WebSale in WebSales.Rows)
            {
                //SET FILTER TO ALLTRIM(websales.status) = "APPROVED" AND ALLTRIM(websales.product_NAME) = 'Alta Gift Card'
                if (WebSale["status"].ToString() == "APPROVED" && WebSale["product_name"].ToString() == "Alta Gift Card")
                {
                    string mTransKey = $"20-{WebSale["itemid"].ToString()}";
                    decimal mcreditamt = 0;
                    decimal mdebitamt = 0;
                    if (WebSale.Field<decimal>("price") > 0)
                    {
                        mcreditamt = WebSale.Field<decimal>("price");
                    }
                    else
                    {
                        mdebitamt = WebSale.Field<decimal>("price");
                    }
                    string mFName = CF.EscapeChar(WebSale["billing_firstname"].ToString());
                    string mLName = CF.EscapeChar(WebSale["billing_lastname"].ToString());
                    if (!CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.artrans", $"transkey={mTransKey} AND  transdate = '{sStart}' AND posnbr IN (20, 21)"))
                    {
                        //*** insert WC totals into AR trans***
                        tQuery = $"INSERT INTO {DW.ActiveDatabase}.artrans (transdate, posnbr, transkey, transtype, arcustid, arname, firstname, lastname, transdesc, debitamt, creditamt, authcode, billdate, resno, dtupdate) Values( '" +
                            $"{sStart}', 20, '{mTransKey}', 'E', '11759', 'ESTORE GIFT CARDS', '{mFName}','{mLName}','{WebSale["product_name"].ToString()}', {mdebitamt.ToString("0#.00")}, {mcreditamt.ToString("0#.00")}, '11759', '{sStart}', " +
                            $"'{WebSale["itemid"].ToString()}', '{DateTime.Now.ToString(Mirror.AxessDateFormat)}')";
                        CF.ExecuteSQL(DW.dwConn, tQuery);
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            CalcTot();
            SaveFields();
            Update_ARTrans();
            SaveFields21();
            SetButtonStatus(true);
            btnSave.Enabled = false;
            btnAbort.Enabled = false;
            SetTextboxStatus(false);
            dtpWorkDate.Enabled = true;
            btnSummaryReport.Enabled = true;
            btnEstoreDetail.Enabled = true;
            btnExit.Enabled = true;
            //if (mpassorders != maxpassorders || mpassreloads != maxpassreloads)
            if (mpassreloads != maxpassreloads)
            {
                WebErrors();
                btnErrorsReport.Visible = true;
            }
            else
            {
                btnErrorsReport.Visible = false;
            }
            EnableRpts();
        }

        private void WebErrors()
        {
            //save AXReloads
            //SELECT orders
            //SET ORDER TO EXORDERID && EXORDERID
            //SELECT salesday
            //SET FILTER TO saledate = mstart AND(nkassanr = 20)
            //INDEX on ALLTRIM(serialkey) TO serialkey
            //SET ORDER TO serialkey   && Serial Key
            //SET DELETED ON

            //*****FIND ITEMS IN SUMO, BUT NOT IN AXESS ****
            //string mrpttxt = "The below orders and/or reloads did not make it into Axess";
            //SELECT websales
            //SET FILTER TO ALLTRIM(websales.status) = "APPROVED" AND nserialno > 0
            foreach (DataRow WebSale in WebSales.Rows)
            {
                string loid = WebSale["itemid"].ToString();
                string lskey = WebSale["nposno"].ToString() + " - " + WebSale["nserialno"].ToString();
                //* * if SP order**
                if (WebSale["product_type"].ToString() != "reload" && WebSale["product_group"].ToString().Substring(0, 6) == "season" && WebSale.Field<int>("tickettype") != 123)
                {
                    if (!CF.RowExists(am.MirrorConn, "tabaxorderlist", $"exorderid='{loid}'"))
                    {
                        WebSale.SetField<string>("confirmed", "NOT IN AXESS");
                    }

                }
                //** if SP Reload**
                if (WebSale["product_type"].ToString() == "reload" && WebSale["product_group"].ToString().Substring(0, 6) == "season" && WebSale.Field<int>("tickettype") != 123)
                {
                    if (!CF.RowExists(DW.dwConn, "salesdata", $"nseriennr={WebSale["nserialno"].ToString()}"))
                    {
                        WebSale.SetField<string>("confirmed", "NOT IN AXESS");
                    }

                }
            }

            foreach (DataRow WebSale in WebSales.Rows)
            {
                if (WebSale["confirmed"].ToString() == "NOT IN AXESS")
                {
                    string litem = WebSale["itemid"].ToString();
                    string lskey = (100 + WebSale.Field<int>("nposno")).ToString() + WebSale["nserialno"].ToString() + '1';
                    string tWhere = string.Empty;
                    if (WebSale["product_group"].ToString().Substring(0, 6) == "season" && WebSale.Field<int>("tickettype") != 123)
                    {
                        if (WebSale["product_type"].ToString() != "reload") //orders
                        {
                            tWhere = $"eitemid={litem.ToString()}";
                        }
                        else    //reloads
                        {
                            tWhere = $"eskey='{lskey}'";
                        }
                        if (!CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.weberrors", $"{tWhere} AND esdate='{sStart}'"))
                        {
                            //string tQuery = $"UPDATE {DW.ActiveDatabase}.weberrors SET esdate = {WebSale.Field<DateTime>("sale_date")}, " +
                            //    $"eorderid = {WebSale.Field<int>("orderid").ToString()}, eitemid = {WebSale.Field<int>("itemid").ToString()}, " +
                            //    $"eskey = '{(100 + WebSale.Field<int>("nposno")).ToString()}{WebSale["nserialno"].ToString()}1', " +
                            //    $"efname = '{WebSale["firstname"].ToString()}', elname = '{WebSale["lastname"].ToString()}', " +
                            //    $"eproduct = '{WebSale["product_name"].ToString()}', eserialno = {WebSale.Field<int>("nserialno")}, eprice = {WebSale.Field<decimal>("price")})";
                            string tQuery = $"INSERT {DW.ActiveDatabase}.weberrors (esdate, eorderid, eitemid, eskey, efname, elname, eproduct, eserialno, eprice) " +
                                $"VALUES ({WebSale.Field<DateTime>("sale_date")}, {WebSale.Field<int>("orderid").ToString()}, {WebSale.Field<int>("itemid").ToString()}, " +
                                $"'{(100 + WebSale.Field<int>("nposno")).ToString()}{WebSale["nserialno"].ToString()}1', '{WebSale["firstname"].ToString()}', elname = '{WebSale["lastname"].ToString()}', " +
                                $"'{WebSale["product_name"].ToString()}', {WebSale.Field<int>("nserialno")}, {WebSale.Field<decimal>("price")})";
                            CF.ExecuteSQL(DW.dwConn, tQuery);
                        }
                    }
                }
            }
            //***** FIND ITEMS IN AXESS, BUT NOT IN SUMO ****
            //SELECT websales
            //INDEX on itemid TO websales
            //   ***REVIEW AXESS ORDERS ***
            //foreach (DataRow Order in AXOrders.Rows)  //should be Orders, not AXOrders. //todo
            //{
            //    string lid = Order["exorderid"].ToString();
            //    if (!CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.salesdata", $"itemid='{lid}'"))
            //    {
            //        Order.SetField<string>("transkey", "NOT IN SUM");
            //        string tQuery = $"INSERT {DW.ActiveDatabase}.weberrors (axsdate, axorderid, eitemid, axexitemid, axfname, axlname, axticktype, eserialno, axprice) " +
            //            $"VALUES ({Order.Field<DateTime>("issue_date")}, {Order.Field<int>("norderid").ToString()}, {lid}, " +
            //            $"'{Order["exorderid"].ToString()}', '{Order["fname"].ToString()}', elname = '{Order["lname"].ToString()}', " +
            //            $"'{Order["tickdesc"].ToString()}', '{Order["persdesc"].ToString()}', {Order.Field<decimal>("tariff")})";
            //        CF.ExecuteSQL(DW.dwConn, tQuery);
            //    }
            //}

            //*** REVIEW AXESS RELOADS ***
            System.Data.DataTable AXReloads = CF.LoadTable(DW.dwConn, $"SELECT saledate, nkassanr, nseriennr, fname,  lname, nkartennr, tickdesc, persdesc, tariff, SPACE(20) as confirmed FROM {DW.ActiveDatabase}.salesdata WHERE saledate = '{sStart}'", "AXReloads");
            foreach (DataRow AXReload in AXReloads.Rows)
            {
                //        lskey = $"{AXReload["nkassanr"].ToString()}-{AXReload["nseriennr"].ToString()}";
                //        if (!CF.RowExists(am.MirrorConn, $"{am.ActiveDatabase}.taborderlist WHERE created = '{sStart}' AND "))
                //        SELECT websales
                //    IF SEEK(lskey)
                //    ELSE
                //        SELECT axreloads
                //        replace confirmed WITH 'NOT IN SUMO DATA'
                //    endif
                //    SELECT axreloads
                //    }
                //SELECT weberrors
                //SET ORDER TO eskey
                //SELECT axreloads
                //SET FILTER TO confirmed = 'NOT IN SUMO DATA'
                //GO top
                //IF EOF()
                //ELSE
                //    DO WHILE NOT EOF()
                //        lskey = ALLTRIM(STR(axreloads.nkassanr)) + '-' + ALLTRIM(STR(axreloads.nseriennr))
                //        SELECT weberrors
                //        IF SEEK(lskey)
                //        ELSE
                //            APPEND blank
                //        ENDIF
                //        replace eskey WITH lskey
                //        replace axsdate WITH axreloads.saledate
                //        replace axskey WITH ALLTRIM(STR(axreloads.nkassanr)) + '-' + ALLTRIM(STR(axreloads.nseriennr))
                //        replace axfname WITH axreloads.fname
                //        replace axlname WITH axreloads.lname
                //        replace axticktype WITH axreloads.tickdesc
                //        replace axperstype WITH axreloads.persdesc
                //        replace axprice WITH axreloads.tariff
                //        SELECT axreloads
                //        skip
                //    enddo()
                //ENDIF
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            txtVISA.Enabled = true;
            txtMastercard.Enabled = true;
            txtAMEX.Enabled = true;
            txtDiscover.Enabled = true;
            SetButtonStatus(false);
            btnSave.Enabled = true;
            btnAbort.Enabled = true;
            txtVISA.Focus();
        }

        private void BtnAbort_Click(object sender, EventArgs e)
        {
            ClrFields();
            SetButtonStatus(true);
            btnSave.Enabled = false;
            btnAbort.Enabled = false;
            SetTextboxStatus(false);
            dtpWorkDate.Enabled = true;
        }

        private void DtpWorkDate_ValueChanged(object sender, EventArgs e)
        {
            mStart = dtpWorkDate.Value;
            sStart = mStart.ToString(Mirror.AxessDateFormat);
            LoadData();
        }
        private void WebsalesReport(DataRow[] WebSalesX, string Title = "")
        {
            Cursor = Cursors.WaitCursor;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            string t = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string r = System.AppDomain.CurrentDomain.BaseDirectory;
            Workbook wb = excel.Workbooks.Open(r + @"\reports\WebSales.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"EStore Sales Report{(Title == string.Empty ? "": $" - {Title}").ToString()}\r\n{sStart}";
            int CurRow = 2;
            foreach (DataRow WebSaleX in WebSalesX)
            {
                string WebID = WebSaleX["WebID"].ToString().Trim();
                WebID = WebID.Length < 8 ? WebSaleX["ItemID"].ToString() : WebID.Substring(0,8);
                ws.Cells[CurRow, 1] = WebSaleX["OrderID"].ToString();
                ws.Cells[CurRow, 2] = WebID;
                ws.Cells[CurRow, 3] = WebSaleX["product_name"].ToString();
                ws.Cells[CurRow, 4] = WebSaleX["firstname"].ToString() + " " + WebSaleX["lastname"].ToString();
                ws.Cells[CurRow, 5] = WebSaleX["njournalno"].ToString();
                ws.Cells[CurRow, 6] = WebSaleX.Field<decimal>("price").ToString("0#.00");
                ws.Cells[CurRow, 7] = WebSaleX.Field<int>("qty").ToString();
                ws.Cells[CurRow++, 8] = WebSaleX.Field<decimal>("total").ToString("0#.00");
            }
            excel.Visible = true;
            ws.PrintOutEx(Type.Missing, Type.Missing, 1, true);
            excel.Visible = false;
            wb.Close(false);
            Cursor = Cursors.Default;
        }

        private void BtnPassCardOrders_Click(object sender, EventArgs e)
        {
            //order, probably going away
            DataRow[] WebSale = WebSales.Select("(status='APPROVED' AND product_type!='reload' AND ((SUBSTR(websales.product_group,1,6) = 'season' AND ticketype <> 123) OR tickettype=99)");
            WebsalesReport(WebSale, "Pass Card Orders");
        }

        private void BtnPrepaidsReloads_Click(object sender, EventArgs e)
        {
            DataRow[] WebSale = WebSales.Select("status IN ('APPROVED','CREDIT_APPLIED') AND product_type='reload' AND SUBSTR(product_group,1,4) <> 'date' AND axess_status <> 'SKIPPED'");
            WebsalesReport(WebSale, "Prepaids & Reloads");

        }

        private void BtnDateSpecificReloads_Click(object sender, EventArgs e)
        {
            DataRow[] WebSale = WebSales.Select("status = 'APPROVED' AND product_type='reload' AND SUBSTR(product_group,1,4) = 'date'");
            WebsalesReport(WebSale, "Specific Reloads");
        }

        private void BtnTicketReservations_Click(object sender, EventArgs e)
        {
            DataRow[] WebSale = WebSales.Select("status = 'APPROVED' AND (product_type='new' AND SUBSTR(product_group,1,4) IN ('date', 'good')) OR product_type='altacard'");
            WebsalesReport(WebSale, "Ticket Reservations");
        }

        private void BtnFamilyPackages_Click(object sender, EventArgs e)
        {
            DataRow[] WebSale = WebSales.Select("status = 'APPROVED' AND tickettype=123");
            WebsalesReport(WebSale, "Family Packages");
        }

        private void BtnSkiSchoolSales_Click(object sender, EventArgs e)
        {
            DataRow[] WebSale = WebSales.Select("status = 'APPROVED' AND SUBSTR(product_group,1,3) = 'ski'");
            WebsalesReport(WebSale, "Ski School Sales");
        }

        private void RCBillReport(DataRow[] RCBillX, string Title = "")
        {
            Cursor = Cursors.WaitCursor;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            string t = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string r = System.AppDomain.CurrentDomain.BaseDirectory;
            Workbook wb = excel.Workbooks.Open(r + @"\reports\RCBill.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"EStore Sales Report{(Title == string.Empty ? "" : $" - {Title}").ToString()}\r\n{sStart}";
            int CurRow = 2;
            string AuthCode = string.Empty;
            decimal Amt = 0;
            decimal Tot = 0;
            int Cnt = 0;
            foreach (DataRow RCBX in RCBillX)
            {
                string NewAuthCode = RCBX["authcode"].ToString();
                if (NewAuthCode != AuthCode && AuthCode != string.Empty)
                {
                    ws.Cells[CurRow, 6] = Amt.ToString("0#.00");
                    ws.Cells[CurRow++, 7] = AuthCode;
                    Tot += Amt;
                    Amt = 0;
                    Cnt++;
                }
                else
                {
                    Amt += RCBX.Field<decimal>("debitamt") - RCBX.Field<decimal>("creditamt");
                    ws.Cells[CurRow, 1] = RCBX.Field<DateTime>("transdate");
                    ws.Cells[CurRow, 2] = RCBX.Field<string>("serialkey");
                    ws.Cells[CurRow, 3] = RCBX.Field<string>("tokenkey");
                    ws.Cells[CurRow, 4] = RCBX["firstname"].ToString() + " " + RCBX["lastname"].ToString();
                    ws.Cells[CurRow++, 5] = (RCBX.Field<decimal>("debitamt") - RCBX.Field<decimal>("creditamt")).ToString("0#.00");
                }
            }
            ws.Cells[CurRow, 6] = Amt.ToString("0#.00");
            ws.Cells[CurRow, 7] = AuthCode;
            CurRow += 3;
            Cnt++;
            Tot += Amt;
            ws.Cells[CurRow, 3] = $"Count {Cnt.ToString()}";
            ws.Cells[CurRow, 5] = $"Total Billing: {Tot.ToString("0#.00")}";
            excel.Visible = true;
            ws.PrintOutEx(Type.Missing, Type.Missing, 1, true);
            excel.Visible = false;
            wb.Close(false);
            Cursor = Cursors.Default;
        }

        private void BtnAuthNetRefunds_Click(object sender, EventArgs e) => RCBillReport(ARTrans.Select($"billdate='{sStart}' AND authcode <> '' AND transtype='R' ORDER BY authcode, billdate"));

        private void BtnRCBillingTotal_Click(object sender, EventArgs e) => RCBillReport(ARTrans.Select($"billdate='{sStart}' AND authcode <> '' AND transtype<>'R' ORDER BY authcode, billdate"));

        private void ReloadReport(int POS)
        {
            Cursor = Cursors.WaitCursor;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            string t = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string r = System.AppDomain.CurrentDomain.BaseDirectory;
            Workbook wb = excel.Workbooks.Open(r + @"\reports\AXReloads.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"Axess Web Reloads Report - POS {POS}\r\n{sStart}";
            int CurRow = 2;
            int Qty = 0;
            decimal Tot = 0;
            foreach (DataRow SaleDay in SalesDay.Select($"nkassa={POS}"))
            {
                ws.Cells[CurRow, 1] = SaleDay["res_no"].ToString();
                ws.Cells[CurRow, 2] = SaleDay["firstname"].ToString() + " " + SaleDay["lastname"].ToString();
                ws.Cells[CurRow, 3] = SaleDay["tickdesc"].ToString();
                ws.Cells[CurRow, 4] = SaleDay["webid"].ToString();
                ws.Cells[CurRow, 5] = SaleDay.Field<int>("qty").ToString();
                ws.Cells[CurRow++, 6] = SaleDay.Field<decimal>("axtotal").ToString("0#.00");
                Qty += SaleDay.Field<int>("qty");
                Tot += SaleDay.Field<decimal>("axtotal");
            }
            CurRow += 2;
            ws.Cells[CurRow, 4] = "\t\t\tTotal";
            ws.Cells[CurRow, 5] = Qty.ToString();
            ws.Cells[CurRow, 6] = Tot.ToString("0#.00");
            excel.Visible = true;
            ws.PrintOutEx(Type.Missing, Type.Missing, 1, true);
            excel.Visible = false;
            wb.Close(false);
            Cursor = Cursors.Default;
        }

        private void BtnAxessPrepaidsReloads_Click(object sender, EventArgs e)
        {
            //axreloads report
            ReloadReport(20);
        }

        private void WillcallReport(DataRow[] WillcallX, string Title = "")
        {
            Cursor = Cursors.WaitCursor;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            string t = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string r = System.AppDomain.CurrentDomain.BaseDirectory;
            Workbook wb = excel.Workbooks.Open(r + @"\reports\WebSales.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"{Title}\r\n{sStart}";
            int CurRow = 2;
            int Qty = 0;
            decimal Tot = 0;
            foreach (DataRow WillX in WillcallX)
            {
                ws.Cells[CurRow, 1] = WillX["res_no"].ToString();
                ws.Cells[CurRow, 2] = WillX["firstname"].ToString() + " " + WillX["lastname"].ToString();
                ws.Cells[CurRow, 3] = WillX["tickdesc"].ToString();
                ws.Cells[CurRow, 4] = WillX["webid"].ToString();
                ws.Cells[CurRow, 5] = WillX.Field<int>("qty").ToString();
                ws.Cells[CurRow++, 6] = WillX.Field<decimal>("axtotal").ToString("0#.00");
                Qty += WillX.Field<int>("qty");
                Tot += WillX.Field<decimal>("axtotal");
            }
            CurRow += 2;
            ws.Cells[CurRow, 4] = "\t\t\tTotal";
            ws.Cells[CurRow, 5] = Qty.ToString();
            ws.Cells[CurRow, 6] = Tot.ToString("0#.00");
            excel.Visible = true;
            ws.PrintOutEx(Type.Missing, Type.Missing, 1, true);
            excel.Visible = false;
            wb.Close(false);
            Cursor = Cursors.Default;
        }

        private void BtnWillcallDateSpecificReloads_Click(object sender, EventArgs e) => WillcallReport(Willcall.Select($"bookdate='{sStart}' AND company='DT' AND tickcode='REL'"));

        private void BtnWillcallTicketReservations_Click(object sender, EventArgs e) => WillcallReport(Willcall.Select($"bookdate='{sStart}' AND company='DT' AND tickcode IN ('NEW','ALT')"));


        private void BtnCreditUses_Click(object sender, EventArgs e)
        {
            DataRow[] AltaCreditX = AltaCredit.Select($"transactiondate='{sStart}' AND POS=20");
            //run altacredit report
            Cursor = Cursors.WaitCursor;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            string t = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string r = System.AppDomain.CurrentDomain.BaseDirectory;
            Workbook wb = excel.Workbooks.Open(r + @"\reports\AltaCredit.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"Alta Credits - Settlement report for POS 20 on {sStart}.";
            int CurRow = 2;
            decimal Tot = 0;
            foreach (DataRow ACX in AltaCreditX)
            {
                ws.Cells[CurRow, 1] = ACX["ecoupon"].ToString();
                ws.Cells[CurRow, 2] = ACX["firstname"].ToString() + " " + ACX["lastname"].ToString();
                ws.Cells[CurRow++, 3] = ACX.Field<decimal>("matused").ToString("0#.00");
                Tot += ACX.Field<decimal>("amtused");
            }
            ws.Cells[CurRow, 2] = "/t/t/t/t/tTotal";
            ws.Cells[CurRow, 3] = Tot.ToString("0#.00");
            excel.Visible = true;
            ws.PrintOutEx(Type.Missing, Type.Missing, 1, true);
            excel.Visible = false;
            wb.Close(false);
            Cursor = Cursors.Default;
        }
    }
}
