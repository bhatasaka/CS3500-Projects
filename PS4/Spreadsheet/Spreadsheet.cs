using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

/// <summary>
/// Author: Bryan Hatasaka
///         u1028471
/// </summary>
namespace SS
{
    /// <summary>
    /// An Spreadsheet object inherits all properties from an AbstractSpreadsheet
    /// - the object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string is a valid cell name if and only if:
    ///   (1) its first character is an underscore or a letter
    ///   (2) its remaining characters (if any) are underscores and/or letters and/or digits
    /// Note that this is the same as the definition of valid variable from the PS3 Formula class.
    /// 
    /// For example, "x", "_", "x2", "y_15", and "___" are all valid cell  names, but
    /// "25", "2x", and "&" are not.  Cell names are case sensitive, so "x" and "X" are
    /// different cell names.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  (This
    /// means that a spreadsheet contains an infinite number of cells.)  In addition to 
    /// a name, each cell has a contents and a value.  The distinction is important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid.)
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        // Dependecy graph object for holding dependencies between cells
        private DependencyGraph dependencies;

        // This dictionary holds all of the non-empty cells.
        // The name of the cell is the key and the cell is the value.
        // Used because of contant time access.
        private Dictionary<String, Cell> cells;

        public override bool Changed { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        /// <summary>
        /// Creates a spreadsheet object containing all empty cells.
        /// </summary>
        public Spreadsheet() : base(s=>true, s=>s, "default")
        {
            dependencies = new DependencyGraph();
            cells = new Dictionary<String,Cell>();
        }

        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version): base(isValid, normalize, version)
        {
            dependencies = new DependencyGraph();
            cells = new Dictionary<String, Cell>();
        }

        public Spreadsheet(String filePath, Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            dependencies = new DependencyGraph();
            cells = new Dictionary<String, Cell>();
            LoadFile(filePath);
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        public override object GetCellContents(string name)
        {
            name = VerifyName(name);
            //Returns an empty string for empty cells
            if (!cells.ContainsKey(name))
                return "";

            Cell cell = cells[name];
            //Data is protected by cell class
            return cell.Contents;
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            //Returns a copy for data protection
            return cells.Keys.ToArray<String>();
        }

        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            //Throws an InvalidNameException if the name is not valid
            name = VerifyName(name);
            if (content == null)
                throw new ArgumentNullException();

            if (Double.TryParse(content, out double parsedDouble))
            {
                return SetCellContents(name, parsedDouble);
            }
            else if (content.Length > 0 &&  content[0].Equals('='))
            {
                Formula formula = new Formula(content.Remove(0, 1), this.Normalize, this.IsValid);
                return SetCellContents(name, formula);
            }
            else
            {
                return SetCellContents(name, content);
            }
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            return HandleSetCell(name, number);
        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            return HandleSetCell(name, text);
        }

