using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// This is the formula evaluator library
/// 
/// Bryan Hatasaka
/// u1028471
/// </summary>
namespace FormulaEvaluator
{
    public static class Evaluator
    {
        public delegate int Lookup(String v);

        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            Stack<int> values = new Stack<int>();
            Stack<String> operators = new Stack<String>();

            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            int parsedInt;


            if(Int32.TryParse() )

            return 0;
        }
    }

}
