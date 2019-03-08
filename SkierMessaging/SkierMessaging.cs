using System;
using System.Data;
using System.IO;
using System.Web;
using System.Windows.Forms;
using asl_SyncLibrary;
using BarcodeLib;

namespace SkierMessaging
{
    public partial class SkierMessaging : Form
    {
        string[] TArgs; 
        DateTime StartDate = new DateTime(2017, 7, 1);

        Mirror AM = new Mirror();
        CommonFunctions CF = new CommonFunctions();
        DataWarehouse DW = new DataWarehouse();
        ASLEmail EM = new ASLEmail();

        string Tab5 = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";

        const string GrayBar = "<hr style='height:20px;border-width:0;color:gray;background-color:gray'>";

        private string AlternateBar(string NewColor = "black", int Height = 20)
        {
            return GrayBar.Replace("gray", NewColor).Replace("20",Height.ToString());

        }

        public SkierMessaging(string[] args = null)
        {
            InitializeComponent();
            TArgs = args;

        }

        private void SkierMessaging_Load(object sender, EventArgs e)
        {
            LblStartTime.Text = $"Start Time: {DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}";
        }

        private void SkierMessaging_Shown(object sender, EventArgs e)
        {
            //put a switch in here for args[0], until then assume only Purchase Confirmation
            //for (int i = 1;i=)
            //string[,] ReceiptLines = BuildReceipt("17-100820");
            //EmailAll();
            //set email flag in salesdata
            DataTable SalesUpdates = CF.LoadTable(DW.dwConn, $"SELECT A.TRANSKEY FROM {DW.ActiveDatabase}.salesdata AS A INNER JOIN applications.pmtdata AS B ON A.TRANSKEY=B.TRANSKEY WHERE A.NTRANSTYPE=0 AND A.SALEDATE >='2018-04-17' AND A.TGROUP IN ('EX','SP') AND A.email<>'' AND A.email_send='N' AND B.PMTKEY NOT IN ('120-1','10510','106-1') AND A.DOB<='2000-04-17' GROUP BY A.TRANSKEY","SalesUpdates");
            if (SalesUpdates != null)
            {
                String CurSales = string.Empty;
                foreach (DataRow tDR in SalesUpdates.Rows)
                {
                    CurSales = CF.AddToList(CurSales,"'" + tDR["transkey"].ToString().Trim() + "'");
                }
                if (CurSales != string.Empty)
                {
                    CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET email_send='Y' WHERE TRANSKEY IN ({CurSales})");
                }
            }
            EmailTransactions($"SELECT A.transkey, A.nperskassa, A.npersnr, A.NKASSANR, A.saledate, A.email FROM applications.salesdata AS A INNER JOIN applications.pmtdata AS B ON B.transkey = A.transkey WHERE A.nkassanr NOT IN (3, 20, 21, 22) AND A.email <> '' AND A.email_send = 'Y' AND A.email_sent = 'N' AND A.saledate > '2018-04-17' AND A.tgroup IN ('SP', 'EX') AND B.PTYPE <> 'AR' AND A.DOB <= '2000-04-17' GROUP BY A.SALEDATE, A.email, A.TRANSKEY ORDER BY A.SALEDATE, A.TRANSKEY");
            if (TArgs == null)
            {
                Application.Exit();
            }
        }

        private string[,] BuildReceipt(string TransKey)
        {
            DataTable PmtData = CF.LoadTable(DW.dwConn, $"SELECT saledate, pmtamt, ptype FROM {DW.ActiveDatabase}.pmtdata WHERE transkey='{TransKey}'", "PmtData");
            if (PmtData != null)
            {
                int PmtCnt = PmtData.Rows.Count;
                string[,] RcptLine = new string[PmtCnt,2];
                int CurPmt = 0;
                foreach (DataRow PmtRow in PmtData.Rows)
                {
                    RcptLine[CurPmt, 0] = PaymentType(PmtRow["ptype"].ToString());
                    RcptLine[CurPmt++, 1] = PmtRow["pmtamt"].ToString();
                }
                return RcptLine;
            }
            return null;
        }

