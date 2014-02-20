using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;

public class GameDataBridge : Singleton<GameDataBridge>
{
    [SerializeField]
    private string m_webStoreURL = @"http://ltrwpdata01.s3-external-3.amazonaws.com/public/";
    [SerializeField]
    private string m_requiredAssetBundlesPackInfo = @"AssetVersions.txt";
    [SerializeField]
    private int m_localDatabaseVersion = 0;
    [SerializeField]
    private TextAsset m_localDatabase;

	public enum DataType // TODO: There should only be one place to reference data types in the whole project. Make a class called Data, give it an enum called Type
	{
		Letters,
		Words,
		Keywords,
	}

	public enum ContentType // TODO: Move this to the Data class
	{
		Sets,
		Voyage,
		Custom,
	}

	ContentType m_contentType = ContentType.Sets;

	public ContentType GetContentType()
	{
		return m_contentType;
	}

	public void SetContentType(ContentType contentType)
	{
		m_contentType = contentType;
	}

    public static IEnumerator WaitForDatabase()
    {
        while (GameDataBridge.Instance == null)
        {
            Debug.Log("Singleton not ready");
            yield return null;
        }
        while (!GameDataBridge.Instance.IsReady())
        {
            //Debug.Log("DB not ready");
            yield return null;
        }
    }

    public string GetOnlineBaseURL()
    {
        return m_webStoreURL;
    }

