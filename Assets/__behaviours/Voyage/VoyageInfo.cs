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
                if(tracker.GetModule() == 0)
                {
                    tracker.RemoveFirstSectionSession();
                }
            }

            Save();
        }
    }
#endif

    static int m_sectionsInSession = 4;
    static int m_sessionsInModule = 16;
    public static int sessionsInModule
    {
        get
        {
            return m_sessionsInModule;
        }
    }
  
    // Bookmarks save the module, session and section being played. They are forgotten between app launches
    class Bookmark
    {
        int m_module;
        public int GetModule()
        {
            return m_module;
        }

        int m_sessionNum;
        public int GetSessionNum()
        {
            return m_sessionNum;
        }

        int m_sectionId;
        public int GetSectionId()
        {
            return m_sectionId;
        }

        public Bookmark (int module, int sessionNum, int sectionId)
        {
            m_module = module;
            m_sessionNum = sessionNum;
            m_sectionId = sectionId;
        }
    }

    Bookmark m_bookmark = null;
    
    public void CreateBookmark(int module, int sessionNum, int sectionId)
    {
        m_bookmark = new Bookmark(module, sessionNum, sectionId);
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
    
    public int currentModule
    {
        get
        {
            return m_bookmark.GetModule();
        }
    }
    
    public int currentSessionNum
    {
        get
        {
            return m_bookmark.GetSessionNum();
        }
    }

    // ProgressTrackers save player progress. They are saved between app launches. 
    class ProgressTracker
    {
        int m_module;
        public int GetModule()
        {
            return m_module;
        }

        Dictionary<int, int> m_sectionSessions = new Dictionary<int, int>();

        public int GetNumSectionsComplete(int sessionNum)
        {
            int sectionsComplete = 0;

            foreach (KeyValuePair<int, int> kvp in m_sectionSessions)
            {
                if(kvp.Value == sessionNum)
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
                if(GetNumSectionsComplete(kvp.Value) >= m_sectionsInSession)
                {
                    completedSessions.Add(kvp.Value);
                }
            }

            return completedSessions.Count;
        }

        public void AddSectionComplete(int sectionId, int sessionNum)
        {
            m_sectionSessions[sectionId] = sessionNum;
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
            Debug.Log("Logging sectionSessions for " + m_module);
            foreach (KeyValuePair<int, int> kvp in m_sectionSessions)
            {
                Debug.Log(kvp.Value + " - " + kvp.Key);
            }
        }

        public ProgressTracker (int module)
        {
            m_module = module;
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

    public void AddSessionBackground(int sessionNum, string spriteName)
    {
        m_sessionBackgrounds [sessionNum] = spriteName;

        Save();
    }

    public string GetSessionBackground(int sessionNum)
    {
        string spriteName = m_sessionBackgrounds.ContainsKey(sessionNum) ? m_sessionBackgrounds [sessionNum] : "Incomplete";
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
    public bool HasCompletedSession(int sessionNum)
    {
        //Debug.Log("Checking completion for " + sessionNum);

        bool hasCompleted = false;

        foreach (ProgressTracker tracker in m_trackers)
        {
            //Debug.Log(tracker.GetModule() + " - " + tracker.GetNumSectionsComplete(sessionNum));
            //tracker.LogSectionSessions();
            if(tracker.GetNumSectionsComplete(sessionNum) >= m_sectionsInSession)
            {
                hasCompleted = true;
                break;
            }
        }

        return hasCompleted;
    }

    public bool NearlyCompletedSession(int sessionNum)
    {
        bool nearlyCompleted = false;
        
        foreach (ProgressTracker tracker in m_trackers)
        {
            if(tracker.GetNumSectionsComplete(sessionNum) == m_sectionsInSession - 1)
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

        ProgressTracker tracker = m_trackers.Find(x => x.GetModule() == module);
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
            ProgressTracker tracker = m_trackers.Find(x => x.GetModule() == m_bookmark.GetModule());
            if(tracker == null)
            {
                Debug.Log("Creating new tracker");
                tracker = new ProgressTracker(m_bookmark.GetModule());
                m_trackers.Add(tracker);
            }
            
            Debug.Log(System.String.Format("Adding to tracker {0} - {1}", m_bookmark.GetSectionId(), m_bookmark.GetSessionNum()));
            
            tracker.AddSectionComplete(m_bookmark.GetSectionId(), m_bookmark.GetSessionNum());
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
        Debug.Log("Logging Tracker " + tracker.GetModule());
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
            bw.Write(tracker.GetModule());

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
