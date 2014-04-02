using UnityEngine;
using System.Collections;
using System;

public class NewStoryBrowserBookButton : MonoBehaviour
{
    [SerializeField]
    private UITexture m_coverSprite;
    [SerializeField]
    private DataRow m_storyData;
    [SerializeField]
    private float m_loadWithinRange = 2.0f;
    [SerializeField]
    private bool m_dynamicLoad = false;
    [SerializeField]
    private GameObject[] m_stars;
    [SerializeField]
    private GameObject m_buyButton;
    [SerializeField]
    private GameObject m_wordsButton;
    [SerializeField]
    private GameObject m_padlockHierarchy;
	[SerializeField]
	private GameObject m_glowTexture;

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

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programsessions WHERE story_id=" + Convert.ToInt32(m_storyData["id"]));

		if(dt.Rows.Count > 0)
		{
			int storySessionNum = Convert.ToInt32(dt.Rows[0]["number"]);
			if(storySessionNum <= JourneyInformation.Instance.GetSessionsCompleted())
			{
				m_glowTexture.SetActive(true);
			}
		}
		
        Refresh();
    }

    public void Refresh()
    {
		m_coverSprite.GetComponent<UITexture>().material.SetFloat("_DesatAmount", 0.0f);

		m_buyButton.SetActive(false);

        int bookId = Convert.ToInt32(m_storyData["id"]);
        if (BuyInfo.Instance.IsBookBought(bookId))
		{
            m_padlockHierarchy.SetActive(false);
            //m_wordsButton.SetActive(true);
            m_coverSprite.GetComponent<UITexture>().material.SetFloat("_DesatAmount", 0.0f);
        }
        else
        {
            m_padlockHierarchy.SetActive(true);
            m_wordsButton.SetActive(false);
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
			SessionInformation.Instance.SelectBookId(bookId);
			TransitionScreen.Instance.ChangeLevel("NewStartStory", false);
			TransitionScreen.Instance.ChangeLevel("NewStories", false);
		}
		else
		{
			BuyBooksCoordinator.Instance.Show(this);
		}
	}

	public void ShowBuyPanel()
	{
		BuyBooksCoordinator.Instance.Show(this);
	}

    public void Purchase()
    {
        int bookId = Convert.ToInt32(m_storyData["id"]);
		Debug.Log("Purchasing " + bookId);
        StartCoroutine(AttemptPurchase());
    }

    bool m_purchaseIsResolved = false;
    IEnumerator AttemptPurchase()
    {
        StoreKitManager.purchaseCancelledEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseFailedEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);

		m_purchaseIsResolved = false;

		Debug.Log("Attempting purchase: " + BuyManager.Instance.BuildStoryProductIdentifier(m_storyData));
		StoreKitBinding.purchaseProduct(BuyManager.Instance.BuildStoryProductIdentifier(m_storyData), 1);

        UnityEngine.Object[] uiCameras = GameObject.FindObjectsOfType(typeof(UICamera));
        foreach (UICamera cam in uiCameras)
        {
            cam.enabled = false;
        }

        float pcTimeOut = 0;
        while (!m_purchaseIsResolved)
        {
            pcTimeOut += Time.deltaTime;
#if UNITY_EDITOR
            if (pcTimeOut > 3.0f)
            {
				Debug.LogWarning("PC TIMEOUT. UNLOCKING BY DEFAULT");
                int bookId = Convert.ToInt32(m_storyData["id"]);
				BuyInfo.Instance.SetBookPurchased(bookId);   
                m_purchaseIsResolved = true;
            }
#endif
            yield return null;
        }

        foreach (UICamera cam in uiCameras)
        {
            if (cam != null)
            {
                cam.enabled = true;
            }
        }

        Refresh();
		BuyBooksCoordinator.Instance.RefreshBuyButton();

        StoreKitManager.purchaseCancelledEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseFailedEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseSuccessfulEvent -= new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
    }

    void StoreKitManager_purchaseSuccessfulEvent(StoreKitTransaction obj)
    {
		Debug.Log("BOOK PURCHASE SUCCESS: " + m_storyData["title"].ToString());

		CharacterPopper popper = UnityEngine.Object.FindObjectOfType(typeof(CharacterPopper)) as CharacterPopper;
		if(popper != null)
		{
			popper.PopCharacter();
		}
		WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");

        int bookId = Convert.ToInt32(m_storyData["id"]);
		if (obj.productIdentifier == BuyManager.Instance.BuildStoryProductIdentifier(m_storyData))
        {
            m_purchaseIsResolved = true;
        }

		BuyInfo.Instance.SetBookPurchased(bookId);
		InfoPanelBox.Instance.HideBuyButtons(false);
    }

    void StoreKitManager_purchaseCancelledEvent(string obj)
    {
		Debug.Log("BOOK PURCHASE CANCELLED: " + m_storyData["title"].ToString());

        m_purchaseIsResolved = true;
    }

	public DataRow GetData()
	{
		return m_storyData;
	}
}
