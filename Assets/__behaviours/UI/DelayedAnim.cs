using UnityEngine;
using System.Collections;

public class DelayedAnim : MonoBehaviour 
{
    [SerializeField]
    private SpriteAnim m_animBehaviour;
    [SerializeField]
    private string m_animationName;
    [SerializeField]
    private string m_audioEvent;
    [SerializeField]
    private float m_delay;

	// Use this for initialization
	IEnumerator Start () 
    {
        yield return new WaitForSeconds(m_delay);
        m_animBehaviour.PlayAnimation(m_animationName);
        WingroveAudio.WingroveRoot.Instance.PostEvent(m_audioEvent);
	}
}
