using UnityEngine;
using System.Collections;

public class ThrobGUIElement : MonoBehaviour {

	[SerializeField]
	private Vector3 m_minScale;
	[SerializeField]
	private Vector3 m_maxScale;
	[SerializeField]
	private float m_minDuration;
	[SerializeField]
	private float m_maxDuration;
	[SerializeField]
	private bool m_startOn;
	[SerializeField]
	private TweenScale m_tweenBehaviour;

	private Vector3 m_targetScale;
	
	EventDelegate m_eventDelegate;
	
	void Awake()
	{
		m_eventDelegate = new EventDelegate(this, "OnTweenFinish");

		if(m_tweenBehaviour == null)
		{
			m_tweenBehaviour = GetComponent<TweenScale>() as TweenScale;

			if(m_tweenBehaviour == null)
			{
				m_tweenBehaviour = gameObject.AddComponent<TweenScale>() as TweenScale;
				m_tweenBehaviour.enabled = false;
			}
		}

		if(m_startOn)
		{
			On ();
		}
	}

	public void On()
	{
		if(m_tweenBehaviour == null)
		{
			m_tweenBehaviour = GetComponent<TweenScale>() as TweenScale;
		}

		m_targetScale = (Random.Range(0, 2) == 0) ? m_minScale : m_maxScale;
		TweenScale.Begin(gameObject, Random.Range(m_minDuration, m_maxDuration), m_targetScale);

		m_tweenBehaviour.onFinished.Add(m_eventDelegate);
	}

	public void Off(bool tweenToDefault = true)
	{
		if(m_tweenBehaviour == null)
		{
			m_tweenBehaviour = GetComponent<TweenScale>() as TweenScale;
		}

		m_tweenBehaviour.onFinished.Remove(m_eventDelegate);

		if(tweenToDefault)
		{
			TweenScale.Begin(gameObject, 0.2f, Vector3.one);
		}
	}

	void OnTweenFinish()
	{
		if(Mathf.Approximately(m_targetScale.x, m_maxScale.x))
		{
			m_targetScale = m_minScale;
		}
		else
		{
			m_targetScale = m_maxScale;
		}

		TweenScale.Begin(gameObject, Random.Range(m_minDuration, m_maxDuration), m_targetScale);
	}
}
