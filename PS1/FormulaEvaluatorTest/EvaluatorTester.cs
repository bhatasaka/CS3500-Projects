using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FormulaEvaluator;

/// <summary>
/// This namespace contains the testing class for the FormuluaEvaluator namespace - Evaluate method
/// 
/// Bryan Hatasaka
/// u1028471
/// </summary>
namespace FormulaEvaluatorTest
{
    /// <summary>
    /// This class contains the testing code for the Evaluate method in the FormulaEvaluator namespace.
    /// All test results are written to the console.
    /// 
    /// </summary>
    class EvaluatorTester
    {
        /// <summary>
        /// Exclusively contains the testing code for the Evaluate method
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //Basic, core tests
            Console.WriteLine("\tBasic variables tests:");
            TestTrueWithAnyLookup("single int", "22", 22);
            TestTrueWithAnyLookup("two int addition", "1 + 1", 2);
            TestTrueWithAnyLookup("two int subtraction", "1 - 1", 0);
            TestTrueWithAnyLookup("two int multiplication", "2 * 3", 6);
            TestTrueWithAnyLookup("two int division", " 6 / 3", 2);
            TestTrueWithAnyLookup("single int, single var addition", "1 + A4", 2);
            TestTrueWithAnyLookup("single int, single var subtraction", "1 - A4", 0);
            TestTrueWithAnyLookup("single int, single var multiplication", "2 * A4", 2);
            TestTrueWithAnyLookup("single int, single var division", "12 / A4", 12);

            //Slightly more complex but still core tests
            Console.WriteLine("\n\tSlightly more complex variable tests:");
            TestTrueWithAnyLookup("order of operations", "4 + 6 * 2", 16);
            TestTrueWithAnyLookup("parenthesis addition and multiplication", "2 * (4 + 6)", 20);
            TestTrueWithAnyLookup("parenthesis all operators", "(8 - 5) * 6 / 2 + 4", 13);
            TestTrueWithAnyLookup("complicated parenthesis", "(8 - 5 * 2 + 6) * 4 / 2 + 4", 12);

            //Testing strange but valid syntax
            Console.WriteLine("\n\tStrange but valid syntax tests:");
            TestTrueWithAnyLookup("many parenthesis", "(((4 + 1))) * (8)", 40);
            TestTrueWithAnyLookup("strange whitespace", " 4-2    *  20   +1    ", -35);


            //Test variable lookup
            Console.WriteLine("\n\tVariable lookup tests:");
            TestTrueWithMultiLookup("A4 variable", "A4 + 2", 6);
            TestTrueWithMultiLookup("Two different variables", "A4 + A6", 10);
            TestTrueWithMultiLookup("Variables with many operators", "A4 - A6 + R8 / 100", 4);

            //Tests that are expected to fail - throw an exception
            Console.WriteLine("\n\tFailing tests:");
            TestExceptioneWithAnyLookup("double operands", "2 2 + 1");
            TestExceptioneWithAnyLookup("double operators", "2 + + 2");
            TestExceptioneWithAnyLookup("operator in front", "+ 2");
            TestExceptioneWithAnyLookup("divide by zero", "2 / 0");
            TestExceptioneWithAnyLookup("bad variable", "2 + A5A");
            TestExceptioneWithAnyLookup("bad variable 2", "2 + 5A");
            TestExceptioneWithAnyLookup("negative first number", "(-2 + 1)");
            TestExceptioneWithAnyLookup("just parenthesis", "()");
            TestExceptioneWithMultiLookup("unrecognized variable", "A2 + 2");

            //Here to keep the console up so the results can be read
            Console.ReadLine();
        }

        /// <summary>
        /// This function takes in a string and returns the integer value 1
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        public static int anyVarLookup(String var)
        {
            return 1;
        }

        /// <summary>
        /// This function will return 3 numbers for three certain strings:
        /// A4 returns 4
        /// A6 returns 6
        /// R8 returns 600
        /// 
        /// otherwise and exception is thrown
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        public static int multiVarLookup(String var)
        {
            if (var.Equals("A4"))
                return 4;
            else if (var.Equals("A6"))
                return 6;
            else if(var.Equals("R8"))
                return 600;
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// This method uses the Evaluate function in the FormulaEvaluator namespace to
        /// evaluate the given string. If the result of the evaluate method using the given string is
        /// the same as the expected int result, "passed" and name is printed to the console.
        /// Otherwise FAILED and the name is printed.
        /// 
        /// Uses anyVarLookup lookup function
        /// </summary>
        /// <param name="name"></param>
        /// <param name="expression"></param>
        /// <param name="expected"></param>
        private static void TestTrueWithAnyLookup(String name, String expression, int expected)
        {
            int result = Evaluator.Evaluate(expression, anyVarLookup);
            if (result != expected)
                Console.WriteLine("\nFAILED " + name + "\n");
            else
                Console.WriteLine("passed " + name);
        }

        /// <summary>
        /// This method uses the Evaluate function in the FormulaEvaluator namespace to
        /// evaluate the given string. If the result of the evaluate method using the given string is
        /// the same as the expected int result, "passed" and name is printed to the console.
        /// Otherwise FAILED and the name is printed.
        /// 
        /// Uses multiVarLookup lookup function
        /// </summary>
        /// <param name="name"></param>
        /// <param name="expression"></param>
        /// <param name="expected"></param>
        private static void TestTrueWithMultiLookup(String name, String expression, int expected)
        {
            int result = Evaluator.Evaluate(expression, multiVarLookup);
            if (result != expected)
                Console.WriteLine("\nFAILED " + name + "\n");
            else
                Console.WriteLine("passed " + name);
        }

        /// <summary>
        /// This method uses the Evaluate function in the FormulaEvaluator namespace to
        /// evaluate the given string. If the result of the evaluate method throws an exception,
        /// "passed" and name is printed to the console.
        /// Otherwise FAILED and the name is printed.
        /// 
        /// Uses anyVarLookup lookup function
        /// </summary>
        /// <param name="name"></param>
        /// <param name="expression"></param>
        private static void TestExceptioneWithAnyLookup(String name, String expression)
        {
            try
            {
                int result = Evaluator.Evaluate(expression, anyVarLookup);
                Console.WriteLine("\nFAILED " + name + "\n");
            }
            catch(ArgumentException)
            {
                Console.WriteLine("passed " + name);
            }
        }

        /// <summary>
        /// This method uses the Evaluate function in the FormulaEvaluator namespace to
        /// evaluate the given string. If the result of the evaluate method throws an exception,
        /// "passed" and name is printed to the console.
        /// Otherwise FAILED and the name is printed.
        /// 
        /// Uses multiVarLookup lookup function
        /// </summary>
        /// <param name="name"></param>
        /// <param name="expression"></param>
        private static void TestExceptioneWithMultiLookup(String name, String expression)
        {
            try
            {
                int result = Evaluator.Evaluate(expression, multiVarLookup);
                Console.WriteLine("\nFAILED " + name + "\n");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("passed " + name);
            }
        }
    }

}
