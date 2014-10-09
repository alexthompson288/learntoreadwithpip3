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
                             .SetEventLabel(string.Format("<{0}>__{1}", SystemInfo.deviceUniqueIdentifier, IPChecker.Instance.ipAddress)));
    }

    public void ActivitiesCompleted()
    {
        string activityName = GameManager.Instance.activity == ProgrammeInfo.voyage ? VoyageInfo.Instance.currentSessionId.ToString() : GameManager.Instance.gameName;

        m_analytics.LogEvent(new EventHitBuilder()
                             .SetEventCategory("ActivitiesCompleted")
                             .SetEventAction(GameManager.Instance.activity)
                             .SetEventLabel(string.Format("<{0}>__{1}__{2}__{3}", 
                                     new object[] 
                                     { 
            SystemInfo.deviceUniqueIdentifier, GameManager.Instance.pipColor.ToString(), activityName, SessionInformation.Instance.GetNumPlayers() 
        })));
    }

    public void ActivitiesCancelled()
    {
        m_analytics.LogEvent(new EventHitBuilder()
                             .SetEventCategory("ActivitiesCancelled")
                             .SetEventAction(GameManager.Instance.activity)
                             .SetEventLabel(string.Format("<{0}>__{1}__{2}", SystemInfo.deviceUniqueIdentifier, GameManager.Instance.pipColor.ToString(), GameManager.Instance.gameName)));
    }

    public void Purchased(string productId)
    {
        m_analytics.LogEvent(new EventHitBuilder()
                             .SetEventCategory("Purchased")
                             .SetEventAction(productId)
                             .SetEventLabel(string.Format("<{0}>__{1}", SystemInfo.deviceUniqueIdentifier, productId)));
    }

    public void PurchaseCancelled(string productId)
    {
        m_analytics.LogEvent(new EventHitBuilder()
                             .SetEventCategory("PurchaseCancelled")
                             .SetEventAction(productId)
                             .SetEventLabel(string.Format("<{0}>__{1}", SystemInfo.deviceUniqueIdentifier, productId)));
    }

    void OnApplicationQuit()
    {
        D.Log("PipGoogleAnalytics.OnApplicationQuit()");
        Exiting();
    }

    // Editor calls OnApplicationPause at launch
#if UNITY_IPHONE && !UNITY_EDITOR 
    void OnApplicationPause()
    {
        D.Log("PipGoogleAnalytics.OnApplicationPause()");
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
                             .SetEventLabel(SystemInfo.deviceUniqueIdentifier)
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
