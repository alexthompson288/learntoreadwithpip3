using UnityEngine;
using System.Collections;

public class MoveBackAndForth : MonoBehaviour 
{
    [SerializeField]
    private TweenBehaviour m_tweenBehaviour;

	// Use this for initialization
	void Start () 
    {
        if (m_tweenBehaviour == null)
        {
            m_tweenBehaviour = GetComponent<TweenBehaviour>() as TweenBehaviour;
        }

        m_tweenBehaviour.CompletedOff += On;
        m_tweenBehaviour.CompletedOn += Off;
	    
        if (m_tweenBehaviour.isOn)
        {
            m_tweenBehaviour.Off();
        }
        else
        {
            m_tweenBehaviour.On();
        }
	}

    void Off(TweenBehaviour behaviour)
    {
        m_tweenBehaviour.Off();
    }

    void On(TweenBehaviour behaviour)
    {
        m_tweenBehaviour.On();
    }
}
