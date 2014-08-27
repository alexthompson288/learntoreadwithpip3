using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class BuyInfo : Singleton<BuyInfo> 
{
    #if UNITY_EDITOR
    [SerializeField]
    private bool m_unlockInEditor;
    [SerializeField]
    private bool m_resetPurchases;
    #endif

    //[SerializeField]
    //private string[] m_mapProductIdentifiers;
    
    [SerializeField]
    private int[] m_defaultUnlockedBooks;
    [SerializeField]
    private int[] m_defaultUnlockedMaps;
    [SerializeField]
    private string[] m_defaultUnlockedGames;
    [SerializeField]
    private string[] m_defaultUnlockedPipisodes;
    
    HashSet<int> m_boughtBooks = new HashSet<int>();
    HashSet<int> m_boughtMaps = new HashSet<int>();
    HashSet<int> m_boughtPipisodes = new HashSet<int>();
    bool m_boughtGames = false;

    // TODO: You need to record whether they have bought everything, all maps, all books versus have bought each item individually.
    //       If they press "Buy All Maps" and you add a new map then the new map should be unlocked
    //       however, this is not the case if they have bought each map individually

    void Awake()
    {
#if UNITY_EDITOR
        if(m_resetPurchases)
        {
            Save ();
        }
#endif
        
        foreach(int book in m_defaultUnlockedBooks)
        {
            m_boughtBooks.Add(book);
        }
        
        foreach(int map in m_defaultUnlockedMaps)
        {
            m_boughtMaps.Add(map);
        }
        
        Load();
    }

    // TODO: Integrate IsBought methods into single method 

    public bool IsPipisodeBought(int pipisodeId)
    {
        #if UNITY_EDITOR
        if(m_unlockInEditor)
        {
            return true;
        }
        #endif
        
        return m_boughtPipisodes.Contains(pipisodeId) || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
    }

    public bool AreAllPipisodesBought()
    {
        #if UNITY_EDITOR
        if(m_unlockInEditor)
        {
            return true;
        }
        #endif
        
        if(((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked)
        {
            return true;
        }
        else
        {
            bool allPipisodesBought = true;
            
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from pipisodes");
            
            foreach(DataRow pipisode in dt.Rows)
            {
                if(pipisode["publishable"] != null && pipisode["publishable"].ToString() == "t" && !m_boughtPipisodes.Contains(Convert.ToInt32(pipisode["id"])))
                {
                    allPipisodesBought = false;
                    break;
                }
            }
            
            ////////D.Log("allBooksBought: " + allBooksBought);
            
            return allPipisodesBought;
        }
    }

    public void SetPipisodePurchased(int pipisodeId)
    {
        m_boughtPipisodes.Add(pipisodeId);
        Save();
    }

    public void SetAllPipisodesPurchased()
    {
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from pipisodes");

        //////D.Log(String.Format("Unlocking all {0} pipisodes", dt.Rows.Count));

        foreach(DataRow pipisode in dt.Rows)
        {
            if(pipisode["publishable"] != null && pipisode["publishable"].ToString() == "t")
            {
                m_boughtPipisodes.Add(Convert.ToInt32(pipisode["id"]));
                break;
            }
        }
    }
    
    public bool IsBookBought(int bookId)
    {
#if UNITY_EDITOR
        if(m_unlockInEditor)
        {
            return true;
        }
#endif
        
        return m_boughtBooks.Contains(bookId) || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
    }
    
    public bool AreAllBooksBought()
    {
#if UNITY_EDITOR
        if(m_unlockInEditor)
        {
            return true;
        }
#endif
        
        if(((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked)
        {
            return true;
        }
        else
        {
            bool allBooksBought = true;
            
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories");
            
            foreach(DataRow story in dt.Rows)
            {
                if(story["publishable"] != null && story["publishable"].ToString() == "t" && !m_boughtBooks.Contains(Convert.ToInt32(story["id"])))
                {
                    allBooksBought = false;
                    break;
                }
            }
            
            ////////D.Log("allBooksBought: " + allBooksBought);
            
            return allBooksBought;
        }
    }
    
    public void SetBookPurchased(int bookId)
    {
#if UNITY_IPHONE
        Dictionary<string, string> ep = new Dictionary<string, string>();
        ep.Add("BookID: ", bookId.ToString());
        //FlurryBinding.logEventWithParameters("BookPurchased", ep, false);
#endif
        
        m_boughtBooks.Add(bookId);
        Save();
    }
    
    public void SetAllBooksPurchased()
    {
#if UNITY_IPHONE
        //FlurryBinding.logEvent("AllBooksPurchased", false);
#endif
        
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories");
        if(dt.Rows.Count > 0)
        {
            //////D.Log("Unlocking all stories. Count " + dt.Rows.Count);
            foreach(DataRow story in dt.Rows)
            {
                if(story["publishable"] != null && story["publishable"].ToString() == "t")
                {
                    //////D.Log("Unlocking " + story["id"].ToString() + " - " + story["title"].ToString());
                    m_boughtBooks.Add(Convert.ToInt32(story["id"]));
                }
            }
        }
        Save ();
    }
    
    public bool IsMapBought(int mapId)
    {
#if UNITY_EDITOR
        if(m_unlockInEditor)
        {
            return true;
        }
#endif
        
        //////D.Log(String.Format("IsMapBought({0}) - {1}", mapId, (m_boughtMaps.Contains(mapId) || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked)));
        
        return m_boughtMaps.Contains(mapId) || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
    }
    
    public bool AreAllMapsBought()
    {
#if UNITY_EDITOR
        if(m_unlockInEditor)
        {
            return true;
        }
#endif
        
        if(((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked)
        {
            return true;
        }
        else
        {
            bool allMapsBought = true;
            
            for(int i = 0; i < BuyManager.Instance.numMaps; ++i)
            {
                if(!m_boughtMaps.Contains(i))
                {
                    allMapsBought = false;
                    break;
                }
            }
            
            
            ////////D.Log("ALL MAPS BOUGHT: " + allMapsBought);
            
            return allMapsBought;
        }
    }
    
    public void SetMapPurchased(int mapId)
    {
#if UNITY_IPHONE
        Dictionary<string, string> ep = new Dictionary<string, string>();
        ep.Add("MapID: ", mapId.ToString());
        //FlurryBinding.logEventWithParameters("MapPurchased", ep, false);
#endif
        
        //////D.Log(String.Format("SetMapPurchased({0})", mapId));
        m_boughtMaps.Add(mapId);
        Save ();
    }
    
    public void SetAllMapsPurchased()
    {
#if UNITY_IPHONE
        //FlurryBinding.logEvent("AllMapsPurchased", false);
#endif
        
        for(int i = 0; i < BuyManager.Instance.numMaps; ++i)
        {
            m_boughtMaps.Add(i);
        }
        Save ();
    }
    
    public bool IsGameBought(string gameSceneName)
    {
#if UNITY_EDITOR
        if(m_unlockInEditor)
        {
            return true;
        }
#endif
        
        return m_boughtGames || Array.IndexOf(m_defaultUnlockedGames, gameSceneName) != -1 || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
    }
    
    public bool AreAllGamesBought()
    {
#if UNITY_EDITOR
        if(m_unlockInEditor)
        {
            return true;
        }
#endif
        
        return m_boughtGames || ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked;
    }
    
    public void SetAllGamesPurchased()
    {
#if UNITY_IPHONE
        //FlurryBinding.logEvent("AllGamesPurchased", false);
#endif
        
        m_boughtGames = true;
        Save ();
    }
    
    public bool IsEverythingBought()
    {
        ////////D.Log("BuyManager.IsEverythingBought()");
        if(((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_isEverythingUnlocked)
        {
            ////////D.Log("Unlocked in settings");
            return true;
        }
        else
        {
            ////////D.Log("Unlocked from purchases: " + (AreAllBooksBought() && AreAllMapsBought() && AreAllGamesBought()));
            return AreAllBooksBought() && AreAllMapsBought() && AreAllGamesBought() && AreAllPipisodesBought();
        }
    }

    void Load()
    {
        DataSaver ds = new DataSaver("BuyInfo");
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);
        
        if (data.Length != 0)
        {
            int numBooks = br.ReadInt32();
            for (int i = 0; i < numBooks; ++i)
            {
                int bookId = br.ReadInt32();
                m_boughtBooks.Add(bookId);
            }
            
            int numMaps = br.ReadInt32();
            for(int i = 0; i < numMaps; ++i)
            {
                int mapId = br.ReadInt32();
                m_boughtMaps.Add(mapId);
            }
            
            m_boughtGames = br.ReadBoolean();

            int numPipisodes = br.ReadInt32();
            for(int i = 0; i < numPipisodes; ++i)
            {
                int pipisodeId = br.ReadInt32();
                m_boughtPipisodes.Add(pipisodeId);
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
        
        bw.Write(m_boughtBooks.Count);
        foreach (int i in m_boughtBooks)
        {
            bw.Write(i);
        }
        
        bw.Write(m_boughtMaps.Count);
        foreach(int i in m_boughtMaps)
        {
            bw.Write(i);
        }
        
        bw.Write(m_boughtGames);
        
        bw.Write(m_boughtPipisodes.Count);
        foreach (int i in m_boughtPipisodes)
        {
            bw.Write(i);
        }
        
        
        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }
}
