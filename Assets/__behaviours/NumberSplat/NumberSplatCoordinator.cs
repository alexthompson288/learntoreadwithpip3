using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Wingrove;

public class NumberSplatCoordinator : GameCoordinator 
{
    [SerializeField]
    private GameObject m_numberPrefab;
    [SerializeField]
    private Transform[] m_locators;
    
    int m_numAnswers = 5;
    
    List<GameWidget> m_remainingWidgets = new List<GameWidget>();
    
    IEnumerator Start () 
    {
        m_numAnswers = Mathf.Min(m_numAnswers, m_locators.Length);
        
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        
        m_dataPool = DataHelpers.GetNumbers();
        
        m_scoreKeeper.SetTargetScore(m_targetScore);
        
        if (m_dataPool.Count > 0)
        {
            StartCoroutine(AskQuestion());
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
    }

    IEnumerator AskQuestion()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");
        
        yield return new WaitForSeconds(0.1f);
        
        HashSet<DataRow> numbers = new HashSet<DataRow>();

        while (numbers.Count < m_numAnswers)
        {
            numbers.Add(GetRandomData());
        }
        
        CollectionHelpers.Shuffle(m_locators);
        
        int locatorIndex = 0;
        foreach(DataRow number in numbers)
        {
            GameObject newNumber = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_numberPrefab, m_locators[locatorIndex], true);
            
            GameWidget widget = newNumber.GetComponent<GameWidget>() as GameWidget;
            widget.SetUp(number);
            widget.Unpressing += OnAnswer;
            m_remainingWidgets.Add(widget);
            ++locatorIndex;
        }

        m_remainingWidgets.Sort((a, b) => a.data.GetInt("value").CompareTo(b.data.GetInt("value")));
    }
    
    void OnAnswer(GameWidget widget)
    {
        if (m_remainingWidgets.Count > 0)
        {
            bool isCorrect = (widget == m_remainingWidgets [0]);
            
            if (isCorrect)
            {
                WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");
                
                PlayShortAudio(widget.data);

                m_remainingWidgets.Remove(widget);

                foreach(GameWidget remainingWidget in m_remainingWidgets)
                {
                    remainingWidget.TintWhite();
                    remainingWidget.ChangeBackgroundState(false);
                }

                if(m_remainingWidgets.Count == 0)
                {
                    ++m_score;
                    m_scoreKeeper.UpdateScore(1);
                    
                    StartCoroutine(ClearAnswers());
                }
            } 
            else
            {
                WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
                widget.TintGray();
                widget.TweenToStartPos();
            }
        }
    }
    
    IEnumerator ClearAnswers()
    {
        yield return new WaitForSeconds(0.8f);

        GameWidget[] widgets = UnityEngine.Object.FindObjectsOfType(typeof(GameWidget)) as GameWidget[];

        if (widgets.Length > 0)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEAR");

            float tweenDuration = widgets[0].scaleTweenDuration;

            for(int i = widgets.Length - 1; i > -1; --i)
            {
                widgets[i].Off();
            }
            
            yield return new WaitForSeconds(tweenDuration + 0.5f);
        }
            
        if (m_score < m_targetScore)
        {
            StartCoroutine(AskQuestion());
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
    }
}

/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Wingrove;

public class NumberSplatCoordinator : GameCoordinator 
{
    [SerializeField]
    private GameObject m_numberPrefab;
    [SerializeField]
    private Transform[] m_locators;

    int m_numAnswers = 5;

    List<GameWidget> m_spawnedWidgets = new List<GameWidget>();

	IEnumerator Start () 
    {
        m_numAnswers = Mathf.Min(m_numAnswers, m_locators.Length);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetNumbers();

        m_scoreKeeper.SetTargetScore(m_targetScore);

        if (m_dataPool.Count > 0)
        {
            StartCoroutine(AskQuestion());
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
	}

    IEnumerator AskQuestion()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");

        yield return new WaitForSeconds(0.1f);

        HashSet<DataRow> numbers = new HashSet<DataRow>();

        m_currentData = null;

        while (numbers.Count < m_numAnswers)
        {
            DataRow number = GetRandomData();

            if(m_currentData == null || Convert.ToInt32(number["value"]) < Convert.ToInt32(m_currentData["value"]))
            {
                m_currentData = number;
            }

            numbers.Add(number);
        }

        CollectionHelpers.Shuffle(m_locators);

        int locatorIndex = 0;
        foreach(DataRow number in numbers)
        {
            GameObject newNumber = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_numberPrefab, m_locators[locatorIndex], true);

            GameWidget widget = newNumber.GetComponent<GameWidget>() as GameWidget;
            //widget.SetUp("numbers", number, false);
            widget.SetUp(number);
            widget.Unpressing += OnAnswer;
            m_spawnedWidgets.Add(widget);
            ++locatorIndex;
        }
    }

    void OnAnswer(GameWidget widget)
    {
        bool isCorrect = (widget.data == m_currentData);

        if (isCorrect)
        {
            //WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");

            PlayShortAudio();

            ++m_score;
            m_scoreKeeper.UpdateScore(1);

            StartCoroutine(ClearAnswers());
        } 
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            widget.TintGray();
            widget.TweenToStartPos();
        }
    }

    IEnumerator ClearAnswers()
    {
        yield return new WaitForSeconds(0.8f);

        float tweenDuration = m_spawnedWidgets [0].scaleTweenDuration;

        for(int i = m_spawnedWidgets.Count - 1; i > -1; --i)
        {
            m_spawnedWidgets[i].Off();
        }

        m_spawnedWidgets.Clear();

        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEAR");

        yield return new WaitForSeconds(tweenDuration + 0.5f);

        if (m_score < m_targetScore)
        {
            StartCoroutine(AskQuestion());
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
    }
}
*/
