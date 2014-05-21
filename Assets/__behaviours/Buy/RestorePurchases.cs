using UnityEngine;
using System.Collections;

public class RestorePurchases : Singleton<RestorePurchases>
{
	[SerializeField]
	private GameObject m_restoreDisplayParent;
	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private float m_restoreDuration = 20f;

	void Start()
	{
		m_restoreDisplayParent.SetActive(false);
	}

	void OnClick()
	{
		//StartCoroutine(BuyManager.Instance.RestorePurchases(m_restoreDuration));

		//m_restoreDisplayParent.SetActive(true);
		//StartCoroutine(WaitForRestore(m_restoreDuration));
	}

	IEnumerator WaitForRestore(float timeLeft)
	{
		bool restore = true;

		while(restore)
		{
			timeLeft -= Time.deltaTime;
			
			int timeElapsed = Mathf.RoundToInt(timeLeft);
			
			m_label.text = timeElapsed.ToString();

			if(timeLeft <= 0)
			{
				restore = false;
			}

			yield return null;
		}

		Off ();
	}

	public void Off()
	{
		m_restoreDisplayParent.SetActive(false);
	}
}
