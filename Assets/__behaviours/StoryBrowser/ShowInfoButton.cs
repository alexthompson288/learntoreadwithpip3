using UnityEngine;
using System.Collections;

public class ShowInfoButton : MonoBehaviour {

	[SerializeField]
	private NewStoryBrowserBookButton m_storyBrowserBookbutton;
	
	void OnClick()
	{
		InfoPanelBox.Instance.SetCurrentBook(m_storyBrowserBookbutton);
		m_storyBrowserBookbutton.ShowInfoPanel();
	}
}
