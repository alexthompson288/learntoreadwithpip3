using UnityEngine;
using System.Collections;

public class ScorePotion : ScoreKeeper 
{
    [SerializeField]
    private Transform m_liquidTop;
    [SerializeField]
    private UISprite m_liquidBody;
    [SerializeField]
    private Transform m_target;
    [SerializeField]
    private float m_tweenDuration = 0.3f;
    [SerializeField]
    private iTween.EaseType m_easeType = iTween.EaseType.linear;

    float m_pointDistance;

    public override void SetTargetScore(int targetScore)
    {
        base.SetTargetScore(targetScore);

        float delta = Mathf.Abs((m_target.position - m_liquidBody.transform.position).magnitude);
        m_pointDistance = Mathf.Lerp(0, delta, 1f / (float)m_targetScore);
        UpdatePotionLevel();
    }

    public override void UpdateScore(int delta = 1)
    {
        base.UpdateScore(delta);
        
        UpdatePotionLevel(); 
    }

    void UpdatePotionLevel()
    {
        Hashtable topTweenArgs = new Hashtable();
        topTweenArgs.Add("time", m_tweenDuration);
        topTweenArgs.Add("easetype", m_easeType);
        topTweenArgs.Add("position", new Vector3(m_liquidTop.position.x, m_liquidBody.transform.position.y + (m_pointDistance * m_score), m_liquidTop.position.z));

        iTween.MoveTo(m_liquidTop.gameObject, topTweenArgs);


        Hashtable bodyTweenArgs = new Hashtable();
        bodyTweenArgs.Add("time", m_tweenDuration);
        bodyTweenArgs.Add("easetype", m_easeType);

        float newScaleY = (m_target.localPosition.y - m_liquidBody.transform.localPosition.y) / m_liquidBody.height * m_score / m_targetScore;
        bodyTweenArgs.Add("scale", new Vector3(m_liquidBody.transform.localScale.x, newScaleY, m_liquidBody.transform.localScale.z));

        iTween.ScaleTo(m_liquidBody.gameObject, bodyTweenArgs);
    }
}
