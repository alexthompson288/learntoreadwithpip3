using UnityEngine;
using System.Collections;

public class FlickableWidget2D : MonoBehaviour 
{
	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private Vector2 m_gravity = new Vector2(0, -0.5f);
	[SerializeField]
	private Vector2 m_maxSpeed = new Vector2 (2, 2);
	
	Vector3 m_dragOffset;
	
	bool m_pressed = false;
	
	Vector3 m_lastPos;
	
	bool m_applyFlick = false;
	
	DataRow m_data = null;
	
	[SerializeField]
	private bool m_limitSpeed;
	[SerializeField]
	private bool m_useGravity;
	
	void Awake()
	{
		m_maxSpeed.x = Mathf.Abs (m_maxSpeed.x);
		m_maxSpeed.y = Mathf.Abs (m_maxSpeed.y);
	}
	
	public void SetUp(DataRow data, Game.Data dataType)
	{
		m_data = data;
		
		string attribute = dataType == Game.Data.Phonemes ? "phoneme" : "word";
		m_label.text = m_data[attribute].ToString();
	}
	
	void FixedUpdate()
	{
		if(!m_pressed)
		{
			if(m_useGravity)
			{
				rigidbody2D.AddForce(m_gravity);
			}

			if(m_applyFlick)
			{
				Vector3 flickVelocity = (transform.position - m_lastPos) / Time.deltaTime;
				Debug.Log("Apply: " + flickVelocity);
				rigidbody2D.velocity = new Vector2(flickVelocity.x, flickVelocity.y);
				m_applyFlick = false;
			}
			
			if(m_limitSpeed)
			{
				Vector2 velocity = rigidbody2D.velocity;

				if(Mathf.Abs(velocity.y) > m_maxSpeed.y)
				{
					velocity.y = velocity.y / Mathf.Abs(velocity.y) * m_maxSpeed.y; // Set velocity.y to equal m_maxSpeed.y but keep its sign
				}
				
				if(Mathf.Abs(velocity.x) > m_maxSpeed.x)
				{
					velocity.x = velocity.x / Mathf.Abs(velocity.x) * m_maxSpeed.x; // Set velocity.x to equal m_maxSpeed.x but keep its sign
				}
				
				rigidbody2D.velocity = velocity;
			}
		}
		else
		{
			rigidbody2D.velocity = Vector2.zero;
		}
		
		m_lastPos = transform.position;
	}
	
	void OnPress(bool pressed)
	{
		m_pressed = pressed;
		
		if (m_pressed)
		{
			rigidbody2D.velocity = Vector2.zero;
			rigidbody2D.Sleep();
			
			iTween.Stop(gameObject);
			
			Ray camPos = UICamera.currentCamera.ScreenPointToRay(new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
			m_dragOffset = new Vector3(camPos.origin.x, camPos.origin.y, 0) - transform.position;
		}
		else
		{
			rigidbody2D.WakeUp();
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
		GUILayout.Label("Velocity: " + rigidbody2D.velocity);
		GUILayout.Label("Speed: " + rigidbody2D.velocity.magnitude);
	}
}
