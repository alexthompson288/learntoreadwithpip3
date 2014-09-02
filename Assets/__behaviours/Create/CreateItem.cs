using UnityEngine;
using System.Collections;

public class CreateItem : MonoBehaviour 
{
    [SerializeField]
    private UITexture m_texture;
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private UIDragScrollView m_dragBehaviour;
    
    private GameObject m_instantiatedSprite;
    private Vector2 m_totalDrag;
    private string m_textureName;
    
    private Transform m_cutOff;
    private AudioClip m_clip;
    
    public void SetUp(string textureName, Texture2D texture, string label, Transform cutOff, AudioClip audioClip)
    {
        m_cutOff = cutOff;
        m_label.text = label == null ? "" : label;
        m_texture.mainTexture = texture;
        m_textureName = textureName;
        m_clip = audioClip;
    }
    
    void OnDrag(Vector2 drag)
    {
        
        Ray camPos = UICamera.currentCamera.ScreenPointToRay(
            new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
        m_totalDrag += drag;
        //if (camPos.origin.y > m_cutOff.transform.position.y)
        if(camPos.origin.x < m_cutOff.transform.position.x)
        {
            m_dragBehaviour.enabled = false;
            if (m_instantiatedSprite == null)
            {
                m_instantiatedSprite = CreateStickerManager.Instance.AddPoppedSprite(m_textureName, (Texture2D)m_texture.mainTexture, new Vector3(camPos.origin.x,camPos.origin.y,0));
                //                m_instantiatedSprite = CollectionRoomCoordinator.Instance.AddPoppedSprite(
                //                    m_textureName, (Texture2D)m_texture.mainTexture,
                //                    new Vector3(camPos.origin.x,camPos.origin.y,0));
                WingroveAudio.WingroveRoot.Instance.PostEvent("PICK_UP_STICKER");
            }
        }
        else
        {
            m_dragBehaviour.enabled = true;
            if (m_instantiatedSprite != null)
            {
                CreateStickerManager.Instance.DestroyPoppedSprite(m_instantiatedSprite);
                m_instantiatedSprite = null;
            }
        }
        
        if (m_instantiatedSprite != null)
        {
            m_instantiatedSprite.transform.position = new Vector3(camPos.origin.x,camPos.origin.y,0);
        }
    }
    
    void OnPress(bool press)
    {
        if (!press)
        {
            if (m_instantiatedSprite != null)
            {
                WingroveAudio.WingroveRoot.Instance.PostEvent("DROP_STICKER");
            }
            m_instantiatedSprite = null;
            m_dragBehaviour.enabled = true;
        }
        else
        {
            CreateStickerManager.Instance.PlayClip(m_clip);
        }
    }
}
