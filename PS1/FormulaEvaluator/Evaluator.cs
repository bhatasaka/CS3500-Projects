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

            //Remove all whitespace
            exp = Regex.Replace(exp, @"\s+", "");

            //Splits the exp string into an array of tokens
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            //Used if an entered token 
            int parsedInt;

            //Traverses through the string
            foreach(String substring in substrings)
            {
                //If the token is an integer
                if(Int32.TryParse(substring, out parsedInt))
                {
                    if(operators.IsOnTop<String>("*") || operators.IsOnTop<String>("/"))
                    {

                    }
                }
            }


            return 0;
        }


    }
    public static class PS1StackExtensions
    {
        public static bool IsOnTop<T>(this Stack<T> stack, T value)
        {
            if (stack.Count > 0 && stack.Peek().Equals(value))
                return true;
            else
                return false;
        }
    }
}
