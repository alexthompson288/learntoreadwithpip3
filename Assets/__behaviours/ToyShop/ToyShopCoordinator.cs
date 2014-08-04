using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToyShopCoordinator : Singleton<ToyShopCoordinator> 
{
    [SerializeField]
    private ToyShopPlayer[] m_gamePlayers;
    [SerializeField]
    private float m_timeAllowed;
    [SerializeField]
    private int m_numToysToSpawn;
    [SerializeField]
    private GameObject m_toyPrefab;
    [SerializeField]
    private GameObject m_sideBarCameraPrefab;
    [SerializeField]
    private int[] m_coinValues;

    List<DataRow> m_numberPool = new List<DataRow>();

    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_numberPool = DataHelpers.GetNumbers();

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

            int numPlayers = GetNumPlayers();
            
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

        for (int i = 0; i < m_gamePlayers.Length; ++i)
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
        StartCoroutine(CompleteGameCo());
    }

    IEnumerator CompleteGameCo()
    {
        int winningIndex = GetNumPlayers() == 2 && m_gamePlayers[0].GetScore() < m_gamePlayers[1].GetScore() ? 1 : 0;

        yield return StartCoroutine(m_gamePlayers[winningIndex].CelebrateVictory());

        GameManager.Instance.CompleteGame();
    }

    public int GetRandomValue()
    {
        return m_numberPool [Random.Range(0, m_numberPool.Count)].GetInt("value");
    }

    public GameObject GetToyPrefab()
    {
        return m_toyPrefab;
    }

    public float GetTimeAllowed()
    {
        return m_timeAllowed;
    }

    public int GetNumToysToSpawn()
    {
        return m_numToysToSpawn;
    }

    public int[] GetCoinValues()
    {
        return m_coinValues;
    }
    
    int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }
}
