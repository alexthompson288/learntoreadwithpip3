using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class ScoreInfo : Singleton<ScoreInfo>
{
    Dictionary<string, int> m_quizScores = new Dictionary<string, int>(); // Use strings as key so that we can use it with stories, colors and sessions
    Dictionary<string, int> m_captionScores = new Dictionary<string, int>(); // Use strings as key so that we can use it with stories, colors and sessions

    void SetScore(int score, Dictionary<string, int> dictionary)
    {
        string key = DataHelpers.GetScoreKey();

        if (dictionary.ContainsKey(key))
        {
            if(score > m_quizScores[key])
            {
                dictionary[key] = score;
            }
        } 
        else
        {
            dictionary.Add(key, score);
        }
        
        Save();
    }

    public int GetQuiz(string key)
    {
        return m_quizScores.ContainsKey(key) ? m_quizScores [key] : 0;
    }

    public void SetQuiz(int score)
    {
        SetScore(score, m_quizScores);
    }

    public int GetCaption(string key)
    {
        return m_captionScores.ContainsKey(key) ? m_captionScores [key] : 0;
    }

    public void SetCaption(int score)
    {
        SetScore(score, m_captionScores);
    }

    void Awake()
    {
        Load();
    }

    void Load()
    {
        DataSaver ds = new DataSaver(System.String.Format("VoyageInfo_{0}", UserInfo.Instance.GetCurrentUser()));
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);
        
        if (data.Length != 0)
        {
            int numQuizScores = br.ReadInt32();
            for(int i = 0; i < numQuizScores; ++i)
            {
                m_quizScores.Add(br.ReadString(), br.ReadInt32());
            }
            
            int numCaptionScores = br.ReadInt32();
            for(int i = 0; i < numCaptionScores; ++i)
            {
                m_captionScores.Add(br.ReadString(), br.ReadInt32());
            }
        }
        
        br.Close();
        data.Close();
    }
    
    void Save()
    {
        DataSaver ds = new DataSaver(System.String.Format("VoyageInfo_{0}", UserInfo.Instance.GetCurrentUser()));
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);
        
        bw.Write(m_quizScores.Count);
        foreach (KeyValuePair<string, int> kvp in m_quizScores)
        {
            bw.Write(kvp.Key);
            bw.Write(kvp.Value);
        }
        
        bw.Write(m_captionScores.Count);
        foreach (KeyValuePair<string, int> kvp in m_captionScores)
        {
            bw.Write(kvp.Key);
            bw.Write(kvp.Value);
        }
        
        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }
}
