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

	IEnumerator Start () 
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		m_loadingIcon.SetActive(false);

		string storyType = SessionInformation.Instance.GetStoryType();
        Debug.Log("storyType: " + storyType);

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

        int bookIndex = 0;


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

	public bool IsBookUnlocked(int bookId)
	{
		return m_unlockedBookIds.Contains(bookId);
	}
}
