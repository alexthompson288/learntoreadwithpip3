using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreKeeper : MonoBehaviour 
{
    public delegate void ScoreKeeperEventHandler (ScoreKeeper keeper);
    public event ScoreKeeperEventHandler Completed;

    [SerializeField]
    private bool m_clampScore = true;
    [SerializeField]
    private List<string> m_correctAudio = new List<string>();
    [SerializeField]
    private List<string> m_incorrectAudio = new List<string>();

    protected int m_numAnswered = 0;
    protected int m_score = 0;
    protected int m_targetScore;

    protected bool m_hasSwitchedOn = false;

    protected void PlayAudio(int delta)
    {
        List<string> eventList = delta > 0 ? m_correctAudio : m_incorrectAudio;

        foreach(string audioEvent in eventList)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent(audioEvent);
        }
    }

    public void AddCorrectAudio(string audioEvent)
    {
        m_correctAudio.Add(audioEvent);
    }

    public void RemoveCorrectAudio(string audioEvent)
    {
        m_correctAudio.Remove(audioEvent);
    }

    public void AddIncorrectAudio(string audioEvent)
    {
        m_incorrectAudio.Remove(audioEvent);
    }

    public void RemoveIncorrectAudio(string audioEvent)
    {
        m_incorrectAudio.Remove(audioEvent);
    }

    public virtual void SetTargetScore(int targetScore) 
    {
        m_targetScore = targetScore;
    }

    public virtual void UpdateScore(int delta = 1)
    {
        ++ m_numAnswered;
        m_score += delta;

        if (m_clampScore)
        {
            m_score = Mathf.Clamp(m_score, 0, m_targetScore);
        }
    }

    public virtual void PlayIncorrectAnimation() {}

    public virtual IEnumerator UpdateScore(GameObject targetGo, int delta = 1) { yield return null; }

    public virtual IEnumerator Celebrate() { yield return null; }

    public virtual bool HasCompleted() 
    {
        //D.Log("base.HasFinished()");
        return true;
    }

    public int GetScore()
    {
        return m_score;
    }

    protected void InvokeCompleted()
    {
        if (Completed != null)
        {
            Completed(this);
        }
    }
}
