using UnityEngine;
using System.Collections;

public class ScoreKeeper : MonoBehaviour 
{
    protected int m_score = 0;
    protected int m_targetScore;

    public virtual void SetTargetScore(int targetScore) 
    {
        m_targetScore = targetScore;
    }

    public virtual void UpdateScore(int delta = 1)
    {
        m_score += delta;
        m_score = Mathf.Clamp(m_score, 0, m_targetScore);
    }

    public virtual IEnumerator UpdateScore(GameObject targetGo, int delta = 1) { Debug.Log("base.UpdateScore - Coroutine"); yield return null; }

    public virtual IEnumerator On() { yield return null; }
}
