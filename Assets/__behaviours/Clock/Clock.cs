using UnityEngine;
using System.Collections;
using System;

public class Clock : MonoBehaviour 
{
    [SerializeField]
    private ClockHand m_minutes;
    [SerializeField]
    private ClockHand m_hours;

    public DateTime GetDateTime()
    {
        string time = System.String.Format("{0}:{1}", m_hours.GetHourString(), m_minutes.GetMinuteString());
        return DateTime.ParseExact(time, "hh:mm", null);
    }
}
