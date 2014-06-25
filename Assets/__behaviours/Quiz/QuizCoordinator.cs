using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class QuizCoordinator : MonoBehaviour 
{
    [SerializeField]
    private UITexture m_questionTexture;
    [SerializeField]
    private UILabel m_questionLabel;
    [SerializeField]
    private GameObject m_answerLabelPrefab;
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;
    [SerializeField]
    private int m_targetScore;
    [SerializeField]
    private Transform[] m_locators;

    int m_numAnswered = 0;
    int m_score = 0;

    List<GameWidget> m_spawnedAnswers = new List<GameWidget>();

    List<DataRow> m_dataPool = new List<DataRow>();

    DataRow m_currentQuestion = null;

    float m_startTime;

    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetQuizQuestions();

#if UNITY_EDITOR
        if(m_dataPool.Count == 0)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from quizquestions WHERE story_id=" + 48);
            if(dt.Rows.Count > 0)
            {
                m_dataPool = dt.Rows;
            }
        }
#endif

        if (m_targetScore > m_dataPool.Count)
        {
            m_targetScore = m_dataPool.Count;
        }

        m_scoreKeeper.SetTargetScore(m_targetScore);

        if (m_dataPool.Count > 0)
        {
            m_startTime = Time.time;
            AskQuestion();
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
    }

    void AskQuestion()
    {
        m_currentQuestion = m_dataPool [Random.Range(0, m_dataPool.Count)];

        m_dataPool.Remove(m_currentQuestion);

        if (m_currentQuestion ["question"] != null)
        {
            m_questionLabel.text = m_currentQuestion["question"].ToString();
            NGUIHelpers.MaxLabelWidth(m_questionLabel, 1600);
        }

        Texture2D tex = DataHelpers.GetPicture(m_currentQuestion);
        m_questionTexture.mainTexture = tex;
        //m_questionTexture.MakePixelPerfect();
        m_questionTexture.gameObject.SetActive(tex != null);

        List<string> answerStrings = new List<string>();

        if (m_currentQuestion ["dummyanswer1"] != null)
        {
            answerStrings.Add(m_currentQuestion ["dummyanswer1"].ToString());
        }

        if (m_currentQuestion ["dummyanswer2"] != null)
        {
            answerStrings.Add(m_currentQuestion ["dummyanswer2"].ToString());
        }

        if (m_currentQuestion ["correctanswer"] != null)
        {
            answerStrings.Add(m_currentQuestion ["correctanswer"].ToString());

            CollectionHelpers.Shuffle(answerStrings);

            for(int i = 0; i < answerStrings.Count && i < m_locators.Length; ++i)
            {
                GameObject newAnswer = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_answerLabelPrefab, m_locators[i]);
                GameWidget answerBehaviour = newAnswer.GetComponent<GameWidget>() as GameWidget;
                answerBehaviour.SetUp(answerStrings[i], true);
                answerBehaviour.Released += OnAnswer;
                answerBehaviour.EnableDrag(false);
                m_spawnedAnswers.Add(answerBehaviour);
            }
        }
        else
        {
            OnAnswer(null);
        }
    }

    void OnAnswer(GameWidget answer)
    {
        bool isCorrect = (answer == null || answer.labelText == m_currentQuestion ["correctanswer"].ToString());

        int scoreIncrease = isCorrect ? 1 : 0;

        m_score += scoreIncrease;
        m_scoreKeeper.UpdateScore(scoreIncrease);

        for (int i = m_spawnedAnswers.Count - 1; i > -1; --i)
        {
            m_spawnedAnswers[i].Off();
        }
        
        m_spawnedAnswers.Clear();

        ++m_numAnswered;

        if (m_numAnswered < m_targetScore && m_dataPool.Count > 0)
        {
            AskQuestion();
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
    }

    IEnumerator CompleteGame()
    {
        float timeTaken = Time.time - m_startTime;

        ScoreInfo.Instance.NewScore(timeTaken, m_score, m_targetScore, ScoreInfo.CalculateScoreStars(m_score, m_targetScore));

        yield return new WaitForSeconds(2f);

        GameManager.Instance.CompleteGame();
    }
}
