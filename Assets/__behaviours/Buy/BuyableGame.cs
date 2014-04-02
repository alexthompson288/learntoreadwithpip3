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
	[SerializeField]
	private GameObject m_multiIcon;
	[SerializeField]
	protected bool m_isTwoPlayer = false;

	protected bool m_isUnlocked;

#if UNITY_STANDALONE
    void Awake()
    {
        m_isTwoPlayer = false;
    }
#endif

	protected virtual void Start()
	{
		if(!m_isTwoPlayer && m_multiIcon != null)
		{
			m_multiIcon.SetActive(false);
		}

		StartCoroutine(StartCo());
	}

	IEnumerator StartCo()
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		Refresh();
	}

	public void Refresh()
	{
		m_isUnlocked = BuyInfo.Instance.IsGameBought(m_gameSceneName);
		
		if(m_background != null)
		{
			m_background.color = m_isUnlocked ?  Color.white : BuyManager.Instance.unbuyableColor;
		}

		if(m_gameIcon != null)
		{
			m_gameIcon.color = m_isUnlocked ? Color.white : BuyManager.Instance.unbuyableColor; 
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
