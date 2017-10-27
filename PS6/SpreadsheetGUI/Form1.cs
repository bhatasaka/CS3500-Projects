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
using System.Xml;
using System.Text.RegularExpressions;

namespace SpreadsheetGUI
{
    public partial class PS6 : Form
    {
        AbstractSpreadsheet spreadsheet;
        string saveFileName;
        public PS6()
        {
            InitializeComponent();
            spreadsheet = new Spreadsheet(isValid, s => s.ToUpper(), "PS6");

            this.spreadsheetPanel1.SelectionChanged += onCellClicked;

            this.AcceptButton = EnterButton;
            saveFileName = null;
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
            if (cellContents is Formula)
                cellContents = "=" + cellContents;

            ContentsBox.Text = cellContents.ToString();
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

        /// <summary>
        /// Will set col and row to be equal to the correct numbers
        /// corresponding to the cell name.
        /// For example: A1 gets translated to col = 0, row = 0
        /// Z99 gets translated to col = 25, row == 98
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="col"></param>
        /// <param name="row"></param>
        private void GetCellIndexes(string cell, out int col, out int row)
        {
            col = cell[0] - 65;
            row = int.Parse(cell.Substring(1)) - 1;
        }

        private void WriteCellContents(SpreadsheetPanel p)
        {
            int row, col;
            p.GetSelection(out col, out row);
            string cellName = GetCellName(col, row);
            ISet<string> cells;
            try
            {
                //Method that may throw the exception
                cells = spreadsheet.SetContentsOfCell(cellName, ContentsBox.Text);

                object cellValue;
                // Iterates through and updates the SpreadsheetPanel to show the value of all cells that
                // may or may not have changed value due to updating this cell. (Will update this selected cell as well)
                foreach (string cell in cells)
                {
                    cellValue = spreadsheet.GetCellValue(cell);
                    GetCellIndexes(cell, out col, out row);
                    p.SetValue(col, row, cellValue.ToString());
                }
            }
            catch(CircularException)
            {
                MessageBox.Show("There are one or more circular references where a cell refers to its own " +
                    "cell either directly or indirectly. To fix this, change the references or remove them.",
                    this.Name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (FormulaFormatException)
            {
                MessageBox.Show("An incorrect reference to another cell was found. Check the cell name. " +
                    "Only the cells available in this spreadsheet can be referenced.",
                    this.Name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void CloseForm(FormClosingEventArgs closeEvent)
        {
            if (spreadsheet.Changed)
            {
                DialogResult saveResult = MessageBox.Show("Save before closing?", this.Name,
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

                if (saveResult.Equals(DialogResult.Cancel))
                    closeEvent.Cancel = true;
                else if (saveResult.Equals(DialogResult.Yes))
                    save();

            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (spreadsheet.Changed)
            {
                DialogResult loadWithoutSavingResult = MessageBox.Show("Are you sure you want to" +
                    " load a spreadsheet? This action will overwrite any unsaved changes.",
                    this.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (loadWithoutSavingResult.Equals(DialogResult.No))
                    return;
            }

            OpenFileDialog open = new OpenFileDialog();
            open.InitialDirectory = "C:\\";
            open.Filter = "Spreadsheet Files (*.spdr)|*.spdr|All Files (*.*)|*.*";
            open.FilterIndex = 1;
            open.RestoreDirectory = true;

            if (open.ShowDialog() == DialogResult.OK)
            {
                //Erase everything currently on the spreadsheet
                spreadsheet = new Spreadsheet(isValid, s => s.ToUpper(), "PS6");
                spreadsheetPanel1.Clear();
                //Attempt to load new data into the spreadsheet
                if (open.FileName.Substring(open.FileName.Length - 5) == ".sprd")
                {
                    try
                    {

                        XmlReader reader = XmlReader.Create(open.FileName);
                        bool enteredCell = false;
                        bool enteredName = false;
                        bool enteredContents = false;
                        string currentCell = "";
                        string currentContents = "";
                        while (reader.Read())
                        {
                            switch(reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    if (reader.Name == "xml") break;
                                    else if (reader.Name == "spreadsheet")
                                    {
                                        if (reader.GetAttribute("version") != "PS6")
                                        {
                                            throw new SpreadsheetReadWriteException("The opened file was" +
                                        "the correct format, but was not version PS6. Please load a " +
                                        "spreadsheet of the correct version.");
                                        }
                                        break;
                                    }
                                    else if (reader.Name == "cell")
                                    {
                                        if (enteredCell == true)
                                            throw new SpreadsheetReadWriteException("Found a cell within" +
                                                "a cell. Please load a valid spreadsheet.");
                                        else enteredCell = true;
                                        break;
                                    }
                                    else if (reader.Name == "name")
                                    {
                                        if (enteredName == true)
                                            throw new SpreadsheetReadWriteException("Found two" +
                                                "names within a cell. Please load a valid spreadsheet.");
                                        else enteredName = true;
                                        break;
                                    }
                                    else if (reader.Name == "contents")
                                    {
                                        if (enteredContents == true)
                                            throw new SpreadsheetReadWriteException("Found two" +
                                                "contents within a cell. Please load a valid spreadsheet.");
                                        else enteredContents = true;
                                        break;
                                    }
                                    else throw new SpreadsheetReadWriteException("Found an unfamiliar" +
                                        "element type in spreadsheet file. File is corrupted.");
                                case XmlNodeType.Text:
                                    if (enteredName == true)
                                    {
                                        currentCell = reader.Value;
                                        break;
                                    }
                                    else if (enteredContents == true)
                                    {
                                        currentContents = reader.Value;
                                        break;
                                    }
                                    else break;
                                case XmlNodeType.EndElement:
                                    if (reader.Name == "spreadsheet")
                                        return;
                                    else if (reader.Name == "cell")
                                    {
                                        if (enteredCell == false)
                                            throw new SpreadsheetReadWriteException("Found a closing" +
                                                "cell tag without an open tag. Please load a valid" +
                                                "spreadsheet.");
                                        else if (currentCell == "" || currentContents == "")
                                            throw new SpreadsheetReadWriteException("Found a cell" +
                                                "with either no name or no contents. Please load a" +
                                                "valid spreadsheet.");
                                        else
                                        {
                                            WriteCellContents(currentCell, currentContents);
                                            enteredCell = false;
                                            break;
                                        }
                                    }
                                    else if (reader.Name == "name")
                                        if (enteredName == false)
                                            throw new SpreadsheetReadWriteException("Found a closing" +
                                                "name tag without an open tag. Please load a valid" +
                                                "spreadsheet.");
                                        else
                                        {
                                            enteredName = false;
                                            break;
                                        }
                                    else if (reader.Name == "contents")
                                        if (enteredContents == false)
                                            throw new SpreadsheetReadWriteException("Found a closing" +
                                                "contents tag without an open tag. Please load a valid" +
                                                "spreadsheet.");
                                        else
                                        {
                                            enteredContents = false;
                                            break;
                                        }
                                    else break;
                            }
                        }
                    }
                    catch (SpreadsheetReadWriteException readwriteExc)
                    {
                        DialogResult errorOpeningFileResult = MessageBox.Show(readwriteExc.Message);
                        return;
                    }
                    catch (XmlException xmlexc)
                    {
                        DialogResult errorOpeningFileResult = MessageBox.Show(xmlexc.Message);
                        return;
                    }
                }
                else
                {
                    DialogResult errorOpeningFileResult = MessageBox.Show("The selected file was" +
                        "not a .sprd file. Please try again.");
                    return;
                }
            }
            else
            {
                DialogResult errorOpeningFileResult = MessageBox.Show("There was an error" +
                    "loading your file. Please try again.");
                return;
            }
        }

        private void WriteCellContents(string cellName, string cellValue)
        {
            int row = ParseRowFromCellName(cellName);
            int col = ParseColFromCellName(cellName);
            ISet<string> cells;

            //Method that may throw the exception
            cells = spreadsheet.SetContentsOfCell(cellName, cellValue);

            object cellValueObject;
            // Iterates through and updates the SpreadsheetPanel to show the value of all cells that
            // may or may not have changed value due to updating this cell. (Will update this selected cell as well)
            foreach (string cell in cells)
            {
                cellValueObject = spreadsheet.GetCellValue(cell);
                GetCellIndexes(cell, out col, out row);
                spreadsheetPanel1.SetValue(col, row, cellValueObject.ToString());
            }
        }

        private int ParseColFromCellName(string cellName)
        {
            char letter = Regex.Match(cellName.ToUpper(), "[A-Z]").Value[0];
            int col = letter - 65;
            return col;
        }

        private int ParseRowFromCellName(string cellName)
        {
            return Convert.ToInt32(Regex.Match(cellName, "[0-9]+").Value) - 1;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save();
        }

        private void save()
        {
            if(saveFileName == null)
                saveAs();
            else
                spreadsheet.Save(saveFileName);
        }

        private void saveAs()
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Spreadsheet Files (*.sprd)|*.sprd|All files (*.*)|*.*";
            save.FilterIndex = 1;
            save.RestoreDirectory = true;
            save.OverwritePrompt = true;

            if (save.ShowDialog() == DialogResult.OK)
            {
                saveFileName = save.FileName;
                spreadsheet.Save(saveFileName);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAs();
        }
    }
}
