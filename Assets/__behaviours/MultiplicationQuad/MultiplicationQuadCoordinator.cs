using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiplicationQuadCoordinator : Singleton<MultiplicationQuadCoordinator> 
{
    [SerializeField]
    private MulitiplicationQuadPlayer[] m_gamePlayers;
    [SerializeField]
    private GameObject m_pointPrefab;
    [SerializeField]
    private float m_timeLimit;
    [SerializeField]
    private bool m_questionsAreShared;
    [SerializeField]
    private GameObject m_sideBarCameraPrefab;
    [SerializeField]
    private int m_maxNumLines = 12;
    [SerializeField]
    private bool m_excludePrimeNumbers = false;

    List<DataRow> m_numberPool = new List<DataRow>();

    int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }

    IEnumerator Start ()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_numberPool = DataHelpers.GetNumbers();
        m_numberPool = DataHelpers.OnlyLowNumbers(m_numberPool, 144);

        int numPlayers = GetNumPlayers();

        m_questionsAreShared = m_questionsAreShared && numPlayers == 2;
        
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

        DataRow sharedData = GetRandomData();
        
        for (int i = 0; i < numPlayers; ++i)
        {
            DataRow currentData = m_questionsAreShared ? sharedData : GetRandomData();
            m_gamePlayers[i].SetCurrentData(currentData);

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

    public void OnCorrectAnswer(MulitiplicationQuadPlayer correctPlayer)
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

    DataRow GetRandomData()
    {
        DataRow data = m_numberPool [Random.Range(0, m_numberPool.Count - 1)];

        if (m_excludePrimeNumbers)
        {
            while (MathHelpers.IsPrime(data.GetInt("value")))
            {
                data = m_numberPool [Random.Range(0, m_numberPool.Count - 1)];
            }
        }

        return data;
    }

    public bool AreQuestionsShared()
    {
        return m_questionsAreShared;
    }

    public int GetMaxNumLines()
    {
        return m_maxNumLines;
    }

    public GameObject GetPointPrefab()
    {
        return m_pointPrefab;
    }

    public float GetTimeLimit()
    {
        return m_timeLimit;
    }
}
