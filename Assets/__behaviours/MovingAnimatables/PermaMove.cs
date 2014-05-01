using UnityEngine;
using System.Collections;

public class PermaMove : MonoBehaviour 
{
	[SerializeField]
	private Vector3 m_direction = Vector3.zero;
	[SerializeField]
	private float m_speed = 0;
	[SerializeField]
	private bool m_startPaused = false;

	bool m_paused = false;

	void Awake()
	{
		if(m_startPaused)
		{
			Pause();
		}
	}

	public void SetDirection(Vector3 direction)
	{
		m_direction = direction;
	}

	public void SetSpeed(float speed)
	{
		m_speed = speed;
	}

	void Update()
	{
		if(!m_paused)
		{
			transform.position = transform.position + (m_direction.normalized * m_speed * Time.deltaTime);
		}
	}

	public void Pause()
	{
		m_paused = true;
	}

	public void Play()
	{
		Debug.Log("PermaMove.Play()");
		m_paused = false;
	}
}
