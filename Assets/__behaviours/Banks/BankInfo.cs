using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;

public class BankInfo : Singleton<BankInfo> 
{
#if UNITY_EDITOR
    [SerializeField]
    private bool m_overwrite;
#endif

    void Start()
    {
#if UNITY_EDITOR
        if(m_overwrite)
        {
            Save();
        }
#endif

        Load();

        UserInfo.Instance.ChangingUser += OnChangeUser;
    }

    void OnChangeUser()
    {
        m_answers.Clear();
        Load();
    }

    // Answer string stores both id and datatype, they need to be stored in a single string because ids are shared between tables and so keys would not be unique if they were kept separate
    Dictionary<string, bool> m_answers = new Dictionary<string, bool>();

    public void NewAnswer(string s, bool isCorrect)
    {
        m_answers [s] = isCorrect;
        Save();
    }

    public void NewAnswer(int id, bool isCorrect)
    {
        NewAnswer(id, GameManager.Instance.dataType, isCorrect);
        Save();
    }

    public void NewAnswer(int id, string dataType, bool isCorrect)
    {
        m_answers [id.ToString() + "_" + dataType] = isCorrect;
        Save();
    }

    // This is used for only for alphabet
    public bool IsAnswer(string s)
    {
        foreach (KeyValuePair<string, bool> kvp in m_answers)
        {
            if(s == kvp.Key)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsCorrect(string s)
    {
        foreach (KeyValuePair<string, bool> kvp in m_answers)
        {
            if(s == kvp.Key && kvp.Value)
            {
                return true;
            }
        }
        
        return false;
    }

    public bool IsAnswer(int id)
    {
        return IsAnswer(id, GameManager.Instance.dataType);
    }

    public bool IsAnswer(int id, string dataType)
    {
        foreach(KeyValuePair<string, bool> kvp in m_answers)
        {
            string idString = Regex.Match(kvp.Key, @"\d+").Value;

            string dataString = Regex.Replace(kvp.Key, @"[\d-]", string.Empty);
            dataString = dataString.Replace("_", "");
            
            //Debug.Log(String.Format("{0} - {1}", idString, dataString));
            
            if(dataString == dataType && idString == id.ToString())
            {
                return true;
            }
        }
        
        return false;
    }

    public bool IsCorrect(int id)
    {
        return IsCorrect(id, GameManager.Instance.dataType);
    }

    public bool IsCorrect(int id, string dataType)
    {
        foreach(KeyValuePair<string, bool> kvp in m_answers)
        {
            string idString = Regex.Match(kvp.Key, @"\d+").Value;

            string dataString = Regex.Replace(kvp.Key, @"[\d-]", string.Empty);
            dataString = dataString.Replace("_", "");

            //Debug.Log(String.Format("{0} - {1}", idString, dataString));
            
            if(dataString == dataType && idString == id.ToString())
            {
                return kvp.Value;
            }
        }

        return false;
    }

    public void ClearAnswers()
    {
        m_answers.Clear();
        Save();
    }

    void Load()
    {
        DataSaver ds = new DataSaver(String.Format("BankInfo_{0}", UserInfo.Instance.GetCurrentUser()));
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);
        
        if (data.Length != 0)
        {
            int numAnswers = br.ReadInt32();
            for(int i = 0; i < numAnswers; ++i)
            {
                m_answers.Add(br.ReadString(), br.ReadBoolean());
            }
        }
        
        br.Close();
        data.Close();
    }
    
    void Save()
    {
        DataSaver ds = new DataSaver(String.Format("BankInfo_{0}", UserInfo.Instance.GetCurrentUser()));
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);
        
        bw.Write(m_answers.Count);
        foreach (KeyValuePair<string, bool> kvp in m_answers)
        {
            bw.Write(kvp.Key);
            bw.Write(kvp.Value);
        }
        
        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }
}
