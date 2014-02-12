using UnityEngine;
using System.Collections;
using Wingrove;
using System.Collections.Generic;
using System.IO;
using System;

public class SavedAssetBundles : Singleton<SavedAssetBundles> 
{

    public class DownloadedAsset
    {
        public int m_version;
        public string m_name;
		public int m_alwaysPresent;
    }

    public class AssetVersionResult
    {
        //public List<DownloadedAsset> m_assetList;
		public List<DownloadedAsset> m_assetList = new List<DownloadedAsset>();
    }
	
    List<DownloadedAsset> m_assetList = new List<DownloadedAsset>();
	AssetVersionResult m_onlineAssetVersions = new AssetVersionResult();
	bool m_initialised = false;
	bool m_allAssetsDownloaded = false;

	// Use this for initialization
	void Start () 
    {
		//Load ();	
	}
	
	public string GetSavedName(string name)
	{
		return (((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).GetConvertedName()+"_"+name).Replace("{PLATFORM}/","");
	}
	
	void Load()
	{
        DataSaver dataSaver = new DataSaver("SavedAssetBundles");

        MemoryStream ms = dataSaver.Load();
        if (ms.Length != 0 )
        {
            BinaryReader br = new BinaryReader(ms);
            
            int numEntries = br.ReadInt32();

            for (int index = 0; index < numEntries; ++index)
            {
                DownloadedAsset da = new DownloadedAsset();
                da.m_version = br.ReadInt32();
                da.m_name = br.ReadString();

                if (AssetBundleLoader.Instance.FileExists(GetSavedName(da.m_name)))
                {
                    Debug.Log("Scan found: " + da.m_name);
                    m_assetList.Add(da);
                }
                else
                {
                    Debug.Log("Scan reports " + da.m_name + " as missing?");
                }
            }

            br.Close();
        }
        ms.Close();		
	}

    public bool HasVersion(string name, int version)
    {
        foreach (DownloadedAsset da in m_assetList)
        {
            if (da.m_name == name)
            {
                if (da.m_version == version)
                {                    
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasAny(string name)
    {
        foreach (DownloadedAsset da in m_assetList)
        {
            if (da.m_name == name)
            {
                return true;
            }
        }
        return false;
    }
	
    public int GetVersion(string name)
    {
        foreach (DownloadedAsset da in m_assetList)
        {
            if (da.m_name == name)
            {
                return da.m_version;
            }
        }
        return 0;
    }

    void Save()
    {
        DataSaver dataSaver = new DataSaver("SavedAssetBundles");

        MemoryStream ms = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write(m_assetList.Count);
        foreach (DownloadedAsset da in m_assetList)
        {
            bw.Write(da.m_version);
            bw.Write(da.m_name);
        }
        bw.Close();        
        dataSaver.Save(ms);
        ms.Close();
    }

    public void AddAssetVersion(string name, int version)
    {
        Debug.Log("Adding " + name + " at version " + version); 
		bool foundExistingAsset = false;
        foreach (DownloadedAsset da in m_assetList)
        {
            if (da.m_name == name)
            {
				foundExistingAsset = true;
                da.m_version = version;
            }
        }
		if (!foundExistingAsset)
		{
	        DownloadedAsset newDa = new DownloadedAsset();
	        newDa.m_name = name;
	        newDa.m_version = version;
	        m_assetList.Add(newDa);
		}
        Save();
    }
	
	public AssetVersionResult GetOnlineAssetVersions()
	{
		return m_onlineAssetVersions;
	}
	
	public bool IsInitialised()
	{
		return m_initialised;
	}
	
	public void StartInitialise(string assetUrl)
	{		
		StartCoroutine(Initialise(assetUrl));
	}
	
	public bool AllAssetsDownloaded()
	{
		return m_allAssetsDownloaded;
	}
	
	public void DownloadAlwaysPresentAssets(string onlineBaseURL)
	{
		StartCoroutine(DownloadAlwaysPresentAssetsCo(onlineBaseURL));
	}
	
	IEnumerator DownloadAlwaysPresentAssetsCo(string onlineBaseURL)
	{
		foreach(DownloadedAsset da in m_onlineAssetVersions.m_assetList)
		{
			if ( da.m_alwaysPresent != 0 )
			{
                if ((!HasAny(da.m_name))
                    || (!HasVersion(da.m_name, da.m_version)))
                {
                    Debug.Log("Need new version of " + da.m_name);
                    yield return StartCoroutine(AssetBundleLoader.Instance.DownloadWebAssetBundle(
                        Path.Combine(onlineBaseURL, da.m_name), GetSavedName(da.m_name), da.m_name, da.m_version));
                }
                else
                {
                    // load it
                    Debug.Log("Already have " + da.m_name);
                    yield return StartCoroutine(
                        AssetBundleLoader.Instance.LoadPersistentDataAssetBundle(GetSavedName(da.m_name))
                    );
                }

			}
		}
        m_allAssetsDownloaded = true;
		yield break;
	}


    IEnumerator Initialise(string assetListFileURL)
    {
		DownloadHelper.TextDownload downloadInfo = new DownloadHelper.TextDownload();
		yield return StartCoroutine(DownloadHelper.DownloadTextFile(assetListFileURL, downloadInfo));
		
		
        if (downloadInfo.m_success)
        {
			m_onlineAssetVersions = new AssetVersionResult();
            m_onlineAssetVersions.m_assetList = new List<DownloadedAsset>();

            string[] fields = downloadInfo.m_textOut.Split('\n', ',');
            for(int index = 0; index < fields.Length; index+=3)
            {
                DownloadedAsset da = new DownloadedAsset();
                da.m_name = fields[index];
                da.m_version = Convert.ToInt32(fields[index+1]);
				da.m_alwaysPresent = Convert.ToInt32 (fields[index+2]);
                m_onlineAssetVersions.m_assetList.Add(da);
            }
        }
		m_initialised = true;
    }
}
