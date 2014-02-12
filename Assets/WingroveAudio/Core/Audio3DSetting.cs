using UnityEngine;
using System.Collections;

namespace WingroveAudio
{

    public class Audio3DSetting : ScriptableObject
    {
        [SerializeField]
        private AudioRolloffMode m_rolloffMode = AudioRolloffMode.Linear;
        [SerializeField]
        private float m_maxDistance = 100.0f;
        [SerializeField]
        private float m_minDistance = 1.0f;
        [SerializeField]
        private AnimationCurve m_customRolloffCurve = new AnimationCurve(new Keyframe[] {new Keyframe(0,1.0f), new Keyframe(1,0.0f)});
        [SerializeField]
        private bool m_useMaxToNormalizeCustomCurve = true;

        public float GetMaxDistance()
        {
            return m_maxDistance;
        }
        public float GetMinDistance()
        {
            return m_minDistance;
        }
        public AudioRolloffMode GetRolloffMode()
        {
            return m_rolloffMode;
        }
        public float EvaluateCustom(float distance)
        {
            return m_customRolloffCurve.Evaluate(m_useMaxToNormalizeCustomCurve ? distance / m_maxDistance : distance);
        }
    }

}