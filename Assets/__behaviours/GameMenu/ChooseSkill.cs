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
		GameMenuCoordinator.Instance.OnChooseSkill(m_gameMenu, m_levelSkill);
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
