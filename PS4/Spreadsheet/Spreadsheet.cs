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

        private bool p_changed;

        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed
        {
            get { return p_changed; }
            protected set { p_changed = value; }
        }

        /// <summary>
        /// Creates a spreadsheet object containing all empty cells.
        /// Imposes no extra validity conditions, normalizes every cell
        /// name to itself, and has version "default"
        /// </summary>
        public Spreadsheet() : base(s => true, s => s, "default")
        {
            dependencies = new DependencyGraph();
            cells = new Dictionary<String, Cell>();
        }

        /// <summary>
        /// Constructs a spreadsheet by recording its variable validity test,
        /// its normalization method, and its version information.  The variable validity
        /// test is used throughout to determine whether a string that consists of one or
        /// more letters followed by one or more digits is a valid cell name.  The variable
        /// equality test should be used thoughout to determine whether two variables are
        /// equal.
        /// </summary>
        /// 
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="version"></param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            dependencies = new DependencyGraph();
            cells = new Dictionary<String, Cell>();
        }

        /// <summary>
        /// Constructs a spreadsheet by recording its variable validity test,
        /// its normalization method, and its version information.  The variable validity
        /// test is used throughout to determine whether a string that consists of one or
        /// more letters followed by one or more digits is a valid cell name.  The variable
        /// equality test should be used thoughout to determine whether two variables are
        /// equal.
        /// Takes in a file path to load a file.
        /// </summary>
        /// 
        /// <param name="filePath"></param>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="version"></param>
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
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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
        /// <returns></returns>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            //Returns a copy for data protection
            return cells.Keys.ToArray<String>();
        }

        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            //Throws an InvalidNameException if the name is not valid
            name = VerifyName(name);
            if (content == null)
                throw new ArgumentNullException();

            p_changed = true;

            if (Double.TryParse(content, out double parsedDouble))
            {
                return SetCellContents(name, parsedDouble);
            }
            else if (content.Length > 0 && content[0].Equals('='))
            {
                Formula formula = new Formula(content.Remove(0, 1), this.Normalize, this.IsValid);
                return SetCellContents(name, formula);
            }
            else //content is a string
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
        /// <param name="name"></param>
        /// <param name="number"></param>
        /// <returns></returns>
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
        /// <param name="name"></param>
        /// <param name="text"></param>
        /// <returns></returns>
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
        /// <param name="name"></param>
        /// <param name="formula"></param>
        /// <returns></returns>
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
        /// <param name="name"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public override string GetSavedVersion(string filename)
        {
            try
            {
                //Using will close the reader if an exception is found. Otherwise if the file doesn't exist,
                // multiple try catch blocks are needed anyway.
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement("spreadsheet"))
                            return reader.GetAttribute(0);
                    }
                }

            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("Could not get version. Check filename/file.");
            }

            //If an XML file that doesn't follow this format/doesn't have a version is read, throw an exception
            throw new SpreadsheetReadWriteException("Could not get version, check filename/file.");
        }

        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>
        /// cell name goes here
        /// </name>
        /// <contents>
        /// cell contents goes here
        /// </contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        /// <param name="filename"></param>
        public override void Save(string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            XmlWriter writer = XmlWriter.Create(filename, settings);
            try
            {
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");

                    writer.WriteStartAttribute("version");
                    writer.WriteString(this.Version);
                    writer.WriteEndAttribute();

                    foreach (String name in GetNamesOfAllNonemptyCells())
                    {
                        cells[name].WriteXML(name, writer);
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Close();
                }
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("While attempting to save the file," +
                    " a problem occured. Check the name and location.");
            }


            p_changed = false;
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object GetCellValue(string name)
        {
            name = VerifyName(name);
            if (cells.ContainsKey(name))
            {
                //Data protected through cell property
                return cells[name].Value;
            }
            else
            {
                //Return an empty string as a cell that doesn't exist has both the contents of
                //and empty string and a value of an empty string.
                return "";
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
            HashSet<String> oldDependees = RecalculateDependecies(name, contents);

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

            //Recalculates all of the values that depend on this cell, except for the current cell
            //This is important if the cell is an empty string and doesn't exist in the dictionary
            foreach (String dependent in allDependentsForCell.Skip(1))
            {
                cells[dependent].CalculateValue(Lookup);
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
        /// Returns the old dependees of the passed cell
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

        /// <summary>
        /// Reads a spreadsheet xml file from the given filename and 
        /// adds every new cell and its contents to the spreadsheet object.
        /// 
        /// Throws an exception if the file cannot be found or another error happens while reading.
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadFile(string filePath)
        {
            try
            {
                //Using allows the reader to be disposed if an exception is thrown.
                //The outer try catch allows an exception to be caught (such as a FileNotFoundException, which
                // using or a single try/catch does not allow, as the reader cannot be disposed of for a bad filename)
                using (XmlReader reader = XmlReader.Create(filePath))
                {
                    String name = null;
                    String contents = null;

                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "name":
                                    reader.Read();
                                    name = reader.Value;
                                    break;
                                case "contents":
                                    reader.Read();
                                    contents = reader.Value;
                                    break;
                            }
                        }

                        if (name != null && contents != null)
                        {
                            SetContentsOfCell(name, contents);
                            name = null;
                            contents = null;
                        }
                    }

                    reader.Close();
                }
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("File cannot be properly read");
            }
        }

        /// <summary>
        /// Will return the value of the cell at the given index.
        /// Throws an exception if the value is not a double
        /// 
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        private double Lookup(String var)
        {
            Object cellValue = GetCellValue(var);
            if (cellValue is double)
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

            /// <summary>
            /// Calculates the value of this cell object given the current 
            /// contents of the cell.
            /// </summary>
            /// <param name="lookup"></param>
            public void CalculateValue(Func<String, double> lookup)
            {
                if (p_contents is Formula form)
                {
                    form = (Formula)p_contents;
                    p_value = form.Evaluate(lookup);
                }
                else
                    p_value = p_contents;
            }

            /// <summary>
            /// Writes this cell object to the given XmlWriter in the following format:
            /// <cell>
            ///     <name>
            ///     cell name goes here
            ///     </name>
            ///     
            ///     <contents>
            ///     cell contents goes here
            ///     </contents>    
            /// </cell>
            /// 
            /// </summary>
            /// <param name="name"></param>
            /// <param name="writer"></param>
            public void WriteXML(string name, XmlWriter writer)
            {
                writer.WriteStartElement("cell");
                writer.WriteElementString("name", name);
                if (p_contents is Formula)
                    writer.WriteElementString("contents", "=" + p_contents.ToString());
                else
                    writer.WriteElementString("contents", p_contents.ToString());

                writer.WriteEndElement();
            }
        }
    }
}

