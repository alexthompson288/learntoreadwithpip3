using UnityEngine;
using System;
using System.IO;

public class FileBrowser : MonoBehaviour 
{
    public void LogPersistentDataPath()
    {
        //////D.Log("LOG PERSISTENT DATAPATH");
        ProcessFolder(Application.persistentDataPath);
    }

    public void LogDataPath()
    {
        //////D.Log("LOG DATAPATH");
        ProcessFolder(Application.dataPath);
    }

    void ProcessFolder(string f) 
    {
        //////D.Log("Folder: " + f);
            
        var txtFiles = Directory.GetFiles(f);
        foreach (string currentFile in txtFiles)
        {
            //////D.Log("File: " + currentFile);
        }
            
        string[] subs = Directory.GetDirectories(f);
        foreach (string sub in subs)
        {
            ProcessFolder(sub);  
        }
    }
        
    // Use this for initialization
    void Start () 
    {
        ////////D.Log("LOG DATAPATH");
        //ProcessFolder(Application.dataPath);

        ////////D.Log("LOG PERSISTENT DATAPATH");
        //ProcessFolder(Application.persistentDataPath);
    }
}
