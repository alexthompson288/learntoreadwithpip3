using UnityEngine;
using System.Collections;
using System;
using Wingrove;

public class BuyAllButton : Singleton<BuyAllButton> 
{
	[SerializeField]
	private string m_productIdentifier = "pip_stories_buy_all_0";

	string m_playerPrefsKey = "BoughtAllBooks";

	void Awake()
	{
		if(!PlayerPrefs.HasKey(m_playerPrefsKey))
		{
			PlayerPrefs.SetInt(m_playerPrefsKey, 0);
		}
	}

	public bool HasBoughtAll()
	{
		return (PlayerPrefs.GetInt(m_playerPrefsKey) == 1);
	}

	public string GetProductIdentifier()
	{
		return m_productIdentifier;
	}

	bool m_purchaseIsResolved = false;

	void OnClick()
	{
		Debug.Log("BuyallButton.OnClick()");
		StartCoroutine(SmallTween());
		StartCoroutine(AttemptPurchase());
	}

	IEnumerator SmallTween()
	{
		Debug.Log("BuyallButton.SmallTween()");
		WingroveAudio.WingroveRoot.Instance.PostEvent("FRUITMACHINE_LEVER_DOWN");
		Vector3 originalPos = transform.localPosition;
		Vector3 newPos = originalPos;
		newPos.y -= 15;
		float tweenDuration = 0.15f;
		TweenPosition.Begin(gameObject, tweenDuration, newPos);
		yield return new WaitForSeconds(tweenDuration);
		TweenPosition.Begin(gameObject, tweenDuration, originalPos);
	}
	
	IEnumerator AttemptPurchase()
	{
		Debug.Log("BuyallButton.AttemptPurchase()");
		StoreKitManager.purchaseCancelledEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
		Debug.Log("Attempting purchase on " + m_productIdentifier);
		m_purchaseIsResolved = false;
		StoreKitBinding.purchaseProduct(m_productIdentifier, 1); // TODO: This needs a proper value
		
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
				/*
				Debug.LogWarning("TIMED OUT. Platform specific compilation now causes book to unlock ONLY in EDITOR");
				int bookId = Convert.ToInt32(m_storyData["id"]);
				SessionInformation.Instance.SetBookPurchased(bookId);   
				*/
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

		StoreKitManager.purchaseCancelledEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseSuccessfulEvent -= new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
	}

	void StoreKitManager_purchaseSuccessfulEvent(StoreKitTransaction obj)
	{
		Debug.Log("PURCHASE SUCCESS");

		PlayerPrefs.SetInt(m_playerPrefsKey, 1);

		if (obj.productIdentifier == m_productIdentifier)
		{
			m_purchaseIsResolved = true;
		} 

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories");
		if(dt.Rows.Count > 0)
		{
			Debug.Log("Unlocking all stories. Count " + dt.Rows.Count);
			foreach(DataRow story in dt.Rows)
			{
				Debug.Log("Unlocking " + story["id"].ToString() + " - " + story["title"].ToString());
				SessionInformation.Instance.SetBookPurchased(Convert.ToInt32(story["id"]));
			}
		}

		NewStoryBrowserBookButton[] books = UnityEngine.Object.FindObjectsOfType(typeof(NewStoryBrowserBookButton)) as NewStoryBrowserBookButton[];

		foreach(NewStoryBrowserBookButton book in books)
		{
			//int bookId = Convert.ToInt32(book.GetData()["id"]);
			//SessionInformation.Instance.SetBookPurchased(bookId); 
			book.Refresh();
		}

		InfoPanelBox.Instance.HideBuyButtons(true);

		CharacterPopper popper = UnityEngine.Object.FindObjectOfType(typeof(CharacterPopper)) as CharacterPopper;
		if(popper != null)
		{
			popper.PopCharacter();
		}
		WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
	}
	
	void StoreKitManager_purchaseCancelledEvent(string obj)
	{
		Debug.Log("PURCHASE FAILED");
		m_purchaseIsResolved = true;
	}
}
