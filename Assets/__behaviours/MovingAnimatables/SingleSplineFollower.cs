using UnityEngine;
using System.Collections;

public class SingleSplineFollower : MonoBehaviour 
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
    [SerializeField]
    private float m_splineChangeSpeed = 1;

    private float m_currentTime;

    int m_currentPathIndex = 0;
    
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

/*
using UnityEngine;
using System.Collections;

public class SplineFollower : MonoBehaviour 
{
    [SerializeField]
    private bool m_startOn = true;
    [SerializeField]
    private Transform m_follower;
    [SerializeField]
    private Spline[] m_paths;
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
    [SerializeField]
    private float m_splineChangeSpeed = 1;

    private float m_currentTime;

    int m_currentPathIndex = 0;
    
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

        if (m_paths.Length == 0)
        {
            m_paths = m_follower.transform.parent.GetComponentsInChildren<Spline>() as Spline[];
        }

        m_initialProportionalDistance = Mathf.Clamp01(m_initialProportionalDistance);
        m_alphaProportionalDistances.x = Mathf.Clamp01(m_alphaProportionalDistances.x);
        m_alphaProportionalDistances.y = Mathf.Clamp01(m_alphaProportionalDistances.y);

        m_currentTime = m_totalTime * m_initialProportionalDistance;
    }

    void Start ()
    {
        if (m_startOn)
        {
            StartCoroutine(Follow());
        }
    }

    public void ChangeSpline(int index = -1)
    {
        StopAllCoroutines();

        m_currentTime = 0;

        if (index == -1)
        {
            ++m_currentPathIndex;
        } 
        else
        {
            m_currentPathIndex = index;
        }

        if (m_currentPathIndex > m_paths.Length - 1)
        {
            m_currentPathIndex = 0;
        }
        else if (m_currentPathIndex < 0)
        {
            m_currentPathIndex = m_currentPathIndex - 1;
        }

        StartCoroutine(TweenToNewSpline());
    }

    public IEnumerator TweenToNewSpline()
    {
        SplineNode[] nodes = m_paths [m_currentPathIndex].SplineNodes;

        if (nodes.Length > 0)
        {
            Hashtable tweenArgs = new Hashtable();

            Vector3 targetPos = nodes [0].transform.position;

            tweenArgs.Add("position", targetPos);
            tweenArgs.Add("easetype", iTween.EaseType.linear);
            tweenArgs.Add("speed", m_splineChangeSpeed);

            float tweenDuration = (targetPos - m_follower.transform.position).magnitude / m_splineChangeSpeed;

            iTween.MoveTo(m_follower.gameObject, tweenArgs);

            yield return new WaitForSeconds(tweenDuration);

            StartCoroutine(Follow());
        }

        yield break;
    }

    public IEnumerator Follow () 
    {
        Spline path = m_paths [m_currentPathIndex];

        if(path != null)
        {
            m_currentTime += Time.deltaTime * m_speedModifier;

            float proportionalDistance = m_currentTime / m_totalTime;

            transform.position = path.GetPositionOnSpline(proportionalDistance);

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
                transform.right = -path.GetTangentToSpline(m_currentTime / m_totalTime);
            }
            
            if(m_currentTime > m_totalTime)
            {
                if(m_destroyOnComplete)
                {
                    Destroy(gameObject);
                    Destroy(path.gameObject);
                }
                else
                {
                    m_currentTime = 0.0f;
                }
            }
        }

        yield return null;

        StartCoroutine(Follow());
    }
}
 */ 