    private SqliteDatabase m_cmsDb;
    bool m_isReady;
	// Use this for initialization
	IEnumerator Start () 
    {
		/*
		Debug.Log("");
		Debug.Log("");
		Debug.Log("");
		Debug.Log("=======================");
		Debug.Log("GameDataBridge.Start()");
		Debug.Log("=======================");
		Debug.Log("");
		Debug.Log("");
		Debug.Log("");
		*/
		bool forceLocal = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_alwaysUseOfflineDatabase;
		bool useLocal = true;
	    int dbVersion = 0;
		if ( !forceLocal )
		{
			SavedAssetBundles.Instance.StartInitialise(m_webStoreURL + m_requiredAssetBundlesPackInfo);
			while (!SavedAssetBundles.Instance.IsInitialised())
			{
				yield return null;
			}
	        SavedAssetBundles.AssetVersionResult avr = SavedAssetBundles.Instance.GetOnlineAssetVersions();

	        if (avr.m_assetList != null)
	        {
	            foreach (SavedAssetBundles.DownloadedAsset da in avr.m_assetList)
	            {
	                Debug.Log(da.m_name + " " + da.m_version);
	                if (da.m_name == "local-store.bytes")
	                {
						//if the version is set as 0 on the online version list, use the built in DB
	                    if (da.m_version != 0)
	                    {
	                        dbVersion = da.m_version;
	                        useLocal = false;
	                    }
	                }
	            }
	        }
		}

		string debugLogPath = Application.persistentDataPath;
		
		// store DB separately for each game
		string dbName = "local-store.bytes";
        string cmsDbPath = Path.Combine(UnityEngine.Application.persistentDataPath,  
			SavedAssetBundles.Instance.GetSavedName(dbName));

		//string cmsDbPath = "jar:file://" + Application.dataPath + "!/assets/" 
			//+ SavedAssetBundles.Instance.GetSavedName(dbName);

		Debug.Log ("Database name for this game: " + dbName + " filename: " + cmsDbPath);
        m_cmsDb = new SqliteDatabase();

#if UNITY_EDITOR || UNITY_IPHONE || UNITY_STANDALONE
		int alwaysZero = 1;
#else
		int alwaysZero = 0;
#endif
        
		if (forceLocal || useLocal || alwaysZero == 0)
		{
            if ((!SavedAssetBundles.Instance.HasAny(dbName)) ||
                (m_localDatabaseVersion > SavedAssetBundles.Instance.GetVersion(dbName)))
            {
                Debug.Log("Using built-in DB, saving to " + cmsDbPath);

#if UNITY_EDITOR || UNITY_IPHONE || UNITY_STANDALONE
				Debug.Log(cmsDbPath);
				System.IO.File.WriteAllBytes(cmsDbPath, m_localDatabase.bytes);
#else
				Debug.Log(1);
				string streamingAssetsPath = "jar:file://" + Application.dataPath + "!/assets/" + "local-store.bytes";
				Debug.Log(streamingAssetsPath);
				WWW loadDB = new WWW(streamingAssetsPath);
				Debug.Log(2);

				float counter = 0;
				bool foundDB = true;
				Debug.Log(3);
				while(!loadDB.isDone) 
				{
					Debug.Log(4);
					counter += Time.deltaTime;
					Debug.Log("counter: " + counter);
					//float progess = loadDB.progress;
					//Debug.Log(progress);

					if(counter > 120)
					{
						Debug.LogError("Could not find database in StreamingAssets");
						foundDB = false;
						break;
					}
				}
				Debug.Log(5);

				Debug.Log("loadDB.bytes: " + loadDB.bytes);

				if(foundDB)
				{
					Debug.Log(6 + "if");
					Debug.Log("Writing db to persistent data path");
					System.IO.File.WriteAllBytes(cmsDbPath, loadDB.bytes);
					Debug.Log(7 + "if");
	                SavedAssetBundles.Instance.AddAssetVersion(dbName, m_localDatabaseVersion);
					Debug.Log(8 + "if");
				}
				else
				{
					Debug.Log(6 + "else");
					Debug.Log("Not writing db because unable to find");
				}
#endif
            }
            else
            {
                Debug.Log("Using existing from " + cmsDbPath);
            }
        }
        else
        {
            if (!SavedAssetBundles.Instance.HasVersion(dbName, dbVersion))
            {
                Debug.Log("Update to database found!");
                WWW www = new WWW(m_webStoreURL + dbName);
				while(!www.isDone)
				{
					DownloadProgressInformation.Instance.SetDownloading("database", www.progress);
					yield return null;
				}
				DownloadProgressInformation.Instance.StopDownloading("database");
                // no download error - save
                if (www.error == null)
                {
                    System.IO.File.WriteAllBytes(cmsDbPath, www.bytes);
                    SavedAssetBundles.Instance.AddAssetVersion(dbName, dbVersion);
                }
                else
                {

                    // download error - if we don't have one at all, use the local DB
                    if (!SavedAssetBundles.Instance.HasAny(dbName))
                    {
                        System.IO.File.WriteAllBytes(cmsDbPath, m_localDatabase.bytes);
                    }
                }
                www.Dispose();
            }
            else
            {
                Debug.Log("Already on latest database!");
            }
        }

#if UNITY_IPHONE
		iPhone.SetNoBackupFlag(cmsDbPath);
#endif

        m_cmsDb.Open(cmsDbPath);
		
		if (!forceLocal )
		{
			SavedAssetBundles.Instance.DownloadAlwaysPresentAssets(m_webStoreURL);
			while(!SavedAssetBundles.Instance.AllAssetsDownloaded())
			{
				yield return null;
			}
		}
		
        m_isReady = true;
        Debug.Log("Database is ready");

        yield break;
	}

    public bool IsReady()
    {
        return m_isReady;
    }

    public SqliteDatabase GetDatabase()
    {
        return m_cmsDb;
    }

    public DataTable GetSectionWords(int sectionId)
    {
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE section_id=" + sectionId.ToString() + " GROUP BY words.id");

        return dt;
    }

	public List<DataRow> GetSectionLetters(int sectionId = -1)
	{
		if(sectionId == -1)
		{
			sectionId = JourneyInformation.Instance.GetCurrentSectionId();
		}

		List<DataRow> letters = new List<DataRow>();

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId + " GROUP BY phonemes.id");
	
		if(dt.Rows.Count > 0)
		{
			letters = dt.Rows;
		}

