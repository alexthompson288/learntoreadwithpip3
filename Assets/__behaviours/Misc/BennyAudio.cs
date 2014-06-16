using UnityEngine;
using System.Collections;

public class BennyAudio : MonoBehaviour 
{
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private AudioClip m_clip;
    [SerializeField]
	private string m_instruction;

	public void SetInstruction (string instruction) 
	{
		m_instruction = instruction;
	}  
	
	void OnClick()
	{
        PlayAudio();
	}

    public void PlayAudio()
    {
        if (m_clip != null && !m_audioSource.isPlaying)
        {
            m_audioSource.clip = m_clip;
            m_audioSource.Play();
        }
        else if(m_instruction != null)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent(m_instruction);
        }
    }

    public bool IsPlayingLocal()
    {
        return m_audioSource.isPlaying;
    }
}
