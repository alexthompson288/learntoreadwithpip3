using UnityEngine;
using System.Collections;

public class ChoosePictureButton : MonoBehaviour 
{
	[SerializeField]
	UITexture m_picture;
	
	// Use this for initialization
	public void SetUp (Texture2D texture, UIDraggablePanel draggablePanel) 
	{
		Debug.Log("ChoosePicture - texture: " + texture);
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
