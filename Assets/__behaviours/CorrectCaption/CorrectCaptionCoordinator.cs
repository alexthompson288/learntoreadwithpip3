using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wingrove;
using System;

public class CorrectCaptionCoordinator : GameCoordinator
{
    [SerializeField]
    private string m_dataType;
    [SerializeField]
    private UITexture m_questionImage;
    [SerializeField]
    private Transform m_questionImageScale;
    [SerializeField]
    private GameObject m_questionImageParent;
    [SerializeField]
    private GameObject m_textPrefab;
    [SerializeField]
    private Transform m_textPosition;
    [SerializeField]
    private UILabel m_questionLabel;
    [SerializeField]
    private UISprite m_questionBackground;
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
    private AudioClip m_wordsInstructions;
    [SerializeField]
    private AudioClip m_captionsInstructions;

    bool m_hasAnsweredIncorrectly = false;

    List<string> m_captionTextAttributes = new List<string>();
    List<string> m_remainingAttributes = new List<string>();

    List<GameObject> m_spawnedQuestionText = new List<GameObject>();

    string m_goodAttribute;

    IEnumerator Start()
    {
        m_noButton.SetInt(0);
        m_yesButton.SetInt(1);
        m_noButton.SingleClicked += OnAnswer;
        m_yesButton.SingleClicked += OnAnswer;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataType = DataHelpers.GameOrDefault(m_dataType);



        //Debug.Log("CorrectCaptionCoordinator.dataType: " + m_dataType);

        if (m_dataType == "words")
        {
            m_dataPool = DataHelpers.GetWords();

            m_goodAttribute = "word";

            m_captionTextAttributes.Add("dummyword1");
            m_captionTextAttributes.Add("dummyword2");

            //m_questionImage.height = 512;
            m_textPosition.transform.localScale = Vector3.one * 2;
        } 
        else
        {
            //Debug.Log("Getting correct captions");
            m_dataPool = DataHelpers.GetCorrectCaptions();
            //Debug.Log("Found " + m_dataPool.Count);
            
            m_goodAttribute = "good_sentence";
            
            m_captionTextAttributes.Add("bad_sentence1");
            m_captionTextAttributes.Add("bad_sentence2");
            m_captionTextAttributes.Add("bad_sentence4");

            //m_questionImage.height = 384;
            m_questionImage.width = 683;
        }

        m_captionTextAttributes.Add(m_goodAttribute);

#if UNITY_EDITOR
        if(m_dataPool.Count == 0 && m_dataType == "correctcaptions")
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from correctcaptions WHERE programsession_id=" + 304);
            m_dataPool = dt.Rows;
        }
#endif

        DataHelpers.OnlyPictureData(m_dataPool);

        //Debug.Log("m_dataPool.Count: " + m_dataPool.Count);

        ClampTargetScore();

        m_scoreKeeper.SetTargetScore(m_targetScore);

        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());

        m_audioSource.clip = m_dataType == "words" ? m_wordsInstructions : m_captionsInstructions;
        m_audioSource.Play();

        if (m_dataPool.Count > 0)
        {
            SetStartTime();
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

            //Debug.Log("m_currentData: " + m_currentData);

            m_hasAnsweredIncorrectly = false;

            m_remainingAttributes.Clear();
            m_remainingAttributes.AddRange(m_captionTextAttributes);

            // Remove any attributes which current data is missing
            for (int i = m_remainingAttributes.Count - 1; i > -1; --i)
            {
                //Debug.Log(System.String.Format("{0} - {1}", m_currentData[m_goodAttribute], m_remainingAttributes[i]));

                if(m_currentData[m_remainingAttributes[i]] == null)
                {
                    m_remainingAttributes.RemoveAt(i);
                }
            }

            CollectionHelpers.Shuffle(m_remainingAttributes);

            Texture2D tex = DataHelpers.GetPicture(m_currentData);

            if (m_currentData[m_goodAttribute] != null && tex != null) // if "good_sentence" is null then the player is unable to answer correctly, if tex is null the question is unfair
            {
                m_questionImage.mainTexture = tex;
                //m_questionImage.MakePixelPerfect();

                SpawnQuestionText();

                TweenQuestionParents(Vector3.one);

                yield return new WaitForSeconds(m_questionTweenDuration);

                EnableAnswerColliders(true);
            }
            else
            {
                //Debug.LogError("BAD QUESTION");
                //Debug.Log("goodAttribute: " + (m_currentData[m_goodAttribute] != null));
                //Debug.Log("tex: " + tex);
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


                UILabel label = newText.GetComponent<UILabel>() as UILabel;

                label.text = word + " ";

                Vector3 wordSize = label.font.CalculatePrintedSize(word + " ", false, UIFont.SymbolStyle.None);

                newText.transform.localPosition = new Vector3(length, 0, 0);

                length += wordSize.x;


                ShowPipPadForWord showPipPadForWord = newText.GetComponent<ShowPipPadForWord>() as ShowPipPadForWord;
                
                showPipPadForWord.SetUp(word, wordSize, true);
            }
        }

        m_textPosition.localPosition = new Vector3(-length / 2 * m_textPosition.transform.localScale.x, m_textPosition.localPosition.y, m_textPosition.localPosition.z);
        //m_questionBackground.width = (int)length + 50;
    }
    
    protected override IEnumerator CompleteGame()
    {
        float timeTaken = Time.time - m_startTime;

        ScoreInfo.Instance.NewScore(timeTaken, m_score, m_targetScore, ScoreInfo.CalculateScoreStars(m_score, m_targetScore));

        //Debug.Log("GameCoordinator.CompleteGame()");
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
        bool yesIsCorrect = (m_remainingAttributes.Last() == m_goodAttribute); // The current attribute is the last one in m_remainingAttributes
        bool isCorrect = (yesIsCorrect == answerIsYes);

        if (isCorrect)
        {
            if(answerIsYes)
            {
                AudioClip clip = DataHelpers.GetShortAudio(m_currentData);

                if(clip != null)
                {
                    m_audioSource.clip = clip;
                    m_audioSource.Play();
                }

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
                //WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");

                m_remainingAttributes.RemoveAt(m_remainingAttributes.Count - 1);

                SpawnQuestionText();
                /*
                iTween.ScaleTo(m_questionLabelParent, Vector3.zero, m_questionTweenDuration);
                WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEAR");

                yield return new WaitForSeconds(m_questionTweenDuration);

                SpawnQuestionText();

                iTween.ScaleTo(m_questionLabelParent, Vector3.one, m_questionTweenDuration);
                WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");
                */
            }
        } 
        else
        {
            m_hasAnsweredIncorrectly = true;
            //string eventName = answerIsYes ? "VOCAL_INCORRECT" : "TROLL_EXHALE";
            //WingroveAudio.WingroveRoot.Instance.PostEvent(eventName);
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
        }

        EnableAnswerColliders(true);
    }

    void TweenQuestionParents(Vector3 newScale)
    {
        iTween.ScaleTo(m_questionImageParent, newScale, m_questionTweenDuration);
        //iTween.ScaleTo(m_questionLabelParent, newScale, m_questionTweenDuration);

        string audioString = Mathf.Approximately(newScale.x, 0) ? "SOMETHING_APPEAR" : "SOMETHING_DISAPPEAR";
        WingroveAudio.WingroveRoot.Instance.PostEvent(audioString);
    }

    void EnableAnswerColliders(bool enable)
    {
        m_yesButton.EnableCollider(enable);
        m_noButton.EnableCollider(enable);
    }
}
