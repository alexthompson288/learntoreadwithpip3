using UnityEngine;
using System.Collections;

public class GameMenuInfo : Singleton<GameMenuInfo> 
{
    Bookmark m_bookmark;
    
    class Bookmark
    {
        ColorInfo.PipColor m_pipColor;
        string m_gameName;

        public Bookmark(string myGameName, ColorInfo.PipColor myPipColor)
        {
            m_gameName = myGameName;
            m_pipColor = myPipColor;
        }

        public Bookmark(ColorInfo.PipColor myPipColor)
        {
            m_pipColor = myPipColor;
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

    public void CreateBookmark(ColorInfo.PipColor pipColor)
    {
        m_bookmark = new Bookmark(pipColor);
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
        GameManager.Instance.Cancelling += OnGameCancel;
    }
    
    void OnGameCancel()
    {
        if(Application.loadedLevelName != "NewGameMenu")
        {
            m_bookmark = null;
        }
    }
}
