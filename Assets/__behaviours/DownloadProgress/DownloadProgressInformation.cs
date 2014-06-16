using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DownloadProgressInformation : Singleton<DownloadProgressInformation> 
{
	Dictionary<string, float> m_activeDownloads = new Dictionary<string, float>();

	public bool IsDownloading(string downloadName)
	{
		return m_activeDownloads.ContainsKey(downloadName);
	}
	
	public float GetDownloadProgress(string downloadName)
	{
		return m_activeDownloads[downloadName];
	}
	
	public void SetDownloading(string downloadName, float amt)
	{
		m_activeDownloads[downloadName] = amt;
	}
	
	public void StopDownloading(string downloadName)
	{
		m_activeDownloads.Remove(downloadName);
	}
}
