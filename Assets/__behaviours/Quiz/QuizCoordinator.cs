using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class QuizCoordinator : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_questionPictureParent;
    [SerializeField]
    private UITexture m_questionPicture;
    [SerializeField]
    private GameObject m_questionLabelParent;
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

    List<GameWidget> m_spawnedAnswers = new List<GameWidget>();

    List<DataRow> m_dataPool = new List<DataRow>();

    DataRow m_currentQuestion = null;

    float m_startTime;

    Vector3 m_pictureOnPos;
    Vector3 m_pictureBeforePos;

    Vector3 m_labelOnPos;
    Vector3 m_labelBeforePos;

    float m_widgetOffDistance = 3;

    bool m_hasAnswered = false;

    IEnumerator Start()
    {
        D.Log("QuizCoordinator.Start()");

        m_pictureOnPos = m_questionPictureParent.transform.position;
        m_pictureBeforePos = new Vector3(m_pictureOnPos.x - m_widgetOffDistance, m_pictureOnPos.y, m_pictureOnPos.z);
        m_questionPictureParent.transform.position = m_pictureBeforePos;

        m_labelOnPos = m_questionLabelParent.transform.position;
        m_labelBeforePos = new Vector3(m_labelOnPos.x - m_widgetOffDistance, m_labelOnPos.y, m_labelOnPos.z);
        m_questionLabelParent.transform.position = m_labelBeforePos;

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

        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());

        if (m_dataPool.Count > 0)
        {
            m_startTime = Time.time;
            StartCoroutine(AskQuestion());
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
    }

    IEnumerator AskQuestion(float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        m_hasAnswered = false;

        m_questionPictureParent.transform.position = m_pictureBeforePos;
        m_questionLabelParent.transform.position = m_labelBeforePos;

        m_currentQuestion = m_dataPool [Random.Range(0, m_dataPool.Count)];

        m_dataPool.Remove(m_currentQuestion);

        if (m_currentQuestion ["question"] != null)
        {
            m_questionLabel.text = m_currentQuestion["question"].ToString();
            NGUIHelpers.MaxLabelWidth(m_questionLabel, 1600);
        }

        Texture2D tex = DataHelpers.GetPicture(m_currentQuestion);
        m_questionPicture.mainTexture = tex;
        //m_questionPicture.MakePixelPerfect();
        m_questionPicture.gameObject.SetActive(tex != null);


        float tweenDuration = 0.5f;

        iTween.MoveTo(m_questionPictureParent, m_pictureOnPos, tweenDuration);
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");

        yield return new WaitForSeconds(tweenDuration / 2);

        iTween.MoveTo(m_questionLabelParent, m_labelOnPos, tweenDuration);
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");

        yield return new WaitForSeconds(tweenDuration / 2);

        
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
                answerBehaviour.Unpressed += OnAnswer;
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
        if (!m_hasAnswered)
        {
            m_hasAnswered = true;

            bool isCorrect = (answer == null || answer.labelText == m_currentQuestion ["correctanswer"].ToString());

            int scoreDelta = isCorrect ? 1 : 0;
            m_scoreKeeper.UpdateScore(scoreDelta);

            for (int i = m_spawnedAnswers.Count - 1; i > -1; --i)
            {
                m_spawnedAnswers [i].Off();
            }
            
            m_spawnedAnswers.Clear();

            float tweenDuration = 0.5f;

            Vector3 pictureOffPos = new Vector3(m_pictureOnPos.x + m_widgetOffDistance, m_pictureOnPos.y, m_pictureOnPos.z);
            iTween.MoveTo(m_questionPictureParent, pictureOffPos, tweenDuration);

            Vector3 labelOffPos = new Vector3(m_labelOnPos.x + m_widgetOffDistance, m_labelOnPos.y, m_labelOnPos.z);
            iTween.MoveTo(m_questionLabelParent, labelOffPos, tweenDuration);


            if (!m_scoreKeeper.HasCompleted() && m_dataPool.Count > 0)
            {
                StartCoroutine(AskQuestion(tweenDuration + 0.1f));
            } 
            else
            {
                StartCoroutine(CompleteGame());
            }
        }
    }

    IEnumerator CompleteGame()
    {
        float timeTaken = Time.time - m_startTime;

        ScoreInfo.Instance.NewScore(timeTaken, m_scoreKeeper.GetScore(), m_targetScore, ScoreInfo.CalculateScoreStars(m_scoreKeeper.GetScore(), m_targetScore));

        yield return new WaitForSeconds(2f);

        GameManager.Instance.CompleteGame();
    }
}
