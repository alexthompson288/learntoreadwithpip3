using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PipisodeInfo : Singleton<PipisodeInfo> 
{
	[SerializeField]
	private int[] m_pipisodeIds;
	[SerializeField]
	private int[] m_defaultUnlockedIds;

	HashSet<int> m_unlockedPipisodes = new HashSet<int>(); // Use a separate container, instead of a dictionary, to track unlocked.
	                                                        // Because you cannot assign dictionaries in editor.

	public void UnlockPipisode(int pipisodeId)
	{
		m_unlockedPipisodes.Add(pipisodeId);
		Save ();
	}

	public void UnlockAllPipisodes()
	{
		foreach(int pipisode in m_pipisodeIds)
		{
			m_unlockedPipisodes.Add(pipisode);
			Save ();
		}
	}

	public int[] GetPipisodeIds()
	{
		return m_pipisodeIds;
	}

	public bool IsUnlocked(int pipisodeId)
	{
		return m_unlockedPipisodes.Contains(pipisodeId);
	}

	void Awake()
	{
		foreach(int pipisodeId in m_defaultUnlockedIds)
		{
			m_unlockedPipisodes.Add(pipisodeId);
		}

		Load();
	}

	void Load()
	{
		DataSaver ds = new DataSaver("PipisodeInfo");
		MemoryStream data = ds.Load();
		BinaryReader br = new BinaryReader(data);
		
		if (data.Length != 0)
		{
			int numPipisodes = br.ReadInt32();
			for(int i = 0; i < numPipisodes; ++i)
			{
				m_unlockedPipisodes.Add(br.ReadInt32());
			}
		}
		
		br.Close();
		data.Close();
	}
	
	void Save()
	{
		DataSaver ds = new DataSaver("PipisodeInfo");
		MemoryStream newData = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(newData);
		
		bw.Write(m_unlockedPipisodes.Count);
		foreach(int pipisode in m_unlockedPipisodes)
		{
			bw.Write(pipisode);
		}
		
		ds.Save(newData);
		
		bw.Close();
		newData.Close();
	}
}
