using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour 
{
    [SerializeField]
    private bool m_startOn = true;
    [SerializeField]
    private bool m_spawnScaleIsZero = true;
    [SerializeField]
    private GameObject m_prefab;
    [SerializeField]
    private Vector2 m_delayRange;

	// Use this for initialization
	void Start () 
    {
        if (m_startOn)
        {
            StartCoroutine(OnCo());
        }
	}
	
    public void On()
    {
        StartCoroutine(OnCo());
    }

    public void Off()
    {
        StopAllCoroutines();
    }

	IEnumerator OnCo()
    {
        Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_prefab, transform, m_spawnScaleIsZero);
        yield return new WaitForSeconds(Random.Range(m_delayRange.x, m_delayRange.y));
        StartCoroutine(OnCo());
    }


}
