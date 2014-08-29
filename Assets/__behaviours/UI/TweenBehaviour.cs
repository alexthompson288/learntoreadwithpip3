using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TweenBehaviour : MonoBehaviour 
{
	[SerializeField]
	private bool m_startOn;
	[SerializeField]
	private Transform m_offLocation;
	[SerializeField]
	private float m_delay;
	[SerializeField]
	private float m_duration = 0.2f;
	[SerializeField]
	private iTween.EaseType m_easetype = iTween.EaseType.easeOutQuad;
	[SerializeField]
	private float m_delayOff;
	[SerializeField]
	private float m_durationOff = 0.2f;
	[SerializeField]
	private iTween.EaseType m_easetypeOff = iTween.EaseType.linear;

	GameObject m_moveable;
	Transform m_onLocation;

	bool m_isOn = true;

	void SetUpNewTransform(Transform tra)
	{
		tra.parent = transform;
		tra.localPosition = Vector3.zero;
		tra.localScale = Vector3.one;
		tra.localRotation = Quaternion.identity;
	}
	
	void Awake()
	{
		m_duration = Mathf.Max (m_duration, 0.05f);
		m_durationOff = Mathf.Max (m_durationOff, 0.05f);
		
		m_delay = Mathf.Max (m_duration, 0);
		m_delayOff = Mathf.Max (m_duration, 0);
		
		int childCount = transform.childCount;
		
		List<Transform> children = new List<Transform>();
		for (int i = 0; i < childCount; ++i) 
		{
			if(transform.GetChild(i) != m_offLocation)
			{
				children.Add(transform.GetChild(i));
			}
		}
		
		m_moveable = new GameObject ("Moveable");
		SetUpNewTransform (m_moveable.transform);
		
		foreach (Transform child in children) 
		{
			child.parent = m_moveable.transform;
		}
		
		m_onLocation = new GameObject ("OnLocation").transform;
		SetUpNewTransform (m_onLocation);
		
		if (!m_startOn) 
		{
			m_moveable.transform.position = m_offLocation.position;
			m_isOn = false;
		}
	}

	public void On()
	{
		if (!m_isOn) 
		{
			m_isOn = true;
			StopCoroutine("OffCo");
			StartCoroutine("OnCo");
		}
	}

	IEnumerator OnCo()
	{
		yield return new WaitForSeconds (m_delay);

		iTween.Stop(m_moveable);
		
		Hashtable tweenArgs = new Hashtable();
		tweenArgs.Add("position", m_onLocation);
		tweenArgs.Add("time", m_duration);
		tweenArgs.Add("easetype", m_easetype);

		iTween.MoveTo (m_moveable, tweenArgs);
	}

	public void Off()
	{
		if (m_isOn) 
		{
			m_isOn = false;
			StopCoroutine("OnCo");
			StartCoroutine("OffCo");
		}
	}

	IEnumerator OffCo()
	{
		yield return new WaitForSeconds (m_delayOff);

		iTween.Stop(m_moveable);
		
		Hashtable tweenArgs = new Hashtable();
		tweenArgs.Add("position", m_offLocation);
		tweenArgs.Add("time", m_durationOff);
		tweenArgs.Add("easetype", m_easetypeOff);
		
		iTween.MoveTo (m_moveable, tweenArgs);
	}

    public bool isOn
    {
        get
        {
            return m_isOn;
        }
    }

    public float GetTotalDuration()
    {
        return m_delay + m_duration;
    }

    public float GetTotalDurationOff()
    {
        return m_delayOff + m_durationOff;
    }
}
