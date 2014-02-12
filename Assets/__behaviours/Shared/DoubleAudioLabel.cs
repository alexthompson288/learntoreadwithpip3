using UnityEngine;
using System.Collections;

public class DoubleAudioLabel : MonoBehaviour 
{
	public delegate void Single(DoubleAudioLabel behaviour);
	public event Single OnSingle;

	public delegate void Double(DoubleAudioLabel behaviour);
	public event Double OnDouble;

	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private AudioSource m_audioSource;

	AudioClip m_audioSingle;
	AudioClip m_audioDouble;

	
	// Use this for initialization
	public void SetUp (string text, AudioClip audioSingle, AudioClip audioDouble = null) 
	{
		m_label.text = text;

		m_audioSingle = audioSingle;
		m_audioDouble = audioDouble;
	}

	void OnClick()
	{
		if(OnSingle != null)
		{
			OnSingle(this);
		}

		StartCoroutine(PlayAudioSingle());
	}

	IEnumerator PlayAudioSingle()
	{
		if(m_audioDouble != null)
		{
			yield return new WaitForSeconds(0.3f);
		}

		if(m_audioSingle != null)
		{
			m_audioSource.clip = m_audioSingle;
			m_audioSource.Play();
		}

		yield break;
	}

	void OnDoubleClick()
	{
		if(m_audioDouble != null)
		{
			StopAllCoroutines();

			m_audioSource.clip = m_audioDouble;
			m_audioSource.Play();
		}

		if(OnDouble != null)
		{
			OnDouble(this);
		}
	}

	public string GetText()
	{
		return m_label.text;
	}

	public void ToUpper()
	{
		m_label.text = m_label.text.ToUpper();
	}

	public void ToLower()
	{
		m_label.text = m_label.text.ToLower();
	}
}
