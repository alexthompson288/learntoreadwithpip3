using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveCurrentLesson : MonoBehaviour 
{
	void OnClick()
	{
		Debug.Log("Saving lesson");
		LessonNameCoordinator.Instance.OnInputFinish();

#if UNITY_IPHONE
		FlurryBinding.logEvent("New Lesson: " + LessonInfo.Instance.GetName(), false);

		Dictionary<string, string> gameNamesDictionary = new Dictionary<string, string>();
		List<string> gameNamesList = LessonInfo.Instance.GetGames();
		for(int i = 0; i < gameNamesList.Count - 1; i += 2)
		{
			string s1 = gameNamesList[i];
			string s2 = (i + 1) < gameNamesList.Count ? gameNamesList[i + 1] : "DefaultPastIndex";
		}

		FlurryBinding.logEventWithParameters("Lesson games", gameNamesDictionary, false);
#endif

		AddMissingData(LessonInfo.DataType.Letters, "setphonemes", "phonemes");
		AddMissingData(LessonInfo.DataType.Words, "setwords", "words");
		AddMissingData(LessonInfo.DataType.Keywords, "setkeywords", "words");
		AddMissingData(LessonInfo.DataType.Stories, "setstories", "stories");

		LessonInfo.Instance.SaveLessons();
	}

	void AddMissingData(LessonInfo.DataType dataType, string columnName, string tableName)
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
				string attributeName = (dataType == LessonInfo.DataType.Letters) ? "phoneme" : "word";

				Debug.Log(datum[attributeName]);

				LessonInfo.Instance.AddData(System.Convert.ToInt32(datum["id"]), dataType);
			}
		}
		else
		{
#if UNITY_IPHONE
			string attribute = "word";

			if(dataType == LessonInfo.DataType.Letters)
			{
				attribute = "phoneme";
			}
			else if(dataType == LessonInfo.DataType.Stories)
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
