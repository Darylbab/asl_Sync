﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace asl_SyncLibrary.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.3.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("server= buy.alta.com; username= syncron; password= debut-drift-dream; Convert Zer" +
            "o Datetime= True; Pooling=False;")]
        public string MySQLWS {
            get {
                return ((string)(this["MySQLWS"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source =(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10" +
            ".34.241.12)(PORT = 1521))) (CONNECT_DATA =(SERVICE_NAME = AX10G) ) ); User Id= c" +
            "wc; Password= cwc;Convert Zero Datetime=True; ")]
        public string Axess {
            get {
                return ((string)(this["Axess"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("server= axess.alta.com; username= root; password= AxA1tA!38;Convert Zero Datetime" +
            "=True;")]
        public string Mirror {
            get {
                return ((string)(this["Mirror"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://extranet.liftopia.com/x/reports/passholders.php")]
        public string LiftopiaMTC_URL {
            get {
                return ((string)(this["LiftopiaMTC_URL"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F88E77D3D9B18A6F3E8ABC4D42929&")]
        public string LiftopiaMTC_Key {
            get {
                return ((string)(this["LiftopiaMTC_Key"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("801001")]
        public string LiftopiaMTC_ID {
            get {
                return ((string)(this["LiftopiaMTC_ID"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("MTCGUESTS")]
        public string LiftopiaMTC_Data_File {
            get {
                return ((string)(this["LiftopiaMTC_Data_File"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\taskman\\c$\\acctg\\winscp\\data\\mtcguests.csv")]
        public string LiftopiaMTC_Path {
            get {
                return ((string)(this["LiftopiaMTC_Path"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://cwc.teamaxess.com:16302/axis_705/services/WTPSInterface?wsdl")]
        public string Axess_WTPSI {
            get {
                return ((string)(this["Axess_WTPSI"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://cwc.teamaxess.com:16302/axis_705/services/WTPSIAddOn?wsdl")]
        public string Axess_WTPSI_AddOn {
            get {
                return ((string)(this["Axess_WTPSI_AddOn"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("alta120")]
        public string Axess_WTPSI_User {
            get {
                return ((string)(this["Axess_WTPSI_User"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("alta")]
        public string Axess_WTPSI_Password {
            get {
                return ((string)(this["Axess_WTPSI_Password"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://cwc.teamaxess.com:16302/axis_705/services/DCI4CRM?wsdl")]
        public string Axess_DCI4CRM {
            get {
                return ((string)(this["Axess_DCI4CRM"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("POS")]
        public string Axess_DCI4CRM_User {
            get {
                return ((string)(this["Axess_DCI4CRM_User"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("705")]
        public string Axess_DCI4CRM_Password {
            get {
                return ((string)(this["Axess_DCI4CRM_Password"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("705")]
        public string Axess_Alta_Project_No {
            get {
                return ((string)(this["Axess_Alta_Project_No"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2018-06-01")]
        public global::System.DateTime SeasonStart {
            get {
                return ((global::System.DateTime)(this["SeasonStart"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2019-05-30")]
        public global::System.DateTime SeasonEnd {
            get {
                return ((global::System.DateTime)(this["SeasonEnd"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\taskman\\c$\\acctg\\temp")]
        public string SyncTempDir {
            get {
                return ((string)(this["SyncTempDir"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\alta.com\\namespace\\APPS\\fpdata\\altaax")]
        public string SyncDataDir {
            get {
                return ((string)(this["SyncDataDir"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\alta.com\\namespace\\APPS\\fpdata\\extsync")]
        public string SyncExtDir {
            get {
                return ((string)(this["SyncExtDir"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\alta.com\\namespace\\APPS\\FPData\\extsync\\uta")]
        public string SyncUTADir {
            get {
                return ((string)(this["SyncUTADir"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\alta.com\\namespace\\APPS\\fpdata\\shared")]
        public string SyncSharedDir {
            get {
                return ((string)(this["SyncSharedDir"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1969-12-31")]
        public global::System.DateTime DefaultBirthdate {
            get {
                return ((global::System.DateTime)(this["DefaultBirthdate"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2018-07-01")]
        public global::System.DateTime YearStart {
            get {
                return ((global::System.DateTime)(this["YearStart"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2019-05-31")]
        public global::System.DateTime YearEnd {
            get {
                return ((global::System.DateTime)(this["YearEnd"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("N")]
        public string TestFlag {
            get {
                return ((string)(this["TestFlag"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2019-05-02")]
        public global::System.DateTime UTAEnd {
            get {
                return ((global::System.DateTime)(this["UTAEnd"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2019-05-31")]
        public global::System.DateTime MtnColEnd {
            get {
                return ((global::System.DateTime)(this["MtnColEnd"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2019-05-31")]
        public global::System.DateTime WBEnd {
            get {
                return ((global::System.DateTime)(this["WBEnd"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2019-05-31")]
        public global::System.DateTime BPEnd {
            get {
                return ((global::System.DateTime)(this["BPEnd"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string NewFLD {
            get {
                return ((string)(this["NewFLD"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UPClist {
            get {
                return ((bool)(this["UPClist"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UPSPCards {
            get {
                return ((bool)(this["UPSPCards"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UPCClink {
            get {
                return ((bool)(this["UPCClink"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UPRestAcct {
            get {
                return ((bool)(this["UPRestAcct"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UPRemSales {
            get {
                return ((bool)(this["UPRemSales"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UPPOEReads {
            get {
                return ((bool)(this["UPPOEReads"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UPChrgs {
            get {
                return ((bool)(this["UPChrgs"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UPAR {
            get {
                return ((bool)(this["UPAR"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UPAXChk {
            get {
                return ((bool)(this["UPAXChk"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool Cleanup {
            get {
                return ((bool)(this["Cleanup"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("7")]
        public int TimeDiff {
            get {
                return ((int)(this["TimeDiff"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2018-19")]
        public string Season {
            get {
                return ((string)(this["Season"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("relay.alta.com")]
        public string SMTPHost {
            get {
                return ((string)(this["SMTPHost"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("MCRMGHMAILQ_0k3bQcWFnR9u")]
        public string SMTPUnlockCode {
            get {
                return ((string)(this["SMTPUnlockCode"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Chilkat.Email2")]
        public string SMTPObject {
            get {
                return ((string)(this["SMTPObject"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("snowbird,=20;deer valley,=30;solitude,=40")]
        public string WasatchBenefits {
            get {
                return ((string)(this["WasatchBenefits"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\taskman\\c$\\acctg\\winscp\\data")]
        public string SyncFTPDir {
            get {
                return ((string)(this["SyncFTPDir"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("999")]
        public int MaxUse_MC {
            get {
                return ((int)(this["MaxUse_MC"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("999")]
        public int MaxUse_MS {
            get {
                return ((int)(this["MaxUse_MS"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public int MaxUse_WB {
            get {
                return ((int)(this["MaxUse_WB"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int MaxUse_BP {
            get {
                return ((int)(this["MaxUse_BP"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("999")]
        public int MaxUse_OS {
            get {
                return ((int)(this["MaxUse_OS"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=184.106.144.107; user= altaadmin; password= 0VM6f58k;")]
        public string MCReciprocity {
            get {
                return ((string)(this["MCReciprocity"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AltaGuests")]
        public string LiftopiaAlta_Data_File {
            get {
                return ((string)(this["LiftopiaAlta_Data_File"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://www.liftopia.com/x/reports/guests.php?PartnerId=141&ApiKey=6EF2313B4B6D6A" +
            "2BFFF24BDB83662&v=2")]
        public string LiftopiaAlta_URL {
            get {
                return ((string)(this["LiftopiaAlta_URL"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("efcbulkimport@rideuta.com")]
        public string UTABulkEmail {
            get {
                return ((string)(this["UTABulkEmail"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("darylb@alta.com")]
        public string EmailFromAddress {
            get {
                return ((string)(this["EmailFromAddress"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2018-11-01")]
        public global::System.DateTime UTABegin {
            get {
                return ((global::System.DateTime)(this["UTABegin"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\taskman\\c$\\acctg\\temp\\ErrLog.CSV")]
        public string TempErrLog {
            get {
                return ((string)(this["TempErrLog"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=altajava\\srsql;Initial Catalog=SR.DB;Integrated Security=True")]
        public string SRConnection {
            get {
                return ((string)(this["SRConnection"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\alta.com\\namespace\\public\\_IT\\Alta Created CSV Files\\")]
        public string ITFileCopyFolder {
            get {
                return ((string)(this["ITFileCopyFolder"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://cwc.teamaxess.com:16351/OBI4POS/")]
        public string Axess_OBI4POS {
            get {
                return ((string)(this["Axess_OBI4POS"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://wtpsi.teamaxess.com:16351/DCI4WTP/DCI4WTPService.svc?wsdl")]
        public string Axess_DCI4WTP {
            get {
                return ((string)(this["Axess_DCI4WTP"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://cwc.teamaxess.com:16351/DCI4Synchronisation/DCI4SynchronisationService.svc" +
            "?")]
        public string Axess_POS_SYNC {
            get {
                return ((string)(this["Axess_POS_SYNC"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("server=web-database.alta.com;username=keyhole;password=MightyM0use;Convert Zero D" +
            "atetime=True;")]
        public string MySQLDW {
            get {
                return ((string)(this["MySQLDW"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("server= asgard.alta.com; username=heimdall; password= ragnarok;Convert Zero Datet" +
            "ime=True;")]
        public string AsgardRW {
            get {
                return ((string)(this["AsgardRW"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("server= asgard.alta.com; username=amora; password= rainbow;Convert Zero Datetime=" +
            "True;")]
        public string AsgardRO {
            get {
                return ((string)(this["AsgardRO"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("server= asgard.alta.com; username=loki; password=S3rpent!;Convert Zero Datetime=T" +
            "rue;")]
        public string AsgardDebug {
            get {
                return ((string)(this["AsgardDebug"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\taskman\\c$\\acctg\\winscp\\data\\")]
        public string SharedDataFolder {
            get {
                return ((string)(this["SharedDataFolder"]));
            }
        }
    }
}
