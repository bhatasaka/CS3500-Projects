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
            spreadsheet = new Spreadsheet(isValid, s => s.ToUpper(), "PS6");

            this.spreadsheetPanel1.SelectionChanged += onCellClicked;

            this.AcceptButton = EnterButton;
        }

        /// <summary>
        /// isValid method to pass to the spreadsheet object to check variables names.
        /// Only allows variables through that are the name of a cell in the spreadsheet.
        /// (Letter followed by number - Ex. A99. No more than one letter, no number higher than 99)
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        private bool isValid(string varName)
        {
            int varLength = varName.Count();
            int varNumber = 0;

            //Checks for length and if the first char is a letter
            if ((varLength == 2 || varLength == 3) && Char.IsLetter(varName[0]))
            {
                //Check that the following characters make a number
                if (int.TryParse(varName.Substring(1), out varNumber))
                {
                    if (varNumber <= 99 && varNumber >= 0)
                        return true;
                }
            }
            return false;
        }

        private void onCellClicked(SpreadsheetPanel p)
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
            Close();
        }

        /// <summary>
        /// Generated.
        /// Handles any closing event, including the X in the upper right hand side of the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PS6_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            CloseForm(e);
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

        private void CloseForm(FormClosingEventArgs closeEvent)
        {
            if (spreadsheet.Changed)
            {
                DialogResult closeWithoutSavingResult = MessageBox.Show("Close without saving?", this.Name,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (closeWithoutSavingResult.Equals(DialogResult.No))
                    closeEvent.Cancel = true;
            }
        }
    }
}
