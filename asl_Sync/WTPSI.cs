using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using MySql.Data.MySqlClient;

namespace asl_SyncLibrary
{
    public class WTPSI
    {
        private CommonFunctions cf = new CommonFunctions();
        private ASLError myError = new ASLError();
        //private DataWarehouse dw = new DataWarehouse();
        
        private long tSessionID = 0;
        private long tErrorNo = 0;
        private string tErrorMsg = string.Empty;
        private decimal tCustomerID = 0;
        private long tPersProjNr = 705;
        private long tPersNr = 0;
        private long tPersKassaNr = 0;
        private string tLastName = string.Empty;
        private string tFirstName = string.Empty;
        private string tGender = string.Empty;
        private string tTitle = string.Empty;
        private string tSalutation = string.Empty;
        private DateTime tDateOfBirth = Convert.ToDateTime(Properties.Settings.Default.DefaultBirthdate);
        private string tEmail = string.Empty;
        private string tStreet = string.Empty;
        private string tCO = string.Empty;
        private string tZipCode = string.Empty;
        private string tCity = string.Empty;
        private string tCountry = string.Empty;
        private string tInfo = string.Empty;
        private string tIUD = string.Empty;
        private DateTime? loginTimestamp = null;


        public long SessionID
        {
            get { return tSessionID; }
        }

        public long ErrorNo
        {
            get { return tErrorNo; }
        }

        public string ErrorMsg
        {
            get { return tErrorMsg; }
        }

        public decimal CustomerID
        {
            get { return tCustomerID; }
        }

        public long PersProjNr
        {
            get { return tPersProjNr; }
        }

        public long PersKassaNr
        {
            get { return tPersKassaNr; }
        }

        public long PersNr
        {
            get { return tPersNr; }
        }

        public string LastName
        {
            get { return tLastName; }
            set { tLastName = value; }
        }

        public string FirstName
        {
            get { return tFirstName; }
            set { tFirstName = value; }
        }

        public string Gender
        {
            get { return tGender; }
            set { tGender = value; }
        }

        public string Title
        {
            get { return tTitle; }
            set { tTitle = value; }
        }

        public string Salutation
        {
            get { return tSalutation; }
            set { tSalutation = value; }
        }

        public DateTime DateOfBirth
        {
            get { return tDateOfBirth; }
            set { tDateOfBirth = value; }
        }

        public string Email
        {
            get { return tEmail; }
            set { tEmail = value; }
        }

        public string Street
        {
            get { return tStreet; }
            set { tStreet = value; }
        }

        public string CO
        {
            get { return tCO; }
            set { tCO = value; }
        }

        public string ZipCode
        {
            get { return tZipCode; }
            set { tZipCode = value; }
        }

        public string City
        {
            get { return tCity; }
            set { tCity = value; }
        }

        public string Country
        {
            get { return tCountry; }
            set { tCountry = value; }
        }

        public string Info
        {
            get { return tInfo; }
            set { tInfo = value; }
        }

        public string IUD
        {
            get { return tIUD; }
            set { switch (value.ToUpper())
                    {
                    case "I":
                    case "U":
                    case "D":
                        tIUD = value.ToUpper();
                        break;
                    default:
                        tIUD = string.Empty;
                        break;
                    }
                }
        }

        private void ClearAll()
        {
            tSessionID = 0;
            tErrorNo = 0;
            tErrorMsg = "";
            ClearCustomerData();
            loginTimestamp = null;
        }

