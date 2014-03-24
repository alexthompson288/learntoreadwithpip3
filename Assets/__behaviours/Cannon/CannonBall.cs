﻿using UnityEngine;
using System.Collections;
using Wingrove;

public class CannonBall : MonoBehaviour 
{
	[SerializeField]
	private string m_pressedAudio = "BUTTON_PRESS";
	[SerializeField]
	private string m_unpressedAudio = "BUTTON_UNPRESS";
	[SerializeField]
	private Vector3 m_gravity = new Vector3(0, -0.5f, 0);
	[SerializeField]
	private bool m_limitSpeed;
	[SerializeField]
	private Vector3 m_maxSpeed = new Vector3 (2, 2, 0);

    CannonBehaviour m_cannon = null;

#if UNITY_EDITOR
	[SerializeField]
	UISprite m_sprite;
#endif

	bool m_useGravity = false;

	DataRow m_data;

	Vector3 m_dragOffset;

	private bool m_canDrag = true;
    public bool canDrag
    {
        get
        {
            return m_canDrag;
        }
    }

	void Start()
	{
		iTween.ScaleFrom (gameObject, Vector3.zero, 0.5f); // This may or may not interfere with rigidbody motion, test it out first
	}

#if UNITY_EDITOR
	void Update()
	{
		Vector3 cannonDelta = transform.position - m_cannon.GetBallCentrePos();
		if (cannonDelta.magnitude > m_cannon.GetMaxPull ()) 
		{
			m_sprite.color = Color.red;
		} 
		else if (cannonDelta.magnitude < m_cannon.GetMinPull ()) 
		{
			m_sprite.color = Color.black;
		} 
		else 
		{
			m_sprite.color = Color.white;
		}
	}
#endif

	public void SetUp(CannonBehaviour cannon, DataRow data = null)
	{
        m_cannon = cannon;
		m_data = data;
	}

	public void SetCanDrag(bool canDrag)
	{
		m_canDrag = canDrag;
	}

	void FixedUpdate()
	{
        rigidbody.AddForce(m_gravity, ForceMode.Acceleration);

		if(m_limitSpeed)
		{
			Vector3 velocity = rigidbody.velocity;

            velocity.x = Mathf.Clamp(velocity.x, -m_maxSpeed.x, m_maxSpeed.x);
            velocity.y = Mathf.Clamp(velocity.y, -m_maxSpeed.y, m_maxSpeed.y);
			
			rigidbody.velocity = velocity;
		}
	}

	void OnPress(bool press)
	{
		WingroveAudio.WingroveRoot.Instance.PostEvent (press ? m_pressedAudio : m_unpressedAudio);
		
		if(m_canDrag)
		{
			if (press)
			{
				iTween.Stop(gameObject);

				Ray camPos = UICamera.currentCamera.ScreenPointToRay(new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
				m_dragOffset = new Vector3(camPos.origin.x, camPos.origin.y, 0) - transform.position;
			}
			else
			{
				m_cannon.OnBallRelease(this);
			}
		}
	}

	void OnDrag(Vector2 dragAmount)
	{
		if(m_canDrag)
		{
			Ray camPos = UICamera.currentCamera.ScreenPointToRay(new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
			transform.position = new Vector3(camPos.origin.x, camPos.origin.y, 0) - m_dragOffset;
			
			m_dragOffset = m_dragOffset - (Time.deltaTime * m_dragOffset);

			Vector3 cannonDelta = transform.position - m_cannon.GetBallCentrePos();
			if(cannonDelta.magnitude > m_cannon.GetMaxPull())
			{
				transform.position = m_cannon.GetBallCentrePos() + (cannonDelta.normalized * m_cannon.GetMaxPull());
			}
		}
	}

    public void On()
    {
        rigidbody.isKinematic = false;
        m_canDrag = false;
    }

    public void Explode()
    {
        m_cannon.OnBallDestroy(this);
        Destroy(gameObject);
    }
}
