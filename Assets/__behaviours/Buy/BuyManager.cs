﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public class BuyManager : Singleton<BuyManager> 
{
#if UNITY_EDITOR
	[SerializeField]
	private bool m_unlockInEditor;
#endif

    [SerializeField]
	private bool m_logProductRequests = false;

	[SerializeField]
	private Color m_buttonEnabled;
	[SerializeField]
	private Color m_buttonDisabled;

	int m_numBooks = 0;

	public Color GetEnabledColor()
	{
		return m_buttonEnabled;
	}

	public Color GetDisabledColor()
	{
		return m_buttonDisabled;
	}

	// Product Purchasing... See below for Data Saving
	[SerializeField]
	private string m_booksProductIdentifier = "pip_stories_buy_all_0";
	[SerializeField]
	private string m_mapsProductIdentifier = "pip_stories_buy_all_0"; // TODO: Change to correct value
	[SerializeField]
	private string m_gamesProductIdentifier = "pip_stories_buy_all_0"; // TODO: Change to correct value
	[SerializeField]
	private string m_everythingProductIdentifier = "pip_stories_buy_all_0"; // TODO: Change to correct value

	public enum BuyType
	{
		Books,
		Maps,
		Games,
		Everything
	}
	
	BuyType m_buyType;

	bool m_purchaseIsResolved = false;

	string m_productIdentifier;

	public void BuyAll(BuyType buyType)
	{
		m_buyType = buyType;
		
		ParentGate.Instance.OnParentGateAnswer += OnParentGateAnswer;
		ParentGate.Instance.On();
	}
	
	public void OnParentGateAnswer(bool isCorrect)
	{
		ParentGate.Instance.OnParentGateAnswer -= OnParentGateAnswer;
		
		if(isCorrect)
		{
			StartCoroutine(AttemptPurchase());
		}
	}
	
	IEnumerator AttemptPurchase()
	{
		Debug.Log("BuyInfo.AttemptPurchase()");
		StoreKitManager.purchaseCancelledEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
		
		Action<StoreKitTransaction> purchaseSuccessfulEvent = new Action<StoreKitTransaction>(StoreKitManager_booksPurchaseSuccessfulEvent);

		switch(m_buyType)
		{
		case BuyType.Books:
			m_productIdentifier = m_booksProductIdentifier;
			purchaseSuccessfulEvent = new Action<StoreKitTransaction>(StoreKitManager_booksPurchaseSuccessfulEvent);
			break;
		case BuyType.Maps:
			m_productIdentifier = m_mapsProductIdentifier;
			purchaseSuccessfulEvent = new Action<StoreKitTransaction>(StoreKitManager_mapsPurchaseSuccessfulEvent);
			break;
		case BuyType.Games:
			m_productIdentifier = m_gamesProductIdentifier;
			purchaseSuccessfulEvent = new Action<StoreKitTransaction>(StoreKitManager_gamesPurchaseSuccessfulEvent);
			break;
		case BuyType.Everything:
			m_productIdentifier = m_everythingProductIdentifier;
			purchaseSuccessfulEvent = new Action<StoreKitTransaction>(StoreKitManager_everythingPurchaseSuccessfulEvent);
			break;
		}
		
		StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(purchaseSuccessfulEvent);
		
		Debug.Log("Attempting purchase on " + m_productIdentifier);
		m_purchaseIsResolved = false;
		StoreKitBinding.purchaseProduct(m_productIdentifier, 1);
		
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
				Debug.Log("PURCHASE TIMED OUT");
				UnlockOnTimeOut();
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

		RefreshBuyAllButtons();
		RefreshBooks();
		RefreshMaps();
		RefreshGames();
		
		StoreKitManager.purchaseCancelledEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseSuccessfulEvent -= new Action<StoreKitTransaction>(purchaseSuccessfulEvent);
	}

#if UNITY_EDITOR
	void UnlockOnTimeOut()
	{
		Debug.Log("UNLOCKING ON TIMEOUT");
		switch(m_buyType)
		{
		case BuyType.Books:
			SetAllBooksPurchased();
			break;
		case BuyType.Maps:
			SetAllMapsPurchased();
			break;
		case BuyType.Games:
			SetAllGamesPurchased();
			break;
		case BuyType.Everything:
			SetAllBooksPurchased();
			SetAllMapsPurchased();
			SetAllGamesPurchased();
			break;
		}
	}
#endif

	public IEnumerator RestorePurchases(float restoreTime)
	{
		//string receiptLocation = StoreKitBinding.getAppStoreReceiptLocation();

		Debug.Log("RestorePurchases - Opening processes");

		StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(StoreKitManager_restorePurchaseSuccessfulEvent);
		StoreKitManager.purchaseCancelledEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);

		StoreKitManager.restoreTransactionsFailedEvent += new Action<string>(StoreKitManager_restoreTransactionsFailedEvent);
		StoreKitManager.restoreTransactionsFinishedEvent += new Action(StoreKitManager_restoreTransactionsFinishEvent);
		
		UnityEngine.Object[] uiCameras = GameObject.FindObjectsOfType(typeof(UICamera));
		foreach (UICamera cam in uiCameras)
		{
			cam.enabled = false;
		}

		// Restore
		Debug.Log("RestorePurchases - Calling restoreCompletedTransactions");
		StoreKitBinding.restoreCompletedTransactions();

		Debug.Log("RestorePurchases - Waiting for " + restoreTime);
		yield return new WaitForSeconds(restoreTime);

		Debug.Log("RestorePurchases - Closing processes");

		foreach (UICamera cam in uiCameras)
		{
			if (cam != null)
			{
				cam.enabled = true;
			}
		}
		
		RefreshBuyAllButtons();
		RefreshBooks();
		RefreshMaps();
		RefreshGames();
		
		StoreKitManager.purchaseSuccessfulEvent -= new Action<StoreKitTransaction>(StoreKitManager_restorePurchaseSuccessfulEvent);
		StoreKitManager.purchaseCancelledEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);

		/*
		float pcTimeOut = 0;
		while (!m_purchaseIsResolved)
		{
			pcTimeOut += Time.deltaTime;
			#if UNITY_EDITOR
			if (pcTimeOut > 3.0f)
			{
				Debug.Log("PURCHASE TIMED OUT");

				StoreKitManager_restoreTransactionsFinishEvent();
			}
			#endif
			yield return null;
		}
		*/
	}

	void StoreKitManager_restoreTransactionsFinishEvent() // TODO: This won't work because this method is called before all of the purchases are processed. You need to find the length of the queue
	{
		Debug.Log("restoreTransactionsFinishEvent");
	}

	void StoreKitManager_restoreTransactionsFailedEvent(string str)
	{
		Debug.Log("restoreTransactionsFailedEvent: " + str);

		StoreKitManager_restoreTransactionsFinishEvent();
	}

	void StoreKitManager_restorePurchaseSuccessfulEvent(StoreKitTransaction obj)
	{
		Debug.Log("restorePurchaseSuccessfulEvent: " + obj.productIdentifier);

		string productId = obj.productIdentifier;

        if (productId == m_booksProductIdentifier)
        {
            SetAllBooksPurchased();
        } 
        else if (productId == m_mapsProductIdentifier)
        {
            SetAllMapsPurchased();
        } 
        else if (productId == m_gamesProductIdentifier)
        {
            SetAllGamesPurchased();
        } 
        else if (productId = m_everythingProductIdentifier)
        {
            SetAllBooksPurchased();
            SetAllMapsPurchased();
            SetAllGamesPurchased();
        }
		else if(productId.Contains("stories")) // Book
		{
			// Find the story id
			string idNum = System.Text.RegularExpressions.Regex.Match(productId, @"\d+").Value;
			int bookId = Convert.ToInt32(idNum);

			SetBookPurchased(bookId);
		}
		else if(productId.Contains("map")) // Map
		{
			// Find the map id
			//string idNum = System.Text.RegularExpressions.Regex.Match(productId, @"\d+").Value;
			int mapId = Array.IndexOf(m_mapProductIdentifiers, productId);

            Debug.Log("mapId: " + mapId);
			
			SetMapPurchased(mapId);
		}
		else
		{
			Debug.LogError("Product Identifier not recognized: " + productId);
		}
	}
	
	void StoreKitManager_everythingPurchaseSuccessfulEvent(StoreKitTransaction obj)
	{
		Debug.Log("PURCHASE SUCCESS: ALL MAPS");
		
		if (obj.productIdentifier == m_productIdentifier)
		{
			m_purchaseIsResolved = true;
		} 


#if UNITY_IPHONE
		FlurryBinding.logEvent("Purchasing Everything", false);
#endif

		SetAllBooksPurchased();
		SetAllMapsPurchased();
		SetAllGamesPurchased();
		
		CelebratePurchase();
	}
	
	void StoreKitManager_gamesPurchaseSuccessfulEvent(StoreKitTransaction obj)
	{
		Debug.Log("PURCHASE SUCCESS: ALL GAMES");
		
		if (obj.productIdentifier == m_productIdentifier)
		{
			m_purchaseIsResolved = true;
		} 
		
		SetAllGamesPurchased();
		
		CelebratePurchase();
	}
	
	void StoreKitManager_mapsPurchaseSuccessfulEvent(StoreKitTransaction obj)
	{
		Debug.Log("PURCHASE SUCCESS: ALL MAPS");
		
		if (obj.productIdentifier == m_productIdentifier)
		{
			m_purchaseIsResolved = true;
		} 
		
		SetAllMapsPurchased();
		
		CelebratePurchase();
	}
	
	void StoreKitManager_booksPurchaseSuccessfulEvent(StoreKitTransaction obj)
	{
		Debug.Log("PURCHASE SUCCESS: ALL STORIES");
		
		if (obj.productIdentifier == m_productIdentifier)
		{
			m_purchaseIsResolved = true;
		} 
		
		SetAllBooksPurchased();

		CelebratePurchase();
	}

	void CelebratePurchase()
	{
		CharacterPopper popper = UnityEngine.Object.FindObjectOfType(typeof(CharacterPopper)) as CharacterPopper;
		if(popper != null)
		{
			popper.PopCharacter();
		}
		WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
	}
	
	void StoreKitManager_purchaseCancelledEvent(string obj)
	{
		Debug.Log("PURCHASE CANCELLED - m_buyType: " + m_buyType.ToString());
		Debug.Log("Cancelled Message: " + obj);
		m_purchaseIsResolved = true;
	}

	void RefreshBooks()
	{
		NewStoryBrowserBookButton[] books = UnityEngine.Object.FindObjectsOfType(typeof(NewStoryBrowserBookButton)) as NewStoryBrowserBookButton[];
		foreach(NewStoryBrowserBookButton book in books)
		{
			book.Refresh();
		}
	}

	void RefreshMaps()
	{
		JourneyMap[] maps = UnityEngine.Object.FindObjectsOfType(typeof(JourneyMap)) as JourneyMap[];
		foreach(JourneyMap map in maps)
		{
			map.Refresh();
		}
	}

	void RefreshGames()
	{
		BuyableGame[] games = UnityEngine.Object.FindObjectsOfType(typeof(BuyableGame)) as BuyableGame[];
		foreach(BuyableGame game in games)
		{
			game.Refresh();
		}
	}

	void RefreshBuyAllButtons()
	{
		BuyAll[] buttons = UnityEngine.Object.FindObjectsOfType(typeof(BuyAll)) as BuyAll[];
		foreach(BuyAll button in buttons)
		{
			button.Refresh();
		}
	}

	///////////////////////////////////////////////////////////////////

