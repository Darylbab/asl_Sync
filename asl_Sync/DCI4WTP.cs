using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using MySql.Data.MySqlClient;

namespace asl_SyncLibrary
{
    public class DCI4WTP
    {
        //CONSTANTS
        private const string AXURL = "http://wtpsi.teamaxess.com:16351/DCI4WTP/DCI4WTPService.svc";
        private const string uName = "Alta";
        private const string pWord = "Alta";
        private const string uSoapName = "Alta";
        private const string pSoapWord = "Alta";
        private const string uLoginID = "Alta DCI4WTP Test";
        private const long lTestProjectNo = 997;
        private const long lProjectNo = 705;
        private const string curCountryCode = "USA";
        
        //LOCAL VARIABLES
        private int tErrorNo = 0;
        private string tErrorMsg = "";
        private long tProjectNo = 0;
        private long tPOSNo = 0;
        //private string tActiveTable = "";
        //private long tLogNoStart = 0;
        //private long tLogNoEnd = 0;
        //private long tSnapshotCount = 0;
        //private bool tIncompleteSnapshot = false;
        private DateTime? loginTimestamp = null;
        private long curProjectNo = 705;
        private long curSessionID = 0;

        private CommonFunctions cf = new CommonFunctions();
        private DataWarehouse dw = new DataWarehouse();
        private ASLError myError = new ASLError();

        //Complex Data Types


        //PROPERTIES
        public long SessionID
        {
            get { return curSessionID; }
            set { curSessionID = value; }
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

        private void ClearAll()
        {
            curSessionID = 0;
            tErrorNo = 0;
            tErrorMsg = "";
            tProjectNo = 0;
            tPOSNo = 0;
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
                curProjectNo = tTestMode ? lTestProjectNo : lProjectNo;
            }
        }

