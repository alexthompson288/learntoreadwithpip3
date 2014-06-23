using UnityEngine;
using System.Collections;

public class ScoreCatapult : ScoreKeeper 
{
    [SerializeField]
    private Transform m_origin;
    [SerializeField]
    private Transform m_target;
    [SerializeField]
    private float m_tweenSpeed = 0.1f;
    [SerializeField]
    private Rigidbody m_rigidbody;
    [SerializeField]
    private Vector3 m_launchForce = new Vector3(0, 8, 0);
    [SerializeField]
    private Vector3 m_launchGravity = new Vector3(0, -6, 0);
    [SerializeField]
    private Transform[] m_lineOrigins;
    [SerializeField]
    private LineRenderer[] m_lineRenderers;
    [SerializeField]
    private Transform[] m_lineEnds;
    [SerializeField]
    private Transform m_lineEndDefaultLocation;
    [SerializeField]
    private UITexture m_hand;
    [SerializeField]
    private Transform m_handFollowLocation;
    [SerializeField]
    private Texture2D m_handReleaseTexture;

    float m_pointDistance;
    
    bool m_hasLaunched = false;
    
    void Start()
    {
        m_launchForce.z = 0;
        
        for (int i = 0; i < m_lineOrigins.Length && i < m_lineRenderers.Length; ++i)
        {
            m_lineRenderers[i].SetVertexCount(2);
            
            Vector3 pos = m_lineOrigins[i].position;
            pos.z = -0.3f;
            m_lineRenderers[i].SetPosition(0, pos);
            
            pos = m_lineEnds[i].position;
            pos.z = -0.3f;
            m_lineRenderers[i].SetPosition(1, pos);
        }
    }

    public override void SetTargetScore(int targetScore)
    {
        base.SetTargetScore(targetScore);
        
        float delta = Mathf.Abs((m_target.position - m_origin.position).magnitude);
        m_pointDistance = Mathf.Lerp(0, delta, 1f / (float)m_targetScore);
    }
    
    public override void UpdateScore(int delta = 1)
    {
        base.UpdateScore(delta);
        PlayAudio(delta);
        TweenRigidbody(); 
    }

    public override IEnumerator On()
    {
        while (!Mathf.Approximately(m_rigidbody.transform.position.y, m_target.position.y) && (m_rigidbody.transform.position.y >= m_target.position.y))
        {
            yield return null;
        } 
        
        Debug.Log("LAUNCH!!!");
        WingroveAudio.WingroveRoot.Instance.PostEvent("CANNON_PLACEHOLDER_LAUNCH");
        m_hasLaunched = true;
        iTween.Stop(m_rigidbody.gameObject);
        m_rigidbody.isKinematic = false;
        m_rigidbody.AddForce(m_rigidbody.transform.TransformDirection(m_launchForce), ForceMode.VelocityChange);

        yield return new WaitForSeconds(2f);
    }

    void TweenRigidbody()
    {
        Debug.Log("ScoreCatapault Tween");
        
        WingroveAudio.WingroveRoot.Instance.PostEvent("CANNON_PLACEHOLDER_STRETCH_SHORT");
        
        Hashtable tweenArgs = new Hashtable();
        
        tweenArgs.Add("position", m_origin.position + (m_target.position - m_origin.position).normalized * m_pointDistance * m_score);
        tweenArgs.Add("speed", m_tweenSpeed);        
        tweenArgs.Add("easetype", iTween.EaseType.easeOutElastic);
        
        iTween.MoveTo(m_rigidbody.gameObject, tweenArgs);
    }
    
    public void ReleaseLineEnd()
    {
        Debug.Log("ReleaseLineEnd()");

        if (m_hasLaunched)
        {
            Debug.Log("Releasing");

            Hashtable tweenArgs = new Hashtable();
            
            //tweenArgs.Add("speed", m_tweenSpeed);
            tweenArgs.Add("time", 0.6f);
            tweenArgs.Add("easetype", iTween.EaseType.easeOutElastic);
            tweenArgs.Add("islocal", true);

            for(int i = 0; i < m_lineEnds.Length; ++i)
            {
                tweenArgs["position"] = new Vector3(m_lineEnds[i].localPosition.x, m_lineEndDefaultLocation.localPosition.y, m_lineEnds[i].localPosition.z);

                m_lineEnds[i].parent = m_lineEndDefaultLocation;
                
                iTween.MoveTo(m_lineEnds[i].gameObject, tweenArgs);
            }
        }
    }
    
    void FixedUpdate()
    {
        if (m_hasLaunched)
        {
            m_rigidbody.AddForce(m_launchGravity, ForceMode.Acceleration);
        }
    }
    
    void Update()
    {
        for (int i = 0; i < m_lineOrigins.Length && i < m_lineRenderers.Length; ++i)
        {
            Vector3 pos = m_lineEnds[i].position;
            pos.z = -0.3f;
            m_lineRenderers[i].SetPosition(1, pos);
        }
        
        if (!m_hasLaunched)
        {
            m_hand.transform.position = m_handFollowLocation.position;
        } 
        else
        {
            m_hand.mainTexture = m_handReleaseTexture;
        }

        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.O))
        {
            UpdateScore(-1);
        } 
        else if(Input.GetKeyDown(KeyCode.P))
        {
            UpdateScore();
        }
        else if(Input.GetKeyDown(KeyCode.I))
        {
            StartCoroutine(On());
        }
        #endif
    }
}

