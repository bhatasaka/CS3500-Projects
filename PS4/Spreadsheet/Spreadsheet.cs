using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
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
            VerifyName(name);
            if (!cells.ContainsKey(name))
                return "";
            Cell cell = cells[name];
            return cell.Contents;
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return cells.Keys.ToArray<String>();
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

            return allDependents;
        }

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

        private void RecalculateDependecies(string name, Object contents)
        {
            if(contents is Formula)
            {
                Formula formula = (Formula)contents;
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

        private class Cell
        {
            private object p_contents;

            public Cell(Object contents)
            {
                p_contents = contents;
            }
            public object Contents
            {
                get { return p_contents; }
            }

        }
    }
}

