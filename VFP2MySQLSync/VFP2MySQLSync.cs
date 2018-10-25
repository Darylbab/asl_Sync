using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using asl_SyncLibrary;

namespace VFP2MySQLSync
{
    public partial class VFP2MySQLSync : Form
    {
        private CommonFunctions CF = new CommonFunctions();
        private DataWarehouse DW = new DataWarehouse();
        private OleDbConnection VFPConn = new OleDbConnection("Provider=VFPOLEDB.1");

        private const string mdatadir = @"g:\fpdata\altaax\";

        public VFP2MySQLSync()
        {
            InitializeComponent();
            LblStartTime.Text = $"Start Time: {DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}";
        }

        private void VFP2MySQLSync_Shown(object sender, EventArgs e)
        {
            VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}persontype.dbf";
            DataTable DTPersonTypes = CF.LoadTable(VFPConn, $"SELECT * FROM persontype", "PersonTypes");
            if (DTPersonTypes != null)
            {
                PBPersonTypes.Maximum = DTPersonTypes.Rows.Count;
                foreach (DataRow DRT in DTPersonTypes.Rows)
                {
                    PBPersonTypes.Value = DTPersonTypes.Rows.IndexOf(DRT);
                    string PersKey = DRT["PersKey"].ToString().Trim();
                    string TQuery = string.Empty;
                    if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.PersonTypes",$"Perskey={PersKey}"))
                    {
                        TQuery = $"UPDATE {DW.ActiveDatabase}.PersonTypes SET " +
                            $"descrip='{CF.EscapeChar(DRT["descrip"].ToString().Trim())}', " +
                            $"active='{CF.FirstChar(DRT["active"].ToString())}', " +
                            $"disctype='{DRT["disctype"].ToString().Trim()}', " +
                            $"depflag='{CF.FirstChar(DRT["depflag"].ToString())}', " +
                            $"utaflag='{CF.FirstChar(DRT["utaflag"].ToString())}', " +
                            $"mtnflag='{CF.FirstChar(DRT["mtnflag"].ToString())}', " +
                            $"wbflag='{CF.FirstChar(DRT["wbflag"].ToString())}', " +
                            $"osaflag='{CF.FirstChar(DRT["osaflag"].ToString())}', " +
                            $"vflag='{CF.FirstChar(DRT["vflag"].ToString())}', " +
                            $"arflag='{CF.FirstChar(DRT["arflag"].ToString())}', " +
                            $"svflag='{CF.FirstChar(DRT["svflag"].ToString().Trim())}', " +
                            $"custid={DRT["custid"].ToString().Trim()}, " +
                            $"tgroup='{DRT["tgroup"].ToString().Trim()}', " +
                            $"acct={DRT["acct"].ToString().Trim()}, " +
                            $"axqty={DRT["axqty"].ToString().Trim()}, " +
                            $"axamt={DRT["axamt"].ToString().Trim()}, " +
                            $"qty={DRT["qty"].ToString().Trim()}, " +
                            $"amt={DRT["amt"].ToString().Trim()}, " +
                            $"variance={DRT["variance"].ToString().Trim()}, " +
                            $"tariff={DRT["tariff"].ToString().Trim()}, " +
                            $"saledate='{DRT["saledate"].ToString().Trim()}', " +
                            $"salesqty={DRT["salesqty"].ToString().Trim()}, " +
                            $"salesamt={DRT["salesamt"].ToString().Trim()}, " +
                            $"refqty={DRT["refqty"].ToString().Trim()}, " +
                            $"refamt={DRT["refamt"].ToString().Trim()}, " +
                            $"canqty={DRT["canqty"].ToString().Trim()}, " +
                            $"canamt={DRT["canamt"].ToString().Trim()}, " +
                            $"netqty={DRT["netqty"].ToString().Trim()}, " +
                            $"netamt={DRT["netamt"].ToString().Trim()} " +
                            $"WHERE perskey={PersKey}";
                    }
                    else
                    {
                        TQuery = $"INSERT INTO {DW.ActiveDatabase}.persontypes (perskey,descrip,active,disctype,depflag,utaflag,mtnflag,wbflag,osaflag,vflag,arflag,svflag,custid,tgroup,acct,axqty,axamt,qty,amt,variance,tariff,saledate,salesqty,salesamt,refqty,refamt,canqty,canamt,netqty,netamt) VALUES (" +
                            $"{PersKey},'{CF.EscapeChar(DRT["descrip"].ToString().Trim())}','{CF.FirstChar(DRT["active"].ToString().Trim())}','{CF.FirstChar(DRT["disctype"].ToString().Trim())}','{CF.FirstChar(DRT["depflag"].ToString().Trim())}','{CF.FirstChar(DRT["utaflag"].ToString().Trim())}','{CF.FirstChar(DRT["mtnflag"].ToString().Trim())}'," +
                            $"'{CF.FirstChar(DRT["wbflag"].ToString().Trim())}','{CF.FirstChar(DRT["osaflag"].ToString().Trim())}','{CF.FirstChar(DRT["vflag"].ToString().Trim())}','{CF.FirstChar(DRT["arflag"].ToString().Trim())}','{CF.FirstChar(DRT["svflag"].ToString().Trim())}',{DRT["custid"].ToString().Trim()},'{DRT["tgroup"].ToString().Trim()}'," +
                            $"{DRT["acct"].ToString().Trim()},{DRT["axqty"].ToString().Trim()},{DRT["axamt"].ToString().Trim()},{DRT["qty"].ToString().Trim()},{DRT["amt"].ToString().Trim()},{DRT["variance"].ToString().Trim()},{DRT["tariff"].ToString().Trim()},'{DRT["saledate"].ToString().Trim()}'," +
                            $"{DRT["salesqty"].ToString().Trim()},{DRT["salesamt"].ToString().Trim()},{DRT["refqty"].ToString().Trim()},{DRT["refamt"].ToString().Trim()},{DRT["canqty"].ToString().Trim()},{DRT["canamt"].ToString().Trim()},{DRT["netqty"].ToString().Trim()},{DRT["netamt"].ToString().Trim()})";
                    }
                    CF.ExecuteSQL(DW.dwConn, TQuery);
                    Application.DoEvents();
                }
            }

            VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}tickettypes.dbf";
            DataTable DTTicketTypes = CF.LoadTable(VFPConn, $"SELECT * FROM tickettypes", "TicketTypes");
            if (DTTicketTypes != null)
            {
                PBTicketTypes.Maximum = DTTicketTypes.Rows.Count;
                foreach (DataRow DRT in DTTicketTypes.Rows)
                {
                    PBTicketTypes.Value = DTTicketTypes.Rows.IndexOf(DRT);
                    string TickType = DRT["TickType"].ToString();
                    string TQuery = string.Empty;
                    if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.TicketTypes", $"TickType={TickType}"))
                    {
                        TQuery = $"UPDATE {DW.ActiveDatabase}.TicketTypes SET "
                            + $"descrip='{CF.EscapeChar(DRT["descrip"].ToString().Trim())}', "
                            + $"tgroup='{DRT["tgroup"].ToString().Trim()}', "
                            + $"acct={DRT["acct"].ToString().Trim()}, "
                            + $"active='{DRT["active"].ToString().Trim().Substring(0, 1)}', "
                            + $"empchrg='{DRT["empchrg"].ToString().Trim()}', "
                            + $"chrgflag='{DRT["chrgflag"].ToString().Trim()}', "
                            + $"discflag='{DRT["discflag"].ToString().Trim()}', "
                            + $"disctype='{DRT["disctype"].ToString().Trim()}', "
                            + $"utaflag='{DRT["utaflag"].ToString().Trim()}', "
                            + $"wbflag='{DRT["wbflag"].ToString().Trim()}', "
                            + $"osaflag='{DRT["osaflag"].ToString().Trim()}', "
                            + $"mcpflag='{DRT["mcpflag"].ToString().Trim()}', "
                            + $"mtnrepflag='{DRT["mtnrepflag"].ToString().Trim()}', "
                            + $"convflag='{DRT["convflag"].ToString().Trim()}', "
                            + $"picflag='{DRT["picflag"].ToString().Trim()}', "
                            + $"sbflag='{DRT["sbflag"].ToString().Trim().Substring(0, 1)}', "
                            + $"xtflag='{DRT["xtflag"].ToString().Trim()}', "
                            + $"forgotflag='{DRT["forgotflag"].ToString().Trim()}', "
                            + $"tokenreq='{DRT["tokenreq"].ToString().Trim()}', "
                            + $"vcamt={DRT["vcamt"].ToString().Trim()}, "
                            + $"offgroup='{DRT["offgroup"].ToString().Trim()}', "
                            + $"axqty={DRT["axqty"].ToString().Trim()}, "
                            + $"reloadqty={DRT["reloadqty"].ToString().Trim()}, "
                            + $"salesqty={DRT["salesqty"].ToString().Trim()}, "
                            + $"salesamt={DRT["salesamt"].ToString().Trim()}, "
                            + $"vcqty={DRT["vcqty"].ToString().Trim()}, "
                            + $"refqty={DRT["refqty"].ToString().Trim()}, "
                            + $"refamt={DRT["refamt"].ToString().Trim()}, "
                            + $"canqty={DRT["canqty"].ToString().Trim()}, "
                            + $"canamt={DRT["canamt"].ToString().Trim()}, "
                            + $"netqty={DRT["netqty"].ToString().Trim()}, "
                            + $"netamt={DRT["netamt"].ToString().Trim()}, "
                            + $"vcacct={DRT["vcacct"].ToString().Trim()}, "
                            + $"openorders={DRT["openorders"].ToString().Trim()}, "
                            + $"comporders={DRT["comporders"].ToString().Trim()}, "
                            + $"ordersamt={DRT["ordersamt"].ToString().Trim()}, "
                            + $"ordersqty={DRT["ordersqty"].ToString().Trim()}, "
                            + $"wcqty={DRT["wcqty"].ToString().Trim()}, "
                            + $"wcamt={DRT["wcamt"].ToString().Trim()}, "
                            + $"wreloadqty={DRT["wreloadqty"].ToString().Trim()}, "
                            + $"wreloadamt={DRT["wreloadamt"].ToString().Trim()}, "
                            + $"clist={DRT["clist"].ToString().Trim()} "
                            + $"WHERE ticktype={TickType}";
                    }
                    else
                    {
                        TQuery = $"INSERT INTO {DW.ActiveDatabase}.tickettypes (ticktype,descrip,tgroup,acct,active,empchrg,chrgflag,discflag,disctype,utaflag,wbflag,osaflag," +
                            $"mcpflag,mtnrepflag,convflag,picflag,sbflag,xtflag,forgotflag,tokenreq,vcamt,offgroup,axqty,reloadqty,salesqty,salesamt,vcqty,refqty,refamt,canqty,canamt," +
                            $"netqty,netamt,vcacct,openorders,comporders,ordersamt,ordersqty,wcqty,wcamt,wreloadqty,wreloadamt,clist) VALUES (" +
                            $"{TickType},'{CF.EscapeChar(DRT["descrip"].ToString().Trim())}','{DRT["tgroup"].ToString().Trim()}',{DRT["acct"].ToString().Trim()},'{DRT["active"].ToString().Trim().Substring(0, 1)}'," +
                            $"'{DRT["empchrg"].ToString().Trim()}','{DRT["chrgflag"].ToString().Trim()}','{DRT["discflag"].ToString().Trim()}','{DRT["disctype"].ToString().Trim()}'," +
                            $"'{DRT["utaflag"].ToString().Trim()}','{DRT["wbflag"].ToString().Trim()}','{DRT["osaflag"].ToString().Trim()}','{DRT["mcpflag"].ToString().Trim()}'," +
                            $"'{DRT["mtnrepflag"].ToString().Trim()}','{DRT["convflag"].ToString().Trim()}','{DRT["picflag"].ToString().Trim()}','{DRT["sbflag"].ToString().Trim().Substring(0, 1)}'," +
                            $"'{DRT["xtflag"].ToString().Trim()}','{DRT["forgotflag"].ToString().Trim()}','{DRT["tokenreq"].ToString().Trim()}',{DRT["vcamt"].ToString().Trim()}," +
                            $"'{DRT["offgroup"].ToString().Trim()}',{DRT["axqty"].ToString().Trim()},{DRT["reloadqty"].ToString().Trim()},{DRT["salesqty"].ToString().Trim()}," +
                            $"{DRT["salesamt"].ToString().Trim()},{DRT["vcqty"].ToString().Trim()},{DRT["refqty"].ToString().Trim()},{DRT["refamt"].ToString().Trim()}," +
                            $"{DRT["canqty"].ToString().Trim()},{DRT["canamt"].ToString().Trim()},{DRT["netqty"].ToString().Trim()},{DRT["netamt"].ToString().Trim()}," +
                            $"{DRT["vcacct"].ToString().Trim()},{DRT["openorders"].ToString().Trim()},{DRT["comporders"].ToString().Trim()},{DRT["ordersamt"].ToString().Trim()}," +
                            $"{DRT["ordersqty"].ToString().Trim()},{DRT["wcqty"].ToString().Trim()},{DRT["wcamt"].ToString().Trim()},{DRT["wreloadqty"].ToString().Trim()}," +
                            $"{DRT["wreloadamt"].ToString().Trim()},{DRT["clist"].ToString().Trim()})";
                    }
                    CF.ExecuteSQL(DW.dwConn, TQuery);
                    Application.DoEvents();
                }
            }
            VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}axpos.dbf";
            DataTable DTAXPOS = CF.LoadTable(VFPConn, $"SELECT * FROM axpos", "axpos");
            if (DTAXPOS != null)
            {
                string TQuery = string.Empty;
                PBAXPOS.Maximum = DTAXPOS.Rows.Count;
                foreach (DataRow DRPOS in DTAXPOS.Rows)
                {
                    PBAXPOS.Value = DTAXPOS.Rows.IndexOf(DRPOS) + 1;
                    if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.axpos", $"nkassanr={DRPOS["nkassanr"].ToString()}"))
                    {
                        TQuery = $"UPDATE {DW.ActiveDatabase}.axpos SET szname='{DRPOS["szname"].ToString()}', location='{DRPOS["location"].ToString()}', nstandortn={DRPOS["nstandortn"].ToString()}, sztelnr='{DRPOS["sztelnr"].ToString()}', szcomputer='{DRPOS["szcomputer"].ToString()}', nkassatypn={DRPOS["nkassatypn"].ToString()}, office='{DRPOS["office"].ToString()}', argroup='{DRPOS["argroup"].ToString()}', arcustid='{DRPOS["arcustid"].ToString()}' WHERE nkassanr={DRPOS["nkassanr"].ToString()}";
                    }
                    else
                    {
                        TQuery = $"INSERT INTO {DW.ActiveDatabase}.axpos (nkassanr,szname,location,nstandortn,sztelnr,szcomputer,nkassatypn,office,argroup,arcustid) VALUES ({DRPOS["nkassanr"].ToString().Trim()},'{DRPOS["szname"].ToString().Trim()}','{DRPOS["location"].ToString().Trim()}','{DRPOS["nstandortn"].ToString().Trim()}','{DRPOS["sztelnr"].ToString().Trim()}','{DRPOS["szcomputer"].ToString().Trim()}','{DRPOS["nkassatypn"].ToString().Trim()}','{DRPOS["office"].ToString().Trim()}','{DRPOS["argroup"].ToString().Trim()}','{DRPOS["arcustid"].ToString().Trim()}')";
                    }
                    CF.ExecuteSQL(DW.dwConn, TQuery);
                    Application.DoEvents();
                }
            }

            Application.Exit();
        }
    }
}
