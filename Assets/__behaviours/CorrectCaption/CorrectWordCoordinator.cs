using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorrectWordCoordinator : Singleton<CorrectWordCoordinator> 
{
    [SerializeField]
    private CorrectWordPlayer[] m_gamePlayers;
    [SerializeField]
    private bool m_questionsAreShared;

    List<DataRow> m_dataPool = new List<DataRow>();
    
    float m_timeStarted;

    string m_dummyAttribute1 = "dummyword1";
    string m_dummyAttribute2 = "dummyword2";

    int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }

    void RemoveIllegalData()
    {
        D.Log("COUNT PRE PICTURE CHECK: " + m_dataPool.Count);
        m_dataPool = DataHelpers.OnlyPictureData(m_dataPool);
        D.Log("COUNT PRE DUMMY CHECK: " + m_dataPool.Count);
        m_dataPool.RemoveAll(x => x[m_dummyAttribute1] == null && x[m_dummyAttribute2] == null);
    }
    
    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        
        m_dataPool = DataHelpers.GetWords();
        RemoveIllegalData();

        int numPlayers = GetNumPlayers();
        
        if (numPlayers == 1)
        {
            CharacterSelectionParent.DisableAll();
            SessionInformation.SetDefaultPlayerVar();
            yield return StartCoroutine(TransitionScreen.WaitForScreenExit());
        }
        else
        {
            SideBarCameraSpawner.Instance.InstantiateSideBarCamera();
            
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
        
        ////D.Log("Starting game");
        
        if (m_dataPool.Count > 0)
        {
            DataRow sharedData = GetRandomData();
            
            m_timeStarted = Time.time;
            
            for (int i = 0; i < numPlayers; ++i)
            {
                DataRow currentData = m_questionsAreShared ? sharedData : GetRandomData();
                m_gamePlayers[i].SetCurrentData(currentData);
                m_gamePlayers[i].StartGame();
            }
        }
        else
        {
            StartCoroutine(CompleteGameCo());
        }
    }

    public void OnCorrectAnswer(CorrectWordPlayer correctPlayer)
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
        DataRow data = m_dataPool [Random.Range(0, m_dataPool.Count)];

        m_dataPool.Remove(data);

        if (m_dataPool.Count == 0)
        {
            m_dataPool = DataHelpers.GetWords();
        }

        return data;
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
        m_dataPool = DataSetters.LevelUpWords();
        RemoveIllegalData();
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

    public string GetDummyAttribute1()
    {
        return m_dummyAttribute1;
    }

    public string GetDummyAttribute2()
    {
        return m_dummyAttribute2;
    }
}
