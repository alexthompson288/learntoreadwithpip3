using UnityEngine;
using System.Collections;

namespace WingroveAudio
{
    [AddComponentMenu("WingroveAudio/Wingrove Group Information")]
    public class WingroveGroupInformation : MonoBehaviour
    {

        private BaseWingroveAudioSource[] m_audioSources;

        public enum HandleRepeatingAudio
        {
            IgnoreRepeatingAudio,
            GiveTimeUntilLoop,
            ReturnFloatMax,
            ReturnNegativeOne
        };
        [SerializeField]
        private HandleRepeatingAudio m_handleRepeatedAudio = HandleRepeatingAudio.ReturnFloatMax;

        // Use this for initialization
        void Awake()
        {
            m_audioSources = GetComponentsInChildren<BaseWingroveAudioSource>();
        }

        public float GetTimeUntilFinished()
        {
            float maxTime = 0.0f;
            if (m_audioSources != null)
            {
                foreach (BaseWingroveAudioSource bwas in m_audioSources)
                {
                    maxTime = Mathf.Max(bwas.GetTimeUntilFinished(m_handleRepeatedAudio), maxTime);
                }
            }
            if (maxTime == float.MaxValue)
            {
                if (m_handleRepeatedAudio == HandleRepeatingAudio.ReturnNegativeOne)
                {
                    maxTime = -1;
                }
            }

            return maxTime;
        }

        public bool IsAnyPlaying()
        {
            if (m_audioSources != null)
            {
                foreach (BaseWingroveAudioSource bwas in m_audioSources)
                {
                    if (bwas.IsPlaying())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

}