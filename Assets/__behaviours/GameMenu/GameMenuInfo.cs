using UnityEngine;
using System.Collections;

public class GameMenuInfo : Singleton<GameMenuInfo> 
{
    Bookmark m_bookmark;
    
    class Bookmark
    {
        bool m_isTwoPlayer;
        ColorInfo.PipColor m_pipColor;
        
        public Bookmark (bool isTwoPlayer, ColorInfo.PipColor pipColor)
        {
            m_isTwoPlayer = isTwoPlayer;
            m_pipColor = pipColor;
        }
        
        public bool IsTwoPlayer()
        {
            return m_isTwoPlayer;
        }
        
        public ColorInfo.PipColor GetPipColor()
        {
            return m_pipColor;
        }
    }

    public void CreateBookmark(bool isTwoPlayer, ColorInfo.PipColor pipColor)
    {
        m_bookmark = new Bookmark(isTwoPlayer, pipColor);
    }

    public bool HasBookmark()
    {
        return m_bookmark != null;
    }

    public bool IsBookmarkTwoPlayer()
    {
        return m_bookmark != null ? m_bookmark.IsTwoPlayer() : false;
    }

    public ColorInfo.PipColor GetBookmarkPipColor()
    {
        return m_bookmark != null ? m_bookmark.GetPipColor() : ColorInfo.PipColor.Pink;
    }

    // Use this for initialization
    void Start () 
    {
        GameManager.Instance.OnCancel += OnGameCancel;
    }
    
    void OnGameCancel()
    {
        m_bookmark = null;
    }
}
