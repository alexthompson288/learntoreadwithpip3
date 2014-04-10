using UnityEngine;
using System.Collections;

public class VoyageMedal : MonoBehaviour 
{
    [SerializeField]
    private float m_medalTweenDuration = 0.5f;
    [SerializeField]
    private float m_interTweenDelay = 0.3f;
    [SerializeField]
    private float m_ribbonTweenDuration = 0.8f;
    [SerializeField]
    private GameObject m_medalParent;
    [SerializeField]
    private Transform m_medalOnLocation;
    [SerializeField]
    private GameObject m_leftRibbon;
    [SerializeField]
    private Transform m_leftRibbonOnLocation;
    [SerializeField]
    private GameObject m_rightRibbon;
    [SerializeField]
    private Transform m_rightRibbonOnLocation;

	// Use this for initialization
	IEnumerator Start () 
    {
        Hashtable medalTweenArgs = new Hashtable();

        medalTweenArgs.Add("position", m_medalOnLocation);
        medalTweenArgs.Add("time", m_medalTweenDuration);
        medalTweenArgs.Add("easetype", iTween.EaseType.easeOutBounce);

        //iTween.MoveTo(m_medalParent, medalTweenArgs);
        iTween.MoveTo(m_medalParent, m_medalOnLocation.position, m_medalTweenDuration);

        yield return new WaitForSeconds(m_medalTweenDuration + m_interTweenDelay);

        iTween.MoveTo(m_leftRibbon, m_leftRibbonOnLocation.position, m_ribbonTweenDuration);
        iTween.MoveTo(m_rightRibbon, m_rightRibbonOnLocation.position, m_ribbonTweenDuration);
    }
}
