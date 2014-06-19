using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public static class DataGetters
{
    // Legacy
    // TODO: Delete when no longer used by UnitProgressCoordinator
    public static List<DataRow> GetSectionLetters(int sectionId)
    {
        List<DataRow> letters = new List<DataRow>();
        
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId + " GROUP BY phonemes.id");
        
        if(dt.Rows.Count > 0)
        {
            letters = dt.Rows;
        }
        
        return letters;
    }
    
    // TODO: Delete when no longer used by UnitProgressCoordinator
    public static List<DataRow> GetSectionSentences(int sectionId)
    {
        List<DataRow> sentenceData = new List<DataRow>();
        
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from sentences WHERE section_id =" + sectionId);
        
        if(dt.Rows.Count > 0)
        {
            sentenceData.AddRange(dt.Rows);
        }
        
        return sentenceData;
    }
    
    // TODO: Delete when not needed
    public static List<DataRow> GetSetData(int setNum, string columnName, string tableName)
    {
        List<DataRow> dataList = new List<DataRow>();
        
        DataTable setTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + setNum);
        
        if (setTable.Rows.Count > 0)
        {
            if (setTable.Rows [0] [columnName] != null)
            {
                string[] dataIds = setTable.Rows [0] [columnName].ToString().Replace(" ", "").Replace("-", "").Replace("'", "").Replace("[", "").Replace("]", "").Split(',');
                
                foreach (string id in dataIds)
                {
                    try
                    {
                        DataTable dataTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from " + tableName + " WHERE id='" + id + "'");
                        
                        if (dataTable.Rows.Count > 0)
                        {
                            dataList.Add(dataTable.Rows [0]);
                        }
                    } catch
                    {
                        Debug.Log(String.Format("Getting set {0} for {1} - Invalid ID: {2}", setNum, tableName, id));
                    }
                }
            }
        }
        
        if (dataList.Count > 0)
        {           
            return dataList;    
        }       
        else       
        {         
            return GetSetData(++setNum, columnName, tableName);       
        }
    }
}
