using UnityEngine;
using System.Collections;

public class LevelMenuSetButton : MonoBehaviour 
{
	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private UISprite m_lockedBackground;
	[SerializeField]
	private UISprite m_background;
	[SerializeField]
	private GameObject m_lock;
	[SerializeField]
	private string m_completedOffSpriteName;
	[SerializeField]
	private string m_completedOnSpriteName;
	[SerializeField]
	private string m_currentOffSpriteName;
	[SerializeField]
	private string m_currentOnSpriteName;

	int m_setNum;

	public void SetUp (int setNum) 
	{
		m_setNum = setNum;

		m_label.text = "Level " + m_setNum.ToString();

		if(m_setNum > SkillProgressInformation.Instance.GetCurrentSkillProgress() + 1)
		{
			m_lock.SetActive(true);
			m_lockedBackground.gameObject.SetActive(true);
		}
		/*
		else if(m_setNum == SkillProgressInformation.Instance.GetCurrentSkillProgress() + 1)
		{
			if(SkillProgressInformation.Instance.IsCurrentSkillRecentlyLeveled())
			{
				m_lock.SetActive(true);
			}
		}
		*/
	}

	void OnClick()
	{
		Game.SetSession(Game.Session.Single);
		LevelMenuCoordinator.Instance.SelectSet(m_setNum);
	}

	public int GetSetNum() // TODO: Delete this method. Only used for debugging
	{
		return m_setNum;
	}

	public IEnumerator Unlock()
	{
		m_lock.SetActive(true);
		m_lockedBackground.gameObject.SetActive(true);

		m_background.spriteName = m_currentOffSpriteName;

		iTween.ScaleTo(gameObject, Vector3.one * 1.2f, 1.0f);
		//Vector3 pos = transform.position;
		//Vector3 tPos = pos * 0.5f;
		//iTween.MoveTo(gameObject, tPos, 1.0f);
		
		foreach (UIWidget widget in GetComponentsInChildren<UIWidget>(true))
		{
			widget.depth += 5;
		}
		
		yield return new WaitForSeconds(1.0f);
		
		CharacterPopper cPop = (CharacterPopper)GameObject.FindObjectOfType(typeof(CharacterPopper));
		if (cPop != null)
		{
			cPop.PopCharacter();
		}
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("SFX_SPARKLE");
		iTween.RotateBy(m_lockedBackground.gameObject, new Vector3(0, 0, Random.Range(-2.0f, 2.0f)), 2.0f);
		iTween.RotateBy(m_lock, new Vector3(0, 0, Random.Range(-2.0f, 2.0f)), 2.0f);
		yield return new WaitForSeconds(0.1f);
		iTween.MoveTo(m_lockedBackground.gameObject, m_lockedBackground.transform.position + (Random.onUnitSphere * 1.5f), 2.0f);
		iTween.MoveTo(m_lock, m_lockedBackground.transform.position + (Random.onUnitSphere * 1.5f), 2.0f);
		yield return new WaitForSeconds(0.1f);
		TweenAlpha.Begin(m_lockedBackground.gameObject, 1.0f, 0.0f);
		TweenAlpha.Begin(m_lock, 1.0f, 0.0f);
		//m_unlockedHierarchy.SetActive(true);
		/*
		foreach (Transform t in m_starSprites)
		{
			t.localScale = Vector3.zero;
		}
		foreach (Transform t in m_starSprites)
		{
			iTween.ScaleTo(t.gameObject, Vector3.one * 2.0f, 0.75f);
			yield return new WaitForSeconds(0.3f);
		}
		*/
		iTween.ScaleTo(gameObject, Vector3.one, 1.0f);
		//iTween.MoveTo(gameObject, pos, 1.0f);
		yield return new WaitForSeconds(1.0f);
		
		foreach (UIWidget widget in GetComponentsInChildren<UIWidget>(true))
		{
			widget.depth -= 5;
		}

		m_lockedBackground.enabled = false;
		m_lock.SetActive(false);
		yield break;
	}
}
