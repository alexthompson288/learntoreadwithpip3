using UnityEngine;
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

    public static DataRow GetSingleTargetData(string dataType, bool guaranteeData = false)
    {
        return GameManager.Instance.GetSingleTargetData(dataType, guaranteeData);
    }

    public static DataTable GetSectionWords(int sectionId)
    {
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE section_id=" + sectionId.ToString() + " GROUP BY words.id");
        
        return dt;
    }
    
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

        if (dataList.Count > 0)
        {
            return dataList;
        } 
        else
        {
            return GetSetData(++setNum, columnName, tableName);
        }
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

    public static List<DataRow> GetData(string dataType)
    {
        List<DataRow> dataPool = GameManager.Instance.GetData(dataType);

        if (dataPool.Count == 0)
        {
            switch(dataType)
            {
                case "phonemes":
                    dataPool = GetPhonemes();
                    break;
                case "words":
                    dataPool = GetWords();
                    break;
                case "keywords":
                    dataPool = GetKeywords();
                    break;
                case "sillywords":
                    dataPool = GetSillyWords();
                    break;
                case "sentences":
                    dataPool = GetSentences();
                    break;
                case "stories":
                    dataPool = GetStories();
                    break;
            }
        }

        return dataPool;
    }

    public static List<DataRow> GetCorrectCaptions()
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("correctcaptions");

        return dataPool;
    }

    public static DataRow GetStory()
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("stories");

        return dataPool.Count > 0 ? dataPool [0] : null;
    }

    public static List<DataRow> GetStories()
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("stories");
         
        if (dataPool.Count == 0)
        {
            dataPool = GetSetData(1, "setstories", "stories");
        }

        return dataPool;
    }

    public static List<DataRow> GetQuizQuestions()
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("quizquestions");

        return dataPool;
    }

    public static List<DataRow> GetSentences()
    {
        return GameManager.Instance.GetData("sentences");
    }
    
    public static List<DataRow> GetPhonemes(bool inclusiveSets = false)
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("phonemes");

        if(dataPool.Count == 0)
        {
            dataPool = inclusiveSets ? GetInclusiveSetData(1, "setphonemes", "phonemes") : GetSetData(1, "setphonemes", "phonemes");
        }

        return dataPool;
    }
    
    public static List<DataRow> GetWords(bool inclusiveSets = false)
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("words");

        if(dataPool.Count == 0)
        {
            dataPool = inclusiveSets ? GetInclusiveSetData(1, "setwords", "words") : GetSetData(1, "setwords", "words");
        }

        return dataPool;
    }
    
    public static List<DataRow> GetKeywords(bool inclusiveSets = false)
    {
        List<DataRow> dataPool =  GameManager.Instance.GetData("keywords");

        if(dataPool.Count == 0)
        {
            dataPool = inclusiveSets ? GetInclusiveSetData(1, "setkeywords", "words") : GetSetData(1, "setkeywords", "words");
        }
        
        return dataPool;
    }
    
    public static List<DataRow> GetSillyWords()
    {
        Debug.Log("GetNonsenseWords()");
        
        List<DataRow> dataPool = GameManager.Instance.GetData("sillywords");
        
        for(int i = dataPool.Count - 1; i > -1; --i)
        {
            if(!WordIsNonsense(dataPool[i]))
            {
                dataPool.RemoveAt(i);
            }
        }
        
        if(dataPool.Count == 0)
        {
            dataPool = GetSectionWords(1395).Rows;
        }
        
        return dataPool;
    }
    
    public static bool DoesSetContainData(DataRow set, string setAttribute, string dataType)
    {
        string[] ids = set[setAttribute].ToString().Replace("[", "").Replace("]", "").Split(',');
        
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

    public static string textAttribute
    {
        get
        {
            return GetTextAttribute(GameManager.Instance.dataType);
        }
    }

    public static string GetTextAttribute(string dataType)
    {
        dataType = dataType.ToLower();

        string attributeName = "phoneme";

        switch (dataType)
        {
            case "words":
            case "keywords":
                attributeName = "word";
                break;
            default:
                break;
        }

        return attributeName;
    }

    public static string GetLabelText(string dataType, DataRow data)
    {
        dataType = dataType.ToLower();

        string textAttribute = GetTextAttribute(dataType);

        string labelText = data [textAttribute] != null ? data [textAttribute].ToString() : "";

        return labelText;
    }

    public static string GetTargetAttribute(string dataType)
    {
        dataType = dataType.ToLower();
        
        string attributeName = "is_target_phoneme";
        
        switch (dataType)
        {
            case "words":
            case "keywords":
                attributeName = "is_target_word";
                break;
            default:
                break;
        }
        
        return attributeName;
    }

    public static string setAttribute
    {
        get
        {
            return GetSetAttribute(GameManager.Instance.dataType);
        }
    }

    public static string GetSetAttribute(string dataType)
    {
        dataType = dataType.ToLower();

        string attributeName = "setphonemes";

        switch (dataType)
        {
            case "words":
                attributeName = "setwords";
                break;
            case "keywords":
                attributeName = "setkeywords";
                break;
            default:
                break;
        }

        return attributeName;
    }

    public static string tableName
    {
        get
        {
            return GetTable(GameManager.Instance.dataType);
        }
    }

    public static string GetTable(string dataType)
    {
        dataType = dataType.ToLower();
        
        string table = "phonemes";
        
        switch (dataType)
        {
            case "words":
            case "keywords":
                table = "words";
                break;
            default:
                break;
        }
        
        return table;
    }

    public static string GetLinkingTable(string dataType)
    {
        dataType = dataType.ToLower();

        string linkingTable = "data_phonemes";
        
        switch (dataType)
        {
            case "words":
            case "keywords":
                linkingTable = "data_words";
                break;
            default:
                break;
        }
        
        return linkingTable;
    }

    public static string GetLinkingAttribute(string dataType)
    {
        dataType = dataType.ToLower();
        
        string linkingAttribute = "phoneme_id";
        
        switch (dataType)
        {
            case "words":
            case "keywords":
                linkingAttribute = "word_id";
                break;
            default:
                break;
        }
        
        return linkingAttribute;
    }

    public static string GetContainerNameA(string dataType)
    {
        string containerName = "phoneme_a";

        switch (dataType.ToLower())
        {
            case "words":
            case "keywords":
                containerName = "word";
                break;
            case "sentences":
                containerName = "sentence";
                break;
        }

        return containerName;
    }

    public static string GetContainerNameB(string dataType)
    {
        string containerName = "phoneme_b";

        switch (dataType.ToLower())
        {
            case "words":
            case "keywords":
                containerName = "word";
                break;
            case "sentences":
                containerName = "sentence";
                break;
        }
        
        return containerName;
    }

    public static string GetPicturePath(string dataType)
    {
        dataType = dataType.ToLower();

        string picturePath = "Images/mnemonics_images_png_250/";

        switch (dataType)
        {
            case "words":
            case "keywords":
                picturePath = "Images/word_images_png_350/_";
                break;
        }

        return picturePath;
    }

    public static string GetLinkedSpriteName(string spriteName)
    {
        string newNameEnd = spriteName [spriteName.Length - 1] == 'a' ? "b" : "a";

        string linkingName = spriteName.Substring(0, spriteName.Length - 1);
        Debug.Log("linkingName: " + linkingName);

        return linkingName + newNameEnd;
    }

    public static string GetFullPicturePath(string dataType, DataRow data)
    {
        dataType = dataType.ToLower();

        string fullPicturePath = "";

        switch (dataType)
        {
            case "phonemes":
                if(data["phoneme"] != null && data["mneumonic"] != null)
                {
                    fullPicturePath = String.Format("Images/mnemonics_images_png_250/{0}_{1}", data["phoneme"], data["mneumonic"]);
                }
                break;
            case "words":
            case "keywords":
                if(data["word"] != null)
                {
                    fullPicturePath = String.Format("Images/word_images_png_350/_{0}", data["word"]);
                }
                break;
        }

        return fullPicturePath;
    }

    public static Texture2D GetPicture(string dataType, DataRow data)
    {
        dataType = dataType.ToLower();

        Texture2D tex = null;

        switch (dataType)
        {
            case "phonemes":
                if(data["phoneme"] != null && data["mneumonic"] != null)
                {
                    Debug.Log("Checking Picture for " + data["id"].ToString());
                    tex = Resources.Load<Texture2D>(String.Format("Images/mnemonics_images_png_250/{0}_{1}", data["phoneme"], data["mneumonic"]).ToString().Replace(" ", "_"));
                    Debug.Log("Found picture: " + tex != null);
                }
                break;
            case "words":
            case "keywords":
                if(data["word"] != null)
                {
                    tex = Resources.Load<Texture2D>("Images/word_images_png_350/_" + data["word"].ToString());
                }
                break;
        }

        return tex;
    }

    public static AudioClip GetLongAudio(string dataType, DataRow data)
    {
        dataType = dataType.ToLower();

        AudioClip clip = null;

        switch (dataType)
        {
            case "phonemes":
                clip = LoaderHelpers.LoadMnemonic(data);
                break;
            case "words":
            case "keywords":
                if(data["word"] != null)
                {
                    LoaderHelpers.LoadAudioForWord(data["word"].ToString());
                }
                break;
        }

        return clip;
    }

    public static AudioClip GetShortAudio(string dataType, DataRow data)
    {
        dataType = dataType.ToLower();
        
        AudioClip clip = null;
        
        switch (dataType)
        {
            case "phonemes":
                if(data["grapheme"] != null)
                {
                    clip = AudioBankManager.Instance.GetAudioClip(data["grapheme"].ToString());
                }
                break;
            case "words":
            case "keywords":
                if(data["word"] != null)
                {
                    LoaderHelpers.LoadAudioForWord(data["word"].ToString());
                }
                break;
        }
        
        return clip;
    }

    public static List<DataRow> OnlyPictureData(string dataType, List<DataRow> dataPool)
    {
        List<DataRow> hasPictureDataPool = new List<DataRow>();

        for (int i = 0; i < dataPool.Count; ++i)
        {
            if(GetPicture(dataType, dataPool[i]) != null)
            {
                hasPictureDataPool.Add(dataPool[i]);
            }
        }

        return hasPictureDataPool;
    }

    public static DataRow GetModule(ColorInfo.PipColor pipColor)
    {
        DataRow module = null;
        
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery(System.String.Format("select * from programmodules WHERE colour='{0}' AND programmename='{1}'", 
                                                                                               ColorInfo.GetColorString(pipColor), GameManager.currentProgramme));

        if (dt.Rows.Count > 0)
        {
            module = dt.Rows[0];
        }

        if (module == null)
        {
            Debug.LogError(String.Format("Could not find module for programmename {0} - colour {1}", GameManager.currentProgramme, pipColor));
        }
        
        return module;
    }
    
    public static int GetModuleId(ColorInfo.PipColor pipColor)
    {
        DataRow module = GetModule(pipColor);

        if (module == null)
        {
            Debug.LogError(String.Format("Could not find module for programmename {0} - colour {1}", GameManager.currentProgramme, pipColor));
        }
        
        return module != null ? System.Convert.ToInt32(module["id"]) : -1;
    }
}