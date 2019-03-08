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


namespace GiftCardImport
{
    public partial class GiftCardImport : Form
    {
        CommonFunctions CF = new CommonFunctions();
        
        
        public GiftCardImport()
        {
            InitializeComponent();
        }

        public void RunImports()
        {
            if (cbRootRez.Checked)
            {
                DataTable RootRez = CF.CSV2Table(@"\\taskman\c$\acctg\winscp\data\rootrez_gc.csv", "RootRez");
                if (CF.TableHasData(RootRez))
                {
                    foreach (DataRow RR in RootRez.Rows)
                    {
                        
                        switch (RR["Status"].ToString().ToUpper())
                        {
                            case "COMPLETED":
                                break;
                            case "CONFIRMED":
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}
