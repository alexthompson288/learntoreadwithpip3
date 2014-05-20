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
        Debug.Log("Waiting for Instance");
        yield return StartCoroutine(AssetBundleLoaderTest.WaitForInstance());

        Debug.Log("Waiting for download");
        yield return StartCoroutine(AssetBundleLoaderTest.Instance.LoadBundleCo<GameObject>(url, 1, "", ""));

        Debug.Log("Getting obj");
        GameObject loadedGo = AssetBundleLoaderTest.Instance.GetObj() as GameObject;

        Debug.Log(loadedGo);

        if (loadedGo != null)
        {
            SpawningHelpers.InstantiateUnderWithIdentityTransforms(loadedGo, m_spawnParent);
        }
	}
}
