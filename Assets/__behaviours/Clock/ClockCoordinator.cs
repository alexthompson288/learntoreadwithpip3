using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClockCoordinator : Singleton<ClockCoordinator>
{
    [SerializeField]
    private ClockPlayer[] m_gamePlayers;
    [SerializeField]
    private GameObject m_sideBarCameraPrefab;
    [SerializeField]
    private bool m_questionsAreShared;

    List<DataRow> m_timePool = new List<DataRow>();

    float m_timeStarted;

    int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }

    IEnumerator Start()
    {
        D.Log("CompleteEquationCoordinator.Start()");
        
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        
        m_timePool = DataHelpers.Times();
        
        m_timePool.Sort(delegate(DataRow x, DataRow y) { return x.GetInt("value").CompareTo(y.GetInt("value")); });
        
        D.Log("m_timePool.Count: " + m_timePool.Count);
        
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
        
        DataRow sharedData = GetRandomData();
        
        m_timeStarted = Time.time;
        
        for (int i = 0; i < numPlayers; ++i)
        {
            DataRow currentData = m_questionsAreShared ? sharedData : GetRandomData();
            m_gamePlayers[i].SetCurrentData(currentData);
            m_gamePlayers[i].StartGame(i == 0);
        }
    }

    DataRow GetRandomData()
    {
        return m_timePool [Random.Range(0, m_timePool.Count)];
    }

    public void CharacterSelected(int characterIndex)
    {
        for (int index = 0; index < GetNumPlayers(); ++index)
        {
            m_gamePlayers[index].HideCharacter(characterIndex);
        }
    }
}
