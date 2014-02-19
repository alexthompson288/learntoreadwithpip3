using UnityEngine;
using System.Collections;

public class LessonMenuCoordinator : MonoBehaviour 
{
	[SerializeField]
	private UIGrid m_grid;
	[SerializeField]
	private UIDraggablePanel m_draggablePanel;

	IEnumerator Start()
	{
		yield return null;
	}
}
