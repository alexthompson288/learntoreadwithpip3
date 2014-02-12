using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace WingroveAudio
{
    [RequireComponent(typeof(WingroveMixBus))]
    [AddComponentMenu("WingroveAudio/Ducking/Level Based Automatic Duck")]
    public class LevelBasedAutomaticDuck : BaseAutomaticDuck
    {

        [SerializeField]
        private WingroveMixBus m_mixBusToMonitor = null;
        [SerializeField]
        private float m_threshold = 0.5f;
        [SerializeField]
        private float m_duckingMixAmount = 1;
        [SerializeField]
        private float m_ratio = 4.0f;
        [SerializeField]
        private float m_attack = 0.0f;
        [SerializeField]
        private float m_release = 0.0f;

        float m_fadeT = 0.0f;

        WingroveMixBus m_mixBus;

        void Awake()
        {
            m_mixBus = GetComponent<WingroveMixBus>();
            m_mixBus.AddDuck(this);
        }

        public WingroveMixBus GetMixBusToMonitor()
        {
            return m_mixBusToMonitor;
        }

        void Update()
        {
            if (m_ratio != 0)
            {
                float target = 0.0f;
                // use peak
                float rmsTarg = m_mixBusToMonitor.GetRMS() / 0.707f;
                float rmsPopRatio = Mathf.Max(0, (rmsTarg - m_threshold)) * m_ratio;
                target = Mathf.Clamp01(rmsPopRatio);

                float fadeSpeed = target > m_fadeT ? (1.0f / Mathf.Max(m_attack, 0.01f)) : (1.0f / Mathf.Max(m_release, 0.01f));

                m_fadeT += Mathf.Min(Mathf.Abs(fadeSpeed * WingroveRoot.GetDeltaTime()), Mathf.Abs(target - m_fadeT)) *
                    Mathf.Sign(target - m_fadeT);
            }
        }

        public override float GetDuckVol()
        {
            return Mathf.Clamp01((1.0f * (1 - m_fadeT)) + (m_duckingMixAmount * m_fadeT));
        }

        public override string GetGroupName()
        {
            return m_mixBusToMonitor.name;
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