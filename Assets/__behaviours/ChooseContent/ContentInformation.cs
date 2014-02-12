using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;

public class ContentInformation : Singleton<ContentInformation> 
{
	Dictionary<string, DataGroup> m_letters = new Dictionary<string, DataGroup>();
	Dictionary<string, DataGroup> m_words = new Dictionary<string, DataGroup>();
	Dictionary<string, DataGroup> m_keywords = new Dictionary<string, DataGroup>();

	Dictionary<string, DataRow> m_targetLetters = new Dictionary<string, DataRow>();
	Dictionary<string, DataRow> m_targetWords = new Dictionary<string, DataRow>();
	Dictionary<string, DataRow> m_targetKeywords = new Dictionary<string, DataRow>();

	bool m_hasLoaded = false;
	bool m_useCustom = false;

	IEnumerator Start()
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		Load();

		m_hasLoaded = true;
	}
	
	bool HasLoaded()
	{
		return m_hasLoaded;
	}

	public bool UseCustom()
	{
		return m_useCustom;
	}

	public void SetUseCustom(bool useCustom)
	{
		m_useCustom = useCustom;
	}

	public static IEnumerator WaitForLoad()
	{
		while(ContentInformation.Instance == null)
		{
			Debug.Log("Waiting for ContentInformation.Instance");
			yield return null;
		}
		while(!ContentInformation.Instance.HasLoaded())
		{
			Debug.Log("Waiting for ContentInformation to load");
			yield return null;
		}
	}

	class DataGroup
	{
		Dictionary<int, DataRow> m_data = new Dictionary<int, DataRow>();
		
		public DataGroup() {}
		
		public void AddData(int id, DataRow data)
		{
			m_data[id] = data;
		}
		
		public void AddData(DataRow data)
		{
			m_data[Convert.ToInt32(data["id"])] = data;
		}
		
		public void RemoveData(int key)
		{
			m_data.Remove(key);
		}
		
		public void RemoveData(DataRow data)
		{
			int keyToRemove = -1;
			foreach(KeyValuePair<int, DataRow> kvp in m_data)
			{
				if(kvp.Key == Convert.ToInt32(data["id"]))
				{
					keyToRemove = kvp.Key;
				}
			}
			
			if(keyToRemove != -1)
			{
				m_data.Remove(keyToRemove);
			}
		}
		
		public Dictionary<int, DataRow> GetDataDictionary ()
		{
			return m_data;
		}
		
		public List<DataRow> GetDataList ()
		{
			if(m_data.Values != null)
			{
				return m_data.Values.ToList();
			}
			else
			{
				return new List<DataRow>();
			}
		}
		
		public int GetDataCount ()
		{
			return m_data.Count;
		}
		
		public bool HasData (int key)
		{
			return m_data.ContainsKey(key);
		}
		
		public bool HasData (DataRow data)
		{
			foreach(KeyValuePair<int, DataRow> kvp in m_data)
			{
				if(kvp.Key == Convert.ToInt32(data["id"]))
				{
					return true;
				}
			}
			
			return false;
		}
	}

	public List<DataRow> GetLetters()
	{
		string user = ChooseUser.Instance.GetCurrentUser();
		if(m_letters.ContainsKey(user))
		{
			return m_letters[user].GetDataList();
		}
		else
		{
			return null;
		}
	}

	public void AddLetter(DataRow letterData, bool save = false)
	{
		Debug.Log("AddLetter: " + letterData["phoneme"].ToString());
		string currentUser = ChooseUser.Instance.GetCurrentUser();

		if(!m_letters.ContainsKey(currentUser))
		{
			m_letters.Add(currentUser, new DataGroup());
		}

		m_letters[currentUser].AddData(letterData);

		if(save)
		{
			Save ();
		}
	}

	public void RemoveLetter(DataRow letterData, bool save = false)
	{
		Debug.Log("RemoveLetter: " + letterData["phoneme"].ToString());
		string currentUser = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_letters.ContainsKey(currentUser))
		{
			m_letters.Add(currentUser, new DataGroup());
		}

		m_letters[currentUser].RemoveData(letterData);

		if(save)
		{
			Save ();
		}
	}

	public bool HasLetter(DataRow letterData)
	{
		string currentUser = ChooseUser.Instance.GetCurrentUser();

		if(m_letters.ContainsKey(currentUser))
		{
			return m_letters[currentUser].HasData(letterData);
		}
		else
		{
			Debug.Log("Defaulting to false for " + currentUser);
			return false;
		}
	}

	public DataRow GetTargetLetter()
	{
		string currentUser = ChooseUser.Instance.GetCurrentUser();
		
		if(m_targetLetters.ContainsKey(currentUser))
		{
			return m_targetLetters[currentUser];
		}
		else
		{
			return null;
		}
	}
	
	public void SetTargetLetter(DataRow letterData)
	{
		if(letterData != null)
		{
			Debug.Log("Setting targetLetter: " + letterData["phoneme"].ToString());
		}
		else
		{
			Debug.Log("Setting targetLetter null");
		}
		m_targetLetters[ChooseUser.Instance.GetCurrentUser()] = letterData;
		Save ();
	}
	
	public bool IsTargetLetter(DataRow letterData)
	{
		string currentUser = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_targetLetters.ContainsKey(currentUser) || m_targetLetters[currentUser] == null)
		{
			return false;
		}
		else
		{
			return ((Convert.ToInt32(m_targetLetters[currentUser]["id"]) == Convert.ToInt32(letterData["id"])));
		}
	}

	public List<DataRow> GetWords()
	{
		Debug.Log("ContentInformation.Instance.GetWords()");
		string user = ChooseUser.Instance.GetCurrentUser();
		Debug.Log("user: " + user);
		if(m_words.ContainsKey(user))
		{
			return m_words[user].GetDataList();
		}
		else
		{
			Debug.Log("Could not find user");
			return new List<DataRow>();
		}
	}

	public void AddWord(DataRow wordData, bool save = false)
	{
		string currentUser = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_words.ContainsKey(currentUser))
		{
			m_words.Add(currentUser, new DataGroup());
		}
		
		m_words[currentUser].AddData(wordData);

		if(save)
		{
			Save ();
		}
	}
	
	public void RemoveWord(DataRow wordData, bool save = false)
	{
		string currentUser = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_words.ContainsKey(currentUser))
		{
			m_words.Add(currentUser, new DataGroup());
		}
		
		m_words[currentUser].RemoveData(wordData);

		if(save)
		{
			Save ();
		}
	}

	public bool HasWord(DataRow wordData)
	{
		string currentUser = ChooseUser.Instance.GetCurrentUser();
		
		if(m_words.ContainsKey(currentUser))
		{
			return m_words[currentUser].HasData(wordData);
		}
		else
		{
			return false;
		}
	}

	public DataRow GetTargetWord()
	{
		string currentUser = ChooseUser.Instance.GetCurrentUser();
		
		if(m_targetWords.ContainsKey(currentUser))
		{
			return m_targetWords[currentUser];
		}
		else
		{
			return null;
		}
	}
	
	public void SetTargetWord(DataRow letterData)
	{
		m_targetWords[ChooseUser.Instance.GetCurrentUser()] = letterData;
		Save ();
	}

	public bool IsTargetWord(DataRow wordData)
	{
		string currentUser = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_targetWords.ContainsKey(currentUser) || m_targetWords[currentUser] == null)
		{
			return false;
		}
		else
		{
			return ((Convert.ToInt32(m_targetWords[currentUser]["id"]) == Convert.ToInt32(wordData["id"])));
		}
	}

	public List<DataRow> GetKeywords()
	{
		string user = ChooseUser.Instance.GetCurrentUser();
		if(m_keywords.ContainsKey(user))
		{
			return m_keywords[user].GetDataList();
		}
		else
		{
			return null;
		}
	}

	public void AddKeyword(DataRow keywordData, bool save = false)
	{
		string currentUser = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_keywords.ContainsKey(currentUser))
		{
			m_keywords.Add(currentUser, new DataGroup());
		}
		
		m_keywords[currentUser].AddData(keywordData);

		if(save)
		{
			Save ();
		}
	}
	
	public void RemoveKeyword(DataRow keywordData, bool save = false)
	{
		string currentUser = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_keywords.ContainsKey(currentUser))
		{
			m_keywords.Add(currentUser, new DataGroup());
		}
		
		m_keywords[currentUser].RemoveData(keywordData);

		if(save)
		{
			Save ();
		}
	}

	public bool HasKeyword(DataRow keywordData)
	{
		string currentUser = ChooseUser.Instance.GetCurrentUser();
		
		if(m_keywords.ContainsKey(currentUser))
		{
			return m_keywords[currentUser].HasData(keywordData);
		}
		else
		{
			return false;
		}
	}

	public DataRow GetTargetKeyword()
	{
		string currentUser = ChooseUser.Instance.GetCurrentUser();
		
		if(m_targetKeywords.ContainsKey(currentUser))
		{
			return m_targetKeywords[currentUser];
		}
		else
		{
			return null;
		}
	}
	
	public void SetTargetKeyword(DataRow keywordData)
	{
		m_targetKeywords[ChooseUser.Instance.GetCurrentUser()] = keywordData;
		Save ();
	}

	public bool IsTargetKeyword(DataRow keywordData)
	{
		string currentUser = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_targetKeywords.ContainsKey(currentUser) || m_targetKeywords[currentUser] == null)
		{
			return false;
		}
		else
		{
			return ((Convert.ToInt32(m_targetKeywords[currentUser]["id"]) == Convert.ToInt32(keywordData["id"])));
		}
	}

	// Use this for initialization
	void Load()
	{
		DataSaver ds = new DataSaver("ContentInformation");
		MemoryStream data = ds.Load();
		BinaryReader br = new BinaryReader(data);

		SqliteDatabase database = GameDataBridge.Instance.GetDatabase();
		
		if (data.Length != 0)
		{
			// Letters
			int numLetterUsers = br.ReadInt32();
			for (int i = 0; i < numLetterUsers; ++i)
			{
				string user = br.ReadString();

				DataGroup dataGroup = new DataGroup();

				int numLetters = br.ReadInt32();
				List<int> letterIds = new List<int>();
				for(int j = 0; j < numLetters; ++j)
				{
					letterIds.Add(br.ReadInt32());
				}

				foreach(int id in letterIds)
				{
					DataTable dt = database.ExecuteQuery("select * from phonemes WHERE id=" + id);
					if(dt.Rows.Count > 0)
					{
						dataGroup.AddData(id, dt.Rows[0]);
					}
				}

				m_letters[user] = dataGroup;
			}

			// Target Letter
			int numTargetLetterUsers = br.ReadInt32();
			for(int i = 0; i < numTargetLetterUsers; ++i)
			{
				string user = br.ReadString();
				DataTable dt = database.ExecuteQuery("select * from phonemes WHERE id=" + br.ReadInt32());

				if(dt.Rows.Count > 0)
				{
					m_targetLetters.Add(user, dt.Rows[0]);
				}
				else
				{
					m_targetLetters.Add(user, null);
				}
			}

			// Words
			int numWordUsers = br.ReadInt32();
			for(int i = 0; i < numWordUsers; ++i)
			{
				string user = br.ReadString();

				DataGroup dataGroup = new DataGroup();

				int numWords = br.ReadInt32();
				List<int> wordIds = new List<int>();
				for(int j = 0; j < numWords; ++j)
				{
					wordIds.Add(br.ReadInt32());
				}

				foreach(int id in wordIds)
				{
					DataTable dt = database.ExecuteQuery("select * from words WHERE id=" + id);
					if(dt.Rows.Count > 0)
					{
						dataGroup.AddData(id, dt.Rows[0]);
					}
				}

				m_letters[user] = dataGroup;
			}

			// Target Word
			int numTargetWordUsers = br.ReadInt32();
			for(int i = 0; i < numTargetWordUsers; ++i)
			{
				string user = br.ReadString();
				DataTable dt = database.ExecuteQuery("select * from words WHERE id=" + br.ReadInt32());
				
				if(dt.Rows.Count > 0)
				{
					m_targetWords.Add(user, dt.Rows[0]);
				}
				else
				{
					m_targetWords.Add(user, null);
				}
			}

			// Keywords
			int numKeywordUsers = br.ReadInt32();
			for(int i = 0; i < numKeywordUsers; ++i)
			{
				string user = br.ReadString();
				
				DataGroup dataGroup = new DataGroup();
				
				int numKeywords = br.ReadInt32();
				List<int> keywordIds = new List<int>();
				for(int j = 0; j < numKeywords; ++j)
				{
					keywordIds.Add(br.ReadInt32());
				}
				
				foreach(int id in keywordIds)
				{
					DataTable dt = database.ExecuteQuery("select * from words WHERE id=" + id);
					if(dt.Rows.Count > 0)
					{
						dataGroup.AddData(id, dt.Rows[0]);
					}
				}
				
				m_letters[user] = dataGroup;
			}

			// Target Keyword
			int numTargetKeywordUsers = br.ReadInt32();
			for(int i = 0; i < numTargetKeywordUsers; ++i)
			{
				string user = br.ReadString();
				DataTable dt = database.ExecuteQuery("select * from words WHERE id=" + br.ReadInt32());
				
				if(dt.Rows.Count > 0)
				{
					m_targetKeywords.Add(user, dt.Rows[0]);
				}
				else
				{
					m_targetKeywords.Add(user, null);
				}
			}
		}
		br.Close();
		data.Close();
	}

	public void Save()
	{
		DataSaver ds = new DataSaver("ContentInformation");
		MemoryStream newData = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(newData);

		// Letters
		bw.Write(m_letters.Count);
		foreach (KeyValuePair<string, DataGroup> kvp in m_letters)
		{
			bw.Write(kvp.Key);

			List<DataRow> data = kvp.Value.GetDataList();

			bw.Write(data.Count);
			foreach(DataRow row in data)
			{
				bw.Write(Convert.ToInt32(row["id"]));
			}
		}

		// Target Letter
		bw.Write(m_targetLetters.Count);
		foreach(KeyValuePair<string, DataRow> kvp in m_targetLetters)
		{
			bw.Write(kvp.Key);

			try
			{
				bw.Write(Convert.ToInt32(kvp.Value["id"]));
			}
			catch
			{
				bw.Write(-1);
			}
		}

		// Words
		bw.Write(m_words.Count);
		foreach (KeyValuePair<string, DataGroup> kvp in m_words)
		{
			bw.Write(kvp.Key);
			
			List<DataRow> data = kvp.Value.GetDataList();
			
			bw.Write(data.Count);
			foreach(DataRow row in data)
			{
				bw.Write(Convert.ToInt32(row["id"]));
			}
		}

		// Target Word
		bw.Write(m_targetWords.Count);
		foreach(KeyValuePair<string, DataRow> kvp in m_targetWords)
		{
			bw.Write(kvp.Key);

			try
			{
				bw.Write(Convert.ToInt32(kvp.Value["id"]));
			}
			catch
			{
				bw.Write(-1);
			}
		}

		// Keywords
		bw.Write(m_keywords.Count);
		foreach (KeyValuePair<string, DataGroup> kvp in m_keywords)
		{
			bw.Write(kvp.Key);
			
			List<DataRow> data = kvp.Value.GetDataList();
			
			bw.Write(data.Count);
			foreach(DataRow row in data)
			{
				bw.Write(Convert.ToInt32(row["id"]));
			}
		}

		// Target Keyword
		bw.Write(m_targetKeywords.Count);
		foreach(KeyValuePair<string, DataRow> kvp in m_targetKeywords)
		{
			bw.Write(kvp.Key);

			try
			{
				bw.Write(Convert.ToInt32(kvp.Value["id"]));
			}
			catch
			{
				bw.Write(-1);
			}
		}
		
		ds.Save(newData);
		
		bw.Close();
		newData.Close();
	}
}
