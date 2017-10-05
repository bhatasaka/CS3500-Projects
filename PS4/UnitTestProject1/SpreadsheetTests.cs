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
            object result = sheetAccessor.Invoke("GetDirectDependents", new String[1] { "A4" });
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

            HashSet<String> expected = new HashSet<string>() { };
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

            for (int i = 0; i < SIZE; i++)
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

        //=====================================
        //=========Grading Tests===============
        //=====================================

        // EMPTY SPREADSHEETS
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestEmptyGetNull()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents(null);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestEmptyGetContents()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents("1AA");
        }

        [TestMethod()]
        public void TestGetEmptyContents()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.AreEqual("", s.GetCellContents("A2"));
        }

        // SETTING CELL TO A DOUBLE
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetNullDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents(null, 1.5);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetInvalidNameDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("1A1A", 1.5);
        }

        [TestMethod()]
        public void TestSimpleSetDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("Z7", 1.5);
            Assert.AreEqual(1.5, (double)s.GetCellContents("Z7"), 1e-9);
        }

        // SETTING CELL TO A STRING
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetNullStringVal()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A8", (string)null);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetNullStringName()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents(null, "hello");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetSimpleString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("1AZ", "hello");
        }

        [TestMethod()]
        public void TestSetGetSimpleString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("Z7", "hello");
            Assert.AreEqual("hello", s.GetCellContents("Z7"));
        }

        // SETTING CELL TO A FORMULA
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetNullFormVal()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A8", (Formula)null);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetNullFormName()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents(null, new Formula("2"));
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetSimpleForm()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("1AZ", new Formula("2"));
        }

        [TestMethod()]
        public void TestSetGetForm()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("Z7", new Formula("3"));
            Formula f = (Formula)s.GetCellContents("Z7");
            Assert.AreEqual(new Formula("3"), f);
            Assert.AreNotEqual(new Formula("2"), f);
        }

        // CIRCULAR FORMULA DETECTION
        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void TestSimpleCircular()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", new Formula("A2"));
            s.SetCellContents("A2", new Formula("A1"));
        }

        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void TestComplexCircular()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", new Formula("A2+A3"));
            s.SetCellContents("A3", new Formula("A4+A5"));
            s.SetCellContents("A5", new Formula("A6+A7"));
            s.SetCellContents("A7", new Formula("A1+A1"));
        }

        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void TestUndoCircular()
        {
            Spreadsheet s = new Spreadsheet();
            try
            {
                s.SetCellContents("A1", new Formula("A2+A3"));
                s.SetCellContents("A2", 15);
                s.SetCellContents("A3", 30);
                s.SetCellContents("A2", new Formula("A3*A1"));
            }
            catch (CircularException e)
            {
                Assert.AreEqual(15, (double)s.GetCellContents("A2"), 1e-9);
                throw e;
            }
        }

        // NONEMPTY CELLS
        [TestMethod()]
        public void TestEmptyNames()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void TestExplicitEmptySet()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("B1", "");
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void TestSimpleNamesString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("B1", "hello");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod()]
        public void TestSimpleNamesDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("B1", 52.25);
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod()]
        public void TestSimpleNamesFormula()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("B1", new Formula("3.5"));
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod()]
        public void TestMixedNames()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", 17.2);
            s.SetCellContents("C1", "hello");
            s.SetCellContents("B1", new Formula("3.5"));
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "A1", "B1", "C1" }));
        }

        // RETURN VALUE OF SET CELL CONTENTS
        [TestMethod()]
        public void TestSetSingletonDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("B1", "hello");
            s.SetCellContents("C1", new Formula("5"));
            Assert.IsTrue(s.SetCellContents("A1", 17.2).SetEquals(new HashSet<string>() { "A1" }));
        }

        [TestMethod()]
        public void TestSetSingletonString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", 17.2);
            s.SetCellContents("C1", new Formula("5"));
            Assert.IsTrue(s.SetCellContents("B1", "hello").SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod()]
        public void TestSetSingletonFormula()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", 17.2);
            s.SetCellContents("B1", "hello");
            Assert.IsTrue(s.SetCellContents("C1", new Formula("5")).SetEquals(new HashSet<string>() { "C1" }));
        }

        [TestMethod()]
        public void TestSetChain()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", new Formula("A2+A3"));
            s.SetCellContents("A2", 6);
            s.SetCellContents("A3", new Formula("A2+A4"));
            s.SetCellContents("A4", new Formula("A2+A5"));
            Assert.IsTrue(s.SetCellContents("A5", 82.5).SetEquals(new HashSet<string>() { "A5", "A4", "A3", "A1" }));
        }

        // CHANGING CELLS
        [TestMethod()]
        public void TestChangeFtoD()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", new Formula("A2+A3"));
            s.SetCellContents("A1", 2.5);
            Assert.AreEqual(2.5, (double)s.GetCellContents("A1"), 1e-9);
        }

        [TestMethod()]
        public void TestChangeFtoS()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", new Formula("A2+A3"));
            s.SetCellContents("A1", "Hello");
            Assert.AreEqual("Hello", (string)s.GetCellContents("A1"));
        }

        [TestMethod()]
        public void TestChangeStoF()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", "Hello");
            s.SetCellContents("A1", new Formula("23"));
            Assert.AreEqual(new Formula("23"), (Formula)s.GetCellContents("A1"));
            Assert.AreNotEqual(new Formula("24"), (Formula)s.GetCellContents("A1"));
        }

        // STRESS TESTS
        [TestMethod()]
        public void TestStress1()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", new Formula("B1+B2"));
            s.SetCellContents("B1", new Formula("C1-C2"));
            s.SetCellContents("B2", new Formula("C3*C4"));
            s.SetCellContents("C1", new Formula("D1*D2"));
            s.SetCellContents("C2", new Formula("D3*D4"));
            s.SetCellContents("C3", new Formula("D5*D6"));
            s.SetCellContents("C4", new Formula("D7*D8"));
            s.SetCellContents("D1", new Formula("E1"));
            s.SetCellContents("D2", new Formula("E1"));
            s.SetCellContents("D3", new Formula("E1"));
            s.SetCellContents("D4", new Formula("E1"));
            s.SetCellContents("D5", new Formula("E1"));
            s.SetCellContents("D6", new Formula("E1"));
            s.SetCellContents("D7", new Formula("E1"));
            s.SetCellContents("D8", new Formula("E1"));
            ISet<String> cells = s.SetCellContents("E1", 0);
            Assert.IsTrue(new HashSet<string>() { "A1", "B1", "B2", "C1", "C2", "C3", "C4", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "E1" }.SetEquals(cells));
        }

        // Repeated for extra weight
        [TestMethod()]
        public void TestStress1a()
        {
            TestStress1();
        }
        [TestMethod()]
        public void TestStress1b()
        {
            TestStress1();
        }
        [TestMethod()]
        public void TestStress1c()
        {
            TestStress1();
        }

        [TestMethod()]
        public void TestStress2()
        {
            Spreadsheet s = new Spreadsheet();
            ISet<String> cells = new HashSet<string>();
            for (int i = 1; i < 200; i++)
            {
                cells.Add("A" + i);
                Assert.IsTrue(cells.SetEquals(s.SetCellContents("A" + i, new Formula("A" + (i + 1)))));
            }
        }
        [TestMethod()]
        public void TestStress2a()
        {
            TestStress2();
        }
        [TestMethod()]
        public void TestStress2b()
        {
            TestStress2();
        }
        [TestMethod()]
        public void TestStress2c()
        {
            TestStress2();
        }

        [TestMethod()]
        public void TestStress3()
        {
            Spreadsheet s = new Spreadsheet();
            for (int i = 1; i < 200; i++)
            {
                s.SetCellContents("A" + i, new Formula("A" + (i + 1)));
            }
            try
            {
                s.SetCellContents("A150", new Formula("A50"));
                Assert.Fail();
            }
            catch (CircularException)
            {
            }
        }

        [TestMethod()]
        public void TestStress3a()
        {
            TestStress3();
        }
        [TestMethod()]
        public void TestStress3b()
        {
            TestStress3();
        }
        [TestMethod()]
        public void TestStress3c()
        {
            TestStress3();
        }

        [TestMethod()]
        public void TestStress4()
        {
            Spreadsheet s = new Spreadsheet();
            for (int i = 0; i < 500; i++)
            {
                s.SetCellContents("A1" + i, new Formula("A1" + (i + 1)));
            }
            HashSet<string> firstCells = new HashSet<string>();
            HashSet<string> lastCells = new HashSet<string>();
            for (int i = 0; i < 250; i++)
            {
                firstCells.Add("A1" + i);
                lastCells.Add("A1" + (i + 250));
            }
            Assert.IsTrue(s.SetCellContents("A1249", 25.0).SetEquals(firstCells));
            Assert.IsTrue(s.SetCellContents("A1499", 0).SetEquals(lastCells));
        }
        [TestMethod()]
        public void TestStress4a()
        {
            TestStress4();
        }
        [TestMethod()]
        public void TestStress4b()
        {
            TestStress4();
        }
        [TestMethod()]
        public void TestStress4c()
        {
            TestStress4();
        }

        [TestMethod()]
        public void TestStress5()
        {
            RunRandomizedTest(47, 2519);
        }

        [TestMethod()]
        public void TestStress6()
        {
            RunRandomizedTest(48, 2521);
        }

        [TestMethod()]
        public void TestStress7()
        {
            RunRandomizedTest(49, 2526);
        }

        [TestMethod()]
        public void TestStress8()
        {
            RunRandomizedTest(50, 2521);
        }

        /// <summary>
        /// Sets random contents for a random cell 10000 times
        /// </summary>
        /// <param name="seed">Random seed</param>
        /// <param name="size">The known resulting spreadsheet size, given the seed</param>
        public void RunRandomizedTest(int seed, int size)
        {
            Spreadsheet s = new Spreadsheet();
            Random rand = new Random(seed);
            for (int i = 0; i < 10000; i++)
            {
                try
                {
                    switch (rand.Next(3))
                    {
                        case 0:
                            s.SetCellContents(randomName(rand), 3.14);
                            break;
                        case 1:
                            s.SetCellContents(randomName(rand), "hello");
                            break;
                        case 2:
                            s.SetCellContents(randomName(rand), randomFormula(rand));
                            break;
                    }
                }
                catch (CircularException)
                {
                }
            }
            ISet<string> set = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.AreEqual(size, set.Count);
        }

        /// <summary>
        /// Generates a random cell name with a capital letter and number between 1 - 99
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        private String randomName(Random rand)
        {
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(rand.Next(26), 1) + (rand.Next(99) + 1);
        }

        /// <summary>
        /// Generates a random Formula
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        private String randomFormula(Random rand)
        {
            String f = randomName(rand);
            for (int i = 0; i < 10; i++)
            {
                switch (rand.Next(4))
                {
                    case 0:
                        f += "+";
                        break;
                    case 1:
                        f += "-";
                        break;
                    case 2:
                        f += "*";
                        break;
                    case 3:
                        f += "/";
                        break;
                }
                switch (rand.Next(2))
                {
                    case 0:
                        f += 7.2;
                        break;
                    case 1:
                        f += randomName(rand);
                        break;
                }
            }
            return f;
        }
    }
}