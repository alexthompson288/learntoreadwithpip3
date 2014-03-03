using UnityEngine;
using System.Collections;

public class JourneyAnimationPoint : MonoBehaviour 
{
	[SerializeField]
	private int m_sessionNum;
	[SerializeField]
	private float m_dragThreshold = 10;

	bool m_mapIsBought;

	bool m_canDrag;

	float m_totalDeltaY;

	void OnClick ()
	{
		if(m_canDrag)
		{
			Debug.Log("Clicked star " + m_sessionNum);
			if(m_mapIsBought)
			{
				SessionManager.Instance.SetSessionType(SessionManager.ST.Voyage);
				SessionManager.Instance.OnChooseSession(m_sessionNum);
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
