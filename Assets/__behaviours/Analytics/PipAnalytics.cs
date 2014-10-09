using UnityEngine;
using System.Collections;

public class PipAnalytics : Singleton<PipAnalytics> 
{
    [SerializeField]
    private GoogleAnalyticsV3 m_analytics;

    float m_startTime;

    string m_deviceType;

    void Start()
    {
#if UNITY_IPHONE
        AppPauseChecker.Instance.LaunchedAfterAppPause += OnLaunchAfterAppPause;
#endif
        StartCoroutine(OnLaunch());
    }

    // TODO: Call Launched after the app is started after Pausing
    void OnLaunchAfterAppPause()
    {
        StopCoroutine("OnLaunch");
        StartCoroutine("OnLaunch");
    }

    IEnumerator OnLaunch()
    {
        m_startTime = Time.time;

        // Give IPChecker time to reset and then resolve
        yield return null; 
        yield return null;
        yield return StartCoroutine(IPChecker.WaitForResolution());
        Launched();
    }

    public void Launched()
    {
        m_startTime = Time.time;

#if UNITY_STANDALONE_OSX
        m_deviceType = "OSX";
#elif UNITY_STANDALONE
        m_deviceType = "Windows";
#else
        m_deviceType = "iOS";
#endif

        m_deviceType += SystemInfo.deviceModel;

        m_analytics.LogEvent(new EventHitBuilder()
                             .SetEventCategory("Launched")
                             .SetEventAction(m_deviceType)
                             .SetEventLabel(IPChecker.Instance.ipAddress));
    }

    public void ActivitiesCompleted()
    {
        m_analytics.LogEvent(new EventHitBuilder()
                             .SetEventCategory("ActivitiesCompleted")
                             .SetEventAction(GameManager.Instance.activity)
                             .SetEventLabel(string.Format("{0}__{1}__{2}", GameManager.Instance.pipColor.ToString(), GameManager.Instance.gameName, SessionInformation.Instance.GetNumPlayers())));
    }

    public void ActivitiesCancelled()
    {
        m_analytics.LogEvent(new EventHitBuilder()
                             .SetEventCategory("ActivitiesCancelled")
                             .SetEventAction(GameManager.Instance.activity)
                             .SetEventLabel(string.Format("{0}__{1}", GameManager.Instance.pipColor.ToString(), GameManager.Instance.gameName)));
    }

    void OnApplicationQuit()
    {
        Exiting();
    }

#if UNITY_IPHONE
    void OnApplicationPause()
    {
        Exiting();
    }
#endif

    void Exiting()
    {
        float fTime = Time.time - m_startTime;
        long lTime;
        try
        {
            lTime = System.Convert.ToInt64(fTime);
        }
        catch
        {
            lTime = -1;
        }

        m_analytics.LogEvent(new EventHitBuilder()
                             .SetEventCategory("Exited")
                             .SetEventAction(m_deviceType)
                             .SetEventValue(lTime));
    }
}
