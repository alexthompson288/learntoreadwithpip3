using UnityEngine;
using System.Collections;

public class SplineFollower : MonoBehaviour 
{
    [SerializeField]
    private Transform m_follower;
    [SerializeField]
    private Spline m_path;
    [SerializeField]
    private float m_totalTime = 20.0f;
    [SerializeField]
    private float m_initialProportionalDistance = 0;
    [SerializeField]
    private float m_speedModifier = 1; 
    [SerializeField]
    private Vector3 m_rotationModifier = Vector3.zero;
    [SerializeField]
    private bool m_destroyOnComplete = false;

    private float m_currentTime;
    
    public void SetSpeedModifier(float newModifier)
    {
        m_speedModifier = newModifier;
    }

    // Use this for initialization
    void Awake () 
    {
        if (m_follower == null)
        {
            m_follower = transform;
        }

        m_initialProportionalDistance = Mathf.Clamp01(m_initialProportionalDistance);
        m_currentTime = m_totalTime * m_initialProportionalDistance;
    }

    // Update is called once per frame
    void Update () 
    {
        if(m_path != null)
        {
            m_currentTime += Time.deltaTime * m_speedModifier;
            transform.position = m_path.GetPositionOnSpline(m_currentTime / m_totalTime);
            transform.right = -m_path.GetTangentToSpline(m_currentTime / m_totalTime);
            
            if(m_currentTime > m_totalTime)
            {
                if(m_destroyOnComplete)
                {
                    Destroy(gameObject);
                }
                else
                {
                    m_currentTime = 0.0f;
                }
            }
        }
    }
}
