using UnityEngine;
using System.Collections;
using System;

public class JourneyAnimationPoint : MonoBehaviour 
{
	[SerializeField]
	private string m_videoName;
	[SerializeField]
	private int m_sessionNum;
	[SerializeField]
	private float m_dragThreshold = 10;

	bool m_mapIsBought;

	bool m_canDrag;

	float m_totalDeltaY;


#if UNITY_STANDALONE || UNITY_WEBPLAYER
	void Start ()
	{
		gameObject.SetActive(false);
	}
#endif

	void OnClick ()
	{
		if(m_canDrag)
		{
			Debug.Log("Clicked star " + m_sessionNum);
			if(m_mapIsBought)
			{
				if(String.IsNullOrEmpty(m_videoName))
				{
					SessionManager.Instance.SetSessionType(SessionManager.ST.Voyage);
					SessionManager.Instance.OnChooseSession(m_sessionNum);
				}
				else
				{
					Handheld.PlayFullScreenMovie(m_videoName, Color.black, FullScreenMovieControlMode.Full, FullScreenMovieScalingMode.AspectFit);
				}
			}
			else
			{
				JourneyCoordinator.Instance.OnClickMapCollider();
			}
		}
	}

	void OnPress (bool pressed) 
	{
		if(!pressed)
		{
			if(m_canDrag)
			{
				JourneyCoordinator.Instance.OnClickMapCollider();
			}
			
			m_canDrag = true;
			m_totalDeltaY = 0;
		}
	}
	
	void OnDrag (Vector2 delta)
	{
		if(m_canDrag)
		{
			m_totalDeltaY += delta.y;
			
			if(Mathf.Abs(m_totalDeltaY) > m_dragThreshold)
			{
				JourneyCoordinator.Instance.OnMapDrag(m_totalDeltaY);
				m_canDrag = false;
			}
		}
	}

	public void SetMapBought()
	{
		m_mapIsBought = true;
	}
}
