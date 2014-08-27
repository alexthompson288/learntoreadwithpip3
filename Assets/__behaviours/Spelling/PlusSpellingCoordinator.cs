using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlusSpellingCoordinator : Singleton<PlusSpellingCoordinator>
{
    [SerializeField]
    private bool m_questionsAreShared;
    [SerializeField]
    private bool m_useNonsenseWords;
    [SerializeField]
    private int m_numLettersToSpawn = 15;
    [SerializeField]
    private PlusSpellingPlayer[] m_gamePlayers;
    [SerializeField]
    private GameObject m_sideBarCameraPrefab;
    [SerializeField]
    private GameObject m_draggablePrefab;

    float m_timeStarted;

    List<DataRow> m_wordPool = new List<DataRow>();

    int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_wordPool = DataHelpers.GetWords();
        m_wordPool = DataHelpers.OnlyPictureData(m_wordPool);

#if UNITY_EDITOR
        //m_wordPool = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words WHERE word='knight'").Rows;
#endif

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
        
        ////D.Log("Starting game");
        
        DataRow sharedData = GetRandomWord();
        
        m_timeStarted = Time.time;
        
        for (int i = 0; i < numPlayers; ++i)
        {
            DataRow currentData = m_questionsAreShared ? sharedData : GetRandomWord();
            m_gamePlayers[i].SetCurrentData(currentData);
            m_gamePlayers[i].StartGame();
        }
    }

    public void OnCorrectAnswer(PlusSpellingPlayer correctPlayer)
    {
        DataRow currentData = GetRandomWord();
        
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

    public void CharacterSelected(int characterIndex)
    {
        for (int index = 0; index < GetNumPlayers(); ++index)
        {
            m_gamePlayers[index].HideCharacter(characterIndex);
        }
    }

    DataRow GetRandomWord()
    {
        DataRow data = m_wordPool [Random.Range(0, m_wordPool.Count)];

        m_wordPool.Remove(data);
        if (m_wordPool.Count == 0)
        {
            m_wordPool = DataHelpers.GetWords();
            m_wordPool = DataHelpers.OnlyPictureData(m_wordPool);
        }

        return data;
    }

    public void OnLevelUp()
    {
        m_wordPool = DataSetters.LevelUpWords();
        m_wordPool = DataHelpers.OnlyPictureData(m_wordPool);
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

        for (int i = 0; i < m_gamePlayers.Length; ++i)
        {
            m_gamePlayers[i].ClearGame();
        }

        yield return new WaitForSeconds(0.15f);

        yield return StartCoroutine(m_gamePlayers[winningIndex].CelebrateVictory());
        
        GameManager.Instance.CompleteGame();
    }

    public GameObject GetDraggablePrefab()
    {
        return m_draggablePrefab;
    }

    public int GetNumLettersToSpawn()
    {
        return m_numLettersToSpawn;
    }

    public bool CanPlayAudio()
    {
        return SessionInformation.Instance.GetNumPlayers() == 2 || m_questionsAreShared;
    }
}
