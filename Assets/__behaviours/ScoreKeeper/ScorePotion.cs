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
    private float m_potionTweenDuration = 0.3f;
    [SerializeField]
    private float m_cauldronTweenDuration = 0.5f;
    [SerializeField]
    private iTween.EaseType m_easeType = iTween.EaseType.linear;
    [SerializeField]
    private UISprite m_cauldron;

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

    public override IEnumerator UpdateScore(GameObject targetGo, int delta = 1)
    {
        base.UpdateScore(delta);

        float tweenDuration = 0.3f;

        iTween.MoveTo(targetGo, m_cauldron.transform.position, m_cauldronTweenDuration);
        iTween.ScaleTo(targetGo, Vector3.zero, m_cauldronTweenDuration);

        yield return new WaitForSeconds(m_cauldronTweenDuration);

        Destroy(targetGo);

        UpdatePotionLevel();
    }

    void UpdatePotionLevel()
    {
        Hashtable topTweenArgs = new Hashtable();
        topTweenArgs.Add("time", m_potionTweenDuration);
        topTweenArgs.Add("easetype", m_easeType);
        topTweenArgs.Add("position", new Vector3(m_liquidTop.position.x, m_liquidBody.transform.position.y + (m_pointDistance * m_score), m_liquidTop.position.z));

        iTween.MoveTo(m_liquidTop.gameObject, topTweenArgs);


        Hashtable bodyTweenArgs = new Hashtable();
        bodyTweenArgs.Add("time", m_potionTweenDuration);
        bodyTweenArgs.Add("easetype", m_easeType);

        float newScaleY = (m_target.localPosition.y - m_liquidBody.transform.localPosition.y) / m_liquidBody.height * m_score / m_targetScore;
        bodyTweenArgs.Add("scale", new Vector3(m_liquidBody.transform.localScale.x, newScaleY, m_liquidBody.transform.localScale.z));

        iTween.ScaleTo(m_liquidBody.gameObject, bodyTweenArgs);
    }
}
