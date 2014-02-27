using UnityEngine;
using System.Collections;

public class JourneyMapCollider : MonoBehaviour 
{
	[SerializeField]
	private float m_dragThreshold = 10;

	private float m_totalDeltaY = 0;

	private bool m_canDrag = true;

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
}
