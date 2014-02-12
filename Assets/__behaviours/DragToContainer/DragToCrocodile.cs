using UnityEngine;
using System.Collections;

public class DragToCrocodile : MonoBehaviour 
{
	[SerializeField]
	private TriggerTracker m_triggerTracker;
	[SerializeField]
	private UISprite m_crocodileSprite;
	[SerializeField]
	private SimpleSpriteAnim m_crocodileAnim;
	[SerializeField]
	private Transform m_mouthPosition;
	[SerializeField]
	private float m_minTimeBetweenIdle;
	[SerializeField]
	private float m_maxTimeBetweenIdle;

	public enum State
	{
		empty,
		open,
		full
	}

	public State m_currentState = State.empty;

	IEnumerator Start()
	{
		while(m_currentState < State.open)
		{
			yield return new WaitForSeconds(Random.Range(m_minTimeBetweenIdle, m_maxTimeBetweenIdle));
			m_crocodileAnim.PlayAnimation("EMPTY_IDLE");
			yield return null;
		}

		while(m_currentState < State.full)
		{
			m_crocodileSprite.spriteName = "crocclose001";
			yield return null;
		}

		while(true)
		{
			yield return new WaitForSeconds(Random.Range(m_minTimeBetweenIdle, m_maxTimeBetweenIdle));
			m_crocodileAnim.PlayAnimation("FULL_IDLE");
			yield return null;
		}
	}


	public bool IsTracking(GameObject go)
	{
		return m_triggerTracker.IsTracking(go);
	}

	public void EnableTrigger(bool enable)
	{
		collider.enabled = enable;
	}

	public void PlayOpen()
	{
		m_crocodileAnim.PlayAnimation("OPEN");
		m_currentState = State.open;
	}

	public void PlayClose()
	{
		m_crocodileAnim.PlayAnimation("CLOSE");
		m_currentState = State.full;
	}

	public Vector3 GetMouthPos()
	{
		return m_mouthPosition.position;
	}
}
