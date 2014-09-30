using UnityEngine;
using System.Collections;

public class LogTriggerEnterExit : MonoBehaviour {

	void OnTriggerEnter(Collider other)
	{
		////////D.Log(other.name + " has entered collider of " + name);
	}
	
	void OnTriggerExit(Collider other)
	{
		////////D.Log(other.name + " has exited collider of " + name);
	}
}
