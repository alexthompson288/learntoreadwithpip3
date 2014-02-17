using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class FlashcardMapCoordinator : MonoBehaviour 
{
	[SerializeField]
	private GameObject m_setButtonPrefab;
	[SerializeField]
	private int m_numSets = 15;
	[SerializeField]
	private UIGrid m_grid;
	[SerializeField]
	private UIDraggablePanel m_draggablePanel;
	
	static string m_flashcardGame;
	public static void SetFlashcardGame(string flashcardGame)
	{
		m_flashcardGame = flashcardGame;
	}

	static bool m_leveledUp;
	public static void LevelUp()
	{
		m_leveledUp = true;
	}
	
	IEnumerator Start () 
	{
		GameDataBridge.Instance.SetContentType(GameDataBridge.ContentType.Sets);
		SkillProgressInformation.Instance.SetCurrentSkill("Flashcard");

		if(System.String.IsNullOrEmpty(m_flashcardGame))
		{
			m_flashcardGame = "NewFlashcardPractice";
		}

		int maxLevel = SkillProgressInformation.Instance.GetCurrentSkillProgress() + 1;

		List<FlashcardSetButton> buttons = new List<FlashcardSetButton>();

		for(int i = 0; i < m_numSets; ++i)
		{
			int setNum = i + 1;

			GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_setButtonPrefab, m_grid.transform);
			newButton.name = setNum.ToString();

			bool isUnlocked = (m_flashcardGame == "NewFlashcardPractice" || setNum < maxLevel || setNum == 1);

			FlashcardSetButton buttonBehaviour = newButton.GetComponentInChildren<FlashcardSetButton>() as FlashcardSetButton;
			buttonBehaviour.SetUp(isUnlocked, setNum, m_draggablePanel);
			buttonBehaviour.OnSingleClick += OnButtonClick;
			buttons.Add(buttonBehaviour);
		}

		m_grid.Reposition();

		yield return StartCoroutine(TransitionScreen.WaitForScreenExit());

		if(maxLevel > 1 && m_flashcardGame == "NewFlashcardPlay")
		{
			Debug.Log("LEVEL UP!");

			foreach(FlashcardSetButton button in buttons)
			{
				if(button.GetSetNum() == maxLevel)
				{
					m_draggablePanel.enabled = false;
					yield return StartCoroutine(button.Unlock());
					m_draggablePanel.enabled = true;
				}
			}

			StartCoroutine(CelebrationCoordinator.Instance.LevelUp(maxLevel, 1f));
			StartCoroutine(CelebrationCoordinator.Instance.RainLettersThenFall());

		}

		m_leveledUp = false;

		yield break;
	}

	void OnButtonClick(bool isUnlocked, int setNum)
	{
		Debug.Log("FlashcardMapCoordinator.OnButtonClick()");
		if(isUnlocked)
		{
			SkillProgressInformation.Instance.SetCurrentLevel(setNum);
			Debug.Log("Flashcard Game: " + m_flashcardGame);
			TransitionScreen.Instance.ChangeLevel(m_flashcardGame, false);
		}
	}
}
