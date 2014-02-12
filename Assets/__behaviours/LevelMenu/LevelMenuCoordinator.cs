using UnityEngine;
using System.Collections;
using Wingrove;
using System.Collections.Generic;

public class LevelMenuCoordinator : Singleton<LevelMenuCoordinator> 
{
	[SerializeField]
	private GameObject m_setButtonPrefab;
	[SerializeField]
	private UIGrid m_setGrid;
	[SerializeField]
	private Collider m_setGridCollider;
	[SerializeField]
	private TweenOnOffBehaviour[] m_playerNumberSelections;
	[SerializeField]
	private TweenOnOffBehaviour m_setSelection;
	[SerializeField]
	private int m_maxLevel = 16;
	[SerializeField]
	private UIDraggableCamera m_draggableCamera;
	[SerializeField]
	private List<LevelMenuSetButton> m_spawnedButtons = new List<LevelMenuSetButton>();

	IEnumerator Start()
	{
		for(int i = m_maxLevel; i > 0; --i)
		{
			GameObject newSetButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_setButtonPrefab, m_setGrid.transform);
			
			newSetButton.GetComponent<LevelMenuSetButton>().SetUp(i);
			m_spawnedButtons.Add(newSetButton.GetComponent<LevelMenuSetButton>() as LevelMenuSetButton);
			newSetButton.GetComponent<UIDragCamera>().draggableCamera = m_draggableCamera;
		}

		m_setGrid.Reposition();

		yield return new WaitForSeconds(0.1f);

		MoveDraggableCameraToSetButton();

		yield return null;
		SessionInformation.Instance.SetRetryScene(Application.loadedLevelName);
		if (!SessionInformation.Instance.SupportsTwoPlayer())
		{
			Debug.Log("Doesn't support 2 players");
			foreach (TweenOnOffBehaviour two in m_playerNumberSelections)
			{
				two.gameObject.SetActive(false);
				two.Off();
			}
			SessionInformation.Instance.SetNumPlayers(1);
			StartCoroutine(ShowDifficultySelect(0.0f));
		}
	}

	public void MoveDraggableCameraToSetButton()
	{
		int highestUnlockedSet = SkillProgressInformation.Instance.GetCurrentSkillProgress() + 1;

		foreach(LevelMenuSetButton button in m_spawnedButtons)
		{
			if(button.GetSetNum() == highestUnlockedSet)
			{
				m_draggableCamera.transform.position = button.transform.position;
				break;
			}
		}
	}
	
	public void SelectPlayerCount(int playerCount)
	{
		SessionInformation.Instance.SetNumPlayers(playerCount);
		foreach (TweenOnOffBehaviour two in m_playerNumberSelections)
		{
			two.Off();
		}
		StartCoroutine(ShowDifficultySelect(1.5f));
	}

	IEnumerator ShowDifficultySelect(float delay)
	{
		yield return new WaitForSeconds(delay);
		WingroveAudio.WingroveRoot.Instance.PostEvent("DIFFICULTY_SELECT");
		m_setSelection.On();
		m_setGridCollider.enabled = true;

		yield return new WaitForSeconds(m_setSelection.GetDuration());

		int highestUnlockedSet = SkillProgressInformation.Instance.GetCurrentSkillProgress() + 1;

		foreach(LevelMenuSetButton button in m_spawnedButtons)
		{
			if(button.GetSetNum() == highestUnlockedSet)
			{
				//if(SkillProgressInformation.Instance.IsCurrentSkillRecentlyLeveled())
				if("HELLO".ToLower() == "hello")
				{
					StartCoroutine(button.Unlock());
				}
				break;
			}
		}
	}

	public void SelectSet(int set)
	{
		SkillProgressInformation.Instance.SetCurrentLevel(set);

		SessionInformation.Instance.SetDifficulty(Mathf.Clamp(set, 0, 2));

		m_setSelection.Off();
		
		Invoke("ChangeLevel", 1.5f);
	}

	public void SelectCustom()
	{
		ContentInformation.Instance.SetUseCustom(true);
		
		m_setSelection.Off();
		
		Invoke("ChangeLevel", 1.5f);
	}

	public void ChangeLevel()
	{
		TransitionScreen.Instance.ChangeLevel(SessionInformation.Instance.GetSelectedGame(), false);
	}
}
