using UnityEngine;
using System.Collections;

public class AudioReference : MonoBehaviour 
{
	[SerializeField]
	private AudioClip m_audioClip;

	public AudioClip GetClip()
	{
		return m_audioClip;
	}
}
