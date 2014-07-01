using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerTracker : MonoBehaviour 
{
	List<GameObject> m_trackedObjects = new List<GameObject>();
	
	void OnTriggerEnter (Collider other) 
	{
		D.Log(other.name + " has entered " + name);
		m_trackedObjects.Add(other.gameObject);
	}

	void OnTriggerExit (Collider other) 
	{
		D.Log(other.name + " has exited " + name);
		if(m_trackedObjects.Contains(other.gameObject))
		{
			m_trackedObjects.Remove(other.gameObject);
		}
	}

	public bool IsTracking(GameObject go)
	{
		return m_trackedObjects.Contains(go);
	}

	void OnDisable()
	{
		m_trackedObjects.Clear();
	}
}
