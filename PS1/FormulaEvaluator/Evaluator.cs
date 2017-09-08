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
        private static Exception malformedException = new ArgumentException("Malformed expression");

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
            //Exception malformedException = new ArgumentException("Malformed expression");

            //Traverses through the string
            foreach (String substring in substrings)
            {
                //Get rid of leading and trailing whitespace
                token = substring.Trim();

                if (token.Equals(""))
                    continue;

                //If the token is an integer
                else if (Int32.TryParse(token, out parsedInt))
                {
                    HandleInt(parsedInt, operators, values);
                }

                //If the token is a variable (checks to see if the first character is a letter)
                else if (Char.IsLetter(token[0]))
                {
                    VerifyVariable(token);

                    //Lookup variable and pass it to the same function that handles a normal integer
                    try
                    {
                        parsedInt = variableEvaluator(token);
                        HandleInt(parsedInt, operators, values);
                    }
                    catch (Exception)
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
                else if (token.Equals("+") || token.Equals("-"))
                {
                    HandlePlusMinus(operators, values);

                    //Pushes the token (+ or -) onto the stack always
                    operators.Push(token);
                }

                //If the token is * or / or (
                else if (token.Equals("*") || token.Equals("/") || token.Equals("("))
                    operators.Push(token);

                //If the token is )
                else if (token.Equals(")"))
                {
                    if (operators.IsOnTop<String>("+") || operators.IsOnTop<String>("-"))
                        HandlePlusMinus(operators, values);

                    if (!operators.IsOnTop<String>("("))
                        throw malformedException;
                    operators.Pop();

                    //If there is a value on the stack, pop it and hand it over to the int handling function
                    if (values.Count > 0)
                        HandleInt(values.Pop(), operators, values);

                }
            }

            //Operator and value stack checking after the last token has been processed
            if (operators.Count == 0 && values.Count == 1)
            {
                return values.Pop();
            }
            else if (operators.Count == 1 && values.Count == 2)
            {
                HandlePlusMinus(operators, values);
                return values.Pop();
            }
            else
                throw malformedException;
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
        /// <param name="valueStack"></param>
        private static void HandleInt(int number, Stack<String> operatorStack, Stack<int> valueStack)
        {
            if (operatorStack.IsOnTop<String>("*") || operatorStack.IsOnTop<String>("/"))
            {
                //If there isn't a value to be multiplied or divided by, throw an exception
                if (valueStack.Count < 1)
                    throw malformedException;

                String op = operatorStack.Pop();
                int preOpValue = valueStack.Pop();

                if (op.Equals("/"))
                {
                    if (number == 0)
                        throw new ArgumentException("Divide By Zero");
                    valueStack.Push(preOpValue / number);
                }
                else
                    valueStack.Push(preOpValue * number);
            }
            else
                valueStack.Push(number);
        }

        private static void HandlePlusMinus(Stack<String> operatorStack, Stack<int> valueStack)
        {
            //Checks if a + or - is currently on the operator stack
            if (operatorStack.IsOnTop<String>("+") || operatorStack.IsOnTop<String>("-"))
            {
                //Checks to make sure there are two values to be added before the current token is put on the stack
                if (valueStack.Count < 2)
                    throw malformedException;

                String op = operatorStack.Pop();
                int postOpvValue = valueStack.Pop();
                int preOpValue = valueStack.Pop();

                if (op.Equals("+"))
                {
                    valueStack.Push(preOpValue + postOpvValue);
                }
                else
                {
                    valueStack.Push(preOpValue - postOpvValue);
                }
            }
        }

        /// <summary>
        /// Makes sure that the variable follows the pattern a variable should of one or more letters followed
        /// by one or more letters
        /// </summary>
        /// <param name="variable"></param>
        private static void VerifyVariable(String variable)
        {
            bool reachedNumber = false;
            for (int letterPos = 1; letterPos < variable.Length; letterPos++)
            {
                char character = variable[letterPos];

                if (reachedNumber && !Char.IsNumber(character))
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

            if (!reachedNumber)
                throw malformedException;
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
