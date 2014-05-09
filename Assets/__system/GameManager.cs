﻿using UnityEngine;
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

    public static IEnumerator WaitForGameManager()
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
        PlayNextGame();
    }

    void PlayNextGame()
    {
        m_state = State.StartGame;

        Debug.Log("Num Scenes: " + m_scenes.Count);
        Debug.Log("Next Scene: " + m_scenes.Peek());

        string nextScene = m_scenes.Dequeue();

        TransitionScreen.Instance.ChangeLevel(nextScene, true);
    }

    public void CompleteGame(bool won = true, string setsScene = "NewScoreDanceScene") // TODO: Deprecate the parameters passed to this method
    {
        Debug.Log("GameManager.CompleteGame()");

        Debug.Log(System.String.Format("{0} scenes remaining", m_scenes.Count));

        if (m_scenes.Count == 0)
        {
            m_state = State.Sleep;

            if(onComplete != null)
            {
                onComplete();
            }

            string returnScene = System.String.IsNullOrEmpty(m_returnScene) ? m_defaultReturnScene : m_returnScene;

            m_returnScene = ""; // Reset return scene so that if it is improperly set next time, we will return to the default return scene

            TransitionScreen.Instance.ChangeLevel(returnScene, false);
        } 
        else
        {
            m_state = State.StartGame;
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
                m_state = State.Sleep;
                if(onCancel != null)
                {
                    onCancel();
                }
                break;
        }
    }

    Queue<string> m_scenes = new Queue<string>();

    public void SetScenes(string scene)
    {
        SetScenes(new string [] { scene });
    }

    public void SetScenes(string[] scenes)
    {
        foreach (string scene in scenes)
        {
            Debug.Log("Adding scene: " + scene);
            m_scenes.Enqueue(scene);
        }
    }


    Dictionary<DataRow, string> m_data = new Dictionary<DataRow, string>();
    Dictionary<DataRow, string> m_targetData = new Dictionary<DataRow, string>();

    public void ClearAllData()
    {
        m_data.Clear();
        m_targetData.Clear();
    }

    public void AddData(string type, List<DataRow> newData)
    {
        AddData(type, newData, m_data);
    }

    public void AddTargetData(string type, List<DataRow> newTargetData)
    {
        AddData(type, newTargetData, m_targetData);
    }

    public void AddTargetData(string type, DataRow target)
    {
        List<DataRow> newTargetData = new List<DataRow>();
        newTargetData.Add(target);
        AddData(type, newTargetData, m_targetData);
    }

    void AddData(string type, List<DataRow> newData, Dictionary<DataRow, string> data)
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
        return GetData(type, m_data);
    }

    public List<DataRow> GetTargetData(string type)
    {
        return GetData(type, m_targetData);
    }

    List<DataRow> GetData(string type, Dictionary<DataRow, string> data)
    {
        List<DataRow> typeMatches = new List<DataRow>();

        foreach (KeyValuePair<DataRow, string> kvp in data)
        {
            if(kvp.Value == type)
            {
                typeMatches.Add(kvp.Key);
            }
        }

        //Dictionary<DataRow, string> typeMatches = m_data.Where(kvp => kvp.Value == type);

        //var duplicateValues = plants.GroupBy(x => x.Value).Where(x => x.Count() > 1);
        //var typeMatches = m_data.GroupBy(x => x.Value).Where(x => x.Count() > 1);

        //return typeMatches.Keys.ToList();

        return typeMatches;
    }

    public DataRow GetSingleData(string type)
    {
        return GetSingleData(type, m_data);
    }

    public DataRow GetSingleTargetData(string type, bool guaranteeData = false)
    {
        DataRow data = GetSingleData(type, m_targetData);

        if (data == null && guaranteeData)
        {
            data = GetSingleData(type, m_data);
        }

        return data;
    }

    DataRow GetSingleData(string type, Dictionary<DataRow, string> data)
    {
        DataRow typeMatch = null;
        
        foreach (KeyValuePair<DataRow, string> kvp in data)
        {
            if(kvp.Value == type)
            {
                typeMatch = kvp.Key;
                break;
            }
        }
        
        return typeMatch;
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