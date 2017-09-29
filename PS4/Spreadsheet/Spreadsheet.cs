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
            cells.Add(new Cell(name, number, false));

            HashSet<string> dependents = GetCellsToRecalculate(name).ToArray();
            return GetCellsToRecalculate(name);
        }

        public override ISet<string> SetCellContents(string name, string text)
        {
            throw new NotImplementedException();
        }

        public override ISet<string> SetCellContents(string name, Formula formula)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            if (name == null)
                throw new InvalidNameException();
            verifyName(name);

            return graph.GetDependents(name);
        }

        private static bool verifyName(string name)
        {
            throw new NotImplementedException();
        }

        private class Cell
        {
            object p_contents;
            bool p_isString;

            object Contents
            {
                get
                { return p_contents; }
            }

            bool isString
            {
                get { return p_isString; }
            }

            public Cell(String name, Object contents, bool isString)
            {
                p_contents = contents;
                this.p_isString = isString;
            }
        }
    }
}

