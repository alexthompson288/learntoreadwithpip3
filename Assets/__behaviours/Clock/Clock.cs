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
        string time = System.String.Format("{0}:{1}", m_hourHand.GetHourString(), m_minuteHand.GetMinuteString());
        return DateTime.ParseExact(time, "hh:mm", null);
    }

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
