using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System.Collections.Generic;

namespace FormulaTester
{
    [TestClass]
    public class FormulaTester
    {
        //======================================
        //Testing the constructor and evaluate
        //======================================
        [TestMethod]
        public void TestEvaluateSingleInt()
        {
            Formula form = new Formula("22");

            object result = form.Evaluate(s => 1);
            if(result is double)
                result = (double)result;

            Assert.AreEqual(22.0, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleAddition()
        {
            Formula form = new Formula("1.0+1");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(2.0, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleSubtraction()
        {
            Formula form = new Formula("4-2");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(2.0, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleMultiplication()
        {
            Formula form = new Formula("4*2");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(8.0, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleDivision()
        {
            Formula form = new Formula("8/4");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(2.0, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleDoubleAddition()
        {
            Formula form = new Formula("1.6+2.8");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(4.4, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleDoubleSubtraction()
        {
            Formula form = new Formula("5.6-3.2");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(5.6-3.2, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleDoubleMultiplication()
        {
            Formula form = new Formula("4.4*8.2");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(36.08, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleDoubleRationalDivision()
        {
            Formula form = new Formula("12.4/2.2");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(12.4/2.2, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleDoubleIrrationalDivision()
        {
            Formula form = new Formula("1/3");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(0.3333333333333333333333333333333333333333333, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleVar()
        {
            Formula form = new Formula("A1");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(1.0, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleVarAddition()
        {
            Formula form = new Formula("A1 + 2");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(3.0, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleVarSubtraction()
        {
            Formula form = new Formula("A1 - 3");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(-2.0, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleVarMultiplication()
        {
            Formula form = new Formula("A1*6");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(6.0, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleVarDivision()
        {
            Formula form = new Formula("A6/4");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(0.25, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleParentheses()
        {
            Formula form = new Formula("(1+6.4)*4");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(29.6, result);
        }

        [TestMethod]
        public void TestEvaluateSimpleOrderOfOperations()
        {
            Formula form = new Formula("1+6.4*4");

            object result = form.Evaluate(s => 1);
            if (result is double)
                result = (double)result;

            Assert.AreEqual(26.6, result);
        }

        //Testing invalid evaluates

        [TestMethod]
        public void TestEvaluateDivideByZero()
        {
            Formula form = new Formula("1/0");

            object result = form.Evaluate(s => 1);


            Assert.IsTrue(result is FormulaError);

            FormulaError error = (FormulaError)result;

            Assert.IsTrue(error.Reason is String);
        }

        [TestMethod]
        public void TestEvaluateDivideByZeroVar()
        {
            Formula form = new Formula("1/A6");

            object result = form.Evaluate(s => 0);


            Assert.IsTrue(result is FormulaError);

            FormulaError error = (FormulaError)result;

            Assert.IsTrue(error.Reason is String);
        }

        [TestMethod]
        public void TestEvaluateDivideByZeroEnd()
        {
            Formula form = new Formula("1/(3-3)");

            object result = form.Evaluate(s => 0);


            Assert.IsTrue(result is FormulaError);

            FormulaError error = (FormulaError)result;

            Assert.IsTrue(error.Reason is String);
        }

        [TestMethod]
        public void TestEvaluateBadLookup()
        {
            Formula form = new Formula("A6*6");

            object result = form.Evaluate(lookup);

            double lookup(String str)
            {
                throw new ArgumentException();
            }

            Assert.IsTrue(result is FormulaError);
        }

        //======================================
        //Additional testing for the constructor
        //======================================
        [TestMethod]
        public void TestConstructorSingleInt()
        {
            Formula form = new Formula("22");
        }

        [TestMethod]
        public void TestConstructorWithNormalizer()
        {
            Formula form = new Formula("a1", normalizer, str => true);

            string normalizer(string str)
            {
                if (str.Equals("a1"))
                    return "A1";
                else
                    return str;
            }
        }

        [TestMethod]
        public void TestConstructorWithNormalizerWorking()
        {
            Formula form = new Formula("a1", normalizer, str => true);

            string normalizer(string str)
            {
                if (str.Equals("a1"))
                    return "A1";
                else
                    return str;
            }
            
            HashSet<String> expected = new HashSet<String>{ "A1" };
            Assert.IsTrue(expected.SetEquals(new HashSet<String>(form.GetVariables())));
        }

        [TestMethod]
        public void TestConstructorWithIsValid()
        {
            Formula form = new Formula("a1", str => str, isValid);

            bool isValid(string str)
            {
                if (str.Equals("a1"))
                    return true;
                else
                    return false;
            }
        }

        [TestMethod]
        public void TestConstructorComplex()
        {
            Formula form = new Formula("(4.32/80.668- 23.4  ) * (A4 - A6) + (5+6) - (A3)");
        }

        //Constructor tests that throw exceptions

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorNoToken()
        {
            Formula form = new Formula("");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorNullToken()
        {
            Formula form = new Formula(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorStartFormWithOperator()
        {
            Formula form = new Formula("+6-7");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorStartFormWithClosingParenthesis()
        {
            Formula form = new Formula(")-5-6");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorNonValidCharacter()
        {
            Formula form = new Formula("5+A4 / '");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorOpenParenthesisFollowingNumber()
        {
            Formula form = new Formula("4+6(-8");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorOpenParenthesisFollowingVar()
        {
            Formula form = new Formula("4+A8(-8");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorOpenParenthesisFollowingClosingParen()
        {
            Formula form = new Formula("4+(6 -2 )(-8");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorUnequalParentheses()
        {
            Formula form = new Formula("(9-2) / 8+6)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorClosingParenFollowingOperator()
        {
            Formula form = new Formula("4+6+)9");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorClosingParenFollowingOpenParen()
        {
            Formula form = new Formula("4+6+()9");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorOperatorFollowingOperator()
        {
            Formula form = new Formula("4/2/-6");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorOperatorFollowingOpeningParen()
        {
            Formula form = new Formula("4/2(-6");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorVarFollowingNumber()
        {
            Formula form = new Formula("5-6A4");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorNumberFollowingClosingParen()
        {
            Formula form = new Formula("4-(6+a4)5");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorEndingWithOperator()
        {
            Formula form = new Formula("4-6-");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorEndingWithOpeningParen()
        {
            Formula form = new Formula("2/6+5(");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorNonValidatedVariable()
        {
            Formula form = new Formula("a6", str => str, isValid);

            bool isValid(string str)
            {
                if (str.Equals("a6"))
                    return false;
                return true;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorNonValidatedVariableBadSymbolInBack()
        {
            Formula form = new Formula("a6_#", str => str, isValid);

            bool isValid(string str)
            {
                if (str.Equals("a6"))
                    return false;
                return true;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructorNonValidatedVariableBadSymbolInFront()
        {
            Formula form = new Formula("_%a6_", str => str, isValid);

            bool isValid(string str)
            {
                if (str.Equals("a6"))
                    return false;
                return true;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructoBadVariable()
        {
            Formula form = new Formula("_7A_");
        }


        //======================================
        //Testing Equals
        //======================================
        [TestMethod]
        public void TestEqualsSimpleDoubles()
        {
            Formula form = new Formula("2.0");
            Formula form2 = new Formula("2");

            Assert.IsTrue(form.Equals(form2));
        }

        [TestMethod]
        public void TestEqualsScientificForm()
        {
            Formula form = new Formula("2.0");
            Formula form2 = new Formula("2E0");

            Assert.IsTrue(form.Equals(form2));
        }

        [TestMethod]
        public void TestEqualsSimpleAddition()
        {
            Formula form = new Formula("2 + 6");
            Formula form2 = new Formula("2+6");

            Assert.IsTrue(form.Equals(form2));
        }

        [TestMethod]
        public void TestEqualsVar()
        {
            Formula form = new Formula("_A7_2-7/4.0*6E1");
            Formula form2 = new Formula("_A7_2 - 7 / 4 * 60");

            Assert.IsTrue(form.Equals(form2));
        }

        [TestMethod]
        public void TestEqualsSameInstance()
        {
            Formula form = new Formula("5");
            Formula form2 = form;

            Assert.IsTrue(form.Equals(form2));
        }

        [TestMethod]
        public void TestEqualsDifferntObject()
        {
            Formula form = new Formula("5");
            String form2 = "hi ^_^";

            Assert.IsFalse(form.Equals(form2));
        }

        [TestMethod]
        public void TestEqualsOpSimple()
        {
            Formula form = new Formula("5+6");
            Formula form2 = new Formula("5.0 +     6E0");

            Assert.IsTrue(form == form2);
        }

        [TestMethod]
        public void TestEqualsOpNullAndNotNullF2()
        {
            Formula form = new Formula("5");
            Formula form2 = new Formula("5");
            form2 = null;

            Assert.IsFalse(form == form2);
        }

        [TestMethod]
        public void TestEqualsOpNullAndNotNullF1()
        {
            Formula form = new Formula("5");
            Formula form2 = new Formula("5");
            form = null;

            Assert.IsFalse(form == form2);
        }

        [TestMethod]
        public void TestEqualsOpNull()
        {
            Formula form = new Formula("5");
            Formula form2 = new Formula("4");
            form = null;
            form2 = null;

            Assert.IsTrue(form == form2);
        }

        [TestMethod]
        public void TestEqualsOpSameInstance()
        {
            Formula form = new Formula("5");
            Formula form2 = form;

            Assert.IsTrue(form == form2);
        }

        [TestMethod]
        public void TestNotEqualsOpSame()
        {
            Formula form = new Formula("4.0");
            Formula form2 = new Formula("4");

            Assert.IsFalse(form != form2);
        }

        [TestMethod]
        public void TestNotEqualsOpDifferent()
        {
            Formula form = new Formula("5.6+11");
            Formula form2 = new Formula("22-26");

            Assert.IsTrue(form != form2);
        }

        [TestMethod]
        public void TestNotEqualsOpDifferentLengths()
        {
            Formula form = new Formula("4-6+2");
            Formula form2 = new Formula("4-6");

            Assert.IsTrue(form != form2);
        }

        [TestMethod]
        public void TestNotEqualsOpDifferentTypesInTheSamePositions()
        {
            Formula form = new Formula("4+3+2-2");
            Formula form2 = new Formula("(4-2)+2");

            Assert.IsTrue(form != form2);
        }

        // ==============================================
        // Testing GetVariables
        //===============================================

        [TestMethod]
        public void TestGetVariablesNoVars()
        {
            Formula form = new Formula("4+2");
            HashSet<String> expectedVariables = new HashSet<string>
            {
                
            };

            Assert.IsTrue(expectedVariables.SetEquals(new HashSet<String>(form.GetVariables())));
        }

        [TestMethod]
        public void TestGetVariablesSingleVar()
        {
            Formula form = new Formula("A4-17");
            HashSet<String> expectedVariables = new HashSet<string>
            {
                "A4"
            };

            Assert.IsTrue(expectedVariables.SetEquals(new HashSet<String>(form.GetVariables())));
        }

        [TestMethod]
        public void TestGetVariablesMultipleVars()
        {
            Formula form = new Formula("A4 - _U8_ + 4");
            HashSet<String> expectedVariables = new HashSet<string>
            {
                "A4", "_U8_"
            };

            Assert.IsTrue(expectedVariables.SetEquals(new HashSet<String>(form.GetVariables())));
        }

        [TestMethod]
        public void TestGetVariablesDuplicates()
        {
            Formula form = new Formula("A4 - U7 - A6 + 5 - A4");
            HashSet<String> expectedVariables = new HashSet<string>
            {
                "A4", "U7", "A6"
            };

            Assert.IsTrue(expectedVariables.SetEquals(new HashSet<String>(form.GetVariables())));
        }

        //=====================
        //ToString and Hashcode
        //=====================
        [TestMethod]
        public void TestToString()
        {
            Formula form = new Formula("A4 - U7     - A6 + 5 - A4");

            Assert.AreEqual("A4-U7-A6+5-A4", form.ToString());
        }

        [TestMethod]
        public void TestGetHashcode()
        {
            Formula form = new Formula("5E2 + A4 - U7     - A6 + 5.00 - A4");
            Formula form2 = new Formula("500 +A4 - U7    - A6 + 5 - A4");

            Assert.IsTrue(form.GetHashCode() == form2.GetHashCode());
        }

        // ==============================================
        // Private function tests
        //===============================================
        /*
        [TestMethod]
        public void TestIsValidTokenDouble()
        {
            Formula form = new Formula("0.0");
            PrivateObject formAcessor = new PrivateObject(form);
           
            Assert.AreEqual(true, formAcessor.Invoke("IsValidToken", new Object[1] { "1.0" }));
        }

        [TestMethod]
        public void TestIsValidTokenOperator()
        {
            Formula form = new Formula("0.0");
            PrivateObject formAcessor = new PrivateObject(form);

            String[] operators = { "(", ")", "+", "-", "*", "/" };
            foreach(String op in operators)
            {
                Assert.AreEqual(true, formAcessor.Invoke("IsValidToken", new String[1] { op }));
            }
        }

        [TestMethod]
        public void TestIsValidTokenNonValidTokens()
        {
            Formula form = new Formula("0.0");
            PrivateObject formAcessor = new PrivateObject(form);

            String token;
            for(int ASCII_value = 0; ASCII_value <= 39; ASCII_value++)
            {
                token = Char.ConvertFromUtf32(ASCII_value);
                Assert.AreEqual(false, formAcessor.Invoke("IsValidToken", new String[1] { token }));
            }
        }

        [TestMethod]
        public void TestIsValidTokenSimpleVar()
        {
            Formula form = new Formula("0.0");
            PrivateObject formAcessor = new PrivateObject(form);

            Assert.AreEqual(true, formAcessor.Invoke("IsValidToken", new String[1] { "_A" }));
        }

        [TestMethod]
        public void TestVerifyVariableSimpleVar()
        {
            Formula form = new Formula("0.0");
            PrivateObject formAcessor = new PrivateObject(form);
            formAcessor.Invoke("VerifyVariable", new String[1] { "A" });

            Assert.AreEqual(true, formAcessor.Invoke("VerifyVariable", new String[1] { "_A" }));
        }
        */

    }
}
