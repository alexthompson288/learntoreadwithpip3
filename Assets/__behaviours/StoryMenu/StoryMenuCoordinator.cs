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
    private Transform[] m_buttonParents;
    [SerializeField]
    private float m_buttonDistance = 240;

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

    IEnumerator Start()
    {
        m_readButton.OnSingleClick += OnClickReadOrPictures;
        m_captionsButton.OnSingleClick += OnClickCaptions;
        m_quizButton.OnSingleClick += OnClickQuiz;
        m_picturesButton.OnSingleClick += OnClickReadOrPictures;
        m_suggestionsButton.OnSingleClick += OnClickSuggestions;

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

    void OnClickReadOrPictures(ClickEvent click)
    {
        if (m_story != null)
        {
            m_isReadingOrPictures = true;
            StoryReaderLogic.SetShowWords(click.GetString() == "Read");

            GameManager.Instance.AddGames(click.GetString(), "NewStories");
            StartActivity();
        }
    }

    void OnClickCaptions(ClickEvent click)
    {
        if (m_story != null)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE story_id=" + System.Convert.ToInt32(m_story["id"]));
            List<DataRow> correctCaptions = DataHelpers.OnlyCorrectCaptions(dt.Rows);

            if(correctCaptions.Count > 0)
            {
                m_isReadingOrPictures = false;
                GameManager.Instance.AddGames("NewCorrectCaption", "NewCorrectCaption");
                GameManager.Instance.AddData("correctcaptions", correctCaptions);
                StartActivity();
            }
        }
    }

    void OnClickQuiz(ClickEvent click)
    {
        if (m_story != null)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE story_id=" + System.Convert.ToInt32(m_story["id"]));
            List<DataRow> quizQuestions = DataHelpers.OnlyQuizQuestions(dt.Rows);

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

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE story_id=" + System.Convert.ToInt32(m_story["id"]));
        GameManager.Instance.AddData("correctcaptions", DataHelpers.OnlyCorrectCaptions(dt.Rows));
        GameManager.Instance.AddData("quizquestions", DataHelpers.OnlyQuizQuestions(dt.Rows));

        Debug.Log("correctcaptions: " + DataHelpers.GetCorrectCaptions().Count);
        Debug.Log("quizquestions: " + DataHelpers.GetQuizQuestions().Count);

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
