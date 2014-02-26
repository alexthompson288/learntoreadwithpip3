using UnityEngine;
using System.Collections;

public class BuyAll : MonoBehaviour 
{
	[SerializeField]
	private BuyManager.BuyType m_buyType;
	[SerializeField]
	private UISprite m_background;

	void Awake()
	{
		bool enable = false;

		switch(m_buyType)
		{
		case BuyManager.BuyType.Books:
			enabled = BuyManager.Instance.AreAllBooksBought();
			break;
		case BuyManager.BuyType.Maps:
			enabled = BuyManager.Instance.AreAllMapsBought();
			break;
		case BuyManager.BuyType.Games:
			enabled = BuyManager.Instance.AreAllGamesBought();
			break;
		case BuyManager.BuyType.Everything:
			enabled = BuyManager.Instance.IsEverythingBought();
			break;
		}

		collider.enabled = enable;

		m_background.color = enable ? BuyManager.Instance.GetEnabledColor() : BuyManager.Instance.GetDisabledColor();
	}
	
	void OnClick () 
	{
		BuyManager.Instance.BuyAll(m_buyType);
	}
}
