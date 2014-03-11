using UnityEngine;
using System.Collections;

public class FlickableWidget : MonoBehaviour 
{
	[SerializeField]
	private Vector3 m_gravity = new Vector3(0, -0.5f, 0);
	[SerializeField]
	private float m_maxFallSpeed = -2f;

	Vector3 m_dragOffset;

	bool m_pressed = false;

	Vector3 m_lastPos;

	bool m_applyFlick = false;

	void Awake()
	{
		m_gravity.z = 0;
	}

	void FixedUpdate()
	{
		if(!m_pressed)
		{
			rigidbody.AddForce(m_gravity);

			if(rigidbody.velocity.y < m_maxFallSpeed)
			{
				Vector3 newVelocity = rigidbody.velocity;
				newVelocity.y = m_maxFallSpeed;
				rigidbody.velocity = newVelocity;
			}

			if(m_applyFlick)
			{
				Vector3 velocity = m_lastPos - transform.position;
				rigidbody.velocity = velocity;
				m_applyFlick = false;
			}
		}

		m_lastPos = transform.position;
	}

	void OnPress(bool pressed)
	{
		m_pressed = pressed;

		if (m_pressed)
		{
			iTween.Stop(gameObject);
				
			Ray camPos = UICamera.currentCamera.ScreenPointToRay(new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
			m_dragOffset = new Vector3(camPos.origin.x, camPos.origin.y, 0) - transform.position;
		}
		else
		{
			m_applyFlick = true;
		}
	}

	void OnDrag(Vector2 dragAmount)
	{
		Ray camPos = UICamera.currentCamera.ScreenPointToRay(
			new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
		transform.position = new Vector3(camPos.origin.x, camPos.origin.y, 0) - m_dragOffset;
			
		m_dragOffset = m_dragOffset - (Time.deltaTime * m_dragOffset);
	}
}
