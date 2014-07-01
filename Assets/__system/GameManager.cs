using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : Singleton<GameManager> 
{
    string m_programme = "Reading1";
    public static string programme
    {
        get
        {
            return Instance.m_programme;
        }
    }
    
    public void SetProgramme(string myProgramme)
    {
        m_programme = myProgramme;
    }

    string m_activityType = "";
    public static string activityType
    {
        get
        {
            return Instance.m_activityType;
        }
    }

    public void SetActivityType(string myActivityType)
    {
        m_activityType = myActivityType;
    }

    public static IEnumerator WaitForInstance()
    {
        while (GameManager.Instance == null)
        {
            yield return null;
        }
    }

    void Start()
    {
        m_defaultReturnScene = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_startingSceneName;
    }

    // I have created this method so as a pre-emptive measure. 
    // If we need to execute something before starting any games then we can add it here without changing code in any other classes
    public void StartGames()  
    {
        PlayNextGame();
    }

    void PlayNextGame()
    {
        D.Log("GameManager.PlayNextGame()");

        m_state = State.StartGame;

        D.Log("Num Scenes: " + m_gameNames.Count);

        D.Log("m_gameNames.Count: " + m_gameNames.Count);

        m_currentGameName = m_gameNames.Dequeue();

        D.Log("m_currentGameName: " + m_currentGameName);

        DataRow currentGame = DataHelpers.GetGame(m_currentGameName);

        if (currentGame != null && currentGame["labeltext"] != null)
        {
            //string audioEvent = "NAV_" + currentGame ["labeltext"].ToString().ToUpper().Replace(" ", "_").Replace("!", "").Replace("?", "");
            //WingroveAudio.WingroveRoot.Instance.PostEvent(audioEvent);
            ResourcesAudio.Instance.PlayFromResources(currentGame["labeltext"].ToString());
        }

        string sceneName = GameLinker.Instance.GetSceneName(m_currentGameName);

        if (!System.String.IsNullOrEmpty(sceneName))
        {
            TransitionScreen.Instance.ChangeLevel(sceneName, true);
        } 
        else
        {
            CompleteGame();
        }
    }

    void Reset()
    {
        m_state = State.Sleep;

        m_data.Clear();
        m_targetData.Clear();
        m_gameNames.Clear();

        m_currentGameName = "";
        m_returnScene = "";

        SessionInformation.Instance.SetNumPlayers(1);
    }

    public void CompleteGame()
    {
        D.Log("GameManager.CompleteGame()");

        D.Log(System.String.Format("{0} scenes remaining", m_gameNames.Count));

        if (m_gameNames.Count == 0)
        {
            string returnScene = System.String.IsNullOrEmpty(m_returnScene) ? m_defaultReturnScene : m_returnScene;

            Reset();

            if(PrivateCompletedAll != null)
            {
                PrivateCompletedAll();
            }

            TransitionScreen.Instance.ChangeLevel(returnScene, false);
        } 
        else
        {
            if(PrivateCompletedSingle != null)
            {
                PrivateCompletedSingle();
            }

            PlayNextGame();
        }
    }

    void OnLevelWasLoaded()
    {
        switch (m_state)
        {
            case State.Sleep:
                break;
            case State.StartGame:
                if(Application.loadedLevelName != "DeliberatelyEmptyScene")
                {
                    m_state = State.Wait;
                }
                break;
            case State.Wait:
                m_state = State.Cancelling;
                break;
            case State.Cancelling: // The purpose of State.Cancelling is so that PrivateCancelling is only called in the scene after DeliberatelyEmptyScene, therefore, we know what scene we cancelled into
                if(PrivateCancelling != null)
                {
                    PrivateCancelling();
                }
                Reset();
                break;
        }
    }

    Queue<string> m_gameNames = new Queue<string>();
    string m_currentGameName;

    public string currentGameName
    {
        get
        {
            return m_currentGameName;
        }
    }

    public void AddGames(DataRow[] games)
    {
        foreach(DataRow game in games)
        {
            m_gameNames.Enqueue(game["name"].ToString());
        }
    }

    public void AddGame(DataRow game)
    {
        m_gameNames.Enqueue(game["name"].ToString());
    }

    public void AddGames(string[] gameNames)
    {
        foreach(string gameName in gameNames)
        {
            m_gameNames.Enqueue(gameName);
        }
    }

    public void AddGame(string gameName)
    {
        m_gameNames.Enqueue(gameName);
    }


    Dictionary<DataRow, string> m_data = new Dictionary<DataRow, string>();
    Dictionary<DataRow, string> m_targetData = new Dictionary<DataRow, string>();

    public void AddData(string type, List<DataRow> newData)
    {
        PrivateAddData(type, newData, m_data);
    }

    public void AddData(string type, DataRow newData)
    {
        List<DataRow> tempData = new List<DataRow>();
        tempData.Add(newData);
        PrivateAddData(type, tempData, m_data);
    }

    public void AddTargetData(string type, List<DataRow> newTargetData)
    {
        PrivateAddData(type, newTargetData, m_targetData);
    }

    public void AddTargetData(string type, DataRow target)
    {
        List<DataRow> newTargetData = new List<DataRow>();
        newTargetData.Add(target);
        PrivateAddData(type, newTargetData, m_targetData);
    }

    void PrivateAddData(string type, List<DataRow> newData, Dictionary<DataRow, string> data)
    {
        foreach (DataRow newDatum in newData)
        {
            data[newDatum] = type;
        }
    }

    public void SetData(Dictionary<DataRow, string> newData)
    {
        m_data = newData;
    }

    public void SetData(string dataType, List<DataRow> newDataPool)
    {
        m_data.Clear();

        foreach (DataRow row in newDataPool)
        {
            m_data[row] = dataType;
        }
    }

    public void SetTargetData(Dictionary<DataRow, string> newTargetData)
    {
        m_targetData = newTargetData;
    }

    public List<DataRow> GetData(string type)
    {
        return PrivateGetData(type, m_data);
    }

    public List<DataRow> GetTargetData(string type)
    {
        return PrivateGetData(type, m_targetData);
    }

    public DataRow GetSingleTargetData(string type)
    {
        DataRow typeMatch = null;
        
        foreach (KeyValuePair<DataRow, string> kvp in m_targetData)
        {
            if(kvp.Value == type)
            {
                typeMatch = kvp.Key;
                break;
            }
        }
        
        return typeMatch;
    }

    List<DataRow> PrivateGetData(string type, Dictionary<DataRow, string> data)
    {
        List<DataRow> typeMatches = new List<DataRow>();

        foreach (KeyValuePair<DataRow, string> kvp in data)
        {
            if(kvp.Value == type)
            {
                typeMatches.Add(kvp.Key);
            }
        }

        return typeMatches;
    }

    string m_defaultReturnScene;
    string m_returnScene;
    
    public void SetReturnScene(string returnScene)
    {
        m_returnScene = returnScene;
    }

    enum State
    {
        Sleep,
        Wait,
        StartGame,
        Cancelling
    }

    State m_state = State.Sleep;

    public delegate void GameManagerEventHandler ();

    private event GameManagerEventHandler PrivateCompletedAll;
    public event GameManagerEventHandler CompletedAll
    {
        add
        {
            if(PrivateCompletedAll == null || !PrivateCompletedAll.GetInvocationList().Contains(value))
            {
                PrivateCompletedAll += value;
            }
        }

        remove
        {
            PrivateCompletedAll -= value;
        }
    }

    private event GameManagerEventHandler PrivateCompletedSingle;
    public event GameManagerEventHandler CompletedSingle
    {
        add
        {
            if(PrivateCompletedSingle == null || !PrivateCompletedSingle.GetInvocationList().Contains(value))
            {
                PrivateCompletedSingle += value;
            }
        }
        
        remove
        {
            PrivateCompletedSingle -= value;
        }
    }

    private event GameManagerEventHandler PrivateCancelling;
    public event GameManagerEventHandler Cancelling
    {
        add
        {
            if(PrivateCancelling == null || !PrivateCancelling.GetInvocationList().Contains(value))
            {
                PrivateCancelling += value;
            }
        }

        remove
        {
            PrivateCancelling -= value;
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && m_state == State.Wait)
        {
            CompleteGame();
        }
    }
#endif
}
