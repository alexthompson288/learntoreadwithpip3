using UnityEngine;
using System.Collections;

public class SetPipPadLanguage : MonoBehaviour 
{
	[SerializeField]
	private string m_language;

	// Use this for initialization
	void OnClick () 
	{
		PipPadBehaviour.Instance.SetLanguage(m_language);
	}
}