        private string PaymentType(string PType)
        {
            switch (PType)
            {
                case "CC":
                    return "Credit Card:";
                case "$":
                    return "Cash:";
                case "V":
                    return "Voucher:";
                case "G":
                    return "Gift Card:";
                case "AC":
                    return "Alta Credit:";
                case "AG":
                    return "Pass Charge:";
                case "AR":
                    return "AR:";
            }
            return string.Empty;
        }


        private void EmailTransactions(string aQuery)
        {
            DataTable Transactions = CF.LoadTable(DW.dwConn, aQuery, "Transactions");
            if (Transactions != null)
            {
                foreach (DataRow Transaction in Transactions.Rows)
                {
                    string TransKey = Transaction["Transkey"].ToString();
                    string[,] Receipt = BuildReceipt(TransKey);
                    if (Receipt != null)
                    {
                        string FormattedReceipt = string.Empty;
                        decimal TotalBilled = 0;
                        for (int i = Receipt.GetLowerBound(0); i <= Receipt.GetUpperBound(0); i++)
                        {
                            TotalBilled += Convert.ToDecimal(Receipt[i, 1]);
                            FormattedReceipt += $"<tr><td align='left'>{Receipt[i, 0].Trim()}</td><td align='right'>${Receipt[i, 1].Trim()}</td></tr>";
                        }
                        if (FormattedReceipt != string.Empty)
                        {
                            //build the receipt
                            FormattedReceipt = $"<table align='right'; width=100%><tr><th align='left';width=75%></th><th align='right';width=25%></th></tr>{FormattedReceipt}<tr><td align='left'>Total:</td><td align='right'>{TotalBilled.ToString("$0.00")}</td></tr></table>";
                            //build purchaser address box
                            DataRow Purchaser = CF.LoadDataRow(AM.MirrorConn, $"SELECT lastname, firstname, gender, dob, cheight, phone, email, street, city, state, zip, country FROM {AM.ActiveDatabase}.tabaxcust WHERE nperskassa={Transaction["nperskassa"].ToString()} AND npersnr={Transaction["npersnr"].ToString()} LIMIT 1");
                            string FormattedUserData = $"<table><tr><td>{Purchaser["firstname"].ToString()} {Purchaser["lastname"].ToString()}</td></tr>" +
                                $"<tr><td>{Purchaser["street"].ToString()}</td></tr>" +
                                $"<tr><td>{Purchaser["city"].ToString()}, {Purchaser["state"].ToString()}  {Purchaser["zip"].ToString()}</td></tr></table>";
                            //build Transaction Table
                            string TransactionTable = $"<table width=100%><th align='left'; width = 80%><b>Transaction# {TransKey},  Sale Date: {Transaction.Field<DateTime>("saledate").ToString(Mirror.AxessDateFormat)}</b></th><th align='center'; width=20%><b>Payment{(Receipt.GetUpperBound(0) != 0 ? "s" : "")} Received</b></th><tr><td>{FormattedUserData}</td><td valign='top'>{FormattedReceipt}</td></tr></table>";
                            //get the sales rows and build their tables.
                            DataTable Sales = CF.LoadTable(DW.dwConn, $"SELECT SERIALKEY, SUM(tfactor) AS Active, COUNT(*) AS Cnt, NPERSKASSA, NPERSNR, tickdesc, persdesc, tariff, wtp64, nkassanr, nseriennr, nunicodenr  FROM {DW.ActiveDatabase}.salesdata WHERE transkey='{TransKey}' GROUP BY serialkey", "SalesData");
                            string FormattedSalesData = $"<table width=100%><th text-align: left;width=100%;height=56px></th><th text-align: right;width=210px;height=56px></th>";
                            string[] Attachments = new string[Sales.Rows.Count];
                            int AttachmentsCount = 0;
                            foreach (DataRow Sale in Sales.Rows)
                            {
                                if (Sale.Field<decimal>("Active") > 0)
                                {
                                    string WTP64 = Sale["wtp64"].ToString().Trim();
                                    DataRow Skier = CF.LoadDataRow(AM.MirrorConn, $"SELECT lastname, firstname, gender, dob, cheight FROM {AM.ActiveDatabase}.tabaxcust WHERE nperskassa={Sale["nperskassa"].ToString()} AND npersnr={Sale["npersnr"].ToString()} LIMIT 1");
                                    string PassInfo = (WTP64 == string.Empty ? "NEW PASS  (See Note B)" : $"WTP# {WTP64} RELOAD (See Note A)");
                                    string SkierName = "";
                                    if (Skier != null)
                                    {
                                        SkierName = $"{Skier["firstname"].ToString()} {Skier["lastname"].ToString()}";
                                    }
                                    FormattedSalesData += $"<tr><td><b>{SkierName}</b></td><td></td></tr>" +
                                        $"<tr><td>{Tab5}{Sale["tickdesc"].ToString()} -- {Sale["persdesc"].ToString()}</td><td align='right'>{Sale.Field<decimal>("tariff").ToString("$0.00")}</td></tr>";
                                    if (!Sale["tickdesc"].ToString().ToUpper().Contains("PACKAGE"))
                                    { 
                                        if (WTP64 != string.Empty)
                                        {
                                            FormattedSalesData += $"<tr><td>{Tab5}<b>RELOAD</b> (See note A)</td><td></td></tr>" +
                                                $"<tr><td>{Tab5}<b>Web ID:</b> {WTP64}</td><td></td></tr>";
                                        }
                                        else
                                        {
                                            string ExternalID = CF.GetSQLField(AM.MirrorConn, $"SELECT szauftragext FROM {AM.ActiveDatabase}.tabprepaidtickets WHERE nkassanr={Sale["nkassanr"].ToString()} AND nseriennr={Sale["nseriennr"].ToString()} AND nunicodenr={Sale["nunicodenr"].ToString()}");
                                            if (ExternalID != string.Empty)
                                            {
                                                System.Diagnostics.Debug.Print($"{ExternalID} -- nkassanr={Sale["nkassanr"].ToString()} AND nseriennr={Sale["nseriennr"].ToString()} AND nunicodenr={Sale["nunicodenr"].ToString()}");
                                                BarcodeLib.Barcode BCode = new BarcodeLib.Barcode();
                                                System.Drawing.Image BCodeImage = BCode.Encode(TYPE.CODE128, ExternalID.Trim(), System.Drawing.Color.Black, System.Drawing.Color.White, 200, 50);
                                                string tFile = $"{ExternalID}.jpg";
                                                bool Found = false;
                                                for (int i=0; i<AttachmentsCount;i++)
                                                {
                                                    if (Attachments[i] == tFile)
                                                    {
                                                        Found = true;
                                                        break;
                                                    }
                                                }
                                                if (!Found)
                                                {
                                                    Attachments[AttachmentsCount++] = tFile;
                                                }
                                                string tImage = $@"C:/temp/{tFile}";
                                                if (File.Exists(tImage))
                                                {
                                                    //File.Delete(tImage);
                                                }
                                                else
                                                {
                                                    BCodeImage.Save(tImage, System.Drawing.Imaging.ImageFormat.Jpeg);
                                                }
                                                FormattedSalesData += $"<tr><td>{Tab5}NEW PASS (See note B)</td><td></td></tr>" +
                                                    $"<tr><td>{Tab5}OrderID: {ExternalID}</td><td>{$@"<img src=""{tFile}"" alt='{ExternalID}'>"}</td></tr>";
                                            }
                                        }
                                    }
                                    FormattedSalesData += $"<tr><td>{AlternateBar("white",5)}</td><td></td></tr>";
                                }
                            }

                            FormattedSalesData += $"</table>";
                            string EmailData = $"<img src='http://www.alta.com/resources/Media/logos/altalogo.jpg' alt='Alta Ski Area'; width=25%><br>" +
                                $"<h1>Thank you for your purchase.</h1><br>Your order information appears below." +
                                GrayBar + TransactionTable + GrayBar + FormattedSalesData +
                                $"{AlternateBar("white", 10)}<b>What You Need To Know:<br><br>(A) RELOADS - </b>Your pass is activated.  Enjoy direct-to-lift access and remember the pass works best when placed in a pocket by itself.  Please verify the Web ID on your pass matches the Web ID on the receipt.<br>{AlternateBar("white", 5)}<b>(B) NEW PASSES - </b>Your pass must be picked up in person.<br>{AlternateBar("white", 5)}" +

                                //regular season
                                //$"<b>Season Pass Office Locations & Hours:<br>Alta Season Pass Offices<br>Summer</b> through November 5th<br>Monday - Thursday, 8:00am - 4:30pm " +
                                //END regular season

                                //Summer Season
                                $"<table style='width:7.5in'>" +
                                $"<table style='width:7.5in'><tr><td align='left'; width=100%><b>PASS PICKUP OPTIONS</b></td></tr></table>" +
                                $"<table cellspacing='0';cellpadding='0';width=100%><tr><th width=5%></th><th align='left';width=20%></th><th align='left';width=35%></th><th align='left';width=5%></th><th align='left';width=35%></th></tr>" +
                                $"<tr><td></td><td>Monday — Thursday</td><td>(Through Nov. 4th, 7 days a week following)</td><td></td><td><table width=100%><tr><td width=30%;align='left'>7:30 to 4:30 pm</td><td width=70%;align='right'>Skier Services</td></tr></table></td></tr>" +
                                $"<tr><td></td><td>Saturday</td><td>(Through Nov. 10th)</td><td></td><td><table width=100%><tr><td align='left';width = 30%>9 to 5 pm</td><td align='right';width=70%>Wasatch Powder House<br>3138 E 6200 S Holladay, UT</td></tr></table></td></tr></table><br><br>" +
                                $"<table width=100%>There is a chill in the air which means that winter is coming.   The countdown has begun with Alta's opening day only {(new DateTime(2018, 11, 24) - DateTime.Now).Days.ToString()} days away (the day after Thanksgiving).  Thanks for skiing Alta.<br></table>" +
                            //END Summer Season


                                $"<br>For questions or further assistance <i><b>do not</b></i> reply to this email.  Call us at (801) 359-1078 or email <i>info@alta.com</i>." +
                                $"<br>Thank you for skiing Alta." +
                                $"<br><b>Alta Ski Area</b>";
                            //send email
                            if (Purchaser["email"].ToString().Contains("@"))
                                {
                                EM.ToAddress = Purchaser["email"].ToString();
                                EM.FromAddress = "info@alta.com";
                                //EM.BCCAddress = "darylb@alta.com";
                                EM.Subject = $"Alta Purchase ({Transaction["saledate"].ToString()})";
                                EM.Body = EmailData;
                                EM.CreateEmail();
                                EM.IsHTML = true;
                                for (int i = 0; i < Sales.Rows.Count; i++)
                                {
                                    if (Attachments[i] != string.Empty && Attachments[i] != null)
                                    {
                                        EM.Body.Replace(Attachments[i], EM.AttachJpeg($@"C:\temp\{Attachments[i]}"));
                                        //File.Delete($@"C:\temp\{Attachments[i]}");
                                    }
                                }
                                if (EM.SendEmail())
                                {
                                    CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET email_sent='Y' WHERE transkey='{Transaction["transkey"].ToString().Trim()}' AND email_send='Y'");
                                }
                            }
                            for (int i = 0; i < Sales.Rows.Count; i++)
                            {
                                //File.Delete($@"C:\temp\{Attachments[i]}");
                            }

                        }
                    }
                }
            }
        }
        private string MakeImageSrcData(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                byte[] filebytes = new byte[fs.Length];
                fs.Read(filebytes, 0, Convert.ToInt32(fs.Length));
                return "data:image/jpg;base64," + Convert.ToBase64String(filebytes, Base64FormattingOptions.None);
            }
        }

        private void EmailAll()
        {
            EmailTransactions($"SELECT transkey, nperskassa, npersnr, saledate FROM {DW.ActiveDatabase}.salesdata WHERE email_send='Y' AND email_sent='N' AND email<>'' AND saledate>'{StartDate.ToString(Mirror.AxessDateFormat)}' AND tgroup IN ('SP','EX')");
        }

        private void EmailByPOS(string aPOSList)
        {
            string Query = $"SELECT transkey FROM {DW.ActiveDatabase}.salesdata WHERE email_send='Y' AND email_sent<>'N' AND email<>'' AND saledate>'{StartDate.ToString(Mirror.AxessDateFormat)}' AND tgroup IN ('SP','EX') AND nkassanrIN{aPOSList} GROUP BY transkey ";

        }


    }
}
