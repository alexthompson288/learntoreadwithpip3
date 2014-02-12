using UnityEngine;
using System.Collections;

public class HideTripleBlackboard : MonoBehaviour 
{
	[SerializeField]
	private TripleBlackboard[] m_blackboards;
	

	void OnClick () 
	{
		collider.enabled = false;
		for(int index = 0; index < m_blackboards.Length; ++index)
		{
			m_blackboards[index].Hide();
		}
	}
}
