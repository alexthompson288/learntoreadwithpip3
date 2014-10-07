using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ProgrammeInfo 
{
    static string m_basicReading = "Reading1";
    public static string basicReading
    {
        get
        {
            return m_basicReading;
        }
    }

    static string m_basicMaths = "Maths1";
    public static string basicMaths
    {
        get
        {
            return m_basicMaths;
        }
    }

    static string m_plusReading = "Reading2";
    public static string plusReading
    {
        get
        {
            return m_plusReading;
        }
    }

    static string m_plusMaths = "Maths2";
    public static string plusMaths
    {
        get
        {
            return m_plusMaths;
        }
    }

    private static string[] m_plusMathsGames = new string[] { "NewCompleteEquationNumbers", "NewClockNumbers", "NewMultiplicationQuadNumbers", "NewToyShopNumbers" };
    public static string[] GetPlusMathsGames()
    {
        return m_plusMathsGames;
    }
    
    private static string[] m_plusReadingGames = new string[] { "NewShoppingList", "NewCorrectWord", "NewPlusQuiz", "NewPlusSpelling" };
    public static string[] GetPlusReadingGames()
    {
        return m_plusReadingGames;
    }

    public static bool isBasic
    {
        get
        {
            return GameManager.Instance.programme == m_basicMaths || GameManager.Instance.programme == m_basicReading;
        }
    }
}
