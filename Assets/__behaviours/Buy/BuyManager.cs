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
    private string m_pipisodesProductIdentifier;
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
        Pipisodes,
		Everything
	}

	bool m_purchaseIsResolved = false;
    bool m_productListResolved = false;

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

    public void BuyPipisode(DataRow pipisodeData)
    {
        m_productIdentifier = BuildPipisodeProductIdentifier(pipisodeData);

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
            case BuyType.Pipisodes:
                m_productIdentifier = m_pipisodesProductIdentifier;
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
			//StartCoroutine(AttemptPurchase());
		}
	}

    [SerializeField]
    public string m_androidPublicKey;


#if UNITY_IPHONE

    /*
    IEnumerator Start()
    {
        yield return null;

        if(m_logProductRequests)
            //////D.Log("PRODUCTLIST: Waiting for db");
        
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        StoreKitManager.productListReceivedEvent += new Action<List<StoreKitProduct>>(StoreKitManager_productListReceivedEvent);
        StoreKitManager.productListRequestFailedEvent += new Action<string>(StoreKitManager_productListFailedEvent);


        if(m_logProductRequests)
            //////D.Log("PRODUCTLIST: Building");

        string[] productIdentifiers = FindAllProductIdentifiers();


        if(m_logProductRequests)
            //////D.Log("PRODUCTLIST: Requesting");

        StoreKitBinding.requestProductData(productIdentifiers);
        
        while(!m_productListResolved)
        {
            yield return null;
        }
        
        StoreKitManager.productListReceivedEvent -= new Action<List<StoreKitProduct>>(StoreKitManager_productListReceivedEvent);
        StoreKitManager.productListRequestFailedEvent -= new Action<string>(StoreKitManager_productListFailedEvent);
        
        if(m_logProductRequests)
            //////D.Log("PRODUCTLIST: Finished");

    }

    void StoreKitManager_productListReceivedEvent(List<StoreKitProduct> productList)
    {
        //////D.Log("PRODUCTLIST: Received " + productList.Count);
        foreach(StoreKitProduct product in productList)
        {
            //////D.Log(product.productIdentifier);
        }
        
        m_productListResolved = true;
    }
    
    void StoreKitManager_productListFailedEvent(string s)
    {
        //////D.Log("PRODUCTLIST: Failed");
        //////D.Log("Failed Message: " + s);
        
        m_productListResolved = true;
    }

	IEnumerator AttemptPurchase()
	{
		//////D.Log("BuyManager.AttemptPurchase()");
		StoreKitManager.purchaseCancelledEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
		
		//////D.Log("Attempting purchase on " + m_productIdentifier);
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
				//////D.Log("PURCHASE TIMED OUT");
				UnlockOnTimeOut();
				m_purchaseIsResolved = true;
			}
			#endif
			yield return null;
		}
		
        NGUIHelpers.EnableUICams(false);

        RefreshAll();

        // TODO: Call RefreshBuyButton on BuyCoordinator
		
		StoreKitManager.purchaseCancelledEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseSuccessfulEvent -= new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
	}

    void StoreKitManager_purchaseSuccessfulEvent(StoreKitTransaction obj)
    {
        //////D.Log("purchaseSuccessfulEvent: " + obj.productIdentifier);
        
        UnlockProduct(obj.productIdentifier);
        
        m_purchaseIsResolved = true;
    }

	void StoreKitManager_purchaseCancelledEvent(string obj)
	{
		//////D.Log("PURCHASE CANCELLED - " + m_productIdentifier);
		//////D.Log("Cancelled Message: " + obj);
		m_purchaseIsResolved = true;
	}

    public IEnumerator RestorePurchases(float restoreTime)
    {
        //////D.Log("RestorePurchases - Opening processes");
        
        StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(StoreKitManager_restorePurchaseSuccessfulEvent);
        StoreKitManager.purchaseCancelledEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseFailedEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
        
        NGUIHelpers.EnableUICams(false);
        
        // Restore
        //////D.Log("RestorePurchases - Calling restoreCompletedTransactions");
        StoreKitBinding.restoreCompletedTransactions();
        
        //////D.Log("RestorePurchases - Waiting for " + restoreTime);
        yield return new WaitForSeconds(restoreTime);
        
        //////D.Log("RestorePurchases - Closing processes");
        
        NGUIHelpers.EnableUICams(false);
        
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
        //////D.Log("restorePurchaseSuccessfulEvent: " + obj.productIdentifier);
        
        UnlockProduct(obj.productIdentifier);
    }
    */
