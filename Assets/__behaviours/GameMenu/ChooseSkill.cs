using UnityEngine;
using System.Collections;

public class ChooseSkill : MonoBehaviour 
{
	[SerializeField]
	private string m_levelSkill;
	[SerializeField]
	private string m_starSkill;
	[SerializeField]
	private GameObject m_gameMenu;

	void OnClick()
	{
#if UNITY_IPHONE
		System.Collections.Generic.Dictionary<string, string> ep = new System.Collections.Generic.Dictionary<string, string>();
		ep.Add("SkillName: ", m_levelSkill);
		FlurryBinding.logEventWithParameters("GameMenu - Skill", ep, false);
#endif

		GameMenuCoordinator.Instance.OnChooseSkill(m_gameMenu, m_levelSkill, m_starSkill);
	}

	public string GetLevelSkill()
	{
		return m_levelSkill;
	}

	public string GetStarSkill()
	{
		return m_starSkill;
	}
}
