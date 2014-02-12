using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace WingroveAudio
{
    [AddComponentMenu("WingroveAudio/Mix Bus")]
    public class WingroveMixBus : MonoBehaviour
    {
        [SerializeField]
        private float m_volumeMult = 1.0f;
        [SerializeField]
        private int m_importance = 0;

        private float m_mixedVol = 1.0f;
        private WingroveMixBus m_parent;
        private int m_totalImportance;

        private List<BaseAutomaticDuck> m_duckList = new List<BaseAutomaticDuck>();
        private List<BaseWingroveAudioSource> m_audioSources = new List<BaseWingroveAudioSource>();
        private List<WingroveMixBus> m_childMixBuses = new List<WingroveMixBus>();

        public float Volume
        {
            set { m_volumeMult = value; }
            get { return m_volumeMult; }
        }

        void Awake()
        {
            m_parent = FindParentMixBus(transform.parent);
            if (m_parent != null)
            {
                m_parent.RegisterMixBus(this);
            }
        }

        void OnDestroy()
        {
            if (m_parent != null)
            {
                m_parent.UnregisterMixBus(this);
            }
        }

        public void RegisterMixBus(WingroveMixBus mixBus)
        {
            m_childMixBuses.Add(mixBus);
        }

        public void UnregisterMixBus(WingroveMixBus mixBus)
        {
            m_childMixBuses.Remove(mixBus);
        }

        public void AddDuck(BaseAutomaticDuck duck)
        {
            m_duckList.Add(duck);
        }

        public void RegisterSource(BaseWingroveAudioSource source)
        {
            m_audioSources.Add(source);
        }

        public void RemoveSource(BaseWingroveAudioSource source)
        {
            m_audioSources.Remove(source);
        }

        public float GetRMS()
        {
            float tSqr = 0;
            foreach (BaseWingroveAudioSource was in m_audioSources)
            {
                float rms = was.GetRMS();
                tSqr += (rms * rms);
            }
            foreach (WingroveMixBus mb in m_childMixBuses)
            {
                float rms = mb.GetRMS();
                tSqr += (rms * rms);
            }
            return Mathf.Sqrt(tSqr);
        }

        public static WingroveMixBus FindParentMixBus(Transform t)
        {
            if (t == null)
            {
                return null;
            }
            else
            {
                WingroveMixBus mixBus = t.GetComponent<WingroveMixBus>();
                if (mixBus == null)
                {
                    return FindParentMixBus(t.parent);
                }
                else
                {
                    return mixBus;
                }
            }
        }

        public static InstanceLimiter FindParentLimiter(Transform t)
        {
            if (t == null)
            {
                return null;
            }
            else
            {
                InstanceLimiter instanceLimiter = t.GetComponent<InstanceLimiter>();
                if (instanceLimiter == null)
                {
                    return FindParentLimiter(t.parent);
                }
                else
                {
                    return instanceLimiter;
                }
            }
        }

        void Update()
        {
            if (m_parent == null)
            {
                m_mixedVol = m_volumeMult;
                m_totalImportance = m_importance;
            }
            else
            {
                m_mixedVol = m_parent.GetMixedVol() * m_volumeMult;
                m_totalImportance = m_parent.GetImportance() + m_totalImportance;
            }

            foreach (BaseAutomaticDuck duck in m_duckList)
            {
                m_mixedVol *= duck.GetDuckVol();
            }

        }

        public List<BaseAutomaticDuck> GetDuckList()
        {
            return m_duckList;
        }

        public float GetMixedVol()
        {
            return m_mixedVol;
        }

        public int GetImportance()
        {
            return m_totalImportance;
        }


    }

}