        public void ClearCustomerData()
        {
            tIUD = string.Empty;
            tCity = string.Empty;
            tCO = string.Empty;
            tCountry = string.Empty;
            tCustomerID = 0;
            tDateOfBirth = cf.nullDate;
            tEmail = string.Empty;
            tFirstName = string.Empty;
            tGender = string.Empty;
            tInfo = string.Empty;
            tLastName = string.Empty;
            tPersKassaNr = 0;
            tPersNr = 0;
            tPersProjNr = 705;
            tSalutation = string.Empty;
            tStreet = string.Empty;
            tTitle = string.Empty;
            tZipCode = string.Empty;
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
            if (SessionID == 0)
            {
                ClearAll();
                XmlDocument localXML = Execute(BuildSOAPEnvelope("login", "<user_name>" + Properties.Settings.Default.Axess_WTPSI_User + "</user_name><password>" + Properties.Settings.Default.Axess_WTPSI_Password + "</password>", false));
                if (tErrorNo == 0)
                {
                    tSessionID = Convert.ToInt64(localXML.GetElementsByTagName("NSESSIONID").Item(0).InnerText);
                    loginTimestamp = DateTime.Now;
                    //dw.SetControl("WTPSI_SID=" + tSessionID + ", WTPSI_LLT='" + loginTimestamp.Value.ToString(Mirror.AxessDateTimeFormat) + "', WTPSI_Status='ACTIVE'");
                }
            }
            else
            {
                tErrorNo = 0;
            }
            return (tErrorNo == 0);
        }

        public bool Logout()
        {
            XmlDocument localXML = BuildSOAPEnvelope("logout", "<user_name>" + Properties.Settings.Default.Axess_WTPSI_User + "</user_name><password>" + Properties.Settings.Default.Axess_WTPSI_Password + "</password>");
            localXML = Execute(localXML);
            ClearAll();
            //dw.SetControl("WTPSI_SID=null, WTPSI_LLT=null, WTPSI_Status='INACTIVE'");
            return (tErrorNo == 0);
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
            XMLData += Parameters;
            if (useSessionID)
            {
                XMLData += "<session_id>" + tSessionID + "</session_id>";
            }
            XMLData += "</" + Function + ">";
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

        public XmlDocument Execute(XmlDocument requestDoc, string aWebService = "")
        {
            if (aWebService == string.Empty)
                aWebService = Properties.Settings.Default.Axess_WTPSI;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(aWebService);
            request.ContentType = "text/xml; charset=UTF-8; action =\"SOAP:Action\"";
            request.Method = "POST";
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
                    return localXML;
                }
            }
        }

        // WTPSI functions

        public bool BlockTicketByJournal(decimal aPOSNr, decimal aJournalNr, DateTime? aEndOfBlockingDate = null)
        {
            if (!Login()) return false;

            XmlDocument tXML = Execute(BuildSOAPEnvelope("blockTicketByJournal", "<i_nProjNr>705</i_nPorjNr><i_nPOSNr>" + aPOSNr.ToString() + "</i_nPOSNr><i_nJournalNr>" + aJournalNr.ToString() + "</i_nJournalNr><i_szEndOfBlockingDate>" + aEndOfBlockingDate.ToString() + "</i_szEndOfBlockingDate>")); 
            return (tErrorNo == 0);
        }

        public bool BlockTicketBySerial(decimal aPOSNr, decimal aSerialNr, decimal aUnicodeNr, DateTime? aValidDate = null, DateTime? aEndOfBlockingDate = null)
        {
            if (!Login()) return false;
            XmlDocument tXML = Execute(BuildSOAPEnvelope("blockTicketBySerial", "<i_nProjNr>705</i_nPorjNr><i_nPOSNr>" + aPOSNr.ToString() + "</i_nPOSNr><i_nSerialNr>" + aSerialNr.ToString() + "</i_nSerialNr><i_nUnicodeNr>" + aUnicodeNr.ToString() + "</i_nUnicodeNr><i_szValidDate>" + aValidDate.ToString() + "</i_szValidDate><i_szEndOfBlockingDate>" + aEndOfBlockingDate.ToString() + "</i_szEndOfBlockingDate>"));
            return (tErrorNo == 0);
        }

