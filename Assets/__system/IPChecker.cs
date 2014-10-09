using UnityEngine;
using System.Collections;

public class IPChecker : Singleton<IPChecker> 
{
    string m_ipAddress = "";
    public string ipAddress
    {
        get
        {
            return m_ipAddress;
        }
    }

    public static IEnumerator WaitForResolution()
    {
        while (Instance == null)
        {
            yield return null;
        }

        while (string.IsNullOrEmpty(Instance.m_ipAddress))
        {
            yield return null;
        }
    }

    void Start()
    {
#if UNITY_IPHONE
        AppPauseChecker.Instance.LaunchedAfterAppPause += OnLaunchAfterAppPause;
#endif

        StartCoroutine("FindIpAddress");
    }

#if UNITY_IPHONE
    void OnLaunchAfterAppPause()
    {
        StopCoroutine("FindIpAddress");
        m_ipAddress = "";
        StartCoroutine("FindIpAddress");
    }
#endif

    IEnumerator FindIpAddress()
    {   
        D.Log("IPChecker.FindIPAddress()");
        WWW myExtIPWWW = null;
        
        try
        {
            myExtIPWWW = new WWW("http://checkip.dyndns.org");
        }
        catch
        {
            myExtIPWWW = null;
        }
        
        if (myExtIPWWW != null)
        {
            while (!myExtIPWWW.isDone)
            {
                yield return null;
            }
            
            try
            {
                m_ipAddress = myExtIPWWW.data;
                m_ipAddress = m_ipAddress.Substring(m_ipAddress.IndexOf(":") + 1);
                m_ipAddress = m_ipAddress.Substring(0, m_ipAddress.IndexOf("<"));
            }
            catch
            {
                m_ipAddress = "No_IP_Found";
            }
        }         
    }
}
