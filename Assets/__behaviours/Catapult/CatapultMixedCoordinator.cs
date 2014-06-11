using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System;

// IMPORTANT: If dataType == "words" and targetShowPictures == true, then m_currentData is a phoneme (not a word)
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
    private DataDisplay m_dataDisplay;
    [SerializeField]
    private bool m_targetsShowPicture;
    [SerializeField]
    private ImageBlackboard m_blackboard;
   
    float m_startTime;
    
    int m_score = 0;
    
    List<DataRow> m_dataPool = new List<DataRow>();
    DataRow m_currentData; 


    // If dataType == "words" and targetShowPictures == true, then m_currentData is a phoneme (not a word)
    public bool IsDataMixed()
    {
        return m_dataType == "words" && m_targetsShowPicture;
    }

    IEnumerator Start()
    {
        m_scoreKeeper.SetTargetScore(m_targetScore);

        m_probabilityTargetIsCorrect = Mathf.Clamp01(m_probabilityTargetIsCorrect);
        
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        DataRow game = DataHelpers.GetCurrentGame();
        if (game != null)
        {
            string gameType = game["gametype"] != null ? game["gametype"].ToString() : "";
            if(gameType.Contains("Phoneme"))
            {
                m_dataType = "phonemes";
            }
            else if(gameType.Contains("Word"))
            {
                m_dataType = "words";
            }

            if(game["picture_game"] != null)
            {
                m_targetsShowPicture = game["picture_game"].ToString() == "t";
            }
        }
        
        m_dataDisplay.SetShowPicture(!m_targetsShowPicture);

        if (System.String.IsNullOrEmpty(m_dataType))
        {
            m_dataType = GameManager.Instance.dataType;
        }
        
        m_dataPool = DataHelpers.GetData(m_dataType);
        m_dataPool = DataHelpers.OnlyPictureData(m_dataType, m_dataPool);
        
        if (m_dataPool.Count > 0)
        {
            if(IsDataMixed())
            {
                m_currentData = DataHelpers.GetSingleTargetData("phonemes");

                if(m_currentData == null)
                {
                    DataRow randomWord = m_dataPool[UnityEngine.Random.Range(0, m_dataPool.Count)];
                    m_currentData = DataHelpers.GetFirstPhonemeInWord(randomWord);
                }
            }
            else
            {
                m_currentData = DataHelpers.GetSingleTargetData(m_dataType, m_dataPool);
            }

            string currentDataType = m_dataType == "phonemes" || IsDataMixed() ? "phonemes" : "words";
            m_dataDisplay.On(currentDataType, m_currentData);

            
            InitializeTargets(UnityEngine.Object.FindObjectsOfType(typeof(Target)) as Target[]);
            
            PlayShortAudio(m_currentData);

            m_startTime = Time.time;
        } 
        else
        {
            StartCoroutine(OnGameComplete());
        }
    }
    
    void InitializeTargets(Target[] targets)
    {
        Debug.Log("targets.Length: " + targets.Length);

        for(int i = 0; i < targets.Length; ++i)
        {

            targets[i].SetShowPicture(m_targetsShowPicture);

            targets[i].OnTargetHit += OnTargetHit;
            targets[i].OnCompleteMove += SetTargetData;
            
            SetTargetData(targets[i]);
            
            float delay = i == 0 ? 0 : UnityEngine.Random.Range(1f, 4f);
            targets[i].On(delay);
        }
    }

    bool IsDataCorrect(DataRow data)
    {
        return IsDataMixed() ? m_currentData.Equals(DataHelpers.GetFirstPhonemeInWord(data)) : data.Equals(m_currentData);
    }

    DataRow FindRandomCorrect()
    {
        int currentId = m_currentData.GetId();

        List<DataRow> correctPool = m_dataPool.FindAll(x => DataHelpers.GetFirstPhonemeInWord(x).GetId() == currentId);

        return correctPool.Count > 0 ? correctPool[UnityEngine.Random.Range(0, correctPool.Count)] : m_dataPool[UnityEngine.Random.Range(0, correctPool.Count)];
    }
    
    void SetTargetData(Target target)
    {
        DataRow targetData = IsDataMixed() ? FindRandomCorrect() : m_currentData;
        if (UnityEngine.Random.Range(0f, 1f) > m_probabilityTargetIsCorrect)
        {
            while (IsDataCorrect(targetData))
            {
                targetData = m_dataPool [UnityEngine.Random.Range(0, m_dataPool.Count)];
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
            ball.GetComponent<CatapultAmmo>().Explode();

            target.OnHit();
            
            GameObject targetDetachable = target.SpawnDetachable();

            StartCoroutine(m_scoreKeeper.UpdateScore(targetDetachable));

            target.ApplyHitForce(ball.transform);

            if(IsDataMixed())
            {
                Texture2D tex = DataHelpers.GetPicture(m_dataType, target.data);
                StopCoroutine("HideBlackboard");
                m_blackboard.ShowImage(tex, target.data["word"].ToString(), DataHelpers.GetFirstPhonemeInWord(target.data)["phoneme"].ToString(), "");
                StartCoroutine("HideBlackboard");
            }

            if(!m_targetsShowPicture && m_dataType == "phonemes")
            {
                PlayShortAudio(target.data);
            }
            else
            {
                PlayLongAudio(target.data);
            }

            m_score++;
            
            if(m_score >= m_targetScore)
            {
                StartCoroutine(OnGameComplete());
            }
        } 

        yield break;
    }

    IEnumerator HideBlackboard()
    {
        yield return new WaitForSeconds(2f);
        m_blackboard.Hide();
    }
    
    IEnumerator OnGameComplete()
    {
        float timeTaken = Time.time - m_startTime;

        ScoreInfo.Instance.NewScore(m_score, m_targetScore, timeTaken, 20, 40);

        yield return StartCoroutine(m_scoreKeeper.On());

        GameManager.Instance.CompleteGame();
    }
    
    void PlayLongAudio(DataRow data)
    {
        string dataType = data ["word"] != null ? "words" : "phonemes";
        AudioClip clip = DataHelpers.GetLongAudio(dataType, data);
        
        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }
    }
    
    void PlayShortAudio(DataRow data)
    {
        string dataType = data ["word"] != null ? "words" : "phonemes";
        AudioClip clip = DataHelpers.GetShortAudio(dataType, data);

        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }
    }
}