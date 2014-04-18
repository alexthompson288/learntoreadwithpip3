using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class FlashcardSetCoordinator : MonoBehaviour 
{
	[SerializeField]
	private GameObject m_flashcardSet;
	[SerializeField]
	private int m_numSets = 15;
	[SerializeField]
	private UIGrid m_grid;
	[SerializeField]
	private UIDraggablePanel m_draggablePanel;
	[SerializeField]
	private FlashcardCoordinator m_flashcardCoordinator;
	[SerializeField]
	private GameObject m_camera;
	[SerializeField]
	private float m_cameraTweenDuration = 0.5f;
	[SerializeField]
	private ClickEvent m_backButton;

	// Use this for initialization
	void Start () 
	{
		m_backButton.OnSingleClick += OnClickBack;

		NavMenu.Instance.HideCallButton();
		Game.SetSession(Game.Session.Single);
		
		for(int i = 0; i < m_numSets; ++i)
		{
			int setNum = i + 1;
			
			GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_flashcardSet, m_grid.transform);
			newButton.name = setNum.ToString();
			
			ClickEvent buttonBehaviour = newButton.GetComponentInChildren<ClickEvent>() as ClickEvent;
			buttonBehaviour.SetInt(setNum);
			buttonBehaviour.OnSingleClick += OnChooseSet;

			newButton.GetComponentInChildren<UILabel>().text = setNum.ToString();
		}
		
		m_grid.Reposition();
	}
	
	void OnChooseSet(ClickEvent click)
	{
		int setNum = click.GetInt();
		//SkillProgressInformation.Instance.SetCurrentLevel(setNum);

        GameManager.Instance.ClearAllData();
        GameManager.Instance.AddData("words", DataHelpers.GetSetData(setNum, "setwords", "words"));

		m_flashcardCoordinator.RefreshWordPool();

		iTween.MoveTo(m_camera, m_flashcardCoordinator.transform.position, m_cameraTweenDuration);

		StartCoroutine(m_flashcardCoordinator.RefreshPipPad());
	}

	void OnClickBack(ClickEvent click)
	{
		PipPadBehaviour.Instance.Hide();
		iTween.MoveTo(m_camera, transform.position, m_cameraTweenDuration);
	}
}
