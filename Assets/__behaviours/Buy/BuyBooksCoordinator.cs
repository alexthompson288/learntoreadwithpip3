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

	Texture2D m_defaultBackgroundTex;
	Color m_defaultBackgroundColor;
	
	NewStoryBrowserBookButton m_currentBook;



	void Awake()
	{
		m_defaultBackgroundTex = m_background.mainTexture as Texture2D;
		m_defaultBackgroundColor = m_background.color;

		m_backCollider.OnSingleClick += OnClickBackCollider;
		m_buyButton.GetComponent<ClickEvent>().OnSingleClick += BuyBook;
	}
	
	public void BuyBook(ClickEvent click)
	{
		ParentGate.Instance.OnParentGateAnswer += OnParentGateAnswer;
		ParentGate.Instance.On();
	}
	
	void OnParentGateAnswer(bool isCorrect)
	{
		ParentGate.Instance.OnParentGateAnswer -= OnParentGateAnswer;

		if(isCorrect && m_currentBook != null)
		{
			Debug.Log("Purchasing book");
			m_currentBook.Purchase();
		}

		EnableUICams();
	}

	public void RefreshBuyButton()
	{
		bool bookIsLocked = m_currentBook != null && !BuyManager.Instance.IsBookBought(Convert.ToInt32(m_currentBook.GetData()["id"]));
		m_buyButton.collider.enabled = bookIsLocked;
		m_buyButton.GetComponentInChildren<UISprite>().color = bookIsLocked ? BuyManager.Instance.GetEnabledColor() : BuyManager.Instance.GetDisabledColor();
	}

	public void Show(NewStoryBrowserBookButton currentBook)
	{
		DisableUICams();

		m_currentBook = currentBook;

		DataRow bookData = m_currentBook.GetData();

		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
		

		RefreshBuyButton();

		
		//m_priceMeshParent.SetActive(true);
		m_tweenBehaviour.On(false);

		
		SetFromText(m_titleLabel, bookData, "title");
		SetFromText(m_descriptionLabel, bookData, "description");
		SetFromText(m_authorLabel, bookData, "author", "by ");
		SetFromText(m_ageGroupLabel, bookData, "storytype");
		SetDifficultyStars(bookData);
		
		string price = "£0.69p";
		try
		{
			int priceTier = Convert.ToInt32(bookData["readingpadart"].ToString());
			
			switch(priceTier)
			{
			case 1:
				price = "£0.69p";
				break;
			case 2:
				price = "£1.49p";
				break;
			default:
				break;
			}
		}
		catch
		{
			Debug.LogWarning("Could not find price tier for " + bookData["title"].ToString());
		}

		m_priceLabel.text = "Buy Book - " + price;

		//m_priceTextMesh.text = price;

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from storypages where story_id='" + bookData["id"].ToString() + "' and pageorder='" + 1 + "'");

		if (dt.Rows.Count > 0)
		{
			DataRow row = dt.Rows[0];
			
			//string imageName = row["image"] == null ? "" : row["image"].ToString().Replace(".png", "");
			string bgImageName = row["backgroundart"] == null ? "" : row["backgroundart"].ToString().Replace(".png", "");
			
			//Texture2D image = LoaderHelpers.LoadObject<Texture2D>("Images/storypages/" + imageName);
			Texture2D bgImage = LoaderHelpers.LoadObject<Texture2D>("Images/storypages/" + bgImageName);

			m_background.mainTexture = bgImage != null ? bgImage : m_defaultBackgroundTex;
			m_background.color = bgImage != null ? Color.white : m_defaultBackgroundColor;
		}
	}

	public void OnClickBackCollider(ClickEvent click)
	{
		m_tweenBehaviour.Off(false);
		EnableUICams();
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
