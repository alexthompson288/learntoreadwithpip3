using UnityEngine;
using System.Collections;

public class ChooseLevel : MonoBehaviour 
{
	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private UISprite m_sprite;
	[SerializeField]
	private Color m_lockedColor;
	[SerializeField]
	private Color m_unlockedColor;

	int m_setNum;
	bool m_isUnlocked;

	public void SetUp(int setNum)
	{
		m_setNum = setNum;
		m_label.text = m_setNum.ToString();
	}

	void OnClick()
	{
		//if(m_isUnlocked)
		//{
#if UNITY_IPHONE
			System.Collections.Generic.Dictionary<string, string> ep = new System.Collections.Generic.Dictionary<string, string>();
			ep.Add("SetNum: ", m_setNum.ToString());
			FlurryBinding.logEventWithParameters("GameMenu - Level", ep, false);
#endif

			OldGameMenuCoordinator.Instance.OnChooseLevel(m_setNum);
		//}
	}

	public void CheckUnlocked(string skillName)
	{
		m_isUnlocked = m_setNum <= SkillProgressInformation.Instance.GetProgress(skillName) + 1;

		m_sprite.color = m_isUnlocked ? m_unlockedColor : m_lockedColor;
	}

	public void CheckUnlocked(int currentLevel)
	{
		m_isUnlocked = m_setNum <= currentLevel;
		m_sprite.color = m_isUnlocked ? m_unlockedColor : m_lockedColor;
	}


}
