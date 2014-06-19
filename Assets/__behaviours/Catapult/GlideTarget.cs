using UnityEngine;
using System.Collections;

public class GlideTarget : Target 
{
    [SerializeField]
    private float m_tweenSpeed = 1f;
    [SerializeField]
    private Vector2 m_durationOff = new Vector2(5f, 15f);
    [SerializeField]
    private Transform m_offLocation;

    Hashtable m_tweenArgs = new Hashtable();

    void Awake()
    {
        m_tweenArgs.Add("speed", m_tweenSpeed);
        m_tweenArgs.Add("position", m_offLocation);
        m_tweenArgs.Add("easetype", iTween.EaseType.linear);
        m_tweenArgs.Add("oncomplete", "Restart");
    }

    public override void ApplyHitForce(Transform ball)
    {
        StopAllCoroutines();
        iTween.Stop(gameObject);
        
        base.ApplyHitForce(ball);
        
        StartCoroutine(RestartCo());
    }

    void Restart()
    {
        StartCoroutine(RestartCo());
    }

    IEnumerator RestartCo()
    {
        yield return new WaitForSeconds(Random.Range(m_durationOff.x, m_durationOff.y));

        ResetSpriteName();

        if (!rigidbody.isKinematic)
        {
            rigidbody.velocity = Vector3.zero;
        } 
        
        rigidbody.isKinematic = true;
        transform.position = m_startLocation.position;

        InvokeMoveCompleted();

        On(0);
    }

    protected override IEnumerator OnCo(float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);

        iTween.MoveTo(gameObject, m_tweenArgs);
    }
    
    public override void Off()
    {
        StopAllCoroutines();
        iTween.Stop(gameObject);
    }
}
