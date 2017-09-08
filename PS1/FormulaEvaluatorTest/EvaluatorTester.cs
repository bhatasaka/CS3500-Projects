using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FormulaEvaluator;


namespace FormulaEvaluatorTest
{
    class EvaluatorTester
    {

        static void Main(string[] args)
        {
            //Testing regular int recognition
            //Console.WriteLine(Evaluator.Evaluate("1", anyVarLookup));

            //Basic Tests
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
            TestTrueWithAnyLookup("order of operations", "4 + 6 * 2", 16);
            TestTrueWithAnyLookup("parenthesis addition and multiplication", "2 * (4 + 6)", 20);
            TestTrueWithAnyLookup("parenthesis all operators", "(8 - 5) * 6 / 2 + 4", 13);

            //Testing strange but valid syntax
            TestTrueWithAnyLookup("many parenthesis", "(((4 + 1))) * (8)", 40);
            TestTrueWithAnyLookup("strange whitespace", " 4-2    *  20   +1    ", -35);

            //Test variable lookup

            //Tests that are expected to fail - throw an exception
            TestExceptioneWithAnyLookup("double operands", "2 2 + 1");

            Console.ReadLine();
        }
        public static int anyVarLookup(String var)
        {
            return 1;
        }

        private static void TestTrueWithAnyLookup(String name, String expression, int expected)
        {
            int result = Evaluator.Evaluate(expression, anyVarLookup);
            if (result != expected)
                Console.WriteLine("\nfailed " + name + "\n");
            else
                Console.WriteLine("passed " + name);
        }

        private static void TestExceptioneWithAnyLookup(String name, String expression)
        {
            try
            {
                int result = Evaluator.Evaluate(expression, anyVarLookup);
                Console.WriteLine("\nfailed " + name + "\n");
            }
            catch(ArgumentException)
            {
                Console.WriteLine("passed " + name);
            }
        }
    }

}
