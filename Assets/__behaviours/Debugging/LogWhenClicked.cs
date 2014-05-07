using UnityEngine;
using System.Collections;

public class LogWhenClicked : MonoBehaviour 
{
	void OnClick () 
	{
		Debug.Log(name + " has been clicked");
	}
}
