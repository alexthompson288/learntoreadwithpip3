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

    public enum PipColor
    {
        Pink,
        Red,
        Yellow,
        Blue
    }

    public Color GetColor(string color)
    {
        switch (color)
        {
            case "Pink":
                return m_pink;
                break;
            case "Red":
                return m_red;
                break;
            case "Yellow":
                return m_yellow;
                break;
            case "Blue":
                return m_blue;
                break;
            default:
                return Color.white;
                break;
        }
    }

    public Color GetColor(PipColor pipColor)
    {
        switch(pipColor)
        {
            case PipColor.Pink:
                return m_pink;
                break;
            case PipColor.Red:
                return m_red;
                break;
            case PipColor.Yellow:
                return m_yellow;
                break;
            case PipColor.Blue:
                return m_blue;
                break;
            default:
                return Color.white;
                break;
        }
    }
}
