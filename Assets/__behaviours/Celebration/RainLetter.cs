using UnityEngine;
using System.Collections;

public class RainLetter : MonoBehaviour 
{
	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private Color[] m_colors;

	void Awake()
	{
		m_label.color = m_colors[Random.Range(0, m_colors.Length)];

		m_label.text = "a";

		for (char i = 'a'; i <= 'z'; ++i)
		{
			string result = i == 'q' ? "qu" : i.ToString();

			if(Random.Range(0, 27) == 0)
			{
				m_label.text = result;
				break;
			}
		}
	}
}
