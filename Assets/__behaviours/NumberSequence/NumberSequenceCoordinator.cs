using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class NumberSequenceCoordinator : GameCoordinator
{
    [SerializeField]
    private Transform[] m_sequenceLocators;
    [SerializeField]
    private int m_numInSequence = 5;
    [SerializeField]
    private GameObject m_numberPrefab;
    [SerializeField]
    private Transform[] m_answerLocators;
    [SerializeField]
    private int m_numAnswers;

    int m_highestNumber = 0;

    int m_currentNumberIndex = 0;

    List<GameWidget> m_spawnedAnswers = new List<GameWidget>();
    List<GameWidget> m_spawnedSequenceNumbers = new List<GameWidget>();

    bool m_hasAnsweredIncorrectly = false;


    IEnumerator Start()
    {
        m_scoreKeeper.SetTargetScore(m_targetScore);

        m_numInSequence = Mathf.Min(m_numInSequence, m_sequenceLocators.Length);
        m_numAnswers = Mathf.Min(m_numAnswers, m_answerLocators.Length);

        System.Array.Sort(m_sequenceLocators, CollectionHelpers.LeftToRight);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetNumbers();

        AskQuestion();
    }

    void AskQuestion()
    {
        int index = Random.Range(0, m_dataPool.Count - m_sequenceLocators.Length);

        List<DataRow> numberSequence = new List<DataRow>();

        while (numberSequence.Count < m_numInSequence)
        {
            numberSequence.Add(m_dataPool[index]);
            ++index;
        }

        m_currentNumberIndex = Random.Range(0, numberSequence.Count);

        for(int i = 0; i < numberSequence.Count && i < m_sequenceLocators.Length; ++i)
        {
            if(i != m_currentNumberIndex)
            {
                GameObject newGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_numberPrefab, m_sequenceLocators[i]);

                GameWidget widget = newGo.GetComponent<GameWidget>() as GameWidget;
                widget.SetUp(numberSequence[i]);
                widget.EnableDrag(false);
                m_spawnedSequenceNumbers.Add(widget);
            }
        }

        m_currentData = numberSequence [m_currentNumberIndex];

        HashSet<DataRow> answers = new HashSet<DataRow>();
        answers.Add(m_currentData);

        while (answers.Count < m_numAnswers)
        {
            answers.Add(GetRandomData());
        }

        CollectionHelpers.Shuffle(m_answerLocators);

        int locatorIndex = 0;
        foreach (DataRow answer in answers)
        {
            GameObject newAnswer = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_numberPrefab, m_answerLocators[locatorIndex]);

            GameWidget widget = newAnswer.GetComponent<GameWidget>() as GameWidget;

            widget.SetUp(answer);
            widget.AllReleaseInteractions += OnAnswer;
            m_spawnedAnswers.Add(widget);

            ++locatorIndex;
        }
    }

    void OnAnswer(GameWidget widget)
    {
        bool isCorrect = widget.data == m_currentData;

        if (isCorrect)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");

            ++m_numAnswered;

            int scoreDelta = m_hasAnsweredIncorrectly ? 0 : 1; 
            m_scoreKeeper.UpdateScore(scoreDelta);

            widget.TweenToPos(m_sequenceLocators[m_currentNumberIndex].position);

            StartCoroutine(ClearQuestions());
        } 
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");

            m_hasAnsweredIncorrectly = true;

            widget.TweenToStartPos();
        }
    }

    IEnumerator ClearQuestions()
    {
        yield return new WaitForSeconds(1f);

        m_hasAnsweredIncorrectly = false;

        CollectionHelpers.DestroyObjects(m_spawnedAnswers, true);
        CollectionHelpers.DestroyObjects(m_spawnedSequenceNumbers, true);

        yield return new WaitForSeconds(0.5f);

        if (m_numAnswered >= m_targetScore)
        {
            StartCoroutine(CompleteGame());
        } 
        else
        {
            AskQuestion();
        }
    }
}
