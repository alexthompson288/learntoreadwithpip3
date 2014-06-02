using UnityEngine;
using System.Collections;

public class UserMenuButton : MonoBehaviour 
{
	[SerializeField]
	UILabel m_label;
	[SerializeField]
    UISprite m_picture;

    string m_spriteNameA;
    string m_spriteNameB;

	// Use this for initialization
	public void SetUp (string userName, string imageName, UIDraggablePanel draggablePanel) 
	{
        Debug.Log("imageName: " + imageName);

		m_label.text = userName;

        m_spriteNameA = imageName;
        m_spriteNameB = DataHelpers.GetLinkedSpriteName(m_spriteNameA);

        bool isCurrentUser = userName == UserInfo.Instance.GetCurrentUser();
        m_picture.spriteName = isCurrentUser ? m_spriteNameB : m_spriteNameA;

        if (isCurrentUser)
        {
            UserMenuCoordinator.Instance.SelectButton(this);
        }

		GetComponent<UIDragPanelContents>().draggablePanel = draggablePanel;
	}

	void OnClick()
	{
		UserInfo.Instance.SetCurrentUser(m_label.text);
        UserMenuCoordinator.Instance.SelectButton(this);
	}

    public void ChangeSprite(bool toStateB)
    {
        m_picture.spriteName = toStateB ? m_spriteNameB : m_spriteNameA;
    }
}
