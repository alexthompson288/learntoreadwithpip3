using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

public class StringHelpers
{
    static System.Random m_random = new System.Random();

    public static char GetRandomLetter()
    {
        int num = m_random.Next(0, 26); // Zero to 25
        return (char)('a' + num);
    }

	public static string Edit (string s, string[] exemptions = null) 
	{
		List<string> exemptionList = new List<string>();

		if(exemptions != null)
		{
			exemptionList = exemptions.ToList();
		}

		StringBuilder sb = new StringBuilder(s.Length);
		
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || exemptionList.Contains(c.ToString()))
				sb.Append(s[i]);
		}

		return sb.ToString();
	}

    static string[] ones = new string[] { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
    static string[] teens = new string[] { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
    static string[] tens = new string[] { "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
    static string[] thousandsGroups = { "", " Thousand", " Million", " Billion" };
    
    private static string FriendlyInteger(int n, string leftDigits, int thousands)
    {
        if (n == 0)
        {
            return leftDigits;
        }
        string friendlyInt = leftDigits;
        if (friendlyInt.Length > 0)
        {
            friendlyInt += " ";
        }
        
        if (n < 10)
        {
            friendlyInt += ones[n];
        }
        else if (n < 20)
        {
            friendlyInt += teens[n - 10];
        }
        else if (n < 100)
        {
            friendlyInt += FriendlyInteger(n % 10, tens[n / 10 - 2], 0);
        }
        else if (n < 1000)
        {
            friendlyInt += FriendlyInteger(n % 100, (ones[n / 100] + " Hundred"), 0);
        }
        else
        {
            friendlyInt += FriendlyInteger(n % 1000, FriendlyInteger(n / 1000, "", thousands+1), 0);
        }
        
        return friendlyInt + thousandsGroups[thousands];
    }
    
    public static string IntegerToWritten(int n)
    {
        if (n == 0)
        {
            return "Zero";
        }
        else if (n < 0)
        {
            return "Negative " + IntegerToWritten(-n);
        }

        return FriendlyInteger(n, "", 0);
    }

    public static string CollectionToString<T>(IList<T> list, char separator = ',')
    {
        string s = "";

        try
        {
            foreach(T t in list)
            {
                s += t.ToString() + separator.ToString();
            }

            s = s.TrimEnd(new char[] { separator } );
        }
        catch
        {
            throw;
        }

        return s;
    }

    public static List<T> StringToList<T>(string unsplit, char separator = ',')
    {
        string[] stringArray = unsplit.Split(new char[] { separator });

        List<T> list = new List<T>();

        try
        {
            foreach (string s in stringArray)
            {
                list.Add((T)Convert.ChangeType(s, typeof(T)));
            }
        }
        catch
        {
            throw;
        }

        return list;
    }

    public static string Reverse(string toReverse)
    {
        char[] arr = toReverse.ToCharArray();
        System.Array.Reverse(arr);
        return new string(arr);
    }

    public static float Evalf(string s)
    {
        StringToFormula stf = new StringToFormula();
        return (float)stf.Eval(s);
    }

    public static double Evald(string s)
    {
        StringToFormula stf = new StringToFormula();
        return stf.Eval(s);
    }

    public class StringToFormula
    {
        private string[] _operators = { "-", "+", "/", "*", "^", "$", "%"};
        private  Func<double, double, double>[] _operations = {
            (a1, a2) => a1 - a2,
            (a1, a2) => a1 + a2,
            (a1, a2) => a1 / a2,
            (a1, a2) => a1 * a2,
            (a1, a2) => Math.Pow(a1, a2),
            (a1, a2) => Math.Pow(a1, 1 / a2),
            (a1, a2) => a1 % a2,
        };
        
        public double Eval(string expression)
        {
            List<string> tokens = getTokens(expression);
            Stack<double> operandStack = new Stack<double>();
            Stack<string> operatorStack = new Stack<string>();
            int tokenIndex = 0;
            
            while (tokenIndex < tokens.Count) {
                string token = tokens[tokenIndex];
                if (token == "(") {
                    string subExpr = getSubExpression(tokens, ref tokenIndex);
                    operandStack.Push(Eval(subExpr));
                    continue;
                }
                if (token == ")") {
                    throw new ArgumentException("Mis-matched parentheses in expression");
                }
                //If this is an operator  
                if (Array.IndexOf(_operators, token) >= 0) {
                    while (operatorStack.Count > 0 && Array.IndexOf(_operators, token) < Array.IndexOf(_operators, operatorStack.Peek())) {
                        string op = operatorStack.Pop();
                        double arg2 = operandStack.Pop();
                        double arg1 = operandStack.Pop();
                        operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
                    }
                    operatorStack.Push(token);
                } else {
                    operandStack.Push(double.Parse(token));
                }
                tokenIndex += 1;
            }
            
            while (operatorStack.Count > 0) {
                string op = operatorStack.Pop();
                double arg2 = operandStack.Pop();
                double arg1 = operandStack.Pop();
                operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
            }
            return operandStack.Pop();
        }
        
        private string getSubExpression(List<string> tokens, ref int index)
        {
            StringBuilder subExpr = new StringBuilder();
            int parenlevels = 1;
            index += 1;
            while (index < tokens.Count && parenlevels > 0) {
                string token = tokens[index];
                if (tokens[index] == "(") {
                    parenlevels += 1;
                }
                
                if (tokens[index] == ")") {
                    parenlevels -= 1;
                }
                
                if (parenlevels > 0) {
                    subExpr.Append(token);
                }
                
                index += 1;
            }
            
            if ((parenlevels > 0)) {
                throw new ArgumentException("Mis-matched parentheses in expression");
            }
            return subExpr.ToString();
        }
        
        private List<string> getTokens(string expression)
        {
            string operators = "()^*/+-";
            List<string> tokens = new List<string>();
            StringBuilder sb = new StringBuilder();
            
            foreach (char c in expression.Replace(" ", string.Empty)) {
                if (operators.IndexOf(c) >= 0) {
                    if ((sb.Length > 0)) {
                        tokens.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    tokens.Add(c.ToString());
                } else {
                    sb.Append(c);
                }
            }
            
            if ((sb.Length > 0)) {
                tokens.Add(sb.ToString());
            }
            return tokens;
        }
    }
}
