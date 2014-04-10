﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public static class DataHelpers 
{
	public static DataRow FindGameForSection (DataRow section) 
	{
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

    public static DataRow FindGameForSection(int sectionId)
    {
    }

    public static DataRow FindTargetData(List<DataRow> dataPool, Game.Data dataType)
    {
        DataRow currentData = null;

        bool isLetterData = (dataType == Game.Data.Phonemes);

        string attribute = isLetterData ? "phoneme" : "word";

        if(Game.session == Game.Session.Premade)
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
        else if(Game.session == Game.Session.Custom)
        {
            currentData = LessonInfo.Instance.GetTargetData(dataType);
        }
        
        if(currentData == null)
        {
            int selectedIndex = UnityEngine.Random.Range(0, dataPool.Count);
            currentData = dataPool[selectedIndex];
            Debug.Log("Random target: " + currentData[attribute].ToString());
        }

        return currentData;
    }

    public static DataTable GetSectionWords(int sectionId = -1)
    {
        if (sectionId == -1)
        {
            sectionId = SessionManager.Instance.GetCurrentSectionId();
        }

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE section_id=" + sectionId.ToString() + " GROUP BY words.id");
        
        return dt;
    }
    
    public static List<DataRow> GetSectionLetters(int sectionId = -1)
    {
        if(sectionId == -1)
        {
            sectionId = SessionManager.Instance.GetCurrentSectionId();
        }
        
        List<DataRow> letters = new List<DataRow>();
        
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId + " GROUP BY phonemes.id");
        
        if(dt.Rows.Count > 0)
        {
            letters = dt.Rows;
        }
        
        return letters;
    }
    
    public static List<DataRow> GetSectionSentences(int sectionId = -1)
    {
        if(sectionId == -1)
        {
            sectionId = SessionManager.Instance.GetCurrentSectionId();
        }
        
        List<DataRow> sentenceData = new List<DataRow>();
        
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from sentences WHERE section_id =" + sectionId);
        
        if(dt.Rows.Count > 0)
        {
            sentenceData.AddRange(dt.Rows);
        }
        
        return sentenceData;
    }

    
    public static List<DataRow> GetSetData(DataRow set, string columnName, string tableName) // TODO: Store the correct values for these strings, all classes which call these functions should use these references
    {
        string[] ids = set[columnName].ToString().Replace(" ", "").Replace("'", "").Replace("-", "").Replace("[", "").Replace("]", "").Split(',');
        
        List<DataRow> data = new List<DataRow>();
        
        foreach(string id in ids)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from " + tableName + " WHERE id='" + id + "'");
            
            if(dt.Rows.Count > 0)
            {
                data.Add(dt.Rows[0]);
            }
        }
        
        return data;
    }
    
    public static List<DataRow> GetSetData(int setNum, string columnName, string tableName)
    {
        List<DataRow> dataList = new List<DataRow>();
        
        DataTable setTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + setNum);
        
        if(setTable.Rows.Count > 0)
        {
            if(setTable.Rows[0][columnName] != null)
            {
                string[] dataIds = setTable.Rows[0][columnName].ToString().Replace(" ","").Replace("-","").Replace("'","").Replace("[", "").Replace("]", "").Split(',');
                
                foreach(string id in dataIds)
                {
                    try
                    {
                        DataTable dataTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from " + tableName + " WHERE id='" + id + "'");
                        
                        if(dataTable.Rows.Count > 0)
                        {
                            dataList.Add(dataTable.Rows[0]);
                        }
                    }
                    catch
                    {
                        Debug.Log(String.Format("Getting set {0} for {1} - Invalid ID: {2}", setNum, tableName, id));
                    }
                }
            }
        }
        
        return dataList;
    }
    
    public static List<DataRow> GetInclusiveSetData(int setNum, string columnName, string tableName)
    {
        List<DataRow> dataList = new List<DataRow>();
        
        for(int i = setNum; i > 0; --i)
        {
            //Debug.Log("Set: " + i);
            
            DataTable setTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + i);
            
            if(setTable.Rows.Count > 0)
            {
                string[] dataIds = setTable.Rows[0][columnName].ToString().Replace("[", "").Replace("]", "").Split(',');
                
                foreach(string id in dataIds)
                {
                    //Debug.Log("id: " + id);
                    DataTable dataTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from " + tableName + " WHERE id='" + id + "'");
                    
                    if(dataTable.Rows.Count > 0)
                    {
                        //Debug.Log("phoneme: " + dataTable.Rows[0]["phoneme"].ToString());
                        dataList.Add(dataTable.Rows[0]);
                    }
                }
            }
        }
        
        return dataList;
    }
    
    public static List<DataRow> GetLetters(bool inclusiveSets = true)
    {
        List<DataRow> letterData = new List<DataRow>();
        
        switch(Game.session)
        {
            case Game.Session.Premade:
                int sectionId = SessionManager.Instance.GetCurrentSectionId();
                Debug.Log("sectionId: " + sectionId);
                DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
                if(dt.Rows.Count > 0)
                {
                    letterData.AddRange(dt.Rows);
                }
                else
                {
                    Debug.Log("No phonemes for sectionId: " + sectionId);
                }
                break;
                
            case Game.Session.Custom:
                letterData.AddRange(LessonInfo.Instance.GetData(Game.Data.Phonemes));
                break;
                
            case Game.Session.Single:
                int setNum = SkillProgressInformation.Instance.GetCurrentLevel();
                //Debug.Log("setNum: " + setNum);
                if(inclusiveSets)
                {
                    letterData = GetInclusiveSetData(setNum, "setphonemes", "phonemes");
                }
                else
                {
                    letterData = GetSetData(setNum, "setphonemes", "phonemes");
                }
                break;
        }
        
        if(letterData.Count == 0)
        {
            int sectionId = 1407;
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
            if(dt.Rows.Count > 0)
            {
                letterData.AddRange(dt.Rows);
            }
        }
        
        return letterData;
    }
    
    public static List<DataRow> GetWords(bool inclusiveSets = false)
    {
        List<DataRow> wordData = new List<DataRow>();
        
        switch(Game.session)
        {
            case Game.Session.Premade:
                int sectionId = SessionManager.Instance.GetCurrentSectionId();
                Debug.Log("sectionId: " + sectionId);
                DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE section_id=" + sectionId);
                if(dt.Rows.Count > 0)
                {
                    wordData.AddRange(dt.Rows);
                }
                break;
                
            case Game.Session.Custom:
                //Debug.Log("ContentInformation.Instance: " + ContentInformation.Instance);
                wordData.AddRange(LessonInfo.Instance.GetData(Game.Data.Words));
                break;
                
            case Game.Session.Single:
                int setNum = SkillProgressInformation.Instance.GetCurrentLevel();
                if(inclusiveSets)
                {
                    wordData.AddRange(GetInclusiveSetData(setNum, "setwords", "words"));
                }
                else
                {
                    wordData.AddRange(GetSetData(setNum, "setwords", "words"));
                }
                break;
        }
        
        for(int i = wordData.Count - 1; i > -1; --i)
        {
            if(WordIsKeyword(wordData[i]) || WordIsNonsense(wordData[i]) || String.IsNullOrEmpty(wordData[i]["word"].ToString()))
            {
                wordData.RemoveAt(i);
            }
        }
        
        if(wordData.Count == 0)
        {
            wordData = GetSectionWords(1509).Rows;
        }
        
        return wordData;
    }
    
    public static List<DataRow> GetKeywords(bool inclusiveSets = false)
    {
        List<DataRow> keywordData = new List<DataRow>();
        
        switch(Game.session)
        {
            case Game.Session.Premade:
                int sectionId = SessionManager.Instance.GetCurrentSectionId();
                Debug.Log("sectionId: " + sectionId);
                DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE section_id=" + sectionId);
                if(dt.Rows.Count > 0)
                {
                    keywordData.AddRange(dt.Rows);
                    
                    /*
                for(int i = keywordData.Count - 1; i > -1; --i)
                {
                    Debug.Log("keywordData[" + i + "]: " + keywordData[i]);
                    if(!WordIsKeyword(keywordData[i]))
                    {
                        keywordData.RemoveAt(i);
                    }
                }
                */
                }
                break;
                
            case Game.Session.Custom:
                keywordData.AddRange(LessonInfo.Instance.GetData(Game.Data.Keywords));
                break;
                
            case Game.Session.Single:
                int setNum = SkillProgressInformation.Instance.GetCurrentLevel();
                while(keywordData.Count == 0)
                {
                    Debug.Log("SetNum: " + setNum);
                    if(inclusiveSets)
                    {
                        keywordData.AddRange(GetInclusiveSetData(setNum, "setkeywords", "words"));
                    }
                    else
                    {
                        keywordData.AddRange(GetSetData(setNum, "setkeywords", "words"));
                    }
                    
                    /*
                for(int i = keywordData.Count - 1; i > -1; --i)
                {
                    if(!WordIsKeyword(keywordData[i]))
                    {
                        keywordData.RemoveAt(i);
                    }
                    else
                    {
                        Debug.Log("Found " + keywordData[i]["word"].ToString());
                    }
                }
                */
                    
                    ++setNum;
                }
                break;
        }
        
        Debug.Log("keywordData.Count: " + keywordData.Count);
        
        foreach(DataRow row in keywordData)
        {
            Debug.Log(row["word"].ToString());
        }
        
        if(keywordData.Count == 0)
        {
            keywordData = GetSectionWords(1414).Rows;
        }
        
        return keywordData;
    }
    
    public static List<DataRow> GetNonsenseWords()
    {
        Debug.Log("GetNonsenseWords()");
        
        List<DataRow> nonsenseData = new List<DataRow>();
        
        switch(Game.session)
        {
            case Game.Session.Premade:
                int sectionId = SessionManager.Instance.GetCurrentSectionId();
                Debug.Log("sectionId: " + sectionId);
                DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE section_id=" + sectionId);
                if(dt.Rows.Count > 0)
                {
                    nonsenseData.AddRange(dt.Rows);
                }
                break;
                
            case Game.Session.Custom:
                List<DataRow> realWords = ContentInformation.Instance.GetWords();
                
                Debug.Log("realWords.Count: " + realWords.Count);
                foreach(DataRow word in realWords)
                {
                    Debug.Log(word["word"].ToString());
                }
                
                DataTable dtSets = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets ORDER BY number DESC");
                
                int highestSetNum = 1;
                
                if(dtSets.Rows.Count > 0)
                {
                    foreach(DataRow set in dtSets.Rows)
                    {
                        string[] wordIds = set["setwords"].ToString().Replace("[", "").Replace("]", "").Split(',');
                        
                        foreach(string id in wordIds)
                        {
                            DataTable dtWords = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words WHERE id='" + id + "'");
                            
                            if(dtWords.Rows.Count > 0 && realWords.Contains(dtWords.Rows[0]) && System.Convert.ToInt32(set["number"]) > highestSetNum)
                            {
                                highestSetNum = System.Convert.ToInt32(set["number"]);
                            }
                        }
                    }
                }
                
                Debug.Log("highestSetNum: " + highestSetNum);
                
                foreach(DataRow set in dtSets.Rows)
                {
                    if(System.Convert.ToInt32(set["number"]) == highestSetNum)
                    {
                        string[] wordIds = set["setsillywords"].ToString().Replace("[", "").Replace("]", "").Split(',');
                        
                        foreach(string id in wordIds)
                        {
                            DataTable dtWords = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words WHERE id='" + id + "'");
                            
                            Debug.Log("Found highest set");
                            foreach(DataRow word in dtWords.Rows)
                            {
                                Debug.Log(word["word"].ToString());
                            }
                            
                            if(dtWords.Rows.Count > 0)
                            {
                                nonsenseData.AddRange(dtWords.Rows);
                            }
                        }
                    }
                }
                break;
                
            case Game.Session.Single:
                int setNum = SkillProgressInformation.Instance.GetCurrentLevel();
                Debug.Log("setNum: " + setNum);
                nonsenseData = GetSetData(setNum, "setsillywords", "words");
                break;
        }
        
        for(int i = nonsenseData.Count - 1; i > -1; --i)
        {
            if(!WordIsNonsense(nonsenseData[i]))
            {
                nonsenseData.RemoveAt(i);
            }
        }
        
        if(nonsenseData.Count == 0)
        {
            nonsenseData = GetSectionWords(1395).Rows;
        }
        
        return nonsenseData;
    }
    
    public static bool SetContainsData(DataRow set, string setAttribute, string dataType)
    {
        string[] ids = set[setAttribute].ToString().Replace("[", "").Replace("]", "").Split(',');
        
        /*
        Debug.Log("ids.Length: " + ids.Length);
        foreach(string id in ids)
        {
            Debug.Log(id);
        }
        */
        
        return (ids.Length > 0);
    }
    
    public static List<DataRow> GetSetPhonemes(DataRow set)
    {
        string[] phonemeIds = set["setphonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
        
        List<DataRow> phonemes = new List<DataRow>();
        
        foreach(string id in phonemeIds)
        {
            DataTable phonemeTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id='" + id + "'");
            
            if(phonemeTable.Rows.Count > 0)
            {
                phonemes.Add(phonemeTable.Rows[0]);
            }
        }
        
        return phonemes;
    }
    
    public static List<DataRow> GetOrderedPhonemes(DataRow word)
    {
        List<DataRow> phonemes = new List<DataRow>();
        string[] phonemeIds = word["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
        foreach(string id in phonemeIds)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id='" + id + "'");
            if(dt.Rows.Count > 0)
            {
                phonemes.Add(dt.Rows[0]);
            }
        }
        
        return phonemes;
    }
    
    static bool WordIsKeyword(DataRow wordData)
    {
        return ((wordData["tricky"] != null && wordData["tricky"].ToString() == "t") || (wordData["nondecodable"] != null && wordData["nondecodable"].ToString() == "t"));
    }
    
    static bool WordIsNonsense(DataRow wordData)
    {
        return (wordData["nonsense"].ToString() == "t");
    }
}
