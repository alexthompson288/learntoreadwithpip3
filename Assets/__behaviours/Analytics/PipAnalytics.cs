using UnityEngine;
using System.Collections;

public class PipAnalytics : Singleton<PipAnalytics> 
{
    [SerializeField]
    private GoogleAnalyticsV3 m_analytics;

    public void Launched()
    {

    }

    public void ActivitiesCompleted()
    {
        m_analytics.LogEvent(new EventHitBuilder()
                             .SetEventCategory("ActivitiesCompleted")
                             .SetEventAction(GameManager.Instance.activity)
                             .SetEventLabel(string.Format("{0}__{1}__{2}", GameManager.Instance.pipColor.ToString(), GameManager.Instance.gameName, SessionInformation.Instance.GetNumPlayers())));

//        googleAnalytics.LogEvent(new EventHitBuilder()
//                                 .SetEventCategory("Achievement")
//                                 .SetEventAction("Unlocked")
//                                 .SetEventLabel("Slay 10 dragons")
//                                 .SetEventValue(5));
    }

    public void ActivitiesCancelled()
    {
        m_analytics.LogEvent(new EventHitBuilder()
                             .SetEventCategory("ActivitiesCancelled")
                             .SetEventAction(GameManager.Instance.activity)
                             .SetEventLabel(string.Format("{0}__{1}", GameManager.Instance.pipColor.ToString(), GameManager.Instance.gameName)));
    }
}
