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

}
