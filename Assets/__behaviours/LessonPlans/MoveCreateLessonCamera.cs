using UnityEngine;
using System.Collections;

public class MoveCreateLessonCamera : MonoBehaviour 
{
	[SerializeField]
	private float m_tweenDuration = 0.5f;
	[SerializeField]
	private GameObject m_camera;
	[SerializeField]
	private GameObject m_moveFrom;
	[SerializeField]
	private GameObject m_moveTo;

	void OnClick()
	{
		StartCoroutine(MoveCamera());
	}

	IEnumerator MoveCamera()
	{
		m_moveTo.SetActive(true);
		iTween.MoveTo(m_camera, m_moveTo.transform.position, m_tweenDuration);

		yield return new WaitForSeconds(m_tweenDuration);

		m_moveFrom.SetActive(false);
	}
}
