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
    private UISprite m_hand;
    [SerializeField]
    private Transform m_handFollowLocation;
    [SerializeField]
    private UISprite m_catapultTroll;
    [SerializeField]
    private GameObject m_explosionLetterParent;
    [SerializeField]
    private GameObject m_explosionTroll;
    [SerializeField]
    private Transform m_explosionPosition;
    [SerializeField]
    private GameObject m_explosionLetterPrefab;
    [SerializeField]
    private Transform m_dropFromPosition;


    float m_pointDistance;
    
    bool m_hasLaunched = false;
    
    void Start()
    {
        m_explosionTroll.SetActive(false);

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
        bool shouldTween = (delta > 0 && m_score < m_targetScore) || (delta < 0 && m_score > 0);

        base.UpdateScore(delta);
        PlayAudio(delta);

        if (shouldTween)
        {
            TweenRigidbody(); 
        }
    }

    public override IEnumerator On()
    {
        if (!m_hasSwitchedOn)
        {
            m_hasSwitchedOn = true;

            while (!Mathf.Approximately(m_rigidbody.transform.position.y, m_target.position.y) && (m_rigidbody.transform.position.y >= m_target.position.y))
            {
                yield return null;
            } 
            
            //D.Log("LAUNCH!!!");
            WingroveAudio.WingroveRoot.Instance.PostEvent("CANNON_PLACEHOLDER_LAUNCH");
            m_hasLaunched = true;
            iTween.Stop(m_rigidbody.gameObject);
            m_rigidbody.isKinematic = false;
            m_rigidbody.AddForce(m_rigidbody.transform.TransformDirection(m_launchForce), ForceMode.VelocityChange);

            m_hand.spriteName = NGUIHelpers.GetLinkedSpriteName(m_hand.spriteName);

            yield return new WaitForSeconds(1f);

            m_catapultTroll.color = new Color(0, 0, 0, 0);

            m_explosionTroll.SetActive(true);
            
            float range = 150f;
            float minX = m_explosionLetterParent.transform.localPosition.x - range;
            float maxX = m_explosionLetterParent.transform.localPosition.x + range;
            
            float minY = m_explosionLetterParent.transform.localPosition.y - range;
            float maxY = m_explosionLetterParent.transform.localPosition.y + range;
            
            m_explosionLetterParent.transform.parent.gameObject.SetActive(true);
            m_explosionLetterParent.transform.localScale = Vector3.zero;
            
            float dropTweenDuration = 4.8f;
            
            Hashtable dropTweenVar = new Hashtable();
            dropTweenVar.Add("position", m_dropFromPosition);
            dropTweenVar.Add("time", dropTweenDuration);
            dropTweenVar.Add("easetype", iTween.EaseType.linear);
            iTween.MoveFrom(m_explosionTroll, dropTweenVar);
            iTween.MoveFrom(m_explosionLetterParent, dropTweenVar);
            
            WingroveAudio.WingroveRoot.Instance.PostEvent("BOMB_WHISTLE");

            yield return new WaitForSeconds(dropTweenDuration);

            //D.Log("SPAWNING");
            for (int i = 0; i < 80; ++i)
            {
                GameObject newExplosionLetter = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_explosionLetterPrefab, m_explosionLetterParent.transform);
                newExplosionLetter.transform.localPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), newExplosionLetter.transform.localPosition.z);
            }
            
            float scaleTweenDuration = 0.2f;
            TweenScale.Begin(m_explosionLetterParent, scaleTweenDuration, Vector3.one);
            TweenScale.Begin(m_explosionTroll, scaleTweenDuration, m_explosionTroll.transform.localScale * 1.5f);
            yield return new WaitForSeconds(scaleTweenDuration);
            m_explosionTroll.SetActive(false);
            Rigidbody[] explosionLetters = m_explosionLetterParent.GetComponentsInChildren<Rigidbody>() as Rigidbody[];
            foreach (Rigidbody letter in explosionLetters)
            {
                letter.AddExplosionForce(Random.Range(0.5f, 3f), m_explosionPosition.position, 0, 0, ForceMode.Impulse);
            }
            
            WingroveAudio.WingroveRoot.Instance.PostEvent("EXPLOSION_1");
            
            yield return new WaitForSeconds(3f);
        }

        yield break;
    }

    void TweenRigidbody()
    {
        //D.Log("ScoreCatapault Tween");

        Vector3 targetPos = m_origin.position + (m_target.position - m_origin.position).normalized * m_pointDistance * m_score;

        WingroveAudio.WingroveRoot.Instance.PostEvent("CANNON_PLACEHOLDER_STRETCH_SHORT");
        
        Hashtable tweenArgs = new Hashtable();
        
        tweenArgs.Add("position", targetPos);
        tweenArgs.Add("speed", m_tweenSpeed);        
        tweenArgs.Add("easetype", iTween.EaseType.easeOutElastic);
        
        iTween.MoveTo(m_rigidbody.gameObject, tweenArgs);
    }
    
    public void ReleaseLineEnd()
    {
        //D.Log("ReleaseLineEnd()");

        if (m_hasLaunched)
        {
            //D.Log("Releasing");

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

    public override bool HasCompleted()
    {
        return m_score >= m_targetScore;
    }
}

