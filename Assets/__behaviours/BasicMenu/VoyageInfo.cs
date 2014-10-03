using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class VoyageInfo : Singleton<VoyageInfo>
{
#if UNITY_EDITOR
    [SerializeField]
    private bool m_overwrite;
#endif

    int m_currentSessionId;
    public int currentSessionId
    {
        get
        {
            return m_currentSessionId;
        }
    }

    public void SetCurrentSessionId(int myCurrentSessionId)
    {
        m_currentSessionId = myCurrentSessionId;
    }

    // Session Backgrounds. Saved between app launches
    Dictionary<int, string> m_sessionBackgrounds = new Dictionary<int, string>();
    
    public void AddSessionBackground(int sessionId, string spriteName)
    {
        m_sessionBackgrounds [sessionId] = spriteName;
        
        Save();
    }
    
    public string GetSessionBackground(int sessionId)
    {
        string spriteName = m_sessionBackgrounds.ContainsKey(sessionId) ? m_sessionBackgrounds [sessionId] : "";
        return spriteName;
    }
    
    // Voyage Progress. Saved between app launches
    HashSet<int> m_completedSessions = new HashSet<int>();
    
    public bool HasCompletedSession(int sessionId)
    {
        return m_completedSessions.Contains(sessionId);
    }
    
    public int GetNumSessionsComplete(int moduleId)
    {
        int sessionsComplete = 0;
        
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programsessions WHERE programmodule_id=" + moduleId);
        
        foreach (DataRow session in dt.Rows)
        {
            if(HasCompletedSession(System.Convert.ToInt32(session["id"])))
            {
                ++sessionsComplete;
            }
        }
        
        return sessionsComplete;
    }

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
        m_completedSessions.Clear();
        m_sessionBackgrounds.Clear();
        
        Load();
    }
    
    void Load()
    {
        DataSaver ds = new DataSaver(System.String.Format("VoyageInfo_{0}", UserInfo.Instance.GetCurrentUserName()));
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);
        
        if (data.Length != 0)
        {
            int numCompletedSessions = br.ReadInt32();
            for(int i = 0; i < numCompletedSessions; ++i)
            {
                m_completedSessions.Add(br.ReadInt32());
            }
            
            int numSessionBackgrounds = br.ReadInt32();
            for(int i = 0; i < numSessionBackgrounds; ++i)
            {
                int sessionId = br.ReadInt32();
                string backgroundName = br.ReadString();
                m_sessionBackgrounds[sessionId] = backgroundName;
            }
        }
        
        br.Close();
        data.Close();
    }
    
    void Save(string user = null)
    {
        if (System.String.IsNullOrEmpty(user))
        {
            user = UserInfo.Instance.GetCurrentUserName();
        }
        
        DataSaver ds = new DataSaver(System.String.Format("VoyageInfo_{0}", user));
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);
        
        bw.Write(m_completedSessions.Count);
        foreach (int session in m_completedSessions)
        {
            bw.Write(session);
        }
        
        bw.Write(m_sessionBackgrounds.Count);
        foreach (KeyValuePair<int, string> kvp in m_sessionBackgrounds)
        {
            bw.Write(kvp.Key);
            bw.Write(kvp.Value);
        }
        
        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }
}
