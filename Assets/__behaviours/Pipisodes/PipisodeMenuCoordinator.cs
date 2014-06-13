using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System;

public class PipisodeMenuCoordinator : MonoBehaviour 
{
    [SerializeField]
    private UIGrid m_grid;
    [SerializeField]
    private UIDraggablePanel m_draggablePanel;
    [SerializeField]
    private GameObject m_pipisodeButtonPrefab;
    [SerializeField]
    private UITexture m_pipisodeImage;
    [SerializeField]
    private UILabel m_titleLabel;
    [SerializeField]
    private GameObject m_overviewScale;
    [SerializeField]
    private UILabel m_overviewLabel;
    [SerializeField]
    private VideoPlayer m_videoPlayer;
    [SerializeField]
    private GameObject m_watchButton;
    [SerializeField]
    private GameObject m_quizButtonParent;
    [SerializeField]
    private PipButton m_quizButton;
    [SerializeField]
    private PipButton m_downloadButton;
    [SerializeField]
    private PipButton m_deleteButton;
    [SerializeField]
    private AnimManager m_pipAnimManager;
    [SerializeField]
    private float m_tweenDuration = 0.5f;
    [SerializeField]
    private UISprite[] m_starSprites;
    
    DataRow m_currentPipisode;

    List<DataRow> m_pipisodes;
    
    // Use this for initialization
    IEnumerator Start () 
    {
        m_videoPlayer.Downloaded += OnVideoDownloadOrDelete;
        m_videoPlayer.Deleted += OnVideoDownloadOrDelete;

        m_quizButton.Unpressing += OnClickQuizButton;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from pipisodes WHERE publishable='t' ORDER BY order_number");
        m_pipisodes = dt.Rows;

        Debug.Log("Found " + m_pipisodes.Count + " pipisodes");
        foreach (DataRow pipisode in m_pipisodes)
        {
            Debug.Log(pipisode["pipisode_title"].ToString());
        }

        if (m_pipisodes.Count > 0)
        {
            SelectPipisode(m_pipisodes[0]);

            for (int i = 0; i < m_pipisodes.Count; ++i)
            {
                GameObject button = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pipisodeButtonPrefab, m_grid.transform);

                button.GetComponent<PipisodeButton>().SetUp(m_pipisodes[i], m_draggablePanel, m_videoPlayer);
                //button.GetComponentInChildren<UITexture>().mainTexture = DataHelpers.GetPicture("pipisodes", m_pipisodes[i]);
                
                button.GetComponent<ClickEvent>().OnSingleClick += OnChoosePipisode;
                button.GetComponent<ClickEvent>().SetData(m_pipisodes[i]);
                
                //button.GetComponent<UIDragPanelContents>().draggablePanel = m_draggablePanel;
            }
            
            m_grid.Reposition();
        }
    }

    void OnClickQuizButton(PipButton button)
    {
        m_pipAnimManager.PlayAnimation("JUMP");

        List<DataRow> quizQuestions = FindQuizQuestions(m_currentPipisode);

        if (quizQuestions.Count > 0)
        {
            StartCoroutine(OnClickQuizCo(quizQuestions));
        }
    }

    IEnumerator OnClickQuizCo(List<DataRow> quizQuestions)
    {
        yield return new WaitForSeconds(1.5f);
        GameManager.Instance.AddGame("NewQuizGame");
        GameManager.Instance.AddData("quizquestions", quizQuestions);
        GameManager.Instance.SetReturnScene(Application.loadedLevelName);

        GameManager.Instance.SetScoreType(m_currentPipisode ["pipisode_title"].ToString());

        GameManager.Instance.StartGames();
    }
    
    void OnChoosePipisode(ClickEvent click)
    {
        SelectPipisode(click.GetData());
    }

    void SelectPipisode(DataRow pipisode)
    {
        m_currentPipisode = pipisode;

        // Labels
        if (m_currentPipisode["pipisode_title"] != null)
        {
            Debug.Log("Chose Pipisode: " + m_currentPipisode["pipisode_title"].ToString());
            
            m_titleLabel.text = pipisode["pipisode_title"].ToString();

            NGUIHelpers.MaxLabelWidth(m_titleLabel, 866);
        }

        string overviewText = m_currentPipisode ["pipisode_overview"] != null ? m_currentPipisode ["pipisode_overview"].ToString() : "";

        if (!String.IsNullOrEmpty(overviewText))
        {
            m_overviewLabel.text = overviewText;
        }

        Vector3 overviewScale = System.String.IsNullOrEmpty(overviewText) ? Vector3.zero : Vector3.one;
        iTween.ScaleTo(m_overviewScale, overviewScale, m_tweenDuration);


        // Image
        m_pipisodeImage.mainTexture = DataHelpers.GetPicture("pipisodes", m_currentPipisode);

        Vector3 imageScale = m_pipisodeImage.mainTexture != null ? Vector3.one : Vector3.zero;
        iTween.ScaleTo(m_pipisodeImage.gameObject, imageScale, m_tweenDuration);


        // Quiz Stars
        ScoreInfo.RefreshScoreStars(m_starSprites, "NewQuizGame", m_currentPipisode ["pipisode_title"].ToString());


        // Watch and Quiz Buttons
        bool hasQuizQuestions = FindQuizQuestions(m_currentPipisode).Count > 0;

        Vector3 quizButtonScale = hasQuizQuestions ? Vector3.one : Vector3.zero;
        iTween.ScaleTo(m_quizButtonParent, quizButtonScale, m_tweenDuration);

        Vector3 watchButtonScale = hasQuizQuestions ? Vector3.one : Vector3.one * 1.5f;
        iTween.ScaleTo(m_watchButton, watchButtonScale, m_tweenDuration);

        Vector3 watchButtonPos = hasQuizQuestions ? new Vector3(0, 70) : Vector3.zero;
        Hashtable watchTweenArgs = new Hashtable();
        watchTweenArgs.Add("position", watchButtonPos);
        watchTweenArgs.Add("time", m_tweenDuration);
        watchTweenArgs.Add("isLocal", true);
        iTween.MoveTo(m_watchButton, watchTweenArgs);


        // Delete and Download Buttons
        m_videoPlayer.SetFilename(m_videoPlayer.GetPipisodeFilename(m_currentPipisode));
        RefreshDownloadAndDeleteButtons();


        m_pipAnimManager.PlayAnimation("THUMBS_UP");
    }

    List<DataRow> FindQuizQuestions(DataRow pipisode)
    {
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE pipisode_id=" + Convert.ToInt32(pipisode["id"]));
        
        List<DataRow> quizQuestions = dt.Rows.FindAll(x => x ["quiz"] != null && x ["quiz"].ToString() == "t");

        return quizQuestions;
    }

    void RefreshDownloadAndDeleteButtons()
    {
        bool hasLocalVideo = m_videoPlayer.HasLocalCopy();
        
        m_deleteButton.EnableCollider(hasLocalVideo);
        Vector3 deleteButtonScale = hasLocalVideo ? Vector3.one : Vector3.zero;
        iTween.ScaleTo(m_deleteButton.gameObject, deleteButtonScale, m_tweenDuration);
        
        m_downloadButton.EnableCollider(!hasLocalVideo);
        Vector3 downloadButtonScale = hasLocalVideo ? Vector3.zero : Vector3.one;
        iTween.ScaleTo(m_downloadButton.gameObject, downloadButtonScale, m_tweenDuration);
    }

    void OnVideoDownloadOrDelete(VideoPlayer videoPlayer)
    {
        RefreshDownloadAndDeleteButtons();
    }
}