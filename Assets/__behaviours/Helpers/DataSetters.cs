using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DataSetters 
{
    public static List<DataRow> LevelUpCorrectCaptions()
    {
        bool hasIncremented = GameManager.Instance.IncrementCurrentColor();
        
        if (hasIncremented)
        {
            int moduleId = DataHelpers.GetModuleId(GameManager.Instance.currentColor);
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE programmodule_id=" + moduleId);
            GameManager.Instance.AddData("correctcaptions", dt.Rows.FindAll(x => x ["correctsentence"] != null && x ["correctsentence"].ToString() == "t"));
        }
        
        return DataHelpers.GetData("correctcaptions");
    }

    public static List<DataRow> LevelUpQuizQuestions()
    {
        bool hasIncremented = GameManager.Instance.IncrementCurrentColor();
        
        if (hasIncremented)
        {
            int moduleId = DataHelpers.GetModuleId(GameManager.Instance.currentColor);
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE programmodule_id=" + moduleId);
            GameManager.Instance.AddData("quizquestions", dt.Rows.FindAll(x => x ["quiz"] != null && x ["quiz"].ToString() == "t"));
        }

        return DataHelpers.GetData("quizquestions");
    }

    public static List<DataRow> LevelUpPhonemes()
    {
        bool hasIncremented = GameManager.Instance.IncrementCurrentColor();
        
        if (hasIncremented)
        {
            int moduleId = DataHelpers.GetModuleId(GameManager.Instance.currentColor);
            GameManager.Instance.AddData("phonemes", DataHelpers.GetModulePhonemes(moduleId));
        }
        
        return DataHelpers.GetData("phonemes");
    }

    public static List<DataRow> LevelUpWords()
    {
        bool hasIncremented = GameManager.Instance.IncrementCurrentColor();

        if (hasIncremented)
        {
            int moduleId = DataHelpers.GetModuleId(GameManager.Instance.currentColor);
            GameManager.Instance.AddData("words", DataHelpers.GetModuleWords(moduleId));
        }

        return DataHelpers.GetData("words");
    }

    public static List<DataRow> LevelUpTimes()
    {
        bool hasIncremented = GameManager.Instance.IncrementCurrentColor();
        
        //////D.Log(System.String.Format("{0} - {1}", GameManager.Instance.currentColor, hasIncremented));
        
        if (hasIncremented)
        {
            DataSetters.AddModuleTimes(GameManager.Instance.currentColor);
        }

        return DataHelpers.GetData("times");
    }

    public static void AddModuleTimes(ColorInfo.PipColor pipColor)
    {
        List<DataRow> times = DataHelpers.GetModuleTimes(pipColor);
        GameManager.Instance.ReplaceData("times", times);
    }

    public static List<DataRow> LevelUpNumbers()
    {
        bool hasIncremented = GameManager.Instance.IncrementCurrentColor();

        if (hasIncremented)
        {
            DataSetters.AddModuleNumbers(GameManager.Instance.currentColor);
        }

        return DataHelpers.GetData("numbers");
    }

    public static void AddModuleNumbers(ColorInfo.PipColor pipColor)
    {
        int moduleId = DataHelpers.GetModuleId(pipColor);

        List<DataRow> existingNumbers = GameManager.Instance.GetData("numbers");
        existingNumbers.Sort((x, y) => x.GetInt("value").CompareTo(y.GetInt("value")));
        
        int previousHighestNumber = existingNumbers.Count == 0 ? -1 : existingNumbers[existingNumbers.Count - 1].GetInt("value");  
        int newHighestNumber = DataHelpers.GetHighestModuleNumber(pipColor);
        
        GameManager.Instance.AddData("numbers", DataHelpers.CreateNumbers(previousHighestNumber + 1, newHighestNumber));
    }
}