        public bool CancelOrderPosition(decimal aOrderNr, decimal aOrderPOSNr, decimal aOrderSubPOSNr)
        {
            if (!Login()) return false;
            XmlDocument tXML = Execute(BuildSOAPEnvelope("cancelOrderPosition", "<i_nProjNr>705</i_nPorjNr><i_nOrderNr>" + aOrderNr.ToString() + "</i_nOrderNr><i_nOrderPosNr>" + aOrderPOSNr.ToString() + "</i_nOrderPosNr><i_nOrderSubPosNr>" + aOrderSubPOSNr.ToString() + "</i_nOrderSubPosNr>"));
            //
            switch (tErrorNo)
            {
                case 0:
                    //no error so simply continue
                    break;
                case -1002:
                case -1034:
                case -2001:
                case -2002:
                case -2004:
                case -6506:
                case -6513:
                case -6514:
                case -6515:
                case -6516:
                case -6531:
                case -6699:
                    break;
                default:
                    //everything else
                    break;
            }
            return (tErrorNo == 0);
        }

        // eBlocking -- -1=Cancel sale and block ticket
        //               0=Cancel sale without blocking
        
        public bool CancelTicket(decimal aPOSNr, decimal aJournalNr, decimal aSerialNr, decimal aUnicodeNr, DateTime? aValidTill, decimal aBlocking)
        {
            if (!Login()) return false;
            if (!((aSerialNr > 0) && (aUnicodeNr > 0 || aValidTill > new DateTime(1900, 1, 1)) || (aJournalNr > 0)))
            {
                tErrorNo = -20000;
                tErrorMsg = "cancelTicket requires either a JournalNr or both a SerialNr and a UnicodeNr or both a SerialNr and ValidTill date.";
            }
            if (aBlocking < -1 || aBlocking > 0) 
            {
                tErrorNo = -20001;
                tErrorMsg = "aBlocking must be set to either -1 (block) or 0 (don't block).";
            }
            if (tErrorNo == 0)
            {
                XmlDocument tXML = Execute(BuildSOAPEnvelope("cancelTicket", "<i_nProjNr>705</i_nPorjNr><i_nJournalNr>" + aJournalNr.ToString() + "</i_nJournalNr><i_nSerialNr>" + aSerialNr.ToString() + "</i_nSerialNr><i_nUnicodeNr>" + aUnicodeNr.ToString() + "</i_nUnicodeNr><i_szValidTill>" + aValidTill + "</i_szValidTill><nBlocking>" + aBlocking.ToString() + "</nBlocking>"));
            }
            switch (tErrorNo)
            {
                case 0:
                    //no error so simply continue
                    break;
                case -1034:
                case -2002:
                case -2004:
                case -6517:
                case -6518:
                case -6519:
                case -6520:
                    break;
                default:
                    //everything else
                    break;
            }
            return (tErrorNo == 0);
        }

        public bool CheckWTPCRC(string aChipID, string aCRC)
        {
            if (!Login()) return false;
            if (aChipID.Length == 0)
            {
                tErrorNo = -20002;
                tErrorMsg = "ChipID cannot be blank.";
            }
            if (aCRC.Length == 0)
            {
                tErrorNo = -20003;
                tErrorMsg = "CRC cannot be blank.";
            }
            if (tErrorNo == 0)
            {
                XmlDocument tXML = Execute(BuildSOAPEnvelope("checkWTPCRC", "<i_szChipID>" + aChipID + "</i_szChipID><i_szCRC>" + aCRC + "</i_szCRC>"));
            }
            switch (tErrorNo)
            {
                case 0:
                    //no error so simply continue
                    break;
                case -2001:
                case -9999:
                case -3103:
                    break;
                default:
                    //everything else
                    break;
            }
            return (tErrorNo == 0);
        }

        public bool CheckWTPAcceptOneProj()
        {
            if (!Login()) return false;

            switch (tErrorNo)
            {
                case 0:
                    //no error so simply continue
                    break;
                case -2001:
                case -9999:
                case -3104:
                    break;
                default:
                    //everything else
                    break;
            }
            return (tErrorNo == 0);
        }

        public bool CheckWTPAcceptAllProj()
        {
            if (!Login()) return false;
            return false;
        }

        public bool CheckWTP64Bit()
        {
            if (!Login()) return false;
            return false;
        }

