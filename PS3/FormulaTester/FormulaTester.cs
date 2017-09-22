using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace FormulaTester
{
    [TestClass]
    public class FormulaTester
    {
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

        // ==============================================
        // Private function tests
        //===============================================

        [TestMethod]
        public void TestIsValidTokenDouble()
        {
            Formula form = new Formula("0.0");
            PrivateObject formAcessor = new PrivateObject(form);

           
            Assert.AreEqual(true, formAcessor.Invoke("IsValidToken", new String[1] { "1.0" }));
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

            Assert.AreEqual(true, formAcessor.Invoke("VerifyVariable", new String[1] { "_A" }));
        }

    }
}
