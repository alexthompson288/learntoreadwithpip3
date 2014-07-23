using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class BonoboAudio : Singleton<BonoboAudio>
{
    [SerializeField]
    private GameObject m_audioPrefab;
    [SerializeField]
    private Transform m_audioSourceParent;
    [SerializeField]
    private AudioRef[] m_audioRefs;

    [System.Serializable]
    class AudioRef
    {
        public string m_eventName;
        public AudioClip m_clip;
        public int m_maxInstances = 1;
    }

    void Awake()
    {
        foreach (AudioRef audioRef in m_audioRefs)
        {
            if(audioRef.m_maxInstances == 0)
            {
                audioRef.m_maxInstances = 1;
            }
            else if(audioRef.m_maxInstances < -1)
            {
                audioRef.m_maxInstances = -1;
            }
        }
    }

    List<BonoboAudioSource> m_audioSources = new List<BonoboAudioSource>();

    public bool TryPlay(string eventName)
    {
        AudioRef audioRef = Array.Find(m_audioRefs, x => x.m_eventName == eventName);

        if (audioRef != null)
        {
            if (audioRef.m_maxInstances == -1 || m_audioSources.FindAll(x => x.eventName == eventName).Count < audioRef.m_maxInstances)
            {
                GameObject newObject = (GameObject)GameObject.Instantiate(m_audioPrefab, transform.position, transform.rotation);
                newObject.transform.parent = m_audioSourceParent;
                
                BonoboAudioSource source = newObject.GetComponent<BonoboAudioSource>() as BonoboAudioSource;
                m_audioSources.Add(source);
                StartCoroutine(source.On(eventName, audioRef.m_clip));
            }
        }

        return audioRef != null;
    }

    /*
    public bool HasReference(string eventName)
    {
        return Array.FindIndex(m_audioRefs, x => x.m_eventName == eventName) != -1;
    }

    public void PlayAudio(string eventName)
    {
        AudioRef audioRef = Array.Find(m_audioRefs, x => x.m_eventName == eventName);

        if (audioRef.m_maxInstances == -1 || m_audioSources.FindAll(x => x.eventName == eventName).Count < audioRef.m_maxInstances)
        {
            GameObject newObject = (GameObject)GameObject.Instantiate(m_audioPrefab, transform.position, transform.rotation);
            newObject.transform.parent = m_audioSourceParent;

            BonoboAudioSource source = newObject.GetComponent<BonoboAudioSource>() as BonoboAudioSource;
            m_audioSources.Add(source);
            StartCoroutine(source.On(eventName, audioRef.m_clip));
        }
    }
    */
    
    public void RemoveSource(BonoboAudioSource source)
    {
        m_audioSources.Remove(source);
    }
}
