using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SkillProgressInformation : Singleton<SkillProgressInformation> 
{
	[SerializeField]
	private int m_starsPerLevel;

	HashSet<string> m_recentlyLeveledSkills = new HashSet<string>();

	public void AddCurrentSkillRecentlyLeveled()
	{
		m_recentlyLeveledSkills.Add(m_currentSkill);
	}

	public void RemoveCurrentSkillRecentlyLeveled()
	{
		if(m_recentlyLeveledSkills.Contains(m_currentSkill))
		{
			m_recentlyLeveledSkills.Remove(m_currentSkill);
		}
	}

	public bool IsCurrentSkillRecentlyLeveled()
	{
		return m_recentlyLeveledSkills.Contains(m_currentSkill);
	}

	void Awake()
	{
		Load ();
	}

	public int GetStarsPerLevel()
	{
		return m_starsPerLevel;
	}

	string m_currentSkill = "dummySkill";

	public string GetCurrentSkill()
	{
		return m_currentSkill;
	}

	public void SetCurrentSkill(string currentSkill)
	{
		m_currentSkill = currentSkill;
	}

	public int GetCurrentSkillProgress()
	{
		return GetProgress(m_currentSkill);
	}

	string m_currentStarSkill;

	public void SetCurrentStarSkill(string currentStarSkill)
	{
		m_currentStarSkill = currentStarSkill;
	}

	int m_currentLevel = 0;

	public int GetCurrentLevel()
	{
		return m_currentLevel;
	}

	public void SetCurrentLevel(int currentLevel)
	{
		m_currentLevel = currentLevel;
	}

	class Progress 
	{
		Dictionary<string, int> m_progress = new Dictionary<string, int>();
		
		public void SetProgress(string progressName, int progressAmount)
		{
			m_progress[progressName] = progressAmount;
		}
		
		public int GetProgress(string progressName)
		{
			if(!m_progress.ContainsKey(progressName))
			{
				m_progress[progressName] = 0;
			}

			//Debug.Log("Progress: " + progressName + " - " + m_progress[progressName]);
			
			return m_progress[progressName];
		}

		public Dictionary<string, int> GetAllProgress()
		{
			return m_progress;
		}
	}

	Dictionary<string, Progress> m_userProgress = new Dictionary<string, Progress>();

	public int GetProgress (string progressName) 
	{
		string user = UserInfo.Instance.GetCurrentUser();

		if(!m_userProgress.ContainsKey(user))
		{
			m_userProgress.Add(user, new Progress());
		}

		return m_userProgress[user].GetProgress(progressName);
	}

	public void SetProgress (string progressName, int progressAmount)
	{
		string user = UserInfo.Instance.GetCurrentUser();
		
		if(!m_userProgress.ContainsKey(user))
		{
			m_userProgress.Add(user, new Progress());
		}

		m_userProgress[user].SetProgress(progressName, progressAmount);

		Save ();
	}

	public int IncrementProgress (string progressName) // TODO: This method should probably return void
	{
		Debug.Log("Incrementing progress for " + progressName);

		string user = UserInfo.Instance.GetCurrentUser();

		if(!m_userProgress.ContainsKey(user))
		{
			m_userProgress.Add(user, new Progress());
		}

		Progress prog = m_userProgress[user];

		prog.SetProgress(progressName, prog.GetProgress(progressName) + 1);

		Save ();

		return prog.GetProgress(progressName);
	}

	public int IncrementCurrentSkillProgress()
	{
		return IncrementProgress(m_currentSkill);
	}

	public int IncrementCurrentStarSkillProgress() // TODO: This method should probably return void
	{
		Debug.Log("IncrementCurrentStarSkillProgress(): " + m_currentStarSkill);
		return IncrementProgress(m_currentStarSkill);
	}

	void Load()
	{
		//Debug.LogError("SkillProgressInformation.Load()");
		DataSaver ds = new DataSaver("SkillProgressInformation");
		MemoryStream data = ds.Load();
		BinaryReader br = new BinaryReader(data);

		if(data.Length != 0)
		{
			int numUsers = br.ReadInt32();
			for(int i = 0; i < numUsers; ++i)
			{
				Progress prog = new Progress();

				int numProgress = br.ReadInt32();
				for(int j = 0; j < numProgress; ++j)
				{
					string progressName = br.ReadString();
					int progressAmount = br.ReadInt32();
					prog.SetProgress(progressName, progressAmount);
				}

				string userName = br.ReadString();

				m_userProgress.Add(userName, prog);
			}
		}

		br.Close();
		data.Close();
	}

	void Save()
	{
		//Debug.LogError("SkillProgressInformation.Save()");
		DataSaver ds = new DataSaver("SkillProgressInformation");
		MemoryStream newData = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(newData);
		
		bw.Write(m_userProgress.Count);
		foreach(KeyValuePair<string, Progress> userKvp in m_userProgress)
		{
			Dictionary<string, int> prog = userKvp.Value.GetAllProgress();

			bw.Write(prog.Count);
			foreach(KeyValuePair<string, int> progKvp in prog)
			{
				bw.Write(progKvp.Key);
				bw.Write(progKvp.Value);
			}

			bw.Write(userKvp.Key);
		}
		
		ds.Save(newData);
		
		bw.Close();
		newData.Close();
	}
}
