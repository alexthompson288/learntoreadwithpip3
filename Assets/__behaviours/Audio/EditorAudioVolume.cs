using UnityEngine;
using System.Collections;

public class EditorAudioVolume : MonoBehaviour 
{
#if UNITY_EDITOR
	[SerializeField]
	private float m_editorVolume;

	void Start () 
	{
		AudioListener.volume = m_editorVolume;
	}
#endif
}
