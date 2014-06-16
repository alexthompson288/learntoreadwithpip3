using UnityEngine;
using System.Collections;

public class ChangeableBennyAudio : Singleton<ChangeableBennyAudio> 
{
	[SerializeField]
	private AudioSource m_audioSource;
	
	string m_initialInstruction = null;
	float m_delayBetweenInstructions = 0;
	AudioClip m_changeableInstruction;

	public void SetUp (string initialInstruction, float delayBetweenInstructions) 
	{
		m_initialInstruction = initialInstruction;

		m_delayBetweenInstructions = delayBetweenInstructions;
	}
	
	public void SetChangeableInstruction (AudioClip changeableInstruction)
	{
		m_changeableInstruction = changeableInstruction;
	}
	
	void OnClick()
	{
		if(m_audioSource.isPlaying)
		{
			return;
		}
		
		if(m_initialInstruction != null)
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent(m_initialInstruction);
		}
		
		StartCoroutine(PlayChangeableInstruction());
	}
	
	IEnumerator PlayChangeableInstruction()
	{
		yield return new WaitForSeconds(m_delayBetweenInstructions);
		
		if(m_audioSource != null)
		{
			m_audioSource.clip = m_changeableInstruction;
			m_audioSource.Play();
		}
	}
}
