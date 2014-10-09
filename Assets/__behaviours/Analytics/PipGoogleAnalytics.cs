using UnityEngine;
using System.Collections;

public class PipGoogleAnalytics : Singleton<PipGoogleAnalytics> 
{
    [SerializeField]
    private GoogleAnalyticsV3 m_devAnalytics;
    [SerializeField]
    private GoogleAnalyticsV3 m_productionAnalytics;

    GoogleAnalyticsV3 m_analytics;

    float m_startTime;

    void Start()
    {
        m_analytics = Debug.isDebugBuild ? m_devAnalytics : m_productionAnalytics;

#if UNITY_IPHONE
        AppPauseChecker.Instance.LaunchedAfterAppPause += OnLaunchAfterAppPause;
#endif
        StartCoroutine(OnLaunch());
    }

    // TODO: Call Launched after the app is started after Pausing
    void OnLaunchAfterAppPause()
    {
        D.Log("PipGoogleAnalytics.OnLaunchAfterAppPause()");

        StopCoroutine("OnLaunch");
        StartCoroutine("OnLaunch");
    }

    IEnumerator OnLaunch()
    {
        D.Log("PipGoogleAnalytics.OnLaunch()");

        m_startTime = Time.time;

        // Give IPChecker time to reset and then resolve
        yield return null; 
        yield return null;
        yield return StartCoroutine(IPChecker.WaitForResolution());
        Launched();
    }

    public void Launched()
    {
        D.Log("PipGoogleAnalytics.Launched()");
        m_startTime = Time.time;

        string deviceType = GetDeviceType();

        m_analytics.LogEvent(new EventHitBuilder()
                             .SetEventCategory("Launched")
                             .SetEventAction(deviceType)
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

        string deviceType = GetDeviceType();

        m_analytics.LogEvent(new EventHitBuilder()
                             .SetEventCategory("Exited")
                             .SetEventAction(deviceType)
                             .SetEventLabel(fTime.ToString())
                             .SetEventValue(lTime));
    }

    string GetDeviceType()
    {
#if UNITY_STANDALONE_OSX
        string platform = "OSX";
#elif UNITY_STANDALONE
        string platform = "Windows";
#else
        string platform = "iOS";
#endif

        return string.Format("{0}__{1}", platform, SystemInfo.deviceModel);
    }
}
