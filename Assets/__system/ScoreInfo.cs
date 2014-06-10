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
        int m_score;
        int m_targetScore;
        float m_time;
        float m_twoStar;
        float m_threeStar;

        public ScoreTracker(string game, string type, int score, int targetScore, float time, float twoStar, float threeStar)
        {
            m_game = game;
            m_type = type;
            m_score = score;
            m_targetScore = targetScore;
            m_time = time;
            m_twoStar = twoStar;
            m_threeStar = threeStar;
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

        public float GetTwoStar()
        {
            return m_twoStar;
        }

        public float GetThreeStar()
        {
            return m_threeStar;
        }
    }

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

    public float GetTwoStar(string game, string type)
    {
        ScoreTracker tracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetType() == type);
        return tracker != null ? tracker.GetTwoStar() : 0f;
    }

    public float GetThreeStar(string game, string type)
    {
        ScoreTracker tracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetType() == type);
        return tracker != null ? tracker.GetThreeStar() : 0f;
    }

    public void NewScore(int score, int targetScore, float time, float twoStar, float threeStar)
    {
        if (SessionInformation.Instance.GetNumPlayers() < 2)
        {
            string game = DataHelpers.GetGameName();
            if(System.String.IsNullOrEmpty(game))
            {
                game = "default";
            }

            string type = DataHelpers.GetScoreType();
            if(System.String.IsNullOrEmpty(type))
            {
                type = "default";
            }

            ScoreTracker newTracker = new ScoreTracker(game, type, score, targetScore, time, twoStar, threeStar);

            ScoreTracker oldTracker = m_scoreTrackers.Find(x => x.GetGame() == game && x.GetType() == type);

            if (oldTracker == null)
            {
                m_scoreTrackers.Add(newTracker);
            } else if (newTracker.GetTime() < oldTracker.GetTime())
            {
                m_scoreTrackers.Remove(oldTracker);
                m_scoreTrackers.Add(newTracker);
            }

            Save();
        }
    }

    public static void RefreshScoreStars(UISprite[] starSprites, string game, string type)
    {
        System.Array.Sort(starSprites, CollectionHelpers.ComparePosX);

        int numStars = 0;
        int targetScore = Instance.GetTargetScore(game, type);
        
        if (targetScore > 0)
        {
            float proportionalScore = Instance.GetProportionalScore(game, type);

            if(proportionalScore > 0.6f)
            {
                numStars = 3;
            }
            else if(proportionalScore > 0.3f)
            {
                numStars = 2;
            }
            else
            {
                numStars = 1;
            }
        }
        
        for (int i = 0; i < starSprites.Length; ++i)
        {
            string spriteName = i < numStars ? "star_active_512" : "star_inactive_512";
            starSprites[i].spriteName = spriteName;
        }
    }

    public static void RefreshTimeStars(UISprite[] starSprites, string game, string type)
    {
        System.Array.Sort(starSprites, CollectionHelpers.ComparePosX);

        int numStars = 0;
        int targetScore = Instance.GetTargetScore(game, type);
        
        if (targetScore > 0)
        {
            float time = Instance.GetTime(game, type);
            
            if(time < Instance.GetThreeStar(game, type))
            {
                numStars = 3;
            }
            else if(time < Instance.GetTwoStar(game, type))
            {
                numStars = 2;
            }
            else
            {
                numStars = 1;
            }
        }
        
        for (int i = 0; i < starSprites.Length; ++i)
        {
            string spriteName = i < numStars ? "star_active_512" : "star_inactive_512";
            starSprites[i].spriteName = spriteName;
        }
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
            Debug.Log("OVERWRITING SCOREINFO");
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
        DataSaver ds = new DataSaver(System.String.Format("ScoreInfo_{0}", UserInfo.Instance.GetCurrentUser()));
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);

        if (data.Length != 0)
        {
            int numTrackers = br.ReadInt32();
            for(int i = 0; i < numTrackers; ++i)
            {
                string game = br.ReadString();
                string type = br.ReadString();
                int score = br.ReadInt32();
                int targetScore = br.ReadInt32();
                float time = br.ReadSingle();
                float twoStar = br.ReadSingle();
                float threeStar = br.ReadSingle();

                m_scoreTrackers.Add(new ScoreTracker(game, type, score, targetScore, time, twoStar, threeStar));
                //m_scoreTrackers.Add(new ScoreTracker(game, type, score, targetScore, time, 30, 60));
            }
        }
        
        br.Close();
        data.Close();
    }

    void Save()
    {
        DataSaver ds = new DataSaver(System.String.Format("ScoreInfo_{0}", UserInfo.Instance.GetCurrentUser()));
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);

        Debug.Log("ScoreInfo.Save: " + System.String.Format("ScoreInfo_{0}", UserInfo.Instance.GetCurrentUser()));

        Debug.Log("SAVING DATA: " + m_scoreTrackers.Count);

        bw.Write(m_scoreTrackers.Count);
        foreach (ScoreTracker tracker in m_scoreTrackers)
        {
            bw.Write(tracker.GetGame());
            bw.Write(tracker.GetType());
            bw.Write(tracker.GetScore());
            bw.Write(tracker.GetTargetScore());
            bw.Write(tracker.GetTime());
            bw.Write(tracker.GetTwoStar());
            bw.Write(tracker.GetThreeStar());
        }

        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }
}
