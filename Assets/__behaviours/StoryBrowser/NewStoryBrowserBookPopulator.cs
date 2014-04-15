using UnityEngine;
using System.Collections;
using Wingrove;
using System.Collections.Generic;
using System;

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

	bool m_productListResolved = false;

	IEnumerator Start () 
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		m_loadingIcon.SetActive(false);

		string storyType = SessionInformation.Instance.GetStoryType();
		if(storyType == "" || storyType == null)
		{
            Debug.Log("Populator defaulting to Pink");
			storyType = "Pink";
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

        //List<string> bookProductIds = new List<string>();

        foreach (DataRow row in dataTable.Rows)
        {
            if (row["publishable"] != null && row["publishable"].ToString() == "t")
            {
                GameObject bookInstance = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_bookPrefab, transform);
                bookInstance.name = bookIndex.ToString() + "_Book";
                NewStoryBrowserBookButton bookButton = bookInstance.GetComponent<NewStoryBrowserBookButton>();
                bookButton.SetUpWith(row);
                ++bookIndex;
            }
        }

        m_grid.Reposition();
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
	


	public bool IsBookUnlocked(int bookId)
	{
		return m_unlockedBookIds.Contains(bookId);
	}

    public void JumpToDifficulty(int difficulty)
    {
		// TODO

    }
}
