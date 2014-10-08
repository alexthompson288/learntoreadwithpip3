using UnityEngine;
using System.Collections;

public class PipAnalytics : Singleton<PipAnalytics> 
{
    [SerializeField]
    private GoogleAnalyticsV3 m_analytics;

    float m_startTime;

    IEnumerator Start()
    {
        yield return StartCoroutine(IPChecker.WaitForResolution());
        Launched();
    }

    // TODO: Call Launched after the app is started after Pausing


    public void Launched()
    {
        m_startTime = Time.time;

#if UNITY_STANDALONE_OSX
        string deviceType = "OSX";
#elif UNITY_STANDALONE
        string deviceType = "Windows";
#else
        string deviceType = "iOS";
#endif

        deviceType += SystemInfo.deviceModel;

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
                             .SetEventLabel(string.Format("{0}__{1}__{2}", GameManager.Instance.pipColor.ToString(), GameManager.Instance.gameName, SessionInformation.Instance.GetNumPlayers())));                                 .SetEventValue(5));
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

    void OnApplicationPause()
    {
        Exiting();
    }

    // TODO
    void Exiting()
    {
        float m_time = Time.time - m_startTime;
    }
}
