using UnityEngine;
using System.Collections;
using System;

public class JourneyMap : MonoBehaviour, IComparable
{
	[SerializeField]
	private Transform m_pointParent;
	[SerializeField]
	private AudioClip m_audioClip;
	[SerializeField]
	private int m_lowestSessionNum;
	[SerializeField]
	private int m_highestSessionNum;

	public bool IsParent(Transform point)
	{
		return point.IsChildOf(m_pointParent);
	}

	public int CompareTo(object obj)
	{
		if(obj == null)
		{
			return 1;
		}
		else
		{
			JourneyMap other = obj as JourneyMap;
			if(other != null)
			{
				return transform.position.y.CompareTo(other.transform.position.y) * -1;
			}
			else
			{
				Debug.Log("obj is a " + obj.GetType().ToString());
				return 1;
			}
		}
	}

	public AudioClip GetAudio()
	{
		return m_audioClip;
	}

	public int GetLowestSessionNum()
	{
		return m_lowestSessionNum;
	}

	public int GetHighestSessionNum()
	{
		return m_highestSessionNum;
	}
}
