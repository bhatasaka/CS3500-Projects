using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Creates a spreadsheet object containing all empty cells.
        /// </summary>
        public Spreadsheet()
        {
            dependencies = new DependencyGraph();
            cells = new Dictionary<String,Cell>();
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        public override object GetCellContents(string name)
        {
            VerifyName(name);
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
        public override ISet<string> SetCellContents(string name, double number)
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
        public override ISet<string> SetCellContents(string name, string text)
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
        public override ISet<string> SetCellContents(string name, Formula formula)
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
            VerifyName(name);
            return dependencies.GetDependents(name);
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
        private static void VerifyName(string name)
        {
            if (name == null)
                throw new InvalidNameException();
            char letter = name[0];
            //Checking the first character to be an _ or a letter
            if (!letter.Equals('_') && !Char.IsLetter(letter))
                throw new InvalidNameException();

            // Traversing through the string to check that the rest of the characters are
            // letters, numbers or underscores
            for(int letterPos = 1; letterPos < name.Length; letterPos++)
            {
                letter = name[letterPos];
                if (!letter.Equals('_') && !Char.IsLetter(letter) && !Char.IsNumber(letter))
                    throw new InvalidNameException();
            }
        }

        /// <summary>
        /// Helper method for the SetCellContents methods.
        /// Creates and adds valid cells to the object dictionary.
        /// Creates, adds and removes dependecies as needed.
        /// 
        /// Throws an InvalidNameException if the name is invalid or null
        /// Throws an ArgumentNullException if the contents are null
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        private HashSet<String> HandleSetCell(string name, Object contents)
        {
            //Throws an InvalidNameException if the name is not valid
            VerifyName(name);
            if (contents == null)
                throw new ArgumentNullException();

            //Checks if the cell already exists
            if (cells.ContainsKey(name))
            {
                cells.Remove(name); //Remove the cell if it exists already
            }
            RecalculateDependecies(name, contents);

            //Makes a new HashSet of all of the cells that will be affected by changing this cell
            // plus this cell.
            //Throws a CircularException if there is a circular dependency.
            HashSet<string> allDependents = new HashSet<string>(GetCellsToRecalculate(name).ToArray<string>())
            {
                name
            };

            //If the cell doesn't contain an empty string, add the new cell to the set
            // (otherwise it stays removed from the dictionary)
            if (!contents.Equals(""))
            {
                Cell cell = new Cell(contents);
                cells.Add(name, cell);
            }

            return allDependents;
        }

        /// <summary>
        /// Recaculates dependencies of the passed cell parameters.
        /// Will add and remove as needed.
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        private void RecalculateDependecies(string name, Object contents)
        {
            if(contents is Formula)
            {
                Formula formula = (Formula)contents;
                //All of the variables in the current formula (aka new dependees)
                HashSet<String> variables = new HashSet<String>(formula.GetVariables());

                //Replace the cell's old dependees with the variables it contains
                dependencies.ReplaceDependees(name, variables);
            }

            //If the contents isn't a formula, remove this cell from the dependents of
            //all of this cell's old dependees
            else
            {
                dependencies.ReplaceDependees(name, new HashSet<String>());
            }
        }

        /// <summary>
        /// A cell is an object which contains an object that may be
        /// a double, a string or a Formula.
        /// </summary>
        private class Cell
        {
            private object p_contents;

            /// <summary>
            /// Creates a cell with the passed contents.
            /// There is an invariant that say a cell will only have contents
            /// that is a double, string or Formula.
            /// </summary>
            /// <param name="contents"></param>
            public Cell(Object contents)
            {
                p_contents = contents;
            }

            /// <summary>
            /// The contents of the cell object
            /// </summary>
            public object Contents
            {
                get { return p_contents; }
            }

        }
    }
}

