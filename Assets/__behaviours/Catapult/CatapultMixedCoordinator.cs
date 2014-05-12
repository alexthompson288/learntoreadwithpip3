using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class CatapultMixedCoordinator : MonoBehaviour 
{
    [SerializeField]
    private string m_dataType;
    [SerializeField]
    private bool m_changeCurrentData;
    [SerializeField]
    private float m_probabilityTargetIsCorrect = 0.5f;
    [SerializeField]
    private int m_targetScore;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private Target[] m_targets;
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;
    [SerializeField]
    private bool m_areBallsSingleUse;
    [SerializeField]
    private bool m_isAnswerAlwaysCorrect;
    [SerializeField]
    private PictureDisplay m_pictureDisplay;
    [SerializeField]
    private bool m_targetsShowPicture;
    [SerializeField]
    private ImageBlackboard m_blackboard;

#if UNITY_EDITOR
    [SerializeField]
    private bool m_useDebugData;
#endif
    
    int m_score = 0;
    
    List<DataRow> m_dataPool = new List<DataRow>();
    DataRow m_currentData;
    
    Dictionary<DataRow, AudioClip> m_shortAudio = new Dictionary<DataRow, AudioClip>();
    Dictionary<DataRow, AudioClip> m_longAudio = new Dictionary<DataRow, AudioClip>();


    // If the dataType is "words" and targets show pictures then the "correct" answer is any picture that starts with the same phoneme as the target word

    
    IEnumerator Start()
    {
        m_pictureDisplay.SetShowPicture(!m_targetsShowPicture);

        m_scoreKeeper.SetTargetScore(m_targetScore);

        CatapultBehaviour cannonBehaviour = Object.FindObjectOfType(typeof(CatapultBehaviour)) as CatapultBehaviour;
        cannonBehaviour.MoveToMultiplayerLocation(0);

        m_probabilityTargetIsCorrect = Mathf.Clamp01(m_probabilityTargetIsCorrect);
        
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        if (System.String.IsNullOrEmpty(m_dataType))
        {
            m_dataType = GameManager.Instance.dataType;
        }
        
        m_dataPool = DataHelpers.GetData(m_dataType);

#if UNITY_EDITOR
        if(m_useDebugData)
        {
            if(m_dataType == "words")
            {
                m_dataPool = DataHelpers.GetSetData(3, "setwords", "words");
            }
        }
#endif

        m_dataPool = DataHelpers.OnlyPictureData(m_dataType, m_dataPool);
        
        if (m_dataPool.Count > 0)
        {
            foreach (DataRow data in m_dataPool)
            {
                if (m_dataType == "phonemes")
                {
                    m_shortAudio [data] = AudioBankManager.Instance.GetAudioClip(data["grapheme"].ToString());
                    m_longAudio [data] = LoaderHelpers.LoadMnemonic(data);
                }
                // If dataType is "words" and targets show pictures then the "correct" answer is any picture that starts with the same phoneme as the target word
                else if(m_dataType == "words" && m_targetsShowPicture)
                {
                    DataRow phonemeData = DataHelpers.GetFirstPhonemeInWord(data);

                    m_shortAudio [data] = AudioBankManager.Instance.GetAudioClip(phonemeData["grapheme"].ToString());
                    m_longAudio [data] = LoaderHelpers.LoadAudioForWord(data);
                    //m_longAudio [data] = LoaderHelpers.LoadMnemonic(phonemeData);
                }
                else
                {
                    m_shortAudio [data] = LoaderHelpers.LoadAudioForWord(data);
                }
            }
            
            m_currentData = DataHelpers.GetSingleTargetData(m_dataType);

            if(m_currentData == null)
            {
                m_currentData = m_dataPool[Random.Range(0, m_dataPool.Count)];
            }

            Debug.Log("m_currentData: " + m_currentData);

            // If dataType is "words" and targets show pictures then the "correct" answer is any picture that starts with the same phoneme as the target word
            if(m_dataType == "words" && m_targetsShowPicture)
            {
                // Picture display should only show the first phoneme of the target word
                m_pictureDisplay.On("phonemes", DataHelpers.GetFirstPhonemeInWord(m_currentData));
            }
            else
            {
                m_pictureDisplay.On(m_dataType, m_currentData);
            }
            
            InitializeTargets(Object.FindObjectsOfType(typeof(Target)) as Target[]);
            
            PlayShortAudio();
        } 
        else
        {
            StartCoroutine(OnGameComplete());
        }
    }
    
    void InitializeTargets(Target[] targets)
    {
        Debug.Log("targets.Length: " + targets.Length);

        foreach(Target target in targets)
        {
            target.SetShowPicture(m_targetsShowPicture);

            target.OnTargetHit += OnTargetHit;
            target.OnCompleteMove += SetTargetData;
            
            SetTargetData(target);
            
            StartCoroutine(target.On(Random.Range(1f, 4f)));
        }
    }

    bool IsDataCorrect(DataRow data)
    {
        if (m_isAnswerAlwaysCorrect)
        {
            return true;
        }
        else if (m_targetsShowPicture && m_dataType == "words")
        {
            return DataHelpers.WordsShareOnsetPhonemes(data, m_changeCurrentData);
        } 
        else
        {
            return data == m_currentData;
        }
    }
    
    void SetTargetData(Target target)
    {
        DataRow targetData = m_currentData;
        if (Random.Range(0f, 1f) > m_probabilityTargetIsCorrect)
        {
            while (IsDataCorrect(targetData))
            {
                targetData = m_dataPool [Random.Range(0, m_dataPool.Count)];
            }
        }
        
        target.SetData(targetData, m_dataType);
    }
    
    void OnTargetHit(Target target, Collider ball)
    {
        StartCoroutine(OnHitTargetCo(target, ball));
    }

    IEnumerator OnHitTargetCo(Target target, Collider ball)
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("CANNON_PLACEHOLDER_HIT_RANDOM");
        
        if (IsDataCorrect(target.data))
        {
            //WingroveAudio.WingroveRoot.Instance.PostEvent("SQUEAL_GAWP");

            ball.GetComponent<CatapultAmmo>().Explode();

            target.OnHit();
            
            GameObject targetDetachable = target.SpawnDetachable();

            StartCoroutine(m_scoreKeeper.UpdateScore(targetDetachable));
            
            target.ApplyHitForce(ball.transform);
            
            if(m_changeCurrentData)
            {
                m_currentData = m_dataPool[Random.Range(0, m_dataPool.Count)];
                PlayShortAudio();
            }

            if(m_targetsShowPicture)
            {
                PlayLongAudio(target.data);
                
                if(m_dataType == "words")
                {
                    yield return null;
                    
                    Texture2D tex = DataHelpers.GetPicture(m_dataType, target.data);
                    m_blackboard.ShowImage(tex, target.data["word"].ToString(), DataHelpers.GetFirstPhonemeInWord(target.data)["phoneme"].ToString(), "");
                }
            }

            m_score++;
            
            if(m_score >= m_targetScore)
            {
                StartCoroutine(OnGameComplete());
            }
        } 
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("HAPPY_GAWP");
            
            //target.Off();
            //StartCoroutine(target.On(Random.Range(0.5f, 1.5f)));
        }
        
        if (m_areBallsSingleUse)
        {
            ball.GetComponent<CatapultAmmo>().Explode();
        }

        yield break;
    }
    
    IEnumerator OnGameComplete()
    {
        yield return null;
    }
    
    void PlayLongAudio(DataRow data = null)
    {
        if (data == null)
        {
            data = m_currentData;
        }

        m_audioSource.clip = m_longAudio [data];
        if (m_audioSource.clip != null)
        {
            m_audioSource.Play();
        } 
        else
        {
            PlayShortAudio();
        }
    }
    
    void PlayShortAudio(DataRow data = null)
    {
        if (data == null)
        {
            data = m_currentData;
        }

        m_audioSource.clip = m_shortAudio [data];
        if (m_audioSource.clip != null)
        {
            m_audioSource.Play();
        }
    }

    void OnGUI()
    {
        GUILayout.Label("Score: " + m_score);
    }
}