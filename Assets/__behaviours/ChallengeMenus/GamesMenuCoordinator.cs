using UnityEngine;
using System.Collections;

public class GamesMenuCoordinator : MonoBehaviour 
{
	[SerializeField]
	private GameObject m_camera;
	[SerializeField]
	private float m_cameraTweenDuration = 0.8f;
	[SerializeField]
	private Transform[] m_menus;

	int m_menuIndex = 0;

	void OnLeftArrowClick(ClickEvent click)
	{
		MoveCamera(-1);
	}

	void OnRightArrowClick(ClickEvent click)
	{
		MoveCamera(1);
	}

	void MoveCamera(int direction)
	{
		m_menuIndex += direction;

		if(m_menuIndex < 0)
		{
			m_menuIndex = m_menus.Length - 1;
		}
		else if(m_menuIndex >= m_menus.Length)
		{
			m_menuIndex = 0;
		}

		iTween.MoveTo(m_camera, m_menus[m_menuIndex].position, m_cameraTweenDuration);

		GamesMenuSkill skill = m_menus[m_menuIndex].GetComponent<GamesMenuSkill>() as GamesMenuSkill;
		SkillProgressInformation.Instance.SetCurrentSkill(skill.GetLevelProgressName());
		SkillProgressInformation.Instance.SetCurrentStarSkill(skill.GetStarProgressName());

		StartCoroutine(CheckLevelUp(m_cameraTweenDuration + 0.3f));


	}

	IEnumerator CheckLevelUp(float delay = 0)
	{
		yield return new WaitForSeconds(delay);

			/*
			SkillProgressInformation.Instance.GetProgress(m_starsProgress);

			if(GetStarsProgress() >= SkillProgressInformation.Instance.GetStarsPerLevel())
			{
				yield return new WaitForSeconds(StarBar.Instance.GetLevelUpDelay());
				
				SkillProgressInformation.Instance.IncrementProgress(m_levelProgress);
				SkillProgressInformation.Instance.SetProgress(m_starsProgress, 0);
				
				// Call level up functions
				yield return StartCoroutine(StarBar.Instance.LevelUp());
				PipPlane.Instance.LevelUp();
			}
			*/
	}


}


