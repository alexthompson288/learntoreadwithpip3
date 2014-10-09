﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public class BuyManager : Singleton<BuyManager> 
{
    public delegate void PurchaseEventHandler(bool success);
    public event PurchaseEventHandler Resolved;

    [SerializeField]
	private bool m_logProductRequests = false;
    [SerializeField]
    private string m_allColorsProductId;
    [SerializeField]
    private string m_allGamesProductId;

	bool m_purchaseIsResolved = false;
    bool m_productListResolved = false;

    bool m_isBuyingBundle;

    string m_currentProductId;

    string m_gamePrefix = "plusgame_";
    string m_colorPrefix = "color_";

    List<ColorInfo.PipColor> GetAllColors()
    {
        List<ColorInfo.PipColor> pipColors = new List<ColorInfo.PipColor>();
        pipColors.Add(ColorInfo.PipColor.Pink);
        pipColors.Add(ColorInfo.PipColor.Red);
        pipColors.Add(ColorInfo.PipColor.Yellow);
        pipColors.Add(ColorInfo.PipColor.Blue);
        pipColors.Add(ColorInfo.PipColor.Green);
        pipColors.Add(ColorInfo.PipColor.Orange);

        return pipColors;
    }

#if UNITY_IPHONE
    string BuildColorProductId(ColorInfo.PipColor pipColor)
    {
        return m_colorPrefix + pipColor.ToString();
    }

    public void BuyColor(ColorInfo.PipColor pipColor)
    {
        D.Log(string.Format("BuyManager.BuyColor({0})", pipColor));
        m_currentProductId = BuildColorProductId(pipColor);

        ParentGate.Instance.Answered += OnParentGateAnswer;
        ParentGate.Instance.On();
    }

    string BuildGameProductId(int gameId)
    {
        return m_gamePrefix + gameId;
    }
    
    public void BuyGame(int gameId)
    {
        D.Log("BuyManager.BuyGame(" + gameId + ")");
        m_currentProductId = BuildGameProductId(gameId);
        
        ParentGate.Instance.Answered += OnParentGateAnswer;
        ParentGate.Instance.Dismissed += OnParentGateDismiss;
        ParentGate.Instance.On();
    }

    public void BuyBundle(bool isColorBundle)
    {
        m_currentProductId = isColorBundle ? m_allColorsProductId : m_allGamesProductId;

        ParentGate.Instance.Answered += OnParentGateAnswer;
        ParentGate.Instance.Dismissed += OnParentGateDismiss;
        ParentGate.Instance.On();
    }
	
	void OnParentGateAnswer(bool isCorrect)
	{
        D.Log("BuyManager.OnParentGateAnswer()");
		ParentGate.Instance.Answered -= OnParentGateAnswer;
        ParentGate.Instance.Dismissed -= OnParentGateDismiss;

		if(isCorrect)
		{
			StartCoroutine(AttemptPurchase());
		}
	}

    void OnParentGateDismiss()
    {
        D.Log("BuyManager.OnParentGateDismiss()");
        ParentGate.Instance.Answered -= OnParentGateAnswer;
        ParentGate.Instance.Dismissed -= OnParentGateDismiss;
    }


    IEnumerator Start()
    {
        D.Log("BuyManager.Start()");

        if (ContentLock.Instance.lockType == ContentLock.Lock.Buy)
        {
            if (m_logProductRequests)
                D.Log("PRODUCTLIST: Waiting set programme");

            yield return StartCoroutine(GameManager.WaitForSetProgramme());

            if (m_logProductRequests)
                D.Log("PRODUCTLIST: Waiting for db");
            
            yield return StartCoroutine(GameDataBridge.WaitForDatabase());

            StoreKitManager.productListReceivedEvent += new Action<List<StoreKitProduct>>(StoreKitManager_productListReceivedEvent);
            StoreKitManager.productListRequestFailedEvent += new Action<string>(StoreKitManager_productListFailedEvent);


            if (m_logProductRequests)
                D.Log("PRODUCTLIST: Building");

            string[] productIdentifiers = FindAllProductIdentifiers();


            if (m_logProductRequests)
                D.Log("PRODUCTLIST: Requesting");

            StoreKitBinding.requestProductData(productIdentifiers);
            
            while (!m_productListResolved)
            {
                yield return null;
            }
            
            StoreKitManager.productListReceivedEvent -= new Action<List<StoreKitProduct>>(StoreKitManager_productListReceivedEvent);
            StoreKitManager.productListRequestFailedEvent -= new Action<string>(StoreKitManager_productListFailedEvent);
            
            if (m_logProductRequests)
                D.Log("PRODUCTLIST: Finished");
        }
        else
        {
            yield return null;
        }
    }

    string[] FindAllProductIdentifiers()
    {
        List<string> productIds = new List<string>();

        if (GameManager.Instance.programme == ProgrammeInfo.plusReading || GameManager.Instance.programme == ProgrammeInfo.plusMaths)
        {
            productIds.Add(m_allGamesProductId);

            SqliteDatabase db = GameDataBridge.Instance.GetDatabase();
            for(int i = 0; i < 2; ++i)
            {
                string[] games = i == 0 ? ProgrammeInfo.GetPlusMathsGames() : ProgrammeInfo.GetPlusReadingGames();

                foreach (string game in games)
                {
                    DataTable dt = db.ExecuteQuery("select * from games WHERE name='" + game + "'");
                    
                    if(dt.Rows.Count > 0)
                    {
                        productIds.Add(BuildGameProductId(dt.Rows[0].GetId()));
                    }
                }
            }
        } 
        else
        {
            productIds.Add(m_allColorsProductId);

            List<ColorInfo.PipColor> pipColors = GetAllColors();
            foreach(ColorInfo.PipColor pipColor in pipColors)
            {
                productIds.Add(BuildColorProductId(pipColor));
            }
        }

        return productIds.ToArray();
    }

    void StoreKitManager_productListReceivedEvent(List<StoreKitProduct> productList)
    {
        D.Log("PRODUCTLIST: Received " + productList.Count);
        foreach(StoreKitProduct product in productList)
        {
            D.Log(product.productIdentifier);
        }
        
        m_productListResolved = true;
    }
    
    void StoreKitManager_productListFailedEvent(string s)
    {
        D.Log("PRODUCTLIST: Failed");
        D.Log("Failed Message: " + s);
        
        m_productListResolved = true;
    }

	IEnumerator AttemptPurchase()
	{
		D.Log("BuyManager.AttemptPurchase()");
		StoreKitManager.purchaseCancelledEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
		
		D.Log("Attempting purchase on " + m_currentProductId);
		m_purchaseIsResolved = false;
		StoreKitBinding.purchaseProduct(m_currentProductId, 1);
		
        NGUIHelpers.EnableUICams(false);
		
		float pcTimeOut = 0;
		while (!m_purchaseIsResolved)
		{
#if UNITY_EDITOR
			pcTimeOut += Time.deltaTime;
			
			if (pcTimeOut > 3.0f)
			{
				D.Log("PURCHASE TIMED OUT");
				UnlockOnTimeOut();
				m_purchaseIsResolved = true;
			}
#endif
			yield return null;
		}
		
        NGUIHelpers.EnableUICams(true);
                		
		StoreKitManager.purchaseCancelledEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseSuccessfulEvent -= new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
	}

    void StoreKitManager_purchaseSuccessfulEvent(StoreKitTransaction obj)
    {
        D.Log("purchaseSuccessfulEvent: " + obj.productIdentifier);
        PipGoogleAnalytics.Instance.Purchased(obj.productIdentifier);
        UnlockProduct(obj.productIdentifier);
        m_purchaseIsResolved = true;

        if (Resolved != null)
        {
            Resolved(true);
        }
    }

	void StoreKitManager_purchaseCancelledEvent(string obj)
	{
		D.Log("PURCHASE CANCELLED - " + m_currentProductId);
		D.Log("Cancelled Message: " + obj);
        PipGoogleAnalytics.Instance.PurchaseCancelled(m_currentProductId);
		m_purchaseIsResolved = true;

        if (Resolved != null)
        {
            Resolved(false);
        }
	}

    bool m_restoreWasSuccess = false;

    public IEnumerator RestorePurchases(TweenBehaviour restoringMoveable, UILabel countdownLabel)
    {
        D.Log("RestorePurchases - Opening processes");
        
        StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(StoreKitManager_restorePurchaseSuccessfulEvent);
        
        NGUIHelpers.EnableUICams(false);
        
        // Restore
        m_restoreWasSuccess = false;

        D.Log("RestorePurchases - Calling restoreCompletedTransactions");
        StoreKitBinding.restoreCompletedTransactions();

#if UNITY_EDITOR
        float restoreTime = 3;
#else
        float restoreTime = 30;
#endif

        D.Log("RestorePurchases - Waiting for " + restoreTime);

        restoringMoveable.On();

        float timeRemaining = restoreTime;
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            countdownLabel.text = Mathf.RoundToInt(timeRemaining).ToString();
            yield return null;
        }

        restoringMoveable.Off();

        if (Resolved != null)
        {
            Resolved(m_restoreWasSuccess);
        }
        
        D.Log("RestorePurchases - Closing processes");
        
        NGUIHelpers.EnableUICams(true);
        
        StoreKitManager.purchaseSuccessfulEvent -= new Action<StoreKitTransaction>(StoreKitManager_restorePurchaseSuccessfulEvent);
    }

    void StoreKitManager_restorePurchaseSuccessfulEvent(StoreKitTransaction obj)
    {
        D.Log("restorePurchaseSuccessfulEvent: " + obj.productIdentifier);
        m_restoreWasSuccess = true;
        UnlockProduct(obj.productIdentifier);
    }
