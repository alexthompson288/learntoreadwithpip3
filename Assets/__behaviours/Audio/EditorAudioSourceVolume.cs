using UnityEngine;
using System.Collections;
using WingroveAudio;

public class EditorAudioSourceVolume : MonoBehaviour
{
    [SerializeField]
    private float m_volume;
    [SerializeField]
    private BaseWingroveAudioSource[] m_audioSources;

#if UNITY_EDITOR
    void Awake()
    {
        foreach (BaseWingroveAudioSource audioSource in m_audioSources)
        {
            audioSource.SetClipMixVolume(m_volume);
        }
    }
#endif
}
