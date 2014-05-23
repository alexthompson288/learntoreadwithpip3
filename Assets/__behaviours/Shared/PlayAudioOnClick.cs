using UnityEngine;
using System.Collections;

public class PlayAudioOnClick : MonoBehaviour 
{
    [SerializeField]
    private AudioSource m_audioSource;

    void OnClick()
    {
        if (m_audioSource.clip != null && !m_audioSource.isPlaying)
        {
            m_audioSource.Play();
        }
    }
}
