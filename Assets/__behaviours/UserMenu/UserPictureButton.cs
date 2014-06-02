using UnityEngine;
using System.Collections;

public class UserPictureButton : MonoBehaviour 
{
    [SerializeField]
    private UISprite m_background;
    [SerializeField]
    private UISprite m_picture;
    [SerializeField]
    private ColorInfo.PipColor m_pipColorA = ColorInfo.PipColor.Blue;
    [SerializeField]
    private ColorInfo.PipColor m_pipColorB = ColorInfo.PipColor.Green;

    string m_spriteNameA;
    string m_spriteNameB;
	

    public void SetUp (string spriteName, UIDraggablePanel draggablePanel) 
    {
        m_spriteNameA = spriteName;
        m_spriteNameB = DataHelpers.GetLinkedSpriteName(m_spriteNameA);

        m_picture.spriteName = m_spriteNameA;
        m_background.color = ColorInfo.GetColor(m_pipColorA);
        
        GetComponent<UIDragPanelContents>().draggablePanel = draggablePanel;
    }

    public void ChangeSprite(bool toStateB)
    {
        m_picture.spriteName = toStateB ? m_spriteNameB : m_spriteNameA;
        m_background.color = toStateB ? ColorInfo.GetColor(m_pipColorB) : ColorInfo.GetColor(m_pipColorA);
    }
	
	void OnClick()
	{
		CreateUserCoordinator.Instance.OnPictureChoose(this);
	}

    public string GetSpriteNameA()
    {
        return m_spriteNameA;
    }
}
