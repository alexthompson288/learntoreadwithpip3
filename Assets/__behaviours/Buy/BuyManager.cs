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

	bool m_purchaseIsResolved = false;


	string m_productIdentifier;

    bool m_isBuyingBundle;

    public void BuyMap(int id)
    {
        m_productIdentifier = BuildMapProductIdentifier(id);

        ParentGate.Instance.OnParentGateAnswer += OnParentGateAnswer;
        ParentGate.Instance.On();
    }

    public void BuyStory(DataRow storyData)
    {
        m_productIdentifier = BuildStoryProductIdentifier(storyData);

        ParentGate.Instance.OnParentGateAnswer += OnParentGateAnswer;
        ParentGate.Instance.On();
    }

	public void Buy(BuyType buyType)
	{
		switch (buyType)
        {
            case BuyType.Books:
                m_productIdentifier = m_booksProductIdentifier;
                break;
            case BuyType.Everything:
                m_productIdentifier = m_everythingProductIdentifier;
                break;
            case BuyType.Games:
                m_productIdentifier = m_gamesProductIdentifier;
                break;
            case BuyType.Maps:
                m_productIdentifier = m_mapsProductIdentifier;
                break;
        }
		
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

    [SerializeField]
    public string m_androidPublicKey;

#if UNITY_ANDROID
    bool m_billingSupported = false;

    IEnumerator Start()
    {
        yield return null;

        GoogleIABManager.billingSupportedEvent += GoogleIAB_billingSupportedEvent;
        GoogleIABManager.billingNotSupportedEvent += GoogleIAB_billingNotSupportedEvent;

        GoogleIAB.init(m_androidPublicKey);


    }

    void GoogleIAB_billingSupportedEvent()
    {
        m_billingSupported = true;

        GoogleIABManager.billingSupportedEvent -= GoogleIAB_billingSupportedEvent;
        GoogleIABManager.billingNotSupportedEvent -= GoogleIAB_billingNotSupportedEvent;
    }

    void GoogleIAB_billingNotSupportedEvent(string message)
    {
        Debug.Log("Billing Unsupported: " + message);

        m_billingSupported = false;

        GoogleIABManager.billingSupportedEvent -= GoogleIAB_billingSupportedEvent;
        GoogleIABManager.billingNotSupportedEvent -= GoogleIAB_billingNotSupportedEvent;
    }

    IEnumerator AttemptPurchase()
    {
        yield return null;
        if (m_billingSupported)
        {
        }
    }

    public IEnumerator RestorePurchases(float restoreTime)
    {
        yield return null;
        if (m_billingSupported)
        {
        }
    }
#elif UNITY_IPHONE
    bool m_productListResolved = false;

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
        StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
		
		Debug.Log("Attempting purchase on " + m_productIdentifier);
		m_purchaseIsResolved = false;
		StoreKitBinding.purchaseProduct(m_productIdentifier, 1);
		
        NGUIHelpers.EnableUICams(false);
		
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
		
        NGUIHelpers.EnableUICams(false);

		RefreshBuyAllButtons();
		RefreshBooks();
		RefreshMaps();
		RefreshGames();

        // TODO: Call RefreshBuyButton on BuyCoordinator
		
		StoreKitManager.purchaseCancelledEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseSuccessfulEvent -= new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
	}

    void StoreKitManager_purchaseSuccessfulEvent(StoreKitTransaction obj)
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
            int mapId = Array.IndexOf(m_mapProductIdentifiers, productId);
            
            Debug.Log("mapId: " + mapId);
            
            BuyInfo.Instance.SetMapPurchased(mapId);
        }
        else
        {
            Debug.LogError("Product Identifier not recognized: " + productId);
        }

        m_purchaseIsResolved = true;
    }

	void StoreKitManager_purchaseCancelledEvent(string obj)
	{
		Debug.Log("PURCHASE CANCELLED - " + m_productIdentifier);
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

    public IEnumerator RestorePurchases(float restoreTime)
    {
        Debug.Log("RestorePurchases - Opening processes");
        
        StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
        StoreKitManager.purchaseCancelledEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseFailedEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
        
        NGUIHelpers.EnableUICams(false);
        
        // Restore
        Debug.Log("RestorePurchases - Calling restoreCompletedTransactions");
        StoreKitBinding.restoreCompletedTransactions();
        
        Debug.Log("RestorePurchases - Waiting for " + restoreTime);
        yield return new WaitForSeconds(restoreTime);
        
        Debug.Log("RestorePurchases - Closing processes");
        
        NGUIHelpers.EnableUICams(false);
        
        RefreshBuyAllButtons();
        RefreshBooks();
        RefreshMaps();
        RefreshGames();
        
        StoreKitManager.purchaseSuccessfulEvent -= new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
        StoreKitManager.purchaseCancelledEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseFailedEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
    }
#endif

#if UNITY_EDITOR
    void UnlockOnTimeOut()
    {
        Debug.Log("UNLOCKING ON TIMEOUT");
        /*
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
        */
    }
#endif

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
}
