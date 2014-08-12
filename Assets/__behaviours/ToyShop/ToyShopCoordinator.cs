using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToyShopCoordinator : Singleton<ToyShopCoordinator> 
{
    [SerializeField]
    private ToyShopPlayer[] m_gamePlayers;
    [SerializeField]
    private float m_timeLimit;
    [SerializeField]
    private int m_numToysToSpawn;
    [SerializeField]
    private GameObject m_toyPrefab;
    [SerializeField]
    private GameObject m_sideBarCameraPrefab;

    List<DataRow> m_dataPool = new List<DataRow>();

    float m_timeStarted;

    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetNumbers();

        int numPlayers = GetNumPlayers();

        if (numPlayers == 1)
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

        m_timeStarted = Time.time;

        for (int i = 0; i < numPlayers; ++i)
        {
            m_gamePlayers[i].StartGame(i == 0);
        }
    }

    public void CharacterSelected(int characterIndex)
    {
        for (int index = 0; index < GetNumPlayers(); ++index)
        {
            m_gamePlayers[index].HideCharacter(characterIndex);
        }
    }

    public void CompleteGame()
    {
        if (GetNumPlayers() == 1)
        {
            PlusScoreInfo.Instance.NewScore(Time.time - m_timeStarted, m_gamePlayers[0].GetScore(), (int)GameManager.Instance.currentColor);
        }

        StartCoroutine(CompleteGameCo());
    }

    IEnumerator CompleteGameCo()
    {
        int winningIndex = GetNumPlayers() == 2 && m_gamePlayers[0].GetScore() < m_gamePlayers[1].GetScore() ? 1 : 0;

        yield return StartCoroutine(m_gamePlayers[winningIndex].CelebrateVictory());

        GameManager.Instance.CompleteGame();
    }

    public void OnLevelUp()
    {
        m_dataPool = DataSetters.LevelUpNumbers();
    }

    public int GetRandomValue()
    {
        return m_dataPool [Random.Range(0, m_dataPool.Count)].GetInt("value");
    }

    public GameObject GetToyPrefab()
    {
        return m_toyPrefab;
    }

    public float GetTimeLimit()
    {
        return m_timeLimit;
    }

    public int GetNumToysToSpawn()
    {
        return m_numToysToSpawn;
    }
    
    int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }
}
