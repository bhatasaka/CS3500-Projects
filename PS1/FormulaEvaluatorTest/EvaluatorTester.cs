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
        public delegate int Lookup(String v);

        static void Main(string[] args)
        {
            //Testing removing whitespace
            String word = "This Is A String \n With Some Spaces";
            Console.WriteLine(word);
            word = Regex.Replace(word, @"\s+", "");
            Console.WriteLine(word);

            //Testing tryparse
            int parsedInt;
            Int32.TryParse("21", out parsedInt);

            if (parsedInt != 21)
                Console.WriteLine("Test 1 - Failed - parse int, regular int");

            if (Int32.TryParse("2.1", out parsedInt))
                Console.WriteLine("Test 2 - Failed - parse int, real number");


            //Testing regular int recognition
            Evaluator.Evaluate("1 * A4", varLookup);

            Console.ReadLine();
        }
        public static int varLookup(String v)
        {
            return 1;
        }
    }

}
