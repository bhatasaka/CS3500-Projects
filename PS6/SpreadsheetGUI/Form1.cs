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
        AbstractSpreadsheet spreadsheet;
        public PS6()
        {
            InitializeComponent();
            spreadsheet = new Spreadsheet(); //TODO

            this.spreadsheetPanel1.SelectionChanged += onCellClicked;

            this.AcceptButton = EnterButton;
        }

        public void onCellClicked(SpreadsheetPanel p)
        {
            int row, col;
            p.GetSelection(out col, out row);
            string cellName = GetCellName(col, row);
            ContentsBox.Text = spreadsheet.GetCellContents(cellName).ToString();
        }

        private string GetCellName(int col, int row)
        {
            col += 65; //Translate to Unicode code
            row += 1;
            return (char)col + row.ToString();
        }

        private void WriteCellContents(SpreadsheetPanel p)
        {
            int row, col;
            p.GetSelection(out col, out row);
            string cellName = GetCellName(col, row);
            spreadsheet.SetContentsOfCell(cellName, ContentsBox.Text);

            p.SetValue(col, row, spreadsheet.GetCellValue(cellName).ToString());
        }

        private void EnterButton_Click(object sender, EventArgs e)
        {
            WriteCellContents(spreadsheetPanel1);
        }
    }
}
