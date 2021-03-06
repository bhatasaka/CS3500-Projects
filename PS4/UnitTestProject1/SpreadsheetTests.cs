﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using SpreadsheetUtilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
/// <summary>
/// Contains the unit tests for the spreadsheet object in the SS namespace.
/// PS5 Branch.
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

            sheet.SetContentsOfCell("_A_4_", "1.1");
        }


        [TestMethod]
        public void testSetCellSimpleText()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("_A_4_", "hello");
        }

        [TestMethod]
        public void testSetCellSimpleFormula()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("_A_4_", "1 + 6");
        }

        [TestMethod]
        public void testOverwriteCell()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("A4", "23");

            Formula form = new Formula("1 + 6");
            sheet.SetContentsOfCell("A4", "=1 + 6");

            object contents = sheet.GetCellContents("A4");
            Assert.IsTrue(contents is Formula);
            Assert.IsTrue(form.Equals((Formula)contents));
        }

        [TestMethod]
        public void testSetCellReturnSimpleSingle()
        {
            Spreadsheet sheet = new Spreadsheet();

            HashSet<String> allDependents = new HashSet<String>(sheet.SetContentsOfCell("A2", "5"));
            HashSet<String> expected = new HashSet<string>() { "A2" };
            Assert.IsTrue(expected.SetEquals(allDependents));
        }

        [TestMethod]
        public void testSetCellReturnSimpleTwo()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("A1", "=A2 + 6");
            HashSet<String> allDependents = new HashSet<String>(sheet.SetContentsOfCell("A2", "5"));
            HashSet<String> expected = new HashSet<string>() { "A1", "A2" };

            Assert.IsTrue(expected.SetEquals(allDependents));
        }

        [TestMethod]
        public void testSetCellReturnIndirect()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("B1", "6");
            sheet.SetContentsOfCell("A1", "=A2 + 6");
            sheet.SetContentsOfCell("A3", "=A2 + 5");

            sheet.SetContentsOfCell("A4", "=A2 + 4");
            sheet.SetContentsOfCell("A4", "=B1 + 4");

            sheet.SetContentsOfCell("A5", "=A3 + 3");

            HashSet<String> allDependents = new HashSet<String>(sheet.SetContentsOfCell("A2", "5"));
            HashSet<String> expected = new HashSet<string>() { "A1", "A2", "A3", "A5" };

            Assert.IsTrue(expected.SetEquals(allDependents));
        }

        //Tests that are expected to throw exceptions
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void testSetCellSimpleDoubleBadName()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("46A", "1.1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void testSetCellSimpleTextBadName()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("*letters4", "hello");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void testSetCellSimpleTextBadName2()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("_A*", "hello");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void testSetCellSimpleFormulaBadName()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("4_A", "1 + 6");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void testSetCellSimpleTextNullName()
        {
            Spreadsheet sheet = new Spreadsheet();

            String name = null;
            sheet.SetContentsOfCell(name, "hello");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void testSetCellSimpleTextNullContents()
        {
            Spreadsheet sheet = new Spreadsheet();
            String text = null;

            sheet.SetContentsOfCell("A46", text);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void testSetCellSimpleFormulaNullContents()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("A_4", null);
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void testSetCellSimpleCircularException()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("A1", "=A2 - 6");
            sheet.SetContentsOfCell("A2", "=A1 + 6");
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void testSetCellComplexCircularException()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("A1", "=A2 - 6");
            sheet.SetContentsOfCell("A2", "4");
            try
            {
                sheet.SetContentsOfCell("A2", "=A1 + 6");
            }
            catch (CircularException)
            {
                Assert.AreEqual(4, (Double)sheet.GetCellContents("A2"));
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void testSetCellSimpleCircularExceptionItself()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("A1", "=A1 - 6");
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void testSetCellSimpleCircularExceptionIndirect()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.SetContentsOfCell("A1", "=A2 - 6");
            sheet.SetContentsOfCell("A2", "=A3 + 6");
            sheet.SetContentsOfCell("A3", "=A4 + 6");
            sheet.SetContentsOfCell("A4", "=A1 + 6");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void testSetCellSimpleFormulaNullName()
        {
            Spreadsheet sheet = new Spreadsheet();

            String name = null;
            sheet.SetContentsOfCell(name, "1");
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

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void testGetSavedVersionBadFilename()
        {
            Spreadsheet sheet = new Spreadsheet();

            sheet.GetSavedVersion("thisisnotarealfile");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void testGetSavedVersionBadFile()
        {
            using(XmlWriter writer = XmlWriter.Create("testBadXmlFile.xml"))
            {
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("badElement");
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Close();
                }
            }

            Spreadsheet sheet = new Spreadsheet();

            sheet.GetSavedVersion("testBadXmlFile.xml");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void testConstructorBadFilename()
        {
            Spreadsheet sheet = new Spreadsheet("thisisnotarealfile", s => true, s => s, "default");
        }

        //Testing other methods
        [TestMethod]
        public void testGetCellContentsDouble()
        {
            Spreadsheet sheet = new Spreadsheet();


            sheet.SetContentsOfCell("_A_4_", "1.1");
            object contents = sheet.GetCellContents("_A_4_");

            Assert.IsTrue(contents is double);
            Assert.AreEqual(1.1, (double)contents);
        }

        [TestMethod]
        public void testGetCellContentsText()
        {
            Spreadsheet sheet = new Spreadsheet();


            sheet.SetContentsOfCell("A4", "text");
            object contents = sheet.GetCellContents("A4");

            Assert.IsTrue(contents is string);
            Assert.AreEqual("text", (string)contents);
        }

        [TestMethod]
        public void testGetCellContentsFormula()
        {
            Spreadsheet sheet = new Spreadsheet();

            Formula form = new Formula("1 + 6");
            sheet.SetContentsOfCell("A4", "=1 + 6");
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

            sheet.SetContentsOfCell("A4", "=1 + 6");
            sheet.SetContentsOfCell("A4", "");

            Assert.IsTrue(new HashSet<String>().SetEquals(new HashSet<String>(sheet.GetNamesOfAllNonemptyCells())));
        }

        [TestMethod]
        public void testGetDirectDependentsEmpty()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A4", "1");
            sheet.SetContentsOfCell("A5", "2.2");

            PrivateObject sheetAccessor = new PrivateObject(sheet);
            object result = sheetAccessor.Invoke("GetDirectDependents", new String[1] { "A4" });
            Assert.IsTrue(result is IEnumerable<String>);
            Assert.IsTrue(new HashSet<String>().SetEquals(new HashSet<String>((IEnumerable<String>)result)));
        }

        [TestMethod]
        public void testGetDirectDependentsSimple()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A4", "1");
            sheet.SetContentsOfCell("A5", "=A4 - 6");

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
            sheet.SetContentsOfCell("A4", "1");
            sheet.SetContentsOfCell("A5", "A4 - 6");
            sheet.SetContentsOfCell("A5", "1.2");

            PrivateObject sheetAccessor = new PrivateObject(sheet);
            object result = sheetAccessor.Invoke("GetDirectDependents", new String[1] { "A4" });

            HashSet<String> expected = new HashSet<string>() { };
            Assert.IsTrue(result is IEnumerable<String>);
            Assert.IsTrue(expected.SetEquals(new HashSet<String>((IEnumerable<String>)result)));
        }

        [TestMethod]
        public void testGetDirectDependentsAddThenRemoveText()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A4", "1");
            sheet.SetContentsOfCell("A5", "A4 - 6");
            sheet.SetContentsOfCell("A5", "hi");

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
            sheet.SetContentsOfCell("A4", "1");
            sheet.SetContentsOfCell("A5", "=A4 - 6");
            sheet.SetContentsOfCell("A5", "");

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
            sheet.SetContentsOfCell("A4", "1");
            sheet.SetContentsOfCell("A6", "4");
            sheet.SetContentsOfCell("A5", "=A4 - 6");
            sheet.SetContentsOfCell("A5", "=A6 + 2");

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
            sheet.SetContentsOfCell("A4", "1");
            sheet.SetContentsOfCell("A6", "4");
            sheet.SetContentsOfCell("A5", "=A4 - 6");
            sheet.SetContentsOfCell("A5", "=A6 + 2");
            sheet.SetContentsOfCell("A7", "=A4 + 1");
            sheet.SetContentsOfCell("A7", "=A5 + 2");
            sheet.SetContentsOfCell("A7", "=A5 + 6");

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
        public void testSaveFormat()
        {
            Spreadsheet sheet = new Spreadsheet(s => true, s => s, "default");
            sheet.SetContentsOfCell("A1", "5");
            sheet.SetContentsOfCell("A2", "=1-A1 + 22");

            sheet.Save("testSheet.xml");
        }

        [TestMethod]
        public void testConstructorLoadFile()
        {
            Spreadsheet sheet = new Spreadsheet(s => true, s => s, "default");
            sheet.SetContentsOfCell("A1", "5");
            sheet.SetContentsOfCell("A2", "=1-A1 + 22");

            sheet.Save("testSheet.xml");

            Spreadsheet sheet1 = new Spreadsheet("testSheet.xml", testSheets => true, s => s, "default");
            Assert.IsTrue(new HashSet<String> { "A1", "A2" }.SetEquals(sheet1.GetNamesOfAllNonemptyCells()));
        }

        [TestMethod]
        public void testConstructorSetVersion()
        {
            Spreadsheet sheet = new Spreadsheet(s => true, s => s, "20.9");

            Assert.AreEqual("20.9", sheet.Version);
        }

        //[TestMethod]
        //public void testPrivateLoadFile()
        //{
        //    Spreadsheet sheet = new Spreadsheet();
        //    PrivateObject sheetAcessor = new PrivateObject(sheet);

        //    sheetAcessor.Invoke("LoadFile", new String[1] { "testSheet.XML" });

        //    Assert.AreEqual(5, (Double)sheet.GetCellContents("A1"));
        //}

        [TestMethod]
        public void testGetSavedVersion()
        {
            Spreadsheet sheet = new Spreadsheet();
            //Test will fail if Save fails
            sheet.Save("testSheet.xml");

            Assert.AreEqual("default", sheet.GetSavedVersion("testSheet.xml"));
        }

        [TestMethod]
        public void testGetValueSimpleAllThreeTypes()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "1");
            sheet.SetContentsOfCell("A2", "=4 - 6");
            sheet.SetContentsOfCell("A3", "Hello!");

            Assert.AreEqual(1, (Double)sheet.GetCellValue("A1"));
            Assert.AreEqual(-2, (Double)sheet.GetCellValue("A2"));
            Assert.AreEqual("Hello!", (String)sheet.GetCellValue("A3"));
        }

        [TestMethod]
        public void testGetValueSimpleDependentFormula()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "1");
            sheet.SetContentsOfCell("A2", "=A1 - 6");

            Assert.AreEqual(-5, (Double)sheet.GetCellValue("A2"));
        }

        [TestMethod]
        public void testGetValueChangedFormula()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "1");
            sheet.SetContentsOfCell("A2", "=A1 - 1");
            sheet.SetContentsOfCell("A5", "=A4 - 1");
            sheet.SetContentsOfCell("A4", "=A3 - 1");
            sheet.SetContentsOfCell("A3", "=A2 - 1");

            sheet.SetContentsOfCell("A1", "20");

            Assert.AreEqual(19, (Double)sheet.GetCellValue("A2"));
        }

        [TestMethod]
        public void testGetValueSimpleDependentFormulaError()
        {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A2", "=A1 - 6");

            Assert.IsTrue(sheet.GetCellValue("A2") is FormulaError);

            sheet.SetContentsOfCell("A1", "1");

            Assert.AreEqual(-5, (Double)sheet.GetCellValue("A2"));
        }

        [TestMethod]
        public void testChanged()
        {
            Spreadsheet sheet = new Spreadsheet();
            Assert.IsFalse(sheet.Changed);

            sheet.SetContentsOfCell("A1", "1");
            sheet.SetContentsOfCell("A2", "=A1 - 6");

            Assert.IsTrue(sheet.Changed);
            sheet.Save("testSheetChanged");

            Assert.IsFalse(sheet.Changed);
        }

        [TestMethod]
        public void stressTest1()
        {
            Spreadsheet sheet = new Spreadsheet();
            int SIZE = 2000;
            HashSet<String> cellNames = new HashSet<String>();

            for (int i = 0; i < SIZE; i++)
            {
                sheet.SetContentsOfCell("A" + i, i.ToString());
                sheet.SetContentsOfCell("B" + i, "Number " + i);
                sheet.SetContentsOfCell("C" + i, "=A" + i + " + 1");

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

        [TestMethod]
        public void stressTest2ManyDependeCells()
        {
            Spreadsheet sheet = new Spreadsheet();
            int SIZE = 10;

            int nextCell;
            for (int i = 0; i < SIZE; i++)
            {
                nextCell = i + 1;
                sheet.SetContentsOfCell("A" + i, "=A" + nextCell + " + 1");
            }
            sheet.SetContentsOfCell("A" + SIZE, "1");

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Assert.AreEqual((Double)SIZE, sheet.GetCellValue("A1"));
            watch.Stop();
            TimeSpan elapsed = watch.Elapsed;
            Assert.IsTrue(elapsed.Seconds < 10);
        }
    }
}