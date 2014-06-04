using UnityEngine;
using System.Collections;

public class SetStoryLanguage : MonoBehaviour {
	[SerializeField]
	private string m_language;
	
	// Update is called once per frame
	void OnClick () 
	{
		//StoryReaderLogic.Instance.SetLanguage(m_language);
        StoryCoordinator.Instance.SetTextAttribute(m_language);
	}
}
