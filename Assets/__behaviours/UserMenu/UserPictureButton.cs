using UnityEngine;
using System.Collections;

public class UserPictureButton : MonoBehaviour 
{
	[SerializeField]
	UITexture m_picture;
	
	// Use this for initialization
	public void SetUp (Texture2D texture, UIDraggablePanel draggablePanel) 
	{
		m_picture.mainTexture = texture;
		
		GetComponent<UIDragPanelContents>().draggablePanel = draggablePanel;
	}
	
	void OnClick()
	{
		CreateUserCoordinator.Instance.OnPictureChoose(this);
	}

	public string GetPictureName()
	{
		return m_picture.mainTexture.name;
	}
}
