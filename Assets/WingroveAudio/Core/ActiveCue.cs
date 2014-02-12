using UnityEngine;
using System.Collections;

namespace WingroveAudio
{

    public class ActiveCue : MonoBehaviour
    {
        public enum CueState
        {
            Initial,
            PlayingFadeIn,
            Playing,
            PlayingFadeOut
        }

        private GameObject m_originatorSource;
        private BaseWingroveAudioSource m_audioClipSource;
        private GameObject m_targetGameObject;
        private bool m_hasTarget;
        private double m_dspStartTime = 0.0f;
        private bool m_hasDSPStartTime = false;

        public void Initialise(GameObject originator, GameObject target)
        {
            m_originatorSource = originator;
            m_targetGameObject = target;
            m_hasTarget = (target != null);
            m_audioClipSource = m_originatorSource.GetComponent<BaseWingroveAudioSource>();
            m_pitch = m_audioClipSource.GetNewPitch();
            m_audioClipSource.AddUsage();
            transform.parent = m_originatorSource.transform;
            Update();            
        }

        void OnDestroy()
        {
            if (m_currentAudioSource != null)
            {
                WingroveRoot.Instance.UnlinkSource(m_currentAudioSource);
                m_currentAudioSource = null;
            }
            m_audioClipSource.RemoveUsage();
        }

        public GameObject GetOriginatorSource()
        {
            return m_originatorSource;
        }

        public GameObject GetTargetObject()
        {
            return m_targetGameObject;
        }

        public int GetImportance()
        {
            return m_audioClipSource.GetImportance();
        }

        public float m_fadeT;
        public float m_fadeSpeed;
        CueState m_currentState;

        bool m_isPaused;
        int m_currentPosition;

        float m_pitch;

        public WingroveRoot.AudioSourcePoolItem m_currentAudioSource;

        float[] m_bufferDataL = new float[512];
        float[] m_bufferDataR = new float[512];

        private float m_rms;

        private bool m_rmsRequested;
        private bool m_hasStartedEver = false;

        void Update()
        {
            bool queueEnableAndPlay = false;
            if (m_currentAudioSource == null)
            {
                // don't bother stealing if we're going to be silent anyway...                
                if (GetTheoreticalVolume() > 0)
                {
                    m_currentAudioSource = WingroveRoot.Instance.TryClaimPoolSource(this);
                }
                if (m_currentAudioSource != null)
                {
                    m_currentAudioSource.m_audioSource.clip = m_audioClipSource.GetAudioClip();
                    if (!m_isPaused)
                    {
                        m_currentAudioSource.m_audioSource.loop = m_audioClipSource.GetLooping();
                        queueEnableAndPlay = true;
                    }
                }
                else
                {
                    if (!m_isPaused)
                    {
                        if (m_hasStartedEver)
                        {
                            m_currentPosition += (int)(WingroveRoot.GetDeltaTime() * m_audioClipSource.GetAudioClip().frequency * GetMixPitch());
                        }
                        if (m_currentPosition > m_audioClipSource.GetAudioClip().samples)
                        {
                            if (m_audioClipSource.GetLooping())
                            {
                                m_currentPosition -= m_audioClipSource.GetAudioClip().samples;
                            }
                            else
                            {
                                StopInternal();
                            }
                        }
                    }
                }
            }
            else
            {
                if (!m_isPaused)
                {
                    m_currentPosition = m_currentAudioSource.m_audioSource.timeSamples;
                }
            }

            if (!m_isPaused)
            {
                switch (m_currentState)
                {
                    case CueState.Initial:
                        break;
                    case CueState.Playing:
                        m_fadeT = 1;
                        break;
                    case CueState.PlayingFadeIn:
                        m_fadeT += m_fadeSpeed * WingroveRoot.GetDeltaTime();
                        if (m_fadeT >= 1)
                        {
                            m_fadeT = 1.0f;
                            m_currentState = CueState.Playing;
                        }
                        break;
                    case CueState.PlayingFadeOut:
                        m_fadeT -= m_fadeSpeed * WingroveRoot.GetDeltaTime();
                        if (m_fadeT <= 0)
                        {
                            m_fadeT = 0.0f;
                            StopInternal();
                            // early return!!!!
                            return;
                        }
                        break;
                }

                if (!m_audioClipSource.GetLooping())
                {
                    if (m_currentPosition > m_audioClipSource.GetAudioClip().samples - 1000)
                    {
                        StopInternal();
                        return;
                    }
                }
            }

            SetMix();

            if (queueEnableAndPlay)
            {
                if (m_currentAudioSource != null)
                {
                    m_currentAudioSource.m_audioSource.enabled = true;
                    m_currentAudioSource.m_audioSource.timeSamples = m_currentPosition;
                    Audio3DSetting settings = m_audioClipSource.Get3DSettings();
                    if (settings == null)
                    {
                        WingroveRoot.Instance.SetDefault3DSettings(m_currentAudioSource.m_audioSource);
                    }
                    else
                    {
                        AudioRolloffMode rolloffMode = settings.GetRolloffMode();
                        if ( rolloffMode != AudioRolloffMode.Custom )
                        {
                            m_currentAudioSource.m_audioSource.rolloffMode = rolloffMode;
                            m_currentAudioSource.m_audioSource.minDistance = settings.GetMinDistance();
                            m_currentAudioSource.m_audioSource.maxDistance = settings.GetMaxDistance();
                        }
                        else
                        {
                            m_currentAudioSource.m_audioSource.rolloffMode = AudioRolloffMode.Linear;
                            m_currentAudioSource.m_audioSource.minDistance = float.MaxValue;
                            m_currentAudioSource.m_audioSource.maxDistance = float.MaxValue;                        

                        }
                    }
                    if ((m_hasDSPStartTime) && (m_dspStartTime > AudioSettings.dspTime))
                    {
                        m_currentAudioSource.m_audioSource.timeSamples = m_currentPosition = 0;
                        m_currentAudioSource.m_audioSource.PlayScheduled(m_dspStartTime);
                    }
                    else
                    {
                        m_currentAudioSource.m_audioSource.Play();
                    }
                }
            }

            m_hasStartedEver = true;
        }

