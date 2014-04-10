using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class JourneyMapCamera : Singleton<JourneyMapCamera> 
{
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private List<JourneyMap> m_maps = new List<JourneyMap>();
	[SerializeField]
	private float m_tweenDuration;
	[SerializeField]
	private Collider m_mapCollider;

	Transform m_currentPos;

	JourneyMap m_currentMap = null;

	void Awake()
	{
		m_maps.Sort();
	}

	public void MoveToStartMap(Transform journeyPoint)
	{
		foreach(JourneyMap map in m_maps)
		{
			if(map.IsParent(journeyPoint))
			{
				m_currentMap = map;
				break;
			}
		}

		if(m_currentMap == null)
		{
			m_currentMap = m_maps[0];
		}

		transform.position = m_currentMap.transform.position;

		StartCoroutine(DisableMaps(0));
		StartCoroutine(ChangeAudio(0));
	}

	public void TweenToPosition (float deltaY)
	{
		m_mapCollider.enabled = false;

		int i = m_maps.IndexOf(m_currentMap);
		
		if(deltaY < 0)
		{
			--i;
		}
		else
		{
			++i;
		}
		
		i = Mathf.Clamp(i, 0, m_maps.Count - 1);
		
		m_currentMap = m_maps[i];
		iTween.MoveTo(gameObject, m_currentMap.transform.position, m_tweenDuration);

		StartCoroutine(DisableMaps(m_tweenDuration / 2));
		StartCoroutine(ChangeAudio(m_tweenDuration / 2));
	}

	IEnumerator ChangeAudio(float delay)
	{
		yield return new WaitForSeconds(delay);

		AudioClip newClip = m_currentMap.GetAudio();

		if(m_audioSource.clip != newClip)
		{
			m_audioSource.clip = newClip;

			if(m_audioSource.clip != null)
			{
				m_audioSource.Play();
			}
		}
	}

	IEnumerator DisableMaps(float delay)
	{
		yield return new WaitForSeconds(delay);

		int i = m_maps.IndexOf(m_currentMap);

		foreach(JourneyMap map in m_maps)
		{
			map.gameObject.SetActive(Mathf.Abs(i - m_maps.IndexOf(map)) <= 1);
		}

		m_mapCollider.enabled = true;
	}
}