        public bool CreateUserInBO(string aLastName, string aFirstName, DateTime aDateOfBirth, string aCareOf = "", string aStreet = "", string aStreet2 = "", string aStreet3 = "", string aCity = "", string aCountry = "USA", string aZipCode = "", string aEmail = "", string aGender = "M", string aTitle = "", string aPersMobilNr = "", string aProvinceShortname = "", string aAddrFaxNr = "", string aAddrTelNr = "", string aSalutation = "", string aInfo = "")
        {
            if (!Login()) return false;
            //bool customerFound = (lookForBOCPerson(aFirstName, aLastName, aDateOfBirth));
            //if (customerFound)
            //{
            //    return true;
            //}
            tCustomerID = GetFreeCustomerID();
            EditCustomer(tCustomerID, aFirstName, aLastName, aDateOfBirth, aCareOf, aStreet, aStreet2, aStreet3, aCity, aCountry, aZipCode, aEmail, aGender, aTitle, aPersMobilNr, aProvinceShortname, aAddrFaxNr, aAddrTelNr, aSalutation, aInfo,"I");
            if (tErrorNo == 0) LookForBOCPerson(aLastName, aFirstName, aDateOfBirth);
            return (tErrorNo == 0);
        }

        public bool CreateWTPNo()
        {
            if (!Login()) return false;

            return false;
        }

        public bool CreateWTPSICustomer(decimal aCustomerID, decimal aPersPOSNr = 20, decimal aPersNr = 0)
        {
            if (!Login()) return false;
            if (aPersNr == 0 )
            {
                Execute(BuildSOAPEnvelope("createWTPSICustomer", "<i_nPersProjNr>705</i_nPersProjNr><i_nPersPOSNr>" + aPersPOSNr.ToString() + "</i_nPersPOSNr><i_nPersNr>" + aPersNr.ToString() + "</i_nPersNr><i_nCustomerID>" + aCustomerID.ToString() + "</i_nCustomerID>"));
                return (tErrorNo == 0);
            }
            return false;
        }

        public bool EditCompany()  //editCompanyII
        {
            if (!Login()) return false;
            return false;
        }

        public bool EditCustomer(decimal aCustomerID, string aFirstName, string aLastName, DateTime aDateOfBirth, string aCareOf, string aStreet = "", string aStreet2 = "", string aStreet3 = "", string aCity = "", string aCountry = "USA", string aZipCode = "", string aEmail = "", string aGender = "M", string aTitle = "", string aPersMobilNr = "", string aProvinceShortname = "", string aAddrFaxNr = "", string aAddrTelNr = "", string aSalutation = "", string aInfo = "", string aFunction = "")  //uses editCustomerIV
        {
            if (!Login()) return false;
            decimal? tCustID = aCustomerID;
            if (aCustomerID == 0)
            {
                tCustID = null;
            }
            if (aCountry.ToUpper() == "UNITED STATES") aCountry = "USA";
            string tParams = "<customer><NCUSTOMERID>" + aCustomerID + "</NCUSTOMERID>";
            tParams += "<SZADDRFAXNR>" + aAddrFaxNr + "</SZADDRFAXNR>";
            tParams += "<SZADDRTELNR>" + aAddrTelNr + "</SZADDRTELNR>";
            tParams += "<SZCITY>" + aCity.ToUpper() + "</SZCITY>";
            tParams += "<SZCO>" + aCareOf.ToUpper() + "</SZCO>";
            tParams += "<SZCOUNTRY>" + aCountry.ToUpper() + "</SZCOUNTRY>";
            tParams += "<SZDATEOFBIRTH>" + aDateOfBirth.ToString(Mirror.AxessDateFormat) + "</SZDATEOFBIRTH>";
            tParams += "<SZEMAIL >" + aEmail.ToUpper() + "</SZEMAIL>";
            tParams += "<SZFIRSTNAME>" + aFirstName.ToUpper() + "</SZFIRSTNAME>";
            tParams += "<SZGENDER>" + aGender.ToUpper() +"</SZGENDER>";
            tParams += "<SZINFO>" + aInfo.ToUpper() + "</SZINFO>";
            tParams += "<SZIUD>" + aFunction+ "</SZIUD>";
            tParams += "<SZLASTNAME>" + aLastName.ToUpper() + "</SZLASTNAME>";
            tParams += "<SZPERSMOBILNR>" + aPersMobilNr + "</SZPERSMOBILNR>";
            tParams += "<SZPROVINCESHORTNAME>" + aProvinceShortname + "</SZPROVINCESHORTNAME>";
            tParams += "<SZSALUTATION>" + aSalutation.ToUpper() + "</SZSALUTATION >";
            tParams += "<SZSTREET>" + aStreet.ToUpper() + "</SZSTREET>";
            tParams += "<SZSTREET2>" + aStreet2.ToUpper() + "</SZSTREET2>";
            tParams += "<SZSTREET3>" + aStreet3.ToUpper() + "</SZSTREET3>";
            tParams += "<SZTITLE>" + aTitle.ToUpper() + "</SZTITLE>";
            tParams += "<SZZIPCODE>" + aZipCode + "</SZZIPCODE></customer>";
            XmlDocument tXML = Execute(BuildSOAPEnvelope("editCustomerIV",tParams));
            return (tErrorNo == 0);
        }

