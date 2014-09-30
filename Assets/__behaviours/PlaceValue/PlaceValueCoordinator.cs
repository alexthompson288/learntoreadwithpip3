using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaceValueCoordinator : Singleton<PlaceValueCoordinator>
{
    [SerializeField]
    private PlaceValuePlayer[] m_gamePlayers;
    [SerializeField]
    private float m_timeLimit;
    [SerializeField]
    private GameObject m_sideBarCameraPrefab;
    [SerializeField]
    private bool m_questionsAreShared;
    [SerializeField]
    private Vector2 m_currentPoolCountRange = new Vector2(1, 2);

    List<DataRow> m_numberPool = new List<DataRow>();

    int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_numberPool = DataHelpers.GetNumbers();

        m_numberPool.Sort(delegate(DataRow x, DataRow y) { return x.GetInt("value").CompareTo(y.GetInt("value")); });
        
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
        
        //////D.Log("Starting game");

        List<DataRow> sharedDataPool = GetRandomDataPool();
        
        for (int i = 0; i < numPlayers; ++i)
        {
            List<DataRow> currentDataPool = m_questionsAreShared ? sharedDataPool : GetRandomDataPool();
            m_gamePlayers[i].SetCurrentDataPool(currentDataPool);

            m_gamePlayers[i].StartGame(i == 0);
        }
    }

    public void OnCorrectAnswer(PlaceValuePlayer correctPlayer)
    {
        List<DataRow> currentDataPool = GetRandomDataPool();
        
        if (m_questionsAreShared && GetNumPlayers() == 2)
        {
            for(int i = 0; i < GetNumPlayers(); ++i)
            {
                m_gamePlayers[i].SetCurrentDataPool(currentDataPool);
                StartCoroutine(m_gamePlayers[i].ClearQuestion());
            }
        }
        else
        {
            correctPlayer.SetCurrentDataPool(currentDataPool);
            StartCoroutine(correctPlayer.ClearQuestion());
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
        StartCoroutine(CompleteGameCo());
    }
    
    IEnumerator CompleteGameCo()
    {
        int winningIndex = GetNumPlayers() == 2 && m_gamePlayers[0].GetScore() < m_gamePlayers[1].GetScore() ? 1 : 0;
        
        yield return StartCoroutine(m_gamePlayers[winningIndex].CelebrateVictory());
        
        GameManager.Instance.CompleteGame();
    }

    public int GetDataTotal(List<DataRow> dataPool)
    {
        int total = 0;

        foreach (DataRow row in dataPool)
        {
            total += row.GetInt("value");
        }

        return total;
    }
    
    public List<DataRow> GetRandomDataPool()
    {
        List<DataRow> dataPool = new List<DataRow>();

        int numData = (int)Random.Range(m_currentPoolCountRange.x, m_currentPoolCountRange.y + 1);

        while (dataPool.Count == 0 || GetDataTotal(dataPool) >= m_numberPool[m_numberPool.Count - 1].GetInt("value"))
        {
            dataPool.Clear();

            for(int i = 0; i < numData; ++i)
            {
                dataPool.Add(m_numberPool[Random.Range(0, m_numberPool.Count - 1)]);
            }
        }

        return dataPool;
    }

    public float GetTimeLimit()
    {
        return m_timeLimit;
    }
}
