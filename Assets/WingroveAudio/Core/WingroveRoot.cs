using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WingroveAudio
{
    [AddComponentMenu("WingroveAudio/Wingrove Root")]
    public class WingroveRoot : MonoBehaviour
    {

        static WingroveRoot s_instance;
        [SerializeField]
        private bool m_useDecibelScale = true;
        [SerializeField]
        private bool m_allowMultipleListeners = false;

        public enum MultipleListenerPositioningModel
        {
            InverseSquareDistanceWeighted
        }

        [SerializeField]
        private AudioRolloffMode m_defaultRolloffMode = AudioRolloffMode.Linear;
        [SerializeField]
        private float m_defaultMaxDistance = 100.0f;
        [SerializeField]
        private float m_defaultMinDistance = 1.0f;

        [SerializeField]
        private MultipleListenerPositioningModel m_listeningModel = MultipleListenerPositioningModel.InverseSquareDistanceWeighted;

        private GUISkin m_editorSkin;

        public static WingroveRoot Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = (WingroveRoot)GameObject.FindObjectOfType(typeof(WingroveRoot));
                }
                return s_instance;
            }
        }

        [SerializeField]
        private int m_audioSourcePoolSize = 32;
        [SerializeField]
        private int m_calculateRMSIntervalFrames = 1;
        [SerializeField]
        private bool m_useDSPTime = true;

        private int m_rmsFrame;
        private static double m_lastDSPTime;
        private static double m_dspDeltaTime;

        public class AudioSourcePoolItem
        {
            public AudioSource m_audioSource;
            public ActiveCue m_user;
            public int m_index;
        }
        List<AudioSourcePoolItem> m_audioSourcePool = new List<AudioSourcePoolItem>();

        Dictionary<string, List<BaseEventReceiveAction>> m_eventReceivers = new Dictionary<string, List<BaseEventReceiveAction>>();

        [SerializeField]
        public EventGroup[] m_eventGroups;

        private List<WingroveListener> m_listeners = new List<WingroveListener>();
		
		
		class ParameterValues
		{
			public Dictionary<string, float> m_parameterValues = new Dictionary<string, float>();
		}
		ParameterValues m_globalValues = new ParameterValues();
		Dictionary<GameObject, ParameterValues> m_objectValues = new Dictionary<GameObject, ParameterValues>();

#if UNITY_EDITOR
        public class LoggedEvent
        {
            public string m_eventName = null;
            public GameObject m_linkedObject = null;
            public double m_time;
        }

        private List<LoggedEvent> m_loggedEvents = new List<LoggedEvent>();
        private int m_maxEvents = 50;
        private double m_startTime;


        public void LogEvent(string eventName, GameObject linkedObject)
        {
            LoggedEvent lge = new LoggedEvent();
            lge.m_eventName = eventName;
            lge.m_linkedObject = linkedObject;
            lge.m_time = AudioSettings.dspTime - m_startTime;
            m_loggedEvents.Add(lge);
            if (m_loggedEvents.Count > m_maxEvents)
            {
                m_loggedEvents.RemoveAt(0);
            }
        }

        public List<LoggedEvent> GetLogList()
        {
            return m_loggedEvents;
        }
