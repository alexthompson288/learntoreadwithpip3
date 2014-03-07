using UnityEngine;
using System.Collections;

public class ChooseStoryType : MonoBehaviour 
{
	[SerializeField]
	private string m_storyType;

	
	// Update is called once per frame
	void OnClick () 
	{
#if UNITY_IPHONE
		System.Collections.Generic.Dictionary<string, string> ep = new System.Collections.Generic.Dictionary<string, string>();
		ep.Add("StoryType", m_storyType);
		FlurryBinding.logEventWithParameters("ChooseStoryType", ep, false);
#endif

		SessionInformation.Instance.SetStoryType(m_storyType);
	}
}
