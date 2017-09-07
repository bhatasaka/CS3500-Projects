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

            int prevValue;
            int result;
            String op = null;
            bool variable = false;

            //Traverses through the string
            foreach(String substring in substrings)
            {
                //If the token is an integer
                if(Int32.TryParse(substring, out parsedInt))
                {
                    HandleInt(parsedInt, operators, values);
                }
                //If the token is a variable
                else if(Char.IsLetter(substring[0]))
                {
                    //Go through and make sure the variable follows the correct pattern (one or more letters followed by one or more numbers)
                    bool reachedNumber = false;
                    for(int letterPos = 1; letterPos < substring.Length; letterPos++)
                    {
                        char character = substring[letterPos];
                        if (reachedNumber && Char.IsNumber(character))
                        {
                        //TODO throw exception
                        }
                        else
                            if (Char.IsNumber(character))
                            reachedNumber = true;
                            
                    }
                }

                //If the token is a variable
            }

            //Operator and value stack checking after the last token has been processed

            return 0;
        }

        private static void HandleInt(int number, Stack<String> operatorStack, Stack<int> valuesStack)
        {
            if (operatorStack.IsOnTop<String>("*") || operatorStack.IsOnTop<String>("/"))
            {
                String op = operatorStack.Pop();
                int prevValue = valuesStack.Pop();
                int result;

                if (op.Equals("/"))
                    result = prevValue / number;
                else
                    result = prevValue * number;

                valuesStack.Push(result);
            }
            else
                valuesStack.Push(number);
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
