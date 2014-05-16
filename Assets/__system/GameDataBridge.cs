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
            Debug.Log("Singleton not ready");
            yield return null;
        }
        while (!GameDataBridge.Instance.IsReady())
        {
            //Debug.Log("DB not ready");
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
        //FlurryBinding.startSession("FV6X7ZZW2B2YVY6BY9RR");
        #if UNITY_IPHONE
        
        Debug.Log("DEVELOPMENT BUILD: " + Debug.isDebugBuild);
        Debug.Log("UP_TO_DATE_TEST_4");
        
        string apiKey = Debug.isDebugBuild ?  "FV6X7ZZW2B2YVY6BY9RR" : "6Z5K6YT4JSC6KYMD77XQ";
        FlurryBinding.startSession(apiKey);
        
        //FlurryBinding.startSession("6Z5K6YT4JSC6KYMD77XQ");
        //#elif UNITY_ANDROID
        //FlurryBinding.startSession("8QN3YHQ67VWSRRG53WKX");
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
                    Debug.Log(da.m_name + " " + da.m_version);
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
        
        Debug.Log ("Database name for this game: " + dbName + " filename: " + cmsDbPath);
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
                Debug.Log("Using built-in DB, saving to " + cmsDbPath);
                
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE
                Debug.Log(cmsDbPath);
                System.IO.File.WriteAllBytes(cmsDbPath, m_localDatabase.bytes);
#endif
                /*
                //#else
                Debug.Log(1);
                string streamingAssetsPath = "jar:file://" + Application.dataPath + "!/assets/" + "local-store.bytes";
                Debug.Log(streamingAssetsPath);
                WWW loadDB = new WWW(streamingAssetsPath);
                Debug.Log(2);
                
                float counter = 0;
                bool foundDB = true;
                Debug.Log(3);
                while(!loadDB.isDone) 
                {
                    Debug.Log(4);
                    counter += Time.deltaTime;
                    Debug.Log("counter: " + counter);
                    //float progess = loadDB.progress;
                    //Debug.Log(progress);
                    
                    if(counter > 120)
                    {
                        Debug.LogError("Could not find database in StreamingAssets");
                        foundDB = false;
                        break;
                    }
                }
                Debug.Log(5);
                
                Debug.Log("loadDB.bytes: " + loadDB.bytes);
                
                if(foundDB)
                {
                    Debug.Log(6 + "if");
                    Debug.Log("Writing db to persistent data path");
                    System.IO.File.WriteAllBytes(cmsDbPath, loadDB.bytes);
                    Debug.Log(7 + "if");
                    SavedAssetBundles.Instance.AddAssetVersion(dbName, m_localDatabaseVersion);
                    Debug.Log(8 + "if");
                }
                else
                {
                    Debug.Log(6 + "else");
                    Debug.Log("Not writing db because unable to find");
                }
                #endif
                */
            }
            else
            {
                Debug.Log("Using existing from " + cmsDbPath);
            }
        }
        else
        {
            if (!SavedAssetBundles.Instance.HasVersion(dbName, dbVersion))
            {
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE
                Debug.Log("Update to database found!");
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
#endif
                www.Dispose();
            }
            else
            {
                Debug.Log("Already on latest database!");
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
        Debug.Log("Database is ready");
        
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