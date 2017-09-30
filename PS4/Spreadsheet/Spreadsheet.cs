using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS
{
    class Spreadsheet : AbstractSpreadsheet
    {
        private DependencyGraph dependencies;
        private Dictionary<String, Cell> cells;

        public Spreadsheet()
        {
            dependencies = new DependencyGraph();
            cells = new Dictionary<String,Cell>();
        }

        public override object GetCellContents(string name)
        {
            //Check and make sure is Formula is a thing
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            throw new NotImplementedException();
        }

        public override ISet<string> SetCellContents(string name, double number)
        {
            return HandleSetCell(name, number);
        }

        public override ISet<string> SetCellContents(string name, string text)
        {
            return HandleSetCell(name, text);
        }

        public override ISet<string> SetCellContents(string name, Formula formula)
        {
            HashSet<String> allDependents = HandleSetCell(name, formula);
            foreach (String variable in formula.GetVariables())
            {
                dependencies.AddDependency(variable, name);
            }
            return allDependents;
        }

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            VerifyName(name);
            return dependencies.GetDependents(name);
        }

        private static bool VerifyName(string name)
        {
            if (name == null)
                throw new ArgumentNullException();
            char letter = name[0];
            //Checking the first character to be an _ or a letter
            if (letter != '_' || !Char.IsLetter(letter))
                return false;

            // Traversing through the string to check that the rest of the characters are
            // letters, numbers or underscores
            for(int letterPos = 1; letterPos < name.Length; letterPos++)
            {
                letter = name[letterPos];
                if (letter != '_' && !Char.IsLetter(letter) && !Char.IsNumber(letter))
                    return false;
            }

            return true;
        }

        private HashSet<String> HandleSetCell(string name, Object contents)
        {
            //Throws an InvalidNameException if the name is not valid
            VerifyName(name);
            if (contents == null)
                throw new ArgumentNullException();

            //Makes a new HashSet of all of the cells that will be affected by changing this cell
            // plus this cell.
            //Throws a CircularException if there is a circular dependency.
            HashSet<string> allDependents = new HashSet<string>(GetCellsToRecalculate(name))
            {
                name
            };

            //Checks if the cell already exists
            if (cells.ContainsKey(name))
                cells.Remove(name); //Remove the cell if it exists already

            //If the cell doesn't contain an empty string, add the new cell to the set
            if (!contents.Equals(""))
            {
                Cell cell = new Cell(contents);
                cells.Add(name, cell);
            }

            return allDependents;
        }

        private class Cell
        {
            private object p_contents;
            private bool p_isString;

            public Cell(Object contents)
            {
                p_contents = contents;
                if (contents is String)
                    p_isString = true;
            }
            public object Contents
            {
                get { return p_contents; }
            }

            public bool IsString
            {
                get { return p_isString; }
            }
        }
    }
}

