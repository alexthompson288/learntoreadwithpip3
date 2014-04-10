using UnityEngine;
using System.Collections;

public class ColorInfo : Singleton<ColorInfo> 
{
    [SerializeField]
    private Color m_pink;

    public enum PipColor
    {
        Pink
    }

    public Color GetColor(PipColor pipColor)
    {
        switch(pipColor)
        {
            case PipColor.Pink:
                return m_pink;
                break;
            default:
                return Color.white;
                break;
        }
    }
}
