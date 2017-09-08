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
            //exp = Regex.Replace(exp, @"\s+", "");

            //Splits the exp string into an array of tokens
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            //Used if an entered token is an integer
            int parsedInt;
            String token;
            Exception malformedException = new ArgumentException("Malformed expression");

            //Traverses through the string
            foreach(String substring in substrings)
            {
                token = substring.Trim();
                //If the token is an integer
                if (Int32.TryParse(token, out parsedInt))
                {
                    HandleInt(parsedInt, operators, values);
                }

                //If the token is a variable (checks to see if the first character is a letter)
                else if (Char.IsLetter(token[0]))
                {
                    //Go through and make sure the variable follows the correct pattern (one or more letters followed by one or more numbers)
                    bool reachedNumber = false;
                    for (int letterPos = 1; letterPos < token.Length; letterPos++)
                    {
                        char character = token[letterPos];
                        if (reachedNumber && Char.IsNumber(character))
                        {
                            throw malformedException;
                        }
                        else if (!reachedNumber && Char.IsNumber(character))
                            reachedNumber = true;
                        else if (!reachedNumber && !Char.IsLetter(character))
                        {
                            throw malformedException;
                        }
                    }

                    //Lookup variable and pass it to the same function that handles a normal integer
                    try
                    {
                        parsedInt = variableEvaluator(token);
                        HandleInt(parsedInt, operators, values);
                    }
                    catch(Exception)
                    {
                        throw new ArgumentException("Unknown variable");
                    }

                }

                //Tokens from now on must be operators.
                //All operators must be of length 1: "+", "-", "*", "/", "(", or ")"
                //If the token is not of length 1, throw an exception
                else if (token.Length != 1)
                    throw malformedException;

                //If the token is + or -
                else if(token.Equals("+") || token.Equals("-"))
                {
                    //Checks if a + or - is currently on the operator stack
                    if (operators.IsOnTop<String>("+") || operators.IsOnTop<String>("-"))
                    {
                        //Checks to make sure there are two values to be added before the current token is put on the stack
                        if(values.Count < 2)
                        {
                            throw malformedException;
                        }

                        int postOpvValue = values.Pop();
                        int preOpValue = values.Pop();
                        String op = operators.Pop();

                        if (op.Equals("+"))
                        {
                            values.Push(preOpValue + postOpvValue);
                        }
                        else
                        {
                            values.Push(preOpValue - postOpvValue);
                        }
                    }

                    //Pushes the token (+ or -) onto the stack always
                    operators.Push(token);
                }
            }

            //Operator and value stack checking after the last token has been processed

            return 0;
        }

        /// <summary>
        /// Will perform the required actions for an integer in the formula evaluator.\n
        /// 
        /// \nIf * or / is at the top of the operator stack, will pop the value stack and pop the operator stack, 
        /// and apply the popped operator to the popped number and passed number. Pushes the result onto the value stack.
        /// \nOtherwise, just pushes the passed number onto the value stack.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="operatorStack"></param>
        /// <param name="valuesStack"></param>
        private static void HandleInt(int number, Stack<String> operatorStack, Stack<int> valuesStack)
        {
            if (operatorStack.IsOnTop<String>("*") || operatorStack.IsOnTop<String>("/"))
            {
                String op = operatorStack.Pop();
                int preOpValue = valuesStack.Pop();

                if (op.Equals("/"))
                {
                    if (number == 0)
                        throw new ArgumentException("Divide By Zero");
                    valuesStack.Push(preOpValue / number);
                }
                else
                    valuesStack.Push(preOpValue * number);
            }
            else
                valuesStack.Push(number);
        }
    }

    /// <summary>
    /// Adds a extension method to the Stack class
    /// </summary>
    public static class PS1StackExtensions
    {
        /// <summary>
        /// Returns true if the specified value is on top of the stack.
        /// False otherwise
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stack"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsOnTop<T>(this Stack<T> stack, T value)
        {
            if (stack.Count > 0 && stack.Peek().Equals(value))
                return true;
            else
                return false;
        }
    }
}
