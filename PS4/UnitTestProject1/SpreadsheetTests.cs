using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using SpreadsheetUtilities;
using System.Collections.Generic;
/// <summary>
/// Contains the unit tests for the spreadsheet object in the SS namespace.
/// Author: Bryan Hatasaka u1028471
/// </summary>
namespace SpreadsheetTests
{
    [TestClass]
    public class UnitTest1
    {
        //Set cell tests
        [TestMethod]
        public void testSetCellSimpleDouble()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("_A_4_", 1.1);
        }

        [TestMethod]
        public void testSetCellSimpleText()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("_A_4_", "hello");
        }

        [TestMethod]
        public void testSetCellSimpleFormula()
        {
            Spreadsheet sheet = new Spreadsheet();

            Formula form = new Formula("1 + 6");
            sheet.SetCellContents("_A_4_", form);
        }

        [TestMethod]
        public void testOverwriteCell()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("A4", 23);

            Formula form = new Formula("1 + 6");
            sheet.SetCellContents("A4", form);

            object contents = sheet.GetCellContents("A4");
            Assert.IsTrue(contents is Formula);
            Assert.IsTrue(form.Equals((Formula)contents));
        }

        [TestMethod]
        public void testSetCellReturnSimpleSingle()
        {
            Spreadsheet sheet = new Spreadsheet();

            HashSet<String> allDependents = new HashSet<String>(sheet.SetCellContents("A2", 5));
            HashSet<String> expected = new HashSet<string>() { "A2" };
            Assert.IsTrue(expected.SetEquals(allDependents));
        }

        [TestMethod]
        public void testSetCellReturnSimpleTwo()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("A1", new Formula("A2 + 6"));
            HashSet<String> allDependents = new HashSet<String>(sheet.SetCellContents("A2", 5));
            HashSet<String> expected = new HashSet<string>() { "A1", "A2" };

            Assert.IsTrue(expected.SetEquals(allDependents));
        }

        [TestMethod]
        public void testSetCellReturnIndirect()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("B1", 6);
            sheet.SetCellContents("A1", new Formula("A2 + 6"));
            sheet.SetCellContents("A3", new Formula("A2 + 5"));

            sheet.SetCellContents("A4", new Formula("A2 + 4"));
            sheet.SetCellContents("A4", new Formula("B1 + 4"));

            sheet.SetCellContents("A5", new Formula("A3 + 3"));

            HashSet<String> allDependents = new HashSet<String>(sheet.SetCellContents("A2", 5));
            HashSet<String> expected = new HashSet<string>() { "A1", "A2", "A3", "A5" };

            Assert.IsTrue(expected.SetEquals(allDependents));
        }

        //Tests that are expected to throw exceptions
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void testSetCellSimpleDoubleBadName()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("46A", 1.1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void testSetCellSimpleTextBadName()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("*letters4", "hello");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void testSetCellSimpleTextBadName2()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("_A*", "hello");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void testSetCellSimpleFormulaBadName()
        {
            Spreadsheet sheet = new Spreadsheet();

            Formula form = new Formula("1 + 6");
            sheet.SetCellContents("4_A", form);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void testSetCellSimpleTextNullName()
        {
            Spreadsheet sheet = new Spreadsheet();

            String name = null;
            sheet.SetCellContents(name, "hello");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void testSetCellSimpleTextNullContents()
        {
            Spreadsheet sheet = new Spreadsheet();
            String text = null;

            sheet.SetCellContents("A46", text);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void testSetCellSimpleFormulaNullContents()
        {
            Spreadsheet sheet = new Spreadsheet();

            Formula form = null;
            sheet.SetCellContents("A_4", form);
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void testSetCellSimpleCircularException()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("A1", new Formula("A2 - 6"));
            sheet.SetCellContents("A2", new Formula("A1 + 6"));
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void testSetCellSimpleCircularExceptionItself()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("A1", new Formula("A1 - 6"));
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void testSetCellSimpleCircularExceptionIndirect()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetCellContents("A1", new Formula("A2 - 6"));
            sheet.SetCellContents("A2", new Formula("A3 + 6"));
            sheet.SetCellContents("A3", new Formula("A4 + 6"));
            sheet.SetCellContents("A4", new Formula("A1 + 6"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void testSetCellSimpleFormulaNullName()
        {
            Spreadsheet sheet = new Spreadsheet();

            String name = null;
            sheet.SetCellContents(name, 1);
        }

        [TestMethod]
        public void testGetDirectDependentsNullName()
        {
            Spreadsheet sheet = new Spreadsheet();
            String name = null;

            PrivateObject sheetAccessor = new PrivateObject(sheet);
            try
            {
            object result = sheetAccessor.Invoke("GetDirectDependents", new String[1] { name });
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                Assert.IsInstanceOfType(e.InnerException, typeof(ArgumentNullException));
            }
        }


        //Testing other methods
        [TestMethod]
        public void testGetCellContentsDouble()
        {
            Spreadsheet sheet = new Spreadsheet();


            sheet.SetCellContents("_A_4_", 1.1);
            object contents = sheet.GetCellContents("_A_4_");

            Assert.IsTrue(contents is double);
            Assert.AreEqual(1.1, (double)contents);
        }

        [TestMethod]
        public void testGetCellContentsText()
        {
            Spreadsheet sheet = new Spreadsheet();


            sheet.SetCellContents("A4", "text");
            object contents = sheet.GetCellContents("A4");

            Assert.IsTrue(contents is string);
            Assert.AreEqual("text", (string)contents);
        }

        [TestMethod]
        public void testGetCellContentsFormula()
        {
            Spreadsheet sheet = new Spreadsheet();

            Formula form = new Formula("1 + 6");
            sheet.SetCellContents("A4", form);
            object contents = sheet.GetCellContents("A4");
            Assert.IsTrue(contents is Formula);
            Assert.AreEqual(form, (Formula)contents);
        }

        [TestMethod]
        public void testGetCellContentsEmpty()
        {
            Spreadsheet sheet = new Spreadsheet();

            object contents = sheet.GetCellContents("A4");
            Assert.IsTrue(contents is String);
            Assert.AreEqual("", contents);
        }

        [TestMethod]
        public void testGetNamesOfAllNonemptyCellsEmpty()
        {
            Spreadsheet sheet = new Spreadsheet();

            Assert.IsTrue(new HashSet<String>().SetEquals(new HashSet<String>(sheet.GetNamesOfAllNonemptyCells())));
        }

        [TestMethod]
        public void testGetNamesOfAllNonemptyCellsAddThenRemove()
        {
            Spreadsheet sheet = new Spreadsheet();

            Formula form = new Formula("1 + 6");
            sheet.SetCellContents("A4", form);
            sheet.SetCellContents("A4", "");

            Assert.IsTrue(new HashSet<String>().SetEquals(new HashSet<String>(sheet.GetNamesOfAllNonemptyCells())));
        }

        [TestMethod]
        public void testGetDirectDependentsEmpty()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("A4", 1);
            sheet.SetCellContents("A5", 2.2);

            PrivateObject sheetAccessor = new PrivateObject(sheet);
            object result = sheetAccessor.Invoke("GetDirectDependents", new String[1] {"A4"});
            Assert.IsTrue(result is IEnumerable<String>);
            Assert.IsTrue(new HashSet<String>().SetEquals(new HashSet<String>((IEnumerable<String>)result)));
        }

        [TestMethod]
        public void testGetDirectDependentsSimple()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("A4", 1);
            sheet.SetCellContents("A5", new Formula("A4 - 6"));

            PrivateObject sheetAccessor = new PrivateObject(sheet);
            object result = sheetAccessor.Invoke("GetDirectDependents", new String[1] { "A4" });

            HashSet<String> expected = new HashSet<string>() { "A5" };
            Assert.IsTrue(result is IEnumerable<String>);
            Assert.IsTrue(expected.SetEquals(new HashSet<String>((IEnumerable<String>)result)));
        }

        [TestMethod]
        public void testGetDirectDependentsAddThenRemoveDouble()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("A4", 1);
            sheet.SetCellContents("A5", new Formula("A4 - 6"));
            sheet.SetCellContents("A5", 1.2);

            PrivateObject sheetAccessor = new PrivateObject(sheet);
            object result = sheetAccessor.Invoke("GetDirectDependents", new String[1] { "A4" });

            HashSet<String> expected = new HashSet<string>() {  };
            Assert.IsTrue(result is IEnumerable<String>);
            Assert.IsTrue(expected.SetEquals(new HashSet<String>((IEnumerable<String>)result)));
        }

        [TestMethod]
        public void testGetDirectDependentsAddThenRemoveText()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("A4", 1);
            sheet.SetCellContents("A5", new Formula("A4 - 6"));
            sheet.SetCellContents("A5", "hi");

            PrivateObject sheetAccessor = new PrivateObject(sheet);
            object result = sheetAccessor.Invoke("GetDirectDependents", new String[1] { "A4" });

            HashSet<String> expected = new HashSet<string>() { };
            Assert.IsTrue(result is IEnumerable<String>);
            Assert.IsTrue(expected.SetEquals(new HashSet<String>((IEnumerable<String>)result)));
        }

        [TestMethod]
        public void testGetDirectDependentsAddThenRemoveEmpty()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("A4", 1);
            sheet.SetCellContents("A5", new Formula("A4 - 6"));
            sheet.SetCellContents("A5", "");

            PrivateObject sheetAccessor = new PrivateObject(sheet);
            object result = sheetAccessor.Invoke("GetDirectDependents", new String[1] { "A4" });

            HashSet<String> expected = new HashSet<string>() { };
            Assert.IsTrue(result is IEnumerable<String>);
            Assert.IsTrue(expected.SetEquals(new HashSet<String>((IEnumerable<String>)result)));
        }


        [TestMethod]
        public void testGetDirectDependentsAddThenRemoveFormula()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("A4", 1);
            sheet.SetCellContents("A6", 4);
            sheet.SetCellContents("A5", new Formula("A4 - 6"));
            sheet.SetCellContents("A5", new Formula("A6 + 2"));

            PrivateObject sheetAccessor = new PrivateObject(sheet);
            object resultA4 = sheetAccessor.Invoke("GetDirectDependents", new String[1] { "A4" });
            object resultA6 = sheetAccessor.Invoke("GetDirectDependents", new String[1] { "A6" });

            HashSet<String> expectedA6 = new HashSet<string>() { "A5" };
            Assert.IsTrue(resultA6 is IEnumerable<String>);
            Assert.IsTrue(expectedA6.SetEquals(new HashSet<String>((IEnumerable<String>)resultA6)));

            HashSet<String> expectedA4 = new HashSet<string>() { };
            Assert.IsTrue(resultA4 is IEnumerable<String>);
            Assert.IsTrue(expectedA4.SetEquals(new HashSet<String>((IEnumerable<String>)resultA4)));
        }

        [TestMethod]
        public void testGetDirectDependentsAddThenRemoveComplex()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("A4", 1);
            sheet.SetCellContents("A6", 4);
            sheet.SetCellContents("A5", new Formula("A4 - 6"));
            sheet.SetCellContents("A5", new Formula("A6 + 2"));
            sheet.SetCellContents("A7", new Formula("A4 + 1"));
            sheet.SetCellContents("A7", new Formula("A5 + 2"));
            sheet.SetCellContents("A7", new Formula("A5 + 6"));

            PrivateObject sheetAccessor = new PrivateObject(sheet);
            object resultA4 = sheetAccessor.Invoke("GetDirectDependents", new String[1] { "A4" });
            object resultA6 = sheetAccessor.Invoke("GetDirectDependents", new String[1] { "A6" });
            object resultA5 = sheetAccessor.Invoke("GetDirectDependents", new String[1] { "A5" });

            HashSet<String> expectedA6 = new HashSet<string>() { "A5" };
            Assert.IsTrue(resultA6 is IEnumerable<String>);
            Assert.IsTrue(expectedA6.SetEquals(new HashSet<String>((IEnumerable<String>)resultA6)));

            HashSet<String> expectedA4 = new HashSet<string>() { };
            Assert.IsTrue(resultA4 is IEnumerable<String>);
            Assert.IsTrue(expectedA4.SetEquals(new HashSet<String>((IEnumerable<String>)resultA4)));

            HashSet<String> expectedA5 = new HashSet<string>() { "A7" };
            Assert.IsTrue(resultA5 is IEnumerable<String>);
            Assert.IsTrue(expectedA5.SetEquals(new HashSet<String>((IEnumerable<String>)resultA5)));
        }

        [TestMethod]
        public void stressTest()
        {
            Spreadsheet sheet = new Spreadsheet();
            int SIZE = 2000;
            HashSet<String> cellNames = new HashSet<String>();

            for(int i = 0; i < SIZE; i++)
            {
                sheet.SetCellContents("A" + i, i);
                sheet.SetCellContents("B" + i, "Number " + i);
                sheet.SetCellContents("C" + i, new Formula("A" + i + " + 1"));

                cellNames.Add("A" + i);
                cellNames.Add("B" + i);
                cellNames.Add("C" + i);
            }

            for (int i = 0; i < SIZE; i++)
            {
                Assert.AreEqual((double)i, sheet.GetCellContents("A" + i));
                Assert.AreEqual("Number " + i, sheet.GetCellContents("B" + i));
                Assert.AreEqual(new Formula("A" + i + " + 1"), sheet.GetCellContents("C" + i));
            }

            Assert.IsTrue(cellNames.SetEquals(new HashSet<String>(sheet.GetNamesOfAllNonemptyCells())));
        }
    }
}