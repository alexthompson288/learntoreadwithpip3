using UnityEngine;
using System.Collections;

public class ToggleAudioOnClick : MonoBehaviour 
{
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private UISprite m_backgroundSprite;
	[SerializeField]
	private string m_offSpriteName = null;
	[SerializeField]
	private string m_onSpriteName = null;

	void OnClick()
	{
		if(m_audioSource.isPlaying)
		{
			m_audioSource.Stop();

			if(m_offSpriteName != null)
			{
				m_backgroundSprite.spriteName = m_offSpriteName;
			}
		}
		else
		{
			m_audioSource.Play();

			if(m_onSpriteName != null)
			{
				m_backgroundSprite.spriteName = m_onSpriteName;
			}
		}
	}
}
