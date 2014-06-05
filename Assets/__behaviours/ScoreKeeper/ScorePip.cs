using UnityEngine;
using System.Collections;

public class ScorePip : ScoreKeeper 
{
    [SerializeField]
    private Transform m_bottom;
    [SerializeField]
    private Transform m_top;
    [SerializeField]
    private Transform m_pipParent;
    [SerializeField]
    private float m_platformTweenDuration = 0.3f;
    [SerializeField]
    private iTween.EaseType m_platformEaseType = iTween.EaseType.easeOutQuad;
    [SerializeField]
    private float m_collectableTweenSpeed = 2f;
    [SerializeField]
    private iTween.EaseType m_collectableEaseType = iTween.EaseType.easeOutQuad;
    [SerializeField]
    private Transform m_collectionPoint;
    [SerializeField]
    private SimpleSpriteAnim m_pipAnim;
    [SerializeField]
    private AnimRandomizer m_pipAnimRandomizer;
    [SerializeField]
    private AnimRandomizer m_popAnimRandomizer;

    float m_pointDistance;

    public override void SetTargetScore(int targetScore)
    {
        base.SetTargetScore(targetScore);
        
        float delta = Mathf.Abs((m_top.localPosition - m_bottom.localPosition).magnitude);
        m_pointDistance = Mathf.Lerp(0, delta, 1f / (float)m_targetScore);
        UpdatePlatformLevel();
    }
    
    public override void UpdateScore(int delta = 1)
    {
        base.UpdateScore(delta);
        UpdatePlatformLevel(); 
    }

    public override IEnumerator UpdateScore(GameObject targetGo, int delta = 1)
    {
        targetGo.transform.parent = m_collectionPoint;
        targetGo.layer = m_collectionPoint.gameObject.layer;
        
        base.UpdateScore(delta);

        iTween.Stop(targetGo);
        
        yield return null;
        
        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("position", m_collectionPoint.position);
        tweenArgs.Add("speed", m_collectableTweenSpeed);
        tweenArgs.Add("easetype", m_collectableEaseType);
        iTween.MoveTo(targetGo, tweenArgs);
        
        float cauldronTweenDuration = Mathf.Abs(((m_collectionPoint.transform.position - targetGo.transform.position).magnitude) / m_collectableTweenSpeed);

        yield return new WaitForSeconds(cauldronTweenDuration);
        
        Destroy(targetGo);
        
        UpdatePlatformLevel();
    }

    void UpdatePlatformLevel()
    {
        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("time", m_platformTweenDuration);
        tweenArgs.Add("easetype", m_platformEaseType);
        tweenArgs.Add("position", new Vector3(m_pipParent.localPosition.x, m_bottom.localPosition.y + (m_pointDistance * m_score), m_pipParent.localPosition.z));
        tweenArgs.Add("islocal", true);
        
        iTween.MoveTo(m_pipParent.gameObject, tweenArgs);

        m_pipAnimRandomizer.Off();
        m_pipAnim.OnAnimFinish += OnScoreAnimFinish;
        string animName = Random.Range(0, 2) == 1 ? "THUMBS_UP" : "GIGGLE";
        m_pipAnim.PlayAnimation(animName);
    }

    bool m_hasFinishedJumpAnim = false;

    public override IEnumerator On()
    {
        Debug.Log("ScorePip.On()");

        m_popAnimRandomizer.Off();

        m_pipAnimRandomizer.Off();

        m_pipAnim.OnAnimFinish -= OnScoreAnimFinish;
        m_pipAnim.OnAnimFinish += OnJumpAnimFinish;
        
        m_pipAnim.PlayAnimation("JUMP");
        
        while (!m_hasFinishedJumpAnim)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        Hashtable tweenArgs = new Hashtable();
        //tweenArgs.Add("speed", 500);
        tweenArgs.Add("speed", 400);
        tweenArgs.Add("easetype", iTween.EaseType.linear);
        //tweenArgs.Add("easetype", iTween.EaseType.easeInOutBack);
        //tweenArgs.Add("easetype", iTween.EaseType.easeInBounce);
        tweenArgs.Add("position", new Vector3(m_pipAnim.transform.localPosition.x + 500, m_pipAnim.transform.localPosition.y));
        tweenArgs.Add("islocal", true);

        iTween.MoveTo(m_pipAnim.gameObject, tweenArgs);  

        m_pipAnim.PlayAnimation("WALK");

        yield return new WaitForSeconds(1f);
    }

    void OnScoreAnimFinish(string animName)
    {
        m_pipAnim.OnAnimFinish -= OnScoreAnimFinish;
        m_pipAnimRandomizer.On();
    }

    void OnJumpAnimFinish(string animName)
    {
        m_pipAnim.OnAnimFinish -= OnJumpAnimFinish;
        m_hasFinishedJumpAnim = true;
    }

    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            StartCoroutine(On());
        }
    }
}
