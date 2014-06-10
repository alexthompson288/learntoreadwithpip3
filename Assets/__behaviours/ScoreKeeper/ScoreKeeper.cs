using UnityEngine;
using System.Collections;

public class ScoreKeeper : MonoBehaviour 
{
    [SerializeField]
    protected bool m_playAudio = true;

    protected int m_score = 0;
    protected int m_targetScore;


    public void SetPlayAudio(bool playAudio)
    {
        m_playAudio = playAudio;
    }

    protected void PlayAudio(int delta)
    {
        if (m_playAudio)
        {
            string audioEvent = delta > 0 ? "VOCAL_CORRECT" : "VOCAL_INCORRECT";
            WingroveAudio.WingroveRoot.Instance.PostEvent(audioEvent);
        }
    }

    public virtual void SetTargetScore(int targetScore) 
    {
        m_targetScore = targetScore;
    }

    public virtual void UpdateScore(int delta = 1)
    {
        m_score += delta;
        m_score = Mathf.Clamp(m_score, 0, m_targetScore);
    }

    public virtual IEnumerator UpdateScore(GameObject targetGo, int delta = 1) { yield return null; }

    public virtual IEnumerator On() { yield return null; }
}
