using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;

public class GameDataBridge : Singleton<GameDataBridge>
{
    [SerializeField]
    private string m_webStoreURL = @"http://ltrwpdata01.s3-external-3.amazonaws.com/public/";
    [SerializeField]
    private string m_requiredAssetBundlesPackInfo = @"AssetVersions.txt";
    [SerializeField]
    private int m_localDatabaseVersion = 0;
    [SerializeField]
    private TextAsset m_localDatabase;
    
    public static IEnumerator WaitForDatabase()
    {
        while (GameDataBridge.Instance == null)
        {
            ////D.Log("Singleton not ready");
            yield return null;
        }
        while (!GameDataBridge.Instance.IsReady())
        {
            //////D.Log("DB not ready");
            yield return null;
        }
    }
    
    public string GetOnlineBaseURL()
    {
        return m_webStoreURL;
    }
    
    void Awake()
    {
        //#if UNITY_EDITOR
        ////FlurryBinding.startSession("FV6X7ZZW2B2YVY6BY9RR");
        #if UNITY_IPHONE
        
        ////D.Log("DEVELOPMENT BUILD: " + Debug.isDebugBuild);
        
        string apiKey = Debug.isDebugBuild ?  "FV6X7ZZW2B2YVY6BY9RR" : "6Z5K6YT4JSC6KYMD77XQ";
        //FlurryBinding.startSession(apiKey);
        
        ////FlurryBinding.startSession("6Z5K6YT4JSC6KYMD77XQ");
        //#elif UNITY_ANDROID
        ////FlurryBinding.startSession("8QN3YHQ67VWSRRG53WKX");
        #endif
    }
    
    private SqliteDatabase m_cmsDb;
    bool m_isReady;
    // Use this for initialization
    IEnumerator Start () 
    {
        bool forceLocal = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_alwaysUseOfflineDatabase;
        bool useLocal = true;
        int dbVersion = 0;
        if ( !forceLocal )
        {
            SavedAssetBundles.Instance.StartInitialise(m_webStoreURL + m_requiredAssetBundlesPackInfo);
            while (!SavedAssetBundles.Instance.IsInitialised())
            {
                yield return null;
            }
            SavedAssetBundles.AssetVersionResult avr = SavedAssetBundles.Instance.GetOnlineAssetVersions();
            
            if (avr.m_assetList != null)
            {
                foreach (SavedAssetBundles.DownloadedAsset da in avr.m_assetList)
                {
                    ////D.Log(da.m_name + " " + da.m_version);
                    if (da.m_name == "local-store.bytes")
                    {
                        //if the version is set as 0 on the online version list, use the built in DB
                        if (da.m_version != 0)
                        {
                            dbVersion = da.m_version;
                            useLocal = false;
                        }
                    }
                }
            }
        }
        
        string debugLogPath = Application.persistentDataPath;
        
        // store DB separately for each game
        string dbName = "local-store.bytes";
        string cmsDbPath = Path.Combine(UnityEngine.Application.persistentDataPath,  
                                        SavedAssetBundles.Instance.GetSavedName(dbName));
        
        //string cmsDbPath = "jar:file://" + Application.dataPath + "!/assets/" 
        //+ SavedAssetBundles.Instance.GetSavedName(dbName);
        
        ////D.Log ("Database name for this game: " + dbName + " filename: " + cmsDbPath);
        m_cmsDb = new SqliteDatabase();
        
#if UNITY_EDITOR || UNITY_IPHONE || UNITY_STANDALONE
        int alwaysZero = 1;
#else
        int alwaysZero = 0;
#endif
        
        if (forceLocal || useLocal || alwaysZero == 0)
        {
            if ((!SavedAssetBundles.Instance.HasAny(dbName)) ||
                (m_localDatabaseVersion > SavedAssetBundles.Instance.GetVersion(dbName)))
            {
                ////D.Log("Using built-in DB, saving to " + cmsDbPath);
                
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE
                ////D.Log(cmsDbPath);
                System.IO.File.WriteAllBytes(cmsDbPath, m_localDatabase.bytes);
#endif
                /*
                //#else
                ////D.Log(1);
                string streamingAssetsPath = "jar:file://" + Application.dataPath + "!/assets/" + "local-store.bytes";
                ////D.Log(streamingAssetsPath);
                WWW loadDB = new WWW(streamingAssetsPath);
                ////D.Log(2);
                
                float counter = 0;
                bool foundDB = true;
                ////D.Log(3);
                while(!loadDB.isDone) 
                {
                    ////D.Log(4);
                    counter += Time.deltaTime;
                    ////D.Log("counter: " + counter);
                    //float progess = loadDB.progress;
                    //////D.Log(progress);
                    
                    if(counter > 120)
                    {
                        ////D.LogError("Could not find database in StreamingAssets");
                        foundDB = false;
                        break;
                    }
                }
                ////D.Log(5);
                
                ////D.Log("loadDB.bytes: " + loadDB.bytes);
                
                if(foundDB)
                {
                    ////D.Log(6 + "if");
                    ////D.Log("Writing db to persistent data path");
                    System.IO.File.WriteAllBytes(cmsDbPath, loadDB.bytes);
                    ////D.Log(7 + "if");
                    SavedAssetBundles.Instance.AddAssetVersion(dbName, m_localDatabaseVersion);
                    ////D.Log(8 + "if");
                }
                else
                {
                    ////D.Log(6 + "else");
                    ////D.Log("Not writing db because unable to find");
                }
                #endif
                */
            }
            else
            {
                ////D.Log("Using existing from " + cmsDbPath);
            }
        }
        else
        {
            if (!SavedAssetBundles.Instance.HasVersion(dbName, dbVersion))
            {
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE
                ////D.Log("Update to database found!");
                WWW www = new WWW(m_webStoreURL + dbName);
                while(!www.isDone)
                {
                    DownloadProgressInformation.Instance.SetDownloading("database", www.progress);
                    yield return null;
                }
                DownloadProgressInformation.Instance.StopDownloading("database");
                // no download error - save
                if (www.error == null)
                {
                    System.IO.File.WriteAllBytes(cmsDbPath, www.bytes);
                    SavedAssetBundles.Instance.AddAssetVersion(dbName, dbVersion);
                }
                else
                {
                    
                    // download error - if we don't have one at all, use the local DB
                    if (!SavedAssetBundles.Instance.HasAny(dbName))
                    {
                        System.IO.File.WriteAllBytes(cmsDbPath, m_localDatabase.bytes);
                    }
                }

                www.Dispose();
#endif

            }
            else
            {
                ////D.Log("Already on latest database!");
            }
        }
        
#if UNITY_IPHONE
        iPhone.SetNoBackupFlag(cmsDbPath);
#endif
        
        m_cmsDb.Open(cmsDbPath);
        
        if (!forceLocal )
        {
            SavedAssetBundles.Instance.DownloadAlwaysPresentAssets(m_webStoreURL);
            while(!SavedAssetBundles.Instance.AllAssetsDownloaded())
            {
                yield return null;
            }
        }
        
        m_isReady = true;
        ////D.Log("Database is ready");
        
        yield break;
    }
    
    public bool IsReady()
    {
        return m_isReady;
    }
    
    public SqliteDatabase GetDatabase()
    {
        return m_cmsDb;
    }
    
    void OnDestroy()
    {
        if(m_cmsDb != null)
        {
            m_cmsDb.Close();
        }
    }
}