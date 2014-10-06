using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NumberQuizCoordinator : Singleton<NumberQuizCoordinator>
{
    [SerializeField]
    private bool m_questionsAreShared;
    [SerializeField]
    private bool m_useDummyCountables;
    [SerializeField]
    private NumberQuizPlayer[] m_gamePlayers;
    [SerializeField]
    private GameObject m_countablePrefab;
    [SerializeField]
    private Countable[] m_countables; 
    [SerializeField]
    private float m_probabilityDataIsTarget;

    List<DataRow> m_dataPool = new List<DataRow>();
    DataRow m_targetData;
    
    float m_timeStarted;

    [System.Serializable]
    public class Countable
    {
        [SerializeField]
        private string m_name;
        public string name
        {
            get
            {
                return m_name;
            }
        }

        [SerializeField]
        private string m_spriteName;
        public string spriteName
        {
            get
            {
                return m_spriteName;
            }
        }
    }
    
    int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }
    
    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        
        m_dataPool = DataHelpers.GetNumbers();
        m_dataPool = DataHelpers.OnlyLowNumbers(m_dataPool, m_gamePlayers [0].GetLocatorCount());

        m_targetData = DataHelpers.GetSingleTargetData("numbers", null);
        D.Log("m_targetData: " + m_targetData);

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

        DataRow sharedData = m_targetData != null ? m_targetData : GetRandomData(false);
        Countable sharedCountable = GetRandomCountable();
        
        m_timeStarted = Time.time;
        
        for (int i = 0; i < numPlayers; ++i)
        {
            DataRow currentData = (m_questionsAreShared || m_targetData != null) ? sharedData : GetRandomData(false);
            Countable currentCountable = m_questionsAreShared ? sharedCountable : GetRandomCountable();
            m_gamePlayers[i].SetCurrentData(currentData);
            m_gamePlayers[i].SetCurrentCountable(currentCountable);
            m_gamePlayers[i].StartGame();
        }
    }
    
    public void OnCorrectAnswer(NumberQuizPlayer correctPlayer)
    {
        DataRow currentData = GetRandomData(true);
        Countable currentCountable = GetRandomCountable();
        
        if (m_questionsAreShared && GetNumPlayers() == 2)
        {
            for(int i = 0; i < GetNumPlayers(); ++i)
            {
                m_gamePlayers[i].SetCurrentData(currentData);
                m_gamePlayers[i].SetCurrentCountable(currentCountable);
                StartCoroutine(m_gamePlayers[i].ClearQuestion());
            }
        }
        else
        {
            correctPlayer.SetCurrentData(currentData);
            correctPlayer.SetCurrentCountable(currentCountable);
            StartCoroutine(correctPlayer.ClearQuestion());
        }
    }
    
    public DataRow GetRandomData(bool useProbabilityDataIsTarget)
    {   
        DataRow data = m_dataPool [Random.Range(0, m_dataPool.Count)];

        if (useProbabilityDataIsTarget && m_targetData != null)
        {
            data = m_targetData;
            if(Random.Range(0f, 1f) > m_probabilityDataIsTarget)
            {
                while(data == m_targetData)
                {
                    data = m_dataPool [Random.Range(0, m_dataPool.Count)];
                }
            }
        } 

        return data;
    }

    public Countable GetRandomCountable()
    {
        return m_countables [Random.Range(0, m_countables.Length)];
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
        m_dataPool = DataSetters.LevelUpNumbers();
        m_dataPool = DataHelpers.OnlyLowNumbers(m_dataPool, m_gamePlayers [0].GetLocatorCount());
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

    public GameObject GetCountablePrefab()
    {
        return m_countablePrefab;
    }

    public string GetRandomCountableSpriteName()
    {
        return m_countables [Random.Range(0, m_countables.Length)].spriteName;
    }

    public bool UseDummyCountables()
    {
        return m_useDummyCountables;
    }
}
