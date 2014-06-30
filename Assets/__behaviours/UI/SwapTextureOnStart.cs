using UnityEngine;
using System.Collections;

public class SwapTextureOnStart : MonoBehaviour 
{
    [SerializeField]
    private UITexture m_texture;
    [SerializeField]
    private float m_delay;
    [SerializeField]
    private Texture2D m_newImage;
    [SerializeField]
    private string m_audioEvent;

	IEnumerator Start () 
    {
        yield return new WaitForSeconds(m_delay);
        m_texture.mainTexture = m_newImage;
        WingroveAudio.WingroveRoot.Instance.PostEvent(m_audioEvent);
	}
}
