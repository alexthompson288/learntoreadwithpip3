using UnityEngine;
using System.Collections;
using Wingrove;
using System;

public class JourneyPoint : MonoBehaviour, IComparable
{
	[SerializeField]
	private float m_dragThreshold = 10;
	[SerializeField]
	private UISprite m_pointBackground;
	[SerializeField]
	private int m_sessionNum;
	[SerializeField]
	private bool m_largeStar;
	[SerializeField]
	private float m_shakeDuration = 0.5f;
	[SerializeField]
	private int m_phonemeId = -1;
	[SerializeField]
	private UILabel m_graphemeLabel;
	[SerializeField]
	private UITexture m_mnemonicTexture;
	[SerializeField]
	private string m_mainColorString = "[333333]";
	[SerializeField]
	private string m_highlightColorString = "[FF0000]";

	bool m_mapIsBought = false;
	
	private float m_totalDeltaY = 0;
	
	private bool m_canDrag = true;

	// Use this for initialization
	void Awake () 
	{
		if(m_sessionNum == -1)
		{
			Debug.LogError(name + "'s session = -1");
		}

		if(m_pointBackground == null)
		{
			m_pointBackground = GetComponentInChildren<UISprite>() as UISprite;
		}
		
		if(m_graphemeLabel == null)
		{
			m_graphemeLabel = GetComponentInChildren<UILabel>() as UILabel;
		}
		
		if(m_mnemonicTexture == null)
		{
			m_mnemonicTexture = GetComponentInChildren<UITexture>() as UITexture;
		}

		/*
		if(m_phonemeId != -1)
		{
			m_pointBackground.transform.localScale *= 1.7f;
		}
		else if(m_largeStar || m_sessionNum % 5 == 0)
		{
			transform.localScale *= 1.7f;
		}
		*/
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
		if(m_canDrag)
		{
			Debug.Log("Clicked star " + m_sessionNum);
			if(m_mapIsBought)
			{
#if UNITY_IPHONE
				System.Collections.Generic.Dictionary<string, string> ep = new System.Collections.Generic.Dictionary<string, string>();
				ep.Add("CurrentSession", m_sessionNum.ToString());
				ep.Add("HighestSession", JourneyInformation.Instance.GetSessionsCompleted().ToString());
				FlurryBinding.logEventWithParameters("StartVoyageSession", ep, true);
#endif

				//JourneyCoordinator.Instance.OnStarClick(m_sessionNum);
				SessionManager.Instance.SetSessionType(SessionManager.ST.Voyage);
				JourneyInformation.Instance.SubscribeOnSessionComplete();
				SessionManager.Instance.OnChooseSession(m_sessionNum);
			}
			else
			{
				JourneyCoordinator.Instance.OnClickMapCollider();
			}
		}
	}

	void OnPress (bool pressed) 
	{
		if(!pressed)
		{
			if(m_canDrag)
			{
				JourneyCoordinator.Instance.OnClickMapCollider();
			}
			
			m_canDrag = true;
			m_totalDeltaY = 0;
		}
	}
	
	void OnDrag (Vector2 delta)
	{
		if(m_canDrag)
		{
			m_totalDeltaY += delta.y;
			
			if(Mathf.Abs(m_totalDeltaY) > m_dragThreshold)
			{
				JourneyCoordinator.Instance.OnMapDrag(m_totalDeltaY);
				m_canDrag = false;
			}
		}
	}

	void CheckForCompletion()
	{
		if(m_phonemeId != -1)
		{
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id=" + m_phonemeId);
			
			if(dt.Rows.Count > 0)
			{
				DataRow data = dt.Rows[0];
				
				string spriteName = AlphabetBookInformation.Instance.GetTexture(System.Convert.ToInt32(data["id"]));
				if(spriteName != null)
				{
					m_pointBackground.spriteName = spriteName;
				}
				else
				{
					m_mnemonicTexture.color = Color.black;
					m_graphemeLabel.color = Color.black;
					m_pointBackground.spriteName = "icon_circle_white";
				}
				
				string graphemeText = data["phoneme"].ToString();
				m_graphemeLabel.text = graphemeText;
				
				if(graphemeText.Length > 2)
				{
					m_graphemeLabel.transform.parent.localScale = Vector3.one * 0.35f;
				}
				else if(graphemeText.Length > 1)
				{
					m_graphemeLabel.transform.parent.localScale = Vector3.one * 0.4f;
				}
				
				string mnemonicText = data["mneumonic"].ToString();
				
				string imageFilename =
					string.Format("Images/mnemonics_images_png_250/{0}_{1}",
					              graphemeText,
					              mnemonicText.Replace(" ", "_"));

				Texture2D tex = (Texture2D)Resources.Load(imageFilename);
				if(tex != null)
				{
					m_mnemonicTexture.mainTexture = tex;
				}
				else
				{
					m_mnemonicTexture.enabled = false;
				}
			}
		}
		else
		{
			m_mnemonicTexture.gameObject.SetActive(false);
			m_graphemeLabel.gameObject.SetActive(false);

			if(JourneyInformation.Instance.IsSessionFinished(m_sessionNum))
			{
				BetterList<string> spriteNames = m_pointBackground.atlas.GetListOfSprites();
				m_pointBackground.spriteName = spriteNames[UnityEngine.Random.Range(1, spriteNames.size)];
			}
			else
			{
				transform.localScale *= 0.8f;
			}
		}

		if(m_sessionNum == JourneyInformation.Instance.GetSessionsCompleted() + 1)
		{
			Debug.Log("Current Point: " + m_sessionNum);
			JourneyPip.Instance.SetCurrentPoint(transform);
		}
	}

	/*
	void CheckForCompletion()
	{
		int sessionsCompleted = JourneyInformation.Instance.GetSessionsCompleted();

		if(m_sessionNum <= sessionsCompleted)
		{
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
	*/

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

	public void SetMapBought()
	{
		m_mapIsBought = true;
	}
}
