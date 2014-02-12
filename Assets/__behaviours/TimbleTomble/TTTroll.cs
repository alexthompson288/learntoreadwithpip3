using UnityEngine;
using System.Collections;

public class TTTroll : MonoBehaviour 
{
	[SerializeField]
	private Transform m_mouth;
	[SerializeField]
	private float m_scaleDuration;
	[SerializeField]
	private SimpleSpriteAnim m_animBehaviour;

	bool m_isOn;

	public void StartOff () // Awake was getting called twice for some reason. Unity bug?
	{
		transform.localScale = Vector3.zero;
		m_isOn = false;
		gameObject.SetActive(false);
	}

	public void On () 
	{
		if(!m_isOn)
		{
			gameObject.SetActive(true);
			m_animBehaviour.PlayAnimation("IDLE");
			TTCoordinator.Instance.AddMouthPosition(m_mouth);
			TweenScale.Begin(gameObject, m_scaleDuration, Vector3.one);
			m_isOn = true;
		}
	}

	public void Off ()
	{
		if(m_isOn && gameObject.activeInHierarchy)
		{
			TTCoordinator.Instance.RemoveMouthPosition(m_mouth);
			m_isOn = false;
			StartCoroutine(OffCo());
		}
	}

	IEnumerator OffCo()
	{
		TweenScale.Begin(gameObject, m_scaleDuration, Vector3.zero);

		yield return new WaitForSeconds(m_scaleDuration);

		gameObject.SetActive(false);
	}

	public void Burp ()
	{
		m_animBehaviour.PlayAnimation("BURP");
	}
}
