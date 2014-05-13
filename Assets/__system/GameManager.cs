﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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

    // I have created this method so as a pre-emptive measure. If we need to execute something before starting any games then we can add it here without changing code in any other classes
    public void StartGames()  
    {
        m_currentGame = "";

        PlayNextGame();
    }

    void PlayNextGame()
    {
        m_state = State.StartGame;

        if (!System.String.IsNullOrEmpty(m_currentGame))
        {
            m_gameDictionary.Remove(m_currentGame);
        }

        m_currentGame = "";

        Debug.Log("Num Scenes: " + m_gameDictionary.Count);

        string nextScene = "";

        foreach (DictionaryEntry game in m_gameDictionary)
        {
            m_currentGame = (string)game.Key;
            nextScene = (string)game.Value;
            Debug.Log(System.String.Format("NextGame: {0} - {1}", m_currentGame, nextScene));
            break;
        }

        if (!System.String.IsNullOrEmpty(nextScene))
        {
            TransitionScreen.Instance.ChangeLevel(nextScene, true);
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
        m_gameDictionary.Clear();

        m_currentGame = "";
        m_returnScene = "";
    }

    public void CompleteGame(bool won = true, string setsScene = "NewScoreDanceScene") // TODO: Deprecate the parameters passed to this method
    {
        Debug.Log("GameManager.CompleteGame()");

        Debug.Log(System.String.Format("{0} scenes remaining", m_gameDictionary.Count));

        if (m_gameDictionary.Count == 0)
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

    // TODO: This does not need to be a Dictionary. Store a queue of game names, find the scene name when you need to load it
    //Queue<string> m_gameDictionary = new Queue<string>();
    OrderedDictionary m_gameDictionary = new OrderedDictionary();

    string m_currentGame;

    /*
    public void AddScenes(string scene)
    {
        AddScenes(new string [] { scene });
    }

    public void AddScenes(string[] scenes)
    {
        foreach (string scene in scenes)
        {
            Debug.Log("Adding scene: " + scene);
            //m_gameDictionary.Enqueue(scene);
        }
    }
    */

    public string GetCurrentGame()
    {
        return m_currentGame;
    }

    public void AddGames(OrderedDictionary games)
    {
        foreach(DictionaryEntry game in games)
        {
            m_gameDictionary.Add(game.Key, game.Value);
        }
    }

    public void AddGames(string dbGameName, string sceneName)
    {
        OrderedDictionary gameDictionary = new OrderedDictionary();
        gameDictionary.Add(dbGameName, sceneName);
        AddGames(gameDictionary);
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
