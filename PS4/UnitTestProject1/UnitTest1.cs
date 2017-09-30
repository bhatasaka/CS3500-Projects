using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using SpreadsheetUtilities;

namespace SpreadsheetTests
{
    [TestClass]
    public class UnitTest1
    {
        //Basic tests - testing in the order they are needed
        //Set cell tests
        [TestMethod]
        public void SetCellSimpleDouble()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("_A_4_", 1.1);
        }

        [TestMethod]
        public void SetCellSimpleText()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("_A_4_", "hello");
        }

        [TestMethod]
        public void SetCellSimpleFormula()
        {
            Spreadsheet sheet = new Spreadsheet();

            Formula form = new Formula("1 + 6");
            sheet.SetCellContents("_A_4_", form);
        }

        //Tests that are expected to throw exceptions
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellSimpleDoubleBadName()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("46A", 1.1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellSimpleTextBadName()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("*letters4", "hello");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellSimpleFormulaBadName()
        {
            Spreadsheet sheet = new Spreadsheet();

            Formula form = new Formula("1 + 6");
            sheet.SetCellContents("4_A", form);
        }
    }
}