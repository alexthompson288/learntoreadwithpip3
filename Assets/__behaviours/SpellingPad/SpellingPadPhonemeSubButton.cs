using UnityEngine;
using System.Collections;

public class SpellingPadPhonemeSubButton : MonoBehaviour {

	[SerializeField]
	private SpellingPadPhoneme m_spellingPadPhoneme;
	
	void OnClick()
	{
		m_spellingPadPhoneme.Activate();
	}
}
