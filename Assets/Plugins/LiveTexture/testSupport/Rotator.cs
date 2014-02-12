using UnityEngine;
using System.Collections;


public class Rotator : MonoBehaviour
{
	public float speed = 35.0f;
	public bool onlyRotateY = false;
	
	private Transform cube;
	
	void Start()
	{
		cube = GetComponent<Transform>();
	}
	
	
	// Update is called once per frame
	void Update()
	{
		if( onlyRotateY )
			cube.Rotate( Vector3.up, Time.deltaTime * speed );
		else
			cube.Rotate( Vector3.up + Vector3.right, Time.deltaTime * speed );
	}
}
