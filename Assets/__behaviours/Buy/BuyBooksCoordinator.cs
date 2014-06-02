using UnityEngine;
using System.Collections;
using Wingrove;
using System;

public class BuyBooksCoordinator : BuyCoordinator<BuyBooksCoordinator> 
{
	[SerializeField]
	private GameObject m_buyButton;
	[SerializeField]
	private TweenOnOffBehaviour m_tweenBehaviour;
	[SerializeField]
	private UILabel m_titleLabel;
	[SerializeField]
	private UILabel m_descriptionLabel;
	[SerializeField]
	private UILabel m_priceLabel;
	[SerializeField]
	private UILabel m_authorLabel;
	[SerializeField]
	private UILabel m_ageGroupLabel;
	[SerializeField]
	private UITexture[] m_difficultyStars;
	[SerializeField]
	private ClickEvent m_backCollider;
	[SerializeField]
	private UITexture m_background;
	[SerializeField]
	private UILabel m_buyAllBooksLabel;
	[SerializeField]
	private Transform m_infoText;
	[SerializeField]
	private Transform m_infoNoBuyLocation;
	[SerializeField]
	private GameObject m_buyButtonsParent;

	Texture2D m_defaultBackgroundTex;
	Color m_defaultBackgroundColor;
	
    //NewStoryBrowserBookButton m_currentBook;

	void Awake()
	{
		m_defaultBackgroundTex = m_background.mainTexture as Texture2D;
		m_defaultBackgroundColor = m_background.color;

		m_backCollider.OnSingleClick += OnClickBackCollider;
		//m_buyButton.GetComponent<ClickEvent>().OnSingleClick += BuyBook;
	}

	void Start()
	{
		if(BuyInfo.Instance.IsEverythingBought())
		{
			m_buyButtonsParent.SetActive(false);
			m_infoText.position = m_infoNoBuyLocation.position;
			m_background.transform.localScale = Vector3.one * 0.7f;
		}
	}

	public override void RefreshBuyButton()
	{
        /*
        if (BuyInfo.Instance != null && m_currentBook != null && m_currentBook.storyData != null)
        {
            Debug.Log("m_currentBook: " + m_currentBook);
            bool bookIsLocked = !BuyInfo.Instance.IsBookBought(Convert.ToInt32(m_currentBook.storyData ["id"]));
            m_buyButton.collider.enabled = bookIsLocked;

            UISprite buyButtonSprite = m_buyButton.GetComponentInChildren<UISprite>() as UISprite;
            if (buyButtonSprite != null)
            {
                buyButtonSprite.color = bookIsLocked ? BuyManager.Instance.buyableColor : BuyManager.Instance.unbuyableColor;
            }
        }
        */
	}

    public void Show(DataRow storyData)
	{
		m_buyAllBooksLabel.text = String.Format("Unlock All {0} Books - £19.99", BuyManager.Instance.numBooks);

		//DisableUICams();

#if UNITY_IPHONE
//		System.Collections.Generic.Dictionary<string, string> ep = new System.Collections.Generic.Dictionary<string, string>();
//		ep.Add("Title", bookData["title"].ToString());
//		ep.Add("isAlreadyBought", BuyInfo.Instance.IsBookBought(Convert.ToInt32(bookData["id"])).ToString());
//		//FlurryBinding.logEventWithParameters("Buy Books Panel", ep, false);
#endif

		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
		
		RefreshBuyButton();

		m_tweenBehaviour.On(false);
		
        SetFromText(m_titleLabel, storyData, "title");
        SetFromText(m_descriptionLabel, storyData, "description");
        SetFromText(m_authorLabel, storyData, "author", "by ");
        SetFromText(m_ageGroupLabel, storyData, "storytype");
        SetDifficultyStars(storyData);
		
		string price = "£0.69";

		m_priceLabel.text = "Buy Book - " + price;


        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from storypages where story_id='" + storyData["id"].ToString() + "' and pageorder='" + 1 + "'");

		if (dt.Rows.Count > 0)
		{
			DataRow row = dt.Rows[0];
			
			string bgImageName = row["backgroundart"] == null ? "" : row["backgroundart"].ToString().Replace(".png", "");
			
			Texture2D bgImage = LoaderHelpers.LoadObject<Texture2D>("Images/storypages/" + bgImageName);

			m_background.mainTexture = bgImage != null ? bgImage : m_defaultBackgroundTex;
			m_background.color = bgImage != null ? Color.white : m_defaultBackgroundColor;
		}
	}

	public void OnClickBackCollider(ClickEvent click)
	{
		m_tweenBehaviour.Off(false);
		//NGUIHelpers.EnableUICams();
	}

	void SetFromText(UILabel label, DataRow dr, string field, string preface = "")
	{
		if ( dr[field] != null )
		{
			label.text = preface + dr[field].ToString();
		}
		else
		{
			label.text = "";
		}
	}
	
	void SetDifficultyStars(DataRow dr)
	{
		if(dr["difficulty"] != null)
		{
			int difficulty = Convert.ToInt32(dr["difficulty"]);
			
			for(int i = 0; i < m_difficultyStars.Length; ++i)
			{
				m_difficultyStars[i].enabled = (i < difficulty);
			}
		}
	}
}
