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

	private State m_state;
	
	float m_timeSessionStarted;

	public float GetTimeSessionStarted()
	{
		return m_timeSessionStarted;
	}
	
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
	int m_activitiesComplete;

	int m_sessionNum;

	ST m_st;

	public enum ST //SessionType
	{
		Voyage,
		Pippisode,
		Lesson
	}

	public int GetSessionNum()
	{
		return m_sessionNum;
	}

	public void OnChooseSession(ST sessionType)
	{
		GameDataBridge.Instance.SetContentType(GameDataBridge.ContentType.Custom);

		m_st = sessionType;
		m_sessionNum = 0;
		m_activitiesComplete = 0;
		m_timeSessionStarted = Time.time;

		new UserStats.Session(m_st, LessonInfo.Instance.GetName());

		PlayNextActivity(LessonInfo.Instance.GetSceneName (m_activitiesComplete));
	}

	public void OnChooseSession(ST sessionType, int sessionNum)
	{
		m_st = sessionType;

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

				m_activitiesComplete = 0;

				new UserStats.Session(m_st, sessionId, m_sessionNum);

				FindNextActivity();
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
	
	public void OnGameFinish(bool wonGame = true)
	{
		Debug.Log("OnGameFinish()");
		if(wonGame)
		{
			++m_activitiesComplete;
		}

		PlayNextActivity( m_st == ST.Lesson ? LessonInfo.Instance.GetSceneName (m_activitiesComplete) : FindNextActivity() );
	}

	string FindNextActivity()
	{
		string sceneName = "";
		
		Debug.Log("Sections Complete: " + m_activitiesComplete);
		for(int i = m_activitiesComplete; i < m_sections.Count; ++i)
		{
			Debug.Log("Finding game for sectionId: " + m_sections[i]["id"].ToString());
			
			DataRow game = DataHelpers.FindGameForSection(m_sections[i]);
			
			if(game != null)
			{
				string dbGameName = game["name"].ToString();
				
				Debug.Log("Found dbGame: " + dbGameName);
				
				if(GameLinker.Instance.IsDBGame(dbGameName))
				{
					sceneName = GameLinker.Instance.GetSceneName(dbGameName);
					
					Debug.Log("Linked to scene game: " + sceneName);
					
					break;
				}
				else
				{
					Debug.Log(dbGameName + " is not linked");
					++m_activitiesComplete;
				}
			}
			else
			{
				++m_activitiesComplete;
			}
		}

		return sceneName;
	}

	void PlayNextActivity(string sceneName)
	{
		if (!String.IsNullOrEmpty (sceneName)) 
		{
			m_state = State.StartGame;
			TransitionScreen.Instance.ChangeLevel (sceneName, false);
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

		string newScene = "NewVoyage";

		if (m_st == ST.Pippisode) 
		{
			newScene = "NewPippisodeMenu";
		} 
		else if (m_st == ST.Lesson) 
		{
			newScene = "NewLessonMenu";
		}

		TransitionScreen.Instance.ChangeLevel(newScene, false);
	}

	public DataRow GetCurrentSection()
	{
		Debug.Log("m_activitiesComplete: " + m_activitiesComplete);
		
		if(m_activitiesComplete < m_sections.Count)
		{
			Debug.Log("Current Section: " + m_sections[m_activitiesComplete]["id"].ToString());
			
			return m_sections[m_activitiesComplete];
		}
		else
		{
			Debug.LogError("There are not enough sections in m_sections");
			return null;
		}
	}
	
	public int GetCurrentSectionId()
	{
		Debug.Log("m_activitiesComplete: " + m_activitiesComplete);
		if(m_activitiesComplete < m_sections.Count)
		{
			Debug.Log("current sectionId: " + m_sections[m_activitiesComplete]["id"].ToString());
			return System.Convert.ToInt32(m_sections[m_activitiesComplete]["id"]);
		}
		else
		{
			Debug.LogError("There are not enough sections in m_sections");
			return -1;
		}
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
