using UnityEngine;
using System.Collections;
using System;

public class NewStoryBrowserBookButton : MonoBehaviour
{
    [SerializeField]
    private UITexture m_coverSprite;
    [SerializeField]
    private float m_loadWithinRange = 2.0f;
    [SerializeField]
    private bool m_dynamicLoad = false;
    [SerializeField]
    private GameObject[] m_stars;
    [SerializeField]
    private GameObject m_padlockHierarchy;

    [SerializeField]
    private DataRow m_storyData;
    public DataRow storyData
    {
        get
        {
            return m_storyData;
        }
    }

    bool m_isLoaded;


    public void SetUpWith(DataRow dataRow)
    {
        m_storyData = dataRow;
        if (Convert.ToInt32(dataRow["difficulty"]) == 1)
        {
            m_stars[0].SetActive(true);
        }
        else if ( Convert.ToInt32(dataRow["difficulty"]) == 2 )
        {
            m_stars[1].SetActive(true);
        }
        else
        {
            m_stars[2].SetActive(true);
        }
		
        Refresh();
    }

    public void Refresh()
    {
		m_coverSprite.GetComponent<UITexture>().material.SetFloat("_DesatAmount", 0.0f);

        int bookId = Convert.ToInt32(m_storyData["id"]);
        if (BuyInfo.Instance.IsBookBought(bookId))
		{
            m_padlockHierarchy.SetActive(false);
            m_coverSprite.GetComponent<UITexture>().material.SetFloat("_DesatAmount", 0.0f);
        }
        else
        {
            m_padlockHierarchy.SetActive(true);
            m_coverSprite.GetComponent<UITexture>().material.SetFloat("_DesatAmount", 1.0f);
        }
    }

    public int GetDifficulty()
    {
        return Convert.ToInt32(m_storyData["difficulty"]);
    }

    void Update()
    {
        if (!m_isLoaded)
        {
            if ((Mathf.Abs(transform.position.x) < m_loadWithinRange)||(!m_dynamicLoad))
            {
                if (m_storyData["storycoverartwork"] != null)
                {
                    string artworkFile = m_storyData["storycoverartwork"].ToString();

                    Texture2D texture = LoaderHelpers.LoadObject<Texture2D>("Images/story_covers/" + artworkFile);
                    if (texture != null)
                    {
                        m_coverSprite.mainTexture = texture;
                    }

                    m_isLoaded = true;
                }

            }
        }
        else
        {
            if ((Mathf.Abs(transform.position.x) > m_loadWithinRange * 1.5f)&&(m_dynamicLoad))
            {
                m_coverSprite.mainTexture = null;
                Resources.UnloadUnusedAssets();
                m_isLoaded = false;
            }
        }
    }

	void OnClick()
	{
		int bookId = Convert.ToInt32(m_storyData["id"]);
		Debug.Log("Clicked bookId: " + bookId);

		if (BuyInfo.Instance.IsBookBought(bookId) || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked)
		{
			//SessionInformation.Instance.SelectBookId(bookId);
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE id=" + bookId);
            if(dt.Rows.Count > 0)
            {
                StoryMenuCoordinator.SetStory(dt.Rows[0]);

    			TransitionScreen.Instance.ChangeLevel("NewStoryMenu", false);
            }
		}
		else
		{
			BuyBooksCoordinator.Instance.Show(this);
		}
	}

    public void Buy()
    {
        BuyManager.Instance.BuyStory(m_storyData);
    }

	public void ShowBuyPanel()
	{
		BuyBooksCoordinator.Instance.Show(this);
	}
}
