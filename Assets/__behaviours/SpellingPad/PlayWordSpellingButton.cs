using UnityEngine;
using System.Collections;

public class PlayWordSpellingButton : MonoBehaviour {

	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private SimpleSpriteAnim m_spriteAnim;
	[SerializeField]
	private GameObject m_hideObject;
	
	public void SetWordAudio(string audioFilename)
	{
		AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(audioFilename);
		if (loadedAudio == null)
		{
			m_hideObject.SetActive(false);
		}
		else
		{
			m_hideObject.SetActive(true);
			m_spriteAnim.PlayAnimation("ON");
			m_audioSource.clip = loadedAudio;
			collider.enabled = true;
		}
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
		collider.enabled = false;
		m_spriteAnim.PlayAnimation("OFF");
		m_audioSource.Play();
		while (m_audioSource.isPlaying)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.2f);
		collider.enabled = true;
		m_spriteAnim.PlayAnimation("ON");
		
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