#else
    IEnumerator Start()
    {
        yield break;
    }
    
    IEnumerator AttemptPurchase()
    {
        yield break;
    }
    
    public IEnumerator RestorePurchases(float restoreTime)
    {
        yield break;
    }
    /*
    bool m_billingSupported = false;
    bool m_billingSupportedResolved = false;

#if !UNITY_EDITOR
    void OnDestroy()
    {
        //GoogleIAB.unbindService();
    }
#endif

    IEnumerator Start()
    {
        GoogleIABManager.billingSupportedEvent += GoogleIAB_billingSupportedEvent;
        GoogleIABManager.billingNotSupportedEvent += GoogleIAB_billingNotSupportedEvent;

        GoogleIAB.init(m_androidPublicKey);

        if(m_logProductRequests)
            //////D.Log("QUERYINVENTORY: Waiting for db");
        
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        while (!m_billingSupportedResolved)
        {
            yield return null;
        }
  
        
        GoogleIABManager.queryInventorySucceededEvent += new Action<List<GooglePurchase>, List<GoogleSkuInfo>>(GoogleIAB_queryInventorySucceededEvent);
        GoogleIABManager.queryInventoryFailedEvent += new Action<string>(GoogleIAB_queryInventoryFailedEvent);


        if(m_logProductRequests)
            //////D.Log("QUERYINVENTORY: Building");

        string[] productIdentifiers = FindAllProductIdentifiers();


        if(m_logProductRequests)
            //////D.Log("QUERYINVENTORY: Requesting");

        GoogleIAB.queryInventory(productIdentifiers);

        
        while(!m_productListResolved)
        {
            yield return null;
        }
        
        GoogleIABManager.queryInventorySucceededEvent -= new Action<List<GooglePurchase>, List<GoogleSkuInfo>>(GoogleIAB_queryInventorySucceededEvent);
        GoogleIABManager.queryInventoryFailedEvent -= new Action<string>(GoogleIAB_queryInventoryFailedEvent);
        
        if(m_logProductRequests)
            //////D.Log("QUERYINVENTORY: Finished");
    }

    void GoogleIAB_billingSupportedEvent()
    {
        m_billingSupported = true;
        ResolveBillingSupport();
    }

    void GoogleIAB_billingNotSupportedEvent(string message)
    {
        //////D.Log("Billing Unsupported: " + message);

        m_billingSupported = false;
        ResolveBillingSupport();
    }

    void ResolveBillingSupport()
    {
        m_billingSupportedResolved = true;
        
        GoogleIABManager.billingSupportedEvent -= GoogleIAB_billingSupportedEvent;
        GoogleIABManager.billingNotSupportedEvent -= GoogleIAB_billingNotSupportedEvent;
    }

    void GoogleIAB_queryInventorySucceededEvent(List<GooglePurchase> purchaseList, List<GoogleSkuInfo> infoList)
    {
        //////D.Log("QUERYINVENTORY: SUCCEEDED - " + purchaseList.Count);

        foreach (GooglePurchase purchase in purchaseList)
        {
            //////D.Log(purchase.productId);
        }

        m_productListResolved = true;
    }

    void GoogleIAB_queryInventoryFailedEvent(string message)
    {
        //////D.Log("QUERYINVENTORY: FAILED - " + message);

        m_productListResolved = true;
    }

    IEnumerator AttemptPurchase()
    {
        //////D.Log("BuyManager.AttemptPurchase() - supported: " + m_billingSupported);

        yield return null;
        if (m_billingSupported)
        {
            GoogleIABManager.purchaseSucceededEvent += new Action<GooglePurchase>(GoogleIAB_purchaseSuccessfulEvent);
            GoogleIABManager.purchaseFailedEvent += new Action<string>(GoogleIAB_purchaseFailedEvent);

            m_purchaseIsResolved = false;

            //////D.Log("Attempting purchase on " + m_productIdentifier);

            GoogleIAB.purchaseProduct(m_productIdentifier);

            NGUIHelpers.EnableUICams(false);
            
            float pcTimeOut = 0;
            while (!m_purchaseIsResolved)
            {
                pcTimeOut += Time.deltaTime;
                #if UNITY_EDITOR
                if (pcTimeOut > 3.0f)
                {
                    //////D.Log("PURCHASE TIMED OUT");
                    UnlockOnTimeOut();
                    m_purchaseIsResolved = true;
                }
                #endif
                yield return null;
            }
            
            NGUIHelpers.EnableUICams(false);
            
            RefreshAll();
            
            // TODO: Call RefreshBuyButton on BuyCoordinator

            GoogleIABManager.purchaseSucceededEvent -= new Action<GooglePurchase>(GoogleIAB_purchaseSuccessfulEvent);
            GoogleIABManager.purchaseFailedEvent -= new Action<string>(GoogleIAB_purchaseFailedEvent);
        }
    }

    void GoogleIAB_purchaseSuccessfulEvent(GooglePurchase obj)
    {
        //////D.Log("purchaseSuccessfulEvent: " + obj.productId);
        
        UnlockProduct(obj.productId);
        
        m_purchaseIsResolved = true;
    }

    void GoogleIAB_purchaseFailedEvent(string message)
    {
        //////D.Log("PURCHASE FAILED: " + message);

        m_purchaseIsResolved = true;
    }

    public IEnumerator RestorePurchases(float restoreTime)
    {
        yield return null;
        if (m_billingSupported)
        {
        }
    }
    */
