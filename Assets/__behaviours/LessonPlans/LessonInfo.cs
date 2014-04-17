﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class LessonInfo : Singleton<LessonInfo> 
{  	
	List<Lesson> m_lessons = new List<Lesson>();
	
	Lesson m_currentLesson;

	int m_currentGame;

#if UNITY_EDITOR
	[SerializeField]
	private bool m_instantiateDebugLesson;
	[SerializeField]
	private bool m_overwriteLessons;
#endif

    public List<string> GetScenes()
    {
        return m_currentLesson != null ? m_currentLesson.GetGames() : new List<string>();
    }

	public string GetSceneName(int index)
	{
		return index < m_currentLesson.GetNumGames () ? m_currentLesson.GetGame (index) : "";
	}

	void Awake()
	{
		Load();

#if UNITY_EDITOR
		if(m_instantiateDebugLesson)
		{
			// TODO: Delete this lesson instantion. Only necessary so that I don't need to change scenes during development
			if(m_lessons.Count == 0)
			{
				Debug.Log("Instantiating debug lesson");
				m_lessons.Add(new Lesson());
			}
			
			m_currentLesson = m_lessons[0];
		}
		if(m_overwriteLessons)
		{
			m_lessons.Clear();
			Save ();
		}
#endif
	}
	
	public void InstantiateLesson()
	{
		m_currentLesson = new Lesson();
		m_lessons.Add(m_currentLesson);
	}

	public void DeleteCurrentLesson()
	{
		m_lessons.Remove(m_currentLesson);
		m_currentLesson = null;
		System.GC.Collect();
	}

	public void SetCurrentLesson(int i)
	{
		Debug.Log("Lessons Count: " + m_lessons.Count);
		Debug.Log("index: " + i);
		m_currentLesson = m_lessons[i];
	}
	
	public List<string> GetLessonNames()
	{
		Debug.Log("There are " + m_lessons.Count + " lessons");
		List<string> lessonNames = new List<string>();
		foreach(Lesson lesson in m_lessons)
		{
			Debug.Log(lesson.GetName());
			lessonNames.Add(lesson.GetName());
		}
		
		return lessonNames;
	}

	public void SaveLessons()
	{
		foreach(Lesson lesson in m_lessons)
		{
			lesson.RemoveNullGames();
		}
		
		Save ();
	}

	class Lesson
	{
		string m_name;
		
		List<string> m_games = new List<string>();
		
		HashSet<int> m_letters = new HashSet<int>();
		int m_targetLetter = -1;
		
		HashSet<int> m_words = new HashSet<int>();
		int m_targetWord = -1;
		
		HashSet<int> m_keywords = new HashSet<int>();
		int m_targetKeyword = -1;

		HashSet<int> m_stories = new HashSet<int>();

		public Lesson()
		{
			m_name = "Default Lesson";
			
			m_targetLetter = -1;
			m_targetWord = -1;
			m_targetKeyword = -1;
		}
		
		public string GetName()
		{
			return m_name;
		}
		
		public void SetName(string name)
		{
			m_name = name;
		}
		
		public void AddGame(string game, int index)
		{
			if(index > m_games.Count)
			{
				Debug.LogError("Adding a game at an index greater than m_games.Count");
			}

			if(index == -1 || index >= m_games.Count)
			{
				m_games.Add(game);
			}
			else
			{
				m_games[index] = game;
			}
		}
		
		public void RemoveGame(int index)
		{
			if(index == m_games.Count - 1)
			{
				m_games.RemoveAt(m_games.Count - 1);
			}
			else
			{
				m_games[index] = null;
			}
		}
		
		public void RemoveNullGames() // TODO: Make sure this method works
		{
			m_games.RemoveAll(System.String.IsNullOrEmpty);
		}
		
		public List<string> GetGames()
		{
			return m_games;
		}
		
		public string GetGame(int index)
		{
			return m_games[index];
		}

		public int GetNumGames()
		{
			return m_games.Count;
		}
		
		public string ToggleData(int id, Game.Data dataType)
		{
			if(dataType == Game.Data.Phonemes)
			{
				if(m_targetLetter == id)
				{
					m_targetLetter = -1;
					m_letters.Remove(id);
					return "Remove";
				}
				else if(m_letters.Contains(id))
				{
					m_targetLetter = id;
					return "Target";
				}
				else
				{
					m_letters.Add(id);
					return "Add";
				}
			}
			else if(dataType == Game.Data.Words)
			{
				if(m_targetWord == id)
				{
					m_targetWord = -1;
					m_words.Remove(id);
					return "Remove";
				}
				else if(m_words.Contains(id))
				{
					m_targetWord = id;
					return "Target";
				}
				else
				{
					m_words.Add(id);
					return "Add";
				}
			}
			else
			{
				if(m_targetKeyword == id)
				{
					m_targetKeyword = -1;
					m_keywords.Remove(id);
					return "Remove";
				}
				else if(m_keywords.Contains(id))
				{
					m_targetKeyword = id;
					return "Target";
				}
				else
				{
					m_keywords.Add(id);
					return "Add";
				}
			}
		}

		public bool IsTarget(int id, Game.Data dataType)
		{
			if(dataType == Game.Data.Phonemes)
			{
				return m_targetLetter == id;
			}
			else if(dataType == Game.Data.Words)
			{
				return m_targetWord == id;
			}
			else
			{
				return m_targetKeyword == id;
			}
		}

		public bool HasData(int id, Game.Data dataType)
		{
			if(dataType == Game.Data.Phonemes)
			{
				return m_letters.Contains(id);
			}
			else if(dataType == Game.Data.Words)
			{
				return m_words.Contains(id);
			}
			else if(dataType == Game.Data.Keywords)
			{
				return m_keywords.Contains(id);
			}
			else
			{
				return m_stories.Contains(id);
			}
		}
		
		public HashSet<int> GetData(Game.Data dataType)
		{
			if(dataType == Game.Data.Phonemes)
			{
				return m_letters;
			}
			else if(dataType == Game.Data.Words)
			{
				return m_words;
			}
			else if(dataType == Game.Data.Keywords)
			{
				return m_keywords;
			}
			else
			{
				Debug.Log("Lesson.GetData() - Stories");
				return m_stories;
			}
		}
		
		public int GetTargetData(Game.Data dataType)
		{
			if(dataType == Game.Data.Phonemes)
			{
				return m_targetLetter;
			}
			else if(dataType == Game.Data.Words)
			{
				return m_targetWord;
			}
			else
			{
				return m_targetKeyword;
			}
		}

		public void AddData(int id, Game.Data dataType)
		{
			if(dataType == Game.Data.Phonemes)
			{
				m_letters.Add(id);
			}
			else if(dataType == Game.Data.Words)
			{
				m_words.Add(id);
			}
			else if(dataType == Game.Data.Keywords)
			{
				m_keywords.Add(id);
			}
			else
			{
				Debug.Log("Lesson.AddData(" + id + ")");
				m_stories.Add(id);
			}
		}
		
		public void RemoveData(int id, Game.Data dataType)
		{
			if(dataType == Game.Data.Phonemes)
			{
				m_letters.Remove(id);
				
				if(m_targetLetter == id)
				{
					m_targetLetter = -1;
				}
			}
			else if(dataType == Game.Data.Words)
			{
				m_words.Remove(id);
				
				if(m_targetWord == id)
				{
					m_targetWord = -1;
				}
			}
			else if(dataType == Game.Data.Keywords)
			{
				m_keywords.Remove(id);
				
				if(m_targetKeyword == id)
				{
					m_targetKeyword = -1;
				}
			}
			else
			{
				m_stories.Remove(id);
			}
		}

		public void ClearData(Game.Data dataType)
		{
			if(dataType == Game.Data.Phonemes)
			{
				m_letters.Clear();
			}
			else if(dataType == Game.Data.Words)
			{
				m_words.Clear();
			}
			else if(dataType == Game.Data.Keywords)
			{
				m_keywords.Clear();
			}
			else
			{
				m_stories.Clear();
			}
		}
	}
	
	public string GetName()
	{
		return m_currentLesson.GetName();
	}
	
	public void SetName(string name)
	{
		m_currentLesson.SetName(name);
	}
	
	public string GetGame(int i)
	{
		return m_currentLesson.GetGame(i);
	}

	public List<string> GetGames() // TODO: Delete. This method is only for debugging.
	{
		return m_currentLesson.GetGames();
	}
	
	public void AddGame(string game, int index = -1)
	{
		m_currentLesson.AddGame(game, index);
	}
	
	public void RemoveGame(int index)
	{
		m_currentLesson.RemoveGame(index);
	}

	public string ToggleData(int id, Game.Data dataType)
	{
		return m_currentLesson.ToggleData(id, dataType);
	}

	public bool IsTarget(int id, Game.Data dataType)
	{
		return m_currentLesson.IsTarget(id, dataType);
	}

	public bool HasData(int id, Game.Data dataType)
	{
		return m_currentLesson.HasData(id, dataType);
	}

	public List<DataRow> GetData(Game.Data dataType)
	{
		Debug.Log("LessonInfo.GetData()");

		HashSet<int> ids = m_currentLesson.GetData(dataType);

		Debug.Log("There are " + ids.Count + " " + dataType);
		
		//string dataName = (dataType == Game.Data.Phonemes) ? "phonemes" : "words";

		string dataName = "words";

		if(dataType == Game.Data.Phonemes)
		{
			dataName = "phonemes";
		}
		else if(dataType == Game.Data.Stories)
		{
			dataName = "stories";
		}
		
		List<DataRow> data = new List<DataRow>();
		
		SqliteDatabase db = GameDataBridge.Instance.GetDatabase();
		
		foreach(int id in ids)
		{
			DataTable dt = db.ExecuteQuery("select * from " + dataName + " WHERE id=" + id);
			
			if(dt.Rows.Count > 0)
			{
				data.Add(dt.Rows[0]);
			}
		}

		Debug.Log(System.String.Format("Found {0} id matches", data.Count));
		
		return data;
	}

	public List<int> GetDataIds(Game.Data dataType)
	{
		return m_currentLesson.GetData(dataType).ToList();
	}
	
	public DataRow GetTargetData(Game.Data dataType)
	{
		DataRow targetData = null;
		
		int id = m_currentLesson.GetTargetData(dataType);
		
		if(id != -1)
		{
			string dataName = (dataType == Game.Data.Phonemes) ? "phonemes" : "words";
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from " + dataName + " WHERE id=" + id);
			
			if(dt.Rows.Count > 0)
			{
				targetData = dt.Rows[0];
			}
		}
		
		return targetData;
	}

	public int GetTargetId(Game.Data dataType)
	{
		return m_currentLesson.GetTargetData(dataType);
	}

	public void AddData(int id, Game.Data dataType)
	{
		m_currentLesson.AddData(id, dataType);
	}

	public void RemoveData(int id, Game.Data dataType)
	{
		m_currentLesson.RemoveData(id, dataType);
	}

	public void ClearData(Game.Data dataType)
	{
		m_currentLesson.ClearData(dataType);
	}

	void Load()
	{
		DataSaver ds = new DataSaver("LessonInfo");
		MemoryStream data = ds.Load();
		BinaryReader br = new BinaryReader(data);

		if(data.Length != 0)
		{
			int numLessons = br.ReadInt32();
			for(int i = 0; i < numLessons; ++i)
			{
				Lesson lesson = new Lesson();

				lesson.SetName(br.ReadString());
				
				int numGames = br.ReadInt32();
				for(int j = 0; j < numGames; ++j)
				{
					lesson.AddGame(br.ReadString(), -1); // TODO: Make sure that this works with null strings
				}
				
				for(int j = 0; j < 3; ++j)
				{
					int numData = br.ReadInt32();
					for(int k = 0; k < numData; ++k)
					{
						lesson.ToggleData(br.ReadInt32(), (Game.Data) j); // Cast j to the Game.Data enum
					}
					
					lesson.ToggleData(br.ReadInt32(), (Game.Data) j); // Target Data. Set as target because added in above for loop
				}

				m_lessons.Add(lesson);
			}
		}
		
		br.Close();
		data.Close();
	}
	
	void Save()
	{
		DataSaver ds = new DataSaver("LessonInfo");
		MemoryStream newData = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(newData);
		
		bw.Write(m_lessons.Count);
		foreach(Lesson lesson in m_lessons)
		{
			bw.Write(lesson.GetName());

			List<string> games = lesson.GetGames();
			bw.Write(games.Count);
			foreach(string game in games)
			{
				bw.Write(game);
			}
			
			for(int i = 0; i < 3; ++i)
			{
				HashSet<int> data = lesson.GetData((Game.Data)i); // Cast i to the Game.Data enum
				bw.Write(data.Count);
				foreach(int id in data)
				{
					bw.Write(id);
				}
				
				bw.Write(lesson.GetTargetData((Game.Data)i)); // Cast i to the Game.Data enum
			}
		}
		
		ds.Save(newData);
		
		bw.Close();
		newData.Close();
	}
}