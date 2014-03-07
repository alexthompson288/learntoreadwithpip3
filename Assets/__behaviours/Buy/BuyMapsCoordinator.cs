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
		m_buyButton.GetComponent<ClickEvent>().OnSingleClick += OnClickBuy;
	}
	
	public void Show(int mapId)
	{
		Debug.Log("Buy Map Show"); 
		m_mapId = mapId;

		DisableUICams();
		m_tweenBehaviour.On(false);
	}

	void OnClickBackCollider(ClickEvent click)
	{
		m_tweenBehaviour.Off(false);
		EnableUICams();
	}

	void OnClickBuy(ClickEvent click)
	{
		StartCoroutine(AttemptPurchase());
	}

	public string BuildProductIdentifier()
	{
		string id = "dummy_product_identifier";

		//string id = "story_" + m_storyData["id"].ToString() + "_" +
			//m_storyData["title"].ToString().Replace(" ", "_").Replace("?", "_").Replace("!", "_").Replace("-", "_").ToLower();

		return id;
	}

	bool m_purchaseIsResolved = false;
	IEnumerator AttemptPurchase()
	{
		StoreKitManager.purchaseCancelledEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
		Debug.Log("Attempting purchase on " + BuildProductIdentifier());
		m_purchaseIsResolved = false;
		StoreKitBinding.purchaseProduct(BuildProductIdentifier(), 1);
		
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
				BuyManager.Instance.SetMapPurchased(m_mapId);   
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
		
		RefreshBuyButton();
		
		StoreKitManager.purchaseCancelledEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseSuccessfulEvent -= new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
	}
	
	void StoreKitManager_purchaseSuccessfulEvent(StoreKitTransaction obj)
	{
		Debug.Log("BOOK PURCHASE SUCCESS");
		
		CharacterPopper popper = UnityEngine.Object.FindObjectOfType(typeof(CharacterPopper)) as CharacterPopper;
		if(popper != null)
		{
			popper.PopCharacter();
		}
		WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");

		if (obj.productIdentifier == BuildProductIdentifier())
		{
			m_purchaseIsResolved = true;
		}
		
		BuyManager.Instance.SetMapPurchased(m_mapId);
	}
	
	void StoreKitManager_purchaseCancelledEvent(string obj)
	{
		Debug.Log("BOOK PURCHASE CANCELLED");
		
		m_purchaseIsResolved = true;
	}

	public void RefreshBuyButton()
	{
		bool mapIsLocked = !BuyManager.Instance.IsBookBought(m_mapId);
		m_buyButton.collider.enabled = mapIsLocked;
		m_buyButton.GetComponentInChildren<UISprite>().color = mapIsLocked ? BuyManager.Instance.GetEnabledColor() : BuyManager.Instance.GetDisabledColor();
	}
}
