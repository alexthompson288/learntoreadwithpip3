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
    private AudioClip m_defaultInstructions;
    [SerializeField]
    private AudioClip m_mixedDataInstructions;
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
    [SerializeField]
    private CatapultBehaviour m_catapult;
   
    float m_startTime;
    
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
            m_dataType = "words";
        }
        
        m_dataPool = DataHelpers.GetData(m_dataType);

        if (IsDataMixed())
        {
            m_dataPool.AddRange(DataHelpers.GetNonReadableWords());
            m_dataPool = DataHelpers.OnlyOrderedPhonemes(m_dataPool);
        }

        m_dataPool = DataHelpers.OnlyPictureData(m_dataPool);

        Target[] targets = UnityEngine.Object.FindObjectsOfType(typeof(Target)) as Target[];

        foreach (Target target in targets)
        {
            target.SetShowPicture(m_targetsShowPicture);
        }

        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());

        m_audioSource.clip = IsDataMixed() ? m_mixedDataInstructions : m_defaultInstructions;
        m_audioSource.Play();

        while (m_audioSource.isPlaying)
        {
            yield return null;
        }

        if (m_dataPool.Count > 0)
        {
            if(IsDataMixed())
            {
                m_currentData = DataHelpers.GetSingleTargetData("phonemes");

                if(m_currentData == null)
                {
                    DataRow randomWord = m_dataPool[UnityEngine.Random.Range(0, m_dataPool.Count)];
                    m_currentData = DataHelpers.GetOnsetPhoneme(randomWord);
                }

                if(!DataHelpers.HasOnsetWords(m_currentData, m_dataPool))
                {
                    m_dataPool.AddRange(DataHelpers.GetOnsetWords(m_currentData, 3, true));
                    m_dataPool = DataHelpers.OnlyPictureData(m_dataPool);
                }

                while(!DataHelpers.HasOnsetWords(m_currentData, m_dataPool))
                {
                    DataRow randomWord = m_dataPool[UnityEngine.Random.Range(0, m_dataPool.Count)];
                    m_currentData = DataHelpers.GetOnsetPhoneme(randomWord);

                    m_dataPool.AddRange(DataHelpers.GetOnsetWords(m_currentData, 3, true));
                    m_dataPool = DataHelpers.OnlyPictureData(m_dataPool);

                    yield return null;
                }
            }
            else
            {
                m_currentData = DataHelpers.GetSingleTargetData(m_dataType, m_dataPool);
            }

            string currentDataType = m_dataType == "phonemes" || IsDataMixed() ? "phonemes" : "words";
            m_dataDisplay.On(currentDataType, m_currentData);

            InitializeTargets(targets);
            
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
        for(int i = 0; i < targets.Length; ++i)
        {
            targets[i].TargetHit += OnTargetHit;
            targets[i].MoveCompleted += SetTargetData;
            
            SetTargetData(targets[i]);
            
            float delay = i == 0 ? 0 : UnityEngine.Random.Range(1f, 4f);
            targets[i].On(delay);
        }
    }

    bool IsDataCorrect(DataRow data)
    {
        return IsDataMixed() ? m_currentData.Equals(DataHelpers.GetOnsetPhoneme(data)) : data.Equals(m_currentData);
    }

    DataRow FindRandomCorrect()
    {
        int currentId = m_currentData.GetId();

        List<DataRow> correctPool = m_dataPool.FindAll(x => DataHelpers.GetOnsetPhoneme(x).GetId() == currentId);

        return correctPool.Count > 0 ? correctPool[UnityEngine.Random.Range(0, correctPool.Count)] : m_dataPool[UnityEngine.Random.Range(0, correctPool.Count)];
    }
    
    void SetTargetData(Target target)
    {
        DataRow targetData = IsDataMixed() ? FindRandomCorrect() : m_currentData;
        if (UnityEngine.Random.Range(0f, 1f) > m_probabilityTargetIsCorrect && m_dataPool.Count > 1)
        {
            int safetyCounter = 0;
            while (IsDataCorrect(targetData) && safetyCounter < 150)
            {
                targetData = m_dataPool [UnityEngine.Random.Range(0, m_dataPool.Count)];
                ++safetyCounter;
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

        ball.GetComponent<CatapultAmmo>().Explode();
        ball.GetComponent<CatapultAmmo>().StopMomentum();

        if (!m_scoreKeeper.HasCompleted())
        {
            if (IsDataCorrect(target.data))
            {
                target.OnHit();
                
                //GameObject targetDetachable = target.SpawnDetachable();

                //StartCoroutine(m_scoreKeeper.UpdateScore(targetDetachable));
                m_scoreKeeper.UpdateScore(1);

                target.ApplyHitForce(ball.transform);

                if (IsDataMixed())
                {
                    Texture2D tex = DataHelpers.GetPicture(target.data);
                    StopCoroutine("HideBlackboard");
                    m_blackboard.ShowImage(tex, target.data ["word"].ToString(), DataHelpers.GetOnsetPhoneme(target.data) ["phoneme"].ToString(), "");
                    StartCoroutine("HideBlackboard");
                }

                if (!m_targetsShowPicture && m_dataType == "phonemes")
                {
                    PlayShortAudio(target.data);
                } else
                {
                    PlayLongAudio(target.data);
                }
                
                if (m_scoreKeeper.HasCompleted())
                {
                    StartCoroutine(OnGameComplete());
                }
            } 
            else
            {
                m_scoreKeeper.UpdateScore(-1);

                if(m_targetsShowPicture)
                {
                    target.ShowLabel();
                }
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
        m_catapult.Off();

        float timeTaken = Time.time - m_startTime;

        float twoStarPerQuestion = 4f;
        float threeStarPerQuestion = 3f;

        int stars = ScoreInfo.CalculateTimeStars(timeTaken, twoStarPerQuestion * (float)m_targetScore, threeStarPerQuestion * (float)m_targetScore);

        ScoreInfo.Instance.NewScore(timeTaken, m_scoreKeeper.GetScore(), m_targetScore, stars);

        yield return StartCoroutine(m_scoreKeeper.On());

        GameManager.Instance.CompleteGame();
    }
    
    void PlayLongAudio(DataRow data)
    {
        string dataType = data ["word"] != null ? "words" : "phonemes";
        AudioClip clip = DataHelpers.GetLongAudio(data);
        
        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }
    }
    
    void PlayShortAudio(DataRow data)
    {
        string dataType = data ["word"] != null ? "words" : "phonemes";
        AudioClip clip = DataHelpers.GetShortAudio(data);

        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }
    }
}