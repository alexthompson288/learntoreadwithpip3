using UnityEngine;
using System.Collections;

public class HTTPService : MonoBehaviour {
	public void SendWWWRequest (string url, System.Action<WWW> callback)
	{
		StartCoroutine(SendWWWRequestCoroutine(url, callback));
	}
	
	public void SendWWWRequest (string url, byte[] postData, Hashtable headers, System.Action<WWW> callback)
	{
		headers["Content-Length"] = postData.Length;
		StartCoroutine(SendWWWRequestCoroutine(url, postData, headers, callback));
	}
	
	public void SendWWWRequest (string url, WWWForm form, System.Action<WWW> callback)
	{
		StartCoroutine(SendWWWRequestCoroutine(url, form, callback));
	}
	
	private IEnumerator SendWWWRequestCoroutine(string url, System.Action<WWW> callback)
	{
		WWW www = new WWW(url);
		yield return www;
		callback(www);
	}
	
	private IEnumerator SendWWWRequestCoroutine (string url, byte[] postData, Hashtable headers, System.Action<WWW> callback)
	{
		WWW www = new WWW(url, postData, headers);
		yield return www;
		callback(www);
	}
	
	private IEnumerator SendWWWRequestCoroutine(string url, WWWForm form, System.Action<WWW> callback)
	{
		WWW www = new WWW(url, form);
		yield return www;
		callback(www);
	}
}
