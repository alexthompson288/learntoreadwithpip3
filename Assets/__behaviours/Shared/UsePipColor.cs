using UnityEngine;
using System.Collections;

public class UsePipColor : MonoBehaviour 
{
    [SerializeField]
    private ColorInfo.PipColor m_color;
    [SerializeField]
    private UIWidget[] m_widgets;

	void Start()
    {
        Color col = ColorInfo.GetColor(m_color);

        foreach (UIWidget widget in m_widgets)
        {
            widget.color = col;
        }
    }
}
