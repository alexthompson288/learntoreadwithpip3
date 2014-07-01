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
  
    // Bookmarks save the module, session and section being played. They are forgotten between app launches
    class Bookmark
    {
        int m_moduleId;
        public int GetModuleId()
        {
            return m_moduleId;
        }

        int m_sessionId;
        public int GetSessionId()
        {
            return m_sessionId;
        }

        int m_sectionId;
        public int GetSectionId()
        {
            return m_sectionId;
        }

        public Bookmark (int moduleId, int sessionId, int sectionId)
        {
            m_moduleId = moduleId;
            m_sessionId = sessionId;
            m_sectionId = sectionId;
        }
    }

    Bookmark m_bookmark = null;
    
    public void CreateBookmark(int moduleId, int sessionId, int sectionId)
    {
        Debug.Log("CREATING BOOKMARK");
        Debug.Log("moduleId: " + moduleId);
        Debug.Log("sessionId: " + sessionId);
        Debug.Log("sectionId: " + sectionId);

        m_bookmark = new Bookmark(moduleId, sessionId, sectionId);
        GameManager.Instance.CompletedAll += OnCompleteGames;
    }
    
    public void DestroyBookmark()
    {
        m_bookmark = null;
    }
    
    public bool hasBookmark
    {
        get
        {
            return m_bookmark != null;
        }
    }
    
    public int currentModuleId
    {
        get
        {
            return m_bookmark.GetModuleId();
        }
    }
    
    public int currentSessionId
    {
        get
        {
            return m_bookmark.GetSessionId();
        }
    }

    public int currentSectionId
    {
        get
        {
            return m_bookmark.GetSectionId();
        }
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
        string spriteName = m_sessionBackgrounds.ContainsKey(sessionId) ? m_sessionBackgrounds [sessionId] : "incomplete";
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

    void OnCompleteGames()
    {
        Debug.Log("VoyageInfo.OnCompleteGames");
        GameManager.Instance.CompletedAll -= OnCompleteGames;
        
        if (m_bookmark != null)
        {
            Debug.Log("Completed session: " + m_bookmark.GetSessionId());
            m_completedSessions.Add(m_bookmark.GetSessionId());
        }
        
        Save();
    }
    
    void OnGameCancel()
    {
        Debug.Log("VoyageInfo.OnGameCancel");
        GameManager.Instance.CompletedAll -= OnCompleteGames;
        
        m_bookmark = null;
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

        GameManager.Instance.Cancelling += OnGameCancel;
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
        DataSaver ds = new DataSaver(System.String.Format("VoyageInfo_{0}", UserInfo.Instance.GetCurrentUser()));
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
            user = UserInfo.Instance.GetCurrentUser();
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