#endif

        // Use this for initialization
        void Awake()
        {
            s_instance = this;
            GameObject pool = new GameObject("AudioSourcePool");
            pool.transform.parent = transform;
            for (int i = 0; i < m_audioSourcePoolSize; ++i)
            {
                GameObject newAudioSource = new GameObject("PooledAudioSource");
                newAudioSource.transform.parent = pool.transform;

                AudioSource aSource = newAudioSource.AddComponent<AudioSource>();

                AudioSourcePoolItem aspi = new AudioSourcePoolItem();
                aspi.m_audioSource = aSource;
                aSource.enabled = false;
                aSource.rolloffMode = m_defaultRolloffMode;
                aSource.maxDistance = m_defaultMaxDistance;
                aSource.minDistance = m_defaultMinDistance;
                aspi.m_index = i;
                m_audioSourcePool.Add(aspi);
            }

            BaseEventReceiveAction[] evrs = GetComponentsInChildren<BaseEventReceiveAction>();
            foreach (BaseEventReceiveAction evr in evrs)
            {
                string[] events = evr.GetEvents();
                if (events != null)
                {
                    foreach (string ev in events)
                    {
                        if (!m_eventReceivers.ContainsKey(ev))
                        {
                            m_eventReceivers[ev] = new List<BaseEventReceiveAction>();
                        }
                        m_eventReceivers[ev].Add(evr);
                    }
                }
            }

            gameObject.AddComponent<AudioListener>();

            transform.position = Vector3.zero;
            m_lastDSPTime = AudioSettings.dspTime;
#if UNITY_EDITOR
            m_startTime = AudioSettings.dspTime;
#endif
        }

        public void SetDefault3DSettings(AudioSource aSource)
        {
            aSource.rolloffMode = m_defaultRolloffMode;
            aSource.maxDistance = m_defaultMaxDistance;
            aSource.minDistance = m_defaultMinDistance;
        }
		
		public float GetParameterForGameObject(string parameter, GameObject go)
		{
			float result = 0.0f;
			if ( go == null )
			{
				m_globalValues.m_parameterValues.TryGetValue(parameter, out result);
			}
			else
			{
				ParameterValues pv = null;
				if ( m_objectValues.TryGetValue(go, out pv) )
				{
					if ( !(pv.m_parameterValues.TryGetValue(parameter, out result)) )
					{
						m_globalValues.m_parameterValues.TryGetValue(parameter, out result);
					}
				}
				else
				{
					m_globalValues.m_parameterValues.TryGetValue(parameter, out result);					
				}
			}
			return result;
		}
		
		public void SetParameterGlobal(string parameter, float setValue)
		{
			m_globalValues.m_parameterValues[parameter] = setValue;
		}
		
		public void SetParameterForObject(string parameter, GameObject go, float setValue)
		{
			ParameterValues pv = null;
			if (!m_objectValues.TryGetValue(go, out pv))
			{
				pv = m_objectValues[go] = new ParameterValues();
			}
			pv.m_parameterValues[parameter] = setValue;
		}

        public bool UseDBScale
        {
            get { return m_useDecibelScale; }
            set { m_useDecibelScale = value; }
        }

        //StoredImportSettings m_storedImportSettings;

        //public bool Is3D(string path)
        //{
        //    if (m_storedImportSettings == null)
        //    {
        //        m_storedImportSettings = (StoredImportSettings)Resources.Load("ImportSettings");
        //    }
        //    return m_storedImportSettings.Is3D(path);
        //}

        public int FindEvent(string eventName)
        {
            int index = 0;
            foreach (EventGroup eg in m_eventGroups)
            {
                if (eg != null && eg.m_events != null)
                {
                    foreach (string st in eg.m_events)
                    {
                        if (st == eventName)
                        {
                            return index;
                        }
                    }
                }
                ++index;
            }
            return -1;
        }

        public GUISkin GetSkin()
        {
            if (m_editorSkin == null)
            {
                m_editorSkin = (GUISkin)Resources.Load("WingroveAudioSkin");
            }
            return m_editorSkin;
        }

        public void PostEvent(string eventName)
        {
            PostEventCL(eventName, (List<ActiveCue>)null, null);
        }

        public void PostEventCL(string eventName, List<ActiveCue> cuesIn)
        {
            PostEventCL(eventName, cuesIn, null);
        }

        public void PostEventGO(string eventName, GameObject targetObject)
        {            
            PostEventGO(eventName, targetObject, null);
        }

        public void PostEventGO(string eventName, GameObject targetObject, List<ActiveCue> cuesOut)
        {
#if UNITY_EDITOR
            LogEvent(eventName, targetObject);
#endif
            List<BaseEventReceiveAction> listOfReceivers = null;
            if (m_eventReceivers.TryGetValue(eventName, out listOfReceivers))
            {
                foreach (BaseEventReceiveAction evr in listOfReceivers)
                {
                    evr.PerformAction(eventName, targetObject, cuesOut);
                }
            }
        }

        public void PostEventCL(string eventName, List<ActiveCue> cuesIn, List<ActiveCue> cuesOut)
        {
#if UNITY_EDITOR            
            LogEvent(eventName, null);
#endif
            List<BaseEventReceiveAction> listOfReceivers = null;
            if (m_eventReceivers.TryGetValue(eventName, out listOfReceivers))
            {
                foreach (BaseEventReceiveAction evr in listOfReceivers)
                {
                    evr.PerformAction(eventName, cuesIn, cuesOut);
                }
            }
        }

        public AudioSourcePoolItem TryClaimPoolSource(ActiveCue cue)
        {
            AudioSourcePoolItem bestSteal = null;
            int lowestImportance = cue.GetImportance();
            float quietestSimilarImportance = 1.0f;
            foreach (AudioSourcePoolItem aspi in m_audioSourcePool)
            {
                if (aspi.m_user == null)
                {
                    aspi.m_user = cue;
                    return aspi;
                }
                else
                {
                    if (aspi.m_user.GetImportance() < cue.GetImportance())
                    {
                        if (aspi.m_user.GetImportance() < lowestImportance)
                        {
                            lowestImportance = aspi.m_user.GetImportance();
                            bestSteal = aspi;
                        }
                    }
                    else if (aspi.m_user.GetImportance() == lowestImportance)
                    {
                        if (aspi.m_user.GetTheoreticalVolume() < quietestSimilarImportance)
                        {
                            quietestSimilarImportance = aspi.m_user.GetTheoreticalVolume();
                            bestSteal = aspi;
                        }
                    }
                }
            }
            if (bestSteal != null)
            {
                bestSteal.m_user.Virtualise();
                bestSteal.m_user = cue;
                return bestSteal;
            }
            return null;
        }

        public void UnlinkSource(AudioSourcePoolItem item)
        {
            //item.m_audioSource.Stop();
            //item.m_audioSource.enabled = false;
            //item.m_audioSource.clip = null;
            // flush test
            GameObject go = item.m_audioSource.gameObject;
            Destroy(item.m_audioSource);
            item.m_audioSource = go.AddComponent<AudioSource>();
            item.m_audioSource.enabled = false;
            item.m_audioSource.rolloffMode = m_defaultRolloffMode;
            item.m_audioSource.maxDistance = m_defaultMaxDistance;
            item.m_audioSource.minDistance = m_defaultMinDistance;
            item.m_user = null;
        }

        public string dbStringUtil(float amt)
        {
            string result = "";
            float dbMix = 20 * Mathf.Log10(amt);
            if (dbMix == 0)
            {
                result = "-0.00 dB";
            }
            else if (float.IsInfinity(dbMix))
            {
                result = "-inf dB";
            }
            else
            {
                result = System.String.Format("{0:0.00}", dbMix) + " dB";
            }
            return result;
        }

        public bool ShouldCalculateMS(int index)
        {
            if (m_calculateRMSIntervalFrames <= 1)
            {
                return true;
            }
            if ((index % m_calculateRMSIntervalFrames)
                == m_rmsFrame)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void Update()
        {
            m_rmsFrame++;
            if (m_rmsFrame >= m_calculateRMSIntervalFrames)
            {
                m_rmsFrame = 0;
            }

            if (m_useDSPTime)
            {
                m_dspDeltaTime = AudioSettings.dspTime - m_lastDSPTime;
            }
            else
            {
                m_dspDeltaTime = Time.deltaTime;
            }
            m_lastDSPTime = AudioSettings.dspTime;
        }

        public static float GetDeltaTime()
        {
            return (float)m_dspDeltaTime;
        }

        public void RegisterListener(WingroveListener listener)
        {
            m_listeners.Add(listener);
        }

        public void UnregisterListener(WingroveListener listener)
        {
            m_listeners.Remove(listener);
        }

        public Vector3 GetRelativeListeningPosition(Vector3 inPosition)
        {
            int listenerCount = m_listeners.Count;
            if (!m_allowMultipleListeners || listenerCount <= 1)
            {
                if (listenerCount == 0)
                {
                    return inPosition;
                }
                return m_listeners[0].transform.worldToLocalMatrix * new Vector4(inPosition.x, inPosition.y, inPosition.z, 1.0f);
                //return inPosition - m_listeners[0].transform.position;
            }
            else
            {
                if (m_listeningModel == MultipleListenerPositioningModel.InverseSquareDistanceWeighted)
                {
                    float totalWeight = 0;
                    Vector3 totalPosition = Vector3.zero;
                    foreach (WingroveListener listener in m_listeners)
                    {
                        Vector3 dist = inPosition - listener.transform.position;
                        if (dist.magnitude == 0)
                        {
                            // early out if one is right here
                            return Vector3.zero;
                        }
                        else
                        {
                            float weight = 1 / (dist.magnitude * dist.magnitude);
                            totalWeight += weight;
                            totalPosition += (Vector3)(listener.transform.worldToLocalMatrix * new Vector4(inPosition.x, inPosition.y, inPosition.z, 1.0f)) * weight;
                        }
                    }
                    totalPosition /= totalWeight;
                    return totalPosition;
                }
                return Vector3.zero;
            }
        }
    }

}