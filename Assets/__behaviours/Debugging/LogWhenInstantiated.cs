using UnityEngine;
using System.Collections;

public class LogWhenInstantiated : MonoBehaviour 
{
	// Use this for initialization
	void Awake () 
    {
        ////////D.Log("LogWhenInstantiated.Start()");
        ////////D.Log(gameObject.name.ToUpper() + " INSTANTIATED");
	}

    void OnDestroy ()
    {
        ////////D.Log("LogWhenInstantiated.OnDestroy()");
        ////////D.Log(gameObject.name.ToUpper() + " DESTROYED");
    }
}
