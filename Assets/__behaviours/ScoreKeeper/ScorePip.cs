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
    private iTween.EaseType m_platformEaseType = iTween.EaseType.linear;
    [SerializeField]
    private float m_collectibleTweenSpeed = 2f;
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
