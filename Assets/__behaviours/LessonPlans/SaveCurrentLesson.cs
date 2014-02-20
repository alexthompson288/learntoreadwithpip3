using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveCurrentLesson : MonoBehaviour 
{
	void OnClick()
	{
		Debug.Log("Saving lesson");
		LessonNameCoordinator.Instance.OnInputFinish();

		AddMissingData(LessonInfo.DataType.Letters, "setphonemes", "phonemes");
		AddMissingData(LessonInfo.DataType.Words, "setwords", "words");
		AddMissingData(LessonInfo.DataType.Keywords, "setkeywords", "words");
		AddMissingData(LessonInfo.DataType.Stories, "setstories", "stories");

		LessonInfo.Instance.SaveLessons();
	}

	void AddMissingData(LessonInfo.DataType dataType, string columnName, string tableName)
	{
		List<DataRow> data = LessonInfo.Instance.GetData(dataType);
		
		SqliteDatabase db = GameDataBridge.Instance.GetDatabase();
		
		if(data.Count == 0)
		{
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
	}
}
