using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wingrove;
using System;

public class CorrectCaptionCoordinator : GameCoordinator
{
    [SerializeField]
    private UITexture m_questionImage;
    [SerializeField]
    private GameObject m_questionImageParent;
    [SerializeField]
    private GameObject m_textPrefab;
    [SerializeField]
    private Transform m_textPosition;
    [SerializeField]
    private UILabel m_questionLabel;
    [SerializeField]
    private GameObject m_questionLabelParent;
    [SerializeField]
    private ClickEvent m_yesButton;
    [SerializeField]
    private ClickEvent m_noButton;
    [SerializeField]
    private float m_probabilityYesIsCorrect = 0.5f;
    [SerializeField]
    private float m_questionTweenDuration = 0.25f;
    [SerializeField]
    private List<string> m_captionTextAttributes;

    bool m_hasAnsweredIncorrectly = false;

    List<string> m_remainingAttributes = new List<string>();

    List<GameObject> m_spawnedQuestionText = new List<GameObject>();

    int m_numAnswered = 0;

    IEnumerator Start()
    {
        m_noButton.SetInt(0);
        m_yesButton.SetInt(1);
        m_noButton.OnSingleClick += OnAnswer;
        m_yesButton.OnSingleClick += OnAnswer;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetCorrectCaptions();

#if UNITY_EDITOR
        if(m_dataPool.Count == 0)
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

        m_hasAnsweredIncorrectly = false;

        m_remainingAttributes.Clear();
        m_remainingAttributes.AddRange(m_captionTextAttributes);
        while(m_currentData[m_remainingAttributes.Last()] == null)
        {
            m_remainingAttributes.RemoveAt(m_remainingAttributes.Count - 1);
        }
        CollectionHelpers.Shuffle(m_remainingAttributes);

        if (m_currentData != null)
        {
            m_dataPool.Remove(m_currentData);

            Texture2D tex = GetCaptionTexture(m_currentData);

            if (m_currentData["good_sentence"] != null && tex != null) // if "good_sentence" is null then the player is unable to answer correctly, if tex is null the question is unfair
            {
                m_questionImage.mainTexture = tex;
                m_questionImage.MakePixelPerfect();

                SpawnQuestionText();

                TweenQuestionParents(Vector3.one);

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

    void SpawnQuestionText()
    {
        for (int i = m_spawnedQuestionText.Count - 1; i > -1; --i)
        {
            Destroy(m_spawnedQuestionText [i]);
        }

        m_spawnedQuestionText.Clear();

        string sentence = m_currentData [m_remainingAttributes.Last()].ToString();

        string[] words = sentence.Split(new char[] {' '});

        float length = 0;

        foreach (string word in words)
        {
            if (!string.IsNullOrEmpty(word) && word != " ")
            {
                GameObject newText = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_textPrefab, m_textPosition);

                m_spawnedQuestionText.Add(newText);

                newText.GetComponent<UILabel>().text = word + " ";
                newText.transform.localPosition = new Vector3(length, 0, 0);
                Vector3 wordSize = newText.GetComponent<UILabel>().font.CalculatePrintedSize(word + " ", false, UIFont.SymbolStyle.None);
                length += wordSize.x;

                ShowPipPadForWord showPipPadForWord = newText.GetComponent<ShowPipPadForWord>() as ShowPipPadForWord;
                
                showPipPadForWord.SetUp(word, wordSize, true);
            }
        }
    }
    
    protected override IEnumerator CompleteGame()
    {
        Debug.Log("GameCoordinator.CompleteGame()");
        yield return null;

        GameManager.Instance.CompleteGame();
    }

    void OnAnswer(ClickEvent button)
    {
        StartCoroutine(OnAnswerCo(button));
    }

    IEnumerator OnAnswerCo(ClickEvent button)
    {
        EnableAnswerColliders(false);
        
        bool answerIsYes = button.GetInt() == 1; 
        bool yesIsCorrect = (m_remainingAttributes.Last() == "good_sentence"); // The current attribute is the last one in m_remainingAttributes
        bool isCorrect = (yesIsCorrect == answerIsYes);

        if (isCorrect)
        {
            if(answerIsYes)
            {
                ++m_numAnswered;

                int scoreDelta = (isCorrect && !m_hasAnsweredIncorrectly) ? 1 : 0;
                m_scoreKeeper.UpdateScore(scoreDelta);
                m_score += scoreDelta;

                TweenQuestionParents(Vector3.zero);
                
                yield return new WaitForSeconds(m_questionTweenDuration);
                
                if (m_numAnswered < m_targetScore && m_dataPool.Count > 0)
                {
                    StartCoroutine(AskQuestion());
                } 
                else
                {
                    StartCoroutine(CompleteGame());
                }
            }
            else
            {
                m_remainingAttributes.RemoveAt(m_remainingAttributes.Count - 1);

                iTween.ScaleTo(m_questionLabelParent, Vector3.zero, m_questionTweenDuration);
                WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEARS");

                yield return new WaitForSeconds(m_questionTweenDuration);

                SpawnQuestionText();

                iTween.ScaleTo(m_questionLabelParent, Vector3.one, m_questionTweenDuration);
                WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEARS");
            }
        } 
        else
        {
            m_hasAnsweredIncorrectly = true;
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
        }

        EnableAnswerColliders(true);
    }

    Texture2D GetCaptionTexture(DataRow data)
    {
        Texture2D tex = Resources.Load<Texture2D>(data ["correct_image_name"].ToString());

        if (tex == null)
        {
            tex = Resources.Load<Texture2D>("Images/storypages/" + data ["correct_image_name"].ToString());
        }

        return tex;
    }

    void TweenQuestionParents(Vector3 newScale)
    {
        iTween.ScaleTo(m_questionImageParent, newScale, m_questionTweenDuration);
        iTween.ScaleTo(m_questionLabelParent, newScale, m_questionTweenDuration);

        string audioString = Mathf.Approximately(newScale.x, 0) ? "SOMETHING_APPEARS" : "SOMETHING_DISAPPEARS";
        WingroveAudio.WingroveRoot.Instance.PostEvent(audioString);
    }

    void EnableAnswerColliders(bool enable)
    {
        m_yesButton.EnableCollider(enable);
        m_noButton.EnableCollider(enable);
    }
}
