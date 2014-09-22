using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerTracker : MonoBehaviour 
{
    public delegate void TriggerTrackerEventHandler (TriggerTracker tracker, Collider other);
    public event TriggerTrackerEventHandler Entered;
    public event TriggerTrackerEventHandler Exited;

	List<GameObject> m_trackedObjects = new List<GameObject>();

    public int GetNumTrackedObjects()
    {
        return m_trackedObjects.Count;
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        if (name == "Mid")
        {
            GUILayout.Label("");
        }
        else if (name == "Right")
        {
            GUILayout.Label("");
            GUILayout.Label("");
        }
        GUILayout.Label(name + ": " + m_trackedObjects.Count.ToString());
    }
#endif

    void Start()
    {
        StartCoroutine(RemoveNull());
    }

    IEnumerator RemoveNull()
    {
        yield return new WaitForSeconds(0.5f);
        m_trackedObjects.RemoveAll(x => x == null);
        StartCoroutine(RemoveNull());
    }
	
	void OnTriggerEnter (Collider other) 
	{
        if (!m_trackedObjects.Contains(other.gameObject))
        {
            m_trackedObjects.Add(other.gameObject);

            if(Entered != null)
            {
                Entered(this, other);
            }
        }
	}

	void OnTriggerExit (Collider other) 
	{
		if(m_trackedObjects.Contains(other.gameObject))
		{
			m_trackedObjects.Remove(other.gameObject);

            if(Exited != null)
            {
                Exited(this, other);
            }
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
