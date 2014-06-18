using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class CatapultCoopCoordinator : MonoBehaviour 
{
    [SerializeField]
    private string m_dataType;
    [SerializeField]
    private bool m_changeCurrentData;
    [SerializeField]
    private float m_probabilityTargetIsCurrent = 0.5f;
    [SerializeField]
    private int m_targetScore;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private Target[] m_leftTargets;
    [SerializeField]
    private Target[] m_rightTargets;
    [SerializeField]
    private Target[] m_topTargets;

    int m_score = 0;

    List<DataRow> m_dataPool = new List<DataRow>();
    DataRow m_currentData;
    
    Dictionary<DataRow, AudioClip> m_shortAudio = new Dictionary<DataRow, AudioClip>();
    Dictionary<DataRow, AudioClip> m_longAudio = new Dictionary<DataRow, AudioClip>();

    void Awake()
    {
        float targetOffDistance = 500;

        foreach (Target target in m_leftTargets)
        {
            target.SetOffPosition(Vector3.left, targetOffDistance);
        }

        foreach (Target target in m_rightTargets)
        {
            target.SetOffPosition(Vector3.right, targetOffDistance);
        }

        foreach (Target target in m_topTargets)
        {
            target.SetOffPosition(Vector3.up, targetOffDistance);
        }
    }

    IEnumerator Start()
    {
        CatapultBehaviour[] cannons = Object.FindObjectsOfType(typeof(CatapultBehaviour)) as CatapultBehaviour[];
        for (int i = 0; i < cannons.Length; ++i)
        {
            cannons[i].MoveToMultiplayerLocation(i);
        }

        m_probabilityTargetIsCurrent = Mathf.Clamp01(m_probabilityTargetIsCurrent);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        if (System.String.IsNullOrEmpty(m_dataType))
        {
            //m_dataType = GameManager.Instance.dataType;
            m_dataType = "words";
        }

        m_dataPool = DataHelpers.GetData(m_dataType);

        if (m_dataPool.Count > 0)
        {
            foreach (DataRow data in m_dataPool)
            {
                if (m_dataType == "phonemes")
                {
                    m_shortAudio [data] = AudioBankManager.Instance.GetAudioClip(data ["grapheme"].ToString());
                    m_longAudio [data] = LoaderHelpers.LoadMnemonic(data);
                } 
                else
                {
                    m_shortAudio [data] = LoaderHelpers.LoadAudioForWord(data);
                }
            }
            
            m_currentData = DataHelpers.GetSingleTargetData(m_dataType);

            InitializeTargets(m_leftTargets);
            InitializeTargets(m_rightTargets);
            InitializeTargets(m_topTargets);
            
            PlayShortAudio();
        } 
        else
        {
            StartCoroutine(OnGameComplete());
        }
    }

    void InitializeTargets(Target[] targets)
    {
        foreach(Target target in targets)
        {
            target.OnTargetHit += OnTargetHit;
            target.OnCompleteMove += SetTargetData;
            
            SetTargetData(target);

            target.On(Random.Range(1f, 4f));
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
        if (target.data == m_currentData)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("GAWP_SQUEAL");

            target.ApplyHitForce(ball.transform);

            if(m_changeCurrentData)
            {
                m_currentData = m_dataPool[Random.Range(0, m_dataPool.Count)];
                PlayShortAudio();
            }
            
            m_score++;
            
            if(m_score >= m_targetScore)
            {
                OnGameComplete();
            }
        } 
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("GAWP_HAPPY");

            target.Off();
            target.On(Random.Range(0.5f, 1.5f));
        }
        
        ball.GetComponent<CatapultAmmo>().Explode();
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