#endif

#if UNITY_EDITOR
    void UnlockOnTimeOut()
    {
        D.Log("UNLOCKING ON TIMEOUT");
    }
#endif

    void UnlockMathsGames()
    {
        string[] mathsGames = ProgrammeInfo.GetPlusMathsGames();

        List<int> ids = new List<int>();

        SqliteDatabase db = GameDataBridge.Instance.GetDatabase();

        foreach (string game in mathsGames)
        {
            DataTable dt = db.ExecuteQuery("select * from games WHERE name='" + game + "'");
            if(dt.Rows.Count > 0)
            {
                ids.Add(dt.Rows[0].GetId());
            }
        }

        BuyInfo.Instance.SetGamesPurchased(ids.ToArray());
    }

    void UnlockReadingGames()
    {
        string[] readingGames = ProgrammeInfo.GetPlusReadingGames();

        List<int> ids = new List<int>();
        
        SqliteDatabase db = GameDataBridge.Instance.GetDatabase();
        
        foreach (string game in readingGames)
        {
            DataTable dt = db.ExecuteQuery("select * from games WHERE name='" + game + "'");
            if(dt.Rows.Count > 0)
            {
                ids.Add(dt.Rows[0].GetId());
            }
        }
        
        BuyInfo.Instance.SetGamesPurchased(ids.ToArray());
    }

    void UnlockAllColors()
    {
        List<ColorInfo.PipColor> pipColors = GetAllColors();

        List<string> colors = new List<string>();
        foreach (ColorInfo.PipColor pipColor in pipColors)
        {
            colors.Add(pipColor.ToString());
        }

        BuyInfo.Instance.SetColorsPurchased(colors);
    }

    void UnlockProduct(string productId)
    {
        if (productId == m_allGamesProductId)
        {
            UnlockAllColors();
        } 
        else if (productId == m_allGamesProductId)
        {
            UnlockMathsGames();
            UnlockReadingGames();
        }
        else if(productId.Contains(m_colorPrefix))
        {
            BuyInfo.Instance.SetColorPurchased(productId.Replace(m_colorPrefix, ""));
        }
        else if(productId.Contains(m_gamePrefix))
        {
            // Find the game id
            string idNum = System.Text.RegularExpressions.Regex.Match(productId, @"\d+").Value;
            int id = Convert.ToInt32(idNum);
            
            BuyInfo.Instance.SetGamePurchased(id);
        }
        else
        {
            D.LogError("Product Identifier not recognized: " + productId);
        }
    }
}
