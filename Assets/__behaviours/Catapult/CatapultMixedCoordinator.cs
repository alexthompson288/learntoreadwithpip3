using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System;

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
   
    
    int m_score = 0;
    
    List<DataRow> m_dataPool = new List<DataRow>();
    DataRow m_currentData;
    
    Dictionary<int, AudioClip> m_shortAudio = new Dictionary<int, AudioClip>();
    Dictionary<int, AudioClip> m_longAudio = new Dictionary<int, AudioClip>();


    // If the dataType is "words" and targets show pictures then the "correct" answer is any picture that starts with the same phoneme as the target word

    
    IEnumerator Start()
    {
        m_scoreKeeper.SetTargetScore(m_targetScore);

        CatapultBehaviour cannonBehaviour = UnityEngine.Object.FindObjectOfType(typeof(CatapultBehaviour)) as CatapultBehaviour;
        cannonBehaviour.MoveToMultiplayerLocation(0);

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
        
        m_pictureDisplay.SetShowPicture(!m_targetsShowPicture);

        if (System.String.IsNullOrEmpty(m_dataType))
        {
            m_dataType = GameManager.Instance.dataType;
        }
        
        m_dataPool = DataHelpers.GetData(m_dataType);

#if UNITY_EDITOR
        if(m_dataPool.Count == 0)
        {
            int pinkModuleId = DataHelpers.GetModuleId(ColorInfo.PipColor.Pink);

            if(m_dataType == "words")
            {
                m_dataPool = DataHelpers.GetModuleWords(pinkModuleId);
            }
            else if(m_dataType == "phonemes")
            {
                m_dataPool = DataHelpers.GetModulePhonemes(pinkModuleId);
            }
        }
#endif

        m_dataPool = DataHelpers.OnlyPictureData(m_dataType, m_dataPool);
        
        if (m_dataPool.Count > 0)
        {
            foreach (DataRow data in m_dataPool)
            {
                int dataId = Convert.ToInt32(data["id"]);

                if (m_dataType == "phonemes")
                {
                    m_shortAudio [dataId] = AudioBankManager.Instance.GetAudioClip(data["grapheme"].ToString());
                    m_longAudio [dataId] = LoaderHelpers.LoadMnemonic(data);
                }
                // If dataType is "words" and targets show pictures then the "correct" answer is any picture that starts with the same phoneme as the target word
                else if(m_dataType == "words" && m_targetsShowPicture)
                {
                    DataRow phonemeData = DataHelpers.GetFirstPhonemeInWord(data);

                    m_shortAudio [Convert.ToInt32(phonemeData["id"])] = AudioBankManager.Instance.GetAudioClip(phonemeData["grapheme"].ToString());
                    m_longAudio [dataId] = LoaderHelpers.LoadAudioForWord(data);
                    //m_longAudio [data] = LoaderHelpers.LoadMnemonic(phonemeData);
                }
                else
                {
                    m_shortAudio [dataId] = LoaderHelpers.LoadAudioForWord(data);
                }
            }

            m_currentData = DataHelpers.GetSingleTargetData(m_dataType);

            if(m_currentData == null)
            {
                m_currentData = m_dataPool[UnityEngine.Random.Range(0, m_dataPool.Count)];
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
            
            InitializeTargets(UnityEngine.Object.FindObjectsOfType(typeof(Target)) as Target[]);
            
            PlayShortAudio();
        } 
        else
        {
            StartCoroutine(OnGameComplete());
        }
    }

    void LogAudio()
    {
#if UNITY_EDITOR
        Debug.Log("LOGGING AUDIO");
        Debug.Log("DATATYPE: " + m_dataType);
        Debug.Log("TARGETPICTURES: " + m_targetsShowPicture);
        
        Debug.Log("LOGGING SHORT - " + m_shortAudio.Count);
        foreach(KeyValuePair<int, AudioClip> kvp in m_shortAudio)
        {
            Debug.Log(kvp.Key + " - " + kvp.Value.name);
        }
        
        Debug.Log("LOGGING LONG - " + m_longAudio.Count);
        foreach(KeyValuePair<int, AudioClip> kvp in m_longAudio)
        {
            Debug.Log(kvp.Key + " - " + kvp.Value.name);
        }
#endif
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
            
            StartCoroutine(target.On(UnityEngine.Random.Range(1f, 4f)));
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
            return DataHelpers.WordsShareOnsetPhonemes(data, m_currentData);
        } 
        else
        {
            return data == m_currentData;
        }
    }
    
    void SetTargetData(Target target)
    {
        DataRow targetData = m_currentData;
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
            //WingroveAudio.WingroveRoot.Instance.PostEvent("GAWP_SQUEAL");

            ball.GetComponent<CatapultAmmo>().Explode();

            target.OnHit();
            
            GameObject targetDetachable = target.SpawnDetachable();

            //iTween.ScaleFrom(targetDetachable, Vector3.zero, 0.15f);

            //yield return new WaitForSeconds(0.16f);

            StartCoroutine(m_scoreKeeper.UpdateScore(targetDetachable));

            target.ApplyHitForce(ball.transform);

            /*
            if(m_changeCurrentData)
            {
                m_currentData = m_dataPool[UnityEngine.Random.Range(0, m_dataPool.Count)];
                PlayShortAudio();
            }
            */

            if(!m_targetsShowPicture && m_dataType == "phonemes")
            {
                PlayShortAudio(target.data);
            }
            else
            {
                PlayLongAudio(target.data);
            }

            if(m_targetsShowPicture)
            {
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
            //WingroveAudio.WingroveRoot.Instance.PostEvent("NEGATIVE_HIT");
            
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
        yield return StartCoroutine(CelebrationCoordinator.Instance.Trumpet());
        GameManager.Instance.CompleteGame();
    }
    
    void PlayLongAudio(DataRow data = null)
    {
        if (data == null)
        {
            data = m_currentData;
        }

        bool hasPlayedAudio = false;

        int dataId = Convert.ToInt32(data ["id"]);

        if (m_longAudio.ContainsKey(dataId))
        {
            m_audioSource.clip = m_longAudio [dataId];
            if (m_audioSource.clip != null)
            {
                hasPlayedAudio = true;
                m_audioSource.Play();
            } 
        }

        if(!hasPlayedAudio)
        {
            PlayShortAudio(data);
        }
    }
    
    void PlayShortAudio(DataRow data = null)
    {
        if (data == null)
        {
            data = m_currentData;
        }

        // TODO: Temporary fix. Come up with a decent solution for this
        if (m_dataType == "words" && m_targetsShowPicture && data["word"] != null)
        {
            data = DataHelpers.GetFirstPhonemeInWord(data);
        }

        int dataId = Convert.ToInt32(data ["id"]);

        if (m_shortAudio.ContainsKey(dataId))
        {
            m_audioSource.clip = m_shortAudio [dataId];
            if (m_audioSource.clip != null)
            {
                m_audioSource.Play();
            }
        } 
#if UNITY_EDITOR
        else
        {
            string attributeName = data["word"] != null ? "word" : "phoneme";
            Debug.LogError("NO SHORT KEY: " + data[attributeName].ToString() + " - " + data["id"].ToString());
            Debug.Log("m_shortAudio.Count: " + m_shortAudio.Count);

            LogAudio();
        }
#endif
    }
}