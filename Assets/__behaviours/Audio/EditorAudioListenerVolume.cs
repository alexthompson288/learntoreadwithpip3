using UnityEngine;
using System.Collections;

public class EditorAudioListenerVolume : MonoBehaviour 
{
#if UNITY_EDITOR
	[SerializeField]
	private float m_editorVolume;
    [SerializeField]
    private bool m_audioOff;

	void Update () 
	{
		AudioListener.volume = m_audioOff ? 0 : m_editorVolume;
	}
#endif
}
