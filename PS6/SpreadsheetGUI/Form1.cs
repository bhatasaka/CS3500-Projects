using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SS;

namespace SpreadsheetGUI
{
    public partial class PS6 : Form
    {
        public PS6()
        {
            InitializeComponent();

            this.spreadsheetPanel1.SelectionChanged += onCellClicked;

            this.AcceptButton = EnterButton;
        }

        public void onCellClicked(SpreadsheetPanel p)
        {
            //Do stuff
        }
    }
}
