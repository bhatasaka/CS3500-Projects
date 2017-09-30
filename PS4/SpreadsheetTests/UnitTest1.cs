using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;

namespace SpreadsheetTests
{
    [TestClass]
    public class UnitTest1
    {
        //Basic tests - testing in the order they are needed
        //Set cell tests
        [TestMethod]
        public void TestMethod1()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("double1", 1.1);
        }
    }
}
