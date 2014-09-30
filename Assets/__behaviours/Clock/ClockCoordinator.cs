using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ClockCoordinator : Singleton<ClockCoordinator>
{
    [SerializeField]
    private ClockPlayer[] m_gamePlayers;
    [SerializeField]
    private GameObject m_sideBarCameraPrefab;
    [SerializeField]
    private bool m_questionsAreShared;
    [SerializeField]
    private int m_cushion = 1;

    List<DataRow> m_dataPool = new List<DataRow>();

    float m_timeStarted;

    int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }

    IEnumerator Start()
    {
        //////D.Log("CompleteEquationCoordinator.Start()");
        
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        
        m_dataPool = DataHelpers.GetTimes();
        m_dataPool.Sort(delegate(DataRow x, DataRow y) { return ((DateTime)x["datetime"]).CompareTo((DateTime)y["datetime"]); });


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
        
        DataRow sharedData = GetRandomData();
        
        m_timeStarted = Time.time;
        
        for (int i = 0; i < numPlayers; ++i)
        {
            DataRow currentData = m_questionsAreShared ? sharedData : GetRandomData();
            m_gamePlayers[i].SetCurrentData(currentData);
            m_gamePlayers[i].StartGame();
        }
    }

    public void OnCorrectAnswer(ClockPlayer correctPlayer)
    {
        DataRow currentData = GetRandomData();
        
        if (m_questionsAreShared && GetNumPlayers() == 2)
        {
            for(int i = 0; i < GetNumPlayers(); ++i)
            {
                m_gamePlayers[i].SetCurrentData(currentData);
                StartCoroutine(m_gamePlayers[i].ClearQuestion());
            }
        }
        else
        {
            correctPlayer.SetCurrentData(currentData);
            StartCoroutine(correctPlayer.ClearQuestion());
        }
    }

    DataRow GetRandomData()
    {
        return m_dataPool [UnityEngine.Random.Range(0, m_dataPool.Count)];
    }

    public void CharacterSelected(int characterIndex)
    {
        for (int index = 0; index < GetNumPlayers(); ++index)
        {
            m_gamePlayers[index].HideCharacter(characterIndex);
        }
    }

    public void OnLevelUp()
    {
        m_dataPool = DataSetters.LevelUpTimes();
        m_dataPool.Sort(delegate(DataRow x, DataRow y) { return ((DateTime)x["datetime"]).CompareTo((DateTime)y["datetime"]); });
        ScoreHealth.RefreshColorAll();
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

    public int GetCushion()
    {
        return m_cushion;
    }
}
