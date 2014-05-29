using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameCoordinator : Singleton<GameCoordinator> 
{
    [SerializeField]
    protected ScoreKeeper m_scoreKeeper;
    [SerializeField]
    protected int m_targetScore;
    [SerializeField]
    protected AudioSource m_audioSource;
    
    protected int m_score = 0;
    protected int m_numAnswered = 0;
    
    protected List<DataRow> m_dataPool = new List<DataRow>();

    protected DataRow m_currentData = null;

    protected Dictionary<DataRow, AudioClip> m_longAudio = new Dictionary<DataRow, AudioClip>();
    protected Dictionary<DataRow, AudioClip> m_shortAudio = new Dictionary<DataRow, AudioClip>();

    protected void ClampTargetScore()
    {
        m_targetScore = Mathf.Min(m_targetScore, m_dataPool.Count);
    }

    protected virtual IEnumerator CompleteGame()
    {
        yield return new WaitForSeconds(2f);

        GameManager.Instance.CompleteGame();
    }

    protected DataRow GetRandomData()
    {
        return m_dataPool.Count > 0 ? m_dataPool[Random.Range(0, m_dataPool.Count)] : null;
    }

    public bool PlayShortAudio(DataRow data = null, bool defaultToCurrent = true)
    {
        return PlayAudio(data, m_shortAudio, defaultToCurrent);
    }

    public bool PlayLongAudio(DataRow data = null, bool defaultToCurrent = true)
    {
        return PlayAudio(data, m_longAudio, defaultToCurrent);
    }

    bool PlayAudio(DataRow data, Dictionary<DataRow, AudioClip> audioDictionary, bool defaultToCurrent)
    {
        bool hasPlayed = false; 
        
        if (data == null && defaultToCurrent)
        {
            data = m_currentData;
        }

        if (data != null)
        {
            if (audioDictionary.ContainsKey(data) && m_audioSource != null)
            {
                m_audioSource.clip = audioDictionary [data];
                
                if (m_audioSource.clip != null)
                {
                    m_audioSource.Play();
                    hasPlayed = true;
                }
            }
        }
        
        return hasPlayed;
    }
}