        public float GetTheoreticalVolume()
        {
            if (m_isPaused)
            {
                return 0;
            }
            else
            {
                float v3D = 1.0f;
                return m_fadeT * m_audioClipSource.GetMixBusLevel() * v3D;
            }
        }
		
		public float GetMixPitch()
		{
			return m_pitch * m_audioClipSource.GetPitchModifier(m_targetGameObject);
		}

        public float ApplyCustom3DCurve(float distance)
        {
            Audio3DSetting settings = m_audioClipSource.Get3DSettings();
            if (settings != null)
            {
                if (settings.GetRolloffMode() == AudioRolloffMode.Custom)
                {
                    return settings.EvaluateCustom(distance);
                }
            }
            return 1.0f;
        }

        public void SetMix()
        {
            if (m_currentAudioSource != null)
            {

                if (m_audioClipSource.GetPositioningType() == BaseWingroveAudioSource.PositioningType.UnityDefault3d2d)
                {
                    if (m_targetGameObject != null)
                    {
                        m_currentAudioSource.m_audioSource.transform.position = 
                            WingroveRoot.Instance.GetRelativeListeningPosition(m_targetGameObject.transform.position);
                    }
                    else
                    {
                        // don't snap back to origin when object destroyed
                        if (!m_hasTarget)
                        {
                            m_currentAudioSource.m_audioSource.transform.position = Vector3.zero;
                        }
                    }
                }
                else
                {
                    m_currentAudioSource.m_audioSource.transform.position = Vector3.zero;
                }
                // apply the full mix, including custom rolloff
                m_currentAudioSource.m_audioSource.volume = m_fadeT * m_audioClipSource.GetMixBusLevel()
                        * m_audioClipSource.GetVolumeModifier(m_targetGameObject) * ApplyCustom3DCurve(
                        m_currentAudioSource.m_audioSource.transform.position.magnitude);
                m_currentAudioSource.m_audioSource.pitch = GetMixPitch();

                if (WingroveRoot.Instance.ShouldCalculateMS(m_currentAudioSource.m_index))
                {
                    if (m_rmsRequested)
                    {
                        float rms = 0;
                        if (m_audioClipSource.GetAudioClip().channels == 2)
                        {
                            m_currentAudioSource.m_audioSource.GetOutputData(m_bufferDataL, 0);
                            m_currentAudioSource.m_audioSource.GetOutputData(m_bufferDataR, 1);
                            for (int index = 0; index < 512; ++index)
                            {
                                rms += (Mathf.Abs(m_bufferDataR[index]) + Mathf.Abs(m_bufferDataL[index]))
                                    * (Mathf.Abs(m_bufferDataR[index]) + Mathf.Abs(m_bufferDataL[index]));
                            }
                        }
                        else
                        {
                            m_currentAudioSource.m_audioSource.GetOutputData(m_bufferDataL, 0);
                            for (int index = 0; index < 512; ++index)
                            {
                                rms += (m_bufferDataL[index] * m_bufferDataL[index]);
                            }
                        }
                        rms = Mathf.Sqrt(Mathf.Clamp01(rms / 512.0f));
                        m_rms = Mathf.Max(rms * m_fadeT * m_audioClipSource.GetMixBusLevel(),
                            m_rms * 0.9f);// ((m_rms * 2 + rms) / 3.0f) * m_fadeT * m_audioClipSource.GetMixBusLevel();
                    }
                    m_rmsRequested = false;
                }                
            }
            else
            {
                m_rms = 0;
            }
        }

