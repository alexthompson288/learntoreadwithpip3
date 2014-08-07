using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompleteEquationCoordinator : Singleton<CompleteEquationCoordinator>
{
    [SerializeField]
    private CompleteEquationPlayer[] m_gamePlayers;
    [SerializeField]
    private GameObject m_answerPrefab;
    [SerializeField]
    private GameObject m_equationPartPrefab;
    [SerializeField]
    private GameObject m_sideBarCameraPrefab;
    [SerializeField]
    private bool m_questionsAreShared;
    [SerializeField]
    private int m_numAnswersToSpawn = 3;
    [SerializeField]
    private AudioSource m_audioSource;

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
        
        D.Log("Starting game");
        
        Equation sharedData = GetRandomEquation();
        
        for (int i = 0; i < numPlayers; ++i)
        {
            Equation currentData = m_questionsAreShared ? sharedData : GetRandomEquation();
            m_gamePlayers[i].SetEquation(currentData);
            
            m_gamePlayers[i].StartGame(i == 0);
        }
    }

    public void OnCorrectAnswer(CompleteEquationPlayer correctPlayer)
    {
        Equation equationData = GetRandomEquation();
        
        if (m_questionsAreShared && GetNumPlayers() == 2)
        {
            for(int i = 0; i < GetNumPlayers(); ++i)
            {
                m_gamePlayers[i].SetEquation(equationData);
                StartCoroutine(m_gamePlayers[i].ClearQuestion());
            }
        }
        else
        {
            correctPlayer.SetEquation(equationData);
            StartCoroutine(correctPlayer.ClearQuestion());
        }
    }

    public void OnLevelUp()
    {
        D.Log("CompleteEquationCoordinator.OnLevelUp()");
        bool hasIncremented = GameManager.Instance.IncrementCurrentColor();

        D.Log(System.String.Format("{0} - {1}", GameManager.Instance.currentColor, hasIncremented));

        if (hasIncremented)
        {
            DataSetters.AddModuleNumbers(GameManager.Instance.currentColor);
            m_numberPool = DataHelpers.GetData("numbers");
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

    public void PlayAudio(DataRow data)
    {
        AudioClip clip = LoaderHelpers.LoadAudioForNumber(data);
        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }
    }
    
    public void CharacterSelected(int characterIndex)
    {
        for (int index = 0; index < GetNumPlayers(); ++index)
        {
            m_gamePlayers[index].HideCharacter(characterIndex);
        }
    }

    public class Equation
    {
        public List<DataRow> m_equationParts = new List<DataRow>();
        public int m_missingIndex;

        public Equation(List<DataRow> myEquationParts, int myMissingIndex)
        {
            m_equationParts = myEquationParts;
            m_missingIndex = myMissingIndex;
        }
    }

    public DataRow GetRandomData()
    {
        return m_numberPool [Random.Range(0, m_numberPool.Count)];
    }

    Equation GetRandomEquation()
    {
        DataRow sum = DataHelpers.GetLegalSum(m_numberPool);
        
        List<DataRow> equationParts = DataHelpers.GetLegalAdditionLHS(sum, m_numberPool);
        equationParts.Add(sum); // Add sum last because it needs to go on RHS and m_equationPartLocators are sorted from left to right
        
        //int missingIndex = Random.Range(0, equationParts.Count);
        int missingIndex = 2;

        return new Equation(equationParts, missingIndex);
    }

    public GameObject GetAnswerPrefab()
    {
        return m_answerPrefab;
    }

    public GameObject GetEquationPartPrefab()
    {
        return m_equationPartPrefab;
    }

    public int GetNumAnswersToSpawn()
    {
        return m_numAnswersToSpawn;
    }
}
