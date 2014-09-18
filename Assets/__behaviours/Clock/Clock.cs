using UnityEngine;
using System.Collections;
using System;

public class Clock : MonoBehaviour 
{
    [SerializeField]
    private ClockHand m_minuteHand;
    [SerializeField]
    private ClockHand m_hourHand;

    ClockHand m_currentHand = null;
    ClockHand m_followHand = null;

    void Awake()
    {
        m_minuteHand.Dragged += OnHandDrag;
        m_hourHand.Dragged += OnHandDrag;
    }

    public DateTime GetDateTime()
    {
        int minutes = m_minuteHand.GetMinutes();
        string minuteString = MathHelpers.GetDigitCount(minutes) == 2 ? minutes.ToString() : System.String.Format("0{0}", minutes);

        int hours = m_hourHand.GetHours(m_minuteHand, true);
        string hourString = MathHelpers.GetDigitCount(hours) == 2 ? hours.ToString() : System.String.Format("0{0}", hours);

        string time = System.String.Format("{0}:{1}", hourString, minuteString);
        return DateTime.ParseExact(time, "hh:mm", null);
    }

    /*
    string GetHourString(int hours)
    {
        return MathHelpers.GetDigitCount(hours) == 2 ? hours.ToString() : System.String.Format("0{0}", hours);
    }
    
    string GetMinuteString(int minutes)
    {
        return MathHelpers.GetDigitCount(minutes) == 2 ? minutes.ToString() : System.String.Format("0{0}", minutes);
    }
    */

    public void OnHandPress(ClockHand hand)
    {
        if (m_currentHand == null)
        {
            m_currentHand = hand;
            m_followHand = m_currentHand == m_minuteHand ? m_hourHand : m_minuteHand;
        }
    }

    public void OnHandUnpress(ClockHand hand)
    {
        if (m_currentHand == hand)
        {
            m_currentHand = null;
            m_followHand = null;
        }
    }

    void OnHandDrag(ClockHand hand)
    {
        if (m_currentHand == hand)
        {
            m_currentHand.Move();
            m_followHand.Follow(m_currentHand.GetProportional());
        }
    }
}
