using UnityEngine;
using System.Collections;

public class WordRoomButton : MonoBehaviour 
{
	[SerializeField]
	private UILabel m_label;

	public void SetUp (string word) 
	{
		m_label.text = word;
	}

	void OnClick () 
	{
		PipPadBehaviour.Instance.Show(m_label.text);
	}
}
