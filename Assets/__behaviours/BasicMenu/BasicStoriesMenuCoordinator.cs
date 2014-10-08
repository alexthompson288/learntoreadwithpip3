using UnityEngine;
using System.Collections;
using Wingrove;
using System.Collections.Generic;

public class BasicStoriesMenuCoordinator : Singleton<BasicStoriesMenuCoordinator> 
{
    [SerializeField]
    private GameObject m_storyPrefab;
    [SerializeField]
    private UIGrid m_storyGrid;
    [SerializeField]
    private UILabel m_titleLabel;
    [SerializeField]
    private UILabel m_descriptionLabel;
    [SerializeField]
    private UITexture m_storyTexture;
    [SerializeField]
    private EventRelay m_readButton;
    [SerializeField]
    private EventRelay m_quizButton;
    [SerializeField]
    private UIScrollView m_scrollView;

    List<GameObject> m_spawnedStories = new List<GameObject>();

    ColorInfo.PipColor m_pipColor;
    DataRow m_currentStory;

    void Awake()
    {
        m_readButton.SingleClicked += OnClickRead;
    }

    public void On(ColorInfo.PipColor myPipColor, int storyId = 0)
    {
        m_pipColor = myPipColor;

        StartCoroutine(OnCo(storyId));
    }

    IEnumerator OnCo(int storyId)
    {
        CollectionHelpers.DestroyObjects(m_spawnedStories);

        yield return null;

        m_scrollView.ResetPosition();
        
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE " + m_pipColor + "='t' ORDER BY fontsize, difficulty");
        
        foreach (DataRow story in dt.Rows)
        {
            if (story ["publishable"] != null && story ["publishable"].ToString() == "t")
            {
                GameObject newStory = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_storyPrefab, m_storyGrid.transform);
                m_spawnedStories.Add(newStory);
                
                newStory.GetComponent<BasicStory>().SetUp(story);
                newStory.GetComponent<EventRelay>().SingleClicked += OnClickStory;
            }
        }

        yield return null;

        m_storyGrid.Reposition();
        m_scrollView.ResetPosition();
        
        if (m_spawnedStories.Count > 0)
        {
            GameObject startingStory = m_spawnedStories.Find(x => x.GetComponent<BasicStory>().story.GetId() == storyId);
            if(startingStory == null)
            {
                startingStory = m_spawnedStories[0];
            }

            OnClickStory(startingStory.GetComponent<EventRelay>() as EventRelay);
        }
    }
    
    void OnClickStory(EventRelay relay)
    {
        m_currentStory = relay.GetComponent<BasicStory>().story;

        (BasicMenuNavigation.Instance as BasicMenuNavigation).SetBookmarkStoryId(m_currentStory.GetId());

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE story_id=" + m_currentStory.GetId());
        bool hasQuizQuestions = DataHelpers.OnlyQuizQuestions(dt.Rows).Count > 0;
        if (hasQuizQuestions)
        {
            m_quizButton.SingleClicked += OnClickQuiz;
        }
        else
        {
            m_quizButton.SingleClicked -= OnClickQuiz;
        }

        StopCoroutine("OnClickStoryCo");
        StartCoroutine("OnClickStoryCo");
    }

    IEnumerator OnClickStoryCo()
    {
        float tweenDuration = 0.2f;

        iTween.ScaleTo(m_storyTexture.gameObject, Vector3.zero, tweenDuration);
        iTween.ScaleTo(m_titleLabel.gameObject, Vector3.zero, tweenDuration);
        iTween.ScaleTo(m_descriptionLabel.gameObject, Vector3.zero, tweenDuration);

        yield return new WaitForSeconds(tweenDuration);

        m_storyTexture.mainTexture = DataHelpers.GetPicture(m_currentStory);
        Vector3 storyPictureScale = m_storyTexture.mainTexture != null ? Vector3.one : Vector3.zero;
        iTween.ScaleTo(m_storyTexture.gameObject, storyPictureScale, tweenDuration);

        m_titleLabel.text = m_currentStory ["title"] != null ? m_currentStory ["title"].ToString() : "";
        iTween.ScaleTo(m_titleLabel.gameObject, Vector3.one, tweenDuration);

        m_descriptionLabel.text = m_currentStory ["description"] != null ? m_currentStory ["description"].ToString() : "";
        iTween.ScaleTo(m_descriptionLabel.gameObject, Vector3.one, tweenDuration);

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE story_id=" + System.Convert.ToInt32(m_currentStory ["id"]));
        Vector3 quizButtonScale = DataHelpers.OnlyQuizQuestions(dt.Rows).Count > 0 ? Vector3.one : Vector3.zero;
        iTween.ScaleTo(m_quizButton.gameObject, quizButtonScale, tweenDuration);
    }

    string CreateScoreType()
    {
        return m_currentStory ["title"].ToString() + "_" + ColorInfo.GetColorString(m_pipColor);
    }

    void OnClickRead(EventRelay relay)
    {
        GameManager.Instance.Reset();

        GameManager.Instance.AddData("stories", m_currentStory);
        GameManager.Instance.AddGame("NewStories");
        
        WingroveAudio.WingroveRoot.Instance.PostEvent("NAV_STORY_TIME");

        StartCoroutine(StartActivity());
    }
    
    void OnClickQuiz(EventRelay relay)
    {
        GameManager.Instance.Reset();

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE story_id=" + System.Convert.ToInt32(m_currentStory.GetInt("id")));
        List<DataRow> quizQuestions = DataHelpers.OnlyQuizQuestions(dt.Rows);

        if (quizQuestions.Count > 0)
        {
            GameManager.Instance.AddData("quizquestions", quizQuestions);
            GameManager.Instance.AddGame("NewQuiz");

            WingroveAudio.WingroveRoot.Instance.PostEvent("NAV_QUIZ");

            ScoreInfo.Instance.SetScoreType(CreateScoreType());
            
            StartCoroutine(StartActivity());
        }
    }
    
    IEnumerator StartActivity()
    {
        yield return null;

        GameManager.Instance.SetCurrentColor(m_pipColor);
        GameManager.Instance.SetReturnScene(Application.loadedLevelName);        

        GameManager.Instance.StartGames();
    }
}
