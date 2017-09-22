﻿// Skeleton written by Joe Zachary for CS 3500, September 2013
// Read the entire skeleton carefully and completely before you
// do anything else!

// Version 1.1 (9/22/13 11:45 a.m.)

// Change log:
//  (Version 1.1) Repaired mistake in GetTokens
//  (Version 1.1) Changed specification of second constructor to
//                clarify description of how validation works

// (Daniel Kopta) 
// Version 1.2 (9/10/17) 

// Change log:
//  (Version 1.2) Changed the definition of equality with regards
//                to numeric tokens


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax; variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        private String normalizedExp;
        
        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
            normalizedExp = formula;
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            //These stacks are used to hold the operators and operands of the infix expression
            Stack<double> values = new Stack<double>();
            Stack<String> operators = new Stack<String>();

            //Splits the exp string into an array of tokens
            string[] substrings = Regex.Split(normalizedExp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

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
                else if (Double.TryParse(token, out double parsedDouble))
                {
                    HandleInt(parsedDouble, operators, values);
                }

                //If the token is a variable (checks to see if the first character is a letter)
                else if (Char.IsLetter(token[0]))
                {
                    //VerifyVariable(token); TODO

                    //Try to lookup the variable and pass it to the same function that handles a normal integer
                    //If an exception is thrown by the delegate looking up the variable, throw an argument exception
                    try
                    {
                        parsedDouble = lookup(token);
                        HandleInt(parsedDouble, operators, values);
                    }
                    catch (Exception)
                    {
                        // throw new ArgumentException("Unknown variable"); TODO
                        return new FormulaError(""); //TODO
                    }

                }

                /*
                //Tokens from now on must be operators.
                //All operators must be of length 1: "+", "-", "*", "/", "(", or ")"
                //If the token is not of length 1, throw an exception
                else if (token.Length != 1)
                    //throw malformedException; //TODO
                */

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
                    {
                        //throw malformedException; TODO
                        return new FormulaError(""); //TODO
                    }
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
                //throw malformedException; TODO
                return new FormulaError(""); //TODO
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            return null;
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            return null;
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object obj)
        {
            return false;
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return true.  If one is
        /// null and one is not, this method should return false.
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            return false;
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return false.  If one is
        /// null and one is not, this method should return true.
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            return false;
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return 0;
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

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
        private static void HandleInt(double number, Stack<String> operatorStack, Stack<double> valueStack)
        {
            if (operatorStack.IsOnTop<String>("*") || operatorStack.IsOnTop<String>("/"))
            {
                //If there isn't a value to be multiplied or divided by, throw an exception
                if (valueStack.Count < 1)
                {
                    //TODO
                    //return new FormulaError("");
                }

                String op = operatorStack.Pop();
                double preOpValue = valueStack.Pop();

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
        private static void HandlePlusMinus(Stack<String> operatorStack, Stack<double> valueStack)
        {
            //Checks if a + or - is currently on the operator stack
            if (operatorStack.IsOnTop<String>("+") || operatorStack.IsOnTop<String>("-"))
            {
                //Checks to make sure there are two values to be added before the current token is put on the stack
                if (valueStack.Count < 2)
                {
                    //throw malformedException; TODO
                }

                String op = operatorStack.Pop();
                double postOpvValue = valueStack.Pop();
                double preOpValue = valueStack.Pop();

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
                    //throw malformedException; TODO
                }
                else if (!reachedNumber && Char.IsNumber(character))
                    reachedNumber = true;
                else if (!reachedNumber && !Char.IsLetter(character))
                {
                    //throw malformedException; TODO
                }
            }

            if (!reachedNumber)
            {
                //throw malformedException;TODO
            }
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }

    /// <summary>
    /// Adds a extension method to the Stack class.
    /// </summary>
    public static class PS3StackExtensions
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
