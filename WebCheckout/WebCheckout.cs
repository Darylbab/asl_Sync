using System;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
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
        
        string ARTransTable = "artrans";
        OleDbConnection VFPConn = new OleDbConnection("Provider=VFPOLEDB.1");

        System.Data.DataTable AltaCredit;
        System.Data.DataTable ARTrans;
        System.Data.DataTable ARTransXT;
        DataRow AXReloads;
        DataRow POS20Sum;
        DataRow POS21Sum;
        System.Data.DataTable SalesDay;
        System.Data.DataTable WebSales;
        System.Data.DataTable Willcall;

        public string gDrive = @"\\alta.com\namespace\apps\altaapps\App_ExcelReports";

        const string IFormat = "#,##0";
        const string DFormat = "#,##0.00";

        private const string mdatadir = @"g:\fpdata\altaax\";

        DateTime RunDate = DateTime.Today.AddDays(-1);   //assume yesterday
        decimal VISA = 0;
        decimal Mastercard = 0;
        decimal Discover = 0;
        decimal Amex = 0;
        decimal CardTot = 0;
        decimal ReloadDisc = 0;
        decimal ReloadTot = 0;
        decimal AXTot = 0;
        decimal AltaCredit1 = 0;
        decimal GiftUse = 0;
        decimal GiftLoads = 0;
        decimal WebShip = 0;
        //decimal CCBill1 = 0;
        decimal PassReloads = 0;
        decimal TicketNew = 0;
        decimal TicketReload = 0;
        decimal SkiSchool = 0;
        decimal AXPassReloads = 0;
        decimal WCNew = 0;
        decimal WCReload = 0;
        decimal FamilyPackage = 0;
        decimal RCBillAmount = 0;
        decimal SalesTotal = 0;
        decimal WebAuthnet = 0;
        decimal WebCoupon = 0;
        decimal WebGiftUse = 0;
        decimal WebTotal = 0;
        decimal AxReceived = 0;
        decimal WCSent = 0;
        decimal AxCancel = 0;
        decimal WCCredit = 0;
        decimal WCRefund = 0;
        decimal TRRetail = 0;
        decimal VCWillcall = 0;
        decimal Short = 0;
        decimal WebSales1 = 0;
        decimal WebRefund = 0;
        decimal WebGift = 0;
        decimal TotRec = 0;
        string StartString = string.Empty;
        string EndString = string.Empty;
        
        public WebCheckout()
        {
            InitializeComponent();
            dtpWorkDate.Value = DateTime.Today.AddDays(-1);            
        }

        //private void WebCheckout_Shown(object sender, EventArgs e) => LoadData();

        private void LoadData()
        {
         //   am.UpdateTables(386244977);
            Cursor OldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            ClrFields();
            StatusText.Enabled = true;
            StatusText.Visible = true;
            StatusText.Text = "Configuring View";
            SetButtonStatus(false);
            SetTextboxStatus(false);
            SetLabelStatus(false);
            btnExit.Enabled = true;
            btnSummaryReport.Enabled = true;
            btnEstoreDetail.Enabled = true;
            RunDate = dtpWorkDate.Value.Date;
            StartString = RunDate.ToString(Mirror.AxessDateFormat);
            EndString = RunDate.AddDays(1).ToString(Mirror.AxessDateFormat);
            StatusText.Text = "Loading Checkout Info";
            System.Windows.Forms.Application.DoEvents();
            //AXCust = CF.LoadTable(am.MirrorConn, $"SELECT * FROM {am.ActiveDatabase}.tabaxcust WHERE ((nperskassa < 60) OR (nperskassa between 117 AND 120)) ORDER BY nperskassa, npersnr", "AXCust");
            //FoxPro
            //VFPConn.ConnectionString = @"provider=vfpoledb.1;data source = G:\FPData\altaax\possum.dbf";
            //POS20Sum = CF.LoadDataRow(VFPConn, $"SELECT cmast, cvisa, cdisc, camex, cardtot, cgiftuse, cgiftadd, totrec, webreloads, weborders, webskisch, webfampck, webtickn, webtickr, webgift, webrefund, ccbillamt, webship, acredit, axtot, axwebdisc FROM possum WHERE saledate={CF.SetFoxProDate(mStart)} AND pos=20");
            //VFPConn.ConnectionString = @"provider=vfpoledb.1;data source = G:\FPData\altaax\possum.dbf";
            //POS21Sum = CF.LoadDataRow(VFPConn, $"SELECT cmast, cvisa, cdisc, camex, cardtot, cgiftuse, cgiftadd, totrec, webreloads, weborders, webskisch, webfampck, webtickn, webtickr, webgift, webrefund, ccbillamt, webship, acredit, axtot, axwebdisc FROM possum WHERE saledate={CF.SetFoxProDate(mStart)} AND pos=21");
            //mySQL
            POS20Sum = CF.LoadDataRow(DW.dwConn, $"SELECT cmast, cvisa, cdisc, camex, cardtot, cgiftuse, cgiftadd, totrec, webreloads, weborders, webskisch, webfampck, webtickn, webtickr, webgift, webrefund, ccbillamt, webship, acredit, axtot, axwebdisc FROM {DW.ActiveDatabase}.possum WHERE saledate='{RunDate.ToString(Mirror.AxessDateFormat)}' AND pos=20");
            POS21Sum = CF.LoadDataRow(DW.dwConn, $"SELECT cmast, cvisa, cdisc, camex, cardtot, cgiftuse, cgiftadd, totrec, webreloads, weborders, webskisch, webfampck, webtickn, webtickr, webgift, webrefund, ccbillamt, webship, acredit, axtot, axwebdisc FROM {DW.ActiveDatabase}.possum WHERE saledate='{RunDate.ToString(Mirror.AxessDateFormat)}' AND pos=21");
            //ClrFields();
            if (POS20Sum["cmast"].ToString() != string.Empty)
            {
                LoadFields();
            }
            StatusText.Text = "Loading eStore Data";
            StatusText.Refresh();
            string Query = $"SELECT DATE(O.created_at) AS sale_date, O.id AS orderid, I.id AS itemid, I.axess_tickettype AS tickettype, I.axess_passtype, " +
                $"CAST(I.product_name AS CHAR (30)) AS product_name, I.price, I.quantity AS qty, I.total, SUBSTR(O.status, 1, 15) AS status, " +
                $"SUBSTR(I.person_firstname, 1, 20) AS firstname, SUBSTR(I.person_lastname, 1, 20) AS lastname, I.axess_wtp AS webid, T.NJOURNALNO, T.NPOSNO AS NPOSNO, " +
                $"T.NSERIALNO, I.product_group, O.subtotal, O.shipping_amount, O.credit_amount, O.giftcard_amount AS gift_card_amount, O.total AS ccamt, " +
                $"P.transaction_id AS tran_approval_code, P.card_type, P.card_number, SPACE(20) AS confirmed " +
                $"FROM altashop.orders AS O " +
                $"INNER JOIN altashop.order_items AS I ON O.id = I.order_id " +
                $"LEFT JOIN altashop.payments AS P ON P.order_id = O.ID " +
                $"LEFT JOIN altashop.axess_transactions AS T ON T.order_item_id=I.id " +
                $"WHERE O.created_at >= '{StartString}' AND O.created_at < '{RunDate.AddDays(1).ToString(Mirror.AxessDateFormat)}' ORDER BY O.id, I.id";
            WebSales = CF.LoadTable(AltaShop.Shop_Alta_ComConn, Query, "WebSales");
            AltaCredit = CF.LoadTable(AltaShop.Shop_Alta_ComConn, $"SELECT date(A.created) AS PayDate, A.order_id, A.amount, C.coupon_code, concat(firstname, ' ', lastname) AS Customer_Name FROM altashop.credits_applied AS A LEFT JOIN altashop.credits AS C ON C.id = A.credit_id WHERE created >='{StartString}' AND created <'{EndString}' ORDER BY A.order_id", "AltaCredits");
//            AltaCredit = CF.LoadTable(BUY.Buy_Alta_ComConn, $"SELECT A.creditkey, A.customerkey, A.ecoupon, A.type, A.season, A.transactionamount AS amtused, A.pos, " +
//                $"A.transaction, A.transactiondate, A.dateexpires, A.reason, A.memo, A.acct, A.masterflag, A.active, C.fname, C.lname " +
//                $"FROM axessutility.altacredit2 AS A " +
//                $"INNER JOIN axessutility.creditcustomers AS C ON A.customerkey=C.customerkey " +
//                $"WHERE A.transactiondate='{RunDate.ToString(Mirror.AxessDateFormat)}' AND A.pos=20", "AltaCredit");
            StatusText.Text = "Loading Alta Credit Info";
            StatusText.Refresh();
            AltaCredit1 = 0;
            foreach (DataRow AC in AltaCredit.Rows)
            {
                AltaCredit1 -= AC.Field<decimal>("amount");
            }
            //set salesdata and pmtdata "saledate" field to match the estore if they are produced and the saledate is different between the estore and these tables.
            foreach (DataRow WebSale in WebSales.Rows)
            {
                if (WebSale["sale_date"].ToString().Trim() != string.Empty && WebSale["nserialno"].ToString().Trim() != string.Empty)
                {
                    AXReloads = CF.LoadDataRow(DW.dwConn, $"SELECT saledate, nlfdzaehler, transkey FROM {DW.ActiveDatabase}.salesdata WHERE nkassanr=20 AND nseriennr={WebSale["nserialno"].ToString()} ORDER BY saledate DESC LIMIT 1");
                    if (AXReloads != null)
                    {
                        if (AXReloads["saledate"].ToString().Trim() != string.Empty)
                        {
                            if (AXReloads.Field<DateTime?>("saledate") != WebSale.Field<DateTime?>("sale_date"))
                            {
                                CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET saledate = '{WebSale.Field<DateTime>("sale_date").ToString(Mirror.AxessDateFormat)}' WHERE nlfdzaehler='{AXReloads["nlfdzaehler"].ToString()}'");
                                CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.pmtdata SET saledate = '{WebSale.Field<DateTime>("sale_date").ToString(Mirror.AxessDateFormat)}' WHERE transkey='{AXReloads["transkey"].ToString()}'");
                            }
                        }
                    }
                }
            }
            //load active willcall records from the estore
            //Willcall = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.willcall WHERE tickcode IN ('NEW', 'ALT') OR (tickcode='REL' AND company='DT')", "willcall");
            string Where = $"billdate = '{StartString}' AND authcode <> '' AND (( arcustid = '11874') or transtype = 'R')";
            string Where2 = $"billdate = '{StartString}' AND authcode <> '' AND arcustid = '11874'";
            //set creditamt and debitamt to 0 when set to null.
            Query = $"UPDATE {DW.ActiveDatabase}.{ARTransTable} SET creditamt = 0 WHERE creditamt is null AND {Where}";
            CF.ExecuteSQL(DW.dwConn, Query);
            CF.ExecuteSQL(DW.dwConn, Query.Replace("credit", "debit"));
            //****get resort charge/ refund totals * **
            string tVal = CF.GetSQLField(DW.dwConn, $"SELECT SUM(debitamt-creditamt) FROM {DW.ActiveDatabase}.{ARTransTable} WHERE transtype <> 'R' AND {Where2}");
            RCBillAmount = (tVal == string.Empty ? 0 : Convert.ToDecimal(tVal));
            tVal = CF.GetSQLField(DW.dwConn, $"SELECT SUM(creditamt-debitamt) FROM {DW.ActiveDatabase}.{ARTransTable} WHERE transtype = 'R' AND {Where2}");
            WebRefund = (tVal == string.Empty ? 0 : Convert.ToDecimal(tVal));
            ARTrans = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.{ARTransTable} WHERE {Where}", "ARTrans");
            StatusText.Text = "Loading Axess Info";
            StatusText.Refresh();
            //PmtDay = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.pmtdata WHERE saledate = '{sStart}' AND nkassanr IN (20,21) AND testflag = 0 ORDER BY transkey", "PmtDay");
            SalesDay = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.salesdata WHERE saledate = '{StartString}' AND nkassanr IN (20, 21) AND testflag = 0 ORDER BY transkey", "SalesDay");
            //GetAXOrders();
            StatusText.Text = "Updating Checkout Detail Files";
            //bool msalesflag = !((POS20Sum["totrec"].ToString() == string.Empty) && (POS21Sum["totrec"].ToString() == string.Empty) && (CCBill1 == 0));
            //if (msalesflag)
            {
                StatusText.Visible = false;
                SetLabelStatus(true);
                SetButtonStatus(false);
                btnEdit.Enabled = true;
                btnExit.Enabled = true;
                btnSummaryReport.Enabled = true;
                btnEstoreDetail.Enabled = true;
                btnPreviousDay.Enabled = true;
                btnNextDay.Enabled = true;
                CalcTot();
                if (CardTot == 0)
                {
                    txtVISA.Enabled = true;
                    txtMastercard.Enabled = true;
                    txtAMEX.Enabled = true;
                    txtDiscover.Enabled = true;
                    txtAuthNetGiftCardUse.Enabled = true;
                    btnSave.Enabled = true;
                    btnAbort.Enabled = true;
                    Notification.Visible = true;
                }
                else
                {
                    txtVISA.Enabled = false;
                    txtMastercard.Enabled = false;
                    txtAMEX.Enabled = false;
                    txtDiscover.Enabled = false;
                    txtAuthNetGiftCardUse.Enabled = false;
                    Notification.Visible = false;
                    btnSummaryReport.Enabled = true;
                    btnEstoreDetail.Enabled = true;
                }
                System.Windows.Forms.Application.DoEvents();
            }
            EnableRpts();
            StatusText.Enabled = false;
            StatusText.Visible = false;
            Cursor.Current = OldCursor;
            System.Windows.Forms.Application.DoEvents();
        }

        private void LoadFields()
        {
            Mastercard = CF.Null2Val(POS20Sum.Field<decimal?>("cmast"));
            VISA = CF.Null2Val(POS20Sum.Field<decimal?>("cvisa"));
            Discover = CF.Null2Val(POS20Sum.Field<decimal?>("cdisc"));
            Amex = CF.Null2Val(POS20Sum.Field<decimal?>("camex"));
            CardTot = CF.Null2Val(POS20Sum.Field<decimal?>("cardtot"));
            GiftUse = CF.Null2Val(POS20Sum.Field<decimal?>("cgiftuse"));
            GiftLoads = CF.Null2Val(POS20Sum.Field<decimal?>("cgiftadd"));

            TotRec = CF.Null2Val(POS20Sum.Field<decimal?>("totrec"));
            PassReloads = CF.Null2Val(POS20Sum.Field<decimal?>("webreloads"));
            //mpassorders = CF.Null2Val(POS20Sum.Field<decimal?>("weborders"));
            //txtPassCardOrders.Text = mpassorders.ToString(DFormat);
            SkiSchool = CF.Null2Val(POS20Sum.Field<decimal?>("webskisch"));
            FamilyPackage = CF.Null2Val(POS20Sum.Field<decimal?>("webfampck"));
            TicketNew = CF.Null2Val(POS20Sum.Field<decimal?>("webtickn"));
            TicketReload = CF.Null2Val(POS20Sum.Field<decimal?>("webtickr"));
            WebGift = CF.Null2Val(POS20Sum.Field<decimal?>("webgift"));
            WebRefund = CF.Null2Val(POS20Sum.Field<decimal?>("webrefund"));
            RCBillAmount = CF.Null2Val(POS20Sum.Field<decimal?>("ccbillamt"));
            WebShip = CF.Null2Val(POS20Sum.Field<decimal?>("webship"));
            AltaCredit1 = CF.Null2Val(POS20Sum.Field<decimal?>("acredit"));
            SalesTotal = CF.Null2Val(POS20Sum.Field<decimal?>("axtot"));
            ReloadDisc = CF.Null2Val(POS20Sum.Field<decimal?>("axwebdisc"));
            //maxsales = 0;
            SetFields();
            System.Windows.Forms.Application.DoEvents();
        }

        private void SetFields()
        {
            SalesTotal = PassReloads + SkiSchool + WebGift + TicketNew + TicketReload + FamilyPackage + ReloadDisc + RCBillAmount + WebRefund + WebShip;
            CardTot = VISA + Mastercard + Amex + Discover;
            WebSales1 = PassReloads + SkiSchool + WebGift + TicketNew + TicketReload + FamilyPackage + ReloadDisc + WebShip;
            //WebAuthnet += RCBillAmount + WebRefund;
            AXTot = WebAuthnet + WebCoupon + WebGiftUse;
            TotRec = CardTot + GiftUse + Math.Abs(AltaCredit1);
            //Short = SalesTotal - TotRec;
            Short = TotRec - SalesTotal;
            if (Convert.ToDouble(Math.Abs(Short)) < 0.01)
            {
                Short = 0;
            }
            txtWillcallDateSpecificReloads.Text = WCReload.ToString(DFormat);
            txtWillcallTicketReservations.Text = WCNew.ToString(DFormat);
            txtBackInWillcall.Text = WCCredit.ToString(DFormat);
            txtAxessPrepaidsReloads.BackColor = (PassReloads != AXPassReloads + ReloadDisc ? Color.Tomato : Color.Gold);
            txtWillcallTicketReservations.BackColor = (TicketNew != WCNew ? Color.Tomato : Color.Gold);
            txtWillcallDateSpecificReloads.BackColor = (TicketReload != WCReload ? Color.Tomato : Color.Gold);
            //**** summarize results***
            //mwebsales = mpassorders + mpassreloads + mskischool + mwebgift + mtickn + mtickr + mfampck + mreloaddisc + mwebship;
            txtEstoreSales.Text = WebSales1.ToString(DFormat);
            txtCreditCards.Text = WebAuthnet.ToString(DFormat);
            txtEstoreReceipts.Text = AXTot.ToString(DFormat);
            txtOverShort.Text = Short.ToString(DFormat);
            txtAuthNetTotal.BackColor = (Convert.ToDouble(Math.Abs(CardTot - WebAuthnet)) <= 0.01 ? Color.Gold : Color.Tomato);
            txtSalesTotal.BackColor = ((SalesTotal != AXTot) || (TotRec != AXTot) || (TotRec != SalesTotal) ? Color.Tomato : Color.Gold);
            txtEstoreReceipts.BackColor = txtSalesTotal.BackColor;
            txtSettledReceipts.BackColor = txtSalesTotal.BackColor;
            txtSentFromWillcall.Text = WCSent.ToString(DFormat);
            txtMastercard.Text = Mastercard.ToString(DFormat);
            txtVISA.Text = VISA.ToString(DFormat);
            txtDiscover.Text = Discover.ToString(DFormat);
            txtAMEX.Text = Amex.ToString(DFormat);
            txtAuthNetTotal.Text = CardTot.ToString(DFormat);
            txtAuthNetGiftCardUse.Text = GiftUse.ToString(DFormat);
            txtSettledReceipts.Text = TotRec.ToString(DFormat);
            txtPrepaidsReloads.Text = PassReloads.ToString(DFormat);
            txtSkiSchoolSales.Text = SkiSchool.ToString(DFormat);
            txtFamilyPackages.Text = FamilyPackage.ToString(DFormat);
            txtTicketReservations.Text = TicketNew.ToString(DFormat);
            txtDateSpecificReloads.Text = TicketReload.ToString(DFormat);
            txtGiftCards.Text = WebGift.ToString(DFormat);
            txtAuthNetRefunds.Text = WebRefund.ToString(DFormat);
            txtRCBillingTotal.Text = RCBillAmount.ToString(DFormat);
            txtShipping.Text = WebShip.ToString(DFormat);
            txtCreditUses.Text = Math.Abs(AltaCredit1).ToString(DFormat);
            txtSalesTotal.Text = SalesTotal.ToString(DFormat);
            txtEstoreReceipts.Text = AXTot.ToString(DFormat);
            txtECoupons.Text = WebCoupon.ToString(DFormat);
            txtGiftCardUse.Text = WebGiftUse.ToString(DFormat);
            Refresh();
            System.Windows.Forms.Application.DoEvents();
        }


        private void BtnEstoreDetail_Click(object sender, EventArgs e)
        {
            DataRow[] WebSaleX= WebSales.Select($"status='APPROVED'");
            Cursor = Cursors.WaitCursor;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook wb = excel.Workbooks.Open(gDrive + @"\EstoreDetail2.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"Estore Detail Sales Report - {StartString}";
            int CurRow = 2;
            foreach (DataRow WSX in WebSaleX)
            {
                ws.Cells[CurRow, 1] = WSX["orderid"].ToString();
                ws.Cells[CurRow, 2] = WSX["tran_approval_code"].ToString();
                ws.Cells[CurRow, 3] = WSX["firstname"].ToString() + " " + WSX["lastname"].ToString();
                ws.Cells[CurRow, 4] = WSX["card_type"].ToString();
                ws.Cells[CurRow, 5] = WSX["card_number"].ToString();
                ws.Cells[CurRow++, 6] = "CC AMT";
                ws.Cells[CurRow, 7] = WSX["ccamt"].ToString();
                ws.Cells[CurRow++, 6] = "Credit AMT";
                ws.Cells[CurRow, 7] = WSX["credit_amount"].ToString();
                ws.Cells[CurRow++, 6] = "Gift Card AMT";
                ws.Cells[CurRow, 7] = WSX["gift_card_amount"].ToString();

            }

            excel.Visible = true;
            ws.PrintOutEx(Type.Missing, Type.Missing, 1, true);
            excel.Visible = false;
            wb.Close(false);
            Cursor = Cursors.Default;
            btnExit.Focus();
        }

        private void BtnExit_Click(object sender, EventArgs e) => System.Windows.Forms.Application.Exit();

        private void BtnLoadData_Click(object sender, EventArgs e) => LoadData();

        private void EnableRpts()
        {
            //btnPassCardOrders.Enabled = (mpassorders == 0);
            btnPrepaidsReloads.Enabled = (PassReloads != 0);
            btnFamilyPackages.Enabled = (FamilyPackage != 0);
            btnTicketReservations.Enabled = (TicketNew != 0);
            btnDateSpecificReloads.Enabled = (TicketReload != 0);
            btnSkiSchoolSales.Enabled = (SkiSchool != 0);
            btnGiftCards.Enabled = (WebGift != 0);
            btnAuthNetRefunds.Enabled = (WebRefund != 0);
            btnRCBillingTotal.Enabled = (RCBillAmount != 0);
            //btnAxessOrders.Enabled = (maxpassorders == 0);
            btnAxessPrepaidsReloads.Enabled = (AXPassReloads != 0);
            btnWillcallTicketReservations.Enabled = (WCNew != 0);
            btnWillcallDateSpecificReloads.Enabled = (WCReload != 0);
            btnCreditUses.Enabled = (AltaCredit1 != 0);
            //btnErrorsReport.Enabled = (mpassorders != maxpassorders || mpassreloads != maxpassreloads);
            btnErrorsReport.Enabled = (PassReloads != AXPassReloads);
            StatusText.Visible = false;
        }

        private void TCalcTot()
        {
            //PREPAIDS / RELOADS
            string tVal = CF.GetSQLField(AltaShop.Shop_Alta_ComConn, $"SELECT SUM(total) FROM altashop.order_items WHERE created_at >= '{StartString}' AND created_at < '{EndString}' AND axess_status NOT IN ('SKIPPED', 'DO-NOT-PROCESS')");
            PassReloads = (tVal == string.Empty ? 0 : Convert.ToDecimal(tVal));
            //SELECT*
            //FROM altashop.order_items
            //where date(created_at) = '20180109' and axess_status != 'SKIPPED'

            //DATE SPECIFIC TICKETS
            tVal = CF.GetSQLField(AltaShop.Shop_Alta_ComConn, $"SELECT SUM(total) FROM altashop.order_items WHERE created_at >= '{StartString}' AND created_at < '{EndString}' AND UPPER(axess_status) IN ('SKIPPED', 'DO-NOT-PROCESS') AND product_group LIKE 'date%' AND axess_passtype LIKE 'reload'");
            TicketReload = (tVal == string.Empty ? 0 : Convert.ToDecimal(tVal));
            //SELECT*
            //FROM altashop.order_items
            //where date(created_at) = '20180109' and axess_status = 'SKIPPED' AND(AXESS_PASSTYPE = 'reload' and product_group like 'date%')

            //TICKET RESERVATIONS
            tVal = CF.GetSQLField(AltaShop.Shop_Alta_ComConn, $"SELECT SUM(total) FROM altashop.order_items WHERE created_at >= '{StartString}' AND created_at < '{EndString}' AND UPPER(axess_status) IN ('SKIPPED', 'DO-NOT-PROCESS') AND ((UPPER(AXESS_PASSTYPE) = 'NEW' OR UPPER(AXESS_PASSTYPE) = 'ALTA') AND (product_group LIKE 'good%' OR product_group LIKE 'date%'))");
            TicketNew = (tVal == string.Empty ? 0 : Convert.ToDecimal(tVal));
            //SELECT*
            //FROM altashop.order_items
            //where date(created_at) = '20180109' and axess_status = 'SKIPPED' AND ((AXESS_PASSTYPE = 'new' or AXESS_PASSTYPE = 'alta')  and(product_group like 'good%' or product_group like 'date%'))

            //GIFT CARDS
            tVal = CF.GetSQLField(AltaShop.Shop_Alta_ComConn, $"SELECT SUM(total) FROM altashop.order_items WHERE created_at >= '{StartString}' AND created_at < '{EndString}' AND product_group LIKE '%GIFT%'");
            WebGift = (tVal == string.Empty ? 0 : Convert.ToDecimal(tVal));
            
        }

        private void CalcTot()
        {
            // ***GET E - STORE TOTALS***
            WebTotal = 0;
            WebShip = 0;
            SkiSchool = 0;
            WebShip = 0;
            FamilyPackage = 0;
            ReloadDisc = 0;
            //RELOADS

            string AltaShopQuery = $"SELECT * FROM (" +
                $"(SELECT (CASE WHEN COUNT(*) = 0 THEN 0 ELSE SUM(I.total) END) AS PassReloads FROM altashop.order_items AS I INNER JOIN altashop.orders AS O ON O.id = I.order_id WHERE O.created_at >= '{StartString}' AND O.created_at < '{EndString}' AND I.axess_status NOT IN ('SKIPPED' , 'DO-NOT-PROCESS') and O.status not in ('cancelled', 'declined')) AS A, " +
                $"(SELECT (CASE WHEN COUNT(*) = 0 THEN 0 ELSE SUM(I.total) END) AS TicketReloads FROM altashop.order_items AS I INNER JOIN altashop.orders AS O ON O.id = I.order_id WHERE O.created_at >= '{StartString}' AND O.created_at < '{EndString}' AND UPPER(I.axess_status) IN ('SKIPPED' , 'DO-NOT-PROCESS') and O.status not in ('cancelled', 'declined') AND I.product_group LIKE 'date%' AND I.axess_passtype LIKE 'reload') AS B, " +
                $"(SELECT  (CASE WHEN COUNT(*) = 0 THEN 0 ELSE SUM(I.total) END) AS TicketNew FROM altashop.order_items AS I INNER JOIN altashop.orders AS O ON O.id = I.order_id WHERE O.created_at >= '{StartString}' AND O.created_at < '{EndString}' AND UPPER(I.axess_status) IN ('SKIPPED' , 'DO-NOT-PROCESS') and O.status not in ('cancelled', 'declined') AND ((UPPER(I.AXESS_PASSTYPE) = 'NEW' OR UPPER(I.AXESS_PASSTYPE) = 'ALTA') AND (I.product_group LIKE 'good%' OR I.product_group LIKE 'date%'))) AS C, " +
                $"(SELECT  (CASE WHEN COUNT(*) = 0 THEN 0 ELSE SUM(I.total) END) AS WebGift FROM altashop.order_items AS I INNER JOIN altashop.orders AS O ON O.id = I.order_id WHERE O.created_at >= '{StartString}' AND O.created_at < '{EndString}'  and O.status not in ('cancelled', 'declined') AND I.product_group LIKE '%GIFT%') AS D, " +
                $"(SELECT  (CASE WHEN COUNT(*) = 0 THEN 0 ELSE SUM(P.amount) END) AS WebGiftUse FROM altashop.payments AS P INNER JOIN altashop.orders AS O ON O.id = P.order_id WHERE O.created_at >= '{StartString}' AND O.created_at < '{EndString}'  and O.status not in ('cancelled', 'declined') AND P.type LIKE '%GIFT%') AS E, " +
                $"(SELECT  (CASE WHEN COUNT(*) = 0 THEN 0 ELSE SUM(P.amount) END) AS WebAuthnet FROM altashop.payments AS P INNER JOIN altashop.orders AS O ON O.id = P.order_id WHERE O.created_at >= '{StartString}' AND O.created_at < '{EndString}'  and O.status not in ('cancelled', 'declined') AND P.type LIKE '%charge') AS F, " +
                $"(SELECT (CASE WHEN COUNT(*) = 0 THEN 0 ELSE SUM(P.amount) END) AS WebCoupon FROM altashop.payments AS P INNER JOIN altashop.orders AS O ON O.id = P.order_id WHERE O.created_at >= '{StartString}' AND O.created_at < '{EndString}'  and O.status not in ('cancelled', 'declined') AND P.type LIKE '%credit%') AS G)";
            DataRow Shop = CF.LoadDataRow(AltaShop.Shop_Alta_ComConn, AltaShopQuery);
            PassReloads = Shop.Field<decimal>("PassReloads");
            //DATE SPECIFIC TICKETS
            TicketReload = Shop.Field<decimal>("TicketReloads");
            //TICKET RESERVATIONS
            TicketNew = Shop.Field<decimal>("TicketNew");
            //GIFT CARDS
            WebGift = Shop.Field<decimal>("WebGift");
            //GIFT CARD USE
            WebGiftUse = Shop.Field<decimal>("WebGiftUse");
            //AUTHNET CHARGES
            WebAuthnet = Shop.Field<decimal>("WebAuthnet");
            //WEB COUPONS/E-COUPONS
            WebCoupon = Shop.Field<decimal>("WebCoupon");
            //**** get resort charge/ refund totals***
            //SELECT artrans
            //RCBillAmount = 0;
            WebRefund = 0;
            foreach (DataRow Trans in ARTrans.Rows)
            {
                //if ((Trans.Field<DateTime>("billdate") == RunDate.Date) && (Trans["authcode"].ToString() == string.Empty))
                    if ((Trans.Field<DateTime>("billdate") == RunDate.Date))
                    {
                            if ((Trans.Field<int>("tokenkey") > 0) && (Trans["arcustid"].ToString() == "11874"))
                            {
                                //RCBillAmount += Trans.Field<decimal>("debitamt") - Trans.Field<decimal>("creditamt");
                            }
                    if (Trans["transtype"].ToString() == "R")
                    {
                        WebRefund += Trans.Field<decimal>("debitamt") - Trans.Field<decimal>("creditamt");
                    }
                }
            }
        
            //*** RELOADS ***
            AXPassReloads = 0;
            AxReceived = 0;
            AxCancel = 0;
            if (SalesDay != null)
            {
                foreach (DataRow SaleDay in SalesDay.Rows)
                {
                    decimal tariff = SaleDay.Field<decimal>("tariff");
                    if (SaleDay.Field<int>("nkassanr") == 20)
                    {
                        AXPassReloads += tariff;
                    }
                    else if (SaleDay.Field<int>("nkassanr") == 21)
                    { if (SaleDay["ttype"].ToString() == "S")
                    {
                        AxReceived += tariff;
                    }
                    else
                    {
                        AxCancel -= tariff;
                    }}
                }
            }

            txtReceivedByAxess21.Text = AxReceived.ToString(DFormat);
            txtCancelledByAxess.Text = AxCancel.ToString(DFormat);
            ReloadTot = AXPassReloads;
            txtAxessPrepaidsReloads.Text = ReloadTot.ToString(DFormat);
            //* **GET WILLCALL TICKETS ***
            string WCQuery = $"SELECT * FROM (" +
                $"(SELECT(CASE WHEN COUNT(*) = 0 THEN 0 ELSE SUM(axtotal) END) AS WCNew FROM applications.willcall WHERE BOOKDATE = '{StartString}' AND COMPANY = 'DT' AND tickcode IN('NEW', 'ALT')) AS A," +
                $"(SELECT(CASE WHEN COUNT(*) = 0 THEN 0 ELSE SUM(axtotal) END) AS WCReload FROM applications.willcall WHERE BOOKDATE = '{StartString}' AND COMPANY = 'DT' AND tickcode = 'REL') AS B," +
                $"(SELECT(CASE WHEN COUNT(*) = 0 THEN 0 ELSE SUM(axtotal) END) AS WCSent FROM applications.willcall WHERE arr_date = '{RunDate.AddDays(1).ToString(Mirror.AxessDateFormat)}' AND COMPANY = 'DT' AND tickcode = 'REL') AS C," +
                $"(SELECT(CASE WHEN COUNT(*) = 0 THEN 0 ELSE SUM(axtotal) END) AS WCCredit FROM applications.willcall WHERE canceldate = '{StartString}' AND COMPANY = 'DT' AND tickcode = 'REL') AS D, " +
                $"(SELECT(CASE WHEN COUNT(*) = 0 THEN 0 ELSE SUM(axtotal) END) AS WCRefund FROM applications.willcall WHERE canceldate = '{StartString}' AND COMPANY = 'DT' AND itemstatus = 'R') AS E);";
            DataRow WC = CF.LoadDataRow(DW.dwConn, WCQuery);
            WCNew = WC.Field<decimal>("WCNew");
            WCReload = WC.Field<decimal>("WCReload");
            WCSent = WC.Field<decimal>("WCSent");
            WCCredit = WC.Field<decimal>("WCCredit");
            WCRefund = WC.Field<decimal>("WCRefund");
            SetFields();
            //txtAxessOrders.BackColor = (mpassorders != maxpassorders ? Color.Red : Color.Yellow);
            System.Windows.Forms.Application.DoEvents();
        }

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
            Mastercard = 0;
            txtMastercard.Text = Mastercard.ToString(DFormat);
            VISA = 0;
            txtVISA.Text = VISA.ToString(DFormat);
            Discover = 0;
            txtDiscover.Text = Discover.ToString(DFormat);
            Amex = 0;
            txtAMEX.Text = Amex.ToString(DFormat);
            CardTot = 0;
            txtAuthNetTotal.Text = CardTot.ToString(DFormat);
            ReloadTot = 0;
            txtAxessPrepaidsReloads.Text = ReloadTot.ToString(DFormat);
            WebSales1 = 0;
            txtEstoreSales.Text = WebSales1.ToString(DFormat);
            Short = 0;
            txtOverShort.Text = Short.ToString(DFormat);
            WebTotal = 0;
            TotRec = 0;
            WebCoupon = 0;
            txtECoupons.Text = WebCoupon.ToString(DFormat);
            WebAuthnet = 0;
            txtCreditCards.Text = string.Empty;
            GiftLoads = 0;
            GiftUse = 0;
            txtGiftCardUse.Text = GiftUse.ToString(DFormat);
            PassReloads = 0;
            SkiSchool = 0;
            WebRefund = 0;
            WebGift = 0;
            RCBillAmount = 0;
            SalesTotal = 0;
            TicketNew = 0;
            TicketReload = 0;
            AltaCredit1 = 0;
            txtCreditUses.Text = Math.Abs(AltaCredit1).ToString(DFormat);
            AXPassReloads = 0;
            WCNew = 0;
            txtWillcallTicketReservations.Text = WCNew.ToString(DFormat);
            WCReload = 0;
            txtWillcallDateSpecificReloads.Text = WCReload.ToString(DFormat);
            AxReceived = 0;
            txtReceivedByAxess21.Text = AxReceived.ToString(DFormat);
            WCSent = 0;
            txtSentFromWillcall.Text = WCSent.ToString(DFormat);
            AxCancel = 0;
            txtCancelledByAxess.Text = AxCancel.ToString(DFormat);
            AXTot = 0;
            txtEstoreReceipts.Text = AXTot.ToString(DFormat);
            WCCredit = 0;
            txtBackInWillcall.Text = WCCredit.ToString(DFormat);
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
            btnErrorsReport.Visible = (PassReloads != AXPassReloads);
            SummaryReport(20); 

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
            //Update Mysql
            string tQuery = string.Empty;
            if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.possum", $"saledate='{StartString}' AND POS=20"))
            {
                //update record
                tQuery = $"UPDATE {DW.ActiveDatabase}.possum SET cmast={Mastercard.ToString()}, cvisa = {VISA.ToString()}, cdisc = {Discover.ToString()}, camex = {Amex.ToString()}, " +
                    $"cardtot = {CardTot.ToString()}, cgiftuse = {GiftUse.ToString()}, cgiftadd = {GiftLoads.ToString()}, cardcheck = {CardTot.ToString()}, " +
                    $"acredit = {Math.Abs(AltaCredit1).ToString()}, webship = {WebShip.ToString()}, webreloads = {PassReloads.ToString()}, " +
                    $"webSKISCH = {SkiSchool.ToString()}, webfampck = {FamilyPackage.ToString()}, webgift = {WebGift.ToString()}, webrefund = {WebRefund.ToString()}, " +
                    $"ccbillamt = {RCBillAmount.ToString()}, webtickn = {TicketNew.ToString()}, webtickr = {TicketReload.ToString()}, webtot = {SalesTotal.ToString()}, " +
                    $"axreloads = {AXPassReloads.ToString()}, axwebdisc = {ReloadDisc.ToString()}, trretail = {TRRetail.ToString()}, " +
                    $"trwillcall ={(WCReload + WCNew).ToString()}, vcwillcall = {VCWillcall.ToString()}, axcard = {WebAuthnet.ToString()}, " +
                    $"axcredit = {WebCoupon.ToString()}, axgift = {WebGiftUse.ToString()}, axtot = {AXTot.ToString()}, axsales = {SalesTotal.ToString()}, ccsettled = '{StartString}' " +
                    $"WHERE saledate = '{StartString}' AND pos=20";
            }
            else
            {
                //insert record
                tQuery = $"INSERT {DW.ActiveDatabase}.possum (saledate, pos, office, cmast, cvisa, cdisc, camex, cardtot, cgiftuse, cgiftadd, cardcheck, acredit, webship, weborders, " +
                    $"webreloads, webSKISCH, webfampck, webgift, webrefund, ccbillamt, webtickn, webtickr, webtot, axorders, axreloads, axwebdisc, trretail, " +
                    $"trwillcall, vcwillcall, axcard, axcredit, axgift, axtot, axsales, ccsettled) VALUES ('{StartString}' ,20 ,'WEB',{Mastercard.ToString()}, {VISA.ToString()}, " +
                    $"{Discover.ToString()}, {Amex.ToString()}, {CardTot.ToString()}, {GiftUse.ToString()}, {GiftLoads.ToString()}, {CardTot.ToString()}, " +
                    $"{Math.Abs(AltaCredit1).ToString()}, {WebShip.ToString()}, 0, {PassReloads.ToString()}, {SkiSchool.ToString()}, " +
                    $"{FamilyPackage.ToString()}, {WebGift.ToString()}, {WebRefund.ToString()}, {RCBillAmount.ToString()}, {TicketNew.ToString()}, {TicketReload.ToString()}, " +
                    $"{SalesTotal.ToString()}, 0, {AXPassReloads.ToString()}, {ReloadDisc.ToString()}, {TRRetail.ToString()}, " +
                    $"{(WCReload + WCNew).ToString()}, {VCWillcall.ToString()}, {WebAuthnet.ToString()}, {WebCoupon.ToString()}, {WebGiftUse.ToString()}, " +
                    $"{AXTot.ToString()}, {SalesTotal.ToString()}, '{StartString}')";
            }
            CF.ExecuteSQL(DW.dwConn, tQuery);

            //Update Foxpro
            string FPDate = CF.SetFoxProDate(RunDate);
            string FPNullDate = "{^0000-00-00}";
            VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}possum.dbf";
            if (CF.RowCount(VFPConn, $"possum", $"POS=20 AND saledate={FPDate}") != 0)
            {
                tQuery = $"UPDATE possum SET cmast={Mastercard.ToString()}, cvisa = {VISA.ToString()}, cdisc = {Discover.ToString()}, camex = {Amex.ToString()}, " +
                    $"cardtot = {CardTot.ToString()}, cgiftuse = {GiftUse.ToString()}, cgiftadd = {GiftLoads.ToString()}, cardcheck = {CardTot.ToString()}, " +
                    $"acredit = {AltaCredit1.ToString()}, webship = {WebShip.ToString()}, webreloads = {PassReloads.ToString()}, " +
                    $"webSKISCH = {SkiSchool.ToString()}, webfampck = {FamilyPackage.ToString()}, webgift = {WebGift.ToString()}, webrefund = {WebRefund.ToString()}, " +
                    $"ccbillamt = {RCBillAmount.ToString()}, webtickn = {TicketNew.ToString()}, webtickr = {TicketReload.ToString()}, webtot = {SalesTotal.ToString()}, " +
                    $"axreloads = {AXPassReloads.ToString()}, axwebdisc = {ReloadDisc.ToString()}, trretail = {TRRetail.ToString()}, " +
                    $"trwillcall ={(WCReload + WCNew).ToString()}, vcwillcall = {VCWillcall.ToString()}, axcard = {WebAuthnet.ToString()}, " +
                    $"axcredit = {WebCoupon.ToString()}, axgift = {WebGiftUse.ToString()}, axtot = {AXTot.ToString()}, axsales = {SalesTotal.ToString()}, ccsettled = {FPDate} " +
                    $"WHERE saledate = {FPDate} AND pos=20";
                VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}possum.dbf";
                CF.ExecuteSQL(VFPConn, tQuery);
            }
            else
            {

                VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}possum.dbf";
                tQuery = $"INSERT INTO possum (saledate, pos, office, cash, cvisa, cmast, cdisc, camex, cgiftuse, cgiftadd, cardcheck, cardtot, " +
                    $"gcertqty, gcertamt, vchrqty, vchramt, snchk, arqty, aramt, arguest, acredit,  webamt, totrec, axcash, axcard, axvchr, axsnchk, axcredit, axar, " +
                    $"axargst, axweb, axgift, axtot, webship, weborders, webreloads, webtickr, webtickn, webSKISCH, webgift, webrefund, ccbillamt, webfampck, webtot, " +
                    $"axorders, axreloads, axwebdisc, trretail, trwillcall, vcwillcall, axsales, notes, cashier1, cashier2, cashier3, cissued, creturn, cused, axused, " +
                    $"balflag, saledetot, emflag, ccsettled, crefno, vrefno, arefno, salestot, pmttot, artrantot, poskey, ccbatchnbr) VALUES " +
                    $"({FPDate} ,20 ,'WEB',0,{VISA.ToString()},{Mastercard.ToString()},{Discover.ToString()}, {Amex.ToString()}, {GiftUse.ToString()}, {GiftLoads.ToString()}, {CardTot.ToString()}, {CardTot.ToString()}, " +
                    $"0,0,0,0,0,0,0,0,{Math.Abs(AltaCredit1).ToString()},0,0,0,{WebAuthnet.ToString()},0,0,0,0," +
                    $"0,0,0,{AXTot.ToString()}, {WebShip.ToString()}, 0, {PassReloads.ToString()},{TicketNew.ToString()}, {TicketReload.ToString()}, {SkiSchool.ToString()}, {WebGift.ToString()}, {WebRefund.ToString()}, {RCBillAmount.ToString()}, {FamilyPackage.ToString()}, {SalesTotal.ToString()}, " +
                    $" 0, {AXPassReloads.ToString()}, {ReloadDisc.ToString()}, {TRRetail.ToString()}, {(WCReload + WCNew).ToString()}, {VCWillcall.ToString()}, {SalesTotal.ToString()},'' , '' ,'' ,'' ,0 ,0 ,0 ,0, " +
                    $".F. ,0 ,.F. , {FPDate}, '','' ,'' , 0, 0, 0, '', 0)";
                CF.ExecuteSQL(VFPConn, tQuery);
            }
        }

        private void SaveFields21()
        {
            //Update Mysql
            string tQuery = string.Empty;
            if (!CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.possum", $"saledate='{StartString}' AND pos=21"))
            {
                tQuery = $"INSERT INTO {DW.ActiveDatabase}.possum (saledate, pos, office, cash, cvisa, cmast, cdisc, camex, cgiftuse, cgiftadd, cardcheck, cardtot, " +
                    $"gcertqty, gcertamt, vchrqty, vchramt, snchk, arqty, aramt, arguest, acredit, webamt, totrec, axcash, axcard, axvchr, axsnchk, axcredit, axar, " +
                    $"axargst, axweb, axgift, axtot, webship, weborders, webreloads, webtickr, webtickn, webskisch, webgift, webrefund, ccbillamt, webfampck, webtot, " +
                    $"axorders, axreloads, axwebdisc, trretail, trwillcall, vcwillcall, axsales, notes, cashier1, cashier2, cashier3, cissued, creturn, cused, axused, " +
                    $"balflag, saledetot, emflag, ccsettled, crefno, vrefno, arefno, salestot, pmttot, artrantot, poskey, ccbatchnbr) VALUES " +
                    $"('{StartString}', 21, 'WEB', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0, 0, {(AxReceived + AxCancel).ToString()}, " +
                    $"0, 0, 0, {(WCSent + WCCredit).ToString()}, 0, 0, 0, 0, 0, 0, {(WCSent + WCCredit).ToString()}, 0, {(AxReceived + AxCancel).ToString()}, 0, " +
                    $"0, 0, 0, 0, null, '','', '', 0, 0, 0, 0, 0, 0, 0, null, '', '', '', 0, 0, 0, '', 0)";
            }
            else
            {
                tQuery = $"UPDATE {DW.ActiveDatabase}.possum SET axreloads = {(AxReceived + AxCancel).ToString()}, webtickr={(WCSent + WCCredit).ToString()}, webtot={(WCSent + WCCredit).ToString()}, axtot = {(AxReceived + AxCancel).ToString()} WHERE saledate='{StartString}' AND pos=21";
            }
            CF.ExecuteSQL(DW.dwConn, tQuery);
            //Update Foxpro
            string FPDate = CF.SetFoxProDate(RunDate);
            string FPNullDate = "{^0000-00-00}";
            VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}possum.dbf";
            if (CF.RowCount(VFPConn, $"possum", $"saledate={FPDate} AND pos=21") == 0)
            {
                //There is a problem with alta credits inserting as negative (acredit in table possum)
                tQuery = $"INSERT INTO possum (saledate, pos, office, cash, cvisa, cmast, cdisc, camex, cgiftuse, cgiftadd, cardcheck, cardtot, " +
                    $"gcertqty, gcertamt, vchrqty, vchramt, snchk, arqty, aramt, arguest, acredit, webamt, totrec, axcash, axcard, axvchr, axsnchk, axcredit, axar, " +
                    $"axargst, axweb, axgift, axtot, webship, weborders, webreloads, webtickr, webtickn, webskisch, webgift, webrefund, ccbillamt, webfampck, webtot, " +
                    $"axorders, axreloads, axwebdisc, trretail, trwillcall, vcwillcall, axsales, notes, cashier1, cashier2, cashier3, cissued, creturn, cused, axused, " +
                    $"balflag, saledetot, emflag, ccsettled, crefno, vrefno, arefno, salestot, pmttot, artrantot, poskey, ccbatchnbr) VALUES " +
                    $"({FPDate}, 21, 'WEB', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0, 0, {(AxReceived + AxCancel).ToString()}, " +
                    $"0, 0, 0, {(WCSent + WCCredit).ToString()}, 0, 0, 0, 0, 0, 0, {(WCSent + WCCredit).ToString()}, 0, {(AxReceived + AxCancel).ToString()}, 0, " +
                    $"0, 0, 0, 0, '', '','', '', 0, 0, 0, 0, 0, 0, 0, {FPNullDate}, '', '', '', 0, 0, 0, '', 0)";
            }
            else
            {
                tQuery = $"UPDATE possum SET axreloads = {(AxReceived + AxCancel).ToString()}, webtickr={(WCSent + WCCredit).ToString()}, webtot={(WCSent + WCCredit).ToString()}, axtot = {(AxReceived + AxCancel).ToString()} " +
                    $"WHERE saledate={FPDate} AND pos=21";
            }
            VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}possum.dbf";
            CF.ExecuteSQL(VFPConn, tQuery);

        }

        private void Update_ARTrans()
        {
            string tQuery = $"SELECT * FROM {DW.ActiveDatabase}.{ARTransTable} WHERE transdate = '{StartString}' AND posnbr IN (20, 21) ORDER BY transkey";
            ARTransXT = CF.LoadTable(DW.dwConn, tQuery, "ARTransXT");
            //*** update artrans with will call Ticket Sales(both new & reload) ***
            Willcall = CF.LoadTable(DW.dwConn, $"SELECT res_no, resprice, firstname, lastname, persdesc, tickdesc FROM {DW.ActiveDatabase}.willcall WHERE bookdate='{StartString}' AND company='DT'", "Willcall");
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
                if (!CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.{ARTransTable}", $"transdate = '{StartString}' AND posnbr IN (20, 21) AND transkey='{mTransKey}'"))
                {
                    //*** insert WC transactions into AR trans***
                    CF.ExecuteSQL(DW.dwConn, $"INSERT INTO {DW.ActiveDatabase}.{ARTransTable} (transdate, posnbr, transkey,transtype,arcustid,arname,firstname, lastname, transdesc, debitamt, creditamt, authcode, billdate, resno, dtupdate) " +
                        $"Values ('{StartString}', 20, '{mTransKey}', 'T', {lARID}, '{lARName}', '{mFName}', '{mLName}', " +
                        $"'{CF.EscapeChar(WillcallRow["tickdesc"].ToString())}-{CF.EscapeChar(WillcallRow["persdesc"].ToString())}', {mDebitAmt.ToString("0#.00")}, {mCreditAmt.ToString("0#.00")}, '{lARID}', " +
                        $"'{StartString}', '{WillcallRow["res_no"].ToString()}','{DateTime.Now.ToString(Mirror.AxessDateFormat)}')");
                }
            }
            //*** update artrans with authnet billings ***
            if (RCBillAmount != 0)
            {
                decimal mcreditamt = 0;
                decimal mdebitamt = 0;
                if (RCBillAmount > 0)
                {
                    mcreditamt = RCBillAmount;
                }
                else
                {
                    mdebitamt = RCBillAmount;
                }
                string mTransKey = $"20-{StartString}-RC";
                string tRecID = CF.GetSQLField(DW.dwConn, $"SELECT recid FROM {DW.ActiveDatabase}.{ARTransTable} WHERE transdate = '{StartString}' AND posnbr IN (20, 21) AND transkey='{mTransKey}' LIMIT 1");
                if (tRecID != null)
                {
                    tQuery = $"UPDATE {DW.ActiveDatabase}.{ARTransTable} SET debitamt={mdebitamt.ToString("0#.00")}, creditamt={mcreditamt.ToString("0#.00")}, dtupdate='{DateTime.Now.ToString(Mirror.AxessDateFormat)}' WHERE recid='{tRecID}'";
                }
                else
                {
                    //*** insert RESORT CHARGE RECEIPTS into AR trans ***
                    tQuery = $"INSERT INTO {DW.ActiveDatabase}.{ARTransTable} (transdate, posnbr, transkey, transtype, arcustid, arname, transdesc, debitamt, creditamt, " +
                        $"authcode, billdate, dtupdate) " +
                        $"Values ('{StartString}', 20,'{mTransKey}', 'E', '11874', 'ASL - PASS CHARGES', 'AUTHNET BILLING', {mdebitamt.ToString("0#.00")}, {mcreditamt.ToString("0#.00")}, '11874', '{StartString}', '{DateTime.Now.ToString(Mirror.AxessDateFormat)}')";
                }
                CF.ExecuteSQL(DW.dwConn, tQuery);
            }
            //*** update artrans with gift cards ***
            foreach (DataRow WebSale in WebSales.Rows)
            {
                //SET FILTER TO ALLTRIM(websales.status) = "APPROVED" AND ALLTRIM(websales.product_NAME) = 'Alta Gift Card'
                if (WebSale["status"].ToString() == "APPROVED" && WebSale["product_name"].ToString().Contains("Gift"))
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
                    if (!CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.{ARTransTable}", $"transkey={mTransKey} AND  transdate = '{StartString}' AND posnbr IN (20, 21)"))
                    {
                        //*** insert WC totals into AR trans***
                        tQuery = $"INSERT INTO {DW.ActiveDatabase}.{ARTransTable} (transdate, posnbr, transkey, transtype, arcustid, arname, firstname, lastname, transdesc, debitamt, creditamt, authcode, billdate, resno, dtupdate) Values( '" +
                            $"{StartString}', 20, '{mTransKey}', 'E', '11759', 'ESTORE GIFT CARDS', '{mFName}','{mLName}','{WebSale["product_name"].ToString()}', {mdebitamt.ToString("0#.00")}, {mcreditamt.ToString("0#.00")}, '11759', '{StartString}', " +
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
            if (PassReloads != AXPassReloads)
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
                //if (WebSale["axess_passtype"].ToString() != "reload" && WebSale["product_group"].ToString().Substring(0, 6) == "season" && WebSale.Field<UInt32>("tickettype") != 123)
                //{
                //    if (!CF.RowExists(am.MirrorConn, $"{am.ActiveDatabase}.tabaxorderdata", $"exorderid='{loid}'"))
                //    {
                //        WebSale.SetField<string>("confirmed", "NOT IN AXESS");
                //    }

                //}
                //** if SP Reload**
                if (WebSale["axess_passtype"].ToString() == "reload" && WebSale["product_group"].ToString().Substring(0, 6) == "season" && WebSale.Field<UInt32>("tickettype") != 123)
                {
                    if (!CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.salesdata", $"nseriennr={WebSale["nserialno"].ToString()}"))
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
                    if (WebSale["product_group"].ToString().Substring(0, 6) == "season" && WebSale.Field<UInt32>("tickettype") != 123)
                    {
                        if (WebSale["axess_passtype"].ToString() != "reload") //orders
                        {
                            tWhere = $"eitemid={litem.ToString()}";
                        }
                        else    //reloads
                        {
                            tWhere = $"eskey='{lskey}'";
                        }
                        if (!CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.weberrors", $"{tWhere} AND esdate='{StartString}'"))
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

            //*** REVIEW AXESS RELOADS ***
            System.Data.DataTable AXReloads = CF.LoadTable(DW.dwConn, $"SELECT saledate, nkassanr, nseriennr, fname,  lname, nkartennr, tickdesc, persdesc, tariff, SPACE(20) as confirmed FROM {DW.ActiveDatabase}.salesdata WHERE saledate = '{StartString}'", "AXReloads");
            foreach (DataRow AXReload in AXReloads.Rows)
            {
                        //string lskey = $"{AXReload["nkassanr"].ToString()}-{AXReload["nseriennr"].ToString()}";
                        //if (!CF.RowExists(am.MirrorConn, $"{am.ActiveDatabase}.taborderlist WHERE created = '{sStart}' AND "))
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
            txtAuthNetGiftCardUse.Enabled = true;
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
            btnLoadData.Enabled = true;
            //mStart = dtpWorkDate.Value;
            //sStart = mStart.ToString(Mirror.AxessDateFormat);
            //LoadData();
        }
        private void WebsalesReport(DataRow[] WebSalesX, string Title = "")
        {
            Cursor = Cursors.WaitCursor;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook wb = excel.Workbooks.Open(gDrive + @"\WebSales2.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"EStore Sales Report{(Title == string.Empty ? "": $" - {Title}").ToString()}\n{StartString}";
            int CurRow = 2;
            Int32 TotQty = 0;
            decimal TotAmt = 0;
            foreach (DataRow WebSaleX in WebSalesX)
            {
                ws.Cells[CurRow, 1] = WebSaleX["Order_ID"].ToString();
                ws.Cells[CurRow, 2] = WebSaleX["SerialKey"].ToString();
                ws.Cells[CurRow, 3] = WebSaleX["product_name"].ToString();
                ws.Cells[CurRow, 4] = WebSaleX["person_firstname"].ToString() + " " + WebSaleX["person_lastname"].ToString();
                ws.Cells[CurRow, 5] = WebSaleX.Field<decimal>("price").ToString("0#.00");
                ws.Cells[CurRow, 6] = WebSaleX["qty"].ToString();
                TotQty += Convert.ToInt32(WebSaleX.Field<UInt32>("qty"));
                ws.Cells[CurRow++, 7] = WebSaleX.Field<decimal>("total").ToString("0#.00");
                TotAmt += WebSaleX.Field<decimal>("total");
            }
            CurRow += 2;
            ws.Cells[CurRow, 4] = "Totals:";
            ws.Cells[CurRow, 6] = TotQty.ToString();
            ws.Cells[CurRow, 7] = TotAmt.ToString("0#.00");
            excel.Visible = true;
            ws.PrintOutEx(Type.Missing, Type.Missing, 1, true);
            excel.Visible = false;
            wb.Close(false);
            Cursor = Cursors.Default;
        }

        private void BtnPassCardOrders_Click(object sender, EventArgs e)
        {
            //order, probably going away
            //System.Data.DataTable WebSalesX = CF.LoadTable(AltaShop.Shop_Alta_ComConn, $"SELECT OI.id, OI.order_id, concat_ws('-', T.nposno, T.nserialno, T.nunicodeno) AS serialkey, OI.product_name, OI.person_firstname, OI.person_lastname, OI.price, OI.quantity AS qty, OI.total FROM altashop.order_items AS OI INNER JOIN altashop.axess_transactions AS T ON T.order_item_id = OI.id WHERE OI.created_at >= '{StartString}' AND OI.created_at < '{EndString}' AND OI.axess_status NOT IN ('SKIPPED', 'DO-NOT-PROCESS')", "WebSalesX");
            //DataRow[] WebSale = WebSalesX.Select();
            //WebsalesReport(WebSale, "Pass Card Orders");
        }

        private void BtnPrepaidsReloads_Click(object sender, EventArgs e)
        {
            System.Data.DataTable WebSalesX = CF.LoadTable(AltaShop.Shop_Alta_ComConn, $"SELECT OI.id, OI.order_id, concat_ws('-', T.nposno, T.nserialno, T.nunicodeno) AS serialkey, OI.product_name, OI.person_firstname, OI.person_lastname, OI.price, OI.quantity AS qty, OI.total FROM altashop.order_items AS OI INNER JOIN altashop.axess_transactions AS T ON T.order_item_id = OI.id INNER JOIN altashop.orders AS O ON O.id = OI.order_id WHERE O.created_at >= '{StartString}' AND O.created_at < '{EndString}' AND OI.axess_status NOT IN ('SKIPPED', 'DO-NOT-PROCESS')", "WebSalesX");
            DataRow[] WebSale = WebSalesX.Select();
            //DataRow[] WebSale = WebSales.Select("status IN ('APPROVED','CREDIT_APPLIED') AND axess_passtype='reload' AND product_group NOT LIKE 'date%' AND status <> 'SKIPPED'");
            WebsalesReport(WebSale, "Prepaids & Reloads");

        }

        private void BtnDateSpecificReloads_Click(object sender, EventArgs e)
        {
            System.Data.DataTable WebSalesX = CF.LoadTable(AltaShop.Shop_Alta_ComConn, $"SELECT OI.id, OI.order_id, concat_ws('-', T.nposno, T.nserialno, T.nunicodeno) AS serialkey, OI.product_name, OI.person_firstname, OI.person_lastname, OI.price, OI.quantity AS qty, OI.total FROM altashop.order_items AS OI LEFT JOIN altashop.axess_transactions AS T ON T.order_item_id = OI.id INNER JOIN altashop.orders AS O ON O.id = OI.order_id WHERE O.created_at >= '{StartString}' AND O.created_at < '{EndString}' AND OI.axess_status IN ('SKIPPED', 'DO-NOT-PROCESS') AND (AXESS_PASSTYPE = 'reload' and product_group like 'date%')", "WebSalesX");
            DataRow[] WebSale = WebSalesX.Select();
            //DataRow[] WebSale = WebSales.Select("status = 'APPROVED' AND axess_passtype='reload' AND product_group LIKE 'date%'");
            WebsalesReport(WebSale, "Specific Reloads");
        }

        private void BtnTicketReservations_Click(object sender, EventArgs e)
        {
            System.Data.DataTable WebSalesX = CF.LoadTable(AltaShop.Shop_Alta_ComConn, $"SELECT I.id, I.order_id, concat_ws('-', T.nposno, T.nserialno, T.nunicodeno) AS serialkey, I.product_name, I.person_firstname, I.person_lastname, I.price, I.quantity AS qty, I.total FROM altashop.order_items AS I INNER JOIN altashop.orders AS O ON O.id = I.order_id LEFT JOIN altashop.payments AS P ON P.order_id = O.ID LEFT JOIN altashop.axess_transactions AS T ON T.order_item_id = I.id WHERE O.created_at >= '{StartString}' AND O.created_at < '{EndString}' AND UPPER(I.axess_status) IN ('SKIPPED' , 'DO-NOT-PROCESS') AND ((UPPER(I.AXESS_PASSTYPE) = 'NEW' OR UPPER(I.AXESS_PASSTYPE) = 'ALTA') AND (I.product_group LIKE 'good%' OR I.product_group LIKE 'date%'))", "WebSalesX");
            DataRow[] WebSale = WebSalesX.Select();
            //DataRow[] WebSale = WebSales.Select("status = 'APPROVED' AND (axess_passtype='new' AND (product_group LIKE 'date%' OR product_group LIKE 'good%')) OR axess_passtype='altacard'");
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
          
            Workbook wb = excel.Workbooks.Open(gDrive + @"\RCBill.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"EStore Sales Report{(Title == string.Empty ? "" : $" - {Title}").ToString()}\r\n{StartString}";
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
                    ws.Cells[CurRow, 3] = RCBX.Field<int>("tokenkey");
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

        private void BtnAuthNetRefunds_Click(object sender, EventArgs e) => RCBillReport(ARTrans.Select($"billdate='{StartString}' AND authcode <> '' AND transtype='R'"));
        // ORDER BY authcode, billdate

        private void BtnRCBillingTotal_Click(object sender, EventArgs e) => RCBillReport(ARTrans.Select($"billdate='{StartString}' AND authcode <> '' AND transtype<>'R'"));
        // ORDER BY authcode, billdate

        private void ReloadReport(int POS)
        {
            Cursor = Cursors.WaitCursor;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook wb = excel.Workbooks.Open(gDrive + @"\AxSales.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"Axess Sales Report - POS {POS}  {StartString}";
            int CurRow = 2;
            int Qty = 0;
            decimal Tot = 0;
            foreach (DataRow SaleDay in SalesDay.Select($"NKASSANR={POS}"))
            {
                ws.Cells[CurRow, 1] = SaleDay["SERIALKEY"].ToString();
                ws.Cells[CurRow, 2] = SaleDay["TICKDESC"].ToString() + " " + SaleDay["PERSDESC"].ToString();
                ws.Cells[CurRow, 3] = SaleDay["FNAME"].ToString() + " " + SaleDay["LNAME"].ToString();
                ws.Cells[CurRow, 4] = SaleDay.Field<int>("QTY").ToString();
                ws.Cells[CurRow++, 5] = SaleDay.Field<decimal>("TARIFF").ToString("0#.00");
                Qty += SaleDay.Field<int>("QTY");
                Tot += SaleDay.Field<decimal>("TARIFF");
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
        private void SummaryReport(int POS)
        {
            Cursor = Cursors.WaitCursor;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();           
            Workbook wb = excel.Workbooks.Open(gDrive + @"\EstoreSummaryReport.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"Estore Summary Checkout Report - POS 20/21  {StartString}";
            //LEFT SIDE COLUMNS FOR SUMMARY REPORT
            ws.Cells[3, 3] = PassReloads.ToString(DFormat);
            ws.Cells[6, 3] = TicketReload.ToString(DFormat);
            ws.Cells[7, 3] = TicketNew.ToString(DFormat);
            ws.Cells[9, 3] = FamilyPackage.ToString(DFormat);
            ws.Cells[10, 3] = SkiSchool.ToString(DFormat);
            ws.Cells[11, 3] = WebGift.ToString(DFormat);
            ws.Cells[12, 3] = RCBillAmount.ToString(DFormat);
            ws.Cells[13, 3] = WebRefund.ToString(DFormat);
            ws.Cells[14, 3] = WebShip.ToString(DFormat);
            ws.Cells[16, 3] = SalesTotal.ToString(DFormat);
            ws.Cells[19, 3] = WebAuthnet.ToString(DFormat);
            ws.Cells[21, 3] = WebGiftUse.ToString(DFormat);
            ws.Cells[22, 3] = WebCoupon.ToString(DFormat);
            ws.Cells[24, 3] = AXTot.ToString(DFormat);

            //RIGHT SIDE FEILDS FOR SUMMARY REPORT
            ws.Cells[3, 6] = ReloadTot.ToString(DFormat);
            ws.Cells[6, 6] = WCReload.ToString(DFormat);
            ws.Cells[7, 6] = WCNew.ToString(DFormat);
            
            ws.Cells[15, 6] = VISA.ToString(DFormat);
            ws.Cells[16, 6] = Mastercard.ToString(DFormat);
            ws.Cells[17, 6] = Discover.ToString(DFormat);
            ws.Cells[18, 6] = Amex.ToString(DFormat);
            ws.Cells[19, 6] = CardTot.ToString(DFormat);
            ws.Cells[21, 6] = GiftUse.ToString(DFormat);
            ws.Cells[22, 6] = Math.Abs(AltaCredit1).ToString(DFormat);
            ws.Cells[24, 6] = TotRec.ToString(DFormat);

            


            //DATE SPECIFIC RELOAD FEILDS
            ws.Cells[29, 5] = WCSent.ToString(DFormat);
            ws.Cells[29, 6] = AxReceived.ToString(DFormat);
            ws.Cells[30, 5] = WCCredit.ToString(DFormat);
            ws.Cells[30, 6] = AxCancel.ToString(DFormat);

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
            Workbook wb = excel.Workbooks.Open(gDrive + @"\WebSales.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"{Title}\r\n{StartString}";
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

        private void BtnWillcallDateSpecificReloads_Click(object sender, EventArgs e) => WillcallReport(Willcall.Select($"bookdate='{StartString}' AND company='DT' AND tickcode='REL'"));

        private void BtnWillcallTicketReservations_Click(object sender, EventArgs e) => WillcallReport(Willcall.Select($"bookdate='{StartString}' AND company='DT' AND tickcode IN ('NEW','ALT')"));


        private void BtnCreditUses_Click(object sender, EventArgs e)
        {
            DataRow[] AltaCreditX = AltaCredit.Select($"transactiondate='{StartString}' AND POS=20");
            //run altacredit report
            Cursor = Cursors.WaitCursor;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook wb = excel.Workbooks.Open(gDrive + @"\AltaCredit.xlsx");
            Worksheet ws = wb.Worksheets[1];
            ws.PageSetup.CenterHeader = $"Alta Credits - Settlement report for POS 20 on {StartString}.";
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

        private void BtnPreviousDay_Click(object sender, EventArgs e) => dtpWorkDate.Value = dtpWorkDate.Value.AddDays(-1);

        private void BtnNextDay_Click(object sender, EventArgs e) => dtpWorkDate.Value = dtpWorkDate.Value.AddDays(1);

        private void CalChargeChanges()
        {
            VISA = Convert.ToDecimal(txtVISA.Text);
            Mastercard = Convert.ToDecimal(txtMastercard.Text);
            Discover = Convert.ToDecimal(txtDiscover.Text);
            Amex = Convert.ToDecimal(txtAMEX.Text);
            GiftUse = Convert.ToDecimal(txtAuthNetGiftCardUse.Text);
            //TotRec = Convert.ToDecimal(txtAuthNetTotal.Text) + Convert.ToDecimal(txtAuthNetGiftCardUse.Text) + Convert.ToDecimal(txtCreditUses.Text);
            SetFields();
        }

        private void TxtVISA_Leave(object sender, EventArgs e) => CalChargeChanges();

        private void TxtMastercard_Leave(object sender, EventArgs e) => CalChargeChanges();

        private void TxtAMEX_Leave(object sender, EventArgs e) => CalChargeChanges();

        private void TxtDiscover_Leave(object sender, EventArgs e) => CalChargeChanges();

        private void txtAuthNetGiftCardUse_Leave(object sender, EventArgs e) => CalChargeChanges();

        private void btnGiftCards_Click(object sender, EventArgs e)
        {
            System.Data.DataTable WebSalesX = CF.LoadTable(AltaShop.Shop_Alta_ComConn, $"SELECT OI.id, OI.order_id, concat_ws('-', T.nposno, T.nserialno, T.nunicodeno) AS serialkey, OI.product_name, OI.person_firstname, OI.person_lastname, OI.price, OI.quantity AS qty, OI.total FROM altashop.order_items AS OI LEFT JOIN altashop.axess_transactions AS T ON T.order_item_id = OI.id WHERE OI.created_at >= '{StartString}' AND OI.created_at < '{EndString}' AND OI.product_group LIKE '%GIFT%'", "WebSalesX");
            DataRow[] WebSale = WebSalesX.Select();
            //DataRow[] WebSale = WebSales.Select("status IN ('APPROVED','CREDIT_APPLIED') AND axess_passtype='reload' AND product_group NOT LIKE 'date%' AND status <> 'SKIPPED'");
            WebsalesReport(WebSale, "Prepaids & Reloads");
        }

    }
}
