using UnityEngine;
using System.Collections;
using Wingrove;

public class ChallengeMenuCoordinator : Singleton<ChallengeMenuCoordinator> 
{
	[SerializeField]
	private string m_levelProgress;
	[SerializeField]
	private string m_starsProgress;

	IEnumerator Start ()
	{
		SkillProgressInformation.Instance.SetCurrentSkill(m_levelProgress);
		SkillProgressInformation.Instance.SetCurrentStarSkill(m_starsProgress);

		if(GetStarsProgress() >= SkillProgressInformation.Instance.GetStarsPerLevel())
		{
			//yield return new WaitForSeconds(StarBar.Instance.GetLevelUpDelay());

			SkillProgressInformation.Instance.IncrementProgress(m_levelProgress);
			SkillProgressInformation.Instance.SetProgress(m_starsProgress, 0);

			// Call level up functions
			//yield return StartCoroutine(StarBar.Instance.LevelUp());
			PipPlane.Instance.LevelUp();
		}

		yield return new WaitForSeconds(0.5f);

		WingroveAudio.WingroveRoot.Instance.PostEvent("CHOOSE_GAME");
	}

	public void Update()
	{
		if(Input.GetKeyDown(KeyCode.L))
		{
			Debug.Log("Setting var");
			SkillProgressInformation.Instance.SetProgress(m_starsProgress, 3);
			SkillProgressInformation.Instance.SetProgress(m_levelProgress, 1);
		}

		if(Input.GetKeyDown(KeyCode.J))
		{
			Debug.Log("Setting var");
			SkillProgressInformation.Instance.SetProgress(m_starsProgress, 2);
			SkillProgressInformation.Instance.SetProgress(m_levelProgress, 1);
		}
	}
	
	public int GetLevelProgress () 
	{
		return SkillProgressInformation.Instance.GetProgress(m_levelProgress);
	}

	public int GetStarsProgress()
	{
		return SkillProgressInformation.Instance.GetProgress(m_starsProgress);
	}
}
