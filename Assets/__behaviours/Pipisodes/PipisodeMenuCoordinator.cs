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
    private ClickEvent m_playButton;
    [SerializeField]
    private UITexture m_backgroundTexture;
    [SerializeField]
    private string m_relativePathMp4 = "Pipisodes/mp4";
    
    DataRow m_currentPipisode;

    List<DataRow> m_pipisodes;
    
    // Use this for initialization
    IEnumerator Start () 
    {
        m_playButton.OnSingleClick += OnClickPlayButton;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from pipisodes");
        m_pipisodes = dt.Rows;

        if (m_pipisodes.Count > 0)
        {
            m_currentPipisode = m_pipisodes[0];
            
            for (int i = 0; i < m_pipisodes.Count; ++i)
            {
                GameObject button = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pipisodeButtonPrefab, m_grid.transform);
                
                button.GetComponentInChildren<UILabel>().text = (i + 1).ToString();

                
                if (!PipisodeInfo.Instance.IsUnlocked(Convert.ToInt32(m_pipisodes[i]["id"])))
                {
                    button.GetComponentInChildren<UISprite>().color = Color.gray;
                }
                
                button.GetComponent<ClickEvent>().OnSingleClick += OnChoosePipisode;
                button.GetComponent<ClickEvent>().SetData(m_pipisodes[i]);
                
                button.GetComponent<UIDragPanelContents>().draggablePanel = m_draggablePanel;
            }
            
            m_grid.Reposition();
        }
    }
    
    void OnClickPlayButton(ClickEvent click)
    {
        Debug.Log("OnClickPlayButton()");
#if UNITY_ANDROID || UNITY_IPHONE
        if(PipisodeInfo.Instance.IsUnlocked(Convert.ToInt32(m_currentPipisode["id"])) && m_currentPipisode["video_filename"] != null)
        {
            Handheld.PlayFullScreenMovie(String.Format("{0}/{1}.mp4", m_relativePathMp4, m_currentPipisode["video_filename"]),
                                         Color.black, FullScreenMovieControlMode.Full, FullScreenMovieScalingMode.AspectFit);
        }
#endif
    }
    
    void OnChoosePipisode(ClickEvent click)
    {
        if (click.GetData() ["pipisode_title"] != null)
        {
            Debug.Log("Chose Pipisode: " + click.GetData()["pipisode_title"].ToString());
        }
        
        m_currentPipisode = click.GetData();
        
        // Change background texture
    }
}