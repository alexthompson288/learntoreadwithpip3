using UnityEngine;
using System.Collections;

public class ColorInfo : Singleton<ColorInfo> 
{
    [SerializeField]
    private Color m_lilac;
    [SerializeField]
    private Color m_pink;
    [SerializeField]
    private Color m_red; // Old (249, 24, 51)  // (255, 64, 79)
    [SerializeField]
    private Color m_yellow;
    [SerializeField]
    private Color m_blue;
    [SerializeField]
    private Color m_green;
    [SerializeField]
    private Color m_orange;
    [SerializeField]
    private Color m_turquoise; // (03, 209, 216)
    [SerializeField]
    private Color m_purple;
    [SerializeField]
    private Color m_gold;
    [SerializeField]
    private Color m_skyBlue;
    [SerializeField]
    private Color m_deepPink; //f577c1
    [SerializeField]
    private Color m_cream;
    [SerializeField]
    private Color m_orangeYellow;
    [SerializeField]
    private Color m_metalGrey;
    [SerializeField]
    private Color m_lightBlue;
    [SerializeField]
    private Color m_tricky;
    [SerializeField]
    private Color m_highFrequency;
    [SerializeField]
    private Color m_lightGreen;
    [SerializeField]
    private Color m_lightRed;

    public enum PipColor
    {
        //Lilac,
        Pink,
        Red,
        Yellow,
        Blue,
        Green,
        Orange,
        Turquoise,
        Purple,
        Gold,
        White,
        SkyBlue,
        DeepPink,
        Cream,
        OrangeYellow,
        MetalGrey,
        LightBlue,
        LightGreen,
        LightRed
    }

    public class NoColor : System.Exception {}

    // Methods are static to reduce typing required in other classes
    public static Color GetColor(PipColor pipColor)
    {
        switch(pipColor)
        {
            //case PipColor.Lilac:
                //return Instance.m_lilac;
                //break;
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
            case PipColor.Turquoise:
                return Instance.m_turquoise;
                break;
            case PipColor.Purple:
                return Instance.m_purple;
                break;
            case PipColor.Gold:
                return Instance.m_gold;
                break;
            case PipColor.White:
                return Color.white;
                break;
            case PipColor.SkyBlue:
                return Instance.m_skyBlue;
                break;
            case PipColor.DeepPink:
                return Instance.m_deepPink;
                break;
            case PipColor.Cream:
                return Instance.m_cream;
                break;
            case PipColor.OrangeYellow:
                return Instance.m_orangeYellow;
                break;
            case PipColor.MetalGrey:
                return Instance.m_metalGrey;
                break;
            case PipColor.LightBlue:
                return Instance.m_lightBlue;
                break;
            case PipColor.LightGreen:
                return Instance.m_lightGreen;
                break;
            case PipColor.LightRed:
                return Instance.m_lightRed;
                break;
            default:
                return Color.white;
                break;
        }
    }

    public static Color GetColor(string color)
    {
        return GetColor(GetPipColor(color));
    }

    public static PipColor GetPipColor(string color)
    {
        try
        {
            PipColor colorValue = (PipColor)System.Enum.Parse(typeof(PipColor), color);
            return colorValue;
        }
        catch
        {
            return PipColor.White;
        }
    }

    public static string GetColorString(PipColor pipColor)
    {
        return pipColor.ToString();
    }

    public static int GetColorIndex(string color)
    {
        int index = -1;

        try
        {
            index = (int)(GetPipColor(color));
        }
        catch
        {
            index = -1;
        }

        return index;
    }

    public static Color GetTricky()
    {
        return Instance.m_tricky;
    }

    public static Color GetHighFrequency()
    {
        return Instance.m_highFrequency;
    }
}
