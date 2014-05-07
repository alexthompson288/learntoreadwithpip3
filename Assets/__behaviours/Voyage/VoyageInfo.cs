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
        GameManager.Instance.OnComplete += OnGameComplete;
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

    // Session Backgrounds. Saved between app launches
    Dictionary<int, string> m_sessionBackgrounds = new Dictionary<int, string>();

    public void AddSessionBackground(int sessionId, string spriteName)
    {
        m_sessionBackgrounds [sessionId] = spriteName;

        Save();
    }

    public string GetSessionBackground(int sessionId)
    {
        string spriteName = m_sessionBackgrounds.ContainsKey(sessionId) ? m_sessionBackgrounds [sessionId] : "Incomplete";
        return spriteName;
    }

    // Voyage Progress. Saved between app launches
    HashSet<int> m_completedSections = new HashSet<int>();

    public bool HasCompletedSection(int sectionId)
    {
        return m_completedSections.Contains(sectionId);
    }

    public bool HasCompletedSession(int sessionId)
    {
        bool hasCompleted = true;

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from sections WHERE programsession_id=" + sessionId);
        foreach (DataRow section in dt.Rows)
        {
            if(!m_completedSections.Contains(System.Convert.ToInt32(section["id"])) && DataHelpers.FindGameForSection(section) != null)
            {
                hasCompleted = false;
                break;
            }
        }

        return hasCompleted;
    }

    public int GetNumRemainingSections(int sessionId)
    {
        int remainingSections = 0;

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from sections WHERE programsession_id=" + sessionId);
        foreach (DataRow section in dt.Rows)
        {
            if(!m_completedSections.Contains(System.Convert.ToInt32(section["id"])) && DataHelpers.FindGameForSection(section) != null)
            {
                ++remainingSections;
            }
        }

        return remainingSections;
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

    void OnGameComplete()
    {
        Debug.Log("VoyageInfo.OnGameComplete");
        GameManager.Instance.OnComplete -= OnGameComplete;
        
        if (m_bookmark != null)
        {
            Debug.Log("Completed section: " + m_bookmark.GetSectionId());
            m_completedSections.Add(m_bookmark.GetSectionId());
        }
        
        Save();
    }
    
    void OnGameCancel()
    {
        Debug.Log("VoyageInfo.OnGameCancel");
        GameManager.Instance.OnComplete -= OnGameComplete;
        
        m_bookmark = null;
    }

    void Start()
    {
#if UNITY_EDITOR
        if(m_overwrite)
        {
            Save();
        }
#endif

        Load();

        GameManager.Instance.OnCancel += OnGameCancel;
    }

    void Load()
    {
        DataSaver ds = new DataSaver(System.String.Format("VoyageInfo_{0}", UserInfo.Instance.GetCurrentUser()));
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);
        
        if (data.Length != 0)
        {
            int numCompletedSections = br.ReadInt32();
            for(int i = 0; i < numCompletedSections; ++i)
            {
                m_completedSections.Add(br.ReadInt32());
            }

            int numSessionBackgrounds = br.ReadInt32();
            for(int i = 0; i < numSessionBackgrounds; ++i)
            {
                m_sessionBackgrounds.Add(br.ReadInt32(), br.ReadString());
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

        bw.Write(m_completedSections.Count);
        foreach (int section in m_completedSections)
        {
            bw.Write(section);
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