        public string GetUserKey()
        {
            if (!Login()) return string.Empty;
            return "";
        }

        public bool GetCompanyData()
        {
            if (!Login()) return false;
            return false;
        }

        public bool CompanyLogin()
        {
            if (!Login()) return false;
            return false;
        }

        public bool ConvertBCToWTPNo()
        {
            if (!Login()) return false;
            return false;
        }

        public bool EditOrderHeader() //editOrderHeaderII
        {
            if (!Login()) return false;
            return false;
        }

        public bool EditOrderPositions()
        {
            if (!Login()) return false;
            return false;
        }

        public bool EditOrderPosWithArticles()
        {
            if (!Login()) return false;
            return false;
        }

        public bool Get32BitWTPNo()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetAdditionalArticles() //getAddArticles2
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetArticles()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetArticleTariff()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetEDEPersonType()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetEDEPool()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetEDETicketType()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetExternalReference()
        {
            if (!Login()) return false;
            return false;
        }


        private decimal GetFreeCustomerID(bool aClearCustomerData = false)
        {
            if (!Login()) return -1;
            XmlDocument tXML = Execute(BuildSOAPEnvelope("getFreeCustomerID", ""));
            if (tErrorNo != 0) return 0;

            try
            {
                tCustomerID = Convert.ToDecimal(tXML.GetElementsByTagName("NRESULTNUMBER").Item(0).InnerText);
            }
            catch
            {
                tCustomerID = 0;
            }
            return tCustomerID;
        }

        public bool GetOrderInfo(long nOrderNO)
        {
            if (!Login()) return false;
            XmlDocument tXML = Execute(BuildSOAPEnvelope("getOrderInfo", "<nOrderNO>" + nOrderNO + "</nOrderNO>"));

            return false;
        }

        public bool GetOrderTariff()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetPackages()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetPackageContent()
        {
            if (!Login())
            {
                return false;
            }

            return false;   //true when code in place
        }

        public bool GetPackageTariffs()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetPersonTypes()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetPhoto()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetPools()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetRandonWTPNO()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetTariff()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetTicketTypes()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetTicketBlockStatus()
        {
            if (!Login()) return false;
            return false;
        }
        public bool GetUserCompanies()
        {
            if (!Login()) return false;
            XmlDocument tXML = Execute(BuildSOAPEnvelope("getXMLUserCompanies", ""));
            Logout();
            return true;
        }

        public bool GetWhoAmIData()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetWTPInformation()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetWTPPersonalisation()
        {
            if (!Login()) return false;
            return false;
        }

        public bool GetXMLUserCompanies()
        {
            if (!Login()) return false;
            return false;
        }

