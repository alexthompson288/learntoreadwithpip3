﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class BuyInfo : Singleton<BuyInfo> 
{
    #if UNITY_EDITOR
    [SerializeField]
    private bool m_overwrite;
    #endif

    // TODO: You need to record whether they have bought package versus have bought each item individually.
    //       If they buy a packgage and you add a new module/games then the new content should be unlocked
    //       however, this is not the case if they purchased content individually

    HashSet<int> m_purchasedModules = new HashSet<int>();
    HashSet<int> m_purchasedGames = new HashSet<int>();

    void Awake()
    {
#if UNITY_EDITOR
        if(m_overwrite)
        {
            Save ();
        }
#endif
        Load();
    }

    ////////////////////////////////////////////////////////////////
    // Modules
    public bool IsModulePurchased(int moduleId)
    {
        return m_purchasedModules.Contains(moduleId);
    }

    public void SetModulePurchased(int moduleId)
    {
        m_purchasedModules.Add(moduleId);
        Save();
    }

    public void SetModulesPurchased(int[] moduleIds)
    {
        foreach (int moduleId in moduleIds)
        {
            m_purchasedModules.Add(moduleId);
        }
        Save();
    }

    ////////////////////////////////////////////////////////////////
    // Games
    public bool IsGamePurchased(int gameId)
    {
        return m_purchasedGames.Contains(gameId);
    }

    public void SetGamePurchased(int gameId)
    {
        m_purchasedGames.Add(gameId);
        Save();
    }

    public void SetGamesPurchased(int[] gameIds)
    {
        foreach (int gameId in gameIds)
        {
            m_purchasedGames.Add(gameId);
        }
        Save();
    }

    ////////////////////////////////////////////////////////////////
    // Load/Save
    void Load()
    {
        DataSaver ds = new DataSaver("BuyInfo");
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);
        
        if (data.Length != 0)
        {
            int numModulesPurchased = br.ReadInt32();
            for (int i = 0; i < numModulesPurchased; ++i)
            {
                m_purchasedModules.Add(br.ReadInt32());
            }

            int numGamesPurchased = br.ReadInt32();
            for(int i = 0; i < numGamesPurchased; ++i)
            {
                m_purchasedGames.Add(br.ReadInt32());
            }
        }
        
        br.Close();
        data.Close();
    }
    
    void Save()
    {
        DataSaver ds = new DataSaver("BuyInfo");
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);
        
        bw.Write(m_purchasedModules.Count);
        foreach (int i in m_purchasedModules)
        {
            bw.Write(i);
        }

        bw.Write(m_purchasedGames.Count);
        foreach (int i in m_purchasedGames)
        {
            bw.Write(i);
        }
        
        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }
}