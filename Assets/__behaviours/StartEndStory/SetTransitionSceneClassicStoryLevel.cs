using UnityEngine;
using System.Collections;

public class SetTransitionSceneClassicStoryLevel : MonoBehaviour 
{
	[SerializeField]
	private TransitionToSceneButton m_transitionToSceneButton;

	// Use this for initialization
	void Awake () 
	{
		if(SessionInformation.Instance.GetStoryType() == "Classic")
		{
			Debug.Log("Changing scene transition");
			m_transitionToSceneButton.SetScene("NewChooseClassicStoryLevel");
		}
		else
		{
			Debug.Log("Keeping scene transition: " + SessionInformation.Instance.GetStoryType());
		}
	}
}
