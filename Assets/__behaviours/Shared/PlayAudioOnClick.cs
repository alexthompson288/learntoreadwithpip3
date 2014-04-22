using UnityEngine;
using System.Collections;

public class PlayAudioOnClick : MonoBehaviour 
{
    [SerializeField]
    private AudioClip m_clip;
    [SerializeField]
    private AudioSource m_source;

	// Use this for initialization
	void Awake () 
    {
        if (m_source == null)
        {
            m_source = GetComponent<AudioSource>() as AudioSource;
        }
	}
	
	public void SetClip(AudioClip clip)
    {
        m_clip = clip;
    }

    void OnClick()
    {
        if (m_source.isPlaying)
        {
            m_source.Stop();
        }
        else if (m_clip != null)
        {
            m_source.clip = m_clip;
            m_source.Play();
        }
    }
}
