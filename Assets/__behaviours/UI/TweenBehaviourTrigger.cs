using UnityEngine;
using System.Collections;

public class TweenBehaviourTrigger : MonoBehaviour 
{
    [SerializeField]
    private bool m_switchOn;
    [SerializeField]
    private TweenBehaviour m_tweenBehaviour;

	void OnClick()
    {
        if (m_switchOn)
        {
            m_tweenBehaviour.On();
        } 
        else
        {
            m_tweenBehaviour.Off();
        }
    }
}
