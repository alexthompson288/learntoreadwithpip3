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

	void Awake()
	{
		#if UNITY_EDITOR
		if(m_overwriteProgress)
		{
			Save();
		}
		#endif
		
		Load ();
	}
	
	void Start()
	{
		SessionManager.Instance.OnSessionCancel += OnSessionCancel;
	}

	void OnSessionComplete()
	{
		Debug.Log("JourneyInformation.OnSessionComplete()");
		
		int sessionNum = SessionManager.Instance.GetSessionNum();
		
		if(sessionNum > GetSessionsCompleted())
		{
			SetSessionsCompleted(sessionNum);
		}

		//ServerPost.Instance.PostVoyageSession (sessionNum);
		
#if UNITY_IPHONE
		Dictionary<string, string> ep = new Dictionary<string, string>();
		ep.Add("CurrentSession", sessionNum.ToString());
		ep.Add("HighestSession", GetSessionsCompleted().ToString());
		
		FlurryBinding.endTimedEvent("StartVoyageSession", ep);
		FlurryBinding.logEventWithParameters("FinishVoyageSession", ep, false);
#endif
		
		SetSessionFinished(sessionNum);
		
		SetRecentlyCompleted(true);
		
		SessionManager.Instance.OnSessionComplete -= OnSessionComplete;
	}

	void OnSessionCancel()
	{
		SessionManager.Instance.OnSessionComplete -= OnSessionComplete;
	}

	public void SubscribeOnSessionComplete()
	{
		SessionManager.Instance.OnSessionComplete += OnSessionComplete;
	}

	// Saved between app close/open
	Dictionary<string, HashSet<int>> m_sessionsFinished = new Dictionary<string, HashSet<int>>();
	Dictionary<string, int> m_sessionsCompleted = new Dictionary<string, int>();
	Dictionary<string, bool> m_recentCompleted = new Dictionary<string, bool>();
	

	
	public bool IsSessionFinished(int sessionNum)
	{
		string user = UserInfo.Instance.GetCurrentUser();
		
		if(!m_sessionsFinished.ContainsKey(user))
		{
			m_sessionsFinished.Add(user, new HashSet<int>());
		}
		
		return m_sessionsFinished[user].Contains(sessionNum);
	}
	
	public void SetSessionFinished(int sessionNum)
	{
		string user = UserInfo.Instance.GetCurrentUser();
		
		if(!m_sessionsFinished.ContainsKey(user))
		{
			m_sessionsFinished.Add(user, new HashSet<int>());
		}
		
		m_sessionsFinished[user].Add(sessionNum);
		
		Save ();
	}
	
	public int GetSessionsCompleted()
	{
		string user = UserInfo.Instance.GetCurrentUser();
		
		if(!m_sessionsCompleted.ContainsKey(user))
		{
			m_sessionsCompleted[user] = 0;
		}
		
		return m_sessionsCompleted[user];
	}
	
	public void SetSessionsCompleted(int session)
	{
		m_sessionsCompleted[UserInfo.Instance.GetCurrentUser()] = session;
		Save ();
	}
	
	public bool HasRecentlyCompleted()
	{
		string user = UserInfo.Instance.GetCurrentUser();
		
		if(!m_recentCompleted.ContainsKey(user))
		{
			m_recentCompleted[user] = false;
		}
		
		return m_recentCompleted[user];
	}
	
	public void SetRecentlyCompleted(bool sessionRecentlyCompleted)
	{
		m_recentCompleted[UserInfo.Instance.GetCurrentUser()] = sessionRecentlyCompleted;
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