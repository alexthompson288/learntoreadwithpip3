using UnityEngine;
using System.Collections;

public class ChooseStoryType : MonoBehaviour 
{
	[SerializeField]
	private string m_storyType;

	
	// Update is called once per frame
	void OnClick () 
	{
		SessionInformation.Instance.SetStoryType(m_storyType);
	}
}
