using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public static class DataHelpers 
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // TODO: Delete these methods when safe

    // TODO: Delete when no longer used by LessonContentCoordinator
    public static bool DoesSetContainData(DataRow set, string setAttribute, string dataType)
    {
        string[] ids = set[setAttribute].ToString().Replace("[", "").Replace("]", "").Split(',');
        
        return (ids.Length > 0);
    }

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


    // TODO: Delete if not needed
    static List<DataRow> GetInclusiveSetData(int setNum, string columnName, string tableName)
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
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

    public static DataRow GetSingleTargetData(string dataType, bool allowNonTarget = true)
    {
        DataRow data = GameManager.Instance.GetSingleTargetData(dataType);

        if (data == null && allowNonTarget)
        {
            List<DataRow> dataPool =  GameManager.Instance.GetData(dataType);

            if(dataPool.Count > 0)
            {
                data = dataPool[UnityEngine.Random.Range(0, dataPool.Count)];
            }
        }

        return data;
    }

    // TODO: Make private when no longer needed by BankIndexCoordinator
    public static List<DataRow> GetSetData(DataRow set, string columnName, string tableName) 
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

    static List<DataRow> GetModuleData(int moduleId, string joinAttributeName, string tableName)
    {
        List<DataRow> dataPool = new List<DataRow>();
        
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE programmodule_id=" + moduleId);
        
        foreach (DataRow set in dt.Rows)
        {
            dataPool.AddRange(GetSetData(set, joinAttributeName, tableName));
        }
        
        return dataPool;
    }
    
    public static List<DataRow> GetModulePhonemes(int moduleId)
    {
        if (moduleId != -1)
        {
            return GetModuleData(moduleId, "setphonemes", "phonemes");
        } 
        else
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE completed='t'");
            return dt.Rows;
        }
    }
    
    public static List<DataRow> GetModuleWords(int moduleId)
    {
        if (moduleId != -1)
        {
            return GetModuleData(moduleId, "setwords", "words");
        }
        else
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id");
            return dt.Rows.FindAll(word => (word["tricky"] != null && word["tricky"].ToString() == "t") || (word["nondecodeable"] != null && word["nondecodeable"].ToString() == "t"));
        }
    }
    
    public static List<DataRow> GetModuleKeywords(int moduleId)
    {
        if (moduleId != -1)
        {
            return GetModuleData(moduleId, "setkeywords", "words");
        }
        else
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id");
            return dt.Rows.FindAll(word => (word["tricky"] == null || word["tricky"].ToString() == "f") && (word["nondecodeable"] == null || word["nondecodeable"].ToString() == "t"));
        }
    }
    
    public static List<DataRow> GetModuleSillywords(int moduleId)
    {
        return GetModuleData(moduleId, "setsillywords", "words");
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
                    dataPool = GetSillywords();
                    break;
                case "sentences":
                    dataPool = GetSentences();
                    break;
                case "stories":
                    dataPool = GetStories();
                    break;
                case "correctcaptions":
                    dataPool = GetCorrectCaptions();
                    break;
                case "quizquestions":
                    dataPool = GetQuizQuestions();
                    break;
            }
        }

        return dataPool;
    }

    public static List<DataRow> GetCorrectCaptions()
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("correctcaptions");

        if (dataPool.Count == 0)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE correctsentence='t'");
            Debug.Log("GETCORRECT: " + dt.Rows.Count);
            dataPool = dt.Rows.FindAll(x => x["correctsentence"] != null && x["correctsentence"].ToString() == "t");
            Debug.Log("POSTCHECK: " + dt.Rows.Count);
        }

        Debug.Log("RETURNING: " + dataPool.Count);
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
         
        int storyId = 1;
        while (dataPool.Count == 0)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE publishable='t' AND id=" + storyId);

            dataPool.AddRange(dt.Rows);

            ++storyId;
        }

        return dataPool;
    }

    public static List<DataRow> GetQuizQuestions()
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("quizquestions");

        if (dataPool.Count == 0)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences");
            dataPool = dt.Rows.FindAll(x => x["quiz"] != null && x["quiz"].ToString() == "t");
        }

        return dataPool;
    }

    public static List<DataRow> GetSentences()
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("sentences");

        if (dataPool.Count == 0)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from sentences WHERE is_target_sentence='t'");
            dataPool = dt.Rows;
        }

        return dataPool;
    }
    
    public static List<DataRow> GetPhonemes()
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("phonemes");

        if(dataPool.Count == 0)
        {
            dataPool = GetModulePhonemes(GetModuleId(ColorInfo.PipColor.Pink));
        }

        return dataPool;
    }
    
    public static List<DataRow> GetWords()
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("words");

        if(dataPool.Count == 0)
        {
            dataPool = GetModuleWords(GetModuleId(ColorInfo.PipColor.Pink));
        }

        return dataPool;
    }
    
    public static List<DataRow> GetKeywords()
    {
        List<DataRow> dataPool =  GameManager.Instance.GetData("keywords");

        if(dataPool.Count == 0)
        {
            dataPool = GetModuleKeywords(GetModuleId(ColorInfo.PipColor.Pink));
        }
        
        return dataPool;
    }
    
    public static List<DataRow> GetSillywords()
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
            dataPool = GetModuleSillywords(GetModuleId(ColorInfo.PipColor.Pink));
        }
        
        return dataPool;
    }

    public static DataRow GetFirstPhonemeInWord(DataRow word)
    {
        string[] phonemeIds = word["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
        foreach(string id in phonemeIds)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id='" + id + "'");
            if(dt.Rows.Count > 0)
            {
                return dt.Rows[0];
            }
        }

        return null;
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

    public static string[] GetOrderedPhonemeStrings(DataRow word)
    {
        return word["ordered_phonemes"] != null ? word["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',') : null;
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

    public static string GetLinkedSpriteName(string spriteName)
    {
        string newNameEnd = spriteName [spriteName.Length - 1] == 'a' ? "b" : "a";

        return spriteName.Substring(0, spriteName.Length - 1) + newNameEnd;
    }

    public static Texture2D GetPicture(string dataType, DataRow data)
    {
        dataType = dataType.ToLower();

        Texture2D tex = null;

        Debug.Log("GetPicture dataType: " + dataType);

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
                Debug.Log("Searching words");
                if(data["word"] != null)
                {
                    Debug.Log("Finding picture for: " + data["word"].ToString());
                    tex = Resources.Load<Texture2D>("Images/word_images_png_350/_" + data["word"].ToString());
                }
                break;
            case "quizquestions":
            case "correctcaptions":
            case "sentences":
                if(data["correct_image_name"] != null)
                {
                    tex = Resources.Load<Texture2D>(data["correct_image_name"].ToString());
                    if(tex == null)
                    {
                        tex = Resources.Load<Texture2D>("Images/storypages/" + data["correct_image_name"].ToString());
                    }

                    Debug.Log("TEX - " + data["correct_image_name"].ToString() + ": " + tex);
                }
                break;
            default:
                break;
        }

        return tex;
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
                    clip = LoaderHelpers.LoadAudioForWord(data["word"].ToString());
                }
                break;
            case "sentences":
                break;
            default:
                break;
        }
        
        return clip;
    }

    public static AudioClip GetLongAudio(string dataType, DataRow data)
    {
        dataType = dataType.ToLower();
        
        switch (dataType)
        {
            case "phonemes":
                return LoaderHelpers.LoadMnemonic(data);
                break;
            default:
                return GetShortAudio(dataType, data);
                break;
        }
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

    public static List<DataRow> OnlyQuizQuestions(List<DataRow> dataPool)
    {
        return dataPool.FindAll(x => x ["quiz"] != null && x ["quiz"].ToString() == "t");
    }

    public static bool IsQuizQuestion(DataRow data) // This should be more efficient than using OnlyQuizQuestions
    {
        return data ["quiz"] != null && data ["quiz"].ToString() == "t";
    }

    public static List<DataRow> OnlyCorrectCaptions(List<DataRow> dataPool)
    {
        return dataPool.FindAll(x => x ["correctsentence"] != null && x ["correctsentence"].ToString() == "t");
    }

    public static bool IsCorrectCaption(DataRow data) // This should be more efficient than using OnlyCorrectCaptions
    {
        return data ["correctsentence"] != null && data ["correctsentence"].ToString() == "t";
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

        /*
        if (module == null)
        {
            Debug.LogError(String.Format("Could not find module for programmename {0} - colour {1}", GameManager.currentProgramme, pipColor));
        }
        */
        
        return module;
    }
    
    public static int GetModuleId(ColorInfo.PipColor pipColor)
    {
        DataRow module = GetModule(pipColor);

        /*
        if (module == null)
        {
            Debug.LogError(String.Format("Could not find module for programmename {0} - colour {1}", GameManager.currentProgramme, pipColor));
        }
        */
        
        return module != null ? System.Convert.ToInt32(module["id"]) : -1;
    }

    public static int GetPreviousModuleId(ColorInfo.PipColor pipColor)
    {
        int previousColorIndex = ((int)pipColor) - 1;
        previousColorIndex = Mathf.Clamp(previousColorIndex, 0, previousColorIndex);

        return GetModuleId((ColorInfo.PipColor)previousColorIndex);
    }

    public static string[] GetCorrectCaptionWords(DataRow data)
    {
        if (data != null && data ["good_sentence"] != null)
        {
            string sentence = data ["good_sentence"].ToString();
            string[] words = sentence.Split(new char[] { ' ' });
            return words;
        } 
        else
        {
            return new string[] {};
        }
    }

    public static string[] GetSentenceWords(DataRow sentenceData)
    {
        if (sentenceData != null && sentenceData ["text"] != null)
        {
            string sentence = sentenceData ["text"].ToString();
            string[] words = sentence.Split(new char[] { ' ' });
            return words;
        } 
        else
        {
            return new string[] {};
        }
    }

    public static string GameOrDefault(string defaultDataType)
    {
        string gameDataType = GetDataType(GameManager.Instance.currentGame);

        return !String.IsNullOrEmpty(gameDataType) ? gameDataType : defaultDataType;
    }

    public static string GetDataType(DataRow game)
    {
        string dataType = "";
        if (game != null)
        {
            string gameType = game["gametype"] != null ? game["gametype"].ToString() : "";

            dataType = gameType.ToLower();
        }

        return dataType;
    }

    public static bool WordsShareOnsetPhonemes(DataRow dataA, DataRow dataB)
    {
        string[] orderedPhonemesA = GetOrderedPhonemeStrings(dataA);
        string[] orderedPhonemesB = GetOrderedPhonemeStrings(dataB);

        return orderedPhonemesA [0] == orderedPhonemesB [0];
    }

    public static List<DataRow> FindSharedOnsetWords(DataRow data, int numToFind)
    {
        HashSet<DataRow> sharedOnsetWords = new HashSet<DataRow>();

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words");

        if (dt.Rows.Count > 0)
        {
            numToFind = Mathf.Min(numToFind, dt.Rows.Count);

            for(int i = 0; i < numToFind; ++i)
            {
                sharedOnsetWords.Add(dt.Rows[UnityEngine.Random.Range(0, dt.Rows.Count)]);
            }
        }

        return sharedOnsetWords.ToList();
    }
}