using UnityEngine;
using System.Collections;

public class LearnPhonemeLetter : MonoBehaviour 
{
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private UISprite m_background;
	[SerializeField]
	private string[] m_backgroundNames;

	AudioClip m_phonemeAudio;
	AudioClip m_mnemonicAudio;

	bool m_pressed = false;

	void Awake()
	{
		m_background.spriteName = m_backgroundNames[Random.Range(0, m_backgroundNames.Length)];
	}

	public void SetUp (AudioClip phonemeAudio, AudioClip mnemonicAudio) 
	{
		m_phonemeAudio = phonemeAudio;
		m_mnemonicAudio = mnemonicAudio;
	}

	void OnClick () 
	{
		StartCoroutine(OnClickCo());
	}

	IEnumerator OnClickCo()
	{
		if(m_mnemonicAudio != null)
		{
			m_audioSource.clip = m_mnemonicAudio;
			m_audioSource.Play();
			yield return new WaitForSeconds(m_audioSource.clip.length + 0.2f);
		}

		if(m_phonemeAudio != null)
		{
			m_audioSource.clip = m_phonemeAudio;
			m_audioSource.Play();
		}
		yield return null;
	}

	public bool GetPressed()
	{
		return m_pressed;
	}

	public void SetPressed(bool pressed)
	{
		m_pressed = pressed;
	}
}