        public float GetRMS()
        {
            m_rmsRequested = true;
            return m_rms;
        }

        public void Play(float fade)
        {
            m_currentPosition = 0;
            if (m_currentAudioSource != null)
            {
                m_currentAudioSource.m_audioSource.timeSamples = 0;
            }
            if (fade == 0.0f)
            {
                m_currentState = CueState.Playing;
            }
            else
            {
                m_currentState = CueState.PlayingFadeIn;
                m_fadeSpeed = 1.0f / fade;
            }
        }

        public void Play(float fade, double dspStartTime)
        {
            m_currentPosition = 0;
            m_hasDSPStartTime = true;
            m_dspStartTime = dspStartTime;
            if (m_currentAudioSource != null)
            {
                m_currentAudioSource.m_audioSource.timeSamples = 0;
            }
            if (fade == 0.0f)
            {
                m_currentState = CueState.Playing;
            }
            else
            {
                m_currentState = CueState.PlayingFadeIn;
                m_fadeSpeed = 1.0f / fade;
            }
        }

        public float GetTime()
        {
            float currentTime =
    (m_currentPosition) /
    (float)(m_audioClipSource.GetAudioClip().frequency * GetMixPitch());

            return currentTime;
        }

        public float GetTimeUntilFinished(WingroveGroupInformation.HandleRepeatingAudio handleRepeat)
        {
            float timeRemaining =
                (m_audioClipSource.GetAudioClip().samples - m_currentPosition) /
                (float)(m_audioClipSource.GetAudioClip().frequency * GetMixPitch());
            if (m_audioClipSource.GetLooping())
            {
                switch(handleRepeat)
                {
                    case WingroveGroupInformation.HandleRepeatingAudio.IgnoreRepeatingAudio:
                        timeRemaining = 0.0f;
                        break;
                    case WingroveGroupInformation.HandleRepeatingAudio.ReturnFloatMax:
                    case WingroveGroupInformation.HandleRepeatingAudio.ReturnNegativeOne:
                        timeRemaining = float.MaxValue;
                        break;
                    case WingroveGroupInformation.HandleRepeatingAudio.GiveTimeUntilLoop:
                    default:
                        break;
                }
            }
            if (m_currentState == CueState.PlayingFadeOut)
            {
                if (m_fadeSpeed != 0)
                {
                    timeRemaining = Mathf.Min(m_fadeT / m_fadeSpeed, timeRemaining);
                }
            }
            return timeRemaining;
        }

        public void Stop(float fade)
        {
            if (fade == 0.0f)
            {
                StopInternal();
            }
            else
            {
                m_currentState = CueState.PlayingFadeOut;
                m_fadeSpeed = 1.0f / fade;
            }
        }

        void StopInternal()
        {
            Unlink();
        }

        public void Unlink()
        {
            if (m_currentAudioSource != null)
            {
                WingroveRoot.Instance.UnlinkSource(m_currentAudioSource);
                m_currentAudioSource = null;
            }
            GameObject.Destroy(gameObject);
        }

        public void Virtualise()
        {
            if (m_currentAudioSource != null)
            {
                WingroveRoot.Instance.UnlinkSource(m_currentAudioSource);
                m_currentAudioSource = null;
            }
        }

        public void Pause()
        {
            if (!m_isPaused)
            {
                if (m_currentAudioSource != null)
                {
                    m_currentAudioSource.m_audioSource.Pause();
                }
            }
            m_isPaused = true;

        }

        public void Unpause()
        {
            if (m_isPaused)
            {
                if (m_currentAudioSource != null)
                {
                    m_currentAudioSource.m_audioSource.Play();
                }
            }
            m_isPaused = false;

        }

        public CueState GetState()
        {
            return m_currentState;
        }
    }

}