using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WingroveAudio
{
    [RequireComponent(typeof(WingroveMixBus))]
    [AddComponentMenu("WingroveAudio/Ducking/Duck on Event")]
    public class DuckOnEvent : BaseAutomaticDuck
    {

        [SerializeField]
        private string m_startDuckEvent = "";
        [SerializeField]
        private string m_endDuckEvent = "";

        [SerializeField]
        private float m_attack = 0.0f;
        [SerializeField]
        private float m_release = 0.0f;

        [SerializeField]
        private float m_duckingMixAmount = 1.0f;

        private bool m_isActive = false;

        enum DuckState
        {
            Inactive,
            Attack,
            Hold,
            Release
        }

        private DuckState m_currentState = DuckState.Inactive;
        private float m_fadeT = 0.0f;
        private float m_faderSpeed = 0.0f;

        private WingroveMixBus m_mixBus;

        void Awake()
        {
            m_mixBus = GetComponent<WingroveMixBus>();
            m_mixBus.AddDuck(this);
        }

        void Update()
        {
            if (m_isActive)
            {

                if ((m_currentState == DuckState.Inactive)
                    || (m_currentState == DuckState.Release))
                {
                    if (m_attack == 0.0f)
                    {
                        m_currentState = DuckState.Hold;
                        m_fadeT = 1.0f;
                    }
                    else
                    {
                        m_currentState = DuckState.Attack;
                        m_faderSpeed = (1.0f) / m_attack;
                    }
                }
            }
            else
            {
                if (m_currentState != DuckState.Inactive)
                {
                    if (m_attack == 0.0f)
                    {
                        m_currentState = DuckState.Inactive;
                        m_fadeT = 0.0f;
                    }
                    else
                    {
                        m_currentState = DuckState.Release;
                        m_faderSpeed = (1.0f) / m_release;
                    }
                }
            }

            switch (m_currentState)
            {
                case DuckState.Attack:
                    m_fadeT += m_faderSpeed * WingroveRoot.GetDeltaTime();
                    if (m_fadeT >= 1.0f)
                    {
                        m_fadeT = 1.0f;
                        m_currentState = DuckState.Hold;
                    }
                    break;
                case DuckState.Hold:
                    m_fadeT = 1.0f;
                    break;
                case DuckState.Release:
                    m_fadeT -= m_faderSpeed * WingroveRoot.GetDeltaTime();
                    if (m_fadeT <= 0.0f)
                    {
                        m_fadeT = 0.0f;
                        m_currentState = DuckState.Inactive;
                    }
                    break;
                case DuckState.Inactive:
                    break;
            }
        }

        public override string GetGroupName()
        {
            return name;
        }

        public override float GetDuckVol()
        {
            return (1.0f * (1 - m_fadeT)) + (m_duckingMixAmount * m_fadeT);
        }

        public override string[] GetEvents()
        {
            return new string[] { m_startDuckEvent, m_endDuckEvent };
        }

        void PerformInternal(string eventName)
        {
            if (eventName == m_startDuckEvent)
            {
                m_isActive = true;
            }
            else if (eventName == m_endDuckEvent)
            {
                m_isActive = false;
            }
        }

        public override void PerformAction(string eventName, GameObject targetObject, List<ActiveCue> cuesOut)
        {
            PerformInternal(eventName);
        }

        public override void PerformAction(string eventName, List<ActiveCue> cuesIn, List<ActiveCue> cuesOut)
        {
            PerformInternal(eventName);
        }
    }

}