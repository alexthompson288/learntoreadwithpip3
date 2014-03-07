using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class GameMenuCoordinator : Singleton<GameMenuCoordinator> 
{
	[SerializeField]
	private GameObject m_camera;
	[SerializeField]
	private float m_cameraTweenDuration = 0.5f;
	[SerializeField]
	private GameObject m_skillMenu;
	[SerializeField]
	private GameObject m_numPlayerMenu;
	[SerializeField]
	private GameObject m_levelMenu;
	[SerializeField]
	private ClickEvent[] m_backButtons;
	[SerializeField]
	private GameObject m_chooseLevelPrefab;
	[SerializeField]
	private UIGrid m_levelMenuGrid;
	[SerializeField]
	private int m_numLevels = 19;
	
	GameObject m_currentGameMenu = null;

	string m_currentGame;

	List<ChooseLevel> m_levels = new List<ChooseLevel>();

	Vector3 m_levelMenuDefaultPos;
	bool m_isTwoPlayer;

	void Start()
	{
		GameDataBridge.Instance.SetContentType(GameDataBridge.ContentType.Sets);

		NavMenu.Instance.HideCallButton();

		m_levelMenuDefaultPos = m_levelMenu.transform.position;

		foreach(ClickEvent click in m_backButtons)
		{
			click.OnSingleClick += OnClickBack;
		}

		for(int i = 0; i < m_numLevels; ++i)
		{
			GameObject levelButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_chooseLevelPrefab, m_levelMenuGrid.transform);
			ChooseLevel level = levelButton.GetComponent<ChooseLevel>() as ChooseLevel;
			level.SetUp(i + 1);
			m_levels.Add(level);
		}

		m_levelMenuGrid.Reposition();
	}

	public void OnChooseSkill(GameObject gameMenu, string levelSkillName, string starSkillName)
	{
		m_currentGameMenu = gameMenu;

		SkillProgressInformation.Instance.SetCurrentSkill(levelSkillName);
		SkillProgressInformation.Instance.SetCurrentStarSkill(starSkillName);

		StartCoroutine(MoveCamera(m_skillMenu, m_currentGameMenu));
	}

	public void OnChooseGame(string game, bool isTwoPlayer)
	{
		m_currentGame = game;
		m_isTwoPlayer = isTwoPlayer;

		//int currentLevel = SkillProgressInformation.Instance.GetProgress(levelSkillName) + 1;
		int currentLevel = SkillProgressInformation.Instance.GetCurrentSkillProgress() + 1;
		foreach(ChooseLevel level in m_levels)
		{
			level.CheckUnlocked(currentLevel);
		}

		m_numPlayerMenu.SetActive(m_isTwoPlayer);

		Debug.Log("m_isTwoPlayer: " + m_isTwoPlayer);
		Debug.Log("m_numPlayerMenu.isActive: " + m_numPlayerMenu.activeInHierarchy);

		if(m_isTwoPlayer)
		{
			m_levelMenu.transform.position = m_levelMenuDefaultPos;
			StartCoroutine(MoveCamera(m_currentGameMenu, m_numPlayerMenu));
		}
		else
		{
			m_levelMenu.transform.position = m_numPlayerMenu.transform.position;
			StartCoroutine(MoveCamera(m_currentGameMenu, m_levelMenu));
		}
	}

	public void OnChooseNumPlayers(int numPlayers)
	{
		SessionInformation.Instance.SetNumPlayers(numPlayers);

		StartCoroutine(MoveCamera(m_numPlayerMenu, m_levelMenu));
	}

	public void OnChooseLevel(int setNum)
	{
		SkillProgressInformation.Instance.SetCurrentLevel(setNum);

		TransitionScreen.Instance.ChangeLevel(m_currentGame, false);
	}

	public void OnClickBack(ClickEvent click)
	{
		Debug.Log("OnClickBack()");
		if(TransformHelpers.ApproxPos(m_camera, m_levelMenu))
		{
			if(m_isTwoPlayer)
			{
				Debug.Log("From level to numPlayers");
				StartCoroutine(MoveCamera(m_levelMenu, m_numPlayerMenu));
			}
			else
			{
				Debug.Log("From level to gameMenu: " + m_currentGameMenu.name);
				StartCoroutine(MoveCamera(m_levelMenu, m_currentGameMenu));
				StartCoroutine(ReturnLevelMenuToDefaultPos());
			}
		}
		else if(TransformHelpers.ApproxPos(m_camera, m_numPlayerMenu))
		{
			Debug.Log("From numPlayers to gameMenu: " + m_currentGameMenu.name);
			StartCoroutine(MoveCamera(m_numPlayerMenu, m_currentGameMenu));
			StartCoroutine(ReturnLevelMenuToDefaultPos());
		}
		else
		{
			Debug.Log("From gameMenu to skillMenu");
			StartCoroutine(MoveCamera(m_currentGameMenu, m_skillMenu));
		}
	}

	// Always return the level menu to the default position of moving the camera back to the game menu
	public IEnumerator ReturnLevelMenuToDefaultPos()
	{
		yield return new WaitForSeconds(m_cameraTweenDuration);
		m_levelMenu.transform.position = m_levelMenuDefaultPos;
	}

	IEnumerator MoveCamera(GameObject from, GameObject to)
	{
		to.SetActive(true);
		iTween.MoveTo(m_camera, to.transform.position, m_cameraTweenDuration);
		
		yield return new WaitForSeconds(m_cameraTweenDuration);
		
		from.SetActive(false);
	}
}
