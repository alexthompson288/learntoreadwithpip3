using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class NumberQuizCoordinator : GameCoordinator 
{
    [SerializeField]
    private int m_numAnswersToSpawn = 3;
    [SerializeField]
    private GameObject m_answerLabelPrefab;
    [SerializeField]
    private Transform[] m_answerLocators;
    [SerializeField]
    private GameObject m_questionSpritePrefab;
    [SerializeField]
    private string[] m_questionSpriteNames;
    [SerializeField]
    private Transform[] m_questionLocators;
    [SerializeField]
    private GameObject m_questionParent;
    [SerializeField]
    private float m_scaleTweenDuration = 0.3f;

    int m_highestNumber = 0;

    int m_currentNumber = 0;

    List<GameWidget> m_spawnedAnswers = new List<GameWidget>();

    List<GameObject> m_spawnedQuestionSprites = new List<GameObject>();

    bool m_hasAnsweredIncorrectly = false;

    int FindRandomNumber()
    {
        return Random.Range(1, m_highestNumber + 1);
    }

	IEnumerator Start () 
    {
        m_scoreKeeper.SetTargetScore(m_targetScore);

        System.Array.Sort(m_questionLocators, CollectionHelpers.ComparePosYThenX);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        //m_highestNumber = DataHelpers.GetHighestNumber();
        m_highestNumber = 10;
        m_highestNumber = Mathf.Min(m_highestNumber, m_questionLocators.Length);

        m_numAnswersToSpawn = Mathf.Min(m_numAnswersToSpawn, m_answerLocators.Length);

        AskQuestion();
	}

    void AskQuestion()
    {
        m_currentNumber = FindRandomNumber();

        string spriteName = m_questionSpriteNames [Random.Range(0, m_questionSpriteNames.Length)];

        for (int i = 0; i < m_currentNumber; ++i)
        {
            GameObject newQuestionObject = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_questionSpritePrefab, m_questionLocators[i]);
            m_spawnedQuestionSprites.Add(newQuestionObject);
            newQuestionObject.GetComponent<UISprite>().spriteName = spriteName;
        }

        HashSet<int> answers = new HashSet<int>();

        answers.Add(m_currentNumber);

        while (answers.Count < m_numAnswersToSpawn)
        {
            answers.Add(FindRandomNumber());
        }

        CollectionHelpers.Shuffle(m_answerLocators);

        int locatorIndex = 0;
        foreach (int answer in answers)
        {
            GameObject newAnswer = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_answerLabelPrefab, m_answerLocators[locatorIndex]);

            GameWidget widget = newAnswer.GetComponent<GameWidget>() as GameWidget;
            widget.SetUp(answer.ToString(), false);
            widget.onAll += OnAnswer;
            m_spawnedAnswers.Add(widget);

            ++locatorIndex;

        }

        iTween.ScaleTo(m_questionParent, Vector3.one, m_scaleTweenDuration);
    }

    void OnAnswer(GameWidget widget)
    {
        bool isCorrect = System.Convert.ToInt32(widget.labelText) == m_currentNumber;

        if (isCorrect)
        {
            ++m_numAnswered;

            int scoreDelta = m_hasAnsweredIncorrectly ? 0 : 1;
            m_scoreKeeper.UpdateScore(scoreDelta);
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");

            StartCoroutine(ClearQuestion());
        } 
        else
        {
            m_hasAnsweredIncorrectly = true;
            widget.TweenToStartPos();
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
        }
    }

    IEnumerator ClearQuestion()
    {
        m_hasAnsweredIncorrectly = false;

        for(int i = m_spawnedAnswers.Count - 1; i > -1; --i)
        {
            m_spawnedAnswers[i].Off();
        }

        m_spawnedAnswers.Clear();

        iTween.ScaleTo(m_questionParent, Vector3.zero, m_scaleTweenDuration);

        yield return new WaitForSeconds(m_scaleTweenDuration);

        for (int i = m_spawnedQuestionSprites.Count - 1; i > -1; --i)
        {
            Destroy(m_spawnedQuestionSprites[i]);
        }

        m_spawnedQuestionSprites.Clear();

        if(m_numAnswered >= m_targetScore)
        {
            StartCoroutine(CompleteGame());
        }
        else
        {
            AskQuestion();
        }
    }
}
