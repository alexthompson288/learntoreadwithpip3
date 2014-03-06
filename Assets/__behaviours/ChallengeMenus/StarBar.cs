using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class StarBar : MonoBehaviour
{
	[SerializeField]
	private ChooseSkill m_skill;
	[SerializeField]
	private GameObject m_starPrefab;
	[SerializeField]
	private string m_starOffString;
	[SerializeField]
	private string m_starOnString;
	[SerializeField]
	private Transform m_lowTransform;
	[SerializeField]
	private Transform m_highTransform;
	[SerializeField]
	private float m_onTweenDuration;
	[SerializeField]
	private UILabel m_numberLabel;
	
	List<GameObject> m_spawnedStars = new List<GameObject>();

	void Start ()
	{
		string levelSkill = m_skill.GetLevelSkill();
		string starSkill = m_skill.GetStarSkill();

		Debug.Log("levelSkill: " + levelSkill);
		Debug.Log("starSkill: " + starSkill);
		Debug.Log("StarProgress: " + SkillProgressInformation.Instance.GetProgress(starSkill));

		if(SkillProgressInformation.Instance.GetProgress(starSkill) >= SkillProgressInformation.Instance.GetStarsPerLevel())
		{
			SkillProgressInformation.Instance.IncrementProgress(levelSkill);
			SkillProgressInformation.Instance.SetProgress(starSkill, 0);
		}

		int starsEarned = SkillProgressInformation.Instance.GetProgress(starSkill);
		int starsTotal = SkillProgressInformation.Instance.GetStarsPerLevel();
		
		SpawnStars(starsEarned, starsTotal);
		
		m_numberLabel.text = (SkillProgressInformation.Instance.GetCurrentSkillProgress() + 1).ToString();
	}
	

	public IEnumerator LevelUp()
	{
		//yield return new WaitForSeconds(m_onTweenDuration);
		float tweenDuration = m_spawnedStars[0].GetComponent<Star>().GetOnTweenDuration();

		WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");

		foreach(GameObject star in m_spawnedStars)
		{
			StartCoroutine(star.GetComponent<Star>().Off());
		}
		m_spawnedStars.Clear();
		
		yield return new WaitForSeconds(tweenDuration + 0.2f);
		
		SetUp();
	}
	
	void SetUp()
	{
		int starsEarned = ChallengeMenuCoordinator.Instance.GetStarsProgress();
		int starsTotal = SkillProgressInformation.Instance.GetStarsPerLevel();

		SpawnStars(starsEarned, starsTotal);

		m_numberLabel.text = (SkillProgressInformation.Instance.GetCurrentSkillProgress() + 1).ToString();
	}

	void SpawnStars(int starsEarned, int starsTotal)
	{
		Vector3 delta = (m_highTransform.transform.localPosition - m_lowTransform.transform.localPosition)
			/ (starsTotal - 1);
		
		for (int index = 0; index < starsTotal; ++index)
		{
			GameObject newStar = SpawningHelpers.InstantiateUnderWithPrefabTransforms(m_starPrefab,
			                                                                          m_lowTransform);
			newStar.transform.localPosition = delta * index;
			m_spawnedStars.Add(newStar);
			
			if(index < starsEarned)
			{
				//iTween.PunchScale(newStar, new Vector3(1.5f, 1.5f, 1.5f), m_onTweenDuration);
				//iTween.PunchRotation(newStar, new Vector3(0f, 0f, 360f), m_onTweenDuration);
				m_spawnedStars[index].GetComponent<UISprite>().spriteName = m_starOnString;
			}
		}
	}

	public float GetLevelUpDelay()
	{
		return m_onTweenDuration;
	}
}
