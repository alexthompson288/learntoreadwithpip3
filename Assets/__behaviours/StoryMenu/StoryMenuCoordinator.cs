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
    private ClickEvent m_quizButton;
    [SerializeField]
    private ClickEvent m_suggestionsButton;
    [SerializeField]
    private UILabel m_titleLabel;
    [SerializeField]
    private UILabel m_authorLabel;
    [SerializeField]
    private UILabel m_descriptionLabel;

    static bool m_isReadingOrPictures;

    IEnumerator Start()
    {
        m_readButton.OnSingleClick += OnClickReadOrPictures;
        m_quizButton.OnSingleClick += OnClickQuiz;
        m_picturesButton.OnSingleClick += OnClickReadOrPictures;
        m_suggestionsButton.OnSingleClick += OnClickSuggestions;

        GameManager.Instance.OnCancel += OnGameCancel;


        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        
        DataRow story = DataHelpers.GetStory();

        bool hasFoundQuizQuestions = false;

        if(story != null)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from quizquestions WHERE story_id=" + story ["id"]);
            
            if (dt.Rows.Count > 0)
            {
                hasFoundQuizQuestions = true;
            }


            m_authorLabel.text = ("by " + story ["author"].ToString());
            m_titleLabel.text = story ["title"].ToString();
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

            if(story["description"] != null)
            {
                m_descriptionLabel.text = story["description"].ToString();
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

        m_quizButton.gameObject.SetActive(hasFoundQuizQuestions);
    }

    void OnClickReadOrPictures(ClickEvent click)
    {
        m_isReadingOrPictures = true;
        StoryReaderLogic.SetShowWords(click.GetString() == "Read");

        GameManager.Instance.AddGames(click.GetString(), "NewStories");
        StartActivity();
    }

    void OnClickQuiz(ClickEvent click)
    {
        m_isReadingOrPictures = false;
        GameManager.Instance.AddGames("NewQuiz", "NewQuiz");

        DataRow story = DataHelpers.GetStory();

        if (story != null)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from quizquestions WHERE story_id=" + story["id"]);

            if(dt.Rows.Count > 0)
            {
                GameManager.Instance.AddData("quizquestions", dt.Rows);
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
