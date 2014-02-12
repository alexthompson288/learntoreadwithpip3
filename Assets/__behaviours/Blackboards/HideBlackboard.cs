using UnityEngine;
using System.Collections;

public class HideBlackboard : MonoBehaviour {
	[SerializeField]
	private Blackboard[] m_blackboards;
	
	void OnClick () 
	{
		for(int index = 0; index < m_blackboards.Length; ++index)
		{
			m_blackboards[index].Hide();
		}
	}

}
