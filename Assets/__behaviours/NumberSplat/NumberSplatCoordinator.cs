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
            AskQuestion();
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
	}

    void AskQuestion()
    {
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
            GameObject newNumber = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_numberPrefab, m_locators[locatorIndex]);

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

        yield return new WaitForSeconds(tweenDuration);

        if (m_score < m_targetScore)
        {
            AskQuestion();
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
    }
}
