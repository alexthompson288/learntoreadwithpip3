using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Benny : Collector 
{
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private List<AudioClip> m_clips = new List<AudioClip>();

    bool m_isPlaying = false;

    public void InsertAudio(AudioClip clip, int index = -1)
    {
        if (index < 0 || index >= m_clips.Count)
        {
            m_clips.Add(clip);
        }
        else
        {
            m_clips[index] = clip;
        }
    }

    public void ChangeLastAudio(AudioClip clip)
    {
        if (m_clips.Count > 0)
        {
            m_clips [m_clips.Count - 1] = clip;
        }
        else
        {
            m_clips.Add(clip);
        }
    }

    void OnClick()
    {
        if (!m_isPlaying)
        {
            m_isPlaying = true;
            StartCoroutine(PlayAudio());
        }
    }

    IEnumerator PlayAudio()
    {
        m_anim.PlayAnimation("OPEN");

        foreach (AudioClip clip in m_clips)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();

            while(m_audioSource.isPlaying)
            {
                yield return null;
            }
        }

        m_anim.PlayAnimation("CLOSE");

        m_isPlaying = false;
    }
}
