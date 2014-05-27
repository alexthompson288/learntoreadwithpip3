using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System;

public class PipisodeMenuCoordinator : MonoBehaviour 
{
    [SerializeField]
    private UIGrid m_doubleGrid;
    [SerializeField]
    private UIGrid m_singleGrid;
    [SerializeField]
    private UIDraggablePanel m_draggablePanel;
    [SerializeField]
    private GameObject m_pipisodeButtonPrefab;
    [SerializeField]
    private UITexture m_backgroundTexture;
    [SerializeField]
    private string m_relativePathMp4 = "Pipisodes/mp4";
    [SerializeField]
    private UILabel m_titleLabel;
    [SerializeField]
    private UILabel m_overviewLabel;
    [SerializeField]
    private VideoPlayer m_videoPlayer;
    [SerializeField]
    private GameObject m_videoButtonsParent;
    [SerializeField]
    private PipButton m_quizButton;
    
    DataRow m_currentPipisode;

    List<DataRow> m_pipisodes;
    
    // Use this for initialization
    IEnumerator Start () 
    {
        m_quizButton.Unpressing += OnClickQuizButton;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from pipisodes WHERE publishable='t' ORDER BY order_number");
        m_pipisodes = dt.Rows;

        UIGrid spawnGrid = m_pipisodes.Count > 5 ? m_doubleGrid : m_singleGrid;

        if (m_pipisodes.Count > 0)
        {
            SelectPipisode(m_pipisodes[0]);

            for (int i = 0; i < m_pipisodes.Count; ++i)
            {
                GameObject button = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pipisodeButtonPrefab, spawnGrid.transform);

                button.GetComponentInChildren<UILabel>().text = m_pipisodes[i]["label_text"] != null ? m_pipisodes[i]["label_text"].ToString() : "";
                
                button.GetComponent<ClickEvent>().OnSingleClick += OnChoosePipisode;
                button.GetComponent<ClickEvent>().SetData(m_pipisodes[i]);
                
                button.GetComponent<UIDragPanelContents>().draggablePanel = m_draggablePanel;
            }
            
            spawnGrid.Reposition();
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

    void OnClickBuyButton(ClickEvent click)
    {
        Debug.Log("OnClickBuyButton()");
    }
    
    void OnChoosePipisode(ClickEvent click)
    {
        SelectPipisode(click.GetData());
    }

    void SelectPipisode(DataRow pipisode)
    {
        if (pipisode["pipisode_title"] != null)
        {
            Debug.Log("Chose Pipisode: " + pipisode["pipisode_title"].ToString());
            
            m_titleLabel.text = pipisode["pipisode_title"].ToString();
            
            Texture2D tex = Resources.Load<Texture2D>(String.Format("Backgrounds/pipisode_{0}", pipisode["pipisode_title"]));
            
            if(tex != null)
            {
                m_backgroundTexture.mainTexture = tex;
            }
        }
        
        if (pipisode["pipisode_overview"] != null)
        {
            m_overviewLabel.text = pipisode["pipisode_overview"].ToString();    
        }

        float tweenDuration = 0.3f;

        bool hasQuizQuestions = FindQuizQuestions(pipisode).Count > 0;

        Debug.Log("hasQuizQuestions - " + pipisode ["pipisode_title"] + ": " + hasQuizQuestions);

        Vector3 quizQuestionsScale = hasQuizQuestions ? Vector3.one : Vector3.zero;
        iTween.ScaleTo(m_quizButton.transform.parent.gameObject, quizQuestionsScale, tweenDuration);

        Vector3 videoButtonsLocalPos = hasQuizQuestions ? Vector3.zero : new Vector3(100, 0);

        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("time", tweenDuration);
        tweenArgs.Add("islocal", true);
        tweenArgs.Add("position", videoButtonsLocalPos); 

        iTween.MoveTo(m_videoButtonsParent, tweenArgs);

        m_videoPlayer.SetFilename(pipisode["image_filename"].ToString() + ".mp4");
        
        m_currentPipisode = pipisode;
        
        // TODO: Change background texture
    }

    List<DataRow> FindQuizQuestions(DataRow pipisode)
    {
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE pipisode_id=" + Convert.ToInt32(pipisode["id"]));
        
        List<DataRow> quizQuestions = dt.Rows.FindAll(x => x ["quiz"] != null && x ["quiz"].ToString() == "t");

        return quizQuestions;
    }
}