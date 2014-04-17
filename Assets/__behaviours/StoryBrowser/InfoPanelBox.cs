using UnityEngine;
using System.Collections;
using System;
using Wingrove;

// TODO: Delete this class after checking for safety
public class InfoPanelBox : Singleton<InfoPanelBox> 
{
	[SerializeField]
	private TweenOnOffBehaviour m_tweenBehaviour;
	[SerializeField]
	private GameObject m_objectToEnable;
	[SerializeField]
	private UILabel m_titleLabel;
	[SerializeField]
	private UILabel m_descriptionLabel;
	[SerializeField]
	private UILabel m_priceLabel;
	[SerializeField]
	private UILabel m_authorLabel;
	[SerializeField]
	private UILabel m_difficultyLabel;
	[SerializeField]
	private UILabel m_levelLabel;
	[SerializeField]
	private UITexture[] m_difficultyStars;
	[SerializeField]
	private GameObject m_priceMeshParent;
	[SerializeField]
	private TextMesh m_priceTextMesh;
	[SerializeField]
	private TweenOnOffBehaviour m_buyButton;
	[SerializeField]
	private TweenOnOffBehaviour m_buyAllButton;

	NewStoryBrowserBookButton m_currentBook = null;

	public NewStoryBrowserBookButton GetCurrentBook()
	{
		return m_currentBook;
	}

	public void SetCurrentBook(NewStoryBrowserBookButton currentBook)
	{
		m_currentBook = currentBook;
	}

	public void Show(DataRow bookRow)
	{
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
		
		int bookId = Convert.ToInt32(bookRow["id"]);
		if (SessionInformation.Instance.IsBookBought(bookId) || 
		    NewStoryBrowserBookPopulator.Instance.IsBookUnlocked(bookId))
		{
			m_buyButton.gameObject.SetActive(false);
		}

		if(((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked)// || BuyAllButton.Instance.HasBoughtAll())
		{
			m_buyButton.gameObject.SetActive(false);
			m_buyAllButton.gameObject.SetActive(false);
		}

		m_priceMeshParent.SetActive(true);
		//m_objectToEnable.SetActive(true);
		m_tweenBehaviour.Off(false);
		
		SetFromText(m_titleLabel, bookRow, "title");
		SetFromText(m_descriptionLabel, bookRow, "description");
		SetFromText(m_authorLabel, bookRow, "author", "by ");
		//SetFromText(m_difficultyLabel, bookRow, "difficulty", "Difficulty: ");
		SetFromText(m_levelLabel, bookRow, "storytype");
		SetDifficultyStars(bookRow);

		string price = "£0.69p";
		try
		{
			int priceTier = Convert.ToInt32(bookRow["readingpadart"].ToString());

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
			Debug.LogWarning("Could not find price tier for " + bookRow["title"].ToString());
		}

		//m_priceLabel.text = price;
		m_priceTextMesh.text = price;
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

	public void HideBuyButtons(bool hideBoth)
	{
		m_buyButton.Off(false);

		if(hideBoth)
		{
			m_buyAllButton.Off(false);
		}
	}
	
	public void Hide()
	{
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");

		m_buyButton.gameObject.SetActive(true);
		m_buyButton.On();
		m_buyAllButton.gameObject.SetActive(true);
		m_buyAllButton.On();

		m_currentBook = null;
		//m_objectToEnable.SetActive(false);
		m_tweenBehaviour.On();
		

		m_priceMeshParent.SetActive(false);
	}
}
