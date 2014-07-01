using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

public static class StringHelpers
{
	public static string Edit (string s, string[] exemptions = null) 
	{
		/*
		string[] specialCharacters = { ".", ",", "?", ":", ";", "!", "£", "%", "$", "(", ")", "[", "]", "{", "}", " ", "\n" };
		foreach(string specialCharacter in specialCharacters)
		{
			if(exemptions == null || Array.IndexOf(exemptions, specialCharacter) == -1)
			{
				s = s.Replace(specialCharacter, "");
			}
		}

		return s;
		*/

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
}
