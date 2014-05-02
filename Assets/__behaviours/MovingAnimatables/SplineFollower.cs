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
    private bool m_constantRotation;
    [SerializeField]
    private Vector3 m_rotationModifier = Vector3.zero;
    [SerializeField]
    private bool m_destroyOnComplete = false;
    [SerializeField]
    private Vector2 m_alphaProportionalDistances = new Vector2(0, 1);
    [SerializeField]
    private UIWidget[] m_alphaWidgets;

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
        m_alphaProportionalDistances.x = Mathf.Clamp01(m_alphaProportionalDistances.x);
        m_alphaProportionalDistances.y = Mathf.Clamp01(m_alphaProportionalDistances.y);

        m_currentTime = m_totalTime * m_initialProportionalDistance;
    }

    // Update is called once per frame
    void Update () 
    {
        if(m_path != null)
        {
            m_currentTime += Time.deltaTime * m_speedModifier;

            float proportionalDistance = m_currentTime / m_totalTime;

            transform.position = m_path.GetPositionOnSpline(proportionalDistance);

            if(m_alphaWidgets.Length > 0)
            {
                float widgetAlpha = 1;

                if(proportionalDistance < m_alphaProportionalDistances.x)
                {
                    widgetAlpha = proportionalDistance / m_alphaProportionalDistances.x;
                }
                else if(proportionalDistance > m_alphaProportionalDistances.y)
                {
                    widgetAlpha = (1 - proportionalDistance) / (1 - m_alphaProportionalDistances.y);
                }

                foreach(UIWidget widget in m_alphaWidgets)
                {
                    widget.alpha = widgetAlpha;
                }
            }

            if(!m_constantRotation)
            {
                transform.right = -m_path.GetTangentToSpline(m_currentTime / m_totalTime);
            }
            
            if(m_currentTime > m_totalTime)
            {
                if(m_destroyOnComplete)
                {
                    Destroy(gameObject);
                    Destroy(m_path.gameObject);
                }
                else
                {
                    m_currentTime = 0.0f;
                }
            }
        }
    }
}
