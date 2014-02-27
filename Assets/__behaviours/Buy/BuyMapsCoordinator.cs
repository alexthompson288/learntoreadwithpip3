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
	[SerializeField]
	private UILabel m_mapNameLabel;
	[SerializeField]
	private UILabel m_descriptionLabel;

	int m_mapId;
	
	void Awake()
	{
		m_backCollider.OnSingleClick += OnClickBackCollider;
		m_buyButton.GetComponent<ClickEvent>().OnSingleClick += OnClickBuy;
	}
	
	public void Show(int mapId)
	{
		m_mapId = mapId;

		string mapName = "";
		string description = "";

		switch(m_mapId)
		{
		case 1:
			name = "Magical Forest A";
			description = "Dummy description is dummy";
			break;
		case 2:
			name = "Magical Forest B";
			description = "Dummy description is dummy";
			break;
		case 3:
			name = "Magical Forest C";
			description = "Dummy description is dummy";
			break;
		case 4:
			name = "Magical Forest D";
			description = "Dummy description is dummy";
			break;
		case 5:
			name = "Underwater A";
			description = "Dummy description is dummy";
			break;
		case 6:
			name = "Underwater B";
			description = "Dummy description is dummy";
			break;
		default:
			name = "Dummy name is dummy";
			description = "Dummy description is dummy";
			break;
		}

		m_mapNameLabel.text = mapName;
		m_descriptionLabel.text = description;

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
