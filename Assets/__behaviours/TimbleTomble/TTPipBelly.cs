using UnityEngine;
using System.Collections;

public class TTPipBelly : MonoBehaviour 
{
	bool m_isTickled = false;

	void OnPress (bool pressed)
	{
		if(!pressed)
		{
			m_isTickled = false;
		}
	}

	// Use this for initialization
	void OnDrag () 
	{
		if(!m_isTickled)
		{
			TTPip.Instance.OnTickle();
			m_isTickled = true;
		}
	}
}
