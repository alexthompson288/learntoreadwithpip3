using UnityEngine;
using System.Collections;

public class DownloadProgressMonitor : MonoBehaviour 
{
	[SerializeField]
	private GameObject m_enableHierarchy;
	[SerializeField]
	private string[] m_downloadsToActivateFor;
	[SerializeField]
	private UISprite m_progressSprite;
	
	// Update is called once per frame
	void Update () 
	{
		bool shouldEnable = false;
		float amt = 0;
		int numDownloading = 0;
		foreach(string st in m_downloadsToActivateFor)
		{
			if ( DownloadProgressInformation.Instance.IsDownloading(st) )
			{
				//numDownloading++;
				//amt += DownloadProgressInformation.Instance.GetDownloadProgress(st);
				shouldEnable = true;
			}
		}
		
		if ( shouldEnable )
		{
			m_enableHierarchy.SetActive(true);
			//m_progressSprite.fillAmount = amt / (float)numDownloading;
		}
		else
		{
			m_enableHierarchy.SetActive(false);
		}
	}
}
