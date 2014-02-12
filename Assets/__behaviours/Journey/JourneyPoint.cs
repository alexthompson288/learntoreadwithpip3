using UnityEngine;
using System.Collections;
using Wingrove;
using System;

public class JourneyPoint : MonoBehaviour, IComparable
{
	[SerializeField]
	private UISprite m_starSprite;
	[SerializeField]
	private string m_onSpriteName;
	[SerializeField]
	private string m_offSpriteName;
	[SerializeField]
	private UITexture m_star;
	[SerializeField]
	private Texture2D m_starOn;
	[SerializeField]
	private Texture2D m_starOff;
	[SerializeField]
	private int m_sessionNum;
	[SerializeField]
	private bool m_largeStar;
	[SerializeField]
	private float m_shakeDuration = 0.5f;

	// Use this for initialization
	void Awake () 
	{
		if(m_sessionNum == -1)
		{
			Debug.LogError(name + "'s session = -1");
		}

		if(m_largeStar || m_sessionNum % 5 == 0)
		{
			transform.localScale *= 1.7f;
		}
	}

	IEnumerator Start ()
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		collider.enabled = true;

		CheckForCompletion();
	}

	IEnumerator Shake()
	{
		//yield return new WaitForSeconds(2f);
		yield return StartCoroutine(TransitionScreen.WaitForScreenExit());

		float starTweenDuration = 3.5f;
		iTween.PunchRotation(gameObject, new Vector3(0f, 0f, 720f), starTweenDuration * 1.5f);
		iTween.ShakePosition(gameObject, Vector3.one * 0.02f, starTweenDuration / 2);

		Vector3 originalScale = transform.localScale;
		iTween.ScaleTo(gameObject, originalScale * 2.5f, starTweenDuration);

		WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");

		yield return new WaitForSeconds(starTweenDuration / 3);

		WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");

		yield return new WaitForSeconds(starTweenDuration * 2 / 3);

		iTween.ScaleTo(gameObject, originalScale, 2.5f);

		yield return new WaitForSeconds(2.7f);

		JourneyCoordinator.Instance.SetActionComplete("StarShake");
	}

	void OnClick ()
	{
		Debug.Log("Clicked star " + m_sessionNum);
#if UNITY_EDITOR
		JourneyCoordinator.Instance.OnStarClick(m_sessionNum);
#else
		if(m_sessionNum <= JourneyInformation.Instance.GetSessionsCompleted() + 1 || JourneyCoordinator.Instance.AreAllUnlocked())
		{
			Debug.Log("Session is unlocked");
			JourneyCoordinator.Instance.OnStarClick(m_sessionNum);
		}
		else
		{
			Debug.Log("Session is locked");
			WingroveAudio.WingroveRoot.Instance.PostEvent("HAPPY_GAWP");
		}
#endif
	}

	void CheckForCompletion()
	{
		int sessionsCompleted = JourneyInformation.Instance.GetSessionsCompleted();

		if(m_sessionNum <= sessionsCompleted)
		{
			if(m_starSprite != null)
			{
				m_starSprite.spriteName = m_onSpriteName;
			}
			else
			{
				m_star.mainTexture = m_starOn;
			}

			if(m_sessionNum == sessionsCompleted)
			{
				StartCoroutine(Shake());
			}
		}
		else if(m_sessionNum == sessionsCompleted + 1)
		{
			JourneyPip.Instance.SetCurrentPoint(transform);
		}
	}

	public int CompareTo(System.Object other)
	{
		if(other == null)
		{
			return 1;
		}

		JourneyPoint otherPoint = other as JourneyPoint;

		if(m_sessionNum < otherPoint.m_sessionNum)
		{
			return - 1;
		}
		else if(m_sessionNum > otherPoint.m_sessionNum)
		{
			return 1;
		}
		else
		{
			return 0;
		}
	}

	public int GetSessionNum()
	{
		return m_sessionNum;
	}

	public void SetSessionNum(int sessionNum) // TODO: Delete - This is only necessary if you set the session numbers via editor scripts.
	{
		Debug.Log("SetSessionNum()");
		m_sessionNum = sessionNum;
	}
}
