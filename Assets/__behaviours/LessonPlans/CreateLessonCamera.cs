using UnityEngine;
using System.Collections;

public class CreateLessonCamera : Singleton<CreateLessonCamera> 
{
	[SerializeField]
	private float m_tweenDuration = 0.5f;

	public void MoveToMenu(Transform menu)
	{
		menu.gameObject.SetActive(true);

		iTween.MoveTo(gameObject, menu.position, m_tweenDuration);
	}

	public float GetTweenDuration()
	{
		return m_tweenDuration;
	}
}
