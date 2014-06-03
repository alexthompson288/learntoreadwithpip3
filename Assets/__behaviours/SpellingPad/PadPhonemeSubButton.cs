using UnityEngine;
using System.Collections;

public class PadPhonemeSubButton : MonoBehaviour 
{
	[SerializeField]
	private PadPhoneme m_padPhoneme;
	
	void OnClick()
	{
		m_padPhoneme.Activate();
	}
}
