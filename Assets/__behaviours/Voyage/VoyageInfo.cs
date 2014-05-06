using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class VoyageInfo : Singleton<VoyageInfo> 
{
#if UNITY_EDITOR
    [SerializeField]
    private bool m_overwrite;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            foreach(ProgressTracker tracker in m_trackers)
            {
                if(tracker.GetModuleId() == 0)
                {
                    tracker.RemoveFirstSectionSession();
                }
            }

            Save();
        }
    }
#endif

    static int m_sectionsPerSession = 4;
    static int m_sessionsPerModule = 16;
    public static int sessionsPerModule
    {
        get
        {
            return m_sessionsPerModule;
        }
    }
  
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

    // ProgressTrackers save player progress. They are saved between app launches. 
    class ProgressTracker
    {
        int m_moduleId;
        public int GetModuleId()
        {
            return m_moduleId;
        }

        Dictionary<int, int> m_sectionSessions = new Dictionary<int, int>();

        public int GetNumSectionsComplete(int sessionId)
        {
            int sectionsComplete = 0;

            foreach (KeyValuePair<int, int> kvp in m_sectionSessions)
            {
                if(kvp.Value == sessionId)
                {
                    ++sectionsComplete;
                }
            }

            return sectionsComplete;
        }

        public int GetNumSessionsComplete()
        {
            HashSet<int> completedSessions = new HashSet<int>();

            foreach (KeyValuePair<int, int> kvp in m_sectionSessions)
            {
                if(GetNumSectionsComplete(kvp.Value) >= m_sectionsPerSession)
                {
                    completedSessions.Add(kvp.Value);
                }
            }

            return completedSessions.Count;
        }

        public void AddSectionComplete(int sectionId, int sessionId)
        {
            m_sectionSessions[sectionId] = sessionId;
        }

        public bool HasCompletedSection(int sectionId)
        {
            return m_sectionSessions.ContainsKey(sectionId);
        }

        public Dictionary<int, int> GetSectionSessions()
        {
            return m_sectionSessions;
        }

        public void LogSectionSessions()
        {
            Debug.Log("Logging sectionSessions for " + m_moduleId);
            foreach (KeyValuePair<int, int> kvp in m_sectionSessions)
            {
                Debug.Log(kvp.Value + " - " + kvp.Key);
            }
        }

        public ProgressTracker (int moduleId)
        {
            m_moduleId = moduleId;
        }

#if UNITY_EDITOR
        public void RemoveFirstSectionSession()
        {
            int toRemove = -1;
            foreach (KeyValuePair<int, int> kvp in m_sectionSessions)
            {
                toRemove = kvp.Key;
                break;
            }

            Debug.Log("Removing: " + toRemove);
            m_sectionSessions.Remove(toRemove);
        }
#endif
    }

    List<ProgressTracker> m_trackers = new List<ProgressTracker>();

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

    public bool HasCompletedSection(int sectionId)
    {
        bool hasCompleted = false;

        foreach(ProgressTracker tracker in m_trackers)
        {
            if(tracker.HasCompletedSection(sectionId))
            {
                hasCompleted = true;
                break;
            }
        }

        return hasCompleted;
    }

    // TODO: Integrate HasCompletedSession and NearlyCompletedSession into single method
    public bool HasCompletedSession(int sessionId)
    {
        //Debug.Log("Checking completion for " + sessionId);

        bool hasCompleted = false;

        foreach (ProgressTracker tracker in m_trackers)
        {
            //Debug.Log(tracker.GetModule() + " - " + tracker.GetNumSectionsComplete(sessionId));
            //tracker.LogSectionSessions();
            if(tracker.GetNumSectionsComplete(sessionId) >= m_sectionsPerSession)
            {
                hasCompleted = true;
                break;
            }
        }

        return hasCompleted;
    }

    public bool NearlyCompletedSession(int sessionId)
    {
        bool nearlyCompleted = false;
        
        foreach (ProgressTracker tracker in m_trackers)
        {
            if(tracker.GetNumSectionsComplete(sessionId) == m_sectionsPerSession - 1)
            {
                nearlyCompleted = true;
                break;
            }
        }
        
        return nearlyCompleted;
    }

    public int GetNumSessionsComplete(int module)
    {
        int sessionsComplete = 0;

        ProgressTracker tracker = m_trackers.Find(x => x.GetModuleId() == module);
        if (tracker != null)
        {
            sessionsComplete = tracker.GetNumSessionsComplete();
        }

        return sessionsComplete;
    }

    void OnGameComplete()
    {
        Debug.Log("VoyageInfo.OnGameComplete");
        GameManager.Instance.OnComplete -= OnGameComplete;
        
        if (m_bookmark != null)
        {
            ProgressTracker tracker = m_trackers.Find(x => x.GetModuleId() == m_bookmark.GetModuleId());
            if(tracker == null)
            {
                Debug.Log("Creating new tracker");
                tracker = new ProgressTracker(m_bookmark.GetModuleId());
                m_trackers.Add(tracker);
            }
            
            Debug.Log(System.String.Format("Adding to tracker {0} - {1}", m_bookmark.GetSectionId(), m_bookmark.GetSessionId()));
            
            tracker.AddSectionComplete(m_bookmark.GetSectionId(), m_bookmark.GetSessionId());
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

    void LogTracker(ProgressTracker tracker)
    {
        Debug.Log("Logging Tracker " + tracker.GetModuleId());
        Dictionary<int, int> sectionSessions = tracker.GetSectionSessions();
        Debug.Log("sectionSessions.Count: " + sectionSessions.Count);
        foreach (KeyValuePair<int, int> kvp in sectionSessions)
        {
            Debug.Log(System.String.Format("{0} - {1}", kvp.Key, kvp.Value));
        }
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
                ProgressTracker tracker = new ProgressTracker(br.ReadInt32());

                int numSectionSessions = br.ReadInt32();
                for(int j = 0; j < numSectionSessions; ++j)
                {
                    tracker.AddSectionComplete(br.ReadInt32(), br.ReadInt32());
                }

                m_trackers.Add(tracker);
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

        bw.Write(m_trackers.Count);
        foreach (ProgressTracker tracker in m_trackers)
        {
            bw.Write(tracker.GetModuleId());

            Dictionary<int, int> sectionSessions = tracker.GetSectionSessions();
            bw.Write(sectionSessions.Count);
            foreach(KeyValuePair<int, int> kvp in sectionSessions)
            {
                Debug.Log(System.String.Format("Writing: {0} - {1}", kvp.Key, kvp.Value));
                bw.Write(kvp.Key);
                bw.Write(kvp.Value);
            }
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
