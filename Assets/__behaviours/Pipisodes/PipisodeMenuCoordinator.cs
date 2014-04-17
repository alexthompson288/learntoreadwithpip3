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
    private ClickEvent m_playButton;
    [SerializeField]
    private ClickEvent m_buyButton;
    [SerializeField]
    private GameObject m_buyButtons;
    [SerializeField]
    private float m_buttonTweenDuration = 0.5f;
    [SerializeField]
    private UITexture m_backgroundTexture;
    [SerializeField]
    private string m_relativePathMp4 = "Pipisodes/mp4";
    [SerializeField]
    private UILabel m_titleLabel;
    [SerializeField]
    private UILabel m_overviewLabel;
    
    DataRow m_currentPipisode;

    List<DataRow> m_pipisodes;
    
    // Use this for initialization
    IEnumerator Start () 
    {
        m_playButton.OnSingleClick += OnClickPlayButton;
        m_buyButton.OnSingleClick += OnClickBuyButton;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from pipisodes ORDER BY order_number");
        m_pipisodes = dt.Rows;

        UIGrid spawnGrid = m_pipisodes.Count > 5 ? m_doubleGrid : m_singleGrid;

        if (m_pipisodes.Count > 0)
        {
            SelectPipisode(m_pipisodes[0]);

            
            for (int i = 0; i < m_pipisodes.Count; ++i)
            {
                GameObject button = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pipisodeButtonPrefab, spawnGrid.transform);
                
                button.GetComponentInChildren<UILabel>().text = (i + 1).ToString();

                if (BuyInfo.Instance.IsPipisodeBought(Convert.ToInt32(m_pipisodes[i]["id"])))
                {
                    button.GetComponentInChildren<UITexture>().gameObject.SetActive(false);
                }
                
                button.GetComponent<ClickEvent>().OnSingleClick += OnChoosePipisode;
                button.GetComponent<ClickEvent>().SetData(m_pipisodes[i]);
                
                button.GetComponent<UIDragPanelContents>().draggablePanel = m_draggablePanel;
            }
            
            spawnGrid.Reposition();
        }
    }
    
    void OnClickPlayButton(ClickEvent click)
    {
        Debug.Log("OnClickPlayButton()");
#if UNITY_ANDROID || UNITY_IPHONE
        if(BuyInfo.Instance.IsPipisodeBought(Convert.ToInt32(m_currentPipisode["id"])) && m_currentPipisode["pipisode_title"] != null)
        {
            string filename = m_currentPipisode["pipisode_title"].ToString().ToLower().Replace(" ", "_");
            Debug.Log("filename: " + filename);

            Handheld.PlayFullScreenMovie(String.Format("{0}/{1}.mp4", m_relativePathMp4, filename),
                                         Color.black, FullScreenMovieControlMode.Full, FullScreenMovieScalingMode.AspectFit);
        }
        else
        {
            Debug.Log("Unlocked: " + BuyInfo.Instance.IsPipisodeBought(Convert.ToInt32(m_currentPipisode["id"])));
            Debug.Log("hasTitle: " + m_currentPipisode["pipisode_title"] != null);
        }
#endif
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

        Debug.Log("id: " + pipisode ["id"].ToString() + " - " + BuyInfo.Instance.IsPipisodeBought(Convert.ToInt32(pipisode ["id"])));

        bool isUnlocked = BuyInfo.Instance.IsPipisodeBought(Convert.ToInt32(pipisode ["id"]));
        
        Vector3 playScale = isUnlocked ? Vector3.one : Vector3.zero;
        iTween.ScaleTo(m_playButton.gameObject, playScale, m_buttonTweenDuration);
        
        Vector3 buyScale = isUnlocked ? Vector3.zero : Vector3.one;
        iTween.ScaleTo(m_buyButtons, buyScale, m_buttonTweenDuration);
        
        m_currentPipisode = pipisode;
        
        // Change background texture
    }
}