        public Serverstatus GetServerStatus(string aURL)
        {
            if (cf.IsURLOnline(Properties.Settings.Default.Axess_DCI4WTP))
            //try
            //{
            //    XmlDocument localXML = Execute(BuildSOAPEnvelope("checkDBConn", "<nprojnr>705</nprojnr>", false));
            //    if ((tErrorNo == 0) || (tErrorNo == -667))
                {
                    return Serverstatus.Active;
                }
            //}
            //catch { }
            PingReply pingReply;
            using (var ping = new Ping())
            {
                pingReply = ping.Send(aURL);
            }
            if (pingReply.Status != IPStatus.Success)
            {
                return Serverstatus.Server_Failed;
            }
            return Serverstatus.Service_Failed;
        }

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
                XMLData += "<i_nSessionID>" + curSessionID + "</i_nSessionID>";
            }
            XMLData += Parameters + "</" + Function + ">";
            try
            {
                string t = string.Empty;
                if (XMLData.Contains("team:"))
                {
                    t = @" xmlns:team=""http://teamaxess.com/""";
                }
                if (Parameters.Contains("axw:"))
                {
                    t += @" xmlns:axw=""http://schemas.datacontract.org/2004/07/AxWebServices""";
                }
                soapXMLDocument.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?><soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""" + @t + "><soapenv:Header/><soapenv:Body>" + XMLData + "</soapenv:Body></soapenv:Envelope>");
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
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Properties.Settings.Default.Axess_DCI4WTP);
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
                                    tErrorMsg = string.Empty;
                                }
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
                myError.UpdateErrorLog(errAppName, tErrorNo, tErrorMsg);
            }
            return new XmlDocument();
        }

        public bool Login()
        {
            if (loginTimestamp != null)
            {
                if (loginTimestamp < DateTime.Now.AddHours(-1)) Logout();
            }
            if (SessionID == 0)
            {
                ClearAll();
                    string t = "<team:i_ctLoginReq>" + 
                    $"<axw:SZCOUNTRYCODE>{curCountryCode}</axw:SZCOUNTRYCODE>" +
                    $"<axw:SZLOGINID>Alta DCI4WTP</axw:SZLOGINID>" + 
                    $"<axw:SZLOGINMODE>CUSTOMER</axw:SZLOGINMODE>" + 
                    $"<axw:SZPASSWORD>Alta</axw:SZPASSWORD>" +
                    $"<axw:SZSOAPPASSWORD>Alta</axw:SZSOAPPASSWORD>" +
                    $"<axw:SZSOAPUSERNAME>Alta</axw:SZSOAPUSERNAME>" +
                    $"<axw:SZUSERNAME>Alta</axw:SZUSERNAME></team:i_ctLoginReq>";

                XmlDocument localXML = Execute(BuildSOAPEnvelope("team:login", t, false));
                if (tErrorNo == 0)
                {
                   tProjectNo = curProjectNo;
                    tPOSNo = Convert.ToInt64(localXML.GetElementsByTagName("NPOSNO").Item(0).InnerText);
                    curSessionID = Convert.ToInt64(localXML.GetElementsByTagName("NSESSIONID").Item(0).InnerText);
                    loginTimestamp = DateTime.Now;
                    dw.SetControl("DCI4WTP_SID=" + curSessionID + ", DCI4WTP_LLT='" + loginTimestamp.Value.ToString(Mirror.AxessDateTimeFormat) + "', DCI4WTP_Status='ACTIVE'");
                }
            }
            else
            {
                tErrorNo = 0;
                tErrorMsg = string.Empty;
            }
            return (tErrorNo == 0);
        }

        public bool Logout()
        {
            XmlDocument localXML = BuildSOAPEnvelope("logout", "<i_szUsername>" + uName + "</i_szUsername><i_szPassword>" + pWord + "</i_szPassword>");
            localXML = Execute(localXML);
            ClearAll();
            dw.SetControl("DCI4CRM_SID=null, DCI4CRM_LLT=null, DCI4CRM_Status='INACTIVE'");
            return (tErrorNo == 0);
        }

        public XmlDocument GetTariff(int nWTPProfileNo, int nPoolNo, int nTicketTypeNo, int nPersonTypNo, string szValidFrom, int nArticleNo)
        {
            string t = $"<i_ctTariffReq xsi:type='urn:D4WTPTARIFFREQUEST' xmlns:urn='urn:DCI4WTP'><CTPRODUCT xsi:type='urn:D4WTPPRODUCT'>" +
                $"<NARTICLENO xsi:type='xsd:decimal'>{nArticleNo}</NARTICLENO>" +
                $"<NPERSONTYPNO xsi:type='xsd:decimal'{nPersonTypNo}</NPERSONTYPNO>" +
                $"<NPOOLNO xsi:type='xsd:decimal'>{nPoolNo}</NPOOLNO>" +
                $"<NPROJNO xsi:type='xsd:decimal'>{curProjectNo}</NPROJNO>" +
                $"<NTICKETTYPENO xsi:type='xsd:decimal'>{nTicketTypeNo}</NTICKETTYPENO>" +
                $"<SZVALIDFROM xsi:type='xsd:string'>{szValidFrom}</SZVALIDFROM>" +
                $"</CTPRODUCT><NSESSIONID xsi:type='xsd:decimal'>{curSessionID}</NSESSIONID>" +
                $"<NWTPPROFILENO xsi:type='xsd:decimal'>{nWTPProfileNo}</NWTPPROFILENO></i_ctTariffReq>";

            return (Execute(BuildSOAPEnvelope("axis:gettariff", t, false)));
        }

        public XmlDocument GetPools(int nWTPProfileNo)
        {
            return Execute(BuildSOAPEnvelope("axis:getpools", $"<i_ctPoolReq xsi:type='urn:D4WTPPOOLREQUEST' xmlns:urn='urn:DCI4WTP'><NSESSIONID xsi:type='xsd:decimal'>{curSessionID}</NSESSIONID><NWTPPROFILENO xsi:type='xsd:decimal'>{nWTPProfileNo}</NWTPPROFILENO><SZCOUNTRYCODE xsi:type='xsd:string'>{curCountryCode}</SZCOUNTRYCODE></i_ctPoolReq>", false));

        }

        public XmlDocument GetWTPConfiguration(int? nParamID)
        {
            return Execute(BuildSOAPEnvelope("axis:getWTPConfiguration", $"<i_ctConfigReq xsi:type='urn: D4WTPCONFIGREQUEST' xmlns:urn='urn: DCI4WTP'><NPARAMID xsi:type='xsd:decimal'>{nParamID}</NPARAMID><NSESSIONID xsi:type='xsd:decimal'>{curSessionID}</NSESSIONID></i_ctConfigReq>", false));
        }

        public XmlDocument GetCustomerProfile(int nCustomerPOSNo, int nCustomerNo, int nEmployeeNo)
        {
            string t = $"<i_ctCProfileReq xsi:type='urn:D4WTPCUSTOMERPROFILEREQUEST' xmlns:urn='urn:DCI4WTP'>" +
                $"<NCUSTOMERNO xsi:type='xsd:decimal'>{nCustomerNo}</NCUSTOMERNO>" +
                $"<NCUSTOMERPOSNO xsi:type='xsd:decimal'>{nCustomerPOSNo}</NCUSTOMERPOSNO>" +
                $"<NCUSTOMERPROJNO xsi:type='xsd:decimal'>{curProjectNo}</NCUSTOMERPROJNO>" +
                $"<NEMPLOYEENO xsi:type='xsd:decimal'>{nEmployeeNo}</NEMPLOYEENO>" +
                $"<NSESSIONID xsi:type='xsd:decimal'>{curSessionID}</NSESSIONID></i_ctCProfileReq>";
            return Execute(BuildSOAPEnvelope("axis:getCustomerProfile", t, false));
        }

        public XmlDocument GetServerVersion()
        {
            return Execute(BuildSOAPEnvelope("team:getServerVersion", "", false));
        }




    }
}
