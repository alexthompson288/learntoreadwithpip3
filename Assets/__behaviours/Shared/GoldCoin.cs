using UnityEngine;
using System.Collections;

public class GoldCoin : MonoBehaviour {
	
	[SerializeField]
	private float m_scaleTweenDuration = 0.5f;
	[SerializeField]
	private float m_moveTweenDuration = 1f;
	[SerializeField]
	private bool m_destroyOnTweenEnd = true;
	
	// Use this for initialization
	void Start () 
	{
		iTween.ScaleFrom(gameObject, Vector3.zero, m_scaleTweenDuration);
	}
	
	public IEnumerator TweenToPosition(Vector3 newPosition)
	{
		yield return null;
		
		iTween.MoveTo(gameObject, newPosition, m_moveTweenDuration);
		// TODO: Visual/Audio effects (e.g. particle system, fast rotation in movement direction, sparkle sound);
		
		if(m_destroyOnTweenEnd)
		{
			//StartCoroutine(Off());
			yield return new WaitForSeconds(m_moveTweenDuration);
			WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEARS");
			iTween.ScaleTo(gameObject, Vector3.zero, m_scaleTweenDuration);
			yield return new WaitForSeconds(m_scaleTweenDuration);
			Destroy(gameObject);
		}
	}

	public float GetTotalTweenDuration()
	{
		return (m_moveTweenDuration + m_scaleTweenDuration);
	}

	public float GetScaleTweenDuration()
	{
		return m_scaleTweenDuration;
	}
}
