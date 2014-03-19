using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class CannonBehaviour : Singleton<CannonBehaviour> 
{
	[SerializeField]
	private GameObject m_cannonBallPrefab;
	[SerializeField]
	private Transform m_ballCentre;
	[SerializeField]
	private Vector2 m_pullRange;
	[SerializeField]
	private Vector2 m_forceRange;

#if UNITY_EDITOR
	[SerializeField]
	private bool m_instantiateDebugBall;
#endif

	List<CannonBall> m_spawnedBalls = new List<CannonBall>();

	void Awake()
	{
		m_pullRange.x = Mathf.Clamp (m_pullRange.x, 0, m_pullRange.x);
		m_forceRange.x = Mathf.Clamp (m_forceRange.x, 0, m_forceRange.x);

#if UNITY_EDITOR
		if(m_instantiateDebugBall)
		{
			SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_cannonBallPrefab, m_ballCentre);
		}
#endif
	}

	public Vector3 GetBallCentrePos()
	{
		return m_ballCentre.position;
	}

	public float GetMaxPull()
	{
		return m_pullRange.y;
	}

	public float GetMinPull()
	{
		return m_pullRange.x;
	}

	public void OnBallRelease(CannonBall ball)
	{
		if ((m_ballCentre.position - ball.rigidbody.transform.position).magnitude < m_pullRange.x) 
		{
			iTween.MoveTo (ball.rigidbody.gameObject, m_ballCentre.position, 0.3f);
		} 
		else 
		{
            Vector3 delta = m_ballCentre.position - ball.rigidbody.transform.position;
            
            float distance = delta.magnitude;
            float proportionalDistance = (distance - m_pullRange.x) / (m_pullRange.y - m_pullRange.x);
            Debug.Log ("proportionalDistance: " + proportionalDistance);
            
            Vector3 direction = delta.normalized;
            Debug.Log ("direction: " + direction);
            
            Vector3 force = Vector3.Lerp (direction * m_forceRange.x, direction * m_forceRange.y, proportionalDistance);
            Debug.Log ("force: " + force);

            ball.On();
            ball.rigidbody.AddForce (force, ForceMode.Acceleration); // ForceMode.Acceleration makes the application of force independent of object mass
		}
	}
}
