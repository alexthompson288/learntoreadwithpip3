using UnityEngine;
using System.Collections;

public class CompleteSentenceWord : MonoBehaviour 
{
	[SerializeField]
	private UILabel m_label;

	string m_word;
	int m_width;

	public void SetUp(string word, bool isTarget)
	{
		m_word = word;
		m_label.text = m_word;

		if(isTarget)
		{
			m_label.alpha = 0;
		}

		m_width = (int)(m_label.font.CalculatePrintedSize(m_label.text, false, UIFont.SymbolStyle.None).x*1.25f);
	}

	public string GetWord()
	{
		return m_word;
	}

	public string GetEditedWord()
	{
		return StringHelpers.Edit(m_word);
	}

	public int GetWidth()
	{
		return m_width;
	}
}
