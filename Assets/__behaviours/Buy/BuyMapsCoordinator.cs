using UnityEngine;
using System.Collections;
using System;

public class BuyMapsCoordinator : BuyCoordinator<BuyMapsCoordinator> 
{
	[SerializeField]
	private GameObject m_buyButton;
	[SerializeField]
	private TweenOnOffBehaviour m_tweenBehaviour;
	[SerializeField]
	private ClickEvent m_backCollider;

	int m_mapId;
	
	void Awake()
	{
		m_backCollider.OnSingleClick += OnClickBackCollider;
		m_buyButton.GetComponent<ClickEvent>().OnSingleClick += BuyMap;
	}
	
	public void Show(int mapId)
	{
		Debug.Log("Buy Map Show"); 
		m_mapId = mapId;

        RefreshBuyButton();

		DisableUICams();
		m_tweenBehaviour.On(false);
	}

	void OnClickBackCollider(ClickEvent click)
	{
		m_tweenBehaviour.Off(false);
		NGUIHelpers.EnableUICams();
	}

	void BuyMap(ClickEvent click)
	{
        BuyManager.Instance.BuyMap(m_mapId);
	}

	public override void RefreshBuyButton()
	{
		bool mapIsLocked = !BuyInfo.Instance.IsMapBought(m_mapId);
		m_buyButton.collider.enabled = mapIsLocked;
		m_buyButton.GetComponentInChildren<UISprite>().color = mapIsLocked ? BuyManager.Instance.buyableColor : BuyManager.Instance.unbuyableColor;
	}
}
