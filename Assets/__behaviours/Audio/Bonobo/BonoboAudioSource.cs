using UnityEngine;
using System.Collections;

public class BonoboAudioSource : MonoBehaviour 
{
    [SerializeField]
    private AudioSource m_audioSource;

    string m_eventName;
    public string eventName
    {
        get
        {
            return m_eventName;
        }
    }

    public IEnumerator On(string myEventName, AudioClip clip)
    {
        m_eventName = myEventName;

        m_audioSource.clip = clip;
        m_audioSource.Play();

        yield return new WaitForSeconds(clip.length);

        BonoboAudio.Instance.RemoveSource(this);

        Destroy(gameObject);
    }
}
