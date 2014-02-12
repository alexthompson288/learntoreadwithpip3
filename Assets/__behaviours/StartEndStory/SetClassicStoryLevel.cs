using UnityEngine;
using System.Collections;

public class SetClassicStoryLevel : MonoBehaviour 
{
	[SerializeField]
	private string m_classicStoryLevel;

	// Use this for initialization
	void OnClick () 
	{
		SessionInformation.Instance.SetClassicStoryLevel(m_classicStoryLevel);
	}
}
