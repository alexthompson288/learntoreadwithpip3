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
    private Transform m_ballDestroyLocation;
	[SerializeField]
	private Vector2 m_pullRange;
	[SerializeField]
	private Vector2 m_forceRange;
    [SerializeField]
    private Transform[] m_multiplayerLocations;

	List<CannonBall> m_spawnedBalls = new List<CannonBall>();

	void Awake()
	{
		m_pullRange.x = Mathf.Clamp (m_pullRange.x, 0, m_pullRange.x);
		m_forceRange.x = Mathf.Clamp (m_forceRange.x, 0, m_forceRange.x);

        StartCoroutine(SpawnBall(0));
	}

    public void MoveToMultiplayerLocation(int index)
    {
        if (index < m_multiplayerLocations.Length)
        {
            m_ballCentre.transform.localPosition = m_multiplayerLocations [index].transform.localPosition;
        }
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
            //Debug.Log ("proportionalDistance: " + proportionalDistance);
            
            Vector3 direction = delta.normalized;
            //Debug.Log ("direction: " + direction);
            
            Vector3 force = Vector3.Lerp (direction * m_forceRange.x, direction * m_forceRange.y, proportionalDistance);
            //Debug.Log ("force: " + force);

            ball.OnLaunch();
            ball.rigidbody.AddForce (force, ForceMode.VelocityChange); // ForceMode.VelocityChange makes the application of force independent of object mass

            StartCoroutine(SpawnBall());
		}
	}

    IEnumerator SpawnBall(float delay = 1)
    {
        yield return new WaitForSeconds(delay);
        GameObject newBall = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_cannonBallPrefab, m_ballCentre);
        newBall.GetComponent<CannonBall>().SetUp(this);
        m_spawnedBalls.Add(newBall.GetComponent<CannonBall>() as CannonBall);
    }

    void Update()
    {
        List<CannonBall> ballsToDestroy = m_spawnedBalls.FindAll(IsBallBelowThreshold);
        foreach (CannonBall ball in ballsToDestroy)
        {
            m_spawnedBalls.Remove(ball);
            Destroy(ball.gameObject);
        }
    }

    bool IsBallBelowThreshold(CannonBall ball)
    {
        return ball.transform.position.y < m_ballDestroyLocation.position.y;
    }

    public void OnBallDestroy(CannonBall ball)
    {
        m_spawnedBalls.Remove(ball);
    }
}
