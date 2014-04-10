using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class VoyageInfo : Singleton<VoyageInfo> 
{
    List<int> m_completedSections = new List<int>();

    int m_currentSection = -1;

    public bool HasCompleted(int sectionId)
    {
        return m_completedSections.Contains(sectionId);
    }

    void Start()
    {
        GameManager.Instance.OnCancel += OnGameCancel;
    }

    void OnGameComplete()
    {
        m_completedSections.Add(m_currentSection);
        m_currentSection = -1;
    }

    void OnGameCancel()
    {
        GameManager.Instance.OnComplete -= OnGameComplete;
        m_currentSection = -1;
    }

    public void OnChooseSection(int sectionId)
    {
        GameManager.Instance.OnComplete += OnGameComplete;
        m_currentSection = sectionId;
    }

    void Load()
    {
        DataSaver ds = new DataSaver("VoyageInfo");
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
        }
        
        br.Close();
        data.Close();
    }
    
    void Save()
    {
        DataSaver ds = new DataSaver("VoyageInfo");
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);
        
        bw.Write(m_completedSections.Count);
        foreach (int i in m_completedSections)
        {
            bw.Write(i);
        }
        
        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }
}
