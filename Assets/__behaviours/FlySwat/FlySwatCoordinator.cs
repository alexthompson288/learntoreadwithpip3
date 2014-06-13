using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlySwatCoordinator : Singleton<FlySwatCoordinator> 
{
    [SerializeField]
    private string m_dataType;
    [SerializeField]
    private GameObject m_flyPrefab;
    [SerializeField]
    private FlySwatPlayer[] m_gamePlayers;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private AudioClip m_instructions;
    [SerializeField]
    private int m_targetScore;
    [SerializeField]
    private Vector2 m_spawnDelay = new Vector2(1, 3);
    [SerializeField]
    private float m_probabilityDataIsCurrent;
    [SerializeField]
    private bool m_waitForBoth;
    int m_numWaitForPlayers;

    List<DataRow> m_dataPool = new List<DataRow>();

    DataRow m_currentData = null;

    int m_numFinished = 0;
    int m_winningIndex = -1;

    float m_startTime;

	// Use this for initialization
	IEnumerator Start () 
    {
        m_numWaitForPlayers = m_waitForBoth ? 2 : 1;
        m_probabilityDataIsCurrent = Mathf.Clamp01(m_probabilityDataIsCurrent);

        if (GetNumPlayers() == 1)
        {
            CharacterSelectionParent.DisableAll();
            SessionInformation.SetDefaultPlayerVar();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            WingroveAudio.WingroveRoot.Instance.PostEvent("INSTRUCTION_CHOOSE_CHARACTER");
        }

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataType = DataHelpers.GameOrDefault(m_dataType);

        m_dataPool = DataHelpers.GetData(m_dataType);
        m_currentData = DataHelpers.GetSingleTargetData(m_dataType, m_dataPool);

        //Debug.Log("dataPool.Count: " + m_dataPool.Count);
        //Debug.Log("currentData: " + m_currentData);

        StartCoroutine(StartGame());
	}

    IEnumerator StartGame()
    {
        m_audioSource.clip = m_instructions;
        m_audioSource.Play();
        
        while(m_audioSource.isPlaying)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.25f);

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
            
            yield return new WaitForSeconds(1.5f);
            
            for (int index = 0; index < numPlayers; ++index)
            {
                m_gamePlayers [index].HideAll();
            }
            
            yield return new WaitForSeconds(1.0f);
        }

        for(int index = 0; index < numPlayers; ++index)
        {
            m_gamePlayers[index].SetUp();
            m_gamePlayers[index].ShowDataDisplay(m_dataType, m_currentData);
        }

        PlayAudio();

        yield return new WaitForSeconds(1f);

        for(int index = 0; index < numPlayers; ++index)
        {
            m_gamePlayers[index].HideDataDisplay();
        }

        m_startTime = Time.time;

        for(int index = 0; index < numPlayers; ++index)
        {
            m_gamePlayers[index].StartGame();
        }
    }

    public void OnPlayerFinish(int playerIndex)
    {
        if (m_winningIndex == -1)
        {
            m_winningIndex = playerIndex;
        }

        ++m_numFinished;

        if (m_numFinished >= m_numWaitForPlayers)
        {
            for(int index = 0; index < GetNumPlayers(); ++index)
            {
                m_gamePlayers[index].StopGame();
            }

            StartCoroutine(CompleteGame());
        }
    }

    bool winningPlayerHasCompletedSequence = false;

    public void OnWinningPlayerCompleteSequence()
    {
        winningPlayerHasCompletedSequence = true;
    }

    IEnumerator CompleteGame()
    {
        if (GetNumPlayers() == 1)
        {
            float timeTaken = Time.time - m_startTime;
            
            float twoStarPerQuestion = 5;
            float threeStarPerQuestion = 4;
            
            Debug.Log("timeTaken: " + timeTaken);
            Debug.Log("twoStar: " + twoStarPerQuestion * m_targetScore);
            Debug.Log("threeStar: " + threeStarPerQuestion * m_targetScore);
            
            ScoreInfo.Instance.NewScore(m_gamePlayers [m_winningIndex].GetScore(), m_targetScore, timeTaken, twoStarPerQuestion * m_targetScore, threeStarPerQuestion * m_targetScore);
        }

        while (!winningPlayerHasCompletedSequence)
        {
            yield return null;
        }

        GameManager.Instance.CompleteGame();
    }

    public void CharacterSelected(int characterIndex)
    {
        for (int index = 0; index < GetNumPlayers(); ++index)
        {
            m_gamePlayers[index].HideCharacter(characterIndex);
        }
    }

    public void PlayAudio()
    {
        if (!m_audioSource.isPlaying)
        {
            AudioClip clip = DataHelpers.GetShortAudio(m_dataType, m_currentData);

            if (clip != null)
            {
                m_audioSource.clip = clip;
                m_audioSource.Play();
            }
        }
    }

    // Getters
    public DataRow GetCurrentData()
    {
        return m_currentData;
    }
    
    public int GetCurrentId()
    {
        return System.Convert.ToInt32(m_currentData ["id"]);
    }
    
    public DataRow GetRandomData()
    {
        return m_dataPool [Random.Range(0, m_dataPool.Count)];
    }
    
    public string GetDataType()
    {
        return m_dataType;
    }
    
    public int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }
    
    public int GetTargetScore()
    {
        return m_targetScore;
    }
    
    public float GetMinSpawnDelay()
    {
        return m_spawnDelay.x;
    }
    
    public float GetMaxSpawnDelay()
    {
        return m_spawnDelay.y;
    }

    public float GetProbabilityDataIsCurrent()
    {
        return m_probabilityDataIsCurrent;
    }
    
    public GameObject GetFlyPrefab()
    {
        return m_flyPrefab;
    }
}