#if UNITY_EDITOR
	[SerializeField]
	private bool m_resetPurchases;
#endif

	// Data Saving
	[SerializeField]
	private string[] m_mapProductIdentifiers;

	[SerializeField]
	private int[] m_defaultUnlockedBooks;
	[SerializeField]
	private int[] m_defaultUnlockedMaps;
	[SerializeField]
	private string[] m_defaultUnlockedGames;
	
	HashSet<int> m_boughtBooks = new HashSet<int>();
	HashSet<int> m_boughtMaps = new HashSet<int>();
	bool m_boughtGames = false;

	void Awake()
	{
#if UNITY_EDITOR
		if(m_resetPurchases)
		{
			Save ();
		}
#endif

		foreach(int book in m_defaultUnlockedBooks)
		{
			m_boughtBooks.Add(book);
		}

		foreach(int map in m_defaultUnlockedMaps)
		{
			m_boughtMaps.Add(map);
		}

		Load();
	}

	void StoreKitManager_productListReceivedEvent(List<StoreKitProduct> productList)
	{
		Debug.Log("PRODUCTLIST: Received " + productList.Count);
		foreach(StoreKitProduct product in productList)
		{
			Debug.Log(product.productIdentifier);
		}

		m_productListResolved = true;
	}

	void StoreKitManager_productListFailedEvent(string s)
	{
		Debug.Log("PRODUCTLIST: Failed");
		Debug.Log("Failed Message: " + s);

		m_productListResolved = true;
	}

	bool m_productListResolved = false;

	public int GetNumBooks()
	{
		return m_numBooks;
	}


	IEnumerator Start()
	{
		if(m_logProductRequests)
			Debug.Log("PRODUCTLIST: Waiting for db");

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		m_numBooks = 0;
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories");
		foreach(DataRow book in dt.Rows)
		{
			if(book["publishable"] != null && book["publishable"].ToString() == "t")
			{
				++m_numBooks;
			}
		}

		if(m_logProductRequests)
			Debug.Log("PRODUCTLIST: Building");

		List<string> productIdentifiers = new List<string>();

		DataTable storyTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE publishable = 't'");
		foreach(DataRow story in storyTable.Rows)
		{
			productIdentifiers.Add(BuildStoryProductIdentifier(story));
		}

		foreach(string mapIdentifier in m_mapProductIdentifiers)
		{
			productIdentifiers.Add(mapIdentifier);
		}

		productIdentifiers.Add(m_booksProductIdentifier);
		productIdentifiers.Add(m_mapsProductIdentifier);
		productIdentifiers.Add(m_gamesProductIdentifier);
		productIdentifiers.Add(m_everythingProductIdentifier);

		StoreKitManager.productListReceivedEvent += new Action<List<StoreKitProduct>>(StoreKitManager_productListReceivedEvent);
		StoreKitManager.productListRequestFailedEvent += new Action<string>(StoreKitManager_productListFailedEvent);

		if(m_logProductRequests)
			Debug.Log("PRODUCTLIST: Requesting " + productIdentifiers.Count);

		StoreKitBinding.requestProductData(productIdentifiers.ToArray());

		while(!m_productListResolved)
		{
			yield return null;
		}

		StoreKitManager.productListReceivedEvent -= new Action<List<StoreKitProduct>>(StoreKitManager_productListReceivedEvent);
		StoreKitManager.productListRequestFailedEvent -= new Action<string>(StoreKitManager_productListFailedEvent);

		if(m_logProductRequests)
			Debug.Log("PRODUCTLIST: Finished");
	}

	public string BuildStoryProductIdentifier(DataRow storyData)
	{
		string id = "stories_" + storyData["id"].ToString() + "_" +
			storyData["title"].ToString().TrimEnd(new char[] { ' ' }).Replace(" ", "_").Replace("?", "").Replace("!", "").Replace("-", "_").Replace("'", "").Replace(".", "").ToLower();
		
		return id;
	}
	
	public string BuildMapProductIdentifier(int map)
	{
		return m_mapProductIdentifiers[map - 1];
	}

	public bool IsBookBought(int bookId)
	{
#if UNITY_EDITOR
		if(m_unlockInEditor)
		{
			return true;
		}
#endif

		return m_boughtBooks.Contains(bookId) || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
	}

	public bool AreAllBooksBought()
	{
#if UNITY_EDITOR
		if(m_unlockInEditor)
		{
			return true;
		}
#endif

		if(((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked)
		{
			return true;
		}
		else
		{
			bool allBooksBought = true;

			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories");

			foreach(DataRow story in dt.Rows)
			{
				if(story["publishable"] != null && story["publishable"].ToString() == "t" && !m_boughtBooks.Contains(Convert.ToInt32(story["id"])))
				{
					allBooksBought = false;
					break;
				}
			}

			//Debug.Log("allBooksBought: " + allBooksBought);

			return allBooksBought;
		}
	}

	public void SetBookPurchased(int bookId)
	{
#if UNITY_IPHONE
		Dictionary<string, string> ep = new Dictionary<string, string>();
		ep.Add("BookID: ", bookId.ToString());
		FlurryBinding.logEventWithParameters("BookPurchased", ep, false);
#endif

		m_boughtBooks.Add(bookId);
		Save();
	}

	public void SetAllBooksPurchased()
	{
#if UNITY_IPHONE
		FlurryBinding.logEvent("AllBooksPurchased", false);
#endif

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories");
		if(dt.Rows.Count > 0)
		{
			Debug.Log("Unlocking all stories. Count " + dt.Rows.Count);
			foreach(DataRow story in dt.Rows)
			{
				if(story["publishable"] != null && story["publishable"].ToString() == "t")
				{
					Debug.Log("Unlocking " + story["id"].ToString() + " - " + story["title"].ToString());
					m_boughtBooks.Add(Convert.ToInt32(story["id"]));
				}
			}
		}
		Save ();
	}

	public bool IsMapBought(int mapId)
	{
#if UNITY_EDITOR
		if(m_unlockInEditor)
		{
			return true;
		}
#endif

        Debug.Log(String.Format("IsMapBought({0}) - {1}", mapId, (m_boughtMaps.Contains(mapId) || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked)));

		return m_boughtMaps.Contains(mapId) || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
	}

	public bool AreAllMapsBought()
	{
#if UNITY_EDITOR
		if(m_unlockInEditor)
		{
			return true;
		}
#endif

		if(((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked)
		{
			return true;
		}
		else
		{
			bool allMapsBought = true;

            for(int i = 0; i < m_mapProductIdentifiers.Length; ++i)
            {
                if(!m_boughtMaps.Contains(i))
                {
                    allMapsBought = false;
                    break;
                }
            }


			//Debug.Log("ALL MAPS BOUGHT: " + allMapsBought);

			return allMapsBought;
		}
	}

	public void SetMapPurchased(int mapId)
	{
#if UNITY_IPHONE
		Dictionary<string, string> ep = new Dictionary<string, string>();
		ep.Add("MapID: ", mapId.ToString());
		FlurryBinding.logEventWithParameters("MapPurchased", ep, false);
#endif

        Debug.Log(String.Format("SetMapPurchased({0})", mapId));
		m_boughtMaps.Add(mapId);
		Save ();
	}

	public void SetAllMapsPurchased()
	{
#if UNITY_IPHONE
		FlurryBinding.logEvent("AllMapsPurchased", false);
#endif

        for(int i = 0; i < m_mapProductIdentifiers.Length; ++i)
        {
			m_boughtMaps.Add(i);
		}
		Save ();
	}

	public bool IsGameBought(string gameSceneName)
	{
#if UNITY_EDITOR
		if(m_unlockInEditor)
		{
			return true;
		}
#endif

		return m_boughtGames || Array.IndexOf(m_defaultUnlockedGames, gameSceneName) != -1 || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
	}

	public bool AreAllGamesBought()
	{
#if UNITY_EDITOR
		if(m_unlockInEditor)
		{
			return true;
		}
#endif

		return m_boughtGames || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
	}

	public void SetAllGamesPurchased()
	{
#if UNITY_IPHONE
		FlurryBinding.logEvent("AllGamesPurchased", false);
#endif

		m_boughtGames = true;
		Save ();
	}

	public bool IsEverythingBought()
	{
		//Debug.Log("BuyManager.IsEverythingBought()");
		if(((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked)
		{
			//Debug.Log("Unlocked in settings");
			return true;
		}
		else
		{
			//Debug.Log("Unlocked from purchases: " + (AreAllBooksBought() && AreAllMapsBought() && AreAllGamesBought()));
			return AreAllBooksBought() && AreAllMapsBought() && AreAllGamesBought();
		}
	}

	void Load()
	{
		//Debug.Log("BuyManager.Load()");

		DataSaver ds = new DataSaver("BuyInfo");
		MemoryStream data = ds.Load();
		BinaryReader br = new BinaryReader(data);
		
		if (data.Length != 0)
		{
			int numBooks = br.ReadInt32();
			for (int i = 0; i < numBooks; ++i)
			{
				int bookId = br.ReadInt32();
				m_boughtBooks.Add(bookId);
			}

			int numMaps = br.ReadInt32();
			for(int i = 0; i < numMaps; ++i)
			{
				int mapId = br.ReadInt32();
				m_boughtMaps.Add(mapId);
			}

			m_boughtGames = br.ReadBoolean();
		}

		br.Close();
		data.Close();
	}
	
	void Save()
	{
		DataSaver ds = new DataSaver("BuyInfo");
		MemoryStream newData = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(newData);
		
		bw.Write(m_boughtBooks.Count);
		foreach (int i in m_boughtBooks)
		{
			bw.Write(i);
		}

		bw.Write(m_boughtMaps.Count);
		foreach(int i in m_boughtMaps)
		{
			bw.Write(i);
		}

		bw.Write(m_boughtGames);


		ds.Save(newData);
		
		bw.Close();
		newData.Close();
	}
}
