using UnityEngine;
using System.Collections;

public class PipColorWidgets : MonoBehaviour 
{
    public delegate void ClickEventHandler(PipColorWidgets colorBehaviour);
    public event ClickEventHandler Clicked;

    [SerializeField]
    private ColorInfo.PipColor m_color;
    [SerializeField]
    private UIWidget[] m_widgets;
    [SerializeField]
    private UILabel[] m_labels;

    public ColorInfo.PipColor color
    {
        get
        {
            return m_color;
        }
    }

	void Start()
    {
        RefreshWidgets();
    }

    void RefreshWidgets()
    {
        Color col = ColorInfo.GetColor(m_color);
        foreach (UIWidget widget in m_widgets)
        {
            widget.color = col;
        }
        
        string colString = ColorInfo.GetColorString(m_color);
        foreach (UILabel label in m_labels)
        {
            label.text = colString;
        }
    }

#if UNITY_EDITOR
    ColorInfo.PipColor m_lastPipColor;

    void Update()
    {
        if (m_color != m_lastPipColor)
        {
            RefreshWidgets();
        }

        m_lastPipColor = m_color;
    }
#endif

    void OnClick()
    {
        if (Clicked != null)
        {
            Clicked(this);
        }
    }
}
