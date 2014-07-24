using UnityEngine;
using System.Collections;
using Wingrove;

public class PrefabTestLoader : MonoBehaviour 
{
    [SerializeField]
    private Transform m_spawnParent;

    string url = "file:///Users/Bonobo/Desktop/capsule_prefab.unity3d";

	// Use this for initialization
	IEnumerator Start () 
    {
        //D.Log("Waiting for Instance");
        yield return StartCoroutine(AssetBundleLoaderTest.WaitForInstance());

        //D.Log("Waiting for download");
        yield return StartCoroutine(AssetBundleLoaderTest.Instance.LoadBundleCo<GameObject>(url, 1, "", ""));

        //D.Log("Getting obj");
        GameObject loadedGo = AssetBundleLoaderTest.Instance.GetObj() as GameObject;

        //D.Log(loadedGo);

        if (loadedGo != null)
        {
            SpawningHelpers.InstantiateUnderWithIdentityTransforms(loadedGo, m_spawnParent);
        }
	}
}
