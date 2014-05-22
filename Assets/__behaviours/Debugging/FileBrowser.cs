using UnityEngine;
using System;
using System.IO;

public class FileBrowser : MonoBehaviour 
{
    public void LogPersistentDataPath()
    {
        Debug.Log("LOG PERSISTENT DATAPATH");
        ProcessFolder(Application.persistentDataPath);
    }

    public void LogDataPath()
    {
        Debug.Log("LOG DATAPATH");
        ProcessFolder(Application.dataPath);
    }

    void ProcessFolder(string f) 
    {
        Debug.Log("Folder: " + f);
            
        var txtFiles = Directory.GetFiles(f);
        foreach (string currentFile in txtFiles)
        {
            Debug.Log("File: " + currentFile);
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
        //Debug.Log("LOG DATAPATH");
        //ProcessFolder(Application.dataPath);

        //Debug.Log("LOG PERSISTENT DATAPATH");
        //ProcessFolder(Application.persistentDataPath);
    }
}
