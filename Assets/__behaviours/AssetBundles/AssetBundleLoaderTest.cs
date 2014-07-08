using UnityEngine;
using System.Collections;

public class AssetBundleLoaderTest : Singleton<AssetBundleLoaderTest> 
{
    public Object Obj; // The object retrieved from the AssetBundle

    //public void LoadBundle<T> (string url, int version, string assetName, string assetPath) where T : Object
    //{
        //StartCoroutine(LoadBundleCo<T> (url, version, assetName, assetPath));
    //}

    public Object GetObj()
    {
        return Obj;
    }
    
    public IEnumerator LoadBundleCo<T> (string url, int version, string assetName, string assetPath) where T : Object 
    {
        Obj = null;

//#if UNITY_EDITOR
        //Obj = Resources.LoadAssetAtPath(assetPath, typeof(T));
        //if (Obj == null)
        //{
            //Debug.LogError ("Asset not found at path: " + assetPath);
        //}
        //yield break;
//#else
        /*
        WWW download;
        if ( Caching.enabled )  
        {       
            while (!Caching.ready)
            {
                yield return null;
            }

            download = WWW.LoadFromCacheOrDownload( url, version );
        }
        else 
        {
            download = new WWW (url);
        }
        */

        WWW download = new WWW (url);
        
        yield return download;
        if ( download.error != null ) 
        {
            Debug.LogError( download.error );
            download.Dispose();
            yield break;
        }
        
        AssetBundle assetBundle = download.assetBundle;
        download.Dispose();
        download = null;
        
        if (assetName == "" || assetName == null)
        {
            Obj = assetBundle.mainAsset;
        }
        else
        {
            Obj = assetBundle.Load(assetName, typeof(T));
        }
        
        assetBundle.Unload(false);
//#endif
    }

    /*
    public static IEnumerator WaitForInstance()
    {
        while (AssetBundleLoaderTest.Instance == null)
        {
            yield return null;
        }
    }
    */
}
