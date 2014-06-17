using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class StoryMenuCoordinator : MonoBehaviour 
{
    [SerializeField]
    private PipButton[] m_colorButtons;
    [SerializeField]
    private GameObject m_bookPrefab;
    [SerializeField]
    private UIGrid m_bookGrid;
    [SerializeField]
    private UIDraggablePanel m_draggablePanel;
    [SerializeField]
    private GameObject m_infoParent;
    [SerializeField]
    private PipButton m_readButton;
    [SerializeField]
    private GameObject m_readButtonParent;
    [SerializeField]
    private PipButton m_picturesButton;
    [SerializeField]
    private GameObject m_picturesButtonParent;
    [SerializeField]
    private PipButton m_quizButton;
    [SerializeField]
    private GameObject m_quizButtonParent;
    [SerializeField]
    private PipButton m_suggestionsButton;
    [SerializeField]
    private GameObject m_suggestionsButtonParent;
    [SerializeField]
    private UILabel m_titleLabel;
    [SerializeField]
    private UILabel m_descriptionLabel;
    [SerializeField]
    private UITexture m_storyPicture;
    [SerializeField]
    private PipColorWidgets[] m_colorSprites;
    [SerializeField]
    private UIGrid m_colorSpriteGrid;
    [SerializeField]
    private float m_tweenDuration = 0.3f;
    [SerializeField]
    private string m_quizGameName = "NewQuizGame";
    [SerializeField]
    private UISprite[] m_starSprites;
    [SerializeField]
    private AnimManager m_pipAnimManager;
    [SerializeField]
    private UIPanel m_storyPanel;

    PipButton m_currentColorButton = null;
    StoryMenuBook m_currentBookButton = null;

    List<StoryMenuBook> m_spawnedBooks = new List<StoryMenuBook>();

    string m_readingString = "read";

    bool m_hasActivatedStoryPanel = false;

    IEnumerator Start ()
    {
        m_readButton.SetString(m_readingString);
        m_readButton.Unpressing += OnPressReadOrPictures;
        m_picturesButton.Unpressing += OnPressReadOrPictures;
        m_quizButton.Unpressing += OnPressQuiz;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        if (StoryMenuInfo.Instance.HasBookmark())
        {
            ColorInfo.PipColor pipColor = StoryMenuInfo.Instance.GetBookmarkPipColor();
            
            m_currentColorButton = System.Array.Find(m_colorButtons, x => x.pipColor == pipColor);
            
            m_currentColorButton.ChangeSprite(true);

            StartCoroutine(ActivateStoryPanel(pipColor, true));
        }

        foreach (PipButton button in m_colorButtons)
        {
            button.Pressing += OnPressColorButton;
        }

        StoryMenuInfo.Instance.DestroyBookmark();
    }

    IEnumerator ActivateStoryPanel(ColorInfo.PipColor pipColor, bool instant)
    {
        m_hasActivatedStoryPanel = true;

        yield return StartCoroutine(SpawnBooks(pipColor));

        if (instant)
        {
            m_storyPanel.alpha = 1;
            m_draggablePanel.GetComponent<UIPanel>().alpha = 1;
        }
        else
        {
            Debug.Log("ALPHA TWEEN");
            float tweenDuration = 0.3f;
            TweenAlpha.Begin(m_storyPanel.gameObject, tweenDuration, 1);
            TweenAlpha.Begin(m_draggablePanel.gameObject, tweenDuration, 1);
        }
    }

    void OnPressReadOrPictures(PipButton button)
    {
        if (m_currentBookButton != null)
        {
            ColorInfo.PipColor startPipColor = m_currentColorButton != null ? m_currentColorButton.pipColor : ColorInfo.PipColor.Pink;
            StoryMenuInfo.Instance.SetStartPipColor(startPipColor);
            StoryMenuInfo.Instance.SetShowText(button.GetString() == m_readingString);

            GameManager.Instance.AddGame("NewStories");

            WingroveAudio.WingroveRoot.Instance.PostEvent("NAV_STORY_TIME");
            
            StartCoroutine(StartActivity());
        }
    }

    void OnPressQuiz(PipButton button)
    {
        if (m_currentBookButton != null)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE story_id=" + System.Convert.ToInt32(m_currentBookButton.GetData()["id"]));
            List<DataRow> quizQuestions = DataHelpers.OnlyQuizQuestions(dt.Rows);
            
            if(quizQuestions.Count > 0)
            {
                GameManager.Instance.AddGame(m_quizGameName);
                GameManager.Instance.AddData("quizquestions", quizQuestions);
                
                WingroveAudio.WingroveRoot.Instance.PostEvent("NAV_QUIZ");

                StartCoroutine(StartActivity());
            }
        }
    }

    string CreateScoreType()
    {
        return m_currentBookButton.GetData() ["title"].ToString() + "_" + ColorInfo.GetColorString(m_currentColorButton.pipColor);
    }

    IEnumerator StartActivity()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_YAY");
        m_pipAnimManager.PlayAnimation("JUMP");

        yield return new WaitForSeconds(1.2f);

        StoryMenuInfo.Instance.SubscribeGameComplete();

        GameManager.Instance.SetScoreType(CreateScoreType());

        GameManager.Instance.SetReturnScene(Application.loadedLevelName);

        DataRow story = m_currentBookButton.GetData();

        GameManager.Instance.AddData("stories", story);

        StoryMenuInfo.Instance.CreateBookmark(System.Convert.ToInt32(story["id"]), m_currentColorButton.pipColor);

        GameManager.Instance.StartGames();
    }

    void OnPressColorButton(PipButton button)
    {
        if (m_currentColorButton != null)
        {
            m_currentColorButton.ChangeSprite(false);
        }
        
        m_currentColorButton = button;

        if (m_hasActivatedStoryPanel)
        {
            StartCoroutine(SpawnBooks(m_currentColorButton.pipColor));
        }
        else
        {
            StartCoroutine(ActivateStoryPanel(m_currentColorButton.pipColor, false));
        }
    }

    void OnPressStoryButton(StoryMenuBook button)
    {
        m_currentBookButton = button;

        Vector3 infoParentScale = m_currentBookButton != null ? Vector3.one : Vector3.zero;
        iTween.ScaleTo(m_infoParent, infoParentScale, m_tweenDuration);

        if (m_currentBookButton != null)
        {
            m_pipAnimManager.PlayAnimation("THUMBS_UP");

            DataRow story = m_currentBookButton.GetData();

            float maxLabelWidth = 600;
            //NGUIHelpers.SetLabel(m_titleLabel, story, "title", maxLabelWidth);
            //NGUIHelpers.SetLabel(m_descriptionLabel, story, "description", maxLabelWidth);

            NGUIHelpers.SetLabel(m_titleLabel, story, "title");
            NGUIHelpers.SetLabel(m_descriptionLabel, story, "description");

            /*
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from storypages where story_id=" + System.Convert.ToInt32(story ["id"]));
            if (dt.Rows.Count > 0)
            {
                m_storyPicture.mainTexture = DataHelpers.GetPicture("storypages", dt.Rows[0]);
            }
            */

            m_storyPicture.mainTexture = DataHelpers.GetPicture("stories", story);

            Vector3 storyPictureScale = m_storyPicture.mainTexture != null ? Vector3.one : Vector3.zero;
            iTween.ScaleTo(m_storyPicture.gameObject, storyPictureScale, m_tweenDuration);


            foreach (PipColorWidgets sprite in m_colorSprites)
            {
                string colorName = ColorInfo.GetColorString(sprite.color).ToLower();

                sprite.gameObject.SetActive(story [colorName] != null && story [colorName].ToString() == "t");
            }

            m_colorSpriteGrid.Reposition();


            if(m_suggestionsButtonParent != null)
            {
                m_suggestionsButtonParent.SetActive(story ["suggestions"] != null);
            }


            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE story_id=" + System.Convert.ToInt32(story ["id"]));

            bool hasQuizQuestions = DataHelpers.OnlyQuizQuestions(dt.Rows).Count > 0;

            Vector3 quizButtonScale = hasQuizQuestions ? Vector3.one : Vector3.zero;
            iTween.ScaleTo(m_quizButtonParent, quizButtonScale, m_tweenDuration);

            Hashtable tweenArgs = new Hashtable();

            tweenArgs.Add("islocal", true);
            tweenArgs.Add("time", m_tweenDuration);

            float readButtonPosY = hasQuizQuestions ? 120 : 70;
            tweenArgs.Add("position", new Vector3(m_readButtonParent.transform.localPosition.x, readButtonPosY));

            iTween.MoveTo(m_readButtonParent, tweenArgs);


            float picturesButtonPosY = hasQuizQuestions ? 0 : -70;
            tweenArgs["position"] = new Vector3(m_picturesButtonParent.transform.localPosition.x, picturesButtonPosY);

            iTween.MoveTo(m_picturesButtonParent, tweenArgs);


            Vector3 activeButtonScale = hasQuizQuestions ? Vector3.one : Vector3.one * 1.2f;
            iTween.ScaleTo(m_readButtonParent, activeButtonScale, m_tweenDuration);
            iTween.ScaleTo(m_picturesButtonParent, activeButtonScale, m_tweenDuration);

            ScoreInfo.RefreshScoreStars(m_starSprites, m_quizGameName, CreateScoreType());
        }
    }

    IEnumerator SpawnBooks(ColorInfo.PipColor pipColor)
    {
        if (m_spawnedBooks.Count > 0)
        {
            CollectionHelpers.DestroyObjects(m_spawnedBooks, true);

            yield return new WaitForSeconds(0.3f);
        }

        string colorName = ColorInfo.GetColorString(pipColor).ToLower();
        
        DataTable dataTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE " + colorName + "='t' ORDER BY fontsize, difficulty");
        
        int bookIndex = 0;
        
        foreach (DataRow story in dataTable.Rows)
        {
            if (story["publishable"] != null && story["publishable"].ToString() == "t")
            {
                GameObject bookInstance = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_bookPrefab, m_bookGrid.transform);
                bookInstance.name = bookIndex.ToString() + "_Book";

                StoryMenuBook bookBehaviour = bookInstance.GetComponent<StoryMenuBook>() as StoryMenuBook;
                bookBehaviour.SetUp(story, m_draggablePanel);
                bookBehaviour.Clicked += OnPressStoryButton;
                m_spawnedBooks.Add(bookBehaviour);
                ++bookIndex;
            }
        }

        m_bookGrid.Reposition();

        if (StoryMenuInfo.Instance.HasBookmark())
        {
            int storyId = StoryMenuInfo.Instance.GetBookmarkStoryId();
            Debug.Log("Finding story with id: " + storyId);
            m_currentBookButton = m_spawnedBooks.Find(x => System.Convert.ToInt32(x.GetData()["id"]) == storyId);
            StoryMenuInfo.Instance.DestroyBookmark();
        } 
        else
        {
            m_currentBookButton = m_spawnedBooks.Count > 0 ? m_spawnedBooks[0] : null;
        }

        OnPressStoryButton(m_currentBookButton);
    }
}
