using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// This is the namespace for the formula evaluator library.
/// This library contains a method that evaluates integer infix arithmetic expressions
/// 
/// Bryan Hatasaka
/// u1028471
/// </summary>
namespace FormulaEvaluator
{
    /// <summary>
    /// This class contains the implementation of the evaluator library
    /// </summary>
    public static class Evaluator
    {
        public delegate int Lookup(String v);
        private static Exception malformedException = new ArgumentException("Malformed expression");

        /// <summary>
        /// This function takes in a string that contains integer arithmetic expressions
        /// written using standard infix notation.
        /// 
        /// This method requires a delegate that takes in a variable (that follows a format of one or more letters
        /// followed by one or more numbers) and returns an integer.
        /// 
        /// This method allows the use of variables in the proper format to be evaluated along with the
        /// standard infix expression, provided that a proper delegate is provided.
        /// 
        /// For example: if the following string is provided: "A4 + 4 / 2" and the provided delgate returns
        /// 4 for the value of A4, the method returns 6.
        /// 
        /// 
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="variableEvaluator"></param>
        /// <returns></returns>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            //These stacks are used to hold the operators and operands of the infix expression
            Stack<int> values = new Stack<int>();
            Stack<String> operators = new Stack<String>();

            //Splits the exp string into an array of tokens
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            //Used while evaluating
            String token;

            //Traverses through the string
            foreach (String substring in substrings)
            {
                //Get rid of leading and trailing whitespace
                token = substring.Trim();

                if (token.Equals(""))
                    continue;

                //If the token is an integer
                else if (Int32.TryParse(token, out int parsedInt))
                {
                    HandleInt(parsedInt, operators, values);
                }

                //If the token is a variable (checks to see if the first character is a letter)
                else if (Char.IsLetter(token[0]))
                {
                    VerifyVariable(token);

                    //Try to lookup the variable and pass it to the same function that handles a normal integer
                    //If an exception is thrown by the delegate looking up the variable, throw an argument exception
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

                    //At this point, only a "(" should be on top of the stack. If there isn't one where expected,
                    //then throw an argument exception
                    if (!operators.IsOnTop<String>("("))
                        throw malformedException;
                    operators.Pop();

                    //If there is a value on the stack, pop it and hand it over to the int handling function
                    if (values.Count > 0)
                        HandleInt(values.Pop(), operators, values);

                }
            }

            //Operator and value stack checking after the last token has been processed.
            //If there are no operators, there should be 1 value on the stack - the result.
            //If there is an operator, there should be only 1 and it should be a + or -.
            //If this is the case, there should be two values on the value stack to be processed.
            //If one of these two conditions are not met, and argument exception is thrown
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
        /// Will perform the required actions for an integer in the formula evaluator.
        /// 
        /// If * or / is at the top of the operator stack, will pop the value stack and pop the operator stack, 
        /// and apply the popped operator to the popped number and passed number. Pushes the result onto the value stack.
        /// Otherwise, just pushes the passed number onto the value stack.
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

        /// <summary>
        /// Performs the required actions for a + or - on top of the stack in the evaluator function.
        /// 
        /// If a + or - is on the stack, the top two values on the valueStack will be applied to the operator
        /// with the highest number being the second number in the expression
        /// </summary>
        /// <param name="operatorStack"></param>
        /// <param name="valueStack"></param>
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
        /// Makes sure that the variable follows the correct format. A variable should be one or more letters followed
        /// by one or more letters. If it's not, an exception is thrown
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
    /// Adds a extension method to the Stack class.
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
