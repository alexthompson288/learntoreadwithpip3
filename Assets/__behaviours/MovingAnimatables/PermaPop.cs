using UnityEngine;
using System.Collections;

public class PermaPop : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_emptyPrefab;
    [SerializeField]
    private float m_initialDelay;
    [SerializeField]
    private float m_onDelay;
    [SerializeField]
    private float m_onDuration;
    [SerializeField]
    private iTween.EaseType m_onEaseType;
    [SerializeField]
    private float m_offDelay;
    [SerializeField]
    private float m_offDuration;
    [SerializeField]
    private iTween.EaseType m_offEaseType;
    [SerializeField]
    private Transform m_offLocation;

    Transform m_onLocation;

    Hashtable m_onTweenArgs = new Hashtable();
    Hashtable m_offTweenArgs = new Hashtable();

	// Use this for initialization
	IEnumerator Start () 
    {
        m_onLocation = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, transform).transform;
        m_onLocation.parent = m_offLocation;

        transform.position = m_offLocation.position;

        m_onTweenArgs.Add("position", m_onLocation);
        m_onTweenArgs.Add("time", m_onDuration);
        m_onTweenArgs.Add("easetype", m_onEaseType);

        m_offTweenArgs.Add("position", m_offLocation);
        m_offTweenArgs.Add("time", m_offDuration);
        m_offTweenArgs.Add("easetype", m_offEaseType);

        while (true)
        {
            iTween.MoveTo(gameObject, m_onTweenArgs);

            yield return new WaitForSeconds(m_onDuration + m_offDelay);

            iTween.MoveTo(gameObject, m_offTweenArgs);

            yield return new WaitForSeconds(m_offDuration + m_onDelay);
        }
	}
}
