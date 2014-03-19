using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class AlphabetBookInformation : Singleton<AlphabetBookInformation> 
{
	Dictionary<string, PhonemeTexture> m_phonemeTextures = new Dictionary<string, PhonemeTexture>();

	class PhonemeTexture
	{
		Dictionary<int, string> m_textures = new Dictionary<int, string>();

		public void AddTexture(int phonemeId, string textureName)
		{
			m_textures[phonemeId] = textureName;
		}

		public string GetTexture(DataRow phoneme)
		{
			if(m_textures.ContainsKey(Convert.ToInt32(phoneme["id"])))
			{
				return m_textures[Convert.ToInt32(phoneme["id"])];
			}
			else
			{
				return null;
			}
		}

		public string GetTexture(int phonemeId)
		{
			if(m_textures.ContainsKey(phonemeId))
			{
				return m_textures[phonemeId];
			}
			else
			{
				return null;
			}
		}


		public Dictionary<int, string> GetAllTextures()
		{
			return m_textures;
		}
	}

	void Awake()
	{
		Load();
	}

	public void AddTexture(DataRow phoneme, string textureName)
	{
		string user = UserInfo.Instance.GetCurrentUser();

		if(!m_phonemeTextures.ContainsKey(user))
		{
			m_phonemeTextures.Add(user, new PhonemeTexture());

		}

		m_phonemeTextures[user].AddTexture(Convert.ToInt32(phoneme["id"]), textureName);

		Save ();
	}

	public string GetTexture(int phonemeId)
	{
		string user = UserInfo.Instance.GetCurrentUser();

		if(m_phonemeTextures.ContainsKey(user))
		{
			return m_phonemeTextures[user].GetTexture(phonemeId);
		}
		else
		{
			return null;
		}
	}

	public Dictionary<int, string> GetAllTextures()
	{
		string user = UserInfo.Instance.GetCurrentUser();

		if(m_phonemeTextures.ContainsKey(user))
		{
			return m_phonemeTextures[user].GetAllTextures();
		}
		else
		{
			return new Dictionary<int, string>();
		}
	}

	void Load()
	{
		DataSaver ds = new DataSaver("PhonemeTextures");
		MemoryStream data = ds.Load();
		BinaryReader br = new BinaryReader(data);
		
		if (data.Length != 0)
		{
			int numUsers = br.ReadInt32();
			for(int i = 0; i < numUsers; ++i)
			{
				string user = br.ReadString();

				PhonemeTexture phonemeTexture = new PhonemeTexture();
				int numPhonemes = br.ReadInt32();
				for(int j = 0; j < numPhonemes; ++j)
				{
					int phonemeId = br.ReadInt32();
					string textureName = br.ReadString();
					phonemeTexture.AddTexture(phonemeId, textureName);
				}

				m_phonemeTextures[user] = phonemeTexture;
			}
		}
		br.Close();
		data.Close();
	}
	
	void Save()
	{
		DataSaver ds = new DataSaver("PhonemeTextures");
		MemoryStream newData = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(newData);
		
		bw.Write(m_phonemeTextures.Count);
		foreach (KeyValuePair<string, PhonemeTexture> userPair in m_phonemeTextures)
		{
			bw.Write(userPair.Key);

			Dictionary<int, string> phonemeTextures = userPair.Value.GetAllTextures();

			bw.Write(phonemeTextures.Count);
			foreach(KeyValuePair<int, string> phonemePair in phonemeTextures)
			{
				bw.Write(phonemePair.Key);
				bw.Write(phonemePair.Value);
			}
		}
		ds.Save(newData);
		
		bw.Close();
		newData.Close();
	}
}
