using UnityEngine;
using System.Collections;

public class PopTarget : Target 
{
    [SerializeField]
    private TweenOnOffBehaviour m_tweenBehaviour;
    [SerializeField]
    private float m_targetDuration = 4f;

    public override IEnumerator On()
    {
        m_tweenBehaviour.On();

        yield return new WaitForSeconds(m_tweenBehaviour.GetTotalDuration() + m_targetDuration);

        m_tweenBehaviour.Off();

        yield return new WaitForSeconds(m_tweenBehaviour.GetTotalDurationOff());

        InvokeOnCompleteMove();

        StartCoroutine(On());
    }

    public override void Off()
    {
        StopAllCoroutines();
        m_tweenBehaviour.Off();
    }
}
