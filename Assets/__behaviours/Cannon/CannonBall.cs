using UnityEngine;
using System.Collections;

public class CannonBall : MonoBehaviour 
{
	DataRow m_data;

	void Start()
	{
		//iTween.ScaleFrom (gameObject, Vector3.zero, 0.5f); // This may or may not interfere with rigidbody motion, test it out first
	}

	public void SetUp(DataRow data)
	{
		m_data = data;
	}


}
