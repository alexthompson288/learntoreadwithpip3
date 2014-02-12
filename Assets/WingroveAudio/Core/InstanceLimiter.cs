using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace WingroveAudio
{
    [AddComponentMenu("WingroveAudio/Instance Limiter")]
    public class InstanceLimiter : MonoBehaviour
    {
        [SerializeField]
        private int m_instanceLimit = 1;
        public enum LimitMethod
        {
            RemoveOldest,            
            DontCreateNew
        }
        [SerializeField]
        private LimitMethod m_limitMethod = LimitMethod.RemoveOldest;

        [SerializeField]
        private float m_removedSourceFade = 0.0f;
        [SerializeField]
        private bool m_ignoreStopping = true;
        [SerializeField]
        private bool m_perGameObject = false;

        List<ActiveCue> m_activeCues = new List<ActiveCue>();
        Dictionary<GameObject, List<ActiveCue>> m_activeCuesPerObject = new Dictionary<GameObject, List<ActiveCue>>();

        public bool CanPlay(GameObject attachedObject)
        {
            Tidy();
            if (m_limitMethod == LimitMethod.DontCreateNew)
            {
                if ((attachedObject!=null)&&(m_perGameObject))
                {
                    List<ActiveCue> numActive = null;
                    if (m_activeCuesPerObject.TryGetValue(attachedObject, out numActive))
                    {
                        if (numActive.Count >= m_instanceLimit)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (m_activeCues.Count >= m_instanceLimit)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void AddCue(ActiveCue cue, GameObject attachedObject)
        {
            m_activeCues.Add(cue);
            if ((attachedObject != null) && (m_perGameObject))
            {
                if (!m_activeCuesPerObject.ContainsKey(attachedObject))
                {
                    m_activeCuesPerObject[attachedObject] = new List<ActiveCue>();
                }
                m_activeCuesPerObject[attachedObject].Add(cue);
            }
            Limit(attachedObject);
        }

        void Tidy()
        {
            Tidy(m_activeCues);

            if (m_perGameObject)
            {
                List<GameObject> keysToRemove = new List<GameObject>();
                foreach (List<ActiveCue> cueList in m_activeCuesPerObject.Values)
                {
                    Tidy(cueList);
                }
                foreach (KeyValuePair<GameObject, List<ActiveCue>> kvp in m_activeCuesPerObject)
                {
                    if (kvp.Value.Count == 0)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
                foreach (GameObject toRemove in keysToRemove)
                {
                    m_activeCuesPerObject.Remove(toRemove);
                }
            }
        }

        void Tidy(List<ActiveCue> list)
        {
            List<ActiveCue> toRemove = new List<ActiveCue>();
            foreach (ActiveCue c in list)
            {
                if (c == null)
                {
                    toRemove.Add(c);
                }
                else if (m_ignoreStopping)
                {
                    if (c.GetState() == ActiveCue.CueState.PlayingFadeOut)
                    {
                        toRemove.Add(c);
                    }
                }
            }

            foreach (ActiveCue cue in toRemove)
            {
                list.Remove(cue);
            }
        }

        void Limit(GameObject attachedObject)
        {
            Tidy();

            if ((attachedObject != null) && (m_perGameObject))
            {
                if (m_activeCuesPerObject.ContainsKey(attachedObject))
                {
                    if (m_activeCuesPerObject[attachedObject].Count > m_instanceLimit)
                    {
                        if (m_activeCuesPerObject[attachedObject][0] != null)
                        {
                            m_activeCuesPerObject[attachedObject][0].Stop(m_removedSourceFade);
                        }
                        m_activeCuesPerObject[attachedObject].Remove(m_activeCuesPerObject[attachedObject][0]);
                    }                    
                }
            }
            else
            {
                if (m_activeCues.Count > m_instanceLimit)
                {
                    if (m_activeCues[0] != null)
                    {
                        m_activeCues[0].Stop(m_removedSourceFade);
                    }
                    m_activeCues.Remove(m_activeCues[0]);
                }
            }
        }
    }

}