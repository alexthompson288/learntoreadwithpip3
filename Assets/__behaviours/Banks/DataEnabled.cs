using UnityEngine;
using System.Collections;

public class DataEnabled : MonoBehaviour 
{
    [SerializeField]
    private bool m_enableOnMatch = false;
    [SerializeField]
    private string[] m_dataMatches;

	void Start () 
    {
        int i = System.Array.IndexOf(m_dataMatches, GameManager.Instance.dataType);

        gameObject.SetActive((i == -1 && m_enableOnMatch) || (i != -1 && !m_enableOnMatch));
	}
}
