using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PunctuationCoordinator : Singleton<PunctuationCoordinator> 
{
    [SerializeField]
    private PunctuationPlayer[] m_gamePlayers;
    [SerializeField]
    private bool m_questionsAreShared;
    [SerializeField]
    private GameObject m_textPrefab;
    [SerializeField]
    private GameObject m_answerPrefab;
    [SerializeField]
    private int m_numAnswersToSpawn;
    
    List<DataRow> m_dataPool = new List<DataRow>();
    
    float m_timeStarted;

    string[] m_punctuation = new string[] { ".", ",", ";" };
    
    int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }

    bool DoesNotContainPunctuation(DataRow data)
    {
        bool doesNotContainPunctuation = true;
        string sentence = data ["good_sentence"].ToString();

        foreach (string punc in m_punctuation)
        {
            if(sentence.Contains(punc))
            {
                doesNotContainPunctuation = false;
            }
        }

        return doesNotContainPunctuation;
    }
    
    IEnumerator Start()
    {
        D.Log("PunctuationCoordinator.Start()");
        
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        
        m_dataPool = DataHelpers.GetCorrectCaptions();

        m_dataPool.RemoveAll(DoesNotContainPunctuation);

        D.Log("m_dataPool.Count: " + m_dataPool.Count);
        
        int numPlayers = GetNumPlayers();
        
        if (numPlayers == 1)
        {
            CharacterSelectionParent.DisableAll();
            SessionInformation.SetDefaultPlayerVar();
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
        
        D.Log("Starting game");
        
        DataRow sharedData = GetRandomData();
        
        m_timeStarted = Time.time;
        
        for (int i = 0; i < numPlayers; ++i)
        {
            DataRow currentData = m_questionsAreShared ? sharedData : GetRandomData();
            m_gamePlayers[i].SetCurrentData(currentData);
            m_gamePlayers[i].StartGame();
        }
    }
    
    public void OnCorrectAnswer(PunctuationPlayer correctPlayer)
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
        return m_dataPool [Random.Range(0, m_dataPool.Count)];
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
        m_dataPool = DataSetters.LevelUpCorrectCaptions();
        m_dataPool.RemoveAll(DoesNotContainPunctuation);
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

    public GameObject GetTextPrefab()
    {
        return m_textPrefab;
    }

    public GameObject GetAnswerPrefab()
    {
        return m_answerPrefab;
    }

    public string[] GetPunctuation()
    {
        return m_punctuation;
    }

    public int GetNumAnswersToSpawn()
    {
        return m_numAnswersToSpawn;
    }
}
