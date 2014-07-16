using UnityEngine;
using System.Collections;

public class DataAudio : Singleton<DataAudio> 
{
    [SerializeField]
    private AudioSource m_audioSource;

    public void PlayNumber(int number)
    {
        Play(LoaderHelpers.LoadAudioForNumber(number));
    }

	public void PlayShort(DataRow data)
    {
        Play(DataHelpers.GetShortAudio(data));
    }

    public void PlayLong(DataRow data)
    {
        Play(DataHelpers.GetLongAudio(data));
    }

    void Play(AudioClip clip)
    {
        if (m_audioSource.isPlaying && m_audioSource.clip != clip)
        {
            m_audioSource.Stop();
        }

        m_audioSource.clip = clip;
        m_audioSource.Play();
    }
}
