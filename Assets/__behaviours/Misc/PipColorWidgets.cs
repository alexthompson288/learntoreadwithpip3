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
        if (m_widgets != null)
        {
            Color col = ColorInfo.GetColor(m_color);
            for (int i = 0; i < m_widgets.Length; ++i)
            {
                m_widgets [i].color = col;
            }
        }

        if (m_labels != null)
        {
            string colString = ColorInfo.GetColorString(m_color);
            for (int i = 0; i < m_labels.Length; ++i)
            {
                m_labels [i].text = colString;
            }
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
