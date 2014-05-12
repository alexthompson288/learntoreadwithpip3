using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorrectCaptionCoordinator : GameCoordinator
{
    [SerializeField]
    private GameObject[] m_questionTweeners;
    [SerializeField]
    private UITexture m_questionImage;
    [SerializeField]
    private UILabel m_questionLabel;
    [SerializeField]
    private ClickEvent m_yesButton;
    [SerializeField]
    private ClickEvent m_noButton;
    [SerializeField]
    private float m_probabilityYesIsCorrect = 0.5f;
    [SerializeField]
    private float m_questionTweenDuration = 0.25f;

    #if UNITY_EDITOR
    [SerializeField]
    private bool m_useDebugData;
    #endif

    bool m_yesIsCorrect;



    IEnumerator Start()
    {
        m_noButton.SetInt(0);
        m_yesButton.SetInt(1);
        m_noButton.OnSingleClick += OnAnswer;
        m_yesButton.OnSingleClick += OnAnswer;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetCorrectCaptions();

#if UNITY_EDITOR
        if(m_useDebugData)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from correctcaptions WHERE programsession_id=" + 304);
            m_dataPool = dt.Rows;
        }
#endif

        for (int i = m_dataPool.Count - 1; i > -1; --i)
        {
            if(GetCaptionTexture(m_dataPool[i]) == null)
            {
                m_dataPool.RemoveAt(i);
            }
        }

        Debug.Log("m_dataPool.Count: " + m_dataPool.Count);

        ClampTargetScore();

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
        m_currentData = GetRandomData();

        if (m_currentData != null)
        {
            m_dataPool.Remove(m_currentData);

            Texture2D tex = GetCaptionTexture(m_currentData);

            if (tex != null)
            {
                m_questionImage.mainTexture = tex;
                m_questionImage.MakePixelPerfect();

                m_yesIsCorrect = Random.Range(0f, 1f) > m_probabilityYesIsCorrect;

                if (m_yesIsCorrect)
                {
                    m_questionLabel.text = m_currentData ["good_sentence"].ToString();
                } 
                else
                {
                    int badSentenceId = Random.Range(1, 5);
                    while(m_currentData ["bad_sentence" + badSentenceId.ToString()] == null)
                    {
                        badSentenceId = Random.Range(1, 5);
                        yield return null; // Defensive yield
                    }

                    m_questionLabel.text = m_currentData ["bad_sentence" + badSentenceId.ToString()].ToString();
                }

                foreach (GameObject tweener in m_questionTweeners)
                {
                    iTween.ScaleTo(tweener, Vector3.one, m_questionTweenDuration);
                }

                yield return new WaitForSeconds(m_questionTweenDuration);

                EnableAnswerColliders(true);
            }
            else
            {
                StartCoroutine(AskQuestion());
            }
        }
        else
        {
            StartCoroutine(CompleteGame());
        }

        yield break;
    }

    protected override IEnumerator CompleteGame()
    {
        Debug.Log("CompleteGame()");
        yield break;
    }

    void OnAnswer(ClickEvent button)
    {
        StartCoroutine(OnAnswerCo(button));
    }

    IEnumerator OnAnswerCo(ClickEvent button)
    {
        EnableAnswerColliders(false);
        
        bool answerIsYes = button.GetInt() == 1; // Separate bool for readability
        bool isCorrect = (m_yesIsCorrect == answerIsYes);
        
        int scoreDelta = isCorrect ? 1 : 0;
        
        m_scoreKeeper.UpdateScore(scoreDelta);
        m_score += scoreDelta;

        foreach (GameObject tweener in m_questionTweeners)
        {
            iTween.ScaleTo(tweener, Vector3.zero, m_questionTweenDuration);
        }

        yield return new WaitForSeconds(m_questionTweenDuration);

        if (m_score < m_targetScore && m_dataPool.Count > 0)
        {
            StartCoroutine(AskQuestion());
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
    }

    Texture2D GetCaptionTexture(DataRow data)
    {
        Texture2D tex = Resources.Load<Texture2D>(data ["image"].ToString());

        if (tex == null)
        {
            tex = Resources.Load<Texture2D>("Images/storypages/" + data ["image"].ToString());
        }

        return tex;
    }

    void EnableAnswerColliders(bool enable)
    {
        m_yesButton.EnableCollider(enable);
        m_noButton.EnableCollider(enable);
    }
}
