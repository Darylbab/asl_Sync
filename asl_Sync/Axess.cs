using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using MySql.Data.MySqlClient;

namespace asl_SyncLibrary
{
    public class Axess_DCI4CRM
    {
        //CONSTANTS
        private const string AXURL = "http://cwc.teamcom:16302/axis_705/services/DCI4CRM?wsdl";
        private const string uName = "705";
        private const string pWord = "705";


        //LOCAL VARIABLES
        private long tSessionID = 0;
        private int tErrorNo = 0;
        private string tErrorMsg = "";
        private long tProjectNo = 0;
        private long tPOSNo = 0;
        private string tActiveTable = "";
        private long tLogNoStart = 0;
        private long tLogNoEnd = 0;
        private long tSnapshotCount = 0;
        private bool tIncompleteSnapshot = false;
        private DateTime? loginTimestamp = null;

        private CommonFunctions cf = new CommonFunctions();
        private ASLError TError = new ASLError();
        //private Asgard ASG = new Asgard();
        //private DataWarehouse dw = new DataWarehouse();

        //PROPERTIES
        public long SessionID
        {
            get { return tSessionID; }
            set { tSessionID = value; }
        }
        public int ErrorNo
        {
            get { return tErrorNo; }
            set { tErrorNo = value; }
        }
        public string ErrorMsg
        {
            get { return tErrorMsg; }
            set { tErrorMsg = value; }
        }
        public long ProjectNo
        {
            get { return tProjectNo; }
            set { tProjectNo = value; }
        }
        public long POSNo
        {
            get { return tPOSNo; }
            set { tPOSNo = value; }
        }
        public string ActiveTable
        {
            get { return tActiveTable; }
            set { tActiveTable = value; }
        }
        public long LogNoStart
        {
            get { return tLogNoStart; }
            set { tLogNoStart = value; }
        }

        public long LogNoEnd
        {
            get { return tLogNoEnd; }
            set { tLogNoEnd = value; }
        }

        public long SnapshotCount
        {
            get { return tSnapshotCount; }
        }

        public bool IncompleteSnapshot
        {
            get { return tIncompleteSnapshot; }
        }


        private void ClearAll()
        {
            tSessionID = 0;
            tErrorNo = 0;
            tErrorMsg = "";
            tProjectNo = 0;
            tPOSNo = 0;
            tLogNoEnd = 0;
            tLogNoStart = 0;
            tActiveTable = "";
            loginTimestamp = null;
        }

        private bool tTestMode = false;

        public bool TestMode
        {
            get
            {
                return tTestMode;
            }
            set
            {
                tTestMode = value;
            }
        }


        //PUBLIC FUNCTIONS

        public Serverstatus GetServerStatus(string aURL)
        {
            try
            {
                XmlDocument localXML = Execute(BuildSOAPEnvelope("checkDBConn", "<nprojnr>705</nprojnr>", false));
                if ((tErrorNo == 0) || (tErrorNo == -667))
                {
                    return Serverstatus.Active;
                }
            }
            catch { }
            PingReply pingReply;
            using (var ping = new Ping())
            {
                pingReply = ping.Send(aURL);
            }
            if (pingReply.Status == IPStatus.Success)
            {
                return Serverstatus.Server_Failed;
            }
            return Serverstatus.Service_Failed;
        }

        public bool Login()
        {
            if (loginTimestamp != null)
            {
                if (loginTimestamp < DateTime.Now.AddHours(-1)) Logout();
            }
            if (tSessionID == 0)
            {
                ClearAll();
                XmlDocument localXML = Execute(BuildSOAPEnvelope("login", $"<i_szUsername>{uName}</i_szUsername><i_szPassword>{pWord}</i_szPassword>", false));
                if (tErrorNo == 0)
                {
                    tProjectNo = Convert.ToInt64(localXML.GetElementsByTagName("NPROJNR").Item(0).InnerText);
                    tPOSNo = Convert.ToInt64(localXML.GetElementsByTagName("NKASSANR").Item(0).InnerText);
                    tSessionID = Convert.ToInt64(localXML.GetElementsByTagName("NSESSIONID").Item(0).InnerText);
                    loginTimestamp = DateTime.Now;
                }
            }
            else
            {
                tErrorNo = 0;
                tErrorMsg = string.Empty;
            }
            return (tErrorNo == 0);
        }

