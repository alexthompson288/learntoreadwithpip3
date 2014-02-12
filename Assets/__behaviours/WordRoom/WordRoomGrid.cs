using UnityEngine;
using System.Collections;
using Wingrove;
using System.Collections.Generic;

public class WordRoomGrid : Singleton<WordRoomGrid>
{
	[SerializeField]
	private GameObject m_buttonPrefab;
	[SerializeField]
	private int[] m_sectionIds;
	
	List<DataRow> m_data = new List<DataRow>();
	List<GameObject> m_createdWords = new List<GameObject>();
	
	// Use this for initialization
	IEnumerator Start()
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		/*
		foreach (int sectionId in ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_difficultyDatabaseIds)
		{
			DataTable dt = GameDataBridge.Instance.GetSectionWords(sectionId);
			
			m_data.AddRange(dt.Rows);
		}
		*/

		int i = 0;
		while(m_data.Count < 21)
		{
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + i);

			if(dt.Rows.Count > 0)
			{
				string[] wordIds = dt.Rows[0]["setwords"].ToString().Replace("[", "").Replace("]", "").Split(',');
				
				List<DataRow> words = new List<DataRow>();
				
				foreach(string id in wordIds)
				{
					DataTable wordTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words WHERE id='" + id + "'");
					
					if(wordTable.Rows.Count > 0)
					{
						m_data.Add(wordTable.Rows[0]);
					}
				}
			}

			++i;
		}


		
		int index = 0;
		foreach (DataRow row in m_data)
		{
			GameObject newWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_buttonPrefab,
			                                                                            transform);

			newWord.GetComponent<WordRoomButton>().SetUp(row["word"].ToString());
			newWord.name = string.Format("WORD_{0:000}_BOX", index);
			index++;
			m_createdWords.Add(newWord);
		}
		
		GetComponent<UIGrid>().Reposition();
		
		yield return new WaitForSeconds(1.0f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("WORD_BANK_1");
	}
}
