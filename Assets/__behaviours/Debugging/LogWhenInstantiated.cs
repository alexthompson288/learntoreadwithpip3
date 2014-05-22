using UnityEngine;
using System.Collections;

public class LogWhenInstantiated : MonoBehaviour 
{
	// Use this for initialization
	void Awake () 
    {
        Debug.Log(gameObject.name.ToUpper + " INSTANTIATED");
	}
}
