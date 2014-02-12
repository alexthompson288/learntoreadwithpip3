using UnityEngine;
using System.Collections;

namespace WingroveAudio
{
    [AddComponentMenu("WingroveAudio/Event Triggers/NGUI Hover Events Trigger")]
    public class NGUIHoverEvents : MonoBehaviour
    {

        public bool m_fireEventOnHover;
        public string m_hoverEvent;

        public bool m_fireEventOnStopHover;
        public string m_stopHoverEvent;


        void OnHover(bool hover)
        {
            if (WingroveRoot.Instance != null)
            {
                if (hover)
                {
                    if (m_fireEventOnHover)
                    {
                        WingroveRoot.Instance.PostEvent(m_hoverEvent);
                    }
                }
                else
                {
                    if (m_fireEventOnStopHover)
                    {
                        WingroveRoot.Instance.PostEvent(m_stopHoverEvent);
                    }
                }
            }
        }
    }
}