using UnityEngine;
using System.Collections;
using System;

public class LoginTestCoordinator : MonoBehaviour 
{
	[SerializeField]
	private string m_loginUrl = "learntoreadwithpip.com/admins/sign_in";

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return new WaitForSeconds (2f);

		Debug.Log ("Creating form");
		WWWForm form = new WWWForm ();

		Debug.Log ("Adding to form");
		form.AddField ("username", "alex@learnwithpip.com");
		form.AddField ("password", "pip12345");

		Debug.Log ("Creating www");
		WWW www = new WWW (m_loginUrl, form);

		Debug.Log ("Wait for download");
		yield return www;

		Debug.Log ("Finished downloading");

		// check for errors
		if (www.error == null)
		{
			Debug.Log("WWW - OK");
			Debug.Log("Data: " + www.data);
			Debug.Log("Text: " + www.text);
		} 
		else 
		{
		    Debug.Log("WWW - ERROR");
			Debug.Log("Error: "+ www.error);
		}
	}
}
