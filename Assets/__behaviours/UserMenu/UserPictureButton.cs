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
    [SerializeField]
    private GameObject m_scaleTweener;

    string m_spriteNameA;
    string m_spriteNameB;
	

    //public void SetUp (string spriteName, UIDraggablePanel draggablePanel) 
    public void SetUp (string spriteName)
    {
        m_spriteNameA = spriteName;
        m_spriteNameB = NGUIHelpers.GetLinkedSpriteName(m_spriteNameA);

        m_picture.spriteName = m_spriteNameA;
        m_background.color = ColorInfo.GetColor(m_pipColorA);
        
        //GetComponent<UIDragPanelContents>().draggablePanel = draggablePanel;
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

    void OnPress(bool pressed)
    {
        iTween.Stop(m_scaleTweener);
        
        float tweenDuration = 0.3f;
        Vector3 localScale = pressed ? Vector3.one * 0.8f : Vector3.one;
        
        iTween.ScaleTo(m_scaleTweener, localScale, tweenDuration);

        string audioEvent = pressed ? "SOMETHING_APPEAR" : "SOMETHING_DISAPPEAR";
        WingroveAudio.WingroveRoot.Instance.PostEvent(audioEvent);
    }
}
