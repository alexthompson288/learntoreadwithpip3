using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class StoryMenuCoordinator : MonoBehaviour 
{
    [SerializeField]
    private PipButton m_readButton;
    [SerializeField]
    private PipButton m_picturesButton;
    [SerializeField]
    private PipButton m_captionsButton;
    [SerializeField]
    private PipButton m_quizButton;
    [SerializeField]
    private PipButton m_suggestionsButton;
    [SerializeField]
    private UILabel m_titleLabel;
    [SerializeField]
    private UILabel m_authorLabel;
    [SerializeField]
    private UILabel m_descriptionLabel;
    [SerializeField]
    private Transform[] m_buttonParents;
    [SerializeField]
    private float m_buttonDistance = 240;

    //////////////////////////////////////////////////////////////////////
    // TODO: Put this in a DataSaver called StoryInfo
    static string m_startColor = "pink";

    public static void SetStartColor(string startColor)
    {
        m_startColor = startColor;
    }

    public static string GetStartColor()
    {
        return m_startColor;
    }

    static bool m_isReadingOrPictures;

    static DataRow m_story;
    public static DataRow story
    {
        get
        {
            return m_story;
        }
    }

    public static void SetStory(DataRow newStory)
    {
        m_story = newStory;
    }
    ////////////////////////////////////////////////////////////////////// 


    string m_readButtonString = "Read";

    IEnumerator Start()
    {
        m_readButton.Unpressed += OnClickReadOrPictures;
        m_picturesButton.Unpressed += OnClickReadOrPictures;
        m_captionsButton.Unpressed += OnClickCaptions;
        m_quizButton.Unpressed += OnClickQuiz;
        m_suggestionsButton.Unpressed += OnClickSuggestions;

        m_readButton.SetString(m_readButtonString);

        GameManager.Instance.OnCancel += OnGameCancel;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        if (m_story == null)
        {
            m_story = DataHelpers.GetStories()[0];
        }
   
        if(m_story != null)
        {
            Debug.Log("Title: " + m_story["title"].ToString());

            int numButtons = 4;

            System.Array.Sort(m_buttonParents, CollectionHelpers.ComparePosX);

            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE story_id=" + m_story ["id"]);
            
            if (DataHelpers.OnlyQuizQuestions(dt.Rows).Count == 0)
            {
                m_quizButton.transform.parent.gameObject.SetActive(false);
                --numButtons;
            }

            if (DataHelpers.OnlyCorrectCaptions(dt.Rows).Count == 0)
            {
                m_captionsButton.transform.parent.gameObject.SetActive(false);
                --numButtons;
            }

            // Position the buttons
            float posX = numButtons % 2 == 0 ? ((numButtons - 2) / 2 * m_buttonDistance) + m_buttonDistance / 2 : (numButtons - 1) / 2 * m_buttonDistance;
            posX *= -1;

            foreach(Transform buttonParent in m_buttonParents)
            {
                if(buttonParent.gameObject.activeInHierarchy)
                {
                    buttonParent.transform.localPosition = new Vector3(posX, buttonParent.transform.localPosition.y, buttonParent.transform.localPosition.z);
                    posX += m_buttonDistance;                                                       
                }
            }

            m_authorLabel.text = ("by " + m_story ["author"].ToString());
            m_titleLabel.text = m_story ["title"].ToString();
            Debug.Log("width: " + m_titleLabel.font.CalculatePrintedSize(m_titleLabel.text, false, UIFont.SymbolStyle.None).x);
            
            if (m_titleLabel.font.CalculatePrintedSize(m_titleLabel.text, false, UIFont.SymbolStyle.None).x > 1400)
            {
                m_titleLabel.transform.localScale = Vector3.one * 0.35f;
            } 
            else if (m_titleLabel.font.CalculatePrintedSize(m_titleLabel.text, false, UIFont.SymbolStyle.None).x > 1300)
            {
                m_titleLabel.transform.localScale = Vector3.one * 0.4f;
            } 
            else if (m_titleLabel.font.CalculatePrintedSize(m_titleLabel.text, false, UIFont.SymbolStyle.None).x > 1000)
            {
                m_titleLabel.transform.localScale = Vector3.one * 0.5f;
            } 
            else if (m_titleLabel.font.CalculatePrintedSize(m_titleLabel.text, false, UIFont.SymbolStyle.None).x > 500)
            {
                m_titleLabel.transform.localScale = Vector3.one * 0.75f;
            }

            Debug.Log("Title Scale: " + m_titleLabel.transform.localScale);

            if(m_story["description"] != null)
            {
                m_descriptionLabel.text = m_story["description"].ToString();
            }
            else
            {
                m_descriptionLabel.gameObject.SetActive(false);
            }
        }
        else
        {
            m_titleLabel.gameObject.SetActive(false);
            m_authorLabel.gameObject.SetActive(false);
            m_descriptionLabel.gameObject.SetActive(false);
        }
    }

    void OnClickReadOrPictures(PipButton button)
    {
        if(m_story != null)
        {
            StoryCoordinator.SetShowWords(button.GetString() == m_readButtonString);

            GameManager.Instance.AddGame("NewStories");

            m_isReadingOrPictures = true;

            WingroveAudio.WingroveRoot.Instance.PostEvent("NAV_STORY_TIME");

            StartActivity();
        }
    }

    void OnClickCaptions(PipButton click)
    {
        if (m_story != null)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE story_id=" + System.Convert.ToInt32(m_story["id"]));
            List<DataRow> correctCaptions = DataHelpers.OnlyCorrectCaptions(dt.Rows);

            if(correctCaptions.Count > 0)
            {
                m_isReadingOrPictures = false;
                GameManager.Instance.AddGame("NewCorrectCaption");
                GameManager.Instance.AddData("correctcaptions", correctCaptions);

                WingroveAudio.WingroveRoot.Instance.PostEvent("NAV_CAPTIONS");

                StartActivity();
            }
        }
    }

    void OnClickQuiz(PipButton click)
    {
        if (m_story != null)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE story_id=" + System.Convert.ToInt32(m_story["id"]));
            List<DataRow> quizQuestions = DataHelpers.OnlyQuizQuestions(dt.Rows);

            if(quizQuestions.Count > 0)
            {
                m_isReadingOrPictures = false;
                GameManager.Instance.AddGame("NewQuizGame");
                GameManager.Instance.AddData("quizquestions", quizQuestions);

                WingroveAudio.WingroveRoot.Instance.PostEvent("NAV_QUIZ");

                StartActivity();
            }
        }
    }

    void OnClickSuggestions(PipButton click)
    {

    }

    void StartActivity(string audioEvent = "")
    {
        GameManager.Instance.OnComplete += OnGameComplete;
        GameManager.Instance.SetReturnScene(Application.loadedLevelName);

        GameManager.Instance.AddData("stories", m_story);

        GameManager.Instance.SetScoreLevel(m_story ["title"].ToString());

        GameManager.Instance.StartGames();
    }

    void OnGameComplete()
    {
        GameManager.Instance.OnComplete -= OnGameComplete;
    }

    void OnGameCancel()
    {
        m_isReadingOrPictures = false;
        GameManager.Instance.OnComplete -= OnGameComplete;
    }
}
