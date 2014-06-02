using UnityEngine;
using System.Collections;

public class UserPictureButton : MonoBehaviour 
{
    [SerializeField]
    private UISprite m_picture;

    string m_spriteNameA;
    string m_spriteNameB;
	

    public void SetUp (string spriteName, UIDraggablePanel draggablePanel) 
    {
        m_picture.spriteName = spriteName;

        m_spriteNameA = m_picture.spriteName;
        m_spriteNameB = DataHelpers.GetLinkedSpriteName(m_spriteNameA);
        
        GetComponent<UIDragPanelContents>().draggablePanel = draggablePanel;
    }

    public void ChangeSprite(bool toStateB)
    {
        m_picture.spriteName = toStateB ? m_spriteNameB : m_spriteNameA;
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
