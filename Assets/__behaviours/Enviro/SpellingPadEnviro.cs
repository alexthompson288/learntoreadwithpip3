using UnityEngine;
using System.Collections;

public class SpellingPadEnviro : MonoBehaviour 
{
	[SerializeField]
	private Texture2D m_spellingPadTexture;

	public Texture2D GetSpellingPadTexture()
	{
		return m_spellingPadTexture;
	}
}
