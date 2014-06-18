using UnityEngine;
using System.Collections;

public class UserMenuButton : MonoBehaviour 
{
    [SerializeField]
    private UISprite m_background;
	[SerializeField]
	UILabel m_label;
	[SerializeField]
    UISprite m_picture;
    [SerializeField]
    private ColorInfo.PipColor m_pipColorA = ColorInfo.PipColor.Blue;
    [SerializeField]
    private ColorInfo.PipColor m_pipColorB = ColorInfo.PipColor.Green;

    string m_spriteNameA;
    string m_spriteNameB;

	// Use this for initialization
	public void SetUp (string userName, string imageName, UIDraggablePanel draggablePanel) 
	{
        Debug.Log("imageName: " + imageName);

		m_label.text = userName;

        m_spriteNameA = imageName;
        m_spriteNameB = NGUIHelpers.GetLinkedSpriteName(m_spriteNameA);

        bool isCurrentUser = userName == UserInfo.Instance.GetCurrentUser();
        m_picture.spriteName = isCurrentUser ? m_spriteNameB : m_spriteNameA;
        m_background.color = isCurrentUser ? ColorInfo.GetColor(m_pipColorB) : ColorInfo.GetColor(m_pipColorA);

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

    public void ChangeSprite(bool toStateB, float tweenDuration)
    {
        m_picture.spriteName = toStateB ? m_spriteNameB : m_spriteNameA;

        Color backgroundColor = toStateB ? ColorInfo.GetColor(m_pipColorB) : ColorInfo.GetColor(m_pipColorA);
        //TweenColor.Begin(m_background.gameObject, tweenDuration, backgroundColor);
        m_background.color = toStateB ? ColorInfo.GetColor(m_pipColorB) : ColorInfo.GetColor(m_pipColorA);
    }
}
