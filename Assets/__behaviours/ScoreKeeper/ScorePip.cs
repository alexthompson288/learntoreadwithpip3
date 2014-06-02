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
    private SimpleSpriteAnim m_anim;

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
    }
}
