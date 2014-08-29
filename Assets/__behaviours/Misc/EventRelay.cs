using UnityEngine;
using System.Collections;

public class EventRelay : MonoBehaviour 
{
	public delegate void SimpleRelayEventHandler(EventRelay relay);
	public event SimpleRelayEventHandler SingleClicked;

	public delegate void BoolRelayEventHandler(EventRelay relay, bool pressed);
	public event BoolRelayEventHandler Pressed;

	public delegate void MouseOrTouchRelayEventHandler(EventRelay relay, UICamera.MouseOrTouch mot);
	public event MouseOrTouchRelayEventHandler Swiped;
	public event MouseOrTouchRelayEventHandler SwipedHorizontal;

	[SerializeField]
	private float m_swipeDistanceThreshold = 20;

	bool m_hasSwiped = false;
	bool m_hasSwipedHorizontal = false;
	bool m_hasDragged = false;

	bool m_isPressed = false;
	public bool isPressed
	{
		get
		{
			return m_isPressed;
		}
	}

	void OnPress(bool pressed)
	{
		m_isPressed = pressed;

		if (!pressed) 
		{
			m_hasDragged = false;
			m_hasSwiped = false;
			m_hasSwipedHorizontal = false;
		}
		
		if (Pressed != null) 
		{
			Pressed(this, pressed);
		}
	}

	void OnDrag(Vector2 delta)
	{
		m_hasDragged = true;

		if (Mathf.Abs(UICamera.currentTouch.totalDelta.x) > m_swipeDistanceThreshold && !m_hasSwiped && SwipedHorizontal != null) 
		{
			Debug.Log("SwipedHorizontal");
			SwipedHorizontal(this, UICamera.currentTouch);
		}

		if (UICamera.currentTouch.totalDelta.magnitude > m_swipeDistanceThreshold) 
		{
			m_hasSwiped = true;

			if(!m_hasSwiped && Swiped != null)
			{
				Debug.Log("Swiped");
				Swiped(this, UICamera.currentTouch);
			}
		}
	}

	/*
	// Player Interactions
	void OnDrag(Vector2 delta)
	{
		if(m_canDrag)
		{
			Ray camPos = UICamera.currentCamera.ScreenPointToRay(
				new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
			transform.position = new Vector3(camPos.origin.x, camPos.origin.y, 0) - m_dragOffset;
			
			m_dragOffset = m_dragOffset - (Time.deltaTime * m_dragOffset);
			
			m_hasDragged = true;
		}
	}
	
	void OnPress(bool pressed)
	{
		if (pressed)
		{
			m_startPosition = transform.position;
			Ray camPos = UICamera.currentCamera.ScreenPointToRay(new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
			m_dragOffset = new Vector3(camPos.origin.x, camPos.origin.y, 0) - transform.position;
			
			m_hasDragged = false;
		} 
	}
	*/

	void OnClick()
	{
		if(SingleClicked != null && (Swiped == null || !m_hasDragged))
		{
			SingleClicked(this);
		}
	}
	
	public void EnableCollider(bool enable)
	{
		collider.enabled = enable;
	}
}