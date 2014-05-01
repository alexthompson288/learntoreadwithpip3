using UnityEngine;
using System.Collections;

public class SequencedRotator : MonoBehaviour 
{
    [SerializeField]
    private RotationSequence[] m_sequences;
    /*
    [SerializeField]
    private float m_defaultSpeed;
    [SerializeField]
    private float m_defaultDelay;
    */
    
    Hashtable m_tweenArgs = new Hashtable();
    
    int m_index = 0;

    [System.Serializable]
    class RotationSequence
    {
        [SerializeField]
        public Vector3 m_eulerTarget;
        [SerializeField]
        public float m_speed;
        [SerializeField]
        public iTween.EaseType m_easeType = iTween.EaseType.linear;
        [SerializeField]
        public float m_postTweenDelay;
    }
    
    // Use this for initialization
    IEnumerator Start () 
    {
        m_tweenArgs.Add("oncomplete", "OnComplete");
        m_tweenArgs.Add("oncompletetarget", gameObject);

        
        yield return null;
        
        if (m_index < m_sequences.Length)
        {
            SetTweenArgs();
            
            iTween.RotateBy(gameObject, m_tweenArgs);
        }
    }
    
    void OnComplete()
    {
        StartCoroutine(OnCompleteCo());
    }
    
    IEnumerator OnCompleteCo()
    {
        float delay = m_sequences[m_index].m_postTweenDelay;
        
        yield return new WaitForSeconds(delay);
        
        ++m_index;
        
        if (m_index >= m_sequences.Length)
        {
            m_index = 0;
        }
        
        SetTweenArgs();
        
        iTween.RotateTo(gameObject, m_tweenArgs);
    }

    void SetTweenArgs()
    {
        m_tweenArgs ["rotation"] = m_sequences [m_index].m_eulerTarget;
        m_tweenArgs ["speed"] = m_sequences[m_index].m_speed;
        m_tweenArgs ["easetype"] = m_sequences [m_index].m_easeType;
    }
}

/*
using UnityEngine;
using System.Collections;

public class SequencedRotator : MonoBehaviour 
{
    [SerializeField]
    private Vector3[] m_eulerTargets;
    [SerializeField]
    private float[] m_speeds;
    [SerializeField]
    private float m_defaultSpeed;
    [SerializeField]
    private float[] m_delays;
    [SerializeField]
    private float m_defaultDelay;

    Hashtable m_tweenArgs = new Hashtable();

    int m_index = 0;

    class RotationSequence
    {
        [SerializeField]
        private Vector3 m_eulerTarget;
        [SerializeField]
        private float m_speed;
        [SerializeField]
        private float m_postTweenDelay;
    }

	// Use this for initialization
	IEnumerator Start () 
    {
        m_tweenArgs.Add("oncomplete", "OnComplete");
        m_tweenArgs.Add("oncompletetarget", gameObject);
        m_tweenArgs.Add("speed", FindSpeed());

        yield return null;

        if (m_index < m_eulerTargets.Length)
        {
            m_tweenArgs.Add("rotation", m_eulerTargets[0]);

            //float delay = m_index < m_delays.Length ? m_delays [m_index] : 0;
            //yield return new WaitForSeconds(delay);

            iTween.RotateTo(gameObject, m_tweenArgs);
        }
	}

    void OnComplete()
    {
        StartCoroutine(OnCompleteCo());
    }

    IEnumerator OnCompleteCo()
    {
        float delay = m_index < m_delays.Length ? m_delays [m_index] : m_defaultDelay;

        yield return new WaitForSeconds(delay);

        ++m_index;

        if (m_index >= m_eulerTargets.Length)
        {
            m_index = 0;
        }

        m_tweenArgs ["rotation"] = m_eulerTargets [m_index];
        m_tweenArgs ["speed"] = FindSpeed();

        iTween.RotateTo(gameObject, m_tweenArgs);
    }

    float FindSpeed()
    {
        return m_index < m_speeds.Length ? m_speeds [m_index] : m_defaultSpeed;
    }
}
*/
