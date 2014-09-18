using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class PlusScoreInfo : Singleton<PlusScoreInfo>
{
    class ScoreTracker
    {
        string m_game;
        string m_type;
        float m_time;
        int m_score;
        int m_maxColor;
        
        public ScoreTracker(string myGame, string myType, float myTime, int myScore, int myMaxColor)
        {
            m_game = myGame;
            m_type = myType;
            m_time = myTime;
            m_score = myScore;
            m_maxColor = myMaxColor;
        }
        
        public string GetGame()
        {
            return m_game;
        }
        
        public string GetType()
        {
            return m_type;
        }

        public float GetTime()
        {
            return m_time;
        }
        
        public int GetScore()
        {
            return m_score;
        }

        public int GetMaxColor()
        {
            return m_maxColor;
        }
    }
    
    string m_scoreType = "";
    
    public void SetScoreType(string scoreType)
    {
        m_scoreType = scoreType;
    }
    
    #if UNITY_EDITOR
    [SerializeField]
    private bool m_overwrite;
    #endif
    
    void Start()
    {
        GameManager.Instance.CompletedAll += OnGameComplete;
        GameManager.Instance.Cancelling += OnGameCancel;
        
        #if UNITY_EDITOR
        if(m_overwrite)
        {
            string[] users = UserInfo.Instance.GetUserNames();
            
            if(users.Length > 0)
            {
                foreach(string user in users)
                {
                    Save(user);
                }
            }
            else
            {
                Save();
            }
        }
        #endif
        
        Load();
        
        UserInfo.Instance.ChangingUser += OnChangeUser;
    }
    
    void OnGameComplete()
    {
        m_scoreType = "";
    }
    
    void OnGameCancel()
    {
        m_newHighScoreTracker = null;
        m_scoreType = "";
    }
    
    // New High Scores
    ScoreTracker m_newHighScoreTracker = null;
    ScoreTracker m_oldHighScoreTracker = null;
    
    public bool HasUnlockTrackers()
    {
        return m_newHighScoreTracker != null && m_oldHighScoreTracker != null;
    }

    public bool HasNewMaxColor()
    {
        return HasUnlockTrackers() && m_newHighScoreTracker.GetMaxColor() > m_oldHighScoreTracker.GetMaxColor();
    }

    public bool HasNewHighScore()
    {
        return HasUnlockTrackers() && m_newHighScoreTracker.GetScore() > m_oldHighScoreTracker.GetScore();
    }
    
    public string GetUnlockGame()
    {
        return m_newHighScoreTracker.GetGame();
    }

    public int GetNewMaxColor()
    {
        return m_newHighScoreTracker.GetMaxColor();
    }

    public int GetNewScore()
    {
        return m_newHighScoreTracker.GetScore();
    }
    
    public int GetOldHighScore()
    {
        return m_oldHighScoreTracker.GetScore();
    }

    public int GetOldMaxColor()
    {
        return m_oldHighScoreTracker.GetMaxColor();
    }

    public void ClearUnlockTrackers()
    {
        m_newHighScoreTracker = null;
        m_oldHighScoreTracker = null;
    }
    
    // Standard Score Trackers
    List<ScoreTracker> m_scoreTrackers = new List<ScoreTracker>();
    
    public int GetScore(string game, string type)
    {
        ScoreTracker tracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetType() == type);
        return tracker != null ? tracker.GetScore() : 0;
    }
    
    public float GetTime(string game, string type)
    {
        ScoreTracker tracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetType() == type);
        return tracker != null ? tracker.GetTime() : 0f;
    }
    
    public int GetMaxColor(string game, string type)
    {
        ScoreTracker tracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetType() == type);
        return tracker != null ? tracker.GetMaxColor() : 0;
    }
    
    public void NewScore(float myTime, int myScore, int myMaxColor)
    {
        if (SessionInformation.Instance.GetNumPlayers() < 2)
        {
            string game = DataHelpers.GetGameName();
            if(System.String.IsNullOrEmpty(game))
            {
                game = "default";
            }
            
            string type = m_scoreType;
            if(System.String.IsNullOrEmpty(type))
            {
                type = "default";
            }
            
            ScoreTracker newTracker = new ScoreTracker(game, type, myTime, myScore, myMaxColor);
            ScoreTracker oldTracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetType() == type);
            
            if (oldTracker == null)
            {
                m_scoreTrackers.Add(newTracker);
                
                m_newHighScoreTracker = newTracker;
                m_oldHighScoreTracker = new ScoreTracker(game, type, 0, 0, 1);
            }
            else 
            {
                int newScore = newTracker.GetScore();
                int oldScore = oldTracker.GetScore();
                
                // Even if newStars == oldStars, the new score might be considered better if it has a better time or proportional score. This would allow us to add features in the future (eg leaderboards etc)
                if (newScore > oldScore || (newScore == oldScore && (newTracker.GetTime() < oldTracker.GetTime()))) 
                {
                    m_scoreTrackers.Remove(oldTracker);
                    m_scoreTrackers.Add(newTracker);
                    
                    m_newHighScoreTracker = newTracker;
                    m_oldHighScoreTracker = oldTracker;
                }
            }
            
            Save();
        }
    }

    void OnChangeUser()
    {
        m_scoreTrackers.Clear();
        Load();
    }
    
    void Load()
    {
        DataSaver ds = new DataSaver(System.String.Format("PlusScoreInfo_{0}", UserInfo.Instance.GetCurrentUserName()));
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);
        
        if (data.Length != 0)
        {
            int numTrackers = br.ReadInt32();
            for(int i = 0; i < numTrackers; ++i)
            {
                string game = br.ReadString();
                string type = br.ReadString();
                float time = br.ReadSingle();
                int score = br.ReadInt32();
                int maxColor = br.ReadInt32();

                m_scoreTrackers.Add(new ScoreTracker(game, type, time, score, maxColor));
            }
        }
        
        br.Close();
        data.Close();
    }
    
    void Save(string userName = null)
    {
        if (System.String.IsNullOrEmpty(userName))
        {
            userName = UserInfo.Instance.GetCurrentUserName();
        }
        
        DataSaver ds = new DataSaver(System.String.Format("PlusScoreInfo_{0}", userName));
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);
        
        bw.Write(m_scoreTrackers.Count);

        foreach (ScoreTracker tracker in m_scoreTrackers)
        {
            bw.Write(tracker.GetGame());
            bw.Write(tracker.GetType());
            bw.Write(tracker.GetTime());
            bw.Write(tracker.GetScore());
            bw.Write(tracker.GetMaxColor());
        }
        
        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }
}

