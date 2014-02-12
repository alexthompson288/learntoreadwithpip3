using UnityEngine;
using System.Collections;

public class ExplosionLetter : MonoBehaviour 
{
	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private Color[] m_colors;

	//Vector3 m_lastVelocity;
	
	//Vector3 m_incident = Vector3.zero;
	//Vector3 m_reflection = Vector3.zero;
	
	void Awake()
	{
		m_label.color = m_colors[Random.Range(0, m_colors.Length)];
		m_label.text = "a";
		
		for (char i = 'a'; i <= 'z'; ++i)
		{
			string result = i == 'q' ? "qu" : i.ToString();
			
			if(Random.Range(0, 27) == 0)
			{
				m_label.text = result;
				break;
			}
		}
	}

	/*
	void LateUpdate()
	{
		m_lastVelocity = rigidbody.velocity;


		if(!Mathf.Approximately(m_reflection.magnitude, 0))
		{
			Debug.DrawRay(transform.position, m_incident, Color.green);
			Debug.DrawRay(transform.position, m_reflection, Color.red);
		}
	}
	*/

	void OnCollisionEnter(Collision other)
	{
		Vector3 normal = other.contacts[0].normal;

		Vector3 reflection = 2 * normal * Vector3.Dot(-rigidbody.velocity, normal) + rigidbody.velocity;
		//Vector3 reflection = 2 * normal * Vector3.Dot(-m_lastVelocity, normal) + m_lastVelocity;
		reflection.Normalize();
		reflection *= rigidbody.velocity.magnitude;

		//m_incident = m_lastVelocity;
		//m_reflection = reflection;

		rigidbody.velocity = reflection;
	}
}
