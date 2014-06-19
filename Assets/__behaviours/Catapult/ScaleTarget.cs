using UnityEngine;
using System.Collections;

public class ScaleTarget : Target 
{
	[SerializeField]
	private Vector2 m_durationOn = new Vector2(3f, 4.5f);
	[SerializeField]
	private Vector2 m_durationOff = new Vector2(1.1f, 2.5f);
	[SerializeField]
	private float m_tweenDurationOn = 0.3f;
    [SerializeField]
    private float m_tweenDurationOff = 0.1f;

    Hashtable m_tweenArgsOff = new Hashtable();

    void Awake()
    {
        transform.localScale = Vector3.zero;

        m_tweenArgsOff.Add("scale", Vector3.zero);
        m_tweenArgsOff.Add("time", m_tweenDurationOff);
        m_tweenArgsOff.Add("easetype", iTween.EaseType.linear);
    }

	public override void ApplyHitForce(Transform ball)
	{		
        MyStopCoroutines();

		base.ApplyHitForce(ball);
		
		StartCoroutine(Restart());
	}

	IEnumerator Restart()
	{
		yield return new WaitForSeconds(2f);
		
        iTween.ScaleTo(gameObject, m_tweenArgsOff);
		
        yield return new WaitForSeconds(m_tweenDurationOff);

        ResetSpriteName();
		
		rigidbody.velocity = Vector3.zero;

		rigidbody.isKinematic = true;

		transform.position = m_startLocation.position;
		
        //Debug.Log("Restart Invoking");
		InvokeMoveCompleted();
		
        On(0.5f);
	}

    protected override IEnumerator OnCo(float initialDelay)
	{
        //Debug.Log("ScaleTarget.OnCo");
		yield return new WaitForSeconds(initialDelay);
		
        iTween.ScaleTo(gameObject, Vector3.one, m_tweenDurationOn);
        collider.enabled = true;
		
		yield return new WaitForSeconds(m_tweenDurationOn + Random.Range(m_durationOn.x, m_durationOn.y));

        iTween.ScaleTo(gameObject, m_tweenArgsOff);
        collider.enabled = false;
		
        yield return new WaitForSeconds(m_tweenDurationOff + Random.Range(m_durationOff.x, m_durationOff.y));
		
        //Debug.Log("Invoking OnCompleteMove");
		InvokeMoveCompleted();
		
		StartCoroutine(OnCo(0.2f));
	}

    public override void MyStopCoroutines()
    {
        StopAllCoroutines();
        base.MyStopCoroutines();

        collider.enabled = false;
    }
	
	public override void Off()
	{
        MyStopCoroutines();
		iTween.ScaleTo(gameObject, m_tweenArgsOff);
	}
}