        //lookForBOCPerson
        //  looks in the back office for a person based on lastname, firstname and DOB.  Can be run with only Lastname and DOB.
        //  sets local variables to match return if found.  If multiples returns first found
        public bool LookForBOCPerson(string aLastName, string aFirstName , DateTime aDateOfBirth)
        {
            if (!Login()) return false;
            XmlDocument tXML = Execute(BuildSOAPEnvelope("lookForBOCPerson", "<i_szFirstname>" + aFirstName + "</i_szFirstname><i_szName>" + aLastName + "</i_szName><i_szBirthdate>" + aDateOfBirth.ToString(Mirror.AxessDateFormat) + "</i_szBirthdate>"));
            if (tErrorNo == 0)  
            { 
                foreach (XmlNode tNode in tXML.GetElementsByTagName("ACTBOCPERSON").Item(0).ChildNodes)
                {
                    if (cf.IsAltaPOS(Convert.ToInt64(tNode.SelectSingleNode("NPERSPOSNR").InnerText)) && (tNode.SelectSingleNode("NCUSTOMERID").InnerText.Trim() != string.Empty))
                    {
                        tCustomerID = Convert.ToInt32(tNode.SelectSingleNode("NCUSTOMERID").InnerText);
                        tPersNr = Convert.ToInt64(tNode.SelectSingleNode("NPERSNR").InnerText);
                        tPersProjNr = Convert.ToInt64(tNode.SelectSingleNode("NPERSPROJNR").InnerText);
                        tPersKassaNr = Convert.ToInt64(tNode.SelectSingleNode("NPERSPOSNR").InnerText);
                        tCity = tNode.SelectSingleNode("SZCITY").InnerText;
                        tDateOfBirth = Convert.ToDateTime(tNode.SelectSingleNode("SZDATEOFBIRTH").InnerText);
                        tFirstName = tNode.SelectSingleNode("SZFIRSTNAME").InnerText;
                        tGender = tNode.SelectSingleNode("SZGENDER").InnerText;
                        tLastName = tNode.SelectSingleNode("SZNAME").InnerText;
                        tSalutation = tNode.SelectSingleNode("SZSALUTATION").InnerText;
                        tStreet = tNode.SelectSingleNode("SZSTREET").InnerText;
                        tTitle = tNode.SelectSingleNode("SZTITLE").InnerText;
                        tZipCode = tNode.SelectSingleNode("SZZIPCODE").InnerText;
                        return true;
                    }
                }
                tErrorNo = -6105;
            }
            if (!(tErrorNo == 0) || (tErrorNo == -6105))
            {

                //myError.ErrorHandler("WTPSI.lookForBOCPerson", tErrorNo, tErrorMsg);
            }
            return false;
        }

        public bool PersonalizeChipID()
        {
            if (!Login()) return false;
            return false;
        }

        public bool PurchaseCustomProduct()
        {
            if (!Login()) return false;
            return false;
        }

        public bool PurchaseCustomProductPackage()
        {
            if (!Login()) return false;
            return false;
        }

        public bool PurchaseProduct()
        {
            if (!Login()) return false;
            return false;
        }

        public bool PurchaseTicketAdvanced()
        {
            if (!Login()) return false;
            return false;
        }

        public bool ReleaseGlobalOrder()
        {
            if (!Login()) return false;
            return false;
        }

        public bool ReleaseOrder()
        {
            if (!Login()) return false;
            return false;
        }

        public bool StoreExtReference()
        {
            if (!Login()) return false;
            return false;
        }

        public bool StorePhoto()
        {
            if (!Login()) return false;
            return false;
        }

        public bool StorePhotoAndTXT()
        {
            if (!Login()) return false;
            return false;
        }

        public bool UnBlockTicketByJournal()
        {
            if (!Login()) return false;
            return false;
        }

        public bool UnBlockTicketBySerial()
        {
            if (!Login()) return false;
            return false;
        }

        public bool UpdatePhoto()
        {
            if (!Login()) return false;
            return false;
        }
    }
}
