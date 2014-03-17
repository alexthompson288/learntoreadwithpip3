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
			m_spawnedButtons.Add(newButton.GetComponent<ClickEvent>() as ClickEvent);
			newButton.GetComponent<UIDragPanelContents>().draggablePanel = m_draggablePanel;
			newButton.GetComponentInChildren<UILabel>().text = lesson;
		} 
		yield return null;

		m_grid.Reposition();
	}

	void ChooseLesson(ClickEvent clickBehaviour)
	{
		int lessonIndex = m_spawnedButtons.IndexOf(clickBehaviour);

		if(lessonIndex < 0)
		{
			lessonIndex = 0;
		}

		EnviroManager.Instance.SetEnvironment(EnviroManager.Environment.Forest);
		LessonInfo.Instance.SetCurrentLesson(lessonIndex);
		LessonInfo.Instance.StartGames();
	}
}
