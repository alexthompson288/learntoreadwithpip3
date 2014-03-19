using UnityEngine;
using System.Collections;
using System;

public class ServerPost : Singleton<ServerPost> 
{
	static string url = "http://pipperformance.herokuapp.com/tests";
	static string userPrefix = "ParentUser_";

	public void PostVoyageSession(int sessionNum)
	{
		WWWForm form = new WWWForm ();

		form.AddField ("test[username]", userPrefix + UserInfo.Instance.GetCurrentUser ());

		form.AddField ("test[session_num]", sessionNum);
		Debug.Log ("sessionNum: " + sessionNum);

		form.AddField ("test[completion_time]", (Time.time - SessionManager.Instance.GetTimeSessionStarted ()).ToString());
		Debug.Log ("time: " + (Time.time - SessionManager.Instance.GetTimeSessionStarted ()).ToString ());

		form.AddField ("test[email]", "dummy@email.com");

		WWW w = new WWW (url, form);

		StartCoroutine (WaitForRequest ("VoyageSessionComplete", w));
	}

	IEnumerator WaitForRequest(string eventName, WWW www)
	{
		Debug.Log ("Waiting for request");
		
		yield return www;
		
		// check for errors
		if (www.error == null)
		{
			Debug.Log(String.Format("WWW {0} - OK", eventName));
			Debug.Log("Data: " + www.data);
			Debug.Log("Text: " + www.text);
		} 
		else 
		{
			Debug.Log(String.Format("WWW {0} - ERROR", eventName));
			Debug.Log("Error: "+ www.error);
		}    
	}
}