        public bool NewLogin()
        {
            //load current data for DCI4CRM
            string TSQL = $"SELECT SessionID, last_login, status, last_status_check FROM sync.server WHERE name='DCI4CRM'";
            DataRow DR = cf.LoadDataRow(Asgard.AsgardCon, TSQL);
            if (DR != null)
            {
                tSessionID = Convert.ToInt64(DR["SessionID"].ToString());
                DateTime? LastLogin = DR.Field<DateTime?>("last_login");
                string CurStatus = DR["Status"].ToString();
                DateTime? LastStatusCheck = DR.Field<DateTime?>("last_status_check");
                if (!(CurStatus == "ACTIVE"))       //not active, so start from beginning
                {

                }
                else
                {

                }

            }


            return (tErrorNo == 0);
        }

        public bool Logout()
        {
            XmlDocument localXML = BuildSOAPEnvelope("logout","<i_szUsername>" + uName + "</i_szUsername><i_szPassword>" + pWord + "</i_szPassword>");
            localXML = Execute(localXML);
            ClearAll();
            //dw.SetControl("DCI4CRM_SID=null, DCI4CRM_LLT=null, DCI4CRM_Status='INACTIVE'");
            return (tErrorNo == 0);
        }

        public XmlDocument GetLogList(long LogNo, long Count)
        {
            //LogList.Initialize();
            XmlDocument localXML = BuildSOAPEnvelope("getLogList", "<i_nLogNr>" + LogNo + "</i_nLogNr><i_nMaxCount>" + Count + "</i_nMaxCount>");
            return (Execute(localXML));
        }


        public long SetEntitySnapshot(string TableName)
        {
            tActiveTable = "";
            //if (TableName.Contains("[1..]"))
            //{
            //    TableName = TableName.Remove(TableName.IndexOf("["));
            //}

            XmlDocument localXML = BuildSOAPEnvelope("SetEntitySnapshot","<i_szTableName>" + TableName + "</i_szTableName>");
            localXML = Execute(localXML);
            tActiveTable = TableName.Remove(TableName.IndexOf("["));
            tSnapshotCount = System.Convert.ToInt64(localXML.GetElementsByTagName("NCOUNT").Item(0).InnerText);
            tLogNoStart = System.Convert.ToInt64(localXML.GetElementsByTagName("NLOGNRBEFORE").Item(0).InnerText);
            tLogNoEnd = System.Convert.ToInt64(localXML.GetElementsByTagName("NLOGNRAFTER").Item(0).InnerText);
            tIncompleteSnapshot = (tErrorNo == 1501);
            return (tSnapshotCount);
        }

        public XmlDocument GetSnapshotList(long StartRow, long RowCount)
        {
            XmlDocument localXML = BuildSOAPEnvelope("GetSnapshotList","<i_nRownum>" + StartRow + "</i_nRownum><i_nMaxRows>" + (RowCount) + "</i_nMaxRows>");
            return (Execute(localXML));
        }

        public long SetEntityDML(XmlNode NewLine)
        {
            XmlDocument localXML = BuildSOAPEnvelope("SetEntityDML","<i_ctDCI4CRMLogLine>" + NewLine.InnerXml + "</i_ctDCI4CRMLogLine>");
            localXML = Execute(localXML);
            if (tErrorNo == 0)
            {
                return (System.Convert.ToInt64(localXML.GetElementsByTagName("NLOGNO").Item(0).InnerText));
            }
            return 0;
        }

        public XmlDocument GetSalesList(long CashierID, long Count, long FinalFilter = 0, long Filter = 0)
        {
            XmlDocument localXML = BuildSOAPEnvelope("GetSalesList","<i_nLfdZaehler>" + CashierID + "</i_nLfdZaehler><i_nMaxCount>" + Count + "</i_nMaxCount><i_nFertigeFilterNr>" + FinalFilter + "</i_nFertigeFilterNr><i_nFilterOptionNr>" + Filter + "</i_nFilterOptionNr>");
            return (Execute(localXML));
        }

        public string GetMediaID(string WebID)
        {
            XmlDocument localXML = BuildSOAPEnvelope("GetMediaID","<i_szWTPNr>" + WebID + "</i_szWTPNr>");
            localXML = Execute(localXML);
            if (tErrorNo == 0)
            {
                return localXML.GetElementsByTagName("SZCONVERSIONTEXT").Item(0).InnerText;
            }
            return "";
        }

        public string GetWebID(string MediaID)
        {
            XmlDocument localXML = BuildSOAPEnvelope("GetWTPNr","<i_szMediaNr>" + MediaID + "</i_szMediaNr>");
            localXML = Execute(localXML);
            if (tErrorNo == 0)
            {
                return localXML.GetElementsByTagName("SZCONVERSIONTEXT").Item(0).InnerText;
            }
            return "";
        }

