using UnityEngine;
using System.Collections;

public class LogWhenInstantiated : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
    {
        Debug.Log(name + " instantiated");
	}
}
