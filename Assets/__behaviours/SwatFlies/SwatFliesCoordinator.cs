using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwatFliesCoordinator : Singleton<SwatFliesCoordinator> 
{
    [SerializeField]
    private string m_dataType;
    [SerializeField]
    private GameObject m_flyPrefab;
    [SerializeField]
    private GamePlayer[] m_gamePlayers;
    [SerializeField]
    private int m_targetScore;
    [SerializeField]
    private float m_probabilityDataIsCurrent;

    List<DataRow> m_dataPool = new List<DataRow>();

    DataRow m_currentData = null;

    public DataRow GetCurrentData()
    {
        return m_currentData;
    }

    public DataRow GetRandomData()
    {
        return m_dataPool [Random.Range(0, m_dataPool.Count)];
    }

    public int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }
    
    public void CharacterSelected(int characterIndex)
    {
        for (int index = 0; index < GetNumPlayers(); ++index)
        {
            m_gamePlayers[index].HideCharacter(characterIndex);
        }
    }

	// Use this for initialization
	IEnumerator Start () 
    {
        m_probabilityDataIsCurrent = Mathf.Clamp01(m_probabilityDataIsCurrent);

        if (GetNumPlayers() == 1)
        {
            CharacterSelectionParent.DisableAll();
            SessionInformation.SetDefaultPlayerVar();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            WingroveAudio.WingroveRoot.Instance.PostEvent("INSTRUCTION_CHOOSE_CHARACTER");
        }

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataType = DataHelpers.GameOrDefault(m_dataType);

        m_dataPool = DataHelpers.GetData(m_dataType);
        m_currentData = DataHelpers.GetSingleTargetData(m_dataType);
	}

    IEnumerator PlayGame()
    {
        yield return null;

        int numPlayers = GetNumPlayers();
        
        if (numPlayers == 2)
        {
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
            
            WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_READY_STEADY_GO");
            
            yield return new WaitForSeconds(1.5f);
            
            for (int index = 0; index < numPlayers; ++index)
            {
                m_gamePlayers [index].HideAll();
            }
            
            yield return new WaitForSeconds(1.0f);
        }
    }
    
    IEnumerator CompleteGame()
    {
        yield return null;
    }
}
