using UnityEngine;
using System.Collections;

public class StoryTypeLabel : MonoBehaviour 
{
	[SerializeField]
	private UILabel m_label;

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		m_label.text = SessionInformation.Instance.GetStoryType();

		if(m_label.text == "Classic")
		{
			m_label.text += "s";
		}
	}
}
