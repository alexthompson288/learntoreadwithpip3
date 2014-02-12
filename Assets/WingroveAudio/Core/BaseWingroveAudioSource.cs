using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace WingroveAudio
{
    public abstract class BaseWingroveAudioSource : MonoBehaviour
    {
        [SerializeField]
        private bool m_looping = false;
        [SerializeField]
        private int m_importance = 0;
        [SerializeField]
        private float m_clipMixVolume = 1.0f;
        [SerializeField]
        private bool m_beatSynchronizeOnStart = false;

        public enum PositioningType
        {
            UnityDefault3d2d,
            AlwaysPositionAtListener
        }

        public enum RetriggerOnSameObject
        {
            PlayAnother,
            DontPlay,
            Restart
        }

        [SerializeField]
        private PositioningType m_positioningType = PositioningType.UnityDefault3d2d;
        [SerializeField]
        private Audio3DSetting m_specify3DSettings = null;

        [SerializeField]
        private float m_randomVariationPitchMin = 1.0f;
        [SerializeField]
        private float m_randomVariationPitchMax = 1.0f;
        [SerializeField]
        private RetriggerOnSameObject m_retriggerOnSameObjectBehaviour = RetriggerOnSameObject.PlayAnother;
         
        protected List<ActiveCue> m_currentActiveCues = new List<ActiveCue>();
        protected WingroveMixBus m_mixBus;
        protected InstanceLimiter m_instanceLimiter;
		protected List<ParameterModifier> m_parameterModifiers = new List<ParameterModifier>();


        public PositioningType GetPositioningType()
        {
            return m_positioningType;
        }

        void Awake()
        {
            m_mixBus = WingroveMixBus.FindParentMixBus(transform);
            m_instanceLimiter = WingroveMixBus.FindParentLimiter(transform);
            if (m_mixBus != null)
            {
                m_mixBus.RegisterSource(this);
            }
			FindParameterModifiers(transform);
            Initialise();
        }
		
		void FindParameterModifiers(Transform t)
		{			
            if (t == null)
            {
                return;
            }
            else
            {
                ParameterModifier[] paramMods = t.gameObject.GetComponents<ParameterModifier>();
				foreach(ParameterModifier mod in paramMods)
				{
					m_parameterModifiers.Add(mod);
				}				
                FindParameterModifiers(t.parent);
			}
		}
		
		public float GetPitchModifier(GameObject go)
		{
			float pMod = 1.0f;
			foreach(ParameterModifier pvMod in m_parameterModifiers)
			{
				pMod *= pvMod.GetPitchMultiplier(go);
			}			
			return pMod;
		}
		
		public float GetVolumeModifier(GameObject go)
		{
			float vMod = m_clipMixVolume;
			foreach(ParameterModifier pvMod in m_parameterModifiers)
			{
				vMod *= pvMod.GetVolumeMultiplier(go);
			}			
			return vMod;
		}


        public bool IsPlaying()
        {
            return (m_currentActiveCues.Count > 0);
        }

        public float GetCurrentTime()
        {
            float result = 0.0f;
            foreach (ActiveCue cue in m_currentActiveCues)
            {
                result = Mathf.Max(cue.GetTime(), result);
            }
            return result;
        }

        public float GetTimeUntilFinished(WingroveGroupInformation.HandleRepeatingAudio handleRepeats)
        {
            float result = 0.0f;
            foreach (ActiveCue cue in m_currentActiveCues)
            {
                result = Mathf.Max(cue.GetTimeUntilFinished(handleRepeats), result);
            }
            return result;
        }

        void Update()
        {
            UpdateInternal();
        }

        protected void UpdateInternal()
        {
            while (m_currentActiveCues.Contains(null))
            {
                m_currentActiveCues.Remove(null);
            }
        }

        void OnDestroy()
        {
            if (m_mixBus != null)
            {
                m_mixBus.RemoveSource(this);
            }
        }

        public float GetClipMixVolume()
        {
            return m_clipMixVolume;
        }

        public int GetImportance()
        {
            if (m_mixBus == null)
            {
                return m_importance;
            }
            else
            {
                return m_importance + m_mixBus.GetImportance();
            }
        }

        public bool HasActiveCues()
        {
            return (m_currentActiveCues.Count != 0);
        }

        public ActiveCue GetCueForGameObject(GameObject go)
        {
            foreach (ActiveCue cue in m_currentActiveCues)
            {
                if (cue.GetTargetObject() == go)
                {
                    return cue;
                }
            }
            return null;
        }

        public float GetRMS()
        {
            float tSqr = 0;
            foreach (ActiveCue cue in m_currentActiveCues)
            {
                float rms = cue.GetRMS();
                tSqr += (rms * rms);
            }

            return Mathf.Sqrt(tSqr);
        }

        public float GetMixBusLevel()
        {
            if (m_mixBus)
            {
                return m_mixBus.GetMixedVol();
            }
            else
            {
                return 1.0f;
            }
        }


        public float GetNewPitch()
        {
            return Random.Range(m_randomVariationPitchMin,
                m_randomVariationPitchMax);
        }

        public bool GetLooping()
        {
            return m_looping;
        }

        public ActiveCue Stop(ActiveCue cue, float fade)
        {
            if (cue == null)
            {
                foreach (ActiveCue fCue in m_currentActiveCues)
                {
                    fCue.Stop(fade);
                }
            }
            else
            {
                cue.Stop(fade);
            }
            return cue;
        }

        public ActiveCue Play(ActiveCue cue, float fade, GameObject target)
        {
            if (m_instanceLimiter == null || m_instanceLimiter.CanPlay(target))
            {
                if ((cue == null)||(m_retriggerOnSameObjectBehaviour == RetriggerOnSameObject.PlayAnother))
                {                    
                    GameObject newCue = new GameObject("Cue");
                    cue = newCue.AddComponent<ActiveCue>();
                    cue.Initialise(gameObject, target);
                    m_currentActiveCues.Add(cue);
                    if (m_beatSynchronizeOnStart)
                    {
                        BeatSyncSource current = BeatSyncSource.GetCurrent();
                        if ( current != null )
                        {
                            cue.Play(fade, current.GetNextBeatTime());
                        }
                        else
                        {
                            cue.Play(fade);
                        }
                    }
                    else
                    {
                        cue.Play(fade);
                    }
                    if (m_instanceLimiter != null)
                    {
                        m_instanceLimiter.AddCue(cue, target);
                    }
                }
                else
                {
                    if (m_retriggerOnSameObjectBehaviour != RetriggerOnSameObject.DontPlay)
                    {
                        cue.Play(fade);
                    }
                }
                

            }
            return cue;
        }

        public ActiveCue Pause(ActiveCue cue)
        {
            if (cue == null)
            {
                foreach (ActiveCue fCue in m_currentActiveCues)
                {
                    fCue.Pause();
                }
            }
            else
            {
                cue.Pause();
            }
            return cue;
        }

        public Audio3DSetting Get3DSettings()
        {
            return m_specify3DSettings;
        }

        public ActiveCue Unpause(ActiveCue cue)
        {
            if (cue == null)
            {
                foreach (ActiveCue fCue in m_currentActiveCues)
                {
                    fCue.Unpause();
                }
            }
            else
            {
                cue.Unpause();
            }
            return cue;
        }

        public virtual AudioClip GetAudioClip()
        {
            // null implementation
            Debug.LogError("Using null implementation");
            return null;
        }
        public abstract void RemoveUsage();
        public abstract void AddUsage();
        public virtual void Initialise()
        {

        }
    }

}