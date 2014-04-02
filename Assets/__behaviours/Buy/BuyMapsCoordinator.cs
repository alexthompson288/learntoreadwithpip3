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
		EnableUICams();
	}

	void BuyMap(ClickEvent click)
	{
        ParentGate.Instance.OnParentGateAnswer += OnParentGateAnswer;
        ParentGate.Instance.On();
	}
    
    void OnParentGateAnswer(bool isCorrect)
    {
        ParentGate.Instance.OnParentGateAnswer -= OnParentGateAnswer;

        EnableUICams();
        
        if(isCorrect)
        {
            Debug.Log("Purchasing map");
            StartCoroutine(AttemptPurchase());
        }
    }

	bool m_purchaseIsResolved = false;
	IEnumerator AttemptPurchase()
	{
		StoreKitManager.purchaseCancelledEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
        Debug.Log("Attempting purchase on " + BuyManager.Instance.BuildMapProductIdentifier(m_mapId));
		m_purchaseIsResolved = false;
		StoreKitBinding.purchaseProduct(BuyManager.Instance.BuildMapProductIdentifier(m_mapId), 1);
		
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
				BuyInfo.Instance.SetMapPurchased(m_mapId);   
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

        JourneyMap[] spawnedMaps = UnityEngine.Object.FindObjectsOfType(typeof(JourneyMap)) as JourneyMap[];
        foreach (JourneyMap map in spawnedMaps)
        {
            map.Refresh();
        }
		
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

        if (obj.productIdentifier == BuyManager.Instance.BuildMapProductIdentifier(m_mapId))
		{
			m_purchaseIsResolved = true;
		}
		
		BuyInfo.Instance.SetMapPurchased(m_mapId);
	}
	
	void StoreKitManager_purchaseCancelledEvent(string obj)
	{
		Debug.Log("BOOK PURCHASE CANCELLED");
		
		m_purchaseIsResolved = true;
	}

	public void RefreshBuyButton()
	{
		bool mapIsLocked = !BuyInfo.Instance.IsMapBought(m_mapId);
		m_buyButton.collider.enabled = mapIsLocked;
		m_buyButton.GetComponentInChildren<UISprite>().color = mapIsLocked ? BuyManager.Instance.buyableColor : BuyManager.Instance.unbuyableColor;
	}
}
