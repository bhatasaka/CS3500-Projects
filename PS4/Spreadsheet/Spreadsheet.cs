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
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            throw new NotImplementedException();
        }

        public override ISet<string> SetCellContents(string name, double number)
        {
            //Throws an InvalidNameException if the name is not valid
            verifyName(name);
            Cell cell = new Cell(name, number);
            HandleCells(cell);

            return allDependentsSet(name);
        }

        public override ISet<string> SetCellContents(string name, string text)
        {
            verifyName(name);
            Cell cell = new Cell(name, text);
            HandleCells(cell);

            return allDependentsSet(name);
        }

        public override ISet<string> SetCellContents(string name, Formula formula)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            if (name == null)
                throw new ArgumentNullException();
            verifyName(name);

            return graph.GetDependents(name);
        }

        private static bool verifyName(string name)
        {
            throw new NotImplementedException();
        }

        private void HandleCells(Cell cell)
        {
            if (cells.Contains(cell))
            {
                cells.Remove(cell);
                if (cell.Name.Equals(""))
                    return;
            }
            cells.Add(cell);
        }

        private HashSet<String> allDependentsSet(string name)
        {
           return new HashSet<string>(GetCellsToRecalculate(name));
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
                get
                { return p_contents; }
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

