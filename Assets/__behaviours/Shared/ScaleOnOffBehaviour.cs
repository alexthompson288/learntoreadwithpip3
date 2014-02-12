using UnityEngine;
using System.Collections;

public class ScaleOnOffBehaviour : MonoBehaviour 
{
	[SerializeField]
	private float m_tweenDuration = 0.3f;

	void Start()
	{
		iTween.ScaleFrom(gameObject, Vector3.zero, m_tweenDuration);
	}
	
	public IEnumerator Off()
	{
		iTween.ScaleTo(gameObject, Vector3.zero, m_tweenDuration);
		
		yield return new WaitForSeconds(m_tweenDuration);
		
		Destroy(gameObject);
	}
	
	public float GetTweenDuration ()
	{
		return m_tweenDuration;
	}
}
