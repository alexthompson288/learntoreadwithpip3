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
            string[] users = UserInfo.Instance.GetUserNames();
            foreach(string user in users)
            {
                Save(user);
            }
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
    //Dictionary<string, bool> m_answers = new Dictionary<string, bool>();
    List<Answer> m_answers = new List<Answer>();

    class Answer
    {
        string m_identifier;
        string m_dataType;
        bool m_isCorrect;

        public Answer(string myIdentifier, string myDataType, bool myIsCorrect)
        {
            m_identifier = myIdentifier;
            m_dataType = myDataType;
            m_isCorrect = myIsCorrect;
        }

        public string GetIdentifier()
        {
            return m_identifier;
        }

        public string GetDataType()
        {
            return m_dataType;
        }

        public bool IsCorrect()
        {
            return m_isCorrect;
        }
    }

    public void NewAnswer(string identifier, string dataType, bool isCorrect)
    {
        int answerIndex = m_answers.FindIndex(x => x.GetIdentifier() == identifier && x.GetDataType() == dataType);

        if (answerIndex != -1)
        {
            m_answers.RemoveAt(answerIndex);
        }

        m_answers.Add(new Answer(identifier, dataType, isCorrect));
        Save();
    }

    public bool IsAnswer(string identifier, string dataType)
    {
        return m_answers.Find(x => x.GetDataType() == dataType && x.GetIdentifier() == identifier) != null;
    }

    public bool IsCorrect(string identifier, string dataType)
    {
        Answer answer = m_answers.Find(x => x.GetDataType() == dataType && x.GetIdentifier() == identifier);
        return answer != null ? answer.IsCorrect() : false;
    }

    public void ClearAnswers(string dataType)
    {
        m_answers.RemoveAll(x => x.GetDataType() == dataType);
        Save();
    }

    /*
    public void NewAnswer(string s, bool isCorrect)
    {
        m_answers [s] = isCorrect;
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

    public bool IsAnswer(int id, string dataType)
    {
        foreach(KeyValuePair<string, bool> kvp in m_answers)
        {
            string idString = Regex.Match(kvp.Key, @"\d+").Value;

            string dataString = Regex.Replace(kvp.Key, @"[\d-]", string.Empty);
            dataString = dataString.Replace("_", "");
            
            //D.Log(String.Format("{0} - {1}", idString, dataString));
            
            if(dataString == dataType && idString == id.ToString())
            {
                return true;
            }
        }
        
        return false;
    }

    public bool IsCorrect(int id, string dataType)
    {
        foreach(KeyValuePair<string, bool> kvp in m_answers)
        {
            string idString = Regex.Match(kvp.Key, @"\d+").Value;

            string dataString = Regex.Replace(kvp.Key, @"[\d-]", string.Empty);
            dataString = dataString.Replace("_", "");

            //D.Log(String.Format("{0} - {1}", idString, dataString));
            
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
    */

    void Load()
    {
        DataSaver ds = new DataSaver(String.Format("BankInfo_{0}", UserInfo.Instance.GetCurrentUserName()));
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);
        
        if (data.Length != 0)
        {
            int numAnswers = br.ReadInt32();
            for(int i = 0; i < numAnswers; ++i)
            {
                m_answers.Add(new Answer(br.ReadString(), br.ReadString(), br.ReadBoolean()));
            }
            // TODO
            /*
            int numAnswers = br.ReadInt32();
            for(int i = 0; i < numAnswers; ++i)
            {
                m_answers.Add(br.ReadString(), br.ReadBoolean());
            }
            */
        }
        
        br.Close();
        data.Close();
    }
    
    void Save(string user = null)
    {
        if (String.IsNullOrEmpty(user))
        {
            user = UserInfo.Instance.GetCurrentUserName();
        }

        DataSaver ds = new DataSaver(String.Format("BankInfo_{0}", user));
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);

        // TODO
        bw.Write(m_answers.Count);
        foreach (Answer answer in m_answers)
        {
            bw.Write(answer.GetIdentifier());
            bw.Write(answer.GetDataType());
            bw.Write(answer.IsCorrect());
        }
        /*
        bw.Write(m_answers.Count);
        foreach (KeyValuePair<string, bool> kvp in m_answers)
        {
            bw.Write(kvp.Key);
            bw.Write(kvp.Value);
        }
        */
        
        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }
}
