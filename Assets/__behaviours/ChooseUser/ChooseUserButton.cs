using UnityEngine;
using System.Collections;

public class ChooseUserButton : MonoBehaviour 
{
	[SerializeField]
	UILabel m_label;
	[SerializeField]
	UITexture m_profilePic;

	// Use this for initialization
	public void SetUp (string userName, string imageName, UIDraggablePanel draggablePanel) 
	{
		m_label.text = userName;

		if(UserInfo.Instance.GetCurrentUser() == userName)
		{
			StartCoroutine(StartOn());
		}

		Texture2D image = Resources.Load<Texture2D>("userPictures/" + imageName);

		if(image != null)
		{
			m_profilePic.mainTexture = image;
		}

		GetComponent<UIDragPanelContents>().draggablePanel = draggablePanel;
	}

	IEnumerator StartOn()
	{
		yield return new WaitForSeconds(0.5f);
		ChooseUserCoordinator.Instance.SelectButton(gameObject);
	}

	void OnClick()
	{
		UserInfo.Instance.SetCurrentUser(m_label.text);
		ChooseUserCoordinator.Instance.SelectButton(gameObject);
	}
}
