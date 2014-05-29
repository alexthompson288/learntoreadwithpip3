using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class NumberSequenceCoordinator : GameCoordinator
{
    [SerializeField]
    private GameObject m_sequenceParent;
    [SerializeField]
    private UILabel[] m_sequenceWidgets;
    [SerializeField]
    private int m_numInSequence = 5;
    [SerializeField]
    private GameObject m_answerPrefab;
    [SerializeField]
    private Transform[] m_answerLocators;
    [SerializeField]
    private int m_numAnswers;
    [SerializeField]
    private float m_scaleTweenDuration = 0.35f;

    int m_highestNumber = 0;

    int m_currentNumber = 0;
    int m_currentNumberIndex = 0;

    List<GameWidget> m_spawnedAnswers = new List<GameWidget>();

    bool m_hasAnsweredIncorrectly = false;

    public int FindLinear(int previous, int increment)
    {
        return previous + increment;
    }

    IEnumerator Start()
    {
        m_scoreKeeper.SetTargetScore(m_targetScore);

        m_numInSequence = Mathf.Min(m_numInSequence, m_sequenceWidgets.Length);
        m_numAnswers = Mathf.Min(m_numAnswers, m_answerLocators.Length);

        System.Array.Sort(m_sequenceWidgets, CollectionHelpers.ComparePosX);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_highestNumber = DataHelpers.GetHighestNumberValue();

        AskQuestion();
    }

    void AskQuestion()
    {
        int lowestNumber = Random.Range(1, m_highestNumber - 4);

        List<int> numberSequence = new List<int>();
        numberSequence.Add(lowestNumber);

        while (numberSequence.Count < m_numInSequence)
        {
            numberSequence.Add(FindLinear(numberSequence[numberSequence.Count - 1], 1));
        }

        m_currentNumberIndex = Random.Range(0, numberSequence.Count);

        for(int i = 0; i < numberSequence.Count && i < m_sequenceWidgets.Length; ++i)
        {
            m_sequenceWidgets[i].text = numberSequence[i].ToString();
            m_sequenceWidgets[i].gameObject.SetActive(i != m_currentNumberIndex);
        }

        m_currentNumber = numberSequence [m_currentNumberIndex];

        HashSet<int> answers = new HashSet<int>();
        answers.Add(m_currentNumber);

        while (answers.Count < m_numAnswers)
        {
            answers.Add(Random.Range(1, m_highestNumber));
        }

        CollectionHelpers.Shuffle(m_answerLocators);

        int locatorIndex = 0;
        foreach (int answer in answers)
        {
            GameObject newAnswer = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_answerPrefab, m_answerLocators[locatorIndex]);

            GameWidget widget = newAnswer.GetComponent<GameWidget>() as GameWidget;

            widget.SetUp(answer.ToString(), false);
            widget.onAll += OnAnswer;
            m_spawnedAnswers.Add(widget);

            ++locatorIndex;
        }

        iTween.ScaleTo(m_sequenceParent, Vector3.one, m_scaleTweenDuration);
    }

    void OnAnswer(GameWidget widget)
    {
        bool isCorrect = System.Convert.ToInt32(widget.labelText) == m_currentNumber;

        if (isCorrect)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");

            ++m_numAnswered;

            int scoreDelta = m_hasAnsweredIncorrectly ? 0 : 1; 
            m_scoreKeeper.UpdateScore(scoreDelta);

            widget.TweenToPos(m_sequenceWidgets[m_currentNumberIndex].transform.position);

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

        for(int i = m_spawnedAnswers.Count - 1; i > -1; --i)
        {
            m_spawnedAnswers[i].Off();
        }
        
        m_spawnedAnswers.Clear();

        iTween.ScaleTo(m_sequenceParent, new Vector3(0, m_sequenceParent.transform.localScale.y, 0), m_scaleTweenDuration);

        yield return new WaitForSeconds(m_scaleTweenDuration * 2);

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
