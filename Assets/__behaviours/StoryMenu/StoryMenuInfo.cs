using UnityEngine;
using System.Collections;

public class StoryMenuInfo : Singleton<StoryMenuInfo> 
{
    ColorInfo.PipColor m_startPipColor = ColorInfo.PipColor.Pink;
    
    public void SetStartPipColor(ColorInfo.PipColor startPipColor)
    {
        m_startPipColor = startPipColor;
    }
    
    public ColorInfo.PipColor GetStartPipColor()
    {
        return m_startPipColor;
    }
    
    bool m_showText = true;

    public bool GetShowText()
    {
        return m_showText;
    }

    public void SetShowText(bool showText)
    {
        m_showText = showText;
    }

    void Start()
    {
        GameManager.Instance.OnCancel += OnGameCancel;
    }

    void OnGameCancel()
    {
        GameManager.Instance.OnComplete -= OnGameComplete;
        m_startPipColor = ColorInfo.PipColor.Pink;
    }

    void OnGameComplete()
    {
        GameManager.Instance.OnComplete -= OnGameComplete;
        m_startPipColor = ColorInfo.PipColor.Pink;
    }

    public void SubscribeGameComplete()
    {
        GameManager.Instance.OnComplete += OnGameComplete;
    }
}
