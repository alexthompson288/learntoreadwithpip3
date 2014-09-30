using UnityEngine;
using System.Collections;

public class ResourcesAudio : Singleton<ResourcesAudio> 
{
    [SerializeField]
    private string m_resourcesRelativePath;
    [SerializeField]
    private string m_filenamePrefix;
    [SerializeField]
    private AudioSource m_audioSource;

	public void PlayFromResources (string eventName, bool cancelIfInUse = false) 
    {
        eventName = eventName.ToLower().Replace("!", "").Replace("?", "").Replace(" ", "_");

        if (!m_audioSource.isPlaying || !cancelIfInUse)
        {
            StopCoroutine("FreeResources");
            StartCoroutine(PlayFromResourcesCo(eventName));
        }
	}

    IEnumerator PlayFromResourcesCo(string eventName)
    {
        while (m_audioSource.isPlaying)
        {
            yield return null;
        }

        //D.Log("Finding: " + m_resourcesRelativePath + m_filenamePrefix + eventName);
        AudioClip clip = Resources.Load<AudioClip>(m_resourcesRelativePath + m_filenamePrefix + eventName);

        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();

            StartCoroutine("FreeResources"); // Call coroutine with string so that we can stop it if PlayFromResources is called before clip is finished playing
        }
    }

    IEnumerator FreeResources()
    {
        while (m_audioSource.isPlaying)
        {
            yield return null;
        }

        m_audioSource.clip = null;

        Resources.UnloadUnusedAssets();
    }
}
