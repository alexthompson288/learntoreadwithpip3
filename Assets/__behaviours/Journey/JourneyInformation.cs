using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class JourneyInformation : Singleton<JourneyInformation> 
{
#if UNITY_EDITOR
	[SerializeField]
	private bool m_overwriteProgress;
#endif

	void OnSessionComplete()
	{
		Debug.Log("JourneyInformation.OnSessionComplete()");
		
		int sessionNum = SessionManager.Instance.GetSessionNum();
		
		if(sessionNum > GetSessionsCompleted())
		{
			SetSessionsCompleted(sessionNum);
		}
		
		#if UNITY_IPHONE
		Dictionary<string, string> ep = new Dictionary<string, string>();
		ep.Add("CurrentSession", sessionNum.ToString());
		ep.Add("HighestSession", GetSessionsCompleted().ToString());
		
		FlurryBinding.endTimedEvent("StartVoyageSession", ep);
		FlurryBinding.logEventWithParameters("FinishVoyageSession", ep, false);
		#endif
		
		SetSessionFinished(sessionNum);
		
		SetRecentlyCompleted(true);
		
		UnsubscribeOnSessionComplete();
	}

	public void SubscribeOnSessionComplete()
	{
		SessionManager.Instance.OnSessionComplete += OnSessionComplete;
	}

	public void UnsubscribeOnSessionComplete()
	{
		SessionManager.Instance.OnSessionComplete -= OnSessionComplete;
	}

	public enum Environment
	{
		Forest,
		Underwater,
		Castle,
		Farm,
		School,
		Space
	}

	Environment m_environment;

	public void SetEnvironment(Environment environment)
	{
		m_environment = environment;
	}

	public Environment GetEnvironment()
	{
		return m_environment;
	}
	
	[SerializeField]
	private TextAsset m_gameNameFile;
	
	// Set from file
	Dictionary<string, string> m_gameNames = new Dictionary<string, string>();
	
	// Saved between app close/open
	Dictionary<string, HashSet<int>> m_sessionsFinished = new Dictionary<string, HashSet<int>>();
	Dictionary<string, int> m_sessionsCompleted = new Dictionary<string, int>();
	Dictionary<string, bool> m_recentCompleted = new Dictionary<string, bool>();
	
	void Awake()
	{
#if UNITY_EDITOR
		if(m_overwriteProgress)
		{
			Save();
		}
#endif

		string allGameNames = m_gameNameFile.text;
		string[] separatedGameNames = allGameNames.Split(',');
		
		for(int i = 0; i < separatedGameNames.Length; ++i)
		{
			separatedGameNames[i] = StringHelpers.Edit(separatedGameNames[i], new string[] { "_" } );
		}
		
		for(int i = 0; i < separatedGameNames.Length - 1; i += 2)
		{
			m_gameNames[separatedGameNames[i]] = separatedGameNames[i + 1];
		}
		
		Load ();
	}
	
	public bool IsSessionFinished(int sessionNum)
	{
		string user = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_sessionsFinished.ContainsKey(user))
		{
			m_sessionsFinished.Add(user, new HashSet<int>());
		}
		
		return m_sessionsFinished[user].Contains(sessionNum);
	}
	
	public void SetSessionFinished(int sessionNum)
	{
		string user = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_sessionsFinished.ContainsKey(user))
		{
			m_sessionsFinished.Add(user, new HashSet<int>());
		}
		
		m_sessionsFinished[user].Add(sessionNum);
		
		Save ();
	}
	
	public int GetSessionsCompleted()
	{
		string user = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_sessionsCompleted.ContainsKey(user))
		{
			m_sessionsCompleted[user] = 0;
		}
		
		return m_sessionsCompleted[user];
	}
	
	public void SetSessionsCompleted(int session)
	{
		m_sessionsCompleted[ChooseUser.Instance.GetCurrentUser()] = session;
		Save ();
	}
	
	public bool HasRecentlyCompleted()
	{
		string user = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_recentCompleted.ContainsKey(user))
		{
			m_recentCompleted[user] = false;
		}
		
		return m_recentCompleted[user];
	}
	
	public void SetRecentlyCompleted(bool sessionRecentlyCompleted)
	{
		m_recentCompleted[ChooseUser.Instance.GetCurrentUser()] = sessionRecentlyCompleted;
		Save ();
	}
	
	void Load()
	{
		DataSaver ds = new DataSaver("JourneyInformation");
		MemoryStream data = ds.Load();
		BinaryReader br = new BinaryReader(data);
		
		try
		{
			int numSessionsCompletedUsers = br.ReadInt32();
			for(int i = 0; i < numSessionsCompletedUsers; ++i)
			{
				string user = br.ReadString();
				int sessionsCompleted = br.ReadInt32();
				
				m_sessionsCompleted[user] = sessionsCompleted;
			}
			
			int numRecentCompletedUsers = br.ReadInt32();
			for(int i = 0; i < numRecentCompletedUsers; ++i)
			{
				string user = br.ReadString();
				bool recentComplete = br.ReadBoolean();
				
				m_recentCompleted[user] = recentComplete;
			}
			
			int numUsersSessionsFinished = br.ReadInt32();
			for(int i = 0; i < numUsersSessionsFinished; ++i)
			{
				string user = br.ReadString();
				
				m_sessionsFinished.Add(user, new HashSet<int>());
				
				int numSessionsFinished = br.ReadInt32();
				for(int j = 0; j < numSessionsFinished; ++j)
				{
					m_sessionsFinished[user].Add(br.ReadInt32());
				}
			}
		}
		catch{}
		
		br.Close();
		data.Close();
	}
	
	void Save()
	{
		DataSaver ds = new DataSaver("JourneyInformation");
		MemoryStream newData = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(newData);
		
		bw.Write(m_sessionsCompleted.Count);
		foreach(KeyValuePair<string, int> kvp in m_sessionsCompleted)
		{
			bw.Write(kvp.Key);
			bw.Write(kvp.Value);
		}
		
		bw.Write(m_recentCompleted.Count);
		foreach(KeyValuePair<string, bool> kvp in m_recentCompleted)
		{
			bw.Write(kvp.Key);
			bw.Write(kvp.Value);
		}
		
		bw.Write(m_sessionsFinished.Count);
		foreach(KeyValuePair<string, HashSet<int>> kvp in m_sessionsFinished)
		{
			bw.Write(kvp.Key);
			
			bw.Write(kvp.Value.Count);
			foreach(int i in kvp.Value)
			{
				bw.Write(i);
			}
		}
		
		ds.Save(newData);
		
		bw.Close();
		newData.Close();
	}
}

