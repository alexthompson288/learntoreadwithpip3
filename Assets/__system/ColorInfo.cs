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
    [SerializeField]
    private Color m_lightBlue;
    [SerializeField]
    private Color m_deepPink; //f577c1
    [SerializeField]
    private Color m_cream;
    [SerializeField]
    private Color m_tricky;
    [SerializeField]
    private Color m_highFrequency;

    public enum PipColor
    {
        Pink,
        Red,
        Yellow,
        Blue,
        Green,
        Orange,
        White,
        LightBlue,
        DeepPink,
        Cream
    }

    public class NoColor : System.Exception {}

    public static IEnumerator WaitForInstance()
    {
        while (ColorInfo.Instance == null)
        {
            yield return null;
        }
    }

    // Methods are static to reduce typing required in other classes
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
            case PipColor.White:
                return Color.white;
                break;
            case PipColor.LightBlue:
                return Instance.m_lightBlue;
                break;
            case PipColor.DeepPink:
                return Instance.m_deepPink;
                break;
            case PipColor.Cream:
                return Instance.m_cream;
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
        switch (color)
        {
            case "Pink":
                return PipColor.Pink;
                break;
            case "Red":
                return PipColor.Red;
                break;
            case "Yellow":
                return PipColor.Yellow;
                break;
            case "Blue":
                return PipColor.Blue;
                break;
            case "Green":
                return PipColor.Green;
                break;
            case "Orange":
                return PipColor.Orange;
                break;
            case "White":
                return PipColor.White;
                break;
            case "LightBlue":
                return PipColor.LightBlue;
                break;
            case "DeepPink":
                return PipColor.DeepPink;
                break;
            case "Cream":
                return PipColor.Cream;
                break;
            default:
                Debug.LogError("No color found for: " + color);
                throw new NoColor();
                break;
        }
    }

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
            case PipColor.White:
                return "White";
                break;
            case PipColor.LightBlue:
                return "LightBlue";
                break;
            case PipColor.DeepPink:
                return "DeepPink";
            case PipColor.Cream:
                return "Cream";
                break;
            default:
                return "White";
                break;
        }
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
