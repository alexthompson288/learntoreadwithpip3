using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlickableWidget : MonoBehaviour 
{
	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private Vector3 m_preFlickGravity = new Vector3(0, -0.2f, 0);
    [SerializeField]
    private Vector3 m_postFlickGravity = new Vector3(0, -0.5f, 0);
	[SerializeField]
    private Vector3 m_maxSpeed = Vector3.one * -1;
    [SerializeField]
    private int m_maxDeltaCount = 4;
    [SerializeField]
    private float m_flickForceModifier = 1;

	Vector3 m_dragOffset;

    // TODO: MAke these into an enum
	bool m_pressed = false;
	bool m_applyFlick = false;
    bool m_hasFlicked = false;

	private DataRow m_data = null;
    public DataRow data
    {
        get
        {
            return m_data;
        }
    }

    List<Vector2> m_deltas = new List<Vector2>();

	void Awake()
	{
		m_maxSpeed.z = 0;
		m_preFlickGravity.z = 0;
        m_postFlickGravity.z = 0;
	}

	public void SetUp(DataRow data, string dataType)
	{
		m_data = data;

		string attribute = dataType == "phonemes" ? "phoneme" : "word";
		m_label.text = m_data[attribute].ToString();
	}

	void FixedUpdate()
	{
		if(!m_pressed)
		{
            if(m_hasFlicked)
            {
			    rigidbody.AddForce(m_preFlickGravity);
            }
            else
            {
                rigidbody.AddForce(m_postFlickGravity);
            }

			if(m_applyFlick)
			{
                Vector3 flickVelocity = Vector3.zero;
                foreach(Vector2 delta in m_deltas)
                {
                    flickVelocity += new Vector3(delta.x, delta.y, 0);
                }

                flickVelocity /= m_deltas.Count;
                flickVelocity *= m_flickForceModifier;
                flickVelocity *= Time.deltaTime;

				Debug.Log("Apply: " + flickVelocity);
				rigidbody.velocity = flickVelocity;
				m_applyFlick = false;
                m_hasFlicked = true;
			}

			if(!Mathf.Approximately(m_maxSpeed.x, -1))
			{
				Vector3 velocity = rigidbody.velocity;
				
                velocity.x = Mathf.Clamp(velocity.x, -m_maxSpeed.x, m_maxSpeed.x);
                velocity.y = Mathf.Clamp(velocity.y, -m_maxSpeed.y, m_maxSpeed.y);           
				
				rigidbody.velocity = velocity;
			}
		}
		else
		{
			rigidbody.velocity = Vector3.zero;
		}
	}

	void OnPress(bool pressed)
	{
		m_pressed = pressed;

		if (m_pressed)
		{
            m_deltas.Clear();

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

        m_deltas.Add(dragAmount);
        while (m_deltas.Count > m_maxDeltaCount)
        {
            m_deltas.RemoveAt(0);
        }
	}

    /*
	void OnGUI()
	{
		GUILayout.Label("Velocity: " + rigidbody.velocity);
		GUILayout.Label("Speed: " + rigidbody.velocity.magnitude);
	}
    */   
}
