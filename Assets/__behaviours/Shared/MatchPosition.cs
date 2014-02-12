using UnityEngine;
using System.Collections;

public class MatchPosition : MonoBehaviour 
{
	Transform m_transformToMatch;

	// Use this for initialization
	void Update () 
	{
		if(m_transformToMatch != null)
		{
			transform.position = m_transformToMatch.position;
		}
	}

	public Transform GetTransformToMatch()
	{
		return m_transformToMatch;
	}
	
	public void SetTransformToMatch(Transform transformToMatch)
	{
		m_transformToMatch = transformToMatch;
	}
}
