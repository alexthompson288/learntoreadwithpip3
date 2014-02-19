using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class LessonInfo : Singleton<LessonInfo> 
{
	public enum DataType
	{
		Letters,
		Words,
		Keywords
	}

	List<Lesson> m_lessons = new List<Lesson>();

	Lesson m_currentLesson;

	void Start()
	{
		Load();

		m_currentLesson = m_lessons[0];
	}

	public void SetCurrentLesson(int i)
	{
		m_currentLesson = m_lessons[i];
	}

	public List<string> GetLessonNames()
	{
		List<string> lessonNames = new List<string>();
		foreach(Lesson lesson in m_lessons)
		{
			lessonNames.Add(lesson.GetName());
		}

		return lessonNames;
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
			if(index == -1)
			{
				m_games.Add(game);
			}
			else
			{
				m_games.Insert(index, game);
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

		public void RemoveGame(string game) // TODO: This should be deleted. I have left it here for now just in case I actually need it
		{
			int index = m_games.IndexOf(game);

			if(index == m_games.Count - 1)
			{
				m_games.Remove(game);
			}
			else
			{
				m_games[index] = null;
			}
		}

		public List<string> GetGames()
		{
			return m_games;
		}

		public string GetGame(int index)
		{
			return m_games[index];
		}

		public void AddData(int id, DataType dataType)
		{
			if(dataType == DataType.Letters)
			{
				m_letters.Add(id);
			}
			else if(dataType == DataType.Words)
			{
				m_words.Add(id);
			}
			else
			{
				m_keywords.Add(id);
			}
		}

		public void RemoveData(int id, DataType dataType)
		{
			if(dataType == DataType.Letters)
			{
				m_letters.Remove(id);

				if(m_targetLetter == id)
				{
					m_targetLetter = -1;
				}
			}
			else if(dataType == DataType.Words)
			{
				m_words.Remove(id);

				if(m_targetWord == id)
				{
					m_targetWord = -1;
				}
			}
			else
			{
				m_keywords.Remove(id);

				if(m_targetKeyword == id)
				{
					m_targetKeyword = -1;
				}
			}
		}

		public HashSet<int> GetData(DataType dataType)
		{
			if(dataType == DataType.Letters)
			{
				return m_letters;
			}
			else if(dataType == DataType.Words)
			{
				return m_words;
			}
			else
			{
				return m_keywords;
			}
		}

		public void SetTargetData(int id, DataType dataType)
		{
			if(dataType == DataType.Letters)
			{
				m_targetLetter = id;
				m_letters.Add(id);
			}
			else if(dataType == DataType.Words)
			{
				m_targetWord = id;
				m_words.Add(id);
			}
			else
			{
				m_targetKeyword = id;
				m_keywords.Add(id);
			}
		}

		public int GetTargetData(DataType dataType)
		{
			if(dataType == DataType.Letters)
			{
				return m_targetLetter;
			}
			else if(dataType == DataType.Words)
			{
				return m_targetWord;
			}
			else
			{
				return m_targetKeyword;
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

	public void AddGame(string game, int index = -1)
	{
		m_currentLesson.AddGame(game, index);
	}

	public void RemoveGame(string game)
	{
		m_currentLesson.RemoveGame(game);
	}

	public List<DataRow> GetData(DataType dataType)
	{
		HashSet<int> ids = m_currentLesson.GetData(dataType);

		string dataName = (dataType == DataType.Letters) ? "phonemes" : "words";

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

		return data;
	}

	public void AddData(int id, DataType dataType)
	{
		m_currentLesson.AddData(id, dataType);
	}

	public void RemoveData(int id, DataType dataType)
	{
		m_currentLesson.RemoveData(id, dataType);
	}

	public DataRow GetTargetData(DataType dataType)
	{
		DataRow targetData = null;

		int id = m_currentLesson.GetTargetData(dataType);

		if(id != -1)
		{
			string dataName = (dataType == DataType.Letters) ? "phonemes" : "words";
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from " + dataName + " WHERE id=" + id);

			if(dt.Rows.Count > 0)
			{
				targetData = dt.Rows[0];
			}
		}

		return targetData;
	}

	public void SetTargetData(int id, DataType dataType)
	{
		m_currentLesson.SetTargetData(id, dataType);
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
						lesson.AddData(br.ReadInt32(), (DataType) j); // Cast j to the DataType enum
					}

					lesson.SetTargetData(br.ReadInt32(), (DataType) j); // Cast j to the DataType enum
				}
			}
		}
		
		br.Close();
		data.Close();
	}

	public void SaveLessons()
	{
		foreach(Lesson lesson in m_lessons)
		{
			lesson.RemoveNullGames();
		}

		Save ();
	}

	void Save()
	{
		DataSaver ds = new DataSaver("LessonInfo");
		MemoryStream newData = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(newData);
		
		bw.Write(m_lessons.Count);
		foreach(Lesson lesson in m_lessons)
		{
			List<string> games = lesson.GetGames();
			bw.Write(games.Count);
			foreach(string game in games)
			{
				bw.Write(game);
			}

			for(int i = 0; i < 3; ++i)
			{
				HashSet<int> data = lesson.GetData((DataType)i); // Cast i to the DataType enum
				bw.Write(data.Count);
				foreach(int id in data)
				{
					bw.Write(id);
				}

				bw.Write(lesson.GetTargetData((DataType)i)); // Cast i to the DataType enum
			}
		}
		
		ds.Save(newData);
		
		bw.Close();
		newData.Close();
	}
}
