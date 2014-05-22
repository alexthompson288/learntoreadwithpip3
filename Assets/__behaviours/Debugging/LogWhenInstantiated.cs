using UnityEngine;
using System.Collections;

public class LogWhenInstantiated : MonoBehaviour 
{
	// Use this for initialization
	void Awake () 
    {
        Debug.Log("LogWhenInstantiated.Start()");
        Debug.Log(gameObject.name.ToUpper() + " INSTANTIATED");
	}

    void OnDestroy ()
    {
        Debug.Log("LogWhenInstantiated.OnDestroy()");
        Debug.Log(gameObject.name.ToUpper() + " DESTROYED");
    }
}
