using UnityEngine;
using System.Collections;

public class BuyAll : MonoBehaviour 
{
	[SerializeField]
	private BuyManager.BuyType m_buyType;
	[SerializeField]
	private UISprite m_background;
	[SerializeField]
	private bool m_isClickable = true;

	IEnumerator Start()
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		Refresh();
	}
	
	void OnClick () 
	{
		if(m_isClickable)
		{
			BuyManager.Instance.Buy(m_buyType);
		}
	}

	public void Refresh()
	{
		bool locked = false;
		
		switch(m_buyType)
		{
		case BuyManager.BuyType.Books:
			locked = !BuyInfo.Instance.AreAllBooksBought();
			break;
		case BuyManager.BuyType.Maps:
			locked = !BuyInfo.Instance.AreAllMapsBought();
			break;
		case BuyManager.BuyType.Games:
			locked = !BuyInfo.Instance.AreAllGamesBought();
			break;
		case BuyManager.BuyType.Everything:
			locked = !BuyInfo.Instance.IsEverythingBought();
			break;
		}

		//Debug.Log("BuyAll - " + m_buyType + " - unlocked: " + !locked);
		
		collider.enabled = locked;

		//Debug.Log(m_buyType + " - collider.enabled: " + collider.enabled);
		
		m_background.color = locked ? BuyManager.Instance.buyableColor : BuyManager.Instance.unbuyableColor;
	}
}
