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
    private UILabel m_overviewLabel;
    [SerializeField]
    private VideoPlayer m_videoPlayer;
    [SerializeField]
    private GameObject m_watchButton;
    [SerializeField]
    private PipButton m_quizButton;
    [SerializeField]
    private PipButton m_downloadButton;
    [SerializeField]
    private PipButton m_deleteButton;
    
    DataRow m_currentPipisode;

    List<DataRow> m_pipisodes;
    
    // Use this for initialization
    IEnumerator Start () 
    {
        m_quizButton.Unpressing += OnClickQuizButton;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from pipisodes WHERE publishable='t' ORDER BY order_number");
        m_pipisodes = dt.Rows;

        if (m_pipisodes.Count > 0)
        {
            SelectPipisode(m_pipisodes[0]);

            for (int i = 0; i < m_pipisodes.Count; ++i)
            {
                GameObject button = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pipisodeButtonPrefab, m_grid.transform);

                //button.GetComponentInChildren<UILabel>().text = m_pipisodes[i]["label_text"] != null ? m_pipisodes[i]["label_text"].ToString() : "";

                button.GetComponentInChildren<UITexture>().mainTexture = DataHelpers.GetPicture("pipisodes", m_pipisodes[i]);
                
                button.GetComponent<ClickEvent>().OnSingleClick += OnChoosePipisode;
                button.GetComponent<ClickEvent>().SetData(m_pipisodes[i]);
                
                button.GetComponent<UIDragPanelContents>().draggablePanel = m_draggablePanel;
            }
            
            m_grid.Reposition();
        }
    }

    void OnClickQuizButton(PipButton button)
    {
        Debug.Log("OnClickQuizButton");
        List<DataRow> quizQuestions = FindQuizQuestions(m_currentPipisode);

        if (quizQuestions.Count > 0)
        {
            GameManager.Instance.AddGame("NewQuizGame");
            GameManager.Instance.AddData("quizquestions", quizQuestions);
            GameManager.Instance.SetReturnScene(Application.loadedLevelName);
            GameManager.Instance.StartGames();
        }
    }
    
    void OnChoosePipisode(ClickEvent click)
    {
        SelectPipisode(click.GetData());
    }

    void SelectPipisode(DataRow pipisode)
    {
        float tweenDuration = 0.25f;

        m_currentPipisode = pipisode;

        // Labels
        if (m_currentPipisode["pipisode_title"] != null)
        {
            Debug.Log("Chose Pipisode: " + m_currentPipisode["pipisode_title"].ToString());
            
            m_titleLabel.text = pipisode["pipisode_title"].ToString();
        }

        if (m_currentPipisode["pipisode_overview"] != null)
        {
            m_overviewLabel.text = m_currentPipisode["pipisode_overview"].ToString();    
        }


        // Image
        m_pipisodeImage.mainTexture = DataHelpers.GetPicture("pipisodes", m_currentPipisode);

        Vector3 imageScale = m_pipisodeImage.mainTexture != null ? Vector3.one : Vector3.zero;
        iTween.ScaleTo(m_pipisodeImage.gameObject, imageScale, tweenDuration);


        // Watch and Quiz Buttons
        bool hasQuizQuestions = FindQuizQuestions(m_currentPipisode).Count > 0;

        Vector3 quizButtonScale = hasQuizQuestions ? Vector3.one : Vector3.zero;
        iTween.ScaleTo(m_quizButton.transform.parent.gameObject, quizButtonScale, tweenDuration);

        Vector3 watchButtonScale = hasQuizQuestions ? Vector3.one : Vector3.one * 1.5f;
        iTween.ScaleTo(m_watchButton, watchButtonScale, tweenDuration);

        Vector3 watchButtonPos = hasQuizQuestions ? new Vector3(0, 70) : Vector3.zero;
        Hashtable watchTweenArgs = new Hashtable();
        watchTweenArgs.Add("position", watchButtonPos);
        watchTweenArgs.Add("time", tweenDuration);
        watchTweenArgs.Add("isLocal", true);
        iTween.MoveTo(m_watchButton, watchTweenArgs);


        // Delete and Download Buttons
        m_videoPlayer.SetFilename(m_currentPipisode["image_filename"].ToString() + ".mp4");

        bool hasLocalVideo = m_videoPlayer.HasLocalCopy();

        m_deleteButton.EnableCollider(hasLocalVideo);
        Vector3 deleteButtonScale = hasLocalVideo ? Vector3.one : Vector3.zero;
        iTween.ScaleTo(m_deleteButton.gameObject, deleteButtonScale, tweenDuration);

        m_downloadButton.EnableCollider(!hasLocalVideo);
        Vector3 downloadButtonScale = hasLocalVideo ? Vector3.zero : Vector3.one;
        iTween.ScaleTo(m_downloadButton.gameObject, downloadButtonScale, tweenDuration);
    }

    List<DataRow> FindQuizQuestions(DataRow pipisode)
    {
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE pipisode_id=" + Convert.ToInt32(pipisode["id"]));
        
        List<DataRow> quizQuestions = dt.Rows.FindAll(x => x ["quiz"] != null && x ["quiz"].ToString() == "t");

        return quizQuestions;
    }
}