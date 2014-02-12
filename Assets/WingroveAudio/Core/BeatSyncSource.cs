using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WingroveAudio
{
    [RequireComponent(typeof(BaseWingroveAudioSource))]
    public class BeatSyncSource : MonoBehaviour
    {
        [SerializeField]
        private float m_bpm = 120;

        BaseWingroveAudioSource m_audioSource;

        static List<BeatSyncSource> m_beatSyncs = new List<BeatSyncSource>();

        // Use this for initialization
        void Start()
        {
            m_audioSource = GetComponent<BaseWingroveAudioSource>();
            m_beatSyncs.Add(this);
        }

        public static BeatSyncSource GetCurrent()
        {
            foreach (BeatSyncSource bss in m_beatSyncs)
            {
                if (bss.IsActive())
                {
                    return bss;
                }
            }
            return null;
        }

        void OnDestroy()
        {
            m_beatSyncs.Remove(this);
        }

        public bool IsActive()
        {
            return m_audioSource.HasActiveCues();
        }

        public double GetNextBeatTime()
        {
            float currentTime = m_audioSource.GetCurrentTime();
            double currentDSPTime = AudioSettings.dspTime;

            int beatIndex = Mathf.CeilToInt(currentTime / (60.0f / m_bpm));
            float nextBeatTime = beatIndex * (60.0f / m_bpm);

            float diff = nextBeatTime - currentTime;
            double dspTime = currentDSPTime + diff;
            return dspTime;
        }

    }

}