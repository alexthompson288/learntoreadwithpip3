using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DataSetters 
{
    public static void LevelUpNumbers(List<DataRow> numberPool)
    {
        D.Log("CompleteEquationCoordinator.OnLevelUp()");
        bool hasIncremented = GameManager.Instance.IncrementCurrentColor();
        
        D.Log(System.String.Format("{0} - {1}", GameManager.Instance.currentColor, hasIncremented));
        
        if (hasIncremented)
        {
            DataSetters.AddModuleNumbers(GameManager.Instance.currentColor);
            numberPool = DataHelpers.GetData("numbers");
        }
    }

    public static void AddModuleTimes(ColorInfo.PipColor pipColor)
    {
        List<DataRow> times = DataHelpers.GetModuleTimes(pipColor);
        GameManager.Instance.ReplaceData("times", times);
    }

    public static void AddModuleNumbers(ColorInfo.PipColor pipColor)
    {
        D.Log("DataSetters.AddModuleNumbers()");

        int moduleId = DataHelpers.GetModuleId(pipColor);

        List<DataRow> existingNumbers = GameManager.Instance.GetData("numbers");
        existingNumbers.Sort((x, y) => x.GetInt("value").CompareTo(y.GetInt("value")));
        
        int previousHighestNumber = existingNumbers.Count == 0 ? -1 : existingNumbers[existingNumbers.Count - 1].GetInt("value");  
        int newHighestNumber = DataHelpers.GetHighestModuleNumber(pipColor);

        D.Log("previousHighest: " + previousHighestNumber);
        D.Log("newHighest: " + newHighestNumber);
        
        GameManager.Instance.AddData("numbers", DataHelpers.CreateNumbers(previousHighestNumber + 1, newHighestNumber));
    }
}
