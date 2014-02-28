using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Wingrove;

public class JourneyCoordinator : Singleton<JourneyCoordinator> 
{
	List<JourneyPoint> m_journeyPoints = new List<JourneyPoint>();

	[SerializeField]
	private GameObject[] m_maps;
	[SerializeField]
	private Transform m_botMapPos;
	[SerializeField]
	private Transform m_centreMapPos;
	[SerializeField]
	private Transform m_topMapPos;
	[SerializeField]
	private float m_mapTweenDuration = 0.5f;
	[SerializeField]
	private Collider m_mapCollider;
	[SerializeField]
	private AudioSource m_phonemeAudioSource;
	[SerializeField]
	private AudioSource m_ambientAudioSource;
	[SerializeField]
	private TweenReceiver m_storyTweenReceiver;
	[SerializeField]
	private Transform m_storyTweenFromPos;
	[SerializeField]
	private GameObject m_storyPrefab;
	[SerializeField]
	private Transform m_storyParent;
	[SerializeField]
	private ClickEvent[] m_arrows;

	List<GameObject> m_spawnedMaps = new List<GameObject>();
	
	[SerializeField]
	private bool m_allUnlocked;

	Dictionary<string, bool> m_actionsComplete = new Dictionary<string, bool>();

	bool m_starHasShook = false;

	JourneyMap m_centreMap = null;

	public void SetActionComplete(string action)
	{
		m_actionsComplete[action] = true;
	}

	public bool AreAllUnlocked()
	{
		return m_allUnlocked;
	}

	void Awake ()
	{
		Debug.Log("JourneyCoordinator.Awake()");

		foreach(ClickEvent arrow in m_arrows)
		{
			arrow.OnSingleClick += OnClickArrow;
		}

		m_actionsComplete.Add("StarShake", false);
		m_actionsComplete.Add("BookTween", false);
		m_actionsComplete.Add("CoinTween", false);
	}

	void OnClickArrow(ClickEvent click)
	{
		OnMapDrag(click.GetInt());
	}

