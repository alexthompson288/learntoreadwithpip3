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
		FlurryBinding.logEvent("Choose story type: " + m_storyType, false);
#endif

		SessionInformation.Instance.SetStoryType(m_storyType);
	}
}
