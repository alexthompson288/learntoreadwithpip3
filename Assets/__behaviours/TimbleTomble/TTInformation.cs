using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TTInformation : Singleton<TTInformation> 
{
	public event MagicChange OnMagicChange;
	public delegate void MagicChange();

	Dictionary<string, int> m_goldCoins = new Dictionary<string, int>();
	Dictionary<string, float> m_magic = new Dictionary<string, float>();
	Dictionary<string, UserGroup> m_unlockableItems = new Dictionary<string, UserGroup>();

	float m_minMagic = 0f;
	float m_maxMagic = 100f;

	bool m_hasLoaded = false;

	bool m_isFirstTimbleTomble = true;

	public class UserGroup
	{
		List<string> m_users = new List<string>();

		public void AddUser(string newUser)
		{
			if(!m_users.Contains(newUser))
			{
				m_users.Add(newUser);
			}
		}

		public bool HasUser(string user)
		{
			return m_users.Contains(user);
		}

		public List<string> GetUsers()
		{
			return m_users;
		}
	}

	void Awake ()
	{
		Load();

		List<string> unlockableItems = new List<string>();

		Object[] unlockableObjects = Resources.LoadAll("tt_stickers");
		for(int i = 0; i < unlockableObjects.Length; ++i)
		{
			unlockableItems.Add(unlockableObjects[i].name);
		}

		Object[] unlockableAnimatedObjects = Resources.LoadAll("tt_animatedStickers");
		for(int i = 0; i < unlockableAnimatedObjects.Length; ++i)
		{
			unlockableItems.Add(unlockableAnimatedObjects[i].name);
		}

		Object[] unlockableBackgroundObjects = Resources.LoadAll("tt_backgrounds");
		for(int i = 0; i < unlockableBackgroundObjects.Length; ++i)
		{
			unlockableItems.Add(unlockableBackgroundObjects[i].name);
		}

		foreach(string item in unlockableItems)
		{
			if(!m_unlockableItems.ContainsKey(item))
			{
				m_unlockableItems.Add(item, new UserGroup());
			}
		}

		List<string> itemsToRemove = new List<string>();

		foreach(KeyValuePair<string, UserGroup> item in m_unlockableItems)
		{
			if(!unlockableItems.Contains(item.Key))
			{
				itemsToRemove.Add(item.Key);
			}
		}

		foreach(string item in itemsToRemove)
		{
			Debug.Log("Removing " + item + " because it is no longer found in Resources");
			m_unlockableItems.Remove(item);
		}

		Save ();

		m_hasLoaded = true;
	}

	// Use this for initialization
	void Start () 
	{
		Resources.UnloadUnusedAssets();
	}

#if UNITY_EDITOR
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Y))
		{
			SetMagic(9);
		}
		else if(Input.GetKeyDown(KeyCode.U))
		{
			SetMagic(29);
		}
		else if(Input.GetKeyDown(KeyCode.I))
		{
			SetMagic(39);
		}
		else if(Input.GetKeyDown(KeyCode.O))
		{
			SetMagic(50);
		}
		else if(Input.GetKeyDown(KeyCode.P))
		{
			SetMagic(100);
		}
	}
#endif

	public int GetGoldCoins()
	{
		string user = ChooseUser.Instance.GetCurrentUser();

		if(!m_goldCoins.ContainsKey(user))
		{
			m_goldCoins[user] = 2;
		}

		return m_goldCoins[user];
	}

	public void SetGoldCoins(int numCoins)
	{
		m_goldCoins[ChooseUser.Instance.GetCurrentUser()] = numCoins;
		Save();
	}

	public float GetMagic()
	{
		string user = ChooseUser.Instance.GetCurrentUser();

		if(!m_magic.ContainsKey(user))
		{
			m_magic[user] = 50;
		}

		return m_magic[user];
	}

	public void SetMagic(float level)
	{
		level = Mathf.Clamp(level, m_minMagic, m_maxMagic);
		m_magic[ChooseUser.Instance.GetCurrentUser()] = level;
		Save();

		if(OnMagicChange != null)
		{
			OnMagicChange();
		}
	}

	public bool IsItemUnlocked(string user, string item)
	{
		if(m_unlockableItems.ContainsKey(item))
		{
			return m_unlockableItems[item].HasUser(user);
		}
		else
		{
			return false;
		}
	}

	public void UnlockItem(string item)
	{
		m_unlockableItems[item].AddUser(ChooseUser.Instance.GetCurrentUser());
		Save();
	}

	void Load()
	{
		DataSaver ds = new DataSaver("TimbleTombleInformation");
		MemoryStream data = ds.Load();
		BinaryReader br = new BinaryReader(data);
		
		if (data.Length != 0)
		{
			int numCoinsUsers = br.ReadInt32();
			for (int i = 0; i < numCoinsUsers; ++i)
			{
				string user = br.ReadString();
				int numCoins = br.ReadInt32();
				m_goldCoins[user] = numCoins;
			}

			
			int numMagicUsers = br.ReadInt32();
			for (int i = 0; i < numMagicUsers; ++i)
			{
				string user = br.ReadString();
				float magic = br.ReadSingle();
				m_magic[user] = magic;
			}
			
			int numUnlockableItems = br.ReadInt32();
			for (int i = 0; i < numUnlockableItems; ++i)
			{
				string item = br.ReadString();

				if(!m_unlockableItems.ContainsKey(item))
				{
					m_unlockableItems.Add(item, new UserGroup());
				}

				int numItemUsers = br.ReadInt32();
				for (int j = 0; j < numItemUsers; ++j)
				{
					m_unlockableItems[item].AddUser(br.ReadString());
				}
			}
		}
		br.Close();
		data.Close();
	}

	void Save()
	{
		DataSaver ds = new DataSaver("TimbleTombleInformation");
		MemoryStream newData = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(newData);
		
		bw.Write(m_goldCoins.Count);
		foreach (KeyValuePair<string, int> kvp in m_goldCoins)
		{
			bw.Write(kvp.Key);
			bw.Write(kvp.Value);
		}
		
		bw.Write(m_magic.Count);
		foreach (KeyValuePair<string, float> kvp in m_magic)
		{
			bw.Write(kvp.Key);
			bw.Write(kvp.Value);
		}
		
		bw.Write(m_unlockableItems.Count);
		foreach (KeyValuePair<string, UserGroup> kvp in m_unlockableItems)
		{
			bw.Write(kvp.Key);

			List<string> itemUsers = kvp.Value.GetUsers();
			bw.Write(itemUsers.Count);
			foreach(string s in itemUsers)
			{
				bw.Write(s);
			}
		}
		
		ds.Save(newData);
		
		bw.Close();
		newData.Close();
	}

	bool HasLoaded()
	{
		return m_hasLoaded;
	}

	public static IEnumerator WaitForLoad()
	{
		while(TTInformation.Instance == null)
		{
			Debug.Log("Waiting for singleton");
			yield return null;
		}
		while(!TTInformation.Instance.HasLoaded())
		{
			Debug.Log("Waiting for TTInformation to load");
			yield return null;
		}
	}
}
