using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public class BuyManager : Singleton<BuyManager> 
{
	[SerializeField]
	private Color m_buttonEnabled;
	[SerializeField]
	private Color m_buttonDisabled;

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

		yield return new WaitForSeconds(restoreTime);

		Debug.Log("RestorePurchases - Closing processes");

		//UnityEngine.Object[] uiCameras = GameObject.FindObjectsOfType(typeof(UICamera));
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

	/*
	int GetNumTransactions()
	{
		int numTransactions = -1;

		string path = StoreKitBinding.getAppStoreReceiptLocation();

		StringBuilder sb = new StringBuilder();
		using (StreamReader sr = new StreamReader(path)) 
		{
			String line;
			// Read and display lines from the file until the end of 
			// the file is reached.
			while ((line = sr.ReadLine()) != null) 
			{
				sb.AppendLine(line);
			}
		}
		string allLines = sb.ToString();

		ReceiptVerification rv = new ReceiptVerification(false, allLines);

		if(rv.Verify())
		{
		}

		return numTransactions;
	}
	*/


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

		if(productId.Contains("story")) // Book
		{
			// Find the story id
			string idNum = System.Text.RegularExpressions.Regex.Match(productId, @"\d+").Value;
			int bookId = Convert.ToInt32(idNum);

			SetBookPurchased(bookId);
		}
		else if(productId.Contains("map")) // Map
		{
			// Find the story id
			string idNum = System.Text.RegularExpressions.Regex.Match(productId, @"\d+").Value;
			int mapId = Convert.ToInt32(idNum);
			
			SetMapPurchased(mapId);
		}
		else if(productId.Contains("stories")) // All Books
		{
			SetAllBooksPurchased();
		}
		else if(productId.Contains("maps")) // All Maps
		{
			SetAllMapsPurchased();
		}
		else if(productId.Contains("games")) // All Games
		{
			SetAllGamesPurchased();
		}
		else if(productId.Contains("everything")) // Everything
		{
			SetAllBooksPurchased();
			SetAllMapsPurchased();
			SetAllGamesPurchased();
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
	private int[] m_maps;

	[SerializeField]
	private int[] m_defaultUnlockedBooks;
	[SerializeField]
	private int[] m_defaultUnlockedMaps;
	[SerializeField]
	private string[] m_defaultUnlockedGames;
	//[SerializeField]
	//private int[] m_defaultUnlockedGames;
	
	HashSet<int> m_boughtBooks = new HashSet<int>();
	HashSet<int> m_boughtMaps = new HashSet<int>();
	bool m_boughtGames = false;

	void Awake()
	{
		Debug.Log("BuyManager.Awake() - UP_TO_DATE_TEST_3");


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
		m_productListResolved = true;
	}

	void StoreKitManager_productListFailedEvent(string s)
	{
		Debug.Log("PRODUCTLIST: Failed");
		Debug.Log("Failed Message: " + s);

		m_productListResolved = true;
	}

	bool m_productListResolved = false;

	IEnumerator Start()
	{
		Debug.Log("PRODUCTLIST: Waiting for db");
		
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		Debug.Log("PRODUCTLIST: Building");
		
		List<string> productIdentifiers = new List<string>();	

		productIdentifiers.Add(m_gamesProductIdentifier);
		
		StoreKitManager.productListReceivedEvent += new Action<List<StoreKitProduct>>(StoreKitManager_productListReceivedEvent);
		StoreKitManager.productListRequestFailedEvent += new Action<string>(StoreKitManager_productListFailedEvent);
		
		Debug.Log("PRODUCTLIST: Requesting " + productIdentifiers.Count + " products");

		foreach(string productId in productIdentifiers)
		{
			Debug.Log(productId);
		}
		
		StoreKitBinding.requestProductData(productIdentifiers.ToArray());
		
		while(!m_productListResolved)
		{
			yield return null;
		}
		
		StoreKitManager.productListReceivedEvent -= new Action<List<StoreKitProduct>>(StoreKitManager_productListReceivedEvent);
		StoreKitManager.productListRequestFailedEvent -= new Action<string>(StoreKitManager_productListFailedEvent);
		
		Debug.Log("PRODUCTLIST: Finished");
	}

	/*
	IEnumerator Start()
	{
		Debug.Log("PRODUCTLIST: Waiting for db");

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		Debug.Log("PRODUCTLIST: Building");

		List<string> productIdentifiers = new List<string>();

		DataTable storyTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE publishable = 't'");
		foreach(DataRow story in storyTable.Rows)
		{
			productIdentifiers.Add(BuildStoryProductIdentifier(story));
		}

		foreach(int map in m_maps)
		{
			productIdentifiers.Add(BuildMapProductIdentifier(map));
		}

		productIdentifiers.Add(m_booksProductIdentifier);
		productIdentifiers.Add(m_mapsProductIdentifier);
		productIdentifiers.Add(m_gamesProductIdentifier);
		productIdentifiers.Add(m_everythingProductIdentifier);

		StoreKitManager.productListReceivedEvent += new Action<List<StoreKitProduct>>(StoreKitManager_productListReceivedEvent);
		StoreKitManager.productListRequestFailedEvent += new Action<string>(StoreKitManager_productListFailedEvent);

		Debug.Log("PRODUCTLIST: Requesting");

		StoreKitBinding.requestProductData(productIdentifiers.ToArray());

		while(!m_productListResolved)
		{
			yield return null;
		}

		StoreKitManager.productListReceivedEvent -= new Action<List<StoreKitProduct>>(StoreKitManager_productListReceivedEvent);
		StoreKitManager.productListRequestFailedEvent -= new Action<string>(StoreKitManager_productListFailedEvent);

		Debug.Log("PRODUCTLIST: Finished");
	}
	*/

	public string BuildStoryProductIdentifier(DataRow storyData)
	{
		string id = "story_" + storyData["id"].ToString() + "_" +
			storyData["title"].ToString().Replace(" ", "_").Replace("?", "_").Replace("!", "_").Replace("-", "_").ToLower();
		
		return id;
	}
	
	public string BuildMapProductIdentifier(int map) // TODO
	{
		return "default map identifier";
	}

	public bool IsBookBought(int bookId)
	{
		return m_boughtBooks.Contains(bookId) || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
	}

	public bool AreAllBooksBought()
	{
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
		FlurryBinding.logEvent("SetBookPurchased: " + bookId.ToString(), false);
#endif

		m_boughtBooks.Add(bookId);
		Save();
	}

	public void SetAllBooksPurchased()
	{
#if UNITY_IPHONE
		FlurryBinding.logEvent("SetAllBooksPurchased", false);
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
		return m_boughtMaps.Contains(mapId) || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
	}

	public bool AreAllMapsBought()
	{
		if(((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked)
		{
			return true;
		}
		else
		{
			bool allMapsBought = true;

			foreach(int map in m_maps)
			{
				if(!m_boughtMaps.Contains(map))
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
		FlurryBinding.logEvent("SetMapPurchased: " + mapId.ToString(), false);
#endif

		m_boughtMaps.Add(mapId);
		Save ();
	}

	public void SetAllMapsPurchased()
	{
#if UNITY_IPHONE
		FlurryBinding.logEvent("SetAllMapsPurchased", false);
#endif

		foreach(int map in m_maps)
		{
			m_boughtMaps.Add(map);
		}
		Save ();
	}

	public bool IsGameBought(string gameSceneName)
	{
		return m_boughtGames || Array.IndexOf(m_defaultUnlockedGames, gameSceneName) != -1 || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
	}

	public bool AreAllGamesBought()
	{
		return m_boughtGames || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
	}

	public void SetAllGamesPurchased()
	{
#if UNITY_IPHONE
		FlurryBinding.logEvent("SetAllGamesPurchased", false);
#endif

		m_boughtGames = true;
		Save ();
	}

	public bool IsEverythingBought()
	{
		if(((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked)
		{
			return true;
		}
		else
		{
			return AreAllBooksBought() && AreAllMapsBought() && AreAllGamesBought();
		}
	}

	void Load()
	{
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
