using UnityEngine;
using System.Collections;
namespace WingroveAudio
{
    [AddComponentMenu("WingroveAudio/Event Triggers/Lifetime Event Trigger")]
    public class LifetimeEventTrigger : MonoBehaviour
    {
        [SerializeField]
        private bool m_fireEventOnStart = false;
        [SerializeField]
        private string m_startEvent = "";
        [SerializeField]
        private bool m_fireEventOnEnable = false;
        [SerializeField]
        private string m_onEnableEvent = "";
        [SerializeField]
        private bool m_fireEventOnDisable = false;
        [SerializeField]
        private string m_onDisableEvent = "";
        [SerializeField]
        private bool m_fireEventOnDestroy = false;
        [SerializeField]
        private string m_onDestroyEvent = "";
        [SerializeField]
        private bool m_dontPlayDestroyIfDisabled = true;
        [SerializeField]
        private bool m_linkToObject = true;

        // Use this for initialization
        void Start()
        {
            if (WingroveRoot.Instance != null)
            {
                if (m_fireEventOnStart)
                {
                    if (m_linkToObject)
                    {
                        WingroveRoot.Instance.PostEventGO(m_startEvent, gameObject);
                    }
                    else
                    {
                        WingroveRoot.Instance.PostEvent(m_startEvent);
                    }
                }
            }
        }

        void OnDisable()
        {
            if (WingroveRoot.Instance != null)
            {
                if (m_fireEventOnDisable)
                {
                    if (m_linkToObject)
                    {
                        WingroveRoot.Instance.PostEventGO(m_onDisableEvent, gameObject);
                    }
                    else
                    {
                        WingroveRoot.Instance.PostEvent(m_onDisableEvent);
                    }
                }
            }
        }

        void OnEnable()
        {
            if (WingroveRoot.Instance != null)
            {
                if (m_fireEventOnEnable)
                {
                    if (m_linkToObject)
                    {
                        WingroveRoot.Instance.PostEventGO(m_onEnableEvent, gameObject);
                    }
                    else
                    {
                        WingroveRoot.Instance.PostEvent(m_onEnableEvent);
                    }
                }
            }
        }


        void OnDestroy()
        {
            if (WingroveRoot.Instance != null)
            {
                if (m_fireEventOnDestroy)
                {
                    if ((gameObject.activeInHierarchy) || (!m_dontPlayDestroyIfDisabled))
                    {
                        if (m_linkToObject)
                        {
                            WingroveRoot.Instance.PostEventGO(m_onDestroyEvent, gameObject);
                        }
                        else
                        {
                            WingroveRoot.Instance.PostEvent(m_onDestroyEvent);
                        }
                    }
                }
            }
        }
    }

}