        public XmlDocument GetUsageList(long ReaderTransNo, long Count, long FinalFilter, long Filter)
        {
            XmlDocument localXML = BuildSOAPEnvelope("GetUsageList","<i_LnfdLeserTransNr>" + ReaderTransNo + "</i_LnfdLeserTransNr><i_nMaxCount>" + Count + "</i_nMaxCount><i_nFertigeFilterNr>" + FinalFilter + "</i_nFertigeFilterNr><i_nFilterOptionNr>" + Filter + "</i_nFilterOptionNr>");
            return Execute(localXML);
        }

        public XmlDocument GetForeignPersonData(string FirstName, string LastName, DateTime DateOfBirth, bool CaseSensitive = false)
        {
            XmlDocument localXML = BuildSOAPEnvelope("GetForeignPersonData","<i_szPrenome>" + FirstName + "</i_szPrenome><i_szName>" + LastName + "</i_szName><i_dtBirthdate>" + DateOfBirth + "</i_dtBirthdate><i_nFilterOptionNr>" + (CaseSensitive ? "0" : "1") + "</i_nFilterOptionNr>");
            return Execute(localXML);
        }

        public XmlDocument GetForeignSalesList(long ProjectNo, long POS, long SerialNo, long ReaderTransNo)
        {
            XmlDocument localXML = BuildSOAPEnvelope("GetForeignSalesList","<i_nVerkProjNr>" + ProjectNo + "</i_nVerkProjNr><i_nKassaNr" + POS + "</inKassaNr><i_SerienNr>" + SerialNo + "</i_SerienNr><i_nLfdLeserTransNr>" + ReaderTransNo + "</i_nLfdLeserTransNr>");
            return Execute(localXML);
        }


        public XmlDocument GetChipOwner(string MediaID, long WebIDFormat = 0)  //defaults to unknown WebID type letting the system determine if possible
        {
            XmlDocument localXML = BuildSOAPEnvelope("GetChipOwner","<i_szMediaID>" + MediaID + "</i_szMediaID><i_nFilterOptionNr>" + WebIDFormat + "</i_nFilterOptionNr>");
            return Execute(localXML);
        }


        public XmlDocument GetBarcodeOwner(string Barcode, long Filter = 0)
        {
            XmlDocument localXML = BuildSOAPEnvelope("GetBarcodeOwner","<i_szBarcode>" + Barcode + "</i_szBarcode><i_nFilterOptionNr>" + Filter + "</i_nFilterOptionNr>");
            return Execute(localXML);
        }

        public XmlDocument GetPaymentData(long ProjectNumber, long POS, long TransactionNumber, long Cashier, long CashierAbrech, long Filter = -1)
        //Filter values
        // -1 = don't use shift
        //  0 = all records
        //  1 = only if shift has been closed before
        //  2 = only if shift has been proofed before
        {
            XmlDocument localXML = BuildSOAPEnvelope("GetPaymentData","<i_nProjNr>" + ProjectNumber + "</i_nProjNr><i_nKassaNr>" + POS + "</i_nKassaNr><i_nTransNr>" + TransactionNumber + "</i_nTransNr><i_nKassierID>" + Cashier + "</i_nKassierID><i_nKassierAbrechNr></i_nKassierAbrechNr><i_nFilterOptionNr>" + Filter + "</i_nFilterOptionNr>");
            return Execute(localXML);
        }

        public XmlDocument GetPaymentModifyList(long PaymentLogNo, long MaxCount = 50000, long FertigeFilterNo = 0, long FilterOptionNo = 0)
        {
            XmlDocument localXML = BuildSOAPEnvelope("GetPaymentModifyList", "<i_nPaymentModifyLogNr>" + PaymentLogNo.ToString() + "</i_nPaymentModifyLogNr><i_nMaxCount>" + MaxCount.ToString() + "</i_nMaxCount><i_nFertigeFilterNr>" + FertigeFilterNo.ToString() + "</i_nFertigeFilterNr><i_nFilterOptionNr>" + FilterOptionNo.ToString() + "</i_nFilterOptionNr>");
            return Execute(localXML);
        }

        public XmlDocument GetSalesModifyList(long SalesLogNo, long MaxCount = 50000, long FertigeFilterNo = 0, long FilterOptionNo = 0)
        {
            XmlDocument localXML = BuildSOAPEnvelope("GetSalesModifyList", "<i_nPaymentModifyLogNr>" + SalesLogNo.ToString() + "</i_nPaymentModifyLogNr><i_nMaxCount>" + MaxCount.ToString() + "</i_nMaxCount><i_nFertigeFilterNr>" + FertigeFilterNo.ToString() + "</i_nFertigeFilterNr><i_nFilterOptionNr>" + FilterOptionNo.ToString() + "</i_nFilterOptionNr>");
            return Execute(localXML);
        }

        
        
