using UnityEngine;
using System.Collections;

namespace WingroveAudio
{
    [AddComponentMenu("WingroveAudio/Event Triggers/NGUI Click Events Trigger")]
    public class NGUIClickEvents : MonoBehaviour
    {

        public bool m_fireEventOnPress;
        public string m_onPressEvent;

        public bool m_fireEventOnClick;
        public string m_onClickEvent;

        public bool m_fireEventOnRelease;
        public string m_onReleaseEvent;

        public bool m_fireEventOnDoubleClick;
        public string m_onDoubleClickEvent;

        void OnPress(bool pressed)
        {
            if (pressed)
            {
                if (WingroveRoot.Instance != null)
                {
                    if (m_fireEventOnPress)
                    {
                        WingroveRoot.Instance.PostEvent(m_onPressEvent);
                    }
                }
            }
            else
            {
                if (WingroveRoot.Instance != null)
                {
                    if (m_fireEventOnRelease)
                    {
                        WingroveRoot.Instance.PostEvent(m_onReleaseEvent);
                    }
                }
            }
        }

        void OnClick()
        {
            if (WingroveRoot.Instance != null)
            {
                if (m_fireEventOnClick)
                {
                    WingroveRoot.Instance.PostEvent(m_onClickEvent);
                }
            }
        }

        void OnDoubleClick()
        {
            if (WingroveRoot.Instance != null)
            {
                if (m_fireEventOnDoubleClick)
                {
                    WingroveRoot.Instance.PostEvent(m_onDoubleClickEvent);
                }
            }
        }

    }
}