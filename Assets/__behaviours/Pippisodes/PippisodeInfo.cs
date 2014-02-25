using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PippisodeInfo : Singleton<PippisodeInfo> 
{
	[SerializeField]
	private int[] m_pippisodeIds;
	[SerializeField]
	private int[] m_defaultUnlockedIds;

	HashSet<int> m_unlockedPippisodes = new HashSet<int>(); // Use a separate container, instead of a dictionary, to track unlocked.
	                                                        // Because you cannot assign dictionaries in editor.

	public void UnlockPippisode(int pippisodeId)
	{
		m_unlockedPippisodes.Add(pippisodeId);
		Save ();
	}

	public void UnlockAllPippisodes()
	{
		foreach(int pippisode in m_pippisodeIds)
		{
			m_unlockedPippisodes.Add(pippisode);
			Save ();
		}
	}

	public int[] GetPippisodeIds()
	{
		return m_pippisodeIds;
	}

	public bool IsUnlocked(int pippisodeId)
	{
		return m_unlockedPippisodes.Contains(pippisodeId);
	}

	void Awake()
	{
		foreach(int pippisodeId in m_defaultUnlockedIds)
		{
			m_unlockedPippisodes.Add(pippisodeId);
		}

		Load();
	}

	void Load()
	{
		DataSaver ds = new DataSaver("PippisodeInfo");
		MemoryStream data = ds.Load();
		BinaryReader br = new BinaryReader(data);
		
		if (data.Length != 0)
		{
			int numPippisodes = br.ReadInt32();
			for(int i = 0; i < numPippisodes; ++i)
			{
				m_unlockedPippisodes.Add(br.ReadInt32());
			}
		}
		
		br.Close();
		data.Close();
	}
	
	void Save()
	{
		DataSaver ds = new DataSaver("PippisodeInfo");
		MemoryStream newData = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(newData);
		
		bw.Write(m_unlockedPippisodes.Count);
		foreach(int pippisode in m_unlockedPippisodes)
		{
			bw.Write(pippisode);
		}
		
		ds.Save(newData);
		
		bw.Close();
		newData.Close();
	}
}