        // PUBLIC UTILITIES



        //Web Functions

        private HttpWebRequest CreateWebRequest(string url, string soapAction)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.ContentType = "text/xml; charset=UTF-8; action =\"" + soapAction + "\"";
            webRequest.Method = "POST";
            return webRequest;
        }

        private XmlDocument BuildSOAPEnvelope(string Function, string Parameters, bool useSessionID = true)
        {
            XmlDocument soapXMLDocument = new XmlDocument();
            string XMLData = "<" + Function + ">";
            if (useSessionID)
            {
                XMLData += "<i_nSessionID>" + tSessionID + "</i_nSessionID>";
            }
            XMLData += Parameters + "</" + Function + ">";
            try
            {
                soapXMLDocument.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?><soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><soap:Body>" + XMLData + "</soap:Body></soap:Envelope>");
            }
            catch (MySqlException ex)
            {
                long ec = ex.ErrorCode;
            }
            return soapXMLDocument;
        }

        public XmlDocument Execute(XmlDocument requestDoc)
        {
            string errAppName = "Execute";
            DateTime curTime = DateTime.Now;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Properties.Settings.Default.Axess_DCI4CRM);
            request.ContentType = "text/xml; charset=UTF-8; action =\"SOAP:Action\"";
            request.Method = "POST";
            try
            {
                using (Stream stream = request.GetRequestStream())
                {
                    requestDoc.Save(stream);
                }
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        string soapResult = rd.ReadToEnd();
                        XmlDocument localXML = new XmlDocument();
                        localXML.LoadXml(soapResult);
                        if (localXML.GetElementsByTagName("NERRORNO").Item(0) == null)
                        {
                            if (localXML.GetElementsByTagName("checkDBConnReturn") != null)
                            {
                                if (localXML.GetElementsByTagName("checkDBConnReturn").Item(0).InnerText.Contains("Error:"))
                                {
                                    tErrorNo = -667;
                                    tErrorMsg = localXML.GetElementsByTagName("checkDBConnReturn").Item(0).InnerText.Replace("Error:", "").Trim();
                                }
                                else
                                {
                                    tErrorNo = 0;
                                    tErrorMsg = string.Empty;                                        }
                            }
                        }
                        else
                        {
                            if (localXML.GetElementsByTagName("NERRORNO").Item(0).InnerText.Length != 0)
                            {
                                tErrorNo = System.Convert.ToInt16(localXML.GetElementsByTagName("NERRORNO").Item(0).InnerText);
                                tErrorMsg = localXML.GetElementsByTagName("SZERRORMESSAGE").Item(0).InnerText;
                            }
                            else
                            {
                                tErrorNo = -666;
                                tErrorMsg = "Axess API returned an invalid response.";
                            }
                        }
                        //if (tErrorNo != 0)
                        //    cf.updateErrorLog(errAppName, tErrorMsg, new MySqlConnection(Properties.Settings.Default.MySQLWS));
                        return localXML;
                    }
                }
            }
            catch (WebException ex)
            {
                tErrorMsg = ex.Message;
                tErrorNo = -666;
                TError.UpdateErrorLog(errAppName, tErrorNo, tErrorMsg);
            }
            return new XmlDocument();
        }

        public string Hex2cardacct(string tcHex)
        {
            string mx = "0000000000" + Hex2decimal(tcHex);
            mx = mx.Substring(mx.Length - 10, 10);
            return mx;
        }

        //*convert webid to decimal
        public string Hex2decimal(string tcHex)
        {
            long.TryParse(tcHex, System.Globalization.NumberStyles.HexNumber, null, out long result);
            return result.ToString();
        }

        public string MediaID_to_ChipID(string aMediaID)
        {
            if (aMediaID.Length != 16) return string.Empty;
            return aMediaID.Substring(14, 2) + aMediaID.Substring(12, 2) + aMediaID.Substring(10, 2) + aMediaID.Substring(8, 2) + aMediaID.Substring(6, 2) + aMediaID.Substring(4, 2) + aMediaID.Substring(2, 2) + aMediaID.Substring(0, 2);
        }



        public string MediaID_2_WTP32(string MediaID)
        {
            if (MediaID != "NULL" && MediaID != "0")
            {
                MediaID = MediaID.Substring(MediaID.Length - 8);
                return (MediaID.Substring(6) + MediaID.Substring(4, 2) + MediaID.Substring(2, 2) + MediaID.Substring(0, 2));
            }
            return "NULL";
        }
    }
}
