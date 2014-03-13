using UnityEngine;
using System.Collections;
using System;

public class ServerPostTest : MonoBehaviour 
{
	[SerializeField]
	private bool m_postData;

	string url = "http://pipperformance.herokuapp.com/tests";

	// Use this for initialization
	void Start () 
	{
		Debug.Log ("ServerPostTest.Start()");
		if (m_postData) 
		{
			PostData ();
		}
		else
		{
			RequestData ();
		}
	}

	void RequestData()
	{
		Debug.Log ("RequestData");
		WWW www = new WWW(url);
		StartCoroutine(WaitForRequest(www));
	}

	bool m_testVar = true;

	void PostData () 
	{
		Debug.Log ("PostData");

		WWWForm form = new WWWForm ();

		form.AddField ("test[username]", "Tom2");
		form.AddField ("test[email]", "dummy@email.com");
		form.AddField ("test[sessions_complete]", 8);
		form.AddField ("test[has_read_story]", Convert.ToInt32(m_testVar));

		Debug.Log ("formData: " + form.data);

		WWW www = new WWW (url, form);

		StartCoroutine (WaitForRequest (www));
	}

	IEnumerator WaitForRequest(WWW www)
	{
		Debug.Log ("Waiting for request");

		yield return www;
			
		// check for errors
		if (www.error == null)
		{
			Debug.Log("WWW Ok!: " + www.data);
			Debug.Log("WWW Text: " + www.text);
		} 
		else 
		{
			Debug.Log("WWW Error: "+ www.error);
		}    
	}    
}
