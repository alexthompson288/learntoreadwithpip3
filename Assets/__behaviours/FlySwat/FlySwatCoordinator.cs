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
    private GameObject m_sideBarCameraPrefab;
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
            GameObject sideBarCamera = (GameObject)GameObject.Instantiate(m_sideBarCameraPrefab, Vector3.zero, Quaternion.identity);
            sideBarCamera.transform.parent = m_gamePlayers[0].transform;
            sideBarCamera.transform.localScale = Vector3.one;

            yield return new WaitForSeconds(0.5f);
            WingroveAudio.WingroveRoot.Instance.PostEvent("INSTRUCTION_CHOOSE_CHARACTER");
        }

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataType = DataHelpers.GameOrDefault(m_dataType);

        m_dataPool = DataHelpers.GetData(m_dataType);
        m_currentData = DataHelpers.GetSingleTargetData(m_dataType, m_dataPool);

        //////D.Log("dataPool.Count: " + m_dataPool.Count);
        //////D.Log("currentData: " + m_currentData);

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
            
            for (int index = 0; index < numPlayers; ++index)
            {
                m_gamePlayers [index].HideAll();
            }
            
            StartCoroutine(m_gamePlayers[0].PlayTrafficLights());
            yield return StartCoroutine(m_gamePlayers[1].PlayTrafficLights());
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

    IEnumerator CompleteGame()
    {
        if (GetNumPlayers() == 1)
        {
            float timeTaken = Time.time - m_startTime;

            ////D.Log("timeTaken: " + timeTaken);
            
            float twoStarPerQuestion = 4f;
            float threeStarPerQuestion = 2.5f;

            //////D.Log("TimeTaken: " + timeTaken);
            //////D.Log("twoStar: " + twoStarPerQuestion * (float)m_targetScore);
            //////D.Log("threeStar: " + threeStarPerQuestion * (float)m_targetScore);
            
            int stars = ScoreInfo.CalculateTimeStars(timeTaken, twoStarPerQuestion * (float)m_targetScore, threeStarPerQuestion * (float)m_targetScore);

            // Game ends when player reaches targetScore
            ScoreInfo.Instance.NewScore(timeTaken, m_targetScore, m_targetScore, stars);
        }
        else
        {
            yield return new WaitForSeconds(0.75f);
            CelebrationCoordinator.Instance.DisplayVictoryLabels(m_winningIndex);
            CelebrationCoordinator.Instance.PopCharacter(m_gamePlayers[m_winningIndex].GetSelectedCharacter(), true);
            yield return new WaitForSeconds(2f);
        }

        yield return StartCoroutine(m_gamePlayers [m_winningIndex].Celebrate());

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
            AudioClip clip = DataHelpers.GetShortAudio(m_currentData);

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
