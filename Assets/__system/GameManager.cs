using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : Singleton<GameManager> 
{
    string m_currentProgramme = "Reading1";
    public static string currentProgramme
    {
        get
        {
            return Instance.m_currentProgramme;
        }
    }
    
    public void SetCurrentProgramme(string newProgramme)
    {
        m_currentProgramme = newProgramme;
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
        m_state = State.StartGame;

        Debug.Log("Num Scenes: " + m_gameNames.Count);

        Debug.Log("m_gameNames.Count: " + m_gameNames.Count);

        m_currentGameName = m_gameNames.Dequeue();

        Debug.Log("m_currentGameName: " + m_currentGameName);

        DataRow currentGame = DataHelpers.FindGame(m_currentGameName);

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
        m_scoreLevel = "";
    }

    public void CompleteGame(bool won = true, string setsScene = "NewScoreDanceScene") // TODO: Deprecate the parameters passed to this method
    {
        Debug.Log("GameManager.CompleteGame()");

        Debug.Log(System.String.Format("{0} scenes remaining", m_gameNames.Count));

        if (m_gameNames.Count == 0)
        {
            string returnScene = System.String.IsNullOrEmpty(m_returnScene) ? m_defaultReturnScene : m_returnScene;

            Reset();

            if(onComplete != null)
            {
                onComplete();
            }

            TransitionScreen.Instance.ChangeLevel(returnScene, false);
        } 
        else
        {
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
                Reset();
                if(onCancel != null)
                {
                    onCancel();
                }
                break;
        }
    }

    string m_scoreLevel = "";

    public string GetScoreLevel()
    {
        return m_scoreLevel;
    }

    public void SetScoreLevel(string scoreLevel)
    {
        m_scoreLevel = scoreLevel;
    }

    //Queue<DataRow> m_gameNames = new Queue<DataRow>();
    //DataRow m_currentGame;
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


    string m_dataType = "";

    public string dataType
    {
        get
        {
            return m_dataType;
        }
    }

    public void SetDataType(string type)
    {
        m_dataType = type;
    }

    public string textAttribute
    {
        get
        {
            switch(m_dataType)
            {
                case "phonemes":
                    return "phoneme";
                    break;
                case "words":
                    return "word";
                    break;
                case "keywords":
                    return "word";
                    break;
                case "stories":
                    return "title";
                    break;
                default:
                    return "default";
                    break;
            }
        }
    }

    enum State
    {
        Sleep,
        Wait,
        StartGame
    }

    State m_state = State.Sleep;

    // TODO: Move these events back to the top of the class. I have moved them to the bottom so that it is easier to write the rest of the class
    public delegate void Complete ();
    private event Complete onComplete;
    public event Complete OnComplete
    {
        add
        {
            if(onComplete == null || !onComplete.GetInvocationList().Contains(value))
            {
                onComplete += value;
            }
        }

        remove
        {
            onComplete -= value;
        }
    }

    public delegate void Cancel ();
    private event Cancel onCancel;
    public event Cancel OnCancel
    {
        add
        {
            if(onCancel == null || !onCancel.GetInvocationList().Contains(value))
            {
                onCancel += value;
            }
        }

        remove
        {
            onCancel -= value;
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
