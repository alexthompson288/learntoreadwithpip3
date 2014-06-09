﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JoinPairsCoordinator : Singleton<JoinPairsCoordinator> 
{
    [SerializeField]
    private string m_dataType = "words";
    [SerializeField]
    private JoinPairsPlayer[] m_gamePlayers;
    [SerializeField]
    private int m_targetScore = 6;
    [SerializeField]
    private int m_pairsToShowAtOnce = 3;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    bool m_waitForBoth;
    [SerializeField]
    private GameObject m_picturePrefab;
    [SerializeField]
    private GameObject m_textPrefab;
    [SerializeField]
    private GameObject m_numberPrefab;

    float m_startTime;

    public string dataType
    {
        get
        {
            return m_dataType;
        }
    }

    public GameObject picturePrefab
    {
        get
        {
            return m_picturePrefab;
        }
    }

    public GameObject textPrefab
    {
        get
        {
            return m_textPrefab;
        }
    }

    public GameObject numberPrefab
    {
        get
        {
            return m_numberPrefab;
        }
    }

    public int targetScore
    {
        get
        {
            return m_targetScore;
        }
    }

    bool m_joinablesAreSameType = false;
    public bool AreJoinablesSameType()
    {
        return m_joinablesAreSameType;
    }

    int m_numWaitForPlayers;

    int m_numFinishedPlayers;

    List<DataRow> m_dataPool = new List<DataRow>();
    public List<DataRow> dataPool
    {
        get
        {
            return m_dataPool;
        }
    }

    Dictionary<DataRow, AudioClip> m_shortAudio = new Dictionary<DataRow, AudioClip>();
    Dictionary<DataRow, AudioClip> m_longAudio = new Dictionary<DataRow, AudioClip>();
    Dictionary<DataRow, Texture2D> m_pictures = new Dictionary<DataRow, Texture2D>();

    public int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }
    
    public void CharacterSelected(int characterIndex)
    {
        for (int index = 0; index < GetNumPlayers(); ++index)
        {
            m_gamePlayers[index].HideCharacter(characterIndex);
        }
    }
    
    // Use this for initialization
    IEnumerator Start () 
    {
        int numPlayers = GetNumPlayers();

        if (numPlayers == 2)
        {
            m_targetScore = 4;
            yield return new WaitForSeconds(0.5f);
            WingroveAudio.WingroveRoot.Instance.PostEvent("INSTRUCTION_CHOOSE_CHARACTER");
        } 
        else
        {
            SessionInformation.SetDefaultPlayerVar();
            CharacterSelectionParent.DisableAll();
        }

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataType = DataHelpers.GameOrDefault(m_dataType);


        for(int index = 0; index < numPlayers; ++index)
        {
            m_gamePlayers[index].SetUp(m_targetScore, m_dataType); 
        }
        
        if(m_waitForBoth && numPlayers == 2)
        {
            m_numWaitForPlayers = 2;
        }
        else
        {
            m_numWaitForPlayers = 1;
        }

        m_joinablesAreSameType = (m_dataType == "numbers" || m_dataType == "shapes");

        m_dataPool = DataHelpers.GetData(m_dataType);

        if (m_dataType != "numbers")
        {
            m_dataPool = DataHelpers.OnlyPictureData(m_dataType, m_dataPool);
        }

        foreach (DataRow data in m_dataPool)
        {
            m_pictures[data] = DataHelpers.GetPicture(m_dataType, data);

            m_shortAudio[data] = DataHelpers.GetShortAudio(m_dataType, data);

            if(m_dataType == "phonemes")
            {
                m_longAudio[data] = DataHelpers.GetLongAudio(m_dataType, data);
            }
        }

        if(m_dataPool.Count > 0)
        {
            StartCoroutine(PlayGame());
        }
        else
        {
            CompleteGame();
        }
	}

    IEnumerator PlayGame()
    {
        int numPlayers = GetNumPlayers();
        if (numPlayers == 2)
        {
            while (true)
            {
                bool allSelected = true;
                for (int index = 0; index < numPlayers; ++index)
                {
                    if (!m_gamePlayers [index].HasSelectedCharacter())
                    {
                        allSelected = false;
                    }
                }
                
                if (allSelected)
                {
                    break;
                }
                
                yield return null;
            }

            yield return new WaitForSeconds(0.8f);

            WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_READY_STEADY_GO");

            yield return new WaitForSeconds(2.5f);
            
            for (int index = 0; index < numPlayers; ++index)
            {
                m_gamePlayers [index].HideAll();
            }
        } 
        else
        {
            yield return StartCoroutine(m_gamePlayers[0].DrawDemoLine());
        }

        m_startTime = Time.time;

        for(int index = 0; index < numPlayers; ++index)
        {
            StartCoroutine(m_gamePlayers[index].SetUpNext());
        }
        
        while(m_numFinishedPlayers < m_numWaitForPlayers)
        {
            yield return null;
        }
        
        for(int index = 0; index < numPlayers; ++index)
        {
            m_gamePlayers[index].DestroyJoinables();
        }
        
        int winningIndex = -1;
        
        for(int index = 0; index < numPlayers; ++index)
        {
            if(m_gamePlayers[index].score >= m_targetScore)
            {
                winningIndex = index;
                break;
            }
        }
        
        SessionInformation.Instance.SetWinner(winningIndex);

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(m_gamePlayers [winningIndex].OnWin());
        
        CompleteGame();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CompleteGame();
        }
    }
#endif

    void CompleteGame()
    {
        float timeTaken = Time.time - m_startTime;
        ScoreInfo.Instance.NewScore(m_targetScore, m_targetScore, timeTaken, 30 / m_targetScore, 60 / m_targetScore);
        GameManager.Instance.CompleteGame();
    }

    public Texture2D GetPicture(DataRow data)
    {
        return m_pictures.ContainsKey(data) ? m_pictures [data] : null;
    }

    public void PlayShortAudio(DataRow data)
    {
        PlayAudio(data, m_shortAudio);
    }

    public void PlayLongAudio(DataRow data)
    {
        PlayAudio(data, m_longAudio);
    }

    void PlayAudio(DataRow data, Dictionary<DataRow, AudioClip> audioDictionary)
    {
        if (audioDictionary.ContainsKey(data))
        {
            AudioClip clip = audioDictionary [data];
            if (clip != null)
            {
                m_audioSource.clip = clip;
                m_audioSource.Play();
            }
        } 
        else
        {
            string attributeName = dataType == "words" ? "word" : "phoneme";
            Debug.LogError("NO KEY for " + data[attributeName].ToString());
            Debug.Log("audioDictionary.Count: " + audioDictionary.Count);
        }
    }

    public int GetPairsToShowAtOnce()
    {
        return m_pairsToShowAtOnce;
    }

    public void IncrementNumFinishedPlayers()
    {
        ++m_numFinishedPlayers;
    }
}
