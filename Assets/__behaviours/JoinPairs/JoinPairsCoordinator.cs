using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JoinPairsCoordinator : Singleton<JoinPairsCoordinator> 
{
    [SerializeField]
    private GameObject m_sideBarCameraPrefab;
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
    private AudioClip m_wordsInstructions;
    [SerializeField]
    private AudioClip m_numbersInstructions;
    [SerializeField]
    private AudioClip m_shapesInstructions;
    [SerializeField]
    bool m_waitForBoth;
    [SerializeField]
    private GameObject m_picturePrefab;
    [SerializeField]
    private GameObject m_textPrefab;
    [SerializeField]
    private GameObject m_numberPrefab;

    float m_startTime;

    int m_winningIndex = -1;

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
            GameObject sideBarCamera = (GameObject)GameObject.Instantiate(m_sideBarCameraPrefab, Vector3.zero, Quaternion.identity);
            sideBarCamera.transform.parent = m_gamePlayers[0].transform;
            sideBarCamera.transform.localScale = Vector3.one;

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

        ////////D.Log("JoinPairs.Start()");
        ////////D.Log("m_dataType:" + m_dataType);
        ////////D.Log("m_dataPool.Count: " + m_dataPool.Count);

        if (m_dataType != "numbers")
        {
            m_dataPool = DataHelpers.OnlyPictureData(m_dataPool);
        }
        else
        {
            m_dataPool = DataHelpers.OnlyLowNumbers(m_dataPool, 12);
        }

        foreach (DataRow data in m_dataPool)
        {
            m_pictures[data] = DataHelpers.GetPicture(data);
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

            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(TransitionScreen.WaitForScreenExit());
            
            PlayAudioInstructions();

            while(m_audioSource.isPlaying)
            {
                yield return null;
            }

            for (int index = 0; index < numPlayers; ++index)
            {
                m_gamePlayers [index].HideAll();
            }

            StartCoroutine(m_gamePlayers[0].PlayTrafficLights());
            yield return StartCoroutine(m_gamePlayers[1].PlayTrafficLights());
        } 
        else
        {
            yield return StartCoroutine(TransitionScreen.WaitForScreenExit());
            
            PlayAudioInstructions();

            yield return new WaitForSeconds(0.75f);

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

        SessionInformation.Instance.SetWinner(m_winningIndex);

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(m_gamePlayers [m_winningIndex].Celebrate());
        
        CompleteGame();
    }

    void PlayAudioInstructions()
    {
        AudioClip clip = null;
        
        switch(m_dataType)
        {
            case "words":
                clip = m_wordsInstructions;
                break;
            case "shapes":
                clip = m_shapesInstructions;
                break;
            case "numbers":
                clip = m_numbersInstructions;
                break;
        }
        
        if(clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }
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

    public void OnPlayerFinish(int playerIndex)
    {
        if (m_winningIndex == -1)
        {
            m_winningIndex = playerIndex;
        }
        
        ++m_numFinishedPlayers;
    }

    void CompleteGame()
    {
        if (GetNumPlayers() == 1)
        {
            float timeTaken = Time.time - m_startTime;

            float twoStarPerQuestion = 20;
            float threeStarPerQuestion = 14;
            
            int stars = ScoreInfo.CalculateTimeStars(timeTaken, twoStarPerQuestion * (float)m_targetScore, threeStarPerQuestion * (float)m_targetScore);

            // Game ends when player reaches targetScore
            ScoreInfo.Instance.NewScore(timeTaken, m_targetScore, m_targetScore, stars);
        }

        GameManager.Instance.CompleteGame();
    }

    public Texture2D GetPicture(DataRow data)
    {
        return m_pictures.ContainsKey(data) ? m_pictures [data] : null;
    }

    public int GetPairsToShowAtOnce()
    {
        return m_pairsToShowAtOnce;
    }

    public void PlayShortAudio(DataRow data)
    {
        PlayAudio(DataHelpers.GetShortAudio(data));
    }

    public void PlayLongAudio(DataRow data)
    {
        PlayAudio(DataHelpers.GetLongAudio(data));
    }

    void PlayAudio(AudioClip clip)
    {
        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }
    }
}
