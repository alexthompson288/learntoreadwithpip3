using UnityEngine;
using System.Collections;

public class PopTarget : Target 
{
    [SerializeField]
    private TweenOnOffBehaviour m_tweenBehaviour;
    [SerializeField]
    private Vector2 m_durationOn = new Vector2(3f, 4.5f);
    [SerializeField]
    private Vector2 m_durationOff = new Vector2(1.1f, 2.5f);
    [SerializeField]
    private Vector3 m_direction;
    [SerializeField]
    private float m_distance;

    public override void SetOffPosition(Vector3 direction, float distance)
    {
        m_direction = direction;
        m_distance = distance;

        m_startLocation.position = m_parent.position;
        m_startLocation.localPosition += m_direction.normalized * m_distance;
    }

    public override void ApplyHitForce(Transform ball)
    {
        StopAllCoroutines();
        m_tweenBehaviour.Stop();

        base.ApplyHitForce(ball);

        StartCoroutine(Restart());
    }

    IEnumerator Restart()
    {
        yield return new WaitForSeconds(2f);

        iTween.ScaleTo(gameObject, Vector3.zero, 0.2f);

        yield return new WaitForSeconds(0.2f);

        rigidbody.velocity = Vector3.zero;

        yield return null; 

        rigidbody.isKinematic = true;

        transform.position = m_startLocation.position;

        InvokeOnCompleteMove();

        iTween.ScaleTo(gameObject, Vector3.one, 0.2f);

        StartCoroutine(On(0));
    }

    public override IEnumerator On(float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);

        m_tweenBehaviour.On();

        yield return new WaitForSeconds(m_tweenBehaviour.GetTotalDuration() + Random.Range(m_durationOn.x, m_durationOn.y));

        m_tweenBehaviour.Off();

        yield return new WaitForSeconds(m_tweenBehaviour.GetTotalDurationOff() + Random.Range(m_durationOff.x, m_durationOff.y));

        InvokeOnCompleteMove();

        StartCoroutine(On(0));
    }

    public override void Off()
    {
        StopAllCoroutines();
        m_tweenBehaviour.Off();
    }
}
