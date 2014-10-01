using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class DeleteNonPlusAudio : MonoBehaviour 
{
//    IEnumerator Start()
//    {
//        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
//
//        GameManager.Instance.SetProgramme(ProgrammeInfo.plusReading);
//
//        ColorInfo.PipColor[] pipColors = new ColorInfo.PipColor[]
//        {
//            ColorInfo.PipColor.Turquoise,
//            ColorInfo.PipColor.Purple,
//            ColorInfo.PipColor.Gold,
//            ColorInfo.PipColor.White
//        };
//
//        List<DataRow> allWords = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words").Rows;
//
//        List<DataRow> plusWords = new List<DataRow>();
//        foreach (ColorInfo.PipColor pipColor in pipColors)
//        {
//            int moduleId = DataHelpers.GetModuleId(pipColor);
//            plusWords.AddRange(DataHelpers.GetModuleWords(moduleId));
//        }
//
//        List<DataRow> nonPlusWords = allWords.Except(plusWords).ToList();
//
//        nonPlusWords.Sort(delegate(DataRow x, DataRow y) { return x["word"].ToString().CompareTo(y["word"].ToString()); });
//
//        string path = Application.dataPath + "/ResourcesBase/CommonAudio/Resources/audio/words/";
//
//        foreach (DataRow word in nonPlusWords)
//        {
//            string fullPath = string.Format("{0}{1}.mp3", path, word["word"]);
//            if(File.Exists(fullPath))
//            {
//                File.Delete(fullPath);
//            }
//        }
//    }
}