        /// <summary>
        /// If the formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.  (No change is made to the spreadsheet.)
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {
            return HandleSetCell(name, formula);
        }

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            //Necessary because VerifyName will throw an InvalidNameException if the 
            //name is invalid and this method throws an Argument null exception if the
            //name is invalid.
            if (name == null)
                throw new ArgumentNullException();
            name = VerifyName(name);
            return dependencies.GetDependents(name);
        }

        public override string GetSavedVersion(string filename)
        {
            throw new NotImplementedException();
        }

        public override void Save(string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.OmitXmlDeclaration = true;

            using (XmlWriter writer = XmlWriter.Create(filename, settings))
            {
                writer.WriteStartElement("spreadsheet");
                writer.WriteStartAttribute("version");
                writer.WriteString(this.Version);
                writer.WriteEndAttribute();

                //writer.WriteAttributeString("Spreadsheet", this.Version);
                foreach(String name in GetNamesOfAllNonemptyCells())
                {
                    cells[name].WriteXML(name, writer);
                }
                writer.WriteEndElement();
            }
        }

        public override object GetCellValue(string name)
        {
            name = VerifyName(name);
            if (cells.ContainsKey(name))
            {
                //Data protected through cell property
                return cells[name].Contents;
            }
            else
            {
                //TODO
                return 0;
            }
        }

        /// <summary>
        /// Helper method for the SetCellContents methods.
        /// Creates and adds valid cells to the object dictionary.
        /// Creates, adds and removes dependecies as needed.
        /// Invariant that the cell name is valid as it was checked in SetContentsOfCell
        /// 
        /// 
        /// Throws an InvalidNameException if the name is invalid or null
        /// Throws an ArgumentNullException if the contents are null
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        private HashSet<String> HandleSetCell(string name, Object contents)
        {
            HashSet<string> allDependentsForCell;
            Cell oldCell = null;

            //Checks if the cell already exists
            if (cells.ContainsKey(name))
            {
                oldCell = cells[name];
                cells.Remove(name); //Remove the cell if it exists already
            }
            HashSet<String> oldDependees = new HashSet<String>(RecalculateDependecies(name, contents));

            //Makes a new HashSet of all of the cells that will be affected by changing this cell
            // plus this cell.
            //Throws a CircularException if there is a circular dependency.

            try
            {
                allDependentsForCell = new HashSet<string>(GetCellsToRecalculate(name).ToArray<string>());
            }
            catch (CircularException)
            {
                //If an exception is found, revert the dependency graph and contents then throw the exception
                dependencies.ReplaceDependees(name, oldDependees);
                if (oldCell != null)
                {
                    cells.Add(name, oldCell);
                }
                throw;
            }

            //If the cell doesn't contain an empty string, add the new cell to the set
            // (otherwise it stays removed from the dictionary)
            if (!contents.Equals(""))
            {
                Cell cell = new Cell(contents, Lookup);
                cells.Add(name, cell);
            }

            return allDependentsForCell;
        }

        /// <summary>
        /// Verifies that the passed string is a valid name.
        /// According to the class descripition:
        /// A string is a valid cell name if and only if:
        ///   (1) its first character is an underscore or a letter
        ///   (2) its remaining characters (if any) are underscores and/or letters and/or digits
        ///   
        /// throws an InvalidNameException if the name is not valid.
        /// </summary>
        /// <param name="name"></param>
        private string VerifyName(string name)
        {
            if (name == null)
                throw new InvalidNameException();
            char letter = name[0];
            //Checking the first character to be an _ or a letter
            if (!letter.Equals('_') && !Char.IsLetter(letter))
                throw new InvalidNameException();

            // Traversing through the string to check that the rest of the characters are
            // letters, numbers or underscores
            for (int letterPos = 1; letterPos < name.Length; letterPos++)
            {
                letter = name[letterPos];
                if (!letter.Equals('_') && !Char.IsLetter(letter) && !Char.IsNumber(letter))
                    throw new InvalidNameException();
            }

            //Return the normalized version of the name after it is validated
            return Normalize(name);
        }

        /// <summary>
        /// Recaculates dependencies of the passed cell parameters.
        /// Will add and remove as needed.
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        private HashSet<String> RecalculateDependecies(string name, Object contents)
        {
            HashSet<String> oldDependendees;
            if (contents is Formula)
            {
                Formula formula = (Formula)contents;
                //All of the variables in the current formula (aka new dependees)
                HashSet<String> variables = new HashSet<String>(formula.GetVariables());

                //Store cell's old dependees in case they need to be reverted
                oldDependendees = new HashSet<String>(dependencies.GetDependees(name));

                //Replace the cell's old dependees with the variables it contains
                dependencies.ReplaceDependees(name, variables);
            }

            //If the contents isn't a formula, remove this cell from the dependents of
            //all of this cell's old dependees
            else
            {
                dependencies.ReplaceDependees(name, new HashSet<String>());
                oldDependendees = new HashSet<string>();
            }

            return oldDependendees;
        }


        private void LoadFile(string filePath)
        {
            throw new NotImplementedException();
        }

        private double Lookup(String var)
        {
            Object cellValue = GetCellValue(var);
            if(cellValue is double)
            {
                return (Double)cellValue;
            }
            else
            {
                throw new ArgumentException("A number value for the given variable could not be found");
            }
        }

        /// <summary>
        /// A cell is an object which contains an object that may be
        /// a double, a string or a Formula.
        /// </summary>
        private class Cell
        {
            private object p_contents;
            private object p_value;

            /// <summary>
            /// The contents of the cell object
            /// </summary>
            public object Contents
            {
                get { return p_contents; }
            }

            public object Value
            {
                get { return p_value; }
            }

            /// <summary>
            /// Creates a cell with the passed contents.
            /// There is an invariant that say a cell will only have contents
            /// that is a double, string or Formula.
            /// </summary>
            /// <param name="contents"></param>
            public Cell(Object contents, Func<String, double> lookup)
            {
                p_contents = contents;
                CalculateValue(lookup);
            }

            public void CalculateValue(Func<String, double> lookup)
            {
                if(p_contents is Formula)
                {
                    Formula form = (Formula)p_contents;
                    p_value = form.Evaluate(lookup);
                }
            }

            public void WriteXML(string name, XmlWriter writer)
            {
                writer.WriteStartElement("cell");
                writer.WriteElementString("name", name);
                writer.WriteElementString("contents", p_contents.ToString());
                writer.WriteEndElement();
            }
        }
    }
}

