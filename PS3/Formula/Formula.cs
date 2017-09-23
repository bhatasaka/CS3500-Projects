// Skeleton written by Joe Zachary for CS 3500, September 2013
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
        //Holds the normalized version of the final formula. Will only hold it if the object can be created.
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
            normalizedExp = VerifySyntax(formula, normalize, isValid);
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
            FormulaError divideByZero = new FormulaError("Error: Divide by 0. Check expression for any possible " +
                "divsions by 0.");

            //Traverses through the string
            foreach (String token in GetTokens(normalizedExp))
            {
                //If the token is an integer
                if (Double.TryParse(token, out double parsedDouble))
                {
                    if (!HandleDouble(parsedDouble, operators, values))
                        return divideByZero;
                }

                //If the token is a variable (checks to see if the first character is a letter)
                else if (token.StartsAsVar())
                {

                    //Try to lookup the variable and pass it to the same function that handles a normal integer
                    //If an exception is thrown by the delegate looking up the variable, return an error
                    try
                    {
                        parsedDouble = lookup(token);
                        if (!HandleDouble(parsedDouble, operators, values))
                            return divideByZero;
                    }
                    catch (Exception)
                    {
                        return new FormulaError("A value for the variable " + token + "could not be found. " +
                            "Check the variable or the lookup delegate");
                    }

                }

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

                    //At this point, only a "(" should be on top of the stack.
                    operators.Pop();

                    //If there is a value on the stack, pop it and hand it over to the int handling function
                    if (values.Count > 0)
                        if (!HandleDouble(values.Pop(), operators, values))
                            return divideByZero;
                }

            }

            //Operator and value stack checking after the last token has been processed.
            //If there are no operators, there should be 1 value on the stack - the result.
            //If there is an operator, there should be only 1 and it should be a + or -.
            //If this is the case, there should be two values on the value stack to be processed.
            if (operators.Count == 0 && values.Count == 1)
            {
                return values.Pop();
            }
            //Operators count should be 1 and values count should be 2
            else
            {
                HandlePlusMinus(operators, values);
                return values.Pop();
            }
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
            HashSet<String> variables = new HashSet<string>();
            foreach(String token in GetTokens(normalizedExp))
            {
                if (token.StartsAsVar())
                {
                    variables.Add(token);
                }
            }

            return variables;
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
            String returnString = normalizedExp;
            return returnString;
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
            //If the object isn't a formula object return false
            if(!(obj is Formula))
            {
                return false;
            }
            String otherForm = ((Formula)obj).normalizedExp;

            Queue<string> thisQueue = new Queue<String>();
            Queue<string> otherQueue = new Queue<String>();
            Double thisNumber;
            Double otherNumber;
            String otherToken;

            //Queueing the tokens in each formula
            foreach (String token in GetTokens(normalizedExp))
            {
                thisQueue.Enqueue(token);
            }
            foreach (String token in GetTokens(otherForm))
            {
                otherQueue.Enqueue(token);
            }

            //Comparing the strings, token by token
            if (thisQueue.Count != otherQueue.Count)
                return false;
            foreach(String thisToken in thisQueue)
            {
                otherToken = otherQueue.Dequeue();
                //Getting double.parse's number then toString in order to compare numbers
                if (Double.TryParse(thisToken, out thisNumber) &&
                    Double.TryParse(otherToken, out otherNumber))
                {
                    if(!(thisNumber.ToString().Equals(otherNumber.ToString())))
                        return false;
                }
                else if (!(thisToken.Equals(otherToken)))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return true.  If one is
        /// null and one is not, this method should return false.
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            if (System.Object.ReferenceEquals(f1, f2))
                return true;
            if((Object)f1 == null || (Object)f2 == null)
                return false;
            return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return false.  If one is
        /// null and one is not, this method should return true.
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            return !(f1 == f2);
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
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
        /// Will perform the required actions for a double in the formula evaluator.
        /// 
        /// If * or / is at the top of the operator stack, will pop the value stack and pop the operator stack, 
        /// and apply the popped operator to the popped number and passed number. Pushes the result onto the value stack.
        /// Otherwise, just pushes the passed number onto the value stack.
        /// 
        /// If divide by zero is attempted, returns false. Else true.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="operatorStack"></param>
        /// <param name="valueStack"></param>
        private static bool HandleDouble(double number, Stack<String> operatorStack, Stack<double> valueStack)
        {
            if (operatorStack.IsOnTop<String>("*") || operatorStack.IsOnTop<String>("/"))
            {
                String op = operatorStack.Pop();
                double preOpValue = valueStack.Pop();

                if (op.Equals("/"))
                {
                    if (number == 0)
                        return false;
                    valueStack.Push(preOpValue / number);
                }
                else
                    valueStack.Push(preOpValue * number);
            }
            else
                valueStack.Push(number);

            return true;
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
                String op = operatorStack.Pop();
                double postOpvValue = valueStack.Pop();
                double preOpValue = valueStack.Pop();

                if (op.Equals("+"))
                    valueStack.Push(preOpValue + postOpvValue);
                else
                    valueStack.Push(preOpValue - postOpvValue);
            }
        }

        /// <summary>
        /// Makes sure that the variable follows the correct format. A variable consists
        /// of a letter or underscore followed by zero or more letters, underscores, or digits
        /// </summary>
        /// <param name="variable"></param>
        private static bool VerifyVariable(String variable)
        {
            bool reachedNumber = false;
            for (int letterPos = 1; letterPos < variable.Length; letterPos++)
            {
                char character = variable[letterPos];

                if (character.Equals('_'))
                {
                    continue;
                }
                else if (reachedNumber && !Char.IsNumber(character))
                {
                    return false;
                }
                else if (!reachedNumber && Char.IsNumber(character))
                {
                    reachedNumber = true;
                }
            }
            return true;
        }

        /// <summary>
        /// This method verifies that the syntax of the passed formula follows the guidelines
        /// for a formula object written in standard infix notation as is defined in the object header.
        /// 
        /// Returns the normalized version of the formula if the syntax can be verified.
        /// Any variables will be normalized and validated by the normalize and isValid functions.
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        private static String VerifySyntax(string formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            if (formula == null)
                throw new FormulaFormatException("formula cannot be null. Check for null formula");
            int openingParenthesis = 0;
            int closingParenthesis = 0;
            int counter = 0;
            StringBuilder finalString = new StringBuilder();
            IEnumerable<string> tokens = GetTokens(formula);
            String lastToken = "";

            //Used to keep track of if a token follows an opening parenthesis or an operator.
            //Only a number variable or opening parenthesis can follow.
            bool followingParenthesis = false;

            //Used to keep track of if a token follows a number, a variable or a closing parenthesis.
            //Only an operator or a closing parenthesis can follow
            bool followingExtra = false;


            foreach (String token in tokens)
            {
                lastToken = token;
                //Checking the first token of the formula to be a number, variable or (
                if (counter == 0)
                {
                    if (token.IsOperator() || token.Equals(")"))
                    {
                        throw new FormulaFormatException("Non-valid starting character found. The formula " +
                            "must start with a number, a variable or an opening parenthesis");
                    }
                }
                counter++;

                if (!IsValidToken(token))
                {
                    throw new FormulaFormatException("A non-valid token was found, check the expression, " +
                        "valid tokens are: (, ), +, -, *, /, variables, and floating-point numbers");
                }

                
                else if (token.Equals("("))
                {
                    openingParenthesis++;
                    followingParenthesis = true;

                    if (followingExtra)
                    {
                        throw new FormulaFormatException("A ('' was found following a number, a variable or closing parenthesis." +
                            " Only operators or closing parenthesis can follow a number, variable or closing parenthesis.");
                    }
                }

                else if (token.Equals(")"))
                {
                    closingParenthesis++;
                    //Make sure that the parentheses are balanced
                    if(closingParenthesis > openingParenthesis)
                    {
                        throw new FormulaFormatException("Unequal parenthesis found. Check for equal " +
                            "numbers of closing and opening parentheses");
                    }

                    if (followingParenthesis)
                    {
                        throw new FormulaFormatException("An ')' was found following an operator/openinng parenthesis." +
                            "Only numbers, variables and opening parenthesis can follow operators/opening parentheses.");
                    }
                    followingExtra = true;
                }
                else if (token.IsOperator())
                {
                    if (followingParenthesis)
                    {
                        throw new FormulaFormatException("An opperator was found following an operator/openinng parenthesis. " +
                            "Only numbers, variables and opening parenthesis can follow operators/opening parentheses.");
                    }
                    else
                        followingParenthesis = true;

                    followingExtra = false;
                }
                else if (token.StartsAsVar())
                {
                    //Can be done because if the variable makes it this far, it has valid syntax
                    finalString.Append(normalize(token));
                    if (!isValid(token))
                    {
                        throw new FormulaFormatException("The variable did not pass the validator test. Check for " +
                            "proper syntax as defined by the passed validator or check the validator.");
                    }

                    followingParenthesis = false;
                    if (followingExtra)
                    {
                        throw new FormulaFormatException("A variable was found following a number, a variable or a " +
                            "closing parenthesis. Check for a variable immediately following one of those.");
                    }
                    else
                        followingExtra = true;

                    //Continue because the variable is already appended to the final string
                    continue;
                }

                //If the token is a number (the only option left)
                else
                {
                    followingParenthesis = false;
                    if (followingExtra)
                    {
                        throw new FormulaFormatException("A number was found following a number, a variable or a " +
                            "closing parenthesis. Check for a variable immediately following one of those.");
                    }
                    else
                        followingExtra = true;

                    //Normalize the number in case the number is in scientific notation or other forms
                    finalString.Append(Double.Parse(token));
                    continue;
                }

                finalString.Append(token);
            }

            if(lastToken.Equals(""))
            {
                throw new FormulaFormatException("No tokens found, the formula must contain at least 1 token.");
            }
            else if (lastToken.IsOperator() || lastToken.Equals("("))
            {
                throw new FormulaFormatException("A non-valid ending character was found. Check the expression" +
                    "for an operator or opening parenthesis at the end.");
            }

            return finalString.ToString();

        }

        /// <summary>
        /// Returns true if the given token is a (, ), +, -, *, /, number or variable.
        /// False otherwise.
        /// This method also verifies variable's syntax. Any variable with incorrect syntax
        /// that is passed will return false.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static bool IsValidToken(String token)
        {
            if (Double.TryParse(token, out double number))
                return true;
            else if (token.StartsAsVar())
            {
                if (!VerifyVariable(token))
                    return false;
                return true;
            }
            else if (token.IsOperator())
            {
                return true;
            }
            else if (token.Equals("(") || token.Equals(")"))
            {
                return true;
            }
            else
                return false;
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

    /// <summary>
    /// Adds two extension methods to the String class
    /// </summary>
    public static class PS3StringExtension
    {
        /// <summary>
        /// Returns true if the called string is an operator.
        /// An operator is defined as +, -, *, or /.
        /// False otherwise.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsOperator(this String str)
        {
            switch (str)
            {
                case ("*"):
                    return true;
                case ("/"):
                    return true;
                case ("+"):
                    return true;
                case ("-"):
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns true if the string starts as a variable should.
        /// A variable is defined in the object description. This method
        /// checks for the first character being _ or a letter.
        /// 
        /// False otherwise.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool StartsAsVar(this String str)
        {
            if(str[0].Equals('_') || Char.IsLetter(str[0]))
            {
                return true;
            }
            return false;
        }
    }
}
