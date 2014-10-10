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

    static string m_voyage = "Voyage";
    public static string voyage
    {
        get
        {
            return m_voyage;
        }
    }

    static string m_progress = "Progress";
    public static string progress
    {
        get
        {
            return m_progress;
        }
    }

    static string m_story = "Story";
    public static string story
    {
        get
        {
            return m_story;
        }
    }

    static string m_storyQuiz = "StoryQuiz";
    public static string storyQuiz
    {
        get
        {
            return m_storyQuiz;
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

    private static ColorInfo.PipColor[] m_basicColors = new ColorInfo.PipColor[] 
    {
        ColorInfo.PipColor.Pink,
        ColorInfo.PipColor.Red,
        ColorInfo.PipColor.Yellow,
        ColorInfo.PipColor.Blue,
        ColorInfo.PipColor.Green,
        ColorInfo.PipColor.Orange
    };
    public static ColorInfo.PipColor[] basicColors
    {
        get
        {
            return m_basicColors;
        }
    }

    private static ColorInfo.PipColor[] m_plusColors = new ColorInfo.PipColor[] 
    {
        ColorInfo.PipColor.Turquoise,
        ColorInfo.PipColor.Purple,
        ColorInfo.PipColor.Gold,
        ColorInfo.PipColor.White
    };
    public static ColorInfo.PipColor[] plusColors
    {
        get
        {
            return m_basicColors;
        }
    }
}