#endif

#if UNITY_EDITOR
    void UnlockOnTimeOut()
    {
        //////D.Log("UNLOCKING ON TIMEOUT");
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

    void UnlockProduct(string productId)
    {
        if (productId == m_booksProductIdentifier)
        {
            BuyInfo.Instance.SetAllBooksPurchased();
        } else if (productId == m_mapsProductIdentifier)
        {
            BuyInfo.Instance.SetAllMapsPurchased();
        } else if (productId == m_gamesProductIdentifier)
        {
            BuyInfo.Instance.SetAllGamesPurchased();
        } else if (productId == m_everythingProductIdentifier)
        {
            BuyInfo.Instance.SetAllBooksPurchased();
            BuyInfo.Instance.SetAllMapsPurchased();
            BuyInfo.Instance.SetAllGamesPurchased();
            BuyInfo.Instance.SetAllPipisodesPurchased();
        } 
        else if (productId == m_pipisodesProductIdentifier)
        {
            // TODO
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
            
            //////D.Log("mapId: " + mapId);
            
            BuyInfo.Instance.SetMapPurchased(mapId);
        }
        else
        {
            //////D.LogError("Product Identifier not recognized: " + productId);
        }
    }

    string[] FindAllProductIdentifiers()
    {
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

        return productIdentifiers.ToArray();
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

    void RefreshAll()
    {
        RefreshBuyAllButtons();
        RefreshBooks();
        RefreshMaps();
        RefreshGames();
    }
    
    void RefreshBooks()
    {
        /*
        NewStoryBrowserBookButton[] books = UnityEngine.Object.FindObjectsOfType(typeof(NewStoryBrowserBookButton)) as NewStoryBrowserBookButton[];
        foreach(NewStoryBrowserBookButton book in books)
        {
            book.Refresh();
        }
        */
    }

    // TODO
    void RefreshMaps()
    {

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

    // TODO
    public string BuildPipisodeProductIdentifier(DataRow pipisodeData)
    {
        return "placeholder_pipisode_product_identifier";
    }
}
