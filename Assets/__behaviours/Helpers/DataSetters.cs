using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DataSetters 
{
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
