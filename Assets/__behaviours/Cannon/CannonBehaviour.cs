using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CannonBehaviour : Singleton<CannonBehaviour> 
{
	[SerializeField]
	private GameObject m_cannonBallPrefab;
	[SerializeField]
	private Transform m_ballRestLocation;
	[SerializeField]
	private Vector2 m_pullRange;
	[SerializeField]
	private Vector2 m_forceRange;

	List<CannonBall> m_spawnedBalls = new List<CannonBall>();

	void Awake()
	{
		m_pullRange.x = Mathf.Clamp (m_pullRange.x, 0, m_pullRange.x);
		m_forceRange.x = Mathf.Clamp (m_forceRange.x, 0, m_forceRange.x);
	}

	public Vector3 GetBallRestPosition()
	{
		return m_ballRestLocation.position;
	}

	public float GetMaxPull()
	{
		return m_pullRange.y;
	}

	public void OnBallRelease(Rigidbody rb)
	{
		if ((m_ballRestLocation.position - rb.transform.position).magnitude < m_pullRange.x) 
		{
			iTween.MoveTo (rb.gameObject, m_ballRestLocation.position, 0.3f);
		} 
		else 
		{
			Launch(rb);
		}
	}

	void Launch(Rigidbody rb)
	{
		Debug.Log ("CannonBehaviour.Launch()");

		Vector3 delta = m_ballRestLocation.position - rb.transform.position;

		float distance = delta.magnitude;
		float proportionalDistance = (distance - m_pullRange.x) / (m_pullRange.y - m_pullRange.x);
		Debug.Log ("proportionalDistance: " + proportionalDistance);

		Vector3 direction = delta.normalized;
		Debug.Log ("direction: " + direction);

		Vector3 force = Vector3.Lerp (direction * m_forceRange.x, direction * m_forceRange.y, proportionalDistance);
		Debug.Log ("force: " + force);

		rb.AddForce (force, ForceMode.Acceleration); // ForceMode.Acceleration makes the application of force independent of object mass
	}
}
