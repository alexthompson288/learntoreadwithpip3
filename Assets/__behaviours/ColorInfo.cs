using UnityEngine;
using System.Collections;

public class ColorInfo : Singleton<ColorInfo> 
{
    [SerializeField]
    private Color m_pink;
    [SerializeField]
    private Color m_red;
    [SerializeField]
    private Color m_yellow;
    [SerializeField]
    private Color m_blue;
    [SerializeField]
    private Color m_green;
    [SerializeField]
    private Color m_orange;

    public enum PipColor
    {
        Pink,
        Red,
        Yellow,
        Blue,
        Green,
        Orange
    }

    // Methods are static to reduce typing required in other classes

    public static Color GetColor(string color)
    {
        switch (color)
        {
            case "Pink":
                return Instance.m_pink;
                break;
            case "Red":
                return Instance.m_red;
                break;
            case "Yellow":
                return Instance.m_yellow;
                break;
            case "Blue":
                return Instance.m_blue;
                break;
            case "Green":
                return Instance.m_green;
                break;
            case "Orange":
                return Instance.m_orange;
                break;
            default:
                return Color.white;
                break;
        }
    }

    public static Color GetColor(PipColor pipColor)
    {
        switch(pipColor)
        {
            case PipColor.Pink:
                return Instance.m_pink;
                break;
            case PipColor.Red:
                return Instance.m_red;
                break;
            case PipColor.Yellow:
                return Instance.m_yellow;
                break;
            case PipColor.Blue:
                return Instance.m_blue;
                break;
            case PipColor.Green:
                return Instance.m_green;
                break;
            case PipColor.Orange:
                return Instance.m_orange;
                break;
            default:
                return Color.white;
                break;
        }
    }

    // Used for stories where strings are used because we may have other options (eg. Classics)
    public static string GetColorString(PipColor pipColor)
    {
        switch(pipColor)
        {
            case PipColor.Pink:
                return "Pink";
                break;
            case PipColor.Red:
                return "Red";
                break;
            case PipColor.Yellow:
                return "Yellow";
                break;
            case PipColor.Blue:
                return "Blue";
                break;
            case PipColor.Green:
                return "Green";
                break;
            case PipColor.Orange:
                return "Orange";
                break;
            default:
                return "Pink";
                break;
        }
    }
}
