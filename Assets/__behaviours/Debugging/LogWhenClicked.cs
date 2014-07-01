using UnityEngine;
using System.Collections;

public class LogWhenClicked : MonoBehaviour 
{
	void OnClick () 
	{
		D.Log(name + " has been clicked");
	}
}
