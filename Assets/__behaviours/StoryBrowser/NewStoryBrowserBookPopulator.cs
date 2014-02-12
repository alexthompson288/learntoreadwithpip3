using UnityEngine;
using System.Collections;
using Wingrove;
using System.Collections.Generic;
//using System;

public class NewStoryBrowserBookPopulator : Singleton<NewStoryBrowserBookPopulator> 
{
    [SerializeField]
    private GameObject m_bookPrefab;
    [SerializeField]
    private UIGrid m_grid;
	[SerializeField]
	private List<int> m_unlockedBookIds = new List<int>();
	[SerializeField]
	private GameObject m_loadingIcon;

	// Use this for initialization

	IEnumerator Start () 
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		m_loadingIcon.SetActive(false);

		string storyType = SessionInformation.Instance.GetStoryType();
		if(storyType == "" || storyType == null)
		{
			Debug.Log("Populator defaulting to Classic");
			storyType = "Classic";
		}
		else
		{
			Debug.Log("Populator storytype: " + storyType);
		}

        DataTable dataTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery(
			"select * from stories WHERE storytype=" + "\'" + storyType +  "\'" + " ORDER BY fontsize, difficulty");

		/*
		if(dataTable.Rows.Count > 0)
		{
			foreach(DataRow story in dataTable.Rows)
			{
				try
				{
					Debug.Log(story["title"].ToString() + " - " + story["fontsize"].ToString() + " - " + story["difficulty"].ToString());
				}
				catch
				{
					Debug.Log("MISSING FIELD");
					Debug.Log(story["title"].ToString());

					if(story["fontsize"] != null)
					{
						Debug.Log(story["fontsize"].ToString());
					}
					else
					{
						Debug.Log("fontsize is null");
					}

					if(story["difficulty"] != null)
					{
						Debug.Log(story["difficulty"].ToString());
					}
					else
					{
						Debug.Log("difficulty is null");
					}
				}
			}
		}
		*/

        int bookIndex = 0;

        List<string> bookProductIds = new List<string>();

        foreach (DataRow row in dataTable.Rows)
        {
            if (row["publishable"] != null && row["publishable"].ToString() == "t")
            {
                GameObject bookInstance = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_bookPrefab, transform);
                bookInstance.name = bookIndex.ToString() + "_Book";
                NewStoryBrowserBookButton bookButton = bookInstance.GetComponent<NewStoryBrowserBookButton>();
                bookButton.SetUpWith(row);
                bookProductIds.Add(bookButton.BuildProductIdentifier());
                ++bookIndex;
            }
        }
		
		//StoreKitManager.productListReceivedEvent += new Action<List<StoreKitProduct>>(StoreKitManager_printProducts);
        
		bookProductIds.Add(BuyAllButton.Instance.GetProductIdentifier());

		StoreKitBinding.requestProductData(bookProductIds.ToArray());        

        m_grid.Reposition();

		//GetAndPrintProducts();
	}

	/*
	void GetAndPrintProducts()
	{
		Debug.Log("GetAndPrintProducts()");

		List<StoreKitProduct> products = StoreKitProduct.productsFromJson();

		Debug.Log("products.Count: " + products.Count);

		foreach(StoreKitProduct product in products)
		{
			Debug.Log("Title: " + product.title);
			Debug.Log("Price: " + product.price);
		}
	}
	*/

	/*
	void StoreKitManager_printProducts(List<StoreKitProduct> products)
	{
		Debug.Log("PRINTING PRODUCTS");
		
		foreach(StoreKitProduct product in products)
		{
			Debug.Log("Name: " + product.title);
			Debug.Log("Price: " + product.price);
		}
	}
	*/

	public bool IsBookUnlocked(int bookId)
	{
		return m_unlockedBookIds.Contains(bookId);
	}

    public void JumpToDifficulty(int difficulty)
    {
		// TODO

    }
}
