using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace WingroveAudio
{
    [AddComponentMenu("WingroveAudio/Event Receive Action")]
    public class EventReceiveAction : BaseEventReceiveAction
    {

        public enum Actions
        {
            Play,
            PlayRandom,
            PlaySequence,
            Stop,
            Pause,
            UnPause,
            PlayRandomNoRepeats
        }

        [SerializeField]
        private string m_event = "";
        [SerializeField]
        private Actions m_action = Actions.Play;
        [SerializeField]
        private float m_fadeLength = 0.0f;
        [SerializeField]
        private float m_delay = 0.0f;
        [SerializeField]
        private int m_noRepeatsMemory = 1;

        private BaseWingroveAudioSource[] m_audioSources;

        int m_sequenceIndex;
        Queue<int> m_previousRandoms = new Queue<int>();
        List<int> m_availableRandoms = new List<int>();

        void Awake()
        {
            m_audioSources = GetComponentsInChildren<BaseWingroveAudioSource>();
            for (int index = 0; index < m_audioSources.Length; ++index)
            {
                m_availableRandoms.Add(index);
            }
        }

        public override string[] GetEvents()
        {
            return new string[] { m_event };
        }

        IEnumerator PerformDelayedAction(string eventName, GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            PerformActionInternal(eventName, go, null);
        }

        IEnumerator PerformDelayedAction(string eventName, List<ActiveCue> cuesIn, float delay)
        {
            yield return new WaitForSeconds(delay);
            PerformActionInternal(eventName, cuesIn, null);
        }

        public override void PerformAction(string eventName, GameObject targetObject, List<ActiveCue> cuesOut)
        {            
            if (m_delay == 0.0f)
            {                
                PerformActionInternal(eventName, targetObject, cuesOut);
            }
            else
            {
                StartCoroutine(PerformDelayedAction(eventName, targetObject, m_delay));
            }
        }

        public void PerformActionInternal(string eventName, GameObject targetObject, List<ActiveCue> cuesOut)
        {
            int randomIndex = Random.Range(0, m_audioSources.Length);
            bool shouldIncreaseSequence = false;
            int index = 0;

            if (m_action == Actions.PlayRandomNoRepeats)
            {
                int randomP = Random.Range(0, m_availableRandoms.Count);
                randomIndex = m_availableRandoms[randomP];

                m_availableRandoms.Remove(randomIndex);
                m_previousRandoms.Enqueue(randomIndex);

                if (m_previousRandoms.Count > Mathf.Min(m_noRepeatsMemory, m_audioSources.Length-1))
                {
                    int toAdd = m_previousRandoms.Dequeue();
                    m_availableRandoms.Add(toAdd);
                }
            }

            foreach (BaseWingroveAudioSource was in m_audioSources)
            {
                ActiveCue useCue = was.GetCueForGameObject(targetObject);

                switch (m_action)
                {
                    case Actions.Pause:
                        useCue = was.Pause(useCue);
                        break;
                    case Actions.UnPause:
                        useCue = was.Unpause(useCue);
                        break;
                    case Actions.Play:
                        useCue = was.Play(useCue, m_fadeLength, targetObject);
                        break;
                    case Actions.PlayRandom:
                        if (index == randomIndex)
                        {                            
                            useCue = was.Play(useCue, m_fadeLength, targetObject);
                        }
                        break;
                    case Actions.PlaySequence:
                        if (index == m_sequenceIndex)
                        {
                            useCue = was.Play(useCue, m_fadeLength, targetObject);
                            shouldIncreaseSequence = true;
                        }
                        break;
                    case Actions.Stop:
                        useCue = was.Stop(useCue, m_fadeLength);
                        break;
                    case Actions.PlayRandomNoRepeats:
                        if (index == randomIndex)
                        {                            
                            useCue = was.Play(useCue, m_fadeLength, targetObject);
                        }
                        break;
                }

                if (cuesOut != null)
                {
                    cuesOut.Add(useCue);
                }
                ++index;
            }

            if (shouldIncreaseSequence)
            {
                m_sequenceIndex = (m_sequenceIndex + 1) % m_audioSources.Length;
            }

        }

        public override void PerformAction(string eventName, List<ActiveCue> cuesIn, List<ActiveCue> cuesOut)
        {
            if (m_delay == 0.0f)
            {
                PerformActionInternal(eventName, cuesIn, cuesOut);
            }
            else
            {
                StartCoroutine(PerformDelayedAction(eventName, cuesIn, m_delay));
            }
        }


        public void PerformActionInternal(string eventName, List<ActiveCue> cuesIn, List<ActiveCue> cuesOut)
        {
            bool disallowNulls = false;
            foreach (BaseWingroveAudioSource was in m_audioSources)
            {
                if (cuesIn != null)
                {
                    foreach (ActiveCue cue in cuesIn)
                    {
                        if (cue.GetOriginatorSource() == was.gameObject)
                        {
                            disallowNulls = true;
                        }
                    }
                }
            }

            int randomIndex = Random.Range(0, m_audioSources.Length);
            bool shouldIncreaseSequence = false;
            int index = 0;


            if (m_action == Actions.PlayRandomNoRepeats)
            {
                int randomP = Random.Range(0, m_availableRandoms.Count);
                randomIndex = m_availableRandoms[randomP];

                m_availableRandoms.Remove(randomIndex);
                m_previousRandoms.Enqueue(randomIndex);

                if (m_previousRandoms.Count > Mathf.Min(m_noRepeatsMemory, m_audioSources.Length - 1))
                {
                    int toAdd = m_previousRandoms.Dequeue();
                    m_availableRandoms.Add(toAdd);
                }
            }

            foreach (BaseWingroveAudioSource was in m_audioSources)
            {
                ActiveCue useCue = null;
                if (cuesIn != null)
                {
                    foreach (ActiveCue cue in cuesIn)
                    {
                        if (cue.GetOriginatorSource() == was.gameObject)
                        {
                            useCue = cue;
                            break;
                        }
                    }
                }

                if ((!disallowNulls) || (useCue != null))
                {
                    switch (m_action)
                    {
                        case Actions.Pause:
                            useCue = was.Pause(useCue);
                            break;
                        case Actions.UnPause:
                            useCue = was.Unpause(useCue);
                            break;
                        case Actions.Play:
                            useCue = was.Play(useCue, m_fadeLength, null);
                            break;
                        case Actions.PlayRandom:
                            if (index == randomIndex)
                            {
                                useCue = was.Play(useCue, m_fadeLength, null);
                            }
                            break;
                        case Actions.PlaySequence:
                            if (index == m_sequenceIndex)
                            {
                                useCue = was.Play(useCue, m_fadeLength, null);
                                shouldIncreaseSequence = true;
                            }
                            break;
                        case Actions.Stop:
                            useCue = was.Stop(useCue, m_fadeLength);
                            break;
                        case Actions.PlayRandomNoRepeats:
                            if (index == randomIndex)
                            {
                                useCue = was.Play(useCue, m_fadeLength, null);
                            }
                            break;
                    }
                }

                if (cuesOut != null)
                {
                    cuesOut.Add(useCue);
                }

                ++index;
            }
            if (shouldIncreaseSequence)
            {
                m_sequenceIndex = (m_sequenceIndex + 1) % m_audioSources.Length;
            }
        }

    }

}