using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class DataHelpers 
{
    // Games
    public static DataRow GetCurrentGame()
    {
        return GetGame(GameManager.Instance.currentGameName);
    }
    
    public static DataRow GetGame(string gameName)
    {
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from games WHERE name='" + gameName + "'");
        return dt.Rows.Count > 0 ? dt.Rows [0] : null;
    }

    public static string GetGameName()
    {
        return GameManager.Instance.currentGameName;
    }

    public static DataRow GetGameForSection (DataRow section) 
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

    // Modules
    public static DataRow GetModule(ColorInfo.PipColor pipColor)
    {
        DataRow module = null;
        
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery(System.String.Format("select * from programmodules WHERE colour='{0}' AND programmename='{1}'", 
                                                                                               ColorInfo.GetColorString(pipColor), GameManager.Instance.programme));
        
        if (dt.Rows.Count > 0)
        {
            module = dt.Rows[0];
        }
        
        return module;
    }
    
    public static int GetModuleId(ColorInfo.PipColor pipColor)
    {
        DataRow module = GetModule(pipColor);
        
        return module != null ? System.Convert.ToInt32(module["id"]) : -1;
    }
    
    public static int GetPreviousModuleId(ColorInfo.PipColor pipColor)
    {
        int previousColorIndex = ((int)pipColor) - 1;
        previousColorIndex = Mathf.Clamp(previousColorIndex, 0, previousColorIndex);
        
        return GetModuleId((ColorInfo.PipColor)previousColorIndex);
    }

    // DataType
    public static string GameOrDefault(string defaultDataType)
    {
        string gameDataType = GetGameDataType(GetCurrentGame());
        string dataType = !String.IsNullOrEmpty(gameDataType) ? gameDataType : defaultDataType;
        
        return dataType;
    }
    
    public static string GetGameDataType(DataRow game)
    {
        string dataType = "";
        if (game != null)
        {
            string gameType = game["gametype"] != null ? game["gametype"].ToString() : "";
            dataType = gameType.ToLower();
        }

        return dataType;
    }

    // Generic
    public static string[] GetArray(DataRow data, string columnName)
    {
        string[] array = data [columnName].ToString().Replace(" ", "").Replace("'", "").Replace("-", "").Replace("[", "").Replace("]", "").Trim(new char[] {'\n'}).Split('\n');
        return array;

        /*
        ////D.Log(data [columnName].ToString());
        string pattern = "[^ ]+";
        Regex rgx = new Regex(pattern);
        MatchCollection mc = rgx.Matches(data[columnName].ToString());
        //MatchCollection mc = rgx.Matches(text);
        string[] items = new string[mc.Count];
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = mc[i].Value;
        }

        foreach (string item in items)
        {
            ////D.Log(item);
        }

        return items;  
        */
    }

    public static string GetDataType(DataRow data)
    {
        string dataType = data.GetTableName();
        switch (data.GetTableName())
        {
            case "data_words":
                dataType = "words";
                break;
            case "data_phonemes":
                dataType = "phonemes";
                break;
            default:
                break;
        }

        return dataType;
    }

    public static List<DataRow> GetSetData(DataRow set, string columnName, string tableName) 
    {
        string[] ids = GetArray(set, columnName);
        
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

    public static Texture2D GetPicture(DataRow data)
    {
        Texture2D tex = null;

        switch (data.GetTableName())
        {
            case "phonemes":
            case "data_phonemes":
                if(data["phoneme"] != null && data["mneumonic"] != null)
                {
                    tex = Resources.Load<Texture2D>(String.Format("Images/mnemonics_images_png_250/{0}_{1}", data["phoneme"], data["mneumonic"]).ToString().Replace(" ", "_"));
                }
                break;
            case "words":
            case "data_words":
                if(data["word"] != null)
                {
                    tex = Resources.Load<Texture2D>("Images/word_images_png_350/_" + data["word"].ToString());
                }
                break;
            case "datasentences":
                if(data["correct_image_name"] != null)
                {
                    tex = Resources.Load<Texture2D>(data["correct_image_name"].ToString());
                    if(tex == null)
                    {
                        tex = Resources.Load<Texture2D>("Images/storypages/" + data["correct_image_name"].ToString());
                    }
                }
                break;
            case "shapes":
                if(data["name"] != null)
                {
                    tex = Resources.Load<Texture2D>(data["name"].ToString());
                }
                break;
            case "stories":
                if(data["storycoverartwork"] != null)
                {
                    tex = LoaderHelpers.LoadObject<Texture2D>("Images/storypages/" + data["storycoverartwork"].ToString());
                }
                break;
            case "storypages":
                string bgImageName = data["backgroundart"] == null ? "" : data["backgroundart"].ToString().Replace(".png", "");
                tex = LoaderHelpers.LoadObject<Texture2D>("Images/storypages/" + bgImageName);
                
                if(tex == null)
                {
                    string imageName = data["image"] == null ? "" : data["image"].ToString().Replace(".png", "");
                    tex = LoaderHelpers.LoadObject<Texture2D>("Images/storypages/" + imageName);
                }
                break;
            case "pipisodes":
                if(data["image_filename"] != null)
                {
                    tex = Resources.Load<Texture2D>(String.Format("Images/mnemonics_images_png_250/{0}", data["image_filename"]));
                }
                break;
            default:
                break;
        }
        
        return tex;
    }

    public static List<DataRow> OnlyPictureData(List<DataRow> dataPool)
    {
        List<DataRow> hasPictureDataPool = new List<DataRow>();
        
        for (int i = 0; i < dataPool.Count; ++i)
        {
            if(GetPicture(dataPool[i]) != null)
            {
                hasPictureDataPool.Add(dataPool[i]);
            }
        }
        
        return hasPictureDataPool;
    }
    
    public static string GetSpriteName(DataRow data)
    {
        string spriteName = "";
        
        switch (data.GetTableName())
        {
            case "numbers":
                spriteName = "digit_" + data["value"].ToString();
                break;
            case "shapes":
                spriteName = String.Format("{0}", data["name"].ToString());
                break;
            default:
                break;
        }
        
        return spriteName;
    }

    public static AudioClip GetShortAudio(DataRow data)
    {
        AudioClip clip = null;

        switch (data.GetTableName())
        {
            case "phonemes":
            case "data_phonemes":
                if(data["grapheme"] != null)
                {
                    clip = AudioBankManager.Instance.GetAudioClip(data["grapheme"].ToString());
                }
                break;
            case "words":
            case "data_words":
                if(data["word"] != null)
                {
                    clip = LoaderHelpers.LoadAudioForWord(data["word"].ToString().ToLower());
                }
                break;
            case "numbers":
                if(data["value"] != null)
                {
                    clip = LoaderHelpers.LoadAudioForNumber(data);
                }
                break;
            default:
                break;
        }
        
        return clip;
    }

    public static AudioClip GetLongAudio(DataRow data)
    {
        switch (data.GetTableName())
        {
            case "phonemes":
            case "data_phonemes":
                return LoaderHelpers.LoadMnemonic(data);
            default:
                return GetShortAudio(data);
                break;
        }
    }

    public static string GetLabelText(DataRow data)
    {   
        string textAttribute = "phoneme";
        
        switch (data.GetTableName())
        {
            case "words":
            case "data_words":
                textAttribute = "word";
                break;
            case "numbers":
                textAttribute = "value";
                break;
            case "shapes":
                textAttribute = "name";
                break;
            default:
                break;
        }
        
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

    public static DataRow GetSingleTargetData(string dataType, List<DataRow> backupDataPool = null)
    {
        DataRow data = GameManager.Instance.GetSingleTargetData(dataType);

        if (data == null && backupDataPool != null && backupDataPool.Count > 0)
        {
            data = backupDataPool[UnityEngine.Random.Range(0, backupDataPool.Count)];
        }

        return data;
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
                case "m_starSpawnGridwords":
                    dataPool = GetNonReadableWords();
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
                case "numbers":
                    dataPool = GetNumbers();
                    break;
                case "shapes":
                    dataPool = GetShapes();
                    break;
            }
        }
        
        return dataPool;
    }

    // Reading
    public static List<DataRow> GetModulePhonemes(int moduleId)
    {
        List<DataRow> modulePhonemes = new List<DataRow>();

        if (moduleId != -1)
        {
            SqliteDatabase db = GameDataBridge.Instance.GetDatabase();

            DataTable setsTable = db.ExecuteQuery("select * from phonicssets WHERE programmodule_id=" + moduleId);

            foreach(DataRow set in setsTable.Rows)
            {
                string[] phonemeIds = GetArray(set, "setphonemes");

                foreach(string phonemeId in phonemeIds)
                {
                    DataTable phonemesTable = db.ExecuteQuery(String.Format("select * from phonemes WHERE id='{0}' AND completed='t'", phonemeId)); 

                    if(phonemesTable.Rows.Count > 0)
                    {
                        modulePhonemes.Add(phonemesTable.Rows[0]);
                    }
                } 
            }
        } 
        else
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE completed='t'");
            modulePhonemes = dt.Rows;
        }

        return modulePhonemes;
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
    
    public static List<DataRow> GetCorrectCaptions()
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("correctcaptions");
        
        if (dataPool.Count == 0)
        {
            ////D.LogWarning("Defaulting to all correctsentences");
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE correctsentence='t'");
            dataPool = dt.Rows.FindAll(x => x["correctsentence"] != null && x["correctsentence"].ToString() == "t");
        }
        
        return dataPool;
    }
    
    public static DataRow GetStory()
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("stories");
        
        if (dataPool.Count == 0)
        {
            dataPool = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE id=" + 1).Rows;
        }
        
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
            ////D.LogWarning("Defaulting to Pink Phonemes");
            dataPool = GetModulePhonemes(GetModuleId(ColorInfo.PipColor.Pink));
        }
        
        return dataPool;
    }
    
    public static List<DataRow> GetNonReadableWords()
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("words");
        
        dataPool.RemoveAll(x => x ["m_starSpawnGrid"] == null || x ["m_starSpawnGrid"].ToString() == "f" || String.IsNullOrEmpty(x["word"].ToString()));
        
        if(dataPool.Count == 0)
        {
            dataPool = GetModuleWords(GetModuleId(ColorInfo.PipColor.Pink));
        }
        
        return dataPool;
    }
    
    public static List<DataRow> GetWords()
    {
        List<DataRow> dataPool = GameManager.Instance.GetData("words");
        
        dataPool.RemoveAll(x => (x ["m_starSpawnGrid"] != null && x ["m_starSpawnGrid"].ToString() == "t") || String.IsNullOrEmpty(x["word"].ToString()));
        
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

    public static List<DataRow> OnlyAlphaChars(List<DataRow> words)
    {
        List<DataRow> legalWords = new List<DataRow>();
        foreach (DataRow word in words)
        {
            bool wordIsLegal = true;

            if(word["word"] != null)
            {
                foreach(char c in word["word"].ToString())
                {
                    if(!Char.IsLetter(c))
                    {
                        wordIsLegal = false;
                    }
                }
            }

            if(wordIsLegal)
            {
                legalWords.Add(word);
            }
        }
        
        return legalWords;
    }

    public static List<DataRow> OnlyOrderedPhonemes(List<DataRow> words)
    {
        List<DataRow> legalWords = new List<DataRow>();
        foreach (DataRow word in words)
        {
            if(HasOrderedPhonemes(word))
            {
                legalWords.Add(word);
            }
        }

        return legalWords;
    }
    
    public static DataRow GetOnsetPhoneme(DataRow word)
    {
        string[] phonemeIds = GetOrderedPhonemeIdStrings(word);

        if (phonemeIds != null)
        {
            foreach (string id in phonemeIds)
            {
                DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id='" + id + "'");
                if (dt.Rows.Count > 0)
                {
                    return dt.Rows [0];
                }
            }
        }
        
        return null;
    }

    public static bool HasOrderedPhonemes(DataRow word)
    {
        return GetArray(word, "ordered_phonemes") != null;
    }

    public static List<DataRow> GetOrderedPhonemes(DataRow word)
    {
        List<DataRow> phonemes = new List<DataRow>();
        string[] phonemeIds = GetArray(word, "ordered_phonemes");

        if (phonemeIds != null)
        {
            foreach (string id in phonemeIds)
            {
                DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id='" + id + "'");
                if (dt.Rows.Count > 0)
                {
                    phonemes.Add(dt.Rows [0]);
                }
            }
        }
        
        return phonemes;
    }

    public static string[] GetOrderedPhonemeIdStrings(DataRow word)
    {
        return word["ordered_phonemes"] != null ? GetArray(word, "ordered_phonemes") : null;
    }
    
    static bool WordIsKeyword(DataRow wordData)
    {
        return ((wordData["tricky"] != null && wordData["tricky"].ToString() == "t") || (wordData["nondecodable"] != null && wordData["nondecodable"].ToString() == "t"));
    }
    
    static bool WordIsNonsense(DataRow wordData)
    {
        return (wordData["nonsense"].ToString() == "t");
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

    public static bool WordsShareOnsetPhonemes(DataRow dataA, DataRow dataB)
    {
        string[] orderedPhonemesA = GetOrderedPhonemeIdStrings(dataA);
        string[] orderedPhonemesB = GetOrderedPhonemeIdStrings(dataB);

        return orderedPhonemesA [0] == orderedPhonemesB [0];
    }

    public static bool HasOnsetWords(DataRow phoneme, List<DataRow> words)
    {
        bool hasOnsetWord = false;

        foreach (DataRow word in words)
        {
            if(phoneme.Equals(GetOnsetPhoneme(word)))
            {
                hasOnsetWord = true;
                break;
            }
        }

        return hasOnsetWord;
    }

    public static List<DataRow> GetOnsetWords(DataRow phoneme, int numToFind, bool onlyPictures)
    {
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words");

        List<DataRow> onsetWords = new List<DataRow>();

        if (onlyPictures)
        {
            onsetWords = dt.Rows.FindAll(x => phoneme.Equals(GetOnsetPhoneme(x)) && GetPicture(x) != null);
        }
        else
        {
            onsetWords = dt.Rows.FindAll(x => phoneme.Equals(GetOnsetPhoneme(x)));
        }

        ////D.Log("Found extra " + onsetWords.Count + " onsetWords");

        numToFind = Mathf.Min(numToFind, onsetWords.Count);

        while (onsetWords.Count > numToFind)
        {
            int removeIndex = UnityEngine.Random.Range(0, onsetWords.Count);
            onsetWords.RemoveAt(removeIndex);
        }

        return onsetWords;
    }

    // Maths
    public static List<DataRow> GetModuleTimes(ColorInfo.PipColor pipColor)
    {
        List<DataRow> times = new List<DataRow>();
        
        // 00.00 - 23.59
        //for (int i = 0; i < 2400; ++i)
        for(int i = 0; i < 1300; ++i)
        {
            if(i % 100 < 60) // Only minutes up to 59 inclusive are allowed
            {
                times.Add(CreateTime(i));
            }
        }

        if (pipColor < ColorInfo.PipColor.White)
        {
            times.RemoveAll(x => x.GetId() % 5 != 0);
        }

        if (pipColor < ColorInfo.PipColor.Gold)
        {
            times.RemoveAll(x => x.GetId() % 10 != 0);
        }

        if (pipColor < ColorInfo.PipColor.Purple)
        {
            times.RemoveAll(x => x.GetId() % 100 % 30 != 0);
        }

        return times;
    }

    public static List<DataRow> GetTimes()
    {
        List<DataRow> times = GameManager.Instance.GetData("times");

        if (times.Count == 0)
        {
            times = GetModuleTimes(ColorInfo.PipColor.Turquoise);
        }

        return times;
    }

    public static DataRow CreateTime(int time)
    {
        DataRow row = new DataRow();
        row["tablename"] = "times";
        row["id"] = time;

        string timeString = time.ToString();
        while (timeString.Length < 4)
        {
            timeString = timeString.Insert(0, "0");
        }
        timeString = timeString.Insert(2, ":");

        row["time"] = timeString;
        row["datetime"] = DateTime.ParseExact(timeString, "hh:mm", null);

        return row;
    }

    public static int GetHighestModuleNumber(ColorInfo.PipColor pipColor)
    {
        switch (pipColor)
        {
            case ColorInfo.PipColor.Pink:
                return 10;
                break;
            case ColorInfo.PipColor.Red:
                return 20;
                break;
            case ColorInfo.PipColor.Yellow:
                return 30;
                break;
            case ColorInfo.PipColor.Blue:
                return 50;
                break;
            case ColorInfo.PipColor.Green:
                return 100;
                break;
            case ColorInfo.PipColor.Orange:
                return 150;
                break;
            case ColorInfo.PipColor.Turquoise:
                return 500;
                break;
            case ColorInfo.PipColor.Purple:
                return 1000;
                break;
            case ColorInfo.PipColor.Gold:
                return 2000;
                break;
            case ColorInfo.PipColor.White:
                return 9999;
                break;
            default:
                return 10;
                break;
        }
    }

    public static List<string> GetNumberSpriteNames(DataRow data)
    {
        List<string> spriteNames = new List<string>();
        
        string valueString = data ["value"].ToString();
        
        foreach (char c in valueString)
        {
            spriteNames.Add("digit_" + c.ToString());
        }
        
        return spriteNames;
    }

    public static int GetHighestNumberValue()
    {
        List<DataRow> boundaryData = GameManager.Instance.GetData("numbers");
        List<int> boundaryValues = GetNumberValues(boundaryData);
        boundaryValues.Sort();

        return boundaryValues [boundaryValues.Count - 1];
    }

    public static List<DataRow> GetNumbers()
    {
        List<DataRow> numbers = GameManager.Instance.GetData("numbers");

        if (numbers.Count == 0)
        {
            for (int i = 1; i < 11; ++i)
            {
                numbers.Add(CreateNumber(i));
            }
        }
        
        return numbers;
    }

    public static List<DataRow> CreateNumbers(int lowest, int highest)
    {
        List<DataRow> numbers = new List<DataRow>();
        for(int i = lowest; i < highest + 1; ++i)
        {
            numbers.Add(CreateNumber(i));
        }

        return numbers;
    }
    /*
    public static List<DataRow> GetNumbers()
    {
        ////D.Log("DataHelpers.GetNumbers()"); 
        List<DataRow> boundaryData = GameManager.Instance.GetData("numbers");

        int[] boundaryValues = new int[2];

        if (boundaryData.Count > 1)
        {
            for (int i = 0; i < 2; ++i)
            {
                boundaryValues [i] = Convert.ToInt32(boundaryData [i] ["value"]);
            }
        }
        else
        {
            boundaryValues[0] = 1;
            boundaryValues[1] = 10;
        }

        ////D.Log("BoundaryValues: " + boundaryValues [0] + " - " + boundaryValues [1]);

        Array.Sort(boundaryValues);

        List<DataRow> numbers = new List<DataRow>();

        for(int i = boundaryValues[0]; i < boundaryValues[1] + 1; ++i)
        {
            numbers.Add(CreateNumber(i));
        }

        return numbers;
    }
    */

    public static DataRow CreateNumber(int value)
    {
        DataRow row = new DataRow();
        row["tablename"] = "numbers";
        row["id"] = value;
        row["value"] = value;

        return row;
    }

    public static List<DataRow> OnlyLowNumbers(List<DataRow> dataPool, int maxValue)
    {
        List<DataRow> lowNumberPool = new List<DataRow>();

        foreach (DataRow number in dataPool)
        {
            if(Convert.ToInt32(number["value"]) <= maxValue)
            {
                lowNumberPool.Add(number);
            }
        }

        return lowNumberPool;
    }

    public static List<DataRow> GetShapes()
    {
        List<string> shapeNames = new List<string>();
        shapeNames.Add("triangle_equilateral");
        shapeNames.Add("triangle_isosceles");
        shapeNames.Add("triangle_scalene");
        shapeNames.Add("square");
        shapeNames.Add("rectangle");
        shapeNames.Add("pentagon");
        //shapeNames.Add("hexagon");
        //shapeNames.Add("heptagon");
        //shapeNames.Add("octagon");

        List<DataRow> shapes = new List<DataRow>();
        for (int i = 0; i < shapeNames.Count; ++i)
        {
            DataRow row = new DataRow();
            row["tablename"] = "shapes";
            row["id"] = i + 1;
            row["name"] = shapeNames[i];
            shapes.Add(row);
        }

        return shapes;
    }

    public static List<DataRow> GetArithmeticOperators()
    {
        List<DataRow> operators = new List<DataRow>();

        // Addition
        DataRow add = new DataRow();
        add ["tablename"] = "arithmetic_operators";
        add ["id"] = 1;
        add ["name"] = "add";
        add ["symbol"] = "+";
        Func <int, int, int> addFunc = (x, y) => x + y;
        add ["operation"] = addFunc;


        operators.Add(add);

        /*
        // Subtraction
        DataRow subtract = new DataRow();
        subtract ["tablename"] = "arithmetic_operators";
        subtract ["id"] = 2;
        subtract ["name"] = "subtract";
        subtract ["symbol"] = "-";
        Func <int, int, int> subtractFunc = (x, y) => x - y;
        subtract ["operation"] = subtractFunc;
         
        operators.Add(subtract);


        // Multiplication
        DataRow multiply = new DataRow();
        multiply ["tablename"] = "arithmetic_operators";
        multiply ["id"] = 3;
        multiply ["name"] = "multiply";
        multiply ["symbol"] = "x";
        Func <int, int, int> multiplyFunc = (x, y) => x * y;
        multiply ["operation"] = multiplyFunc;
        
        operators.Add(multiply);


        // Division
        DataRow divide = new DataRow();
        divide ["tablename"] = "arithmetic_operators";
        divide ["id"] = 4;
        divide ["name"] = "divide";
        divide ["symbol"] = "/";
        Func <int, int, int> divideFunc = (x, y) => x / y;
        divide ["operation"] = divideFunc;
        
        operators.Add(divide);
        */

        return operators;
    }

    public static List<DataRow> GetLegalAdditionLHS(DataRow sumData, List<DataRow> dataPool)
    {
        List<int> numbers = GetNumberValues(dataPool);
        
        numbers.Sort();

        int sum = Convert.ToInt32(sumData ["value"]);

        int[] equationPartValues = new int[2];

        bool foundEquationParts = false;

        while (!foundEquationParts)
        {
            int firstPart = numbers[UnityEngine.Random.Range(0, numbers.Count)];

            for(int i = 0; i < 100; ++i)
            {
                int secondPart = numbers[UnityEngine.Random.Range(0, numbers.Count)];

                if((firstPart + secondPart) == sum)
                {
                    equationPartValues[0] = firstPart;
                    equationPartValues[1] = secondPart;
                    foundEquationParts = true;
                    break;
                }
            }
        }

        List<DataRow> equationParts = new List<DataRow>();

        for (int i = 0; i < equationPartValues.Length; ++i)
        {
            equationParts.Add(GetNumberWithValue(dataPool, equationPartValues[i]));
        }

        return equationParts;
    }

    public static DataRow GetLegalSum(List<DataRow> dataPool)
    {
        List<int> numbers = GetNumberValues(dataPool);

        numbers.Sort();

        int minLegal = numbers [0] + numbers [0];

        return GetNumberWithValue(dataPool, UnityEngine.Random.Range(minLegal, numbers[numbers.Count - 1] + 1));
    }

    public static List<int> GetNumberValues(List<DataRow> dataPool)
    {
        List<int> numbers = new List<int>();
        foreach (DataRow data in dataPool)
        {
            if(data["value"] != null)
            {
                numbers.Add(Convert.ToInt32(data["value"]));
            }
        }

        return numbers;
    }

    public static DataRow GetNumberWithValue(List<DataRow> dataPool, int value)
    {
        foreach (DataRow data in dataPool)
        {
            if(data["value"] != null && Convert.ToInt32(data["value"]) == value)
            {
                return data;
            }
        }

        return null;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Legacy
    /// 
    // TODO: Delete when no longer used by LessonContentCoordinator
    public static bool DoesSetContainData(DataRow set, string setAttribute, string dataType)
    {
        string[] ids = GetArray(set, setAttribute);
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

    public static List<DataRow> DeprecatedGetSetData(int setNum, string columnName, string tableName)
    {
        List<DataRow> dataList = new List<DataRow>();
        
        DataTable setTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + setNum);
        
        if (setTable.Rows.Count > 0)
        {
            if (setTable.Rows [0] [columnName] != null)
            {
                string[] dataIds = setTable.Rows [0] [columnName].ToString().Replace(" ", "").Replace("-", "").Replace("'", "").Replace("[", "").Replace("]", "").Trim(new char[] { '\n' }).Split('\n');
                
                foreach (string id in dataIds)
                {
                    try
                    {
                        DataTable dataTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from " + tableName + " WHERE id='" + id + "'");
                        
                        if (dataTable.Rows.Count > 0)
                        {
                            dataList.Add(dataTable.Rows [0]);
                        }
                    } 
                    catch
                    {
                        ////D.Log(String.Format("Getting set {0} for {1} - Invalid ID: {2}", setNum, tableName, id));
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
            return DeprecatedGetSetData(++setNum, columnName, tableName);       
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}