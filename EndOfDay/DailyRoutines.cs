using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using asl_SyncLibrary;

namespace DailyRoutines
{
    public partial class frmDailyRoutines : Form
    {
        Mirror AM = new Mirror();
        BUY_ALTA_COM BUY = new BUY_ALTA_COM();
        CommonFunctions CF = new CommonFunctions();
        DataWarehouse DW = new DataWarehouse();


        public frmDailyRoutines()
        {
            InitializeComponent();
        }


        private void btnRunUpdates_Click(object sender, EventArgs e)
        {
            if (cbCalcIKONCharges.Checked)
            {
                string RunDate = DateTime.Today.AddDays(-1).ToString(Mirror.AxessDateFormat);
                string Query = $"UPDATE {DW.ActiveDatabase}.salesdata AS S INNER JOIN {DW.ActiveDatabase}.skivisits AS V ON V.SERIALKEY = S.SERIALKEY SET S.TARIFF = 70 WHERE S.nkassanr=27 AND S.saledate <= '{RunDate}' AND (V.ALTAFLAG = 'Y' AND V.SBFLAG = 'N')";
                CF.ExecuteSQL(DW.dwConn, Query);
                Query = $"UPDATE {DW.ActiveDatabase}.salesdata AS S INNER JOIN {DW.ActiveDatabase}.skivisits AS V ON V.SERIALKEY = S.SERIALKEY SET S.TARIFF = 35 WHERE S.nkassanr=27 AND S.saledate <= '{RunDate}' AND (V.ALTAFLAG = 'Y' AND V.SBFLAG = 'Y')";
                CF.ExecuteSQL(DW.dwConn, Query);
                Query = $"UPDATE {DW.ActiveDatabase}.pmtdata AS P INNER JOIN {DW.ActiveDatabase}.salesdata AS S ON S.transkey = P.TRANSKEY INNER JOIN {DW.ActiveDatabase}.skivisits AS V ON V.SERIALKEY = S.SERIALKEY SET P.pmtamt = 70 WHERE S.nkassanr = 27 AND S.saledate <= '{RunDate}' AND (V.ALTAFLAG = 'Y' AND V.SBFLAG = 'N')";
                CF.ExecuteSQL(DW.dwConn, Query);
                Query = $"UPDATE {DW.ActiveDatabase}.pmtdata AS P INNER JOIN {DW.ActiveDatabase}.salesdata AS S ON S.transkey = P.TRANSKEY INNER JOIN {DW.ActiveDatabase}.skivisits AS V ON V.SERIALKEY = S.SERIALKEY SET P.pmtamt = 35 WHERE S.nkassanr = 27 AND S.saledate <= '{RunDate}' AND (V.ALTAFLAG = 'Y' AND V.SBFLAG = 'Y')";
                CF.ExecuteSQL(DW.dwConn, Query);
                Query = $"DELETE FROM {DW.ActiveDatabase}.salesdata WHERE nkassanr = 27 AND saledate <= '{RunDate}' AND tariff = 0";
                //CF.ExecuteSQL(DW.dwConn, Query);
                Query = $"DELETE FROM {DW.ActiveDatabase}.pmtdata WHERE nkassanr = 27 AND saledate <= '{RunDate}' AND pmtamt = 0";
                //CF.ExecuteSQL(DW.dwConn, Query);
                AM.UpdateLog("MidnightUpdate", 0);
            }
            if (cbSetSPCardStatus.Checked)
            {
                DW.UpdateSPCardStatus();
                //DW.UpdateSPCardsUses();
            }
            if (cbUpdateUses.Checked)
            {
                //DW.UpdateCListPermUses();
                DW.UpdateSPCardsUses();
                DW.UpdateSalesDataUses();
                //UpdateMTCIssuedUses();
            }
        }

