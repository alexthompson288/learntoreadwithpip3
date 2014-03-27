using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveCurrentLesson : MonoBehaviour 
{
#if UNITY_IPHONE
	Dictionary<string, string> m_eventParameters = new Dictionary<string, string>();
#endif
	
	void OnClick()
	{
		Debug.Log("Saving lesson");
		LessonNameCoordinator.Instance.OnInputFinish();
		
		#if UNITY_IPHONE
		m_eventParameters.Clear();

		m_eventParameters.Add("Name", LessonInfo.Instance.GetName());

		List<string> gameNamesList = LessonInfo.Instance.GetGames();
		for(int i = 0; i < gameNamesList.Count; ++i)
		{
			m_eventParameters.Add("Game " + i.ToString(), gameNamesList[i]);
		}
		#endif
		
		AddMissingData(Game.Data.Phonemes, "setphonemes", "phonemes");
		AddMissingData(Game.Data.Words, "setwords", "words");
		AddMissingData(Game.Data.Keywords, "setkeywords", "words");
		AddMissingData(Game.Data.Stories, "setstories", "stories");

#if UNITY_IPHONE
		FlurryBinding.logEventWithParameters("SaveLesson", m_eventParameters, false);
#endif
		
		LessonInfo.Instance.SaveLessons();
	}
	
	void AddMissingData(Game.Data dataType, string columnName, string tableName)
	{
		List<DataRow> data = LessonInfo.Instance.GetData(dataType);
		
		if(data.Count == 0)
		{
			#if UNITY_IPHONE
			m_eventParameters.Add(dataType.ToString(), "NO DATA");
			#endif
			
			Debug.Log("Adding: " + columnName.Replace("set", ""));
			
			int setNum = 1;
			
			while(data.Count == 0 && setNum < 50)
			{
				data = GameDataBridge.Instance.GetSetData(setNum, columnName, tableName);
				++setNum;
			}

			if(dataType != Game.Data.Stories)
			{
				foreach(DataRow datum in data)
				{
					LessonInfo.Instance.AddData(System.Convert.ToInt32(datum["id"]), dataType);
				}
			}
			else
			{
				Debug.Log("Adding story: " + data[0]["title"].ToString());
				LessonInfo.Instance.AddData(System.Convert.ToInt32(data[0]["id"]), dataType);
			}
		}
		else
		{
			#if UNITY_IPHONE
			string attribute = "word";
			
			if(dataType == Game.Data.Phonemes)
			{
				attribute = "phoneme";
			}
			else if(dataType == Game.Data.Stories)
			{
				attribute = "title";
			}

			for(int i = 0; i < data.Count; ++i)
			{
				m_eventParameters.Add(dataType.ToString() + "_" + i.ToString(), data[i][attribute].ToString());
			}
			#endif
		}
	}
}

/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveCurrentLesson : MonoBehaviour 
{
	Dictionary<string, string> m_eventParameters = new Dictionary<string, string>();

	void OnClick()
	{
		m_eventParameters.Clear();

		Debug.Log("Saving lesson");
		LessonNameCoordinator.Instance.OnInputFinish();

#if UNITY_IPHONE
		Dictionary<string, string> ep = new Dictionary<string, string>();
		ep.Add("LessonName", LessonInfo.Instance.GetName());
		FlurryBinding.logEvent("NewLesson", false);

		Dictionary<string, string> gameNamesDictionary = new Dictionary<string, string>();
		List<string> gameNamesList = LessonInfo.Instance.GetGames();
		for(int i = 0; i < gameNamesList.Count - 1; i += 2)
		{
			string s1 = gameNamesList[i];
			string s2 = (i + 1) < gameNamesList.Count ? gameNamesList[i + 1] : "DefaultPastIndex";
		}

		FlurryBinding.logEventWithParameters("Lesson games", gameNamesDictionary, false);
#endif

		AddMissingData(Game.Data.Phonemes, "setphonemes", "phonemes");
		AddMissingData(Game.Data.Words, "setwords", "words");
		AddMissingData(Game.Data.Keywords, "setkeywords", "words");
		AddMissingData(Game.Data.Stories, "setstories", "stories");

		LessonInfo.Instance.SaveLessons();
	}

	void AddMissingData(Game.Data dataType, string columnName, string tableName)
	{
		List<DataRow> data = LessonInfo.Instance.GetData(dataType);
		
		if(data.Count == 0)
		{
#if UNITY_IPHONE
			FlurryBinding.logEvent("No lesson data for " + dataType, false);
#endif

			Debug.Log("Adding: " + columnName.Replace("set", ""));
			
			int setNum = 1;
			
			while(data.Count == 0 && setNum < 50)
			{
				data = GameDataBridge.Instance.GetSetData(setNum, columnName, tableName);
				++setNum;
			}
			
			foreach(DataRow datum in data)
			{
				string attributeName = (dataType == Game.Data.Phonemes) ? "phoneme" : "word";

				Debug.Log(datum[attributeName]);

				LessonInfo.Instance.AddData(System.Convert.ToInt32(datum["id"]), dataType);
			}
		}
		else
		{
#if UNITY_IPHONE
			string attribute = "word";

			if(dataType == Game.Data.Phonemes)
			{
				attribute = "phoneme";
			}
			else if(dataType == Game.Data.Stories)
			{
				attribute = "title";
			}

			Dictionary<string, string> dataDictionary = new Dictionary<string, string>();
			for(int i = 0; i < data.Count - 1; i += 2)
			{
				try
				{
					string s1 = data[i][attribute].ToString();
				}
				catch
				{
					if(data[i][attribute] == null)
					{
						Debug.LogError(string.Format("ID {0} has no {1} attribute", data[i]["id"].ToString(), attribute));
					}
					else
					{
						Debug.LogError("Error logging data for " + dataType);
					}
				}

				string s2 = (i + 1) < data.Count ? data[i + 1][attribute].ToString() : "DefaultPastIndex";
			}

			FlurryBinding.logEventWithParameters("Lesson data for " + dataType, dataDictionary, false);
#endif
		}
	}
}
*/
