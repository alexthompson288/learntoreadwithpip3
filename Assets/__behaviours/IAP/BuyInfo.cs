using UnityEngine;
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

    HashSet<string> m_purchasedColors = new HashSet<string>();
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
    // Colors
    public bool IsColorPurchased(ColorInfo.PipColor pipColor)
    {
        return ContentLock.Instance.lockType == ContentLock.Lock.None || m_purchasedColors.Contains(pipColor.ToString()) || pipColor == ColorInfo.PipColor.Pink;
    }

    public void SetColorPurchased(string colorName)
    {
        m_purchasedColors.Add(colorName);
        Save();
    }

    public void SetColorsPurchased(string[] colorNames)
    {
        foreach (string colorName in colorNames)
        {
            m_purchasedColors.Add(colorName);
        }
        Save();

        D.Log("LOG ALL PURCHASED COLORS");
        foreach (string color in m_purchasedColors)
        {
            D.Log(color);
        }
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
            int numColorsPurchased = br.ReadInt32();
            for (int i = 0; i < numColorsPurchased; ++i)
            {
                m_purchasedColors.Add(br.ReadString());
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
        
        bw.Write(m_purchasedColors.Count);
        foreach (string s in m_purchasedColors)
        {
            bw.Write(s);
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
