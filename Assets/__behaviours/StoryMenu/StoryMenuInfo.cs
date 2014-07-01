using UnityEngine;
using System.Collections;

public class StoryMenuInfo : Singleton<StoryMenuInfo> 
{
    class Bookmark
    {
        int m_storyId;
        ColorInfo.PipColor m_pipColor;

        public Bookmark(int storyId, ColorInfo.PipColor pipColor)
        {
            m_storyId = storyId;
            m_pipColor = pipColor;
        }

        public int GetStoryId()
        {
            return m_storyId;
        }

        public ColorInfo.PipColor GetPipColor()
        {
            return m_pipColor;
        }
    }

    Bookmark m_bookmark;

    public void CreateBookmark(int storyId, ColorInfo.PipColor pipColor)
    {
        m_bookmark = new Bookmark(storyId, pipColor);
    }

    public void DestroyBookmark()
    {
        m_bookmark = null;
    }

    public bool HasBookmark()
    {
        return m_bookmark != null;
    }

    public int GetBookmarkStoryId()
    {
        return m_bookmark != null ? m_bookmark.GetStoryId() : -1;
    }

    public ColorInfo.PipColor GetBookmarkPipColor()
    {
        return m_bookmark != null ? m_bookmark.GetPipColor() : ColorInfo.PipColor.Pink;
    }

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
        GameManager.Instance.Cancelling += OnGameCancel;
    }

    void OnGameCancel()
    {
        GameManager.Instance.CompletedAll -= OnGameComplete;
        m_startPipColor = ColorInfo.PipColor.Pink;

        if(Application.loadedLevelName != "NewStoryMenu")
        {
            m_bookmark = null;
        }
    }

    void OnGameComplete()
    {
        GameManager.Instance.CompletedAll -= OnGameComplete;
        m_startPipColor = ColorInfo.PipColor.Pink;
    }

    public void SubscribeGameComplete()
    {
        GameManager.Instance.CompletedAll += OnGameComplete;
    }
}
