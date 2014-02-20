using UnityEngine;
using System.Collections;

public class LessonNameCoordinator : Singleton<LessonNameCoordinator> 
{
	[SerializeField]
	private UILabel m_label;

	void Start()
	{
		m_label.text = LessonInfo.Instance.GetName();
		gameObject.SetActive(false);
	}

	public void OnInputFinish()
	{
		Debug.Log("OnInputFinish()");
		LessonInfo.Instance.SetName(m_label.text);
	}
}
