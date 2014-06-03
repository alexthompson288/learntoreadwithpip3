using UnityEngine;
using System.Collections;

public class PlayWordSpellingButton : MonoBehaviour 
{
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private GameObject m_hideObject;
	
	public void SetWordAudio(string audioFilename)
	{
        m_audioSource.clip = LoaderHelpers.LoadAudioForWord(audioFilename);
	}
	
	public void OnDisable()
	{
		m_audioSource.clip = null;
		Resources.UnloadUnusedAssets();
	}
	
	void OnClick()
	{
		SpeakAll();
	}

	public void SpeakAll()
	{
		StartCoroutine(SpellingPadBehaviour.Instance.Segment(m_audioSource.clip.length + 0.15f));
	}
	
	public void Speak()
	{
		StartCoroutine(PlayWord());
	}
	
	IEnumerator PlayWord()
	{
		SpellingPadBehaviour.Instance.HighlightWholeWord();

		m_audioSource.Play();
		while (m_audioSource.isPlaying)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.2f);		
	}

	public float GetClipLength()
	{
		float clipLength = 0;
		if(m_audioSource.clip != null)
		{
			clipLength = m_audioSource.clip.length;
		}
		return clipLength;
	}
}
