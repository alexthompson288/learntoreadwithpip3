﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public class BuyManager : Singleton<BuyManager> 
{
    [SerializeField]
	private bool m_logProductRequests = false;
    [SerializeField]
    private string m_mathsModulesProductId;
    [SerializeField]
    private string m_readingModulesProductId;
    [SerializeField]
    private string m_allModulesProductId;
    [SerializeField]
    private string m_mathsGamesProductId;
    [SerializeField]
    private string m_readingGamesProductId;
    [SerializeField]
    private string m_allGamesProductId;

	bool m_purchaseIsResolved = false;
    bool m_productListResolved = false;

    bool m_isBuyingBundle;

    string m_currentProductId;

    string m_gamePrefix = "game_";
    string m_modulePrefix = "module_";

    public enum BundleType
    {
        MathsModules,
        ReadingModules,
        AllModules,
        MathsGames,
        ReadingGames,
        AllGames
    }

    string BuildModuleProductId(int moduleId)
    {
        return m_modulePrefix + moduleId.ToString();
    }

    public void BuyModule(int id)
    {
        m_currentProductId = BuildModuleProductId(id);

        ParentGate.Instance.Answered += OnParentGateAnswer;
        ParentGate.Instance.On();
    }

    string BuildGameProductId(string gameName)
    {
        return m_gamePrefix + gameName;
    }
    
    public void BuyGame(string gameName)
    {
        m_currentProductId = BuildGameProductId(gameName);
        
        ParentGate.Instance.Answered += OnParentGateAnswer;
        ParentGate.Instance.On();
    }

    public void BuyBundle(BundleType bundleType)
    {
        bool validType = true;
        switch (bundleType)
        {
            case BundleType.MathsModules:
                m_currentProductId = m_mathsModulesProductId;
                break;
            case BundleType.ReadingModules:
                m_currentProductId = m_readingModulesProductId;
                break;
            case BundleType.AllModules:
                m_currentProductId = m_allModulesProductId;
                break;
            case BundleType.MathsGames:
                m_currentProductId = m_mathsGamesProductId;
                break;
            case BundleType.ReadingGames:
                m_currentProductId = m_readingGamesProductId;
                break;
            case BundleType.AllGames:
                m_currentProductId = m_allGamesProductId;
                break;
            default:
                validType = false;
                break;
        }

        if (validType)
        {
            ParentGate.Instance.Answered += OnParentGateAnswer;
            ParentGate.Instance.On();
        }
    }
	
	public void OnParentGateAnswer(bool isCorrect)
	{
		ParentGate.Instance.Answered -= OnParentGateAnswer;
		
		if(isCorrect)
		{
			StartCoroutine(AttemptPurchase());
		}
	}


#if UNITY_IPHONE
    IEnumerator Start()
    {
        yield return null;

        if(m_logProductRequests)
            D.Log("PRODUCTLIST: Waiting for db");
        
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        StoreKitManager.productListReceivedEvent += new Action<List<StoreKitProduct>>(StoreKitManager_productListReceivedEvent);
        StoreKitManager.productListRequestFailedEvent += new Action<string>(StoreKitManager_productListFailedEvent);


        if(m_logProductRequests)
            D.Log("PRODUCTLIST: Building");

        string[] productIdentifiers = FindAllProductIdentifiers();


        if(m_logProductRequests)
            D.Log("PRODUCTLIST: Requesting");

        StoreKitBinding.requestProductData(productIdentifiers);
        
        while(!m_productListResolved)
        {
            yield return null;
        }
        
        StoreKitManager.productListReceivedEvent -= new Action<List<StoreKitProduct>>(StoreKitManager_productListReceivedEvent);
        StoreKitManager.productListRequestFailedEvent -= new Action<string>(StoreKitManager_productListFailedEvent);
        
        if(m_logProductRequests)
            D.Log("PRODUCTLIST: Finished");

    }

    // TODO
    string[] FindAllProductIdentifiers()
    {
        List<string> productIds = new List<string>();

        productIds.Add(m_mathsGamesProductId);
        productIds.Add(m_readingGamesProductId);
        productIds.Add(m_allGamesProductId);

        productIds.Add(m_mathsModulesProductId);
        productIds.Add(m_readingModulesProductId);
        productIds.Add(m_allModulesProductId);

        string[] mathsGames = ProgrammeInfo.GetPlusMathsGames();
        foreach (string game in mathsGames)
        {
            productIds.Add(BuildGameProductId(game));
        }

        string[] readingGames = ProgrammeInfo.GetPlusReadingGames();
        foreach (string game in readingGames)
        {
            productIds.Add(BuildGameProductId(game));
        }

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programmodules WHERE programmename='" + ProgrammeInfo.basicMaths + "'");
        foreach (DataRow module in dt.Rows)
        {
            productIds.Add(BuildModuleProductId(module.GetId()));
        }

        dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programmodules WHERE programmename='" + ProgrammeInfo.basicReading + "'");
        foreach (DataRow module in dt.Rows)
        {
            productIds.Add(BuildModuleProductId(module.GetId()));
        }

        return null;
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
			pcTimeOut += Time.deltaTime;
			#if UNITY_EDITOR
			if (pcTimeOut > 3.0f)
			{
				D.Log("PURCHASE TIMED OUT");
				UnlockOnTimeOut();
				m_purchaseIsResolved = true;
			}
			#endif
			yield return null;
		}
		
        NGUIHelpers.EnableUICams(false);
                		
		StoreKitManager.purchaseCancelledEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
		StoreKitManager.purchaseFailedEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseSuccessfulEvent -= new Action<StoreKitTransaction>(StoreKitManager_purchaseSuccessfulEvent);
	}

    void StoreKitManager_purchaseSuccessfulEvent(StoreKitTransaction obj)
    {
        D.Log("purchaseSuccessfulEvent: " + obj.productIdentifier);
        UnlockProduct(obj.productIdentifier);
        m_purchaseIsResolved = true;
    }

	void StoreKitManager_purchaseCancelledEvent(string obj)
	{
		D.Log("PURCHASE CANCELLED - " + m_currentProductId);
		D.Log("Cancelled Message: " + obj);
		m_purchaseIsResolved = true;
	}

    public IEnumerator RestorePurchases(float restoreTime)
    {
        D.Log("RestorePurchases - Opening processes");
        
        StoreKitManager.purchaseSuccessfulEvent += new Action<StoreKitTransaction>(StoreKitManager_restorePurchaseSuccessfulEvent);
        StoreKitManager.purchaseCancelledEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseFailedEvent += new Action<string>(StoreKitManager_purchaseCancelledEvent);
        
        NGUIHelpers.EnableUICams(false);
        
        // Restore
        D.Log("RestorePurchases - Calling restoreCompletedTransactions");
        StoreKitBinding.restoreCompletedTransactions();
        
        D.Log("RestorePurchases - Waiting for " + restoreTime);
        yield return new WaitForSeconds(restoreTime);
        
        D.Log("RestorePurchases - Closing processes");
        
        NGUIHelpers.EnableUICams(false);
        
        StoreKitManager.purchaseSuccessfulEvent -= new Action<StoreKitTransaction>(StoreKitManager_restorePurchaseSuccessfulEvent);
        StoreKitManager.purchaseCancelledEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
        StoreKitManager.purchaseFailedEvent -= new Action<string>(StoreKitManager_purchaseCancelledEvent);
    }

    void StoreKitManager_restorePurchaseSuccessfulEvent(StoreKitTransaction obj)
    {
        D.Log("restorePurchaseSuccessfulEvent: " + obj.productIdentifier);
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
        BuyInfo.Instance.SetGamesPurchased(mathsGames);
    }

    void UnlockReadingGames()
    {
        string[] readingGames = ProgrammeInfo.GetPlusReadingGames();
        BuyInfo.Instance.SetGamesPurchased(readingGames);
    }

    void UnlockMathsModules()
    {
        UnlockProgramModules(ProgrammeInfo.basicMaths);
    }

    void UnlockReadingModules()
    {
        UnlockProgramModules(ProgrammeInfo.basicMaths);
    }

    void UnlockProgramModules(string programmename)
    {
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programmodules WHERE programmename='" + programmename + "'");
        if (dt.Rows.Count > 0)
        {
            int[] moduleIds = new int[dt.Rows.Count];
            
            for(int i = 0; i < moduleIds.Length; ++i)
            {
                moduleIds[i] = dt.Rows[i].GetId();
            }
            
            BuyInfo.Instance.SetModulesPurchased(moduleIds);
        }
    }

    void UnlockProduct(string productId)
    {
        if (productId == m_mathsModulesProductId)
        {
            UnlockMathsModules();
        } 
        else if (productId == m_readingModulesProductId)
        {
            UnlockReadingModules();
        } 
        else if (productId == m_allModulesProductId)
        {
            UnlockMathsModules();
            UnlockReadingModules();
        } 
        else if (productId == m_mathsGamesProductId)
        {
            UnlockMathsGames();
        } 
        else if (productId == m_readingGamesProductId)
        {
            UnlockReadingGames();
        }
        else if (productId == m_allGamesProductId)
        {
            UnlockMathsGames();
            UnlockReadingGames();
        }
        else if(productId.Contains(m_modulePrefix))
        {
            // Find the module id
            string idNum = System.Text.RegularExpressions.Regex.Match(productId, @"\d+").Value;
            int bookId = Convert.ToInt32(idNum);
            
            BuyInfo.Instance.SetModulePurchased(bookId);
        }
        else if(productId.Contains(m_gamePrefix))
        {
            BuyInfo.Instance.SetGamePurchased(productId.Replace(m_gamePrefix, ""));
        }
        else
        {
            D.LogError("Product Identifier not recognized: " + productId);
        }
    }
}
