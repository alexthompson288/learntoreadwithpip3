using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace WingroveAudio
{
    [RequireComponent(typeof(WingroveMixBus))]
    [AddComponentMenu("WingroveAudio/Ducking/Simple Automatic Duck")]
    public class SimpleAutomaticDuck : BaseAutomaticDuck
    {
        [SerializeField]
        private GameObject m_groupToMonitor = null;
        [SerializeField]
        private float m_duckingMixAmount = 1;
        [SerializeField]
        private float m_attack = 0.0f;
        [SerializeField]
        private float m_release = 0.0f;

        private BaseWingroveAudioSource[] m_audioSources;

        enum DuckState
        {
            Inactive,
            Attack,
            Hold,
            Release
        }

        DuckState m_currentState;
        float m_fadeT = 0.0f;
        float m_faderSpeed = 0.0f;

        WingroveMixBus m_mixBus;

        void Awake()
        {
            m_audioSources = m_groupToMonitor.GetComponentsInChildren<BaseWingroveAudioSource>();
            m_mixBus = GetComponent<WingroveMixBus>();
            m_mixBus.AddDuck(this);
        }

        void Update()
        {
            bool hasActiveCues = false;
            foreach (BaseWingroveAudioSource was in m_audioSources)
            {
                if (was.HasActiveCues())
                {
                    hasActiveCues = true;
                }
            }

            if (hasActiveCues)
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

        public override float GetDuckVol()
        {
            return (1.0f * (1 - m_fadeT)) + (m_duckingMixAmount * m_fadeT);
        }

        public override string GetGroupName()
        {
            return m_groupToMonitor.name;
        }

        public override string[] GetEvents()
        {
            return null;
        }

        public override void PerformAction(string eventName, GameObject targetObject, List<ActiveCue> cuesOut)
        {

        }

        public override void PerformAction(string eventName, List<ActiveCue> cuesIn, List<ActiveCue> cuesOut)
        {

        }
    }

}