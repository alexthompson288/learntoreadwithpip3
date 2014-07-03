using UnityEngine;
using System.Collections;

public class EditorAudioListenerVolume : MonoBehaviour 
{
#if UNITY_EDITOR
	[SerializeField]
	private float m_editorVolume;

	void Update () 
	{
		AudioListener.volume = m_editorVolume;
	}
#endif
}
