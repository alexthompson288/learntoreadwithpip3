using UnityEngine;
using System.Collections;
using Wingrove;
using System;

public class BuyBooksCoordinator : MonoBehaviour 
{
	[SerializeField]
	private GameObject m_buyButton;
	[SerializeField]
	private GameObject m_buyAllBooksButton;
	[SerializeField]
	private GameObject m_buyEverythingButton;
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
	private UILabel m_levelLabel;
	[SerializeField]
	private UITexture[] m_difficultyStars;
	
	NewStoryBrowserBookButton m_currentBook;

	void Awake()
	{
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
			RefreshBuyButton();
		}
	}

	void RefreshBuyButton()
	{
		bool bookIsUnlocked = BuyManager.Instance.IsBookBought(Convert.ToInt32(m_currentBook.GetData()["id"])) || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
		m_buyButton.collider.enabled = bookIsUnlocked;
		m_buyButton.GetComponentInChildren<UISprite>().color = bookIsUnlocked ? BuyManager.Instance.GetEnabledColor() : BuyManager.Instance.GetDisabledColor();
	}

	public void Show(NewStoryBrowserBookButton currentBook, DataRow bookRow)
	{
		m_currentBook = currentBook;

		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
		

		RefreshBuyButton();

		
		//m_priceMeshParent.SetActive(true);
		m_tweenBehaviour.On();
		
		SetFromText(m_titleLabel, bookRow, "title");
		SetFromText(m_descriptionLabel, bookRow, "description");
		SetFromText(m_authorLabel, bookRow, "author", "by ");
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

		m_priceLabel.text = price;

		//m_priceTextMesh.text = price;
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
