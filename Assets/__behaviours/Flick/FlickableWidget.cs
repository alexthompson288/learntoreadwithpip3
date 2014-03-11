using UnityEngine;
using System.Collections;

public class FlickableWidget : MonoBehaviour 
{
	[SerializeField]
	private Vector3 m_gravity = new Vector3(0, -0.5f, 0);
	[SerializeField]
	private Vector3 m_maxSpeed = new Vector3 (2, 2, 0);

	Vector3 m_dragOffset;

	bool m_pressed = false;

	Vector3 m_lastPos;

	bool m_applyFlick = false;

	DataRow m_data = null;

	void Awake()
	{
		m_maxSpeed.x = Mathf.Abs (m_maxSpeed.x);
		m_maxSpeed.y = Mathf.Abs (m_maxSpeed.y);
		m_maxSpeed.z = 0;
		m_gravity.z = 0;
	}

	public void SetUp(DataRow data)
	{
		m_data = data;
	}

	void FixedUpdate()
	{
		if(!m_pressed)
		{
			rigidbody.AddForce(m_gravity);

			if(m_applyFlick)
			{
				Vector3 flickVelocity = (transform.position - m_lastPos) / Time.deltaTime;
				Debug.Log("Apply: " + flickVelocity);
				rigidbody.velocity = flickVelocity;
				m_applyFlick = false;
			}

			Vector3 velocity = rigidbody.velocity;
			
			if(Mathf.Abs(velocity.y) > m_maxSpeed.y)
			{
				velocity.y = velocity.y / Mathf.Abs(velocity.y) * m_maxSpeed.y; // Set velocity.y to equal m_maxSpeed.y but keep its sign
			}
			
			if(Mathf.Abs(velocity.x) > m_maxSpeed.x)
			{
				velocity.x = velocity.x / Mathf.Abs(velocity.x) * m_maxSpeed.x; // Set velocity.x to equal m_maxSpeed.x but keep its sign
			}
			
			rigidbody.velocity = velocity;
		}
		else
		{
			rigidbody.velocity = Vector3.zero;
		}

		m_lastPos = transform.position;
	}

	void OnPress(bool pressed)
	{
		m_pressed = pressed;

		if (m_pressed)
		{
			rigidbody.velocity = Vector3.zero;

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

	void OnGUI()
	{
		GUILayout.Label ("Velocity: " + rigidbody.velocity);
		GUILayout.Label ("Speed: " + rigidbody.velocity.magnitude);
	}
}
