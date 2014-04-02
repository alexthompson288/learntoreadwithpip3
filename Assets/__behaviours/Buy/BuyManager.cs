using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public class BuyManager : Singleton<BuyManager> 
{
    [SerializeField]
	private bool m_logProductRequests = false;
	[SerializeField]
	private string m_booksProductIdentifier;
	[SerializeField]
    private string m_mapsProductIdentifier;
	[SerializeField]
    private string m_gamesProductIdentifier;
	[SerializeField]
    private string m_everythingProductIdentifier;
    [SerializeField]
    private string[] m_mapProductIdentifiers;
   

    [SerializeField]
    private Color m_buttonBuyable;
    public Color buyableColor
    {
        get
        {
            return m_buttonBuyable;
        }
    }
    
    [SerializeField]
    private Color m_buttonUnbuyable;
    public Color unbuyableColor
    {
        get
        {
            return m_buttonUnbuyable;
        }
    }


    int m_numBooks = 0;
    public int numBooks
    {
        get
        {
            return m_numBooks;
        }
    }
    
    public int numMaps
    {
        get
        {
            return m_mapProductIdentifiers.Length;
        }
    }


	public enum BuyType
	{
		Books,
		Maps,
		Games,
		Everything
	}
	
	BuyType m_buyType;


	bool m_purchaseIsResolved = false;
    bool m_productListResolved = false;

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

    void CelebratePurchase()
    {
        CharacterPopper popper = UnityEngine.Object.FindObjectOfType(typeof(CharacterPopper)) as CharacterPopper;
        if(popper != null)
        {
            popper.PopCharacter();
        }
        WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
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

    ////////////////////////////////////////////////////////////
    // Android

    IEnumerator Start()
    {
        yield return null;
    }

    IEnumerator AttemptPurchase()
    {
        yield return null;
    }

    public IEnumerator RestorePurchases(float restoreTime)
    {
        yield return null;
    }

    ////////////////////////////////////////////////////////////
    // iOS
#if UNITY_IPHONE
    IEnumerator Start()
    {
        if(m_logProductRequests)
            Debug.Log("PRODUCTLIST: Waiting for db");
        
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        
        if(m_logProductRequests)
            Debug.Log("PRODUCTLIST: Building");
        
        List<string> productIdentifiers = new List<string>();
        
        DataTable storyTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE publishable = 't'");
        foreach(DataRow story in storyTable.Rows)
        {
            productIdentifiers.Add(BuildStoryProductIdentifier(story));
        }
        
        m_numBooks = storyTable.Rows.Count;
        
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

	public IEnumerator RestorePurchases(float restoreTime)
	{
		//string receiptLocation = StoreKitBinding.getAppStoreReceiptLocation();

		Debug.Log("RestorePurchases - Opening processes");

		StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(StoreKitManager_restorePurchaseSuccessfulEvent);
		StoreKitManager.purchaseCancelledEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
		
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
	}

	void StoreKitManager_restorePurchaseSuccessfulEvent(StoreKitTransaction obj)
	{
		Debug.Log("restorePurchaseSuccessfulEvent: " + obj.productIdentifier);

		string productId = obj.productIdentifier;

        if (productId == m_booksProductIdentifier)
        {
            BuyInfo.Instance.SetAllBooksPurchased();
        } 
        else if (productId == m_mapsProductIdentifier)
        {
            BuyInfo.Instance.SetAllMapsPurchased();
        } 
        else if (productId == m_gamesProductIdentifier)
        {
            BuyInfo.Instance.SetAllGamesPurchased();
        } 
        else if (productId == m_everythingProductIdentifier)
        {
            BuyInfo.Instance.SetAllBooksPurchased();
            BuyInfo.Instance.SetAllMapsPurchased();
            BuyInfo.Instance.SetAllGamesPurchased();
        }
		else if(productId.Contains("stories")) // Book
		{
			// Find the story id
			string idNum = System.Text.RegularExpressions.Regex.Match(productId, @"\d+").Value;
			int bookId = Convert.ToInt32(idNum);

			BuyInfo.Instance.SetBookPurchased(bookId);
		}
		else if(productId.Contains("map")) // Map
		{
			// Find the map id
			//string idNum = System.Text.RegularExpressions.Regex.Match(productId, @"\d+").Value;
			int mapId = Array.IndexOf(m_mapProductIdentifiers, productId);

            Debug.Log("mapId: " + mapId);
			
			BuyInfo.Instance.SetMapPurchased(mapId);
		}
		else
		{
			Debug.LogError("Product Identifier not recognized: " + productId);
		}
	}
	
	void StoreKitManager_everythingPurchaseSuccessfulEvent(StoreKitTransaction obj)
	{
        Debug.Log("PURCHASE SUCCESS: EVERYTHING");

		if (obj.productIdentifier == m_productIdentifier)
		{
			m_purchaseIsResolved = true;
		} 

		FlurryBinding.logEvent("Purchasing Everything", false);

		BuyInfo.Instance.SetAllBooksPurchased();
		BuyInfo.Instance.SetAllMapsPurchased();
		BuyInfo.Instance.SetAllGamesPurchased();
		
		CelebratePurchase();
	}
	
	void StoreKitManager_gamesPurchaseSuccessfulEvent(StoreKitTransaction obj)
	{
		Debug.Log("PURCHASE SUCCESS: ALL GAMES");
		
		if (obj.productIdentifier == m_productIdentifier)
		{
			m_purchaseIsResolved = true;
		} 
		
		BuyInfo.Instance.SetAllGamesPurchased();
		
		CelebratePurchase();
	}
	
	void StoreKitManager_mapsPurchaseSuccessfulEvent(StoreKitTransaction obj)
	{
		Debug.Log("PURCHASE SUCCESS: ALL MAPS");
		
		if (obj.productIdentifier == m_productIdentifier)
		{
			m_purchaseIsResolved = true;
		} 
		
		BuyInfo.Instance.SetAllMapsPurchased();
		
		CelebratePurchase();
	}
	
	void StoreKitManager_booksPurchaseSuccessfulEvent(StoreKitTransaction obj)
	{
		Debug.Log("PURCHASE SUCCESS: ALL STORIES");
		
		if (obj.productIdentifier == m_productIdentifier)
		{
			m_purchaseIsResolved = true;
		} 
		
		BuyInfo.Instance.SetAllBooksPurchased();

		CelebratePurchase();
	}

	void StoreKitManager_purchaseCancelledEvent(string obj)
	{
		Debug.Log("PURCHASE CANCELLED - m_buyType: " + m_buyType.ToString());
		Debug.Log("Cancelled Message: " + obj);
		m_purchaseIsResolved = true;
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
#endif

#if UNITY_EDITOR
    void UnlockOnTimeOut()
    {
        Debug.Log("UNLOCKING ON TIMEOUT");
        switch(m_buyType)
        {
            case BuyType.Books:
                BuyInfo.Instance.SetAllBooksPurchased();
                break;
            case BuyType.Maps:
                BuyInfo.Instance.SetAllMapsPurchased();
                break;
            case BuyType.Games:
                BuyInfo.Instance.SetAllGamesPurchased();
                break;
            case BuyType.Everything:
                BuyInfo.Instance.SetAllBooksPurchased();
                BuyInfo.Instance.SetAllMapsPurchased();
                BuyInfo.Instance.SetAllGamesPurchased();
                break;
        }
    }
#endif
}
