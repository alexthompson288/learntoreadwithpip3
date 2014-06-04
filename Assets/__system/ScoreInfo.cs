using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class ScoreInfo : Singleton<ScoreInfo>
{
    class ScoreTracker
    {
        string m_game;
        string m_level;
        int m_score;
        int m_targetScore;
        float m_time;

        public ScoreTracker(string game, string level, int score, int targetScore, float time)
        {
            m_game = game;
            m_level = level;
            m_score = score;
            m_targetScore = targetScore;
            m_time = time;
        }

        public string GetGame()
        {
            return m_game;
        }

        public string GetLevel()
        {
            return m_level;
        }

        public int GetScore()
        {
            return m_score;
        }

        public int GetTargetScore()
        {
            return m_targetScore;
        }

        public float GetProportionalScore()
        {
            return (float)m_score / (float)m_targetScore;
        }

        public float GetTime()
        {
            return m_time;
        }
    }

    List<ScoreTracker> m_scoreTrackers = new List<ScoreTracker>();

    public int GetScore(string game, string level)
    {
        ScoreTracker tracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetLevel() == level);
        return tracker != null ? tracker.GetScore() : 0;
    }

    public int GetTargetScore(string game, string level)
    {
        ScoreTracker tracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetLevel() == level);
        return tracker != null ? tracker.GetTargetScore() : 0;
    }

    public float GetTime(string game, string level)
    {
        ScoreTracker tracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetLevel() == level);
        return tracker != null ? tracker.GetTime() : 0f;
    }

    public void NewScore(int score, int targetScore, float time = 0f)
    {
        string game = DataHelpers.GetGameName();
        string level = DataHelpers.GetScoreLevel();

        ScoreTracker newTracker = new ScoreTracker(game, level, score, targetScore, time);

        ScoreTracker oldTracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetLevel() == level);

        if (oldTracker == null)
        {
            m_scoreTrackers.Add(newTracker);
        } 
        else if (newTracker.GetProportionalScore() > oldTracker.GetProportionalScore())
        {
            m_scoreTrackers.Remove(oldTracker);
            m_scoreTrackers.Add(newTracker);
        }

        Save();
    }

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
        m_scoreTrackers.Clear();
        Load();
    }

    void Load()
    {
        DataSaver ds = new DataSaver(System.String.Format("VoyageInfo_{0}", UserInfo.Instance.GetCurrentUser()));
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);
        
        if (data.Length != 0)
        {
            int numTrackers = br.ReadInt32();
            for(int i = 0; i < numTrackers; ++i)
            {
                string game = br.ReadString();
                string level = br.ReadString();
                int score = br.ReadInt32();
                int targetScore = br.ReadInt32();
                float time = br.ReadSingle();

                m_scoreTrackers.Add(new ScoreTracker(game, level, score, targetScore, time));
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

        bw.Write(m_scoreTrackers.Count);
        foreach (ScoreTracker tracker in m_scoreTrackers)
        {
            bw.Write(tracker.GetGame());
            bw.Write(tracker.GetLevel());
            bw.Write(tracker.GetScore());
            bw.Write(tracker.GetTargetScore());
            bw.Write(tracker.GetTime());
        }

        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }

    /*
    void SaveDictionary(BinaryWriter bw, Dictionary<string, ScoreTracker> dictionary)
    {
        bw.Write(dictionary.Count);
        foreach (KeyValuePair<string, ScoreTracker> kvp in dictionary)
        {
            bw.Write(kvp.Key);
            bw.Write(kvp.Value.GetScore());
            bw.Write(kvp.Value.GetTargetScore());
            bw.Write(kvp.Value.GetTime());
        }
    }

    void LoadDictionary(BinaryReader br, Dictionary<string, ScoreTracker> dictionary)
    {
        int numTrackers = br.ReadInt32();
        for (int i = 0; i < numTrackers; ++i)
        {
            string key = br.ReadString();
            int score = br.ReadInt32();
            int targetScore = br.ReadInt32();
            float time = br.ReadSingle();

            dictionary[key] = new ScoreTracker(score, targetScore, time);
        }
    }

    Dictionary<string, ScoreTracker> m_quizScores = new Dictionary<string, ScoreTracker>(); // Use strings as key so that we can use it with stories, colors and sessions
    Dictionary<string, ScoreTracker> m_captionScores = new Dictionary<string, ScoreTracker>(); // Use strings as key so that we can use it with stories, colors and sessions
    
    void SetScore(ScoreTracker score, Dictionary<string, ScoreTracker> dictionary)
    {
        string key = DataHelpers.GetScoreKey();

        if (dictionary.ContainsKey(key))
        {
            if(score.GetProportionalScore() > m_quizScores[key].GetProportionalScore())
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

    public int GetQuizScore(string key)
    {
        return m_quizScores.ContainsKey(key) ? m_quizScores [key].GetScore() : 0;
    }

    public int GetQuizTargetScore(string key)
    {
        return m_quizScores.ContainsKey(key) ? m_quizScores [key].GetTargetScore() : 0;
    }

    public void SetQuiz(int score, int targetScore, float time = 0)
    {
        SetScore(new ScoreTracker(score, targetScore, time), m_quizScores);
    }

    public int GetCaptionScore(string key)
    {
        return m_captionScores.ContainsKey(key) ? m_captionScores [key].GetScore() : 0;
    }

    public int GetCaptionTargetScore(string key)
    {
        return m_captionScores.ContainsKey(key) ? m_captionScores [key].GetTargetScore() : 0;
    }

    public void SetCaption(int score, int targetScore, float time = 0)
    {
        SetScore(new ScoreTracker(score, targetScore, time), m_captionScores);
    }
    */
}
