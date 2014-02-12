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
}
