using UnityEngine;
using System.Collections;

public static class DownloadHelper {

	public class TextDownload
	{
		public string m_textOut;
		public bool m_success;
	}

	public static IEnumerator DownloadTextFile(string url, TextDownload downloadInfo)
	{
		Debug.Log ("Downloading: " + url);
		WWW fileDownload = new WWW(url);
        yield return fileDownload;
        if (fileDownload.error == null)
        {
			Debug.Log ("Download of " + url + " was successful");
			downloadInfo.m_textOut = fileDownload.text;
			downloadInfo.m_success = true;
		}
		else
		{
			Debug.Log ("Download of " + url + " failed! " + fileDownload.error);
			downloadInfo.m_textOut = null;
			downloadInfo.m_success = false;
		}
		fileDownload.Dispose();
	}
}
