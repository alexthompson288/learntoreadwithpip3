using UnityEngine;
using System.Collections;

public class GameMenuInfo : Singleton<GameMenuInfo> 
{
    Bookmark m_bookmark;
    
    class Bookmark
    {
        bool m_isTwoPlayer;
        ColorInfo.PipColor m_pipColor;
        string m_gameName;
        
        public Bookmark (bool isTwoPlayer, ColorInfo.PipColor pipColor)
        {
            m_isTwoPlayer = isTwoPlayer;
            m_pipColor = pipColor;
        }

        public Bookmark(string gameName, ColorInfo.PipColor pipColor)
        {
            m_gameName = gameName;
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

        public string GetGameName()
        {
            return m_gameName;
        }
    }

    public void CreateBookmark(bool isTwoPlayer, ColorInfo.PipColor pipColor)
    {
        m_bookmark = new Bookmark(isTwoPlayer, pipColor);
    }

    public void CreateBookmark(string gameName, ColorInfo.PipColor pipColor)
    {
        m_bookmark = new Bookmark(gameName, pipColor);
    }

    public void DestroyBookmark()
    {
        m_bookmark = null;
    }

    public bool HasBookmark()
    {
        return m_bookmark != null;
    }

    public bool IsTwoPlayer()
    {
        return m_bookmark != null ? m_bookmark.IsTwoPlayer() : false;
    }

    public ColorInfo.PipColor GetPipColor()
    {
        return m_bookmark != null ? m_bookmark.GetPipColor() : ColorInfo.PipColor.Pink;
    }

    public string GetGameName()
    {
        return m_bookmark != null ? m_bookmark.GetGameName() : "";
    }

    // Use this for initialization
    void Start () 
    {
        GameManager.Instance.OnCancel += OnGameCancel;
    }
    
    void OnGameCancel()
    {
        if(Application.loadedLevelName != "NewGameMenu")
        {
            m_bookmark = null;
        }
    }
}
