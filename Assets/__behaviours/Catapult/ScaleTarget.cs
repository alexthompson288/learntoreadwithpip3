using UnityEngine;
using System.Collections;

public class ScaleTarget : Target 
{
	[SerializeField]
	private Vector2 m_durationOn = new Vector2(3f, 4.5f);
	[SerializeField]
	private Vector2 m_durationOff = new Vector2(1.1f, 2.5f);
	[SerializeField]
	private float m_tweenDuration = 0.3f;

    void Awake()
    {
        transform.localScale = Vector3.zero;
    }

	public override void ApplyHitForce(Transform ball)
	{
		StopAllCoroutines();
		
		base.ApplyHitForce(ball);
		
		StartCoroutine(Restart());
	}

	IEnumerator Restart()
	{
		yield return new WaitForSeconds(2f);
		
		iTween.ScaleTo(gameObject, Vector3.zero, m_tweenDuration);
		
		yield return new WaitForSeconds(m_tweenDuration);

        ResetSpriteName();
		
		rigidbody.velocity = Vector3.zero;

		rigidbody.isKinematic = true;

		transform.position = m_startLocation.position;
		
        //Debug.Log("Restart Invoking");
		InvokeOnCompleteMove();
		
		StartCoroutine(On(0));
	}

	public override IEnumerator On(float initialDelay)
	{
		yield return new WaitForSeconds(initialDelay);
		
		iTween.ScaleTo(gameObject, Vector3.one, m_tweenDuration);
		
		yield return new WaitForSeconds(m_tweenDuration + Random.Range(m_durationOn.x, m_durationOn.y));
		
		iTween.ScaleTo(gameObject, Vector3.zero, m_tweenDuration);
		
		yield return new WaitForSeconds(m_tweenDuration + Random.Range(m_durationOff.x, m_durationOff.y));
		
        //Debug.Log("On Invoking");
		InvokeOnCompleteMove();
		
		StartCoroutine(On(0));
	}
	
	public override void Off()
	{
		StopAllCoroutines();
		iTween.ScaleTo(gameObject, Vector3.zero, m_tweenDuration);
	}
}
