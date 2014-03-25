using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class CannonCoopCoordinator : MonoBehaviour 
{
    [SerializeField]
    private GameDataBridge.DataType m_dataType;
    [SerializeField]
    private bool m_changeCurrentData;
    [SerializeField]
    private float m_probabilityTargetIsCurrent = 0.5f;
    [SerializeField]
    private int m_popTargetscore;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private Target[] m_popTargets;

    int m_score = 0;

    List<DataRow> m_dataPool = new List<DataRow>();
    DataRow m_currentData;
    
    Dictionary<DataRow, AudioClip> m_shortAudio = new Dictionary<DataRow, AudioClip>();
    Dictionary<DataRow, AudioClip> m_longAudio = new Dictionary<DataRow, AudioClip>();

    IEnumerator Start()
    {
        m_probabilityTargetIsCurrent = Mathf.Clamp01(m_probabilityTargetIsCurrent);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        switch (m_dataType)
        {
            case GameDataBridge.DataType.Letters:
                m_dataPool = GameDataBridge.Instance.GetLetters();
                break;
            case GameDataBridge.DataType.Words:
                m_dataPool = GameDataBridge.Instance.GetWords();
                break;
            case GameDataBridge.DataType.Keywords:
                m_dataPool = GameDataBridge.Instance.GetKeywords();
                break;
        }

        if (m_dataPool.Count > 0)
        {
            foreach (DataRow data in m_dataPool)
            {
                if (m_dataType == GameDataBridge.DataType.Letters)
                {
                    m_shortAudio [data] = AudioBankManager.Instance.GetAudioClip(data ["grapheme"].ToString());
                    m_longAudio [data] = LoaderHelpers.LoadMnemonic(data);
                } 
                else
                {
                    m_shortAudio [data] = LoaderHelpers.LoadAudioForWord(data);
                }
            }
            
            m_currentData = DataHelpers.FindTargetData(m_dataPool, m_dataType);

            foreach(Target target in m_popTargets)
            {
                target.OnTargetHit += OnTargetHit;
                target.OnCompleteMove += SetTargetData;

                SetTargetData(target);
            }
            
            PlayShortAudio();
        } 
        else
        {
            StartCoroutine(OnGameComplete());
        }
    }

    void SetTargetData(Target target)
    {
        DataRow targetData = m_currentData;
        if (Random.Range(0f, 1f) > m_probabilityTargetIsCurrent)
        {
            while (targetData == m_currentData)
            {
                targetData = m_dataPool [Random.Range(0, m_dataPool.Count)];
            }
        }

        target.SetData(targetData, m_dataType);
    }

    void OnTargetHit(Target target, Collider ball)
    {
        if (target.data == null)
        {
            target.ApplyHitForce(ball.transform);
        }
        else if (target.data == m_currentData)
        {
            target.Explode();
            m_currentData = m_dataPool[Random.Range(0, m_dataPool.Count)];
            PlayShortAudio();
            
            m_score++;
            
            if(m_score >= m_popTargetscore)
            {
                OnGameComplete();
            }
        } 
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("HAPPY_GAWP");
        }
        
        ball.GetComponent<CannonBall>().Explode();
    }

    IEnumerator OnGameComplete()
    {
        yield return null;
    }

    void PlayLongAudio()
    {
        m_audioSource.clip = m_longAudio [m_currentData];
        if (m_audioSource.clip != null)
        {
            m_audioSource.Play();
        } 
        else
        {
            PlayShortAudio();
        }
    }
    
    void PlayShortAudio()
    {
        m_audioSource.clip = m_shortAudio [m_currentData];
        if (m_audioSource.clip != null)
        {
            m_audioSource.Play();
        }
    }
}
