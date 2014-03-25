using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public static class DataHelpers 
{
	/*
	public static DataRow FindGameForSection (DataRow section) 
	{
		int sectionId = Convert.ToInt32(section["id"]);

		SqliteDatabase db = GameDataBridge.Instance.GetDatabase();

		DataTable dtGameInstances = db.ExecuteQuery
			("select * from gameinstances_sections INNER JOIN gameinstances ON gameinstance_id=gameinstances.id WHERE section_id=" + sectionId);
		
		if(dtGameInstances.Rows.Count > 0)
		{
			int gameId = Convert.ToInt32(dtGameInstances.Rows[0]["game_id"]);
			
			DataTable dtGames = db.ExecuteQuery("select * from games WHERE id=" + gameId);
			if(dtGames.Rows.Count > 0)
			{
				return dtGames.Rows[0];
			}
		}

		return null;
	}
	*/

	public static DataRow FindGameForSection (DataRow section) 
	{
		//Debug.Log("Finding for id: " + section["id"].ToString());
		int gameId = Convert.ToInt32(section["game_id"]);
			
		DataTable dtGames = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from games WHERE id=" + gameId);
		if(dtGames.Rows.Count > 0)
		{
			return dtGames.Rows[0];
		}
		else
		{
			return null;
		}
	}

	public static List<DataRow> FindPhonemes (DataRow section)
	{
		int sectionId = Convert.ToInt32(section["id"]);
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
		return dt.Rows;
	}

    public static DataRow FindTargetData(List<DataRow> dataPool, GameDataBridge.DataType dataType)
    {
        LessonInfo.DataType lessonDataType = LessonInfo.DataType.Letters;

        if (dataType == GameDataBridge.DataType.Words)
        {
            lessonDataType = LessonInfo.DataType.Words;
        } 
        else if (dataType == GameDataBridge.DataType.Keywords)
        {
            lessonDataType = LessonInfo.DataType.Keywords;
        }

        return FindTargetData(dataPool, lessonDataType);
    }

    public static DataRow FindTargetData(List<DataRow> dataPool, LessonInfo.DataType dataType)
    {
        DataRow currentData = null;

        bool isLetterData = (dataType == LessonInfo.DataType.Letters);

        string attribute = isLetterData ? "phoneme" : "word";


        if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Session)
        {
            string sessionTargetAttribute = isLetterData ? "is_target_phoneme" : "is_target_word";

            foreach(DataRow letter in dataPool)
            {
                if(letter[sessionTargetAttribute] != null && letter[sessionTargetAttribute].ToString() == "t")
                {
                    Debug.Log("Found target: " + letter[attribute].ToString());
                    currentData = letter;
                    break;
                }
            }
        }
        else if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Custom)
        {
            currentData = LessonInfo.Instance.GetTargetData(dataType);
        }
        
        if(currentData == null) // Even if we are in the Voyage, we might need to execute this if a database error means that there is no target phoneme
        {
            int selectedIndex = UnityEngine.Random.Range(0, dataPool.Count);
            currentData = dataPool[selectedIndex];
            Debug.Log("Random target: " + currentData[attribute].ToString());
        }

        return currentData;
    }
}
