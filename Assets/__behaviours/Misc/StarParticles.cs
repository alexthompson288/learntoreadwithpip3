using UnityEngine;
using System.Collections;

public class StarParticles : MonoBehaviour {
	[SerializeField]
	private ParticleSystem m_particles;

	// Use this for initialization
	void Start () 
	{
		m_particles.enableEmission = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(transform.hasChanged)
		{
			m_particles.enableEmission = true;
			////D.Log("Particles!!!");
		}
		else
		{
			m_particles.enableEmission = false;
		}
	}
}
