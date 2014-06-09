using UnityEngine;
using System.Collections;
using System;

public class NewStoryBrowserBookButton : MonoBehaviour
{
    [SerializeField]
    private UISprite m_background;
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

        m_background.spriteName = "storycover_" + m_storyData ["storytype"].ToString();

		
        Refresh();
    }

    public void Refresh()
    {
		//m_coverSprite.GetComponent<UITexture>().material.SetFloat("_DesatAmount", 0.0f);

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
                        //m_coverSprite.mainTexture = texture;
                    }

                    m_isLoaded = true;
                }

            }
        }
        else
        {
            if ((Mathf.Abs(transform.position.x) > m_loadWithinRange * 1.5f)&&(m_dynamicLoad))
            {
                //m_coverSprite.mainTexture = null;
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
                OldStoryMenuCoordinator.SetStory(dt.Rows[0]);

    			TransitionScreen.Instance.ChangeLevel("NewStoryMenu", false);
            }
		}
		else
		{
			//BuyBooksCoordinator.Instance.Show(this);
		}
	}

    public void Buy()
    {
        BuyManager.Instance.BuyStory(m_storyData);
    }

	public void ShowBuyPanel()
	{
		//BuyBooksCoordinator.Instance.Show(this);
	}

    public IEnumerator Off()
    {
        float tweenDuration = 0.25f;
        iTween.ScaleTo(gameObject, Vector3.zero, tweenDuration);

        yield return new WaitForSeconds(tweenDuration);

        Destroy(gameObject);
    }
}
