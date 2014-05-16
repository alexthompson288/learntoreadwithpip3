using UnityEngine;
using System.Collections;
using LifeboatCommon;
using System.Collections.Generic;
using System.IO;

public class AssetBundleLoader : Singleton<AssetBundleLoader> {

    [SerializeField]
    private GameObject m_loadingHierarchy;    

    List<AssetBundle> m_loadedAssetBundles = new List<AssetBundle>();

    public static string GetStreamingAssetsPath()
    {
        string streamingPath = "/";
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                streamingPath = "/Android/";
                break;
            case RuntimePlatform.IPhonePlayer:
                streamingPath = "/iPhone/";
                break;
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                streamingPath = "/StandaloneOSXIntel/";
                break;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                streamingPath = "/StandaloneWindows/";
                break;
        }
        return Application.streamingAssetsPath + streamingPath;
    }

    public string GetPlatformName()
    {
        string platformName = "";
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                platformName = "Android";
                break;
            case RuntimePlatform.IPhonePlayer:
                platformName = "iPhone";
                break;
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                platformName = "StandaloneOSXIntel";
                break;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                platformName = "StandaloneWindows";
                break;
        }
        return platformName;
    }

    public bool FileExists(string filename)
    {
        string pathToFile = Path.Combine(UnityEngine.Application.persistentDataPath, filename);
        return File.Exists(pathToFile);
    }

    public IEnumerator LoadPersistentDataAssetBundle(string assetBundleName)
    {
        
        if (m_loadingHierarchy != null)
        {
            m_loadingHierarchy.SetActive(true);
        }
        string fullPath = Path.Combine(UnityEngine.Application.persistentDataPath, assetBundleName);
        Debug.Log("Loading asset bundle: " + fullPath);
        AssetBundle loadedAssetBundle = null;

        if (fullPath.Contains("://"))
        {
			// this might happen on android.
            using (WWW compressedLoader = new WWW(fullPath))
            {
                yield return compressedLoader;

                loadedAssetBundle = compressedLoader.assetBundle;
            }
        }
        else
        {
            loadedAssetBundle = AssetBundle.CreateFromFile(fullPath);
        }

        if (loadedAssetBundle != null)
        {
            m_loadedAssetBundles.Add(loadedAssetBundle);
        }

        if (m_loadingHierarchy != null)
        {
            m_loadingHierarchy.SetActive(false);
        }
        yield break;
    }

    public IEnumerator LoadStreamingDataAssetBundle(string assetBundleName)
    {
        if (m_loadingHierarchy != null)
        {
            m_loadingHierarchy.SetActive(true);
        }
        string fullPath = GetStreamingAssetsPath() + assetBundleName + ".unity3d";

        AssetBundle loadedAssetBundle = null;

        if (fullPath.Contains("://"))
        {
            using (WWW compressedLoader = new WWW(fullPath))
            {
                yield return compressedLoader;

                loadedAssetBundle = compressedLoader.assetBundle;
            }            
        }
        else
		{
            loadedAssetBundle = AssetBundle.CreateFromFile(fullPath);
		}

        if (loadedAssetBundle != null)
        {
            m_loadedAssetBundles.Add(loadedAssetBundle);
        }

        if (m_loadingHierarchy != null)
        {
            m_loadingHierarchy.SetActive(false);
        }
        yield break;
    }


    public IEnumerator DownloadWebAssetBundle(string assetBundleURL, string saveAsFileName, string saveAsRefName, int saveAsVersion)
    {
        if (m_loadingHierarchy != null)
        {
            m_loadingHierarchy.SetActive(true);
        }
        AssetBundle loadedAssetBundle = null;
        string urlFixed = assetBundleURL.Replace("{PLATFORM}", GetPlatformName());
		Debug.Log ("Downloading AssetBundle from " + urlFixed);
        using (WWW compressedLoader = new WWW(urlFixed))
        {
            while (!compressedLoader.isDone)
            {
                DownloadProgressInformation.Instance.SetDownloading("storydata", compressedLoader.progress);
                yield return null;
            }

            DownloadProgressInformation.Instance.StopDownloading("storydata");

            if (compressedLoader.error == null)
            {
                Debug.Log("Download successful " + urlFixed);
                loadedAssetBundle = compressedLoader.assetBundle;
                // cache for next time
                string outputPath = Path.Combine(UnityEngine.Application.persistentDataPath,
                                                    saveAsFileName);

#if UNITY_IPHONE || UNITY_ANDROID || UNITY_STANDALONE
                File.WriteAllBytes(outputPath, compressedLoader.bytes);
                Debug.Log("Saving to " + outputPath + " and logging as version " + saveAsVersion);
                SavedAssetBundles.Instance.AddAssetVersion(saveAsRefName, saveAsVersion);
#endif
            }
            else
            {
                Debug.Log(compressedLoader.error);
            }
        }

        if (loadedAssetBundle != null)
        {
            m_loadedAssetBundles.Add(loadedAssetBundle);
        }

        if (m_loadingHierarchy != null)
        {
            m_loadingHierarchy.SetActive(false);
        }
        yield break;
    }

    public void UnloadAssetBundle()
    {
		//TODO
    }

    public T FindAsset<T>(string assetName) where T : Object
    {
        foreach (AssetBundle ab in m_loadedAssetBundles)
        {
            if (ab.Contains(assetName))
            {
                T returnObject = (T)ab.Load(assetName, typeof(T));
                if (returnObject != null)
                {
                    return returnObject;
                }
            }
        }
        return null;
    }
}
