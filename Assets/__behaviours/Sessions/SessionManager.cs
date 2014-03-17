using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class SessionManager : Singleton<SessionManager> 
{
	enum State 
	{
		Sleep,
		Waiting,
		StartGame
	}

	void OnLevelWasLoaded(int level)
	{
		switch (m_state) 
		{
		case State.Sleep:
			break;

		case State.Waiting:
			m_state = State.Sleep;
			if(OnSessionCancel != null)
			{
				OnSessionCancel();
			}
			break;

		case State.StartGame:
			m_state = State.Waiting;
			break;
		}
	}

	State m_state;

	float m_timeSessionStarted;

	public float GetTimeSessionStarted()
	{
		return m_timeSessionStarted;
	}

	// TODO: Need OnSessionCancel
	public delegate void SessionCancel();
	public event SessionCancel OnSessionCancel;

	public delegate void SessionComplete();
	private SessionComplete onSessionComplete;
	public event SessionComplete OnSessionComplete
	{
		add
		{
			if(onSessionComplete == null || !onSessionComplete.GetInvocationList().Contains(value))
			{
				onSessionComplete += value;
			}
		}
		remove
		{
			onSessionComplete -= value;
		}
	}


	
	List<DataRow> m_sections = new List<DataRow>();
	int m_sectionsComplete;

	int m_sessionNum;

	ST m_st;

	public enum ST //SessionType
	{
		Voyage,
		Pippisode
	}

	public void SetSessionType(ST type)
	{
		m_st = type;
	}

	public int GetSessionNum()
	{
		return m_sessionNum;
	}

	public void OnChooseSession(int sessionNum)
	{
		GameDataBridge.Instance.SetContentType(GameDataBridge.ContentType.Session);

		m_sections.Clear();

		SqliteDatabase db = GameDataBridge.Instance.GetDatabase();

		DataTable dtTest = db.ExecuteQuery("select * from programsessions ORDER BY number DESC");

		dtTest = db.ExecuteQuery("select * from programsessions WHERE id=" + 280);
		if(dtTest.Rows.Count > 0)
		{
			Debug.Log("2-FOUND: " + dtTest.Rows[0]["number"].ToString());
		}
		else
		{
			Debug.Log("Couldn't find by id");
		}
		
		DataTable dtSessions = db.ExecuteQuery("select * from programsessions WHERE number=" + sessionNum);
		
		Debug.Log("sessionNum: " + sessionNum);

		m_timeSessionStarted = Time.time;
		
		if(dtSessions.Rows.Count > 0)
		{
			m_sessionNum = sessionNum;
			
			List<DataRow> games = new List<DataRow>();
			
			int sessionId = Convert.ToInt32(dtSessions.Rows[0]["id"]);
			Debug.Log("sessionId: " + sessionId);
			DataTable dtSections = db.ExecuteQuery("select * from sections WHERE programsession_id=" + sessionId + " ORDER BY number");
			
			if(dtSections.Rows.Count > 0)
			{
				Debug.Log("There are " + dtSections.Rows.Count + " sections");
				
				m_sections = dtSections.Rows;

				Debug.Log("Printing game names");
				foreach(DataRow section in m_sections)
				{
					DataRow game = DataHelpers.FindGameForSection(section);
					if(game != null)
					{
						Debug.Log(game["name"].ToString());
					}
					else
					{
						Debug.Log("No game for section " + section["id"].ToString());
					}
				}

				m_sectionsComplete = 0;

				PlayNextGame();
			}
			else // Session empty
			{
				Debug.Log("Session has no sections");
				CompleteSession();
			}
		}
		else // No session found
		{
			Debug.Log("No session found");
			CompleteSession();
		}
	}

	public DataRow GetCurrentSection()
	{
		Debug.Log("m_sectionsComplete: " + m_sectionsComplete);
		
		if(m_sectionsComplete < m_sections.Count)
		{
			Debug.Log("Current Section: " + m_sections[m_sectionsComplete]["id"].ToString());
			
			return m_sections[m_sectionsComplete];
		}
		else
		{
			Debug.LogError("There are not enough sections in m_sections");
			return null;
		}
	}

	public int GetCurrentSectionId()
	{
		Debug.Log("m_sectionsComplete: " + m_sectionsComplete);
		if(m_sectionsComplete < m_sections.Count)
		{
			Debug.Log("current sectionId: " + m_sections[m_sectionsComplete]["id"].ToString());
			return System.Convert.ToInt32(m_sections[m_sectionsComplete]["id"]);
		}
		else
		{
			Debug.LogError("There are not enough sections in m_sections");
			return -1;
		}
	}
	
	public void OnGameFinish(bool wonGame = true)
	{
		Debug.Log("OnGameFinish()");
		if(wonGame)
		{
			++m_sectionsComplete;
		}
		
		PlayNextGame();
	}
	
	void PlayNextGame()
	{
		string gameName = null;
		
		Debug.Log("Sections Complete: " + m_sectionsComplete);
		for(int i = m_sectionsComplete; i < m_sections.Count; ++i)
		{
			Debug.Log("Finding game for sectionId: " + m_sections[i]["id"].ToString());
			
			DataRow game = DataHelpers.FindGameForSection(m_sections[i]);
			
			if(game != null)
			{
				string dbGameName = game["name"].ToString();
				
				Debug.Log("Found dbGame: " + dbGameName);
				
				if(GameLinker.Instance.IsDBGame(dbGameName))
				{
					gameName = GameLinker.Instance.GetSceneName(dbGameName);
					
					Debug.Log("Linked to scene game: " + gameName);
					
					break;
				}
				else
				{
					Debug.Log(dbGameName + " is not linked");
					++m_sectionsComplete;
				}
			}
			else
			{
				++m_sectionsComplete;
			}
		}
		
		if(gameName != null)
		{
			Debug.Log("Next Game: " + gameName);
			m_state = State.StartGame;
			TransitionScreen.Instance.ChangeLevel(gameName, false);
		}
		else
		{
			CompleteSession();
		}
	}

	void CompleteSession()
	{
		Debug.Log("Session Complete");
		
		if(onSessionComplete != null)
		{
			onSessionComplete();
		}

		m_state = State.Sleep;
		
		string newScene = m_st == ST.Pippisode ? "NewPippisodeMenu" : "NewVoyage";
		TransitionScreen.Instance.ChangeLevel(newScene, false);
	}

	#if UNITY_EDITOR
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.S) && GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Session)
		{
			OnGameFinish();
		}
	}
	#endif
}
