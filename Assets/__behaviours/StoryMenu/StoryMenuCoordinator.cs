using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StoryMenuCoordinator : MonoBehaviour 
{
    [SerializeField]
    private ClickEvent m_readButton;
    [SerializeField]
    private ClickEvent m_picturesButton;
    [SerializeField]
    private ClickEvent m_captionsButton;
    [SerializeField]
    private ClickEvent m_quizButton;
    [SerializeField]
    private ClickEvent m_suggestionsButton;
    [SerializeField]
    private UILabel m_titleLabel;
    [SerializeField]
    private UILabel m_authorLabel;
    [SerializeField]
    private UILabel m_descriptionLabel;
    [SerializeField]
    private GameObject m_quizParent;

    static bool m_isReadingOrPictures;

    static DataRow m_story;
    public static void SetStory(DataRow story)
    {
        m_story = story;
    }

    IEnumerator Start()
    {
        m_readButton.OnSingleClick += OnClickReadOrPictures;
        m_captionsButton.OnSingleClick += OnClickCaptions;
        m_quizButton.OnSingleClick += OnClickQuiz;
        m_picturesButton.OnSingleClick += OnClickReadOrPictures;
        m_suggestionsButton.OnSingleClick += OnClickSuggestions;

        GameManager.Instance.OnCancel += OnGameCancel;


        yield return StartCoroutine(GameDataBridge.WaitForDatabase());


        //DataRow story = DataHelpers.GetStory();

        if (m_story == null)
        {
            m_story = DataHelpers.GetStories()[0];
        }

        bool hasFoundQuizQuestions = false;

        if(m_story != null)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from quizquestions WHERE story_id=" + m_story ["id"]);
            
            if (dt.Rows.Count > 0)
            {
                hasFoundQuizQuestions = true;
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

        //m_quizParent.SetActive(hasFoundQuizQuestions);
    }

    void OnClickReadOrPictures(ClickEvent click)
    {
        m_isReadingOrPictures = true;
        StoryReaderLogic.SetShowWords(click.GetString() == "Read");

        GameManager.Instance.AddGames(click.GetString(), "NewStories");
        StartActivity();
    }

    void OnClickCaptions(ClickEvent click)
    {
        DataRow story = DataHelpers.GetStory();
        
        if (story != null)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE story_id=" + System.Convert.ToInt32(story["id"]));
            List<DataRow> correctCaptions = dt.Rows.FindAll(x => x["correctsentence"] != null && x["correctsentence"].ToString() == "t");

            if(correctCaptions.Count > 0)
            {
                m_isReadingOrPictures = false;
                GameManager.Instance.AddGames("NewCorrectCaptions", "NewCorrectCaptions");
                GameManager.Instance.AddData("correctcaptions", correctCaptions);
                StartActivity();
            }
        }
    }

    void OnClickQuiz(ClickEvent click)
    {
        DataRow story = DataHelpers.GetStory();

        if (story != null)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE story_id=" + System.Convert.ToInt32(story["id"]));
            List<DataRow> quizQuestions = dt.Rows.FindAll(x => x["quiz"] != null && x["quiz"].ToString() == "t");

            if(quizQuestions.Count > 0)
            {
                m_isReadingOrPictures = false;
                GameManager.Instance.AddGames("NewQuiz", "NewQuiz");
                GameManager.Instance.AddData("quizquestions", quizQuestions);
                StartActivity();
            }
        }
    }

    void OnClickSuggestions(ClickEvent click)
    {

    }

    void StartActivity()
    {
        GameManager.Instance.OnComplete += OnGameComplete;
        GameManager.Instance.SetReturnScene(Application.loadedLevelName);
        GameManager.Instance.AddData("stories", m_story);
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
