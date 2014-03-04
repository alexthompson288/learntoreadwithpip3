using UnityEngine;
using System.Collections;
using System;

public class JourneyMap : MonoBehaviour, IComparable
{
	[SerializeField]
	private int m_mapId;
	[SerializeField]
	private GameObject m_lockParent;
	[SerializeField]
	private UITexture m_background;
	[SerializeField]
	private Transform m_pointParent;
	[SerializeField]
	private AudioClip m_audioClip;
	[SerializeField]
	private int m_lowestSessionNum;
	[SerializeField]
	private int m_highestSessionNum;

	void Start()
	{
		Refresh();
	}

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

	public void Refresh()
	{
		bool isUnlocked = BuyManager.Instance.IsMapBought(m_mapId);

		if(m_background != null)
		{
			m_background.color = isUnlocked ? Color.white : Color.gray;
		}

		if(m_lockParent != null)
		{
			m_lockParent.SetActive(!isUnlocked);
		}

		if(isUnlocked)
		{
			JourneyPoint[] points = GetComponentsInChildren<JourneyPoint>() as JourneyPoint[];
			foreach(JourneyPoint point in points)
			{
				point.SetMapBought();
			}

			JourneyAnimationPoint[] animationPoints = GetComponentsInChildren<JourneyAnimationPoint>() as JourneyAnimationPoint[];
			foreach(JourneyAnimationPoint point in animationPoints)
			{
				point.SetMapBought();
			}
		}
	}
	
	public void OnClickMapCollider()
	{
		if(!BuyManager.Instance.IsMapBought(m_mapId))
		{
			BuyMapsCoordinator.Instance.Show(m_mapId);
		}
	}
}
