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
        private DependencyGraph graph;
        private HashSet<Cell> cells;

        public Spreadsheet()
        {
            graph = new DependencyGraph();
            cells = new HashSet<Cell>();
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
            return HandleSetCell(name, formula);
        }

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            verifyName(name);

            return graph.GetDependents(name);
        }

        private static bool verifyName(string name)
        {
            if (name == null)
                throw new ArgumentNullException();
            throw new NotImplementedException();
        }

        private HashSet<String> HandleSetCell(string name, Object contents)
        {
            //Throws an InvalidNameException if the name is not valid
            verifyName(name);
            if (contents == null)
                throw new ArgumentNullException();

            //Throws a CircularException if there is a circular dependency
            HashSet<string> allDependents = new HashSet<string>(GetCellsToRecalculate(name));
            allDependents.Add(name);

            Cell cell = new Cell(name, contents);

            //Checks if the cell already exists
            if (cells.Contains(cell))
                cells.Remove(cell); //Remove the cell if it exists already
            //If the cell doesn't contain an empty string, add the new cell to the set
            if (!cell.Contents.Equals(""))
                cells.Add(cell);

            return allDependents;
        }

        private class Cell
        {
            private object p_contents;
            private bool p_isString;
            private string p_name;

            public Cell(String name, Object contents)
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

            public string Name
            {
                get { return p_name;  }
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
        }
    }
}

