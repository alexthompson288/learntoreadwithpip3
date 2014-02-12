using UnityEngine;
using System.Collections;

public class LettersChallengeMenuParticles : MonoBehaviour 
{	
	[SerializeField]
	ParticleSystem m_particles;
	
	// Update is called once per frame
	void Update () 
	{
		if(SessionInformation.Instance.GetCoins() == 0)
		{
			m_particles.enableEmission = false;
		}
		else
		{
			m_particles.enableEmission = true;
		}
	}
}
