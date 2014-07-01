using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class ScoreInfo : Singleton<ScoreInfo>
{
    class ScoreTracker
    {
        string m_game;
        string m_type;
        float m_time;
        int m_score;
        int m_targetScore;
        int m_stars;

        public ScoreTracker(string myGame, string myType, float myTime, int myScore, int myTargetScore, int myStars)
        {
            m_game = myGame;
            m_type = myType;
            m_time = myTime;
            m_score = myScore;
            m_targetScore = myTargetScore;
            m_stars = myStars;
        }

        public string GetGame()
        {
            return m_game;
        }

        public string GetType()
        {
            return m_type;
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

        public int GetStars()
        {
            return m_stars;
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
        GameManager.Instance.CompletedAll += OnGameCompleteOrCancel;
        GameManager.Instance.Cancelling += OnGameCompleteOrCancel;

#if UNITY_EDITOR
        if(m_overwrite)
        {
            D.Log("OVERWRITING SCOREINFO");
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

    void OnGameCompleteOrCancel()
    {
        m_scoreType = "";
    }

    // New High Scores
    ScoreTracker m_newHighScore = null;
    int m_previousHighScore;

    public bool HasNewHighScore()
    {
        return m_newHighScore != null;
    }

    public string GetNewHighScoreGame()
    {
        return m_newHighScore.GetGame();
    }

    public int GetNewHighScoreStars()
    {
        return m_newHighScore.GetStars();
    }

    public void RemoveNewHighScore()
    {
        m_newHighScore = null;
    }

    public int GetPreviousHighScoreStars()
    {
        return m_previousHighScore;
    }

    // Standard Score Trackers
    List<ScoreTracker> m_scoreTrackers = new List<ScoreTracker>();

    public int GetScore(string game, string type)
    {
        ScoreTracker tracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetType() == type);
        return tracker != null ? tracker.GetScore() : 0;
    }

    public int GetTargetScore(string game, string type)
    {
        ScoreTracker tracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetType() == type);
        return tracker != null ? tracker.GetTargetScore() : 0;
    }

    public float GetProportionalScore(string game, string type)
    {
        ScoreTracker tracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetType() == type);
        return tracker != null ? tracker.GetProportionalScore() : 0;
    }

    public float GetTime(string game, string type)
    {
        ScoreTracker tracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetType() == type);
        return tracker != null ? tracker.GetTime() : 0f;
    }

    public int GetStars(string game, string type)
    {
        ScoreTracker tracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetType() == type);
        return tracker != null ? tracker.GetStars() : 0;
    }

    public void NewScore(float myTime, int myScore, int myTargetScore, int myStars)
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

            ScoreTracker newTracker = new ScoreTracker(game, type, myTime, myScore, myTargetScore, myStars);

            ScoreTracker oldTracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetType() == type);


            if (oldTracker == null)
            {
                m_scoreTrackers.Add(newTracker);

                m_newHighScore = newTracker;
                m_previousHighScore = 0;
            }
            else 
            {
                int newStars = newTracker.GetStars();
                int oldStars = oldTracker.GetStars();

                // Even if newStars == oldStars, the new score might be considered better if it has a better time or proportional score. This would allow us to add features in the future (eg leaderboards etc)
                if (newStars > oldStars 
                    || (newStars == oldStars && (newTracker.GetTime() < oldTracker.GetTime() || (newTracker.GetProportionalScore() > oldTracker.GetProportionalScore())))) 
                {
                    D.Log("NEW HIGH SCORE");
                    m_scoreTrackers.Remove(oldTracker);
                    m_scoreTrackers.Add(newTracker);

                    m_newHighScore = newTracker;
                    m_previousHighScore = oldTracker.GetStars();
                }
            }

            Save();
        }
    }

    public static void RefreshStars(UISprite[] starSprites, string game, string type)
    {
        System.Array.Sort(starSprites, CollectionHelpers.LocalLeftToRight);

        int numStars = Instance.GetStars(game, type);
        
        for (int i = 0; i < starSprites.Length; ++i)
        {
            string spriteName = i < numStars ? "star_active_512" : "star_inactive_512";
            starSprites[i].spriteName = spriteName;
        }
    }

    public static int CalculateScoreStars(int score, int targetScore)
    {
        int stars = 0;
        
        float proportionalScore = (float)score / (float)targetScore;
        
        if (proportionalScore >= 0.7f)
        {
            stars = 3;
        }
        else if (proportionalScore >= 0.4f)
        {
            stars = 2;
        }
        else
        {
            stars = 1;
        }
        
        if (score == 0)
        {
            stars = 0;
        }
        
        return stars;
    }
    
    public static int CalculateTimeStars(float time, float twoStars, float threeStars)
    {
        if (time <= threeStars)
        {
            return 3;
        }
        else if (time <= twoStars)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    void OnChangeUser()
    {
        m_scoreTrackers.Clear();
        Load();
    }

    void Load()
    {
        DataSaver ds = new DataSaver(System.String.Format("ScoreInfo_{0}", UserInfo.Instance.GetCurrentUser()));
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);

        if (data.Length != 0)
        {
            int numTrackers = br.ReadInt32();
            for(int i = 0; i < numTrackers; ++i)
            {
                /*
                string m_game;
                string m_type;
                float m_time;
                int m_score;
                int m_targetScore;
                int m_stars;
                 */ 

                string game = br.ReadString();
                string type = br.ReadString();
                float time = br.ReadSingle();
                int score = br.ReadInt32();
                int targetScore = br.ReadInt32();
                int stars = br.ReadInt32();

                m_scoreTrackers.Add(new ScoreTracker(game, type, time, score, targetScore, stars));
            }
        }
        
        br.Close();
        data.Close();
    }

    void Save(string userName = null)
    {
        if (System.String.IsNullOrEmpty(userName))
        {
            userName = UserInfo.Instance.GetCurrentUser();
        }

        DataSaver ds = new DataSaver(System.String.Format("ScoreInfo_{0}", userName));
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);

        D.Log("ScoreInfo.Save: " + System.String.Format("ScoreInfo_{0}", userName));

        D.Log("SAVING DATA: " + m_scoreTrackers.Count);

        bw.Write(m_scoreTrackers.Count);
        foreach (ScoreTracker tracker in m_scoreTrackers)
        {
            bw.Write(tracker.GetGame());
            bw.Write(tracker.GetType());
            bw.Write(tracker.GetTime());
            bw.Write(tracker.GetScore());
            bw.Write(tracker.GetTargetScore());
            bw.Write(tracker.GetStars());
        }

        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }
}
