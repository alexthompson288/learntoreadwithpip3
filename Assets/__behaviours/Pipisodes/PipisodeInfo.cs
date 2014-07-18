using UnityEngine;
using System.Collections;

public class PipisodeInfo : Singleton<PipisodeInfo>
{
    DataRow m_currentPipisode = null;

    public void SetCurrentPipisode(DataRow myCurrentPipisode)
    {
        m_currentPipisode = myCurrentPipisode;
    }

    public DataRow GetCurrentPipisode()
    {
        return m_currentPipisode;
    }

    public bool HasBookmark()
    {
        return m_currentPipisode != null;
    }

    void Start () 
    {
        GameManager.Instance.Cancelling += OnGameCancel;
    }
    
    void OnGameCancel()
    {
        if(Application.loadedLevelName != "NewPipisodeMenu")
        {
            m_currentPipisode = null;
        }
    }
}
