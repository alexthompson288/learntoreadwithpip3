﻿using UnityEngine;
using System.Collections;

public class StoryPlayLineButton : MonoBehaviour 
{
    [SerializeField]
    AudioSource m_audioSource;
    [SerializeField]
    private GameObject m_hideObject;

    public void SetLineAudio(string audioClipName)
    {
        AudioClip newClip = LoaderHelpers.LoadObject<AudioClip>(audioClipName);
        m_audioSource.clip = newClip;
        if (newClip == null)
        {
			//D.Log("Couldn't find audio for: " + audioClipName);
            m_hideObject.SetActive(false);
        }
        else
        {
            collider.enabled = true;
            m_hideObject.SetActive(true);
        }
        Resources.UnloadUnusedAssets();
    }

    void OnPress(bool isDown)
    {
        if (!isDown)
        {
            StartCoroutine(PlayLineAudio());
        }
    }

    IEnumerator PlayLineAudio()
    {
        collider.enabled = false;
        m_audioSource.Play();
        if (m_audioSource.clip != null)
        {
            yield return new WaitForSeconds(m_audioSource.clip.length + 0.2f);
        }
        collider.enabled = true;
    }
}
