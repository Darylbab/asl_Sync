using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;
using MySql.Data.MySqlClient;


namespace asl_SyncLibrary
{
    public class POSSync
    {
        ASLError TError = new ASLError();
        CommonFunctions CF = new CommonFunctions();

        private int TErrorNo = 0;
        private string TErrorMsg = "";
        private long TSessionID = 0;


        public Serverstatus GetServerStatus(string aURL)
        {
            if (CF.IsURLOnline(Properties.Settings.Default.Axess_POS_SYNC))
            {
                return Serverstatus.Active;
            }
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

        private XmlDocument BuildSOAPEnvelope(string Function, string Parameters, bool useSessionID = true)
        {
            XmlDocument soapXMLDocument = new XmlDocument();
            string XMLData = "<" + Function + ">";
            if (useSessionID)
            {
                XMLData += "<i_nSessionID>" + TSessionID + "</i_nSessionID>";
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
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Properties.Settings.Default.Axess_POS_SYNC);
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
                                    TErrorNo = -667;
                                    TErrorMsg = localXML.GetElementsByTagName("checkDBConnReturn").Item(0).InnerText.Replace("Error:", "").Trim();
                                }
                                else
                                {
                                    TErrorNo = 0;
                                    TErrorMsg = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            if (localXML.GetElementsByTagName("NERRORNO").Item(0).InnerText.Length != 0)
                            {
                                TErrorNo = System.Convert.ToInt16(localXML.GetElementsByTagName("NERRORNO").Item(0).InnerText);
                                TErrorMsg = localXML.GetElementsByTagName("SZERRORMESSAGE").Item(0).InnerText;
                            }
                            else
                            {
                                TErrorNo = -666;
                                TErrorMsg = "Axess API returned an invalid response.";
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
                TErrorMsg = ex.Message;
                TErrorNo = -666;
                TError.UpdateErrorLog(errAppName, TErrorNo, TErrorMsg);
            }
            return new XmlDocument();
        }




    }
}
