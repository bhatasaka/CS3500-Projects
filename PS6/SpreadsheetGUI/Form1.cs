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
using SpreadsheetUtilities;

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
            object cellContents = spreadsheet.GetCellContents(cellName);
            string cellValue;
            if (cellContents is Formula)
                cellValue = "=" + cellContents;
            else
                cellValue = cellContents.ToString();

            ContentsBox.Text = cellValue;
            cellValueLabel.Text = spreadsheet.GetCellValue(cellName).ToString();

            ContentsBox.Focus();
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

            object cellValue = spreadsheet.GetCellValue(cellName);

            p.SetValue(col, row, cellValue.ToString());
        }

        private void EnterButton_Click(object sender, EventArgs e)
        {
            WriteCellContents(spreadsheetPanel1);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Tell the application context to run the form on the same
            // thread as the other forms.
            PS6ApplicationContext.getAppContext().RunForm(new PS6());
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (spreadsheet.Changed)
            {
                DialogResult closeWithoutSavingResult = MessageBox.Show("Close without saving?", this.Name,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (closeWithoutSavingResult.Equals(DialogResult.No))
                    return;
            }
            Close();
        }
    }
}
