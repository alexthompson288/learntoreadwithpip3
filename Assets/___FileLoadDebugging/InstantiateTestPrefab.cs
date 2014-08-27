using UnityEngine;
using System.Collections;
using Wingrove;

public class InstantiateTestPrefab : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_testPrefab;

	// Use this for initialization
	void Start () 
    {
	    if (m_testPrefab != null)
        {
            GameObject newGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_testPrefab, transform);
            newGo.transform.position = new Vector3(3, 3, 0);
        } 
        else
        {
            //////D.Log("MISSING TEST PREFAB");
        }
	}
}
