using UnityEngine;
using System.Collections;

public class PadLetter : MonoBehaviour 
{
    [SerializeField]
    private UILabel m_label;

    Collider m_other;

    int m_positionIndex;
    public int GetPositionIndex()
    {
        return m_positionIndex;
    }

    public enum State
    {
        Unanswered,
        Hint,
        Answered
    }
    
    private State m_state;
    public State state
    {
        get
        {
            return m_state;
        }
    }

    public void SetUp(string letter, int positionIndex)
    {
        m_label.text = letter;
        m_positionIndex = positionIndex;
    }

    public void ChangeState(State newState, bool useLock = false)
    {
        // if useLock is true, m_state will not change to a lower state 
        if (!useLock || newState > m_state)
        {
            m_state = newState;
            
            float alphaTweenDuration = 0.25f;
            
            switch(m_state)
            {
                case State.Unanswered:
                    TweenAlpha.Begin(m_label.gameObject, alphaTweenDuration, 0);
                    break;
                case State.Hint:
                    TweenAlpha.Begin(m_label.gameObject, alphaTweenDuration, 0.5f);
                    break;
                case State.Answered:
                    TweenAlpha.Begin(m_label.gameObject, alphaTweenDuration, 1);
                    break;
            }
        }
    }

    public void Highlight()
    {
        
    }

    public Collider GetOther()
    {
        return m_other;
    }
    
    public void EnableTrigger(bool enable)
    {
        collider.enabled = false;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if(m_other == null)
        {
            m_other = other;
        }
        Debug.Log("Enter - m_other: " + m_other);
    }
    
    void OnTriggerExit(Collider other)
    {
        if(other == m_other)
        {
            m_other = null;
        }
        Debug.Log("Exit - m_other: " + m_other);
    }

    public float GetWidth()
    {
        return NGUIHelpers.GetLabelWidth(m_label);
    }

    public string GetLetter()
    {
        return m_label.text;
    }
}
