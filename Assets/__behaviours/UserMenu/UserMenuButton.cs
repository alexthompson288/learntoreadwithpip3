using UnityEngine;
using System.Collections;

public class UserMenuButton : MonoBehaviour 
{
	[SerializeField]
	UILabel m_label;
	[SerializeField]
	UITexture m_profilePic;

	// Use this for initialization
	public void SetUp (string userName, string imageName, UIDraggablePanel draggablePanel) 
	{
        Debug.Log("imageName: " + imageName);

		m_label.text = userName;

		Texture2D image = Resources.Load<Texture2D>("userPictures/" + imageName);

		if(image != null)
		{
			m_profilePic.mainTexture = image;
		}

		GetComponent<UIDragPanelContents>().draggablePanel = draggablePanel;
	}

	void OnClick()
	{
		UserInfo.Instance.SetCurrentUser(m_label.text);
        TransitionScreen.Instance.ChangeLevel("NewVoyage", true);
	}
}
