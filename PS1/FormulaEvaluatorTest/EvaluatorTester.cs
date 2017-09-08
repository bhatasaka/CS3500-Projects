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
            //Testing regular int recognition
            Console.WriteLine(Evaluator.Evaluate("(1 + 2) * 3", varLookup));

            Console.ReadLine();

        }
        public static int varLookup(String v)
        {
            return 1;
        }
    }

}
