using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class StoryCoordinator : Singleton<StoryCoordinator>
{
    [SerializeField]
    private GameObject m_audioPlayButton;
    [SerializeField]
    private UILabel m_pageCountLabel;
    [SerializeField]
    private GameObject m_fadePanel;
    [SerializeField]
    private float m_fadeDuration = 0.5f;
    [SerializeField]
    private Transform[] m_textAnchors;
    [SerializeField]
    private GameObject m_textPrefab;
    [SerializeField]
    private UITexture m_storyPicture;


    static bool m_showWords = true;
    public static void SetShowWords(bool showWords)
    {
        m_showWords = showWords;
    }

    int m_storyId = 85;

    int m_numPages;
    int m_currentPage;

    string m_currentLanguage = "text";

    bool m_canTurn = false;

    List<GameObject> m_textObjects = new List<GameObject>();
    
    List<string> m_decodeList = new List<string>();

	// Use this for initialization
	IEnumerator Start () 
    {
        m_canTurn = false;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        DataRow story = DataHelpers.GetStory();
        if (story != null)
        {
            m_storyId = System.Convert.ToInt32(story["id"]);
        }

        UserStats.Activity.SetStoryId (m_storyId);
        
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from storypages where story_id='" + m_storyId + "'");
        
        m_numPages = dt.Rows.Count;
        Debug.Log("There are " + m_numPages + " pages");
	}

    void UpdatePage()
    {


        iTween.FadeTo(m_fadePanel, 1, m_fadeDuration);
    }
}