/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class JourneyInformation : Singleton<JourneyInformation> 
{
	void OnSessionComplete()
	{
		Debug.Log("JourneyInformation.OnSessionComplete()");

		int sessionNum = SessionManager.Instance.GetSessionNum();

		if(sessionNum > GetSessionsCompleted())
		{
			SetSessionsCompleted(sessionNum);
		}

#if UNITY_IPHONE
		//FlurryBinding.endTimedEvent("StartVoyageSession_" + sessionNum.ToString());

		Dictionary<string, string> ep = new Dictionary<string, string>();
		ep.Add("CurrentSession", sessionNum.ToString());
		ep.Add("HighestSession", GetSessionsCompleted().ToString());

		//Dictionary<string, string> eventParameters = new Dictionary<string, string>();
		//eventParameters.Add("Current", sessionNum.ToString());
		//eventParameters.Add("Highest", GetSessionsCompleted().ToString());

		FlurryBinding.endTimedEvent("StartVoyageSession", ep);
		FlurryBinding.logEventWithParameters("FinishVoyageSession", ep, false);
#endif

		SetSessionFinished(sessionNum);

		SetRecentlyCompleted(true);

		SessionManager.Instance.OnSessionComplete -= OnSessionComplete;
	}

	[SerializeField]
	private TextAsset m_gameNameFile;

	// Set from file
	Dictionary<string, string> m_gameNames = new Dictionary<string, string>();

	// Saved between app close/open
	Dictionary<string, HashSet<int>> m_sessionsFinished = new Dictionary<string, HashSet<int>>();
	Dictionary<string, int> m_sessionsCompleted = new Dictionary<string, int>();
	Dictionary<string, bool> m_recentCompleted = new Dictionary<string, bool>();
	Dictionary<string, int> m_lastLetterUnlocked = new Dictionary<string, int>();

	// Forgotten between app close/open
	int m_sectionsComplete = 0;
	List<DataRow> m_sections = new List<DataRow>();
	int m_currentSessionNum;

	void Awake()
	{
		string allGameNames = m_gameNameFile.text;
		string[] separatedGameNames = allGameNames.Split(',');

		for(int i = 0; i < separatedGameNames.Length; ++i)
		{
			separatedGameNames[i] = StringHelpers.Edit(separatedGameNames[i], new string[] { "_" } );
		}

		for(int i = 0; i < separatedGameNames.Length - 1; i += 2)
		{
			m_gameNames[separatedGameNames[i]] = separatedGameNames[i + 1];
		}

		Load ();
	}

	public void SubscribeOnSessionComplete()
	{
		SessionManager.Instance.OnSessionComplete += OnSessionComplete;
	}

	public bool IsSessionFinished(int sessionNum)
	{
		string user = ChooseUser.Instance.GetCurrentUser();

		if(!m_sessionsFinished.ContainsKey(user))
		{
			m_sessionsFinished.Add(user, new HashSet<int>());
		}

		return m_sessionsFinished[user].Contains(sessionNum);
	}

	public void SetSessionFinished(int sessionNum)
	{
		string user = ChooseUser.Instance.GetCurrentUser();

		if(!m_sessionsFinished.ContainsKey(user))
		{
			m_sessionsFinished.Add(user, new HashSet<int>());
		}

		m_sessionsFinished[user].Add(sessionNum);

		Save ();
	}

	public int GetSessionsCompleted()
	{
		string user = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_sessionsCompleted.ContainsKey(user))
		{
			m_sessionsCompleted[user] = 0;
		}

		return m_sessionsCompleted[user];
	}
	
	public void SetSessionsCompleted(int session)
	{
		m_sessionsCompleted[ChooseUser.Instance.GetCurrentUser()] = session;
		Save ();
	}

	public bool HasRecentlyCompleted()
	{
		string user = ChooseUser.Instance.GetCurrentUser();

		if(!m_recentCompleted.ContainsKey(user))
		{
			m_recentCompleted[user] = false;
		}

		return m_recentCompleted[user];
	}
	
	public void SetRecentlyCompleted(bool sessionRecentlyCompleted)
	{
		m_recentCompleted[ChooseUser.Instance.GetCurrentUser()] = sessionRecentlyCompleted;
		Save ();
	}

	public int GetLastLetterUnlocked()
	{
		string user = ChooseUser.Instance.GetCurrentUser();
		
		if(!m_lastLetterUnlocked.ContainsKey(user))
		{
			m_lastLetterUnlocked[user] = -1;
		}
		
		return m_lastLetterUnlocked[user];
	}

	public void SetLastLetterUnlocked(DataRow letter)
	{
		string user = ChooseUser.Instance.GetCurrentUser();

		m_lastLetterUnlocked[user] = -1;

		if(letter != null)
		{
			m_lastLetterUnlocked[user] = System.Convert.ToInt32(letter["id"]);
		}

		Save ();
	}

	void Load()
	{
		DataSaver ds = new DataSaver("JourneyInformation");
		MemoryStream data = ds.Load();
		BinaryReader br = new BinaryReader(data);
		
		try
		{
			int numSessionsCompletedUsers = br.ReadInt32();
			for(int i = 0; i < numSessionsCompletedUsers; ++i)
			{
				string user = br.ReadString();
				int sessionsCompleted = br.ReadInt32();
				
				m_sessionsCompleted[user] = sessionsCompleted;
			}

			int numRecentCompletedUsers = br.ReadInt32();
			for(int i = 0; i < numRecentCompletedUsers; ++i)
			{
				string user = br.ReadString();
				bool recentComplete = br.ReadBoolean();

				m_recentCompleted[user] = recentComplete;
			}
			
			int numLetterUnlockedUsers = br.ReadInt32();
			for(int i = 0; i < numLetterUnlockedUsers; ++i)
			{
				string user = br.ReadString();
				int lastLetterUnlocked = br.ReadInt32();

				m_lastLetterUnlocked[user] = lastLetterUnlocked;
			}

			int numUsersSessionsFinished = br.ReadInt32();
			for(int i = 0; i < numUsersSessionsFinished; ++i)
			{
				string user = br.ReadString();

				m_sessionsFinished.Add(user, new HashSet<int>());

				int numSessionsFinished = br.ReadInt32();
				for(int j = 0; j < numSessionsFinished; ++j)
				{
					m_sessionsFinished[user].Add(br.ReadInt32());
				}
			}
		}
		catch{}
		
		br.Close();
		data.Close();
	}
	
	void Save()
	{
		DataSaver ds = new DataSaver("JourneyInformation");
		MemoryStream newData = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(newData);
		
		bw.Write(m_sessionsCompleted.Count);
		foreach(KeyValuePair<string, int> kvp in m_sessionsCompleted)
		{
			bw.Write(kvp.Key);
			bw.Write(kvp.Value);
		}
		
		bw.Write(m_recentCompleted.Count);
		foreach(KeyValuePair<string, bool> kvp in m_recentCompleted)
		{
			bw.Write(kvp.Key);
			bw.Write(kvp.Value);
		}

		bw.Write(m_lastLetterUnlocked.Count);
		foreach(KeyValuePair<string, int> kvp in m_lastLetterUnlocked)
		{
			bw.Write(kvp.Key);
			bw.Write(kvp.Value);
		}

		bw.Write(m_sessionsFinished.Count);
		foreach(KeyValuePair<string, HashSet<int>> kvp in m_sessionsFinished)
		{
			bw.Write(kvp.Key);

			bw.Write(kvp.Value.Count);
			foreach(int i in kvp.Value)
			{
				bw.Write(i);
			}
		}
		
		ds.Save(newData);
		
		bw.Close();
		newData.Close();
	}

	public void SetCurrentSessionNum(int sessionNum)
	{
		m_currentSessionNum = sessionNum;
	}
	
	public int GetSectionsComplete() 
	{
		return m_sectionsComplete;
	}
	
	public void SetSectionsComplete(int sectionsComplete) 
	{
		m_sectionsComplete = sectionsComplete;
	}

	public void SetSections(List<DataRow> sections) 
	{
		m_sections = sections;

		for(int i = 0; i < m_sections.Count; ++i)
		{
			Debug.Log("m_sections[" + i + "]: " + m_sections[i]["id"].ToString());
		}

		Debug.Log("Printing game names");
		foreach(DataRow section in m_sections)
		{
			DataRow game = DataHelpers.FindGameForSection(section);
			if(game != null)
			{
				Debug.Log(game["name"].ToString());
			}
			else
			{
				Debug.Log("No game for section " + section["id"].ToString());
			}
		}
	}

	public DataRow GetCurrentSection()
	{
		Debug.Log("m_sectionsComplete: " + m_sectionsComplete);

		if(m_sectionsComplete < m_sections.Count)
		{
			Debug.Log("Current Section: " + m_sections[m_sectionsComplete]["id"].ToString());

			return m_sections[m_sectionsComplete];
		}
		else
		{
			Debug.LogError("There are not enough sections in m_sections");
			return null;
		}
	}

	public int GetCurrentSectionId()
	{
		Debug.Log("m_sectionsComplete: " + m_sectionsComplete);
		if(m_sectionsComplete < m_sections.Count)
		{
			Debug.Log("current sectionId: " + m_sections[m_sectionsComplete]["id"].ToString());
			return System.Convert.ToInt32(m_sections[m_sectionsComplete]["id"]);
		}
		else
		{
			Debug.LogError("There are not enough sections in m_sections");
			return -1;
		}
	}
	
	public void OnGameFinish(bool wonGame = true)
	{
		Debug.Log("OnGameFinish()");
		if(wonGame)
		{
			++m_sectionsComplete;
		}

		PlayNextGame();
	}

	public void PlayNextGame()
	{
		string gameName = null;

		Debug.Log("Sections Complete: " + m_sectionsComplete);
		for(int i = m_sectionsComplete; i < m_sections.Count; ++i)
		{
			Debug.Log("Finding game for sectionId: " + m_sections[i]["id"].ToString());

			DataRow game = DataHelpers.FindGameForSection(m_sections[i]);
			
			if(game != null)
			{
				string dbGameName = game["name"].ToString();

				Debug.Log("Found dbGame: " + dbGameName);

				if(m_gameNames.ContainsKey(dbGameName))
				{
					gameName = m_gameNames[dbGameName];

					Debug.Log("Linked to scene game: " + gameName);

					break;
				}
				else
				{
					Debug.Log(dbGameName + " is not linked");
					++m_sectionsComplete;
				}
			}
			else
			{
				++m_sectionsComplete;
			}
		}
		
		if(gameName != null)
		{
			Debug.Log("Next Game: " + gameName);
			TransitionScreen.Instance.ChangeLevel(gameName, false);
		}
		else
		{
			Debug.Log("No more games");

			if(m_currentSessionNum > GetSessionsCompleted())
			{
				SetSessionsCompleted(m_currentSessionNum);
				TTInformation.Instance.SetGoldCoins(TTInformation.Instance.GetGoldCoins() + 1);
				SetRecentlyCompleted(true);
			}

			TransitionScreen.Instance.ChangeLevel("NewVoyage", true);
		}
	}
}
*/
