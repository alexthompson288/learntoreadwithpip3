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
			newButton.GetComponent<ClickEvent>().SingleClicked += ChooseLesson;
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

        GameManager.Instance.SetReturnScene(Application.loadedLevelName);

        // TODO: You need store gameIds LessonInfo
        //OrderedDictionary gameDictionary = new OrderedDictionary();
        //List<string> sceneList = LessonInfo.Instance.GetScenes();
        //foreach (string scene in sceneList)
        //{
           // gameDictionary.Add(scene, scene);
        //}
        //GameManager.Instance.AddGames(gameDictionary);
        //GameManager.Instance.AddGames(LessonInfo.Instance.GetScenes().ToArray());

        GameManager.Instance.AddData("phonemes", LessonInfo.Instance.GetData("phonemes"));
        GameManager.Instance.AddTargetData("phonemes", LessonInfo.Instance.GetTargetData("phonemes"));

        GameManager.Instance.AddData("words", LessonInfo.Instance.GetData("words"));
        GameManager.Instance.AddTargetData("words", LessonInfo.Instance.GetTargetData("words"));

        GameManager.Instance.AddData("keywords", LessonInfo.Instance.GetData("keywords"));
        GameManager.Instance.AddTargetData("keywords", LessonInfo.Instance.GetTargetData("keywords"));

        GameManager.Instance.AddData("stories", LessonInfo.Instance.GetData("stories"));

        GameManager.Instance.StartGames();
	}
}
