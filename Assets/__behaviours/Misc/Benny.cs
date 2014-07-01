using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Benny : Collector 
{
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private AudioClip[] m_clips = new AudioClip[2];


    public void SetFirst(AudioClip clip)
    {
        if (m_clips.Length == 0)
        {
            m_clips = new AudioClip[2];
        }

        m_clips[0] = clip;
    }

    public void SetSecond(AudioClip clip)
    {
        if (m_clips.Length == 0)
        {
            m_clips = new AudioClip[2];
        }

        m_clips[1] = clip;
    }

    bool m_isPlaying = false;

    void Awake()
    {
        m_anim.AnimFinished += OnAnimFinish;
    }

    void OnClick()
    {
        if (!m_isPlaying)
        {
            m_isPlaying = true;
            StartCoroutine(PlayAudio());
        }
    }

    public IEnumerator PlayAudio()
    {
        m_anim.PlayAnimation("OPEN");

        foreach (AudioClip clip in m_clips)
        {
            if (clip != null)
            {
                m_audioSource.clip = clip;
                m_audioSource.Play();
            }

            while (m_audioSource.isPlaying)
            {
                yield return null;
            }
        }
        
        m_anim.PlayAnimation("CLOSE");

        m_isPlaying = false;
    }

    void OnAnimFinish(SpriteAnim anim, string animName)
    {
        if (animName == "OPEN")
        {
            m_anim.StopRandom();
        }
    }

    public bool IsPlaying()
    {
        return m_audioSource.isPlaying;
    }
}
