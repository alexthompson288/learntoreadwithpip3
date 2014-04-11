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

    // Permanently saved
    List<int> m_completedSections = new List<int>();
    //List<int> m_completedSessions = new List<int>(); // TODO: Replace list with dictionary
    Dictionary<int, int> m_moduleProgress = new Dictionary<int, int>();

    // Forgotten when app is closed
    int m_currentSection = -1;


    GameLocation m_currentLocation = null;

    public void SetCurrentLocation(int module, int sessionNum, ColorInfo.PipColor color)
    {
        m_currentLocation = new GameLocation(module, sessionNum, color);
    }

    public void SetCurrentLocationNull()
    {
        m_currentLocation = null;
    }

    public bool hasCurrent
    {
        get
        {
            Debug.Log("hasCurrent: " + (m_currentLocation != null));
            return m_currentLocation != null;
        }
    }
   
    public int currentModule
    {
        get
        {
            return m_currentLocation.GetModule();
        }
    }

    public int currentSessionNum
    {
        get
        {
            return m_currentLocation.GetSessionNum();
        }
    }

    public ColorInfo.PipColor currentColor
    {
        get
        {
            return m_currentLocation.GetColor();
        }
    }

    class GameLocation
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

        ColorInfo.PipColor m_color;
        public ColorInfo.PipColor GetColor()
        {
            return m_color;
        }

        public GameLocation (int module, int sessionNum, ColorInfo.PipColor color)
        {
            m_module = module;
            m_sessionNum = sessionNum;
            m_color = color;
        }
    }

    public bool HasCompleted(int sectionId)
    {
        return m_completedSections.Contains(sectionId);
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

    void OnGameComplete()
    {
        Debug.Log("VoyageInfo.OnGameComplete: " + m_currentSection);
        GameManager.Instance.OnComplete -= OnGameComplete;
        m_currentSection = -1;

        m_completedSections.Add(m_currentSection);

        Save();
    }

    void OnGameCancel()
    {
        Debug.Log("VoyageInfo.OnGameCancel");
        GameManager.Instance.OnComplete -= OnGameComplete;
        m_currentSection = -1;

        m_currentLocation = null;
    }

    public void OnChooseSection(int sectionId)
    {
        GameManager.Instance.OnComplete += OnGameComplete;
        m_currentSection = sectionId;
    }

    void Load()
    {
        DataSaver ds = new DataSaver(System.String.Format("VoyageInfo_{0}", UserInfo.Instance.GetCurrentUser()));
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);
        
        if (data.Length != 0)
        {
            int numCompletedSections = br.ReadInt32();
            for (int i = 0; i < numCompletedSections; ++i)
            {
                int sectionId = br.ReadInt32();
                m_completedSections.Add(sectionId);
            }

            /*
            int numModules = br.ReadInt32();
            for(int i = 0; i < numModules; ++i)
            {
                int moduleIndex = br.ReadInt32();
                int numCompletedSessions = br.ReadInt32();
                m_moduleProgress.Add(moduleIndex, completedSessions);
            }
            */
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
        foreach (int i in m_completedSections)
        {
            bw.Write(i);
        }

        //bw.Write(m_moduleProgress.Count);
        //foreach(KeyValuePair<int, int> kvp in m_
        
        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }
}
