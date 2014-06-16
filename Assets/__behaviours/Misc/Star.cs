using UnityEngine;
using System.Collections;

public class Star : MonoBehaviour 
{
	[SerializeField]
	private float m_onTweenDuration = 1.5f;
	[SerializeField]
	private float m_offTweenDuration = 0.5f;
	[SerializeField]
	private UISprite m_sprite;
	[SerializeField]
	private string m_onString;
	[SerializeField]
	private string m_offString;

	void Start ()
	{
		iTween.ScaleFrom(gameObject, Vector3.zero, m_offTweenDuration); 
	}

	public void On()
	{
		iTween.PunchScale(gameObject, new Vector3(1.5f, 1.5f, 1.5f), m_onTweenDuration);
		iTween.PunchRotation(gameObject, new Vector3(0f, 0f, 360f), m_onTweenDuration);
		m_sprite.spriteName = m_onString;
	}
	
	public IEnumerator Off()
	{
		iTween.ScaleTo(gameObject, transform.localScale * 3, m_onTweenDuration);
		//iTween.PunchScale(gameObject, new Vector3(1.5f, 1.5f, 1.5f), m_onTweenDuration);
		//iTween.PunchRotation(gameObject, new Vector3(0f, 0f, 360f), m_onTweenDuration);
		
		yield return new WaitForSeconds(m_onTweenDuration);
		
		iTween.ScaleTo(gameObject, Vector3.zero, m_offTweenDuration);
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
		
		yield return new WaitForSeconds(m_offTweenDuration);
		
		Destroy(gameObject);
	}

	public float GetOnTweenDuration()
	{
		return m_onTweenDuration;
	}
}
