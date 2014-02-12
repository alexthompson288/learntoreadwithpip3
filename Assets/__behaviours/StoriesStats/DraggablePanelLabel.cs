using UnityEngine;
using System.Collections;

public class DraggablePanelLabel : MonoBehaviour 
{
	[SerializeField]
	private UITexture m_background;
	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private BoxCollider m_collider;

	public string m_word;

	// Use this for initialization
	public void SetUp (string word) 
	{
		Debug.Log("SetUp: " + word);
		m_word = word;
		m_label.text = m_word;

		int width = (int)(m_label.font.CalculatePrintedSize(m_label.text, false, UIFont.SymbolStyle.None).x*1.1f + 30f);
		m_background.width = width;
		m_collider.size = new Vector3(width, m_collider.size.y, m_collider.size.z);

		Debug.Log("m_word: " + m_word);
		Debug.Log("text: " + m_label.text);
			
	}
}