		return letters;
	}

	public List<DataRow> GetSectionSentences(int sectionId = -1)
	{
		if(sectionId == -1)
		{
			sectionId = JourneyInformation.Instance.GetCurrentSectionId();
		}
		
		List<DataRow> sentenceData = new List<DataRow>();
		
		DataTable dt = m_cmsDb.ExecuteQuery("select * from sentences WHERE section_id =" + sectionId);
		
		if(dt.Rows.Count > 0)
		{
			sentenceData.AddRange(dt.Rows);
		}
		
		return sentenceData;
	}

    void OnDestroy()
    {
		if(m_cmsDb != null)
		{
        	m_cmsDb.Close();
		}
    }

	public List<DataRow> GetSetData(DataRow set, string columnName, string tableName) // TODO: Store the correct values for these strings, all classes which call these functions should use these references
	{
		string[] ids = set[columnName].ToString().Replace("[", "").Replace("]", "").Split(',');
		
		List<DataRow> data = new List<DataRow>();
		
		foreach(string id in ids)
		{
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from " + tableName + " WHERE id='" + id + "'");
			
			if(dt.Rows.Count > 0)
			{
				data.Add(dt.Rows[0]);
			}
		}
		
		return data;
	}

	public List<DataRow> GetSetData(int setNum, string columnName, string tableName)
	{
		List<DataRow> dataList = new List<DataRow>();

		DataTable setTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + setNum);

		if(setTable.Rows.Count > 0)
		{
			if(setTable.Rows[0][columnName] != null)
			{
				string[] dataIds = setTable.Rows[0][columnName].ToString().Replace(" ","").Replace("-","").Replace("'","").Replace("[", "").Replace("]", "").Split(',');

				foreach(string id in dataIds)
				{
					try
					{
						DataTable dataTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from " + tableName + " WHERE id='" + id + "'");
						
						if(dataTable.Rows.Count > 0)
						{
							dataList.Add(dataTable.Rows[0]);
						}
					}
					catch
					{
						Debug.Log(String.Format("Getting set {0} for {1} - Invalid ID: {2}", setNum, tableName, id));
					}
				}
			}
		}

		return dataList;
	}

	public List<DataRow> GetInclusiveSetData(int setNum, string columnName, string tableName)
	{
		List<DataRow> dataList = new List<DataRow>();

		for(int i = setNum; i > 0; --i)
		{
			//Debug.Log("Set: " + i);

			DataTable setTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + i);

			if(setTable.Rows.Count > 0)
			{
				string[] dataIds = setTable.Rows[0][columnName].ToString().Replace("[", "").Replace("]", "").Split(',');
				
				foreach(string id in dataIds)
				{
					//Debug.Log("id: " + id);
					DataTable dataTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from " + tableName + " WHERE id='" + id + "'");
					
					if(dataTable.Rows.Count > 0)
					{
						//Debug.Log("phoneme: " + dataTable.Rows[0]["phoneme"].ToString());
						dataList.Add(dataTable.Rows[0]);
					}
				}
			}
		}
		
		return dataList;
	}

	public List<DataRow> GetLetters(bool inclusiveSets = true)
	{
		List<DataRow> letterData = new List<DataRow>();

		switch(m_contentType)
		{
		case ContentType.Voyage:
			int sectionId = JourneyInformation.Instance.GetCurrentSectionId();
			Debug.Log("sectionId: " + sectionId);
			DataTable dt = m_cmsDb.ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
			if(dt.Rows.Count > 0)
			{
				letterData.AddRange(dt.Rows);
			}
			else
			{
				Debug.Log("No phonemes for sectionId: " + sectionId);
			}
			break;

		case ContentType.Custom:
			letterData.AddRange(LessonInfo.Instance.GetData(LessonInfo.DataType.Letters));
			break;

		case ContentType.Sets:
			int setNum = SkillProgressInformation.Instance.GetCurrentLevel();
			//Debug.Log("setNum: " + setNum);
			if(inclusiveSets)
			{
				letterData = GetInclusiveSetData(setNum, "setphonemes", "phonemes");
			}
			else
			{
				letterData = GetSetData(setNum, "setphonemes", "phonemes");
			}
			break;
		}

		if(letterData.Count == 0)
		{
			int sectionId = 1407;
			DataTable dt = m_cmsDb.ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
			if(dt.Rows.Count > 0)
			{
				letterData.AddRange(dt.Rows);
			}
		}

		return letterData;
	}

	public List<DataRow> GetWords(bool inclusiveSets = false)
	{
		List<DataRow> wordData = new List<DataRow>();
		
		switch(m_contentType)
		{
		case ContentType.Voyage:
			int sectionId = JourneyInformation.Instance.GetCurrentSectionId();
			Debug.Log("sectionId: " + sectionId);
			DataTable dt = m_cmsDb.ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE section_id=" + sectionId);
			if(dt.Rows.Count > 0)
			{
				wordData.AddRange(dt.Rows);
			}
			break;
			
		case ContentType.Custom:
			//Debug.Log("ContentInformation.Instance: " + ContentInformation.Instance);
			wordData.AddRange(LessonInfo.Instance.GetData(LessonInfo.DataType.Words));
			break;
			
		case ContentType.Sets:
			int setNum = SkillProgressInformation.Instance.GetCurrentLevel();
			if(inclusiveSets)
			{
				wordData.AddRange(GetInclusiveSetData(setNum, "setwords", "words"));
			}
			else
			{
				wordData.AddRange(GetSetData(setNum, "setwords", "words"));
			}
			break;
		}

		for(int i = wordData.Count - 1; i > -1; --i)
		{
			if(WordIsKeyword(wordData[i]) || WordIsNonsense(wordData[i]) || String.IsNullOrEmpty(wordData[i]["word"].ToString()))
			{
				wordData.RemoveAt(i);
			}
		}

		if(wordData.Count == 0)
		{
			wordData = GetSectionWords(1509).Rows;
		}
		
		return wordData;
	}

	public List<DataRow> GetKeywords(bool inclusiveSets = false)
	{
		List<DataRow> keywordData = new List<DataRow>();
		
		switch(m_contentType)
		{
		case ContentType.Voyage:
			int sectionId = JourneyInformation.Instance.GetCurrentSectionId();
			Debug.Log("sectionId: " + sectionId);
			DataTable dt = m_cmsDb.ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE section_id=" + sectionId);
			if(dt.Rows.Count > 0)
			{
				keywordData.AddRange(dt.Rows);

				/*
				for(int i = keywordData.Count - 1; i > -1; --i)
				{
					Debug.Log("keywordData[" + i + "]: " + keywordData[i]);
					if(!WordIsKeyword(keywordData[i]))
					{
						keywordData.RemoveAt(i);
					}
				}
				*/
			}
			break;
			
		case ContentType.Custom:
			keywordData.AddRange(LessonInfo.Instance.GetData(LessonInfo.DataType.Keywords));
			break;
			
		case ContentType.Sets:
			int setNum = SkillProgressInformation.Instance.GetCurrentLevel();
			while(keywordData.Count == 0)
			{
				if(inclusiveSets)
				{
					keywordData.AddRange(GetInclusiveSetData(setNum, "setwords", "words"));
				}
				else
				{
					keywordData.AddRange(GetSetData(setNum, "setwords", "words"));
				}

				for(int i = keywordData.Count - 1; i > -1; --i)
				{
					if(!WordIsKeyword(keywordData[i]))
					{
						keywordData.RemoveAt(i);
					}
				}

				++setNum;
			}
			break;
		}

		Debug.Log("keywordData.Count: " + keywordData.Count);

		foreach(DataRow row in keywordData)
		{
			Debug.Log(row["word"].ToString());
		}

		if(keywordData.Count == 0)
		{
			keywordData = GetSectionWords(1414).Rows;
		}
		
		return keywordData;
	}

	public List<DataRow> GetNonsenseWords()
	{
		Debug.Log("GetNonsenseWords()");

		List<DataRow> nonsenseData = new List<DataRow>();
		
		switch(m_contentType)
		{
		case ContentType.Voyage:
			int sectionId = JourneyInformation.Instance.GetCurrentSectionId();
			Debug.Log("sectionId: " + sectionId);
			DataTable dt = m_cmsDb.ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE section_id=" + sectionId);
			if(dt.Rows.Count > 0)
			{
				nonsenseData.AddRange(dt.Rows);
			}
			break;
			
		case ContentType.Custom:
			List<DataRow> realWords = ContentInformation.Instance.GetWords();

			Debug.Log("realWords.Count: " + realWords.Count);
			foreach(DataRow word in realWords)
			{
				Debug.Log(word["word"].ToString());
			}

			DataTable dtSets = m_cmsDb.ExecuteQuery("select * from phonicssets ORDER BY number DESC");

			int highestSetNum = 1;

			if(dtSets.Rows.Count > 0)
			{
				foreach(DataRow set in dtSets.Rows)
				{
					string[] wordIds = set["setwords"].ToString().Replace("[", "").Replace("]", "").Split(',');

					foreach(string id in wordIds)
					{
						DataTable dtWords = m_cmsDb.ExecuteQuery("select * from words WHERE id='" + id + "'");

						if(dtWords.Rows.Count > 0 && realWords.Contains(dtWords.Rows[0]) && System.Convert.ToInt32(set["number"]) > highestSetNum)
						{
							highestSetNum = System.Convert.ToInt32(set["number"]);
						}
					}
				}
			}

			Debug.Log("highestSetNum: " + highestSetNum);

			foreach(DataRow set in dtSets.Rows)
			{
				if(System.Convert.ToInt32(set["number"]) == highestSetNum)
				{
					string[] wordIds = set["setsillywords"].ToString().Replace("[", "").Replace("]", "").Split(',');
					
					foreach(string id in wordIds)
					{
						DataTable dtWords = m_cmsDb.ExecuteQuery("select * from words WHERE id='" + id + "'");

						Debug.Log("Found highest set");
						foreach(DataRow word in dtWords.Rows)
						{
							Debug.Log(word["word"].ToString());
						}

						if(dtWords.Rows.Count > 0)
						{
							nonsenseData.AddRange(dtWords.Rows);
						}
					}
				}
			}
			break;
			
		case ContentType.Sets:
			int setNum = SkillProgressInformation.Instance.GetCurrentLevel();
			Debug.Log("setNum: " + setNum);
			nonsenseData = GetSetData(setNum, "setsillywords", "words");
			break;
		}
		
		for(int i = nonsenseData.Count - 1; i > -1; --i)
		{
			if(!WordIsNonsense(nonsenseData[i]))
			{
				nonsenseData.RemoveAt(i);
			}
		}

		if(nonsenseData.Count == 0)
		{
			nonsenseData = GetSectionWords(1395).Rows;
		}
		
		return nonsenseData;
	}

	public bool SetContainsData(DataRow set, string setAttribute, string dataType)
	{
		string[] ids = set[setAttribute].ToString().Replace("[", "").Replace("]", "").Split(',');

		/*
		Debug.Log("ids.Length: " + ids.Length);
		foreach(string id in ids)
		{
			Debug.Log(id);
		}
		*/

		return (ids.Length > 0);
	}

	public List<DataRow> GetSetPhonemes(DataRow set)
	{
		string[] phonemeIds = set["setphonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
		
		List<DataRow> phonemes = new List<DataRow>();
		
		foreach(string id in phonemeIds)
		{
			DataTable phonemeTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id='" + id + "'");
			
			if(phonemeTable.Rows.Count > 0)
			{
				phonemes.Add(phonemeTable.Rows[0]);
			}
		}

		return phonemes;
	}

	public List<DataRow> GetOrderedPhonemes(DataRow word)
	{
		List<DataRow> phonemes = new List<DataRow>();
		string[] phonemeIds = word["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
		foreach(string id in phonemeIds)
		{
			DataTable dt = m_cmsDb.ExecuteQuery("select * from phonemes WHERE id='" + id + "'");
			if(dt.Rows.Count > 0)
			{
				phonemes.Add(dt.Rows[0]);
			}
		}

		return phonemes;
	}

	bool WordIsKeyword(DataRow wordData)
	{
		return ((wordData["tricky"] != null && wordData["tricky"].ToString() == "t") || (wordData["nondecodable"] != null && wordData["nondecodable"].ToString() == "t"));
	}

	bool WordIsNonsense(DataRow wordData)
	{
		return (wordData["nonsense"].ToString() == "t");
	}


}