	IEnumerator Start()
	{
		GameDataBridge.Instance.SetContentType(GameDataBridge.ContentType.Session);

		m_phonemeAudioSource.loop = false;
		m_ambientAudioSource.loop = true;

		while(JourneyMapCamera.Instance == null)
		{
			yield return null;
		}

		int sessionsCompleted = JourneyInformation.Instance.GetSessionsCompleted();
		//int sessionsCompleted = 0;

		Debug.Log("sessionsCompleted: " + sessionsCompleted);

		if(sessionsCompleted > 0)
		{
			for(int i = 0; i < m_maps.Length; ++i)
			{
				JourneyMap mapBehaviour = m_maps[i].GetComponent<JourneyMap>() as JourneyMap;

				if(sessionsCompleted >= mapBehaviour.GetLowestSessionNum() && sessionsCompleted <= mapBehaviour.GetHighestSessionNum())
				{
					Debug.Log("CurrentMap: " + m_maps[i].name);

					if(i > 0)
					{
						SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_maps[i - 1], m_topMapPos); // top map
					}

					SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_maps[i], m_centreMapPos); // centre map

					if(i < m_maps.Length - 1)
					{
						SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_maps[i + 1], m_botMapPos); // bot map
					}

					if(sessionsCompleted == mapBehaviour.GetHighestSessionNum()) // Spawn the previous map and then tween to the next one
					{
						StartCoroutine(EndMapDrag(1));
					}

					break;
				}
			}
		}
		else
		{
			SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_maps[0], m_centreMapPos); // centre map
			SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_maps[1], m_botMapPos); // bot map
		}

		m_centreMap = m_centreMapPos.GetChild(0).GetComponent<JourneyMap>() as JourneyMap;

		PlayAmbientAudio();

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		yield return new WaitForSeconds(2f);
		Debug.Log("Tweening items");

		if(JourneyInformation.Instance.HasRecentlyCompleted())
		{
			JourneyInformation.Instance.SetRecentlyCompleted(false);

			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programsessions WHERE number=" + sessionsCompleted);
			if(dt.Rows.Count > 0 && dt.Rows[0]["story_id"] != null)
			{
				//m_storyTweenReceiver.OnTweenFinish += OnStoryTweenFinish;
				//m_storyTweenReceiver.TweenObject(m_storyTweenFromPos);
				StartCoroutine(TweenStory());
			}
			else
			{
				m_actionsComplete["BookTween"] = true;
			}

			if(sessionsCompleted % 5 == 0) // End of unit
			{
				StartCoroutine(CelebrationCoordinator.Instance.Trumpet());
				StartCoroutine(CelebrationCoordinator.Instance.TrumpetOff());
			}
		}
		else
		{
			m_actionsComplete["BookTween"] = true;
			m_actionsComplete["CoinTween"] = true;
		}
	}

	IEnumerator TweenStory()
	{
		GameObject story = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_storyPrefab, m_storyParent);

		iTween.ScaleFrom(story, Vector3.zero, 0.5f);

		WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHINGS_APPEARS");

		yield return new WaitForSeconds(2.2f);

		iTween.ScaleTo(story, Vector3.zero, 0.5f);

		WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHINGS_DISAPPEARS");

		yield return new WaitForSeconds(0.6f);

		Destroy(story);
	}

	void OnStoryTweenFinish(TweenReceiver tweenBehaviour)
	{
		m_actionsComplete["BookTween"] = true;
		m_storyTweenReceiver.OnTweenFinish -= OnStoryTweenFinish;
	}

	IEnumerator EndMapDrag(float deltaY)
	{
		while(m_actionsComplete.ContainsValue(false))
		{
			yield return null;
		}

		OnMapDrag(deltaY);
	}

	public void OnMapDrag(float deltaY)
	{
		Debug.Log("deltaY: " + deltaY);
		if(deltaY > 0)
		{
			if(m_botMapPos.childCount > 0)
			{
				Debug.Log("Scrolling");

				StartCoroutine(DisableArrows());

				if(m_topMapPos.childCount > 0)
				{
					Destroy(m_topMapPos.GetChild(0).gameObject);
				}

				Transform newTopMap = m_centreMapPos.transform.GetChild(0);
				newTopMap.parent = m_topMapPos;
				iTween.MoveTo(newTopMap.gameObject, m_topMapPos.position, m_mapTweenDuration);

				Transform newCentreMap = m_botMapPos.transform.GetChild(0);
				newCentreMap.parent = m_centreMapPos;
				iTween.MoveTo(newCentreMap.gameObject, m_centreMapPos.position, m_mapTweenDuration);

				for(int i = 0; i < m_maps.Length; ++i)
				{
					if(m_maps[i].GetComponent<JourneyMap>().GetLowestSessionNum() == newCentreMap.GetComponent<JourneyMap>().GetLowestSessionNum() && i < m_maps.Length - 1)
					{
						StartCoroutine(SpawnNewMap(m_maps[i + 1], m_botMapPos));
						break;
					}
				}
			}
			else
			{
				Debug.Log("Reached end");
			}
		}
		else if(deltaY < 0)
		{
			if(m_topMapPos.childCount > 0)
			{
				Debug.Log("Scrolling");

				StartCoroutine(DisableArrows());

				if(m_botMapPos.childCount > 0)
				{
					Destroy(m_botMapPos.GetChild(0).gameObject);
				}
			
				Transform newBotMap = m_centreMapPos.transform.GetChild(0);
				newBotMap.parent = m_botMapPos;
				iTween.MoveTo(newBotMap.gameObject, m_botMapPos.position, m_mapTweenDuration);
			
				Transform newCentreMap = m_topMapPos.transform.GetChild(0);
				newCentreMap.parent = m_centreMapPos;
				iTween.MoveTo(newCentreMap.gameObject, m_centreMapPos.position, m_mapTweenDuration);

				Debug.Log("newCentreMap.name: " + newCentreMap.name);
				Debug.Log("newCentreMap.lowestSession: " + newCentreMap.GetComponent<JourneyMap>().GetLowestSessionNum());

				for(int i = 0; i < m_maps.Length; ++i)
				{
					if(m_maps[i].GetComponent<JourneyMap>().GetLowestSessionNum() == newCentreMap.GetComponent<JourneyMap>().GetLowestSessionNum() && i > 0)
					{
						StartCoroutine(SpawnNewMap(m_maps[i - 1], m_topMapPos));
						break;
					}
				}
			}
			else
			{
				Debug.Log("Reached end");
			}
		}

		m_centreMap = m_centreMapPos.GetChild(0).GetComponent<JourneyMap>() as JourneyMap;

		PlayAmbientAudio();
	}

	IEnumerator DisableArrows()
	{
		foreach(ClickEvent arrow in m_arrows)
		{
			arrow.gameObject.SetActive(false);
		}

		yield return new WaitForSeconds(m_mapTweenDuration);

		foreach(ClickEvent arrow in m_arrows)
		{
			arrow.gameObject.SetActive(true);
		}
	}

	void PlayAmbientAudio()
	{
		AudioClip centreAudio = m_centreMapPos.GetComponentInChildren<JourneyMap>().GetAudio();
		
		if(centreAudio != null && centreAudio != m_ambientAudioSource.clip)
		{
			m_ambientAudioSource.clip = centreAudio;
			m_ambientAudioSource.Play();
		}
	}

	IEnumerator SpawnNewMap(GameObject newMap, Transform mapParent)
	{
		Debug.Log("Spawn " + newMap.name + " at " + mapParent.name);
		m_mapCollider.enabled = false;
		yield return new WaitForSeconds(m_mapTweenDuration);

		SpawningHelpers.InstantiateUnderWithIdentityTransforms(newMap, mapParent);
		m_mapCollider.enabled = true;

	}

	public void AddJourneyPoint (JourneyPoint newPoint)
	{
		m_journeyPoints.Add(newPoint);
	}

	public void OnStarClick (int sessionNum) 
	{
		SessionManager.Instance.OnChooseSession(sessionNum);
		/*
		SqliteDatabase db = GameDataBridge.Instance.GetDatabase();
		
		DataTable dtSessions = db.ExecuteQuery("select * from programsessions WHERE number=" + sessionNum);

		Debug.Log("sessionNum: " + sessionNum);
		
		if(dtSessions.Rows.Count > 0)
		{
			JourneyInformation.Instance.SetCurrentSessionNum(sessionNum);

			List<DataRow> games = new List<DataRow>();
			
			int sessionId = Convert.ToInt32(dtSessions.Rows[0]["id"]);
			Debug.Log("sessionId: " + sessionId);
			DataTable dtSections = db.ExecuteQuery("select * from sections WHERE programsession_id=" + sessionId + " ORDER BY number");

			if(dtSections.Rows.Count > 0)
			{
				Debug.Log("There are " + dtSections.Rows.Count + " sections");

				List<DataRow> sections = dtSections.Rows;
				JourneyInformation.Instance.SetSections(sections);
				JourneyInformation.Instance.SetSectionsComplete(0);

				JourneyInformation.Instance.PlayNextGame();
			}
			else
			{
				JourneyInformation.Instance.SetSessionsCompleted(JourneyInformation.Instance.GetSessionsCompleted() + 1);
			}
		}
		else
		{
			JourneyInformation.Instance.SetSessionsCompleted(JourneyInformation.Instance.GetSessionsCompleted() + 1);
		}
		*/
	}

	public void OnClickMapCollider()
	{
		if(m_centreMap != null)
		{
			m_centreMap.OnClickMapCollider();
		}
	}

	public void OnClickLetter(DataRow data)
	{
		AudioClip phonemeAudio = AudioBankManager.Instance.GetAudioClip(data["grapheme"].ToString());

		if(phonemeAudio != null)
		{
			m_phonemeAudioSource.clip = phonemeAudio;
			m_phonemeAudioSource.Play();
		}
	}

	public void OnDoubleClickLetter(DataRow data)
	{
		AudioClip mnemonicAudio = LoaderHelpers.LoadMnemonic(data);

		if(mnemonicAudio != null)
		{
			m_phonemeAudioSource.clip = mnemonicAudio;
			m_phonemeAudioSource.Play();
		}

		Resources.UnloadUnusedAssets();
	}
}