        private void frmDailyRoutines_Shown(object sender, EventArgs e)
        {
            string RunDate = DateTime.Today.AddDays(-1).ToString(Mirror.AxessDateFormat);
            string Query = $"UPDATE {DW.ActiveDatabase}.salesdata AS S INNER JOIN {DW.ActiveDatabase}.skivisits AS V ON V.SERIALKEY = S.SERIALKEY SET S.TARIFF = 70 WHERE S.nkassanr=27 AND S.saledate <= '{RunDate}' AND (V.ALTAFLAG = 'Y' AND V.SBFLAG = 'N')";
            CF.ExecuteSQL(DW.dwConn, Query);
            Query = $"UPDATE {DW.ActiveDatabase}.salesdata AS S INNER JOIN {DW.ActiveDatabase}.skivisits AS V ON V.SERIALKEY = S.SERIALKEY SET S.TARIFF = 35 WHERE S.nkassanr=27 AND S.saledate <= '{RunDate}' AND (V.ALTAFLAG = 'Y' AND V.SBFLAG = 'Y')";
            CF.ExecuteSQL(DW.dwConn, Query);
            Query = $"UPDATE {DW.ActiveDatabase}.pmtdata AS P INNER JOIN {DW.ActiveDatabase}.salesdata AS S ON S.transkey = P.TRANSKEY INNER JOIN {DW.ActiveDatabase}.skivisits AS V ON V.SERIALKEY = S.SERIALKEY SET P.pmtamt = 70 WHERE S.nkassanr = 27 AND S.saledate <= '{RunDate}' AND (V.ALTAFLAG = 'Y' AND V.SBFLAG = 'N')";
            CF.ExecuteSQL(DW.dwConn, Query);
            Query = $"UPDATE {DW.ActiveDatabase}.pmtdata AS P INNER JOIN {DW.ActiveDatabase}.salesdata AS S ON S.transkey = P.TRANSKEY INNER JOIN {DW.ActiveDatabase}.skivisits AS V ON V.SERIALKEY = S.SERIALKEY SET P.pmtamt = 35 WHERE S.nkassanr = 27 AND S.saledate <= '{RunDate}' AND (V.ALTAFLAG = 'Y' AND V.SBFLAG = 'Y')";
            CF.ExecuteSQL(DW.dwConn, Query);
            Query = $"DELETE FROM {DW.ActiveDatabase}.salesdata WHERE nkassanr = 27 AND saledate <= '{RunDate}' AND tariff = 0";
            //CF.ExecuteSQL(DW.dwConn, Query);
            Query = $"DELETE FROM {DW.ActiveDatabase}.pmtdata WHERE nkassanr = 27 AND saledate <= '{RunDate}' AND pmtamt = 0";
            //CF.ExecuteSQL(DW.dwConn, Query);
            DW.UpdateSPCardStatus();
            //DW.UpdateCListPermUses();
            DW.UpdateSPCardsUses();
            DW.UpdateSalesDataUses();
            //UpdateMTCIssuedUses();
            AM.UpdateLog("MidnightUpdate", 0);
            Application.Exit();
        }

        //private void UpdateMTCIssuedUses()
        //{
        //    DataTable MTCIssued = CF.LoadTable(BUY.Buy_Alta_ComConn, $"SELECT recid, serialkey, uses FROM asbshared.mtncol_issued WHERE serialkey <> '' ORDER BY recid", "MTCIssued");
        //    if (CF.TableHasData(MTCIssued))
        //    {
        //        foreach (DataRow MTC in MTCIssued.Rows)
        //        {
        //            System.Diagnostics.Debug.Print($"{MTCIssued.Rows.IndexOf(MTC) + 1} of {MTCIssued.Rows.Count}");
        //            long Uses = CF.RowCount(DW.dwConn, $"{DW.ActiveDatabase}.skivisits", $"READDATE >= '2018-11-23' AND serialkey = '{MTC["serialkey"].ToString()}'");
        //            if (Uses != MTC.Field<Int32>("uses"))
        //            {
        //                CF.ExecuteSQL(BUY.Buy_Alta_ComConn, $"UPDATE asbshared.mtncol_issued SET uses={Uses} WHERE recid={MTC["recid"].ToString()}");
        //            }
        //        }
        //    }
        //}

    }
}
