using UnityEngine;
using System.Collections;

public class BuyableGame : MonoBehaviour 
{
	[SerializeField]
	protected string m_gameSceneName;
	[SerializeField]
	private GameObject m_lock;
	[SerializeField]
	protected UITexture m_gameIcon;
	[SerializeField]
	private UISprite m_background;

	protected bool m_isUnlocked;

	protected virtual void Start()
	{
		StartCoroutine(StartCo());
	}

	IEnumerator StartCo()
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		Refresh();
	}

	public void Refresh()
	{
		Debug.Log("BuyableGame.Refresh()");

		m_isUnlocked = BuyManager.Instance.IsGameBought(m_gameSceneName);
		
		if(m_background != null)
		{
			m_background.color = m_isUnlocked ?  Color.white : BuyManager.Instance.GetDisabledColor();
		}

		if(m_gameIcon != null)
		{
			m_gameIcon.color = m_isUnlocked ? Color.white : BuyManager.Instance.GetDisabledColor(); 
		}

		if(m_lock != null)
		{
			m_lock.SetActive(!m_isUnlocked);
		}
	}

	public string GetGameSceneName()
	{
		return m_gameSceneName;
	}
}
