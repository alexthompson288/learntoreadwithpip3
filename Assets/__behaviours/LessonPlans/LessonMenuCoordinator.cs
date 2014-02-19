using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class LessonMenuCoordinator : MonoBehaviour 
{
	[SerializeField]
	private UIGrid m_grid;
	[SerializeField]
	private UIDraggablePanel m_draggablePanel;
	[SerializeField]
	private GameObject m_chooseLessonButtonPrefab;

	List<ClickEvent> m_spawnedButtons = new List<ClickEvent>();

	IEnumerator Start()
	{
		List<string> lessonNames = LessonInfo.Instance.GetLessonNames();

		foreach(string lesson in lessonNames)
		{
			GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_chooseLessonButtonPrefab, m_grid.transform);
			newButton.GetComponent<ClickEvent>().OnSingleClick += ChooseLesson;
			newButton.GetComponent<UIDragPanelContents>().draggablePanel = m_draggablePanel;
			newButton.GetComponentInChildren<UILabel>().text = lesson;
		} 
		yield return null;
	}

	void ChooseLesson(ClickEvent clickBehaviour)
	{
		LessonInfo.Instance.SetCurrentLesson(m_spawnedButtons.IndexOf(clickBehaviour));
	}
}
