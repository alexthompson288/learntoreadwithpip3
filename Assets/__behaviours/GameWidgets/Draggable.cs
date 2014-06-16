using UnityEngine;
using System.Collections;

public class Draggable : MonoBehaviour 
{
	public event Release OnRelease;
	public delegate void Release(Draggable draggable);

	[SerializeField]
	private AudioSource m_audioSource;

	Vector3 m_dragOffset;
	Vector3 m_startPosition;

	DataRow m_data;



	// Use this for initialization
	void Start () 
	{
		transform.localScale = Vector3.one;
		iTween.ScaleFrom(gameObject, Vector3.zero, 0.5f);
	}

	public void SetUp(DataRow data, AudioClip clip = null)
	{
		m_data = data;

		m_audioSource.clip = clip;
	}

	public DataRow GetData()
	{
		return m_data;
	}

	/*
	void OnClick()
	{
		if(enabled && m_audioSource.clip != null)
		{
			m_audioSource.Play();
		}
	}
	*/

	void OnPress(bool press)
	{
		if(enabled)
		{
			if (press)
			{
				if(m_audioSource.clip != null)
				{
					m_audioSource.Play();
				}

				iTween.Stop(gameObject);
					
				m_startPosition = transform.position;
				Ray camPos = UICamera.currentCamera.ScreenPointToRay(new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
				m_dragOffset = new Vector3(camPos.origin.x, camPos.origin.y, 0) - transform.position;
			}
			else
			{
				if(OnRelease != null)
				{
					OnRelease(this);
				}
			}
		}
	}
	
	void OnDrag(Vector2 dragAmount)
	{
		if(enabled)
		{
			Ray camPos = UICamera.currentCamera.ScreenPointToRay(
				new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
			transform.position = new Vector3(camPos.origin.x, camPos.origin.y, 0) - m_dragOffset;
				
			m_dragOffset = m_dragOffset - (Time.deltaTime * m_dragOffset);
		}
	}
	
	public void TweenToStartPos()
	{
		iTween.MoveTo(gameObject, m_startPosition, 0.5f);
	}
	
	public void TweenToPos(Vector3 newPos)
	{
		iTween.MoveTo(gameObject, newPos, 0.5f);
	}
	
	IEnumerator DestroyCoroutine()
	{
		collider.enabled = false;
		iTween.Stop(gameObject);
		iTween.ScaleTo(gameObject, Vector3.zero, 0.5f);
		
		yield return new WaitForSeconds(0.5f);
		
		Destroy(gameObject);
	}
	
	public void Off()
	{
		StartCoroutine(DestroyCoroutine());
	}
}
