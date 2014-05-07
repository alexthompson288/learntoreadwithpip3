using UnityEngine;
using System.Collections;

public class ShowInfoButton : MonoBehaviour 
{
	[SerializeField]
	private NewStoryBrowserBookButton m_storyBrowserBookbutton;
	
	void OnClick()
	{
		m_storyBrowserBookbutton.ShowBuyPanel();
	}
}
