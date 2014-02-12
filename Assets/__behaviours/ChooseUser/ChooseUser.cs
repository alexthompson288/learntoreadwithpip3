using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ChooseUser : Singleton<ChooseUser> 
{
	string m_currentUser; 

	Dictionary<string, string> m_users = new Dictionary<string, string>();

	void Awake()
	{		 
		Load();

		if(m_users.Count == 0)
		{
			string newUser = "Pip";
			m_currentUser = newUser;
			CreateUser(newUser, "pip_state_b");
			Save ();
		}
	}

	public string GetCurrentUser ()
	{
		if(m_currentUser == null)
		{
			m_currentUser = "Pip";
		}

		return m_currentUser;
	}

	public Dictionary<string, string> GetUsers()
	{
		return m_users;
	}

	// Use this for initialization
	public void SetCurrentUser (string user) 
	{
		m_currentUser = user;
		Save ();
	}

	public void CreateUser(string user, string imageName)
	{
		m_users[user] = imageName;
		Save ();
	}

	public void DeleteUser(string user)
	{
		m_users.Remove(user);
		Save ();
	}

	public bool HasUser(string user)
	{
		return m_users.ContainsKey(user);
	}

	void Load()
	{
		DataSaver ds = new DataSaver("UserInformation");
		MemoryStream data = ds.Load();
		BinaryReader br = new BinaryReader(data);
		
		if (data.Length != 0)
		{
			int numUsers = br.ReadInt32();
			for(int i = 0; i < numUsers; ++i)
			{
				string user = br.ReadString();
				string imageName = br.ReadString();
				m_users[user] = imageName;
			}

			m_currentUser = br.ReadString();
		}
		br.Close();
		data.Close();
	}
	
	void Save()
	{
		DataSaver ds = new DataSaver("UserInformation");
		MemoryStream newData = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(newData);
		
		bw.Write(m_users.Count);
		foreach (KeyValuePair<string, string> kvp in m_users)
		{
			bw.Write(kvp.Key);
			bw.Write(kvp.Value);
		}

		bw.Write(m_currentUser);
		
		ds.Save(newData);
		
		bw.Close();
		newData.Close();
	}
}
