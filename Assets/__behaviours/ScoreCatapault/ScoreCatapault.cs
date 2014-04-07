﻿using UnityEngine;
using System.Collections;

public class ScoreCatapault : MonoBehaviour 
{
    public delegate void ReachTargetScore(int targetScore);
    public event ReachTargetScore OnReachTargetScore;

    public delegate void Launch(int targetScore);
    public event Launch OnLaunch;

    [SerializeField]
    private Transform m_origin;
    [SerializeField]
    private Transform m_target;
    [SerializeField]
    private float m_elasticTweenSpeed = 0.1f;
    [SerializeField]
    private float m_linearTweenSpeed = 0.5f;
    [SerializeField]
    private Rigidbody m_rigidbody;
    [SerializeField]
    private Vector3 m_launchForce = new Vector3(0, 3, 0);
    [SerializeField]
    private Vector3 m_launchGravity = new Vector3(0, -2, 0);
    [SerializeField]
    private bool m_autoLaunch = false;
    [SerializeField]
    private Transform[] m_lineOrigins;
    [SerializeField]
    private LineRenderer[] m_lineRenderers;
    [SerializeField]
    private Transform m_lineEnd;
    [SerializeField]
    private Transform m_lineEndDefaultPos;
    [SerializeField]
    private bool m_isFinalPullLinear;

    public bool isFinalPullLinear
    {
        get
        {
            return m_isFinalPullLinear;
        }
    }

    float m_pointDistance;

    int m_targetScore;
    int m_score = 0;

    bool m_hasLaunched = false;

    void Awake()
    {
        m_launchForce.z = 0;

        for (int i = 0; i < m_lineOrigins.Length && i < m_lineRenderers.Length; ++i)
        {
            m_lineRenderers[i].SetVertexCount(2);

            Vector3 pos = m_lineOrigins[i].position;
            pos.z = -0.3f;
            m_lineRenderers[i].SetPosition(0, pos);

            pos = m_lineEnd.position;
            pos.z = -0.3f;
            m_lineRenderers[i].SetPosition(1, pos);
        }

#if UNITY_EDITOR
        SetScore(m_debugTargetScore);
#endif
    }

#if UNITY_EDITOR
    [SerializeField]
    private int m_debugTargetScore = 5;
#endif

    void Update()
    {
        for (int i = 0; i < m_lineOrigins.Length && i < m_lineRenderers.Length; ++i)
        {
            Vector3 pos = m_lineEnd.position;
            pos.z = -0.3f;
            m_lineRenderers[i].SetPosition(1, pos);
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.O))
        {
            IncrementScore(-1);
        } 
        else if(Input.GetKeyDown(KeyCode.P))
        {
            IncrementScore();
        }
#endif
    }

    public void ReleaseLineEnd()
    {
        if (m_hasLaunched)
        {
            m_lineEnd.parent = m_lineEndDefaultPos;

            Hashtable tweenArgs = new Hashtable();

            tweenArgs.Add("position", m_lineEndDefaultPos);
            tweenArgs.Add("speed", m_elasticTweenSpeed);
            tweenArgs.Add("easetype", iTween.EaseType.easeOutElastic);

            iTween.MoveTo(m_lineEnd.gameObject, tweenArgs);
        }
    }

    public void SetScore(int targetScore)
    {
        m_targetScore = targetScore;

        Debug.Log("targetScore: " + m_targetScore);

        float delta = Mathf.Abs((m_target.position - m_origin.position).magnitude);
        m_pointDistance = Mathf.Lerp(0, delta, 1f / (float)m_targetScore);

        Debug.Log("origin: " + m_origin.position);
        Debug.Log("target: " + m_target.position);
        Debug.Log("delta: " + delta);
        Debug.Log("pointDistance: " + m_pointDistance);
    }

    public void IncrementScore(int scoreIncrement = 1)
    {
        Debug.Log("IncrementScore");
        m_score += scoreIncrement;
        m_score = Mathf.Clamp(m_score, 0, m_targetScore);

        TweenRigidbody(); 
    }

    public void TweenRigidbody()
    {
        Debug.Log("ScoreCatapault Tween");

        WingroveAudio.WingroveRoot.Instance.PostEvent("CANNON_PLACEHOLDER_STRETCH_SHORT");

        Hashtable tweenArgs = new Hashtable();

        bool useLinear = (m_isFinalPullLinear && m_score >= m_targetScore);

        tweenArgs.Add("position", m_origin.position + (m_target.position - m_origin.position).normalized * m_pointDistance * m_score);

        float speed = useLinear ? m_linearTweenSpeed : m_elasticTweenSpeed;
        tweenArgs.Add("speed", speed);

        iTween.EaseType easeType = useLinear ? iTween.EaseType.linear : iTween.EaseType.easeOutElastic;
        tweenArgs.Add("easetype", easeType);

        iTween.MoveTo(m_rigidbody.gameObject, tweenArgs);

        if (m_score >= m_targetScore)
        {
            if(OnReachTargetScore != null)
            {
                OnReachTargetScore(m_score);
            }

            if(m_autoLaunch)
            {
                StartCoroutine(WaitForAutoLaunch());
                //LaunchRigidbody();
            }
        }
    }

    IEnumerator WaitForAutoLaunch()
    {
        yield return null;
        if (Mathf.Approximately(m_rigidbody.transform.position.y, m_target.position.y) || m_rigidbody.transform.position.y < m_target.position.y)
        {
            LaunchRigidbody();
        } 
        else
        {
            StartCoroutine(WaitForAutoLaunch());
        }
    }
    
    public void LaunchRigidbody()
    {
        if(OnLaunch != null)
        {
            OnLaunch(m_score);
        }

        Debug.Log("LAUNCH!!!");
        WingroveAudio.WingroveRoot.Instance.PostEvent("CANNON_PLACEHOLDER_LAUNCH");
        m_hasLaunched = true;
        iTween.Stop(m_rigidbody.gameObject);
        m_rigidbody.isKinematic = false;
        m_rigidbody.AddForce(m_launchForce, ForceMode.VelocityChange);
    }

    void FixedUpdate()
    {
        if (m_hasLaunched)
        {
            m_rigidbody.AddForce(m_launchGravity, ForceMode.Acceleration);
        }
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        GUILayout.Label("Score: " + m_score);
        GUILayout.Label("Target Score: " + m_targetScore);
    }
#endif
}
