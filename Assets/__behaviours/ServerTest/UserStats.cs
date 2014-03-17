using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UserStats : Singleton<UserStats> 
{
	static string m_url = "http://pipperformance.herokuapp.com/tests";
	static string m_email = "bonobo@pip.com";
	static string m_userPrefix = "ParentUser_";
	
	Dictionary<GameDataBridge.DataType, string> m_dataAttributes = new Dictionary<GameDataBridge.DataType, string>();

	void Start()
	{
		SessionManager.Instance.OnSessionComplete += OnSessionComplete;
		SessionManager.Instance.OnSessionCancel += OnSessionCancel;
	}
	
	// Called every time a new level is loaded
	void OnLevelWasLoaded(int level)
	{
		Debug.Log ("UserStats.OnLevelWasLoaded()");
		
		Game newGame = Game.OnNewScene();
		if (newGame != null && Session.Current != null) 
		{
			Session.Current.OnNewGame(newGame);
		}

		if (Application.loadedLevelName == "NewStories") 
		{
			StartCoroutine(CreateStory());
		} 
		else if (Story.Current != null) 
		{
			Story.Current.EndStory(false);
		}
	}
	
	private class PipPadCall
	{
		string m_word;
		int m_wordId;
		
		public PipPadCall(string word, int wordId)
		{
			m_word = word;
			m_wordId = wordId;
		}
	}

	IEnumerator CreateStory()
	{
		yield return StartCoroutine (StoryReaderLogic.WaitForStoryData ());
		
		int storyId = StoryReaderLogic.Instance.GetStoryId ();
		
		DataTable dt = GameDataBridge.Instance.GetDatabase ().ExecuteQuery ("select * from stories WHERE id=" + storyId);
		string title = dt.Rows.Count > 0 ? dt.Rows[0]["title"].ToString () : "MissingTitle";
		
		new Story (title, storyId);
	}
	
	public class Story : TimedEvent
	{
		private static Story m_current;
		public static Story Current
		{
			get 
			{
				return m_current;
			}
		}

		string m_title;
		int m_id;

		List<PipPadCall> m_pipPadCalls = new List<PipPadCall> ();

		public Story(string title, int id) : base()
		{
			m_title = title;
			m_id = id;
			m_current = this;
		}

		public static void OnNewScene()
		{
			if (m_current != null) 
			{
				m_current.PostData();
				m_current = null;
			}
		}

		public void OnCallPipPad(string word, int wordId)
		{
			m_pipPadCalls.Add (new PipPadCall (word, wordId));
		}

		public void EndStory(bool completed)
		{
			EndEvent ();
			PostData ();

			if (completed) 
			{
				SetHasFinishedTrue();
			}

			m_current = null;
		}

		public void PostData()
		{
			Debug.Log ("Session.PostData()");
			
			WWWForm form = new WWWForm();
			
			UserStats.Instance.AddUserStats (form);
			
			form.AddField ("test[title]" , m_title);
			form.AddField ("test[m_id]", m_id);
			form.AddField ("test[num_pip_pad_calls]", m_pipPadCalls.Count);
			
			// TODO: Add PipPadCall data
			
			AddBaseStats (form);
			
			WWW www = new WWW (m_url, form);
			
			UserStats.Instance.WaitForRequest ("Story", www);
		}
	}

	void OnSessionComplete()
	{
		if (Session.Current != null) 
		{
			Session.Current.EndSession(true);
		}
	}
	
	void OnSessionCancel()
	{
		if (Session.Current != null) 
		{
			Session.Current.EndSession(false);
		}
	}
	
	public class Session : TimedEvent
	{
		private static Session m_current;
		public static Session Current
		{
			get
			{
				return m_current;
			}
		}

		int m_sessionNum;
		SessionManager.ST m_sessionType;
		List<Game> m_games = new List<Game> ();

		public Session(int sessionNum, SessionManager.ST sessionType) : base()
		{
			m_sessionNum = sessionNum;
			m_sessionType = sessionType;
			m_current = this;
		}

		public void OnNewGame(Game newGame)
		{
			m_games.Add (newGame);
		}

		public void EndSession(bool finished)
		{
			EndEvent ();

			if (finished) 
			{
				SetHasFinishedTrue ();
			}

			PostData ();

			m_current = null;
		}
		
		private void PostData()
		{
			Debug.Log ("Session.PostData()");
			
			WWWForm form = new WWWForm();
			
			UserStats.Instance.AddUserStats (form);
			
			form.AddField ("test[session_num]" , m_sessionNum);
			form.AddField ("test[m_session_type]", m_sessionType.ToString ());

			// TODO: Add game data
			
			AddBaseStats (form);
			
			WWW www = new WWW (m_url, form);
			
			UserStats.Instance.WaitForRequest ("Session", www);
		}
	}

	public class Game : TimedEvent
	{
		private class IncorrectAnswer
		{
			string m_answer;
			int m_answerId;
			
			string m_correct;
			int m_correctId;
			
			public IncorrectAnswer(string answer, int answerId, string correct, int correctId)
			{
				m_answer = answer;
				m_answerId = answerId;
				
				m_correct = correct;
				m_correctId = correctId;
			}
		}

		private static Game m_current = null;
		public static Game Current
		{
			get
			{
				return m_current;
			}
		}
		
		string m_sceneName;
		GameDataBridge.DataType m_dataType;
		
		int m_numAnswers = 0;
		List<IncorrectAnswer> m_incorrectAnswers = new List<IncorrectAnswer>();
		
		public static Game OnNewScene()
		{
			Debug.Log ("Game.OnNewScene()");

			if (m_current != null) 
			{
				m_current.EndEvent ();
				m_current.PostData ();
			}

			// If we are in a game then create a new current game
			Game newCurrent = null;
			
			if(GameLinker.Instance.IsSceneGame(Application.loadedLevelName))
			{
				newCurrent = new Game();
			}

			m_current = newCurrent; // m_current is static

			return m_current;
		}
		
		private Game() : base()
		{
			Debug.Log ("new Game()");
			
			m_sceneName = Application.loadedLevelName;
			m_dataType = GameDataBridge.Instance.dataType;
			m_current = this;
		}
		
		public void OnAnswer()
		{
			Debug.Log ("Game.OnAnswer()");

			++m_numAnswers;
		}
		
		public void OnIncorrect(DataRow answer, DataRow correct)
		{
			Debug.Log ("Game.OnIncorrect()");

			string attribute = GameDataBridge.Instance.GetAttribute (m_dataType);
				
			m_incorrectAnswers.Add(new IncorrectAnswer(answer[attribute].ToString(),
				                                       Convert.ToInt32(answer["id"]),
				                                       correct[attribute].ToString(),
				                                       Convert.ToInt32(correct["id"])));
		}
		
		public void OnIncorrect(string answer, int answerId, string correct, int correctId)
		{
			m_incorrectAnswers.Add (new IncorrectAnswer (answer, answerId, correct, correctId));
		}
		
		public void FinishGame()
		{
			EndEvent ();
			SetHasFinishedTrue();
		}
		
		private void PostData()
		{
			Debug.Log ("Game.PostData()");
			
			WWWForm form = new WWWForm();

			UserStats.Instance.AddUserStats (form);

			form.AddField ("test[scene_name]" , m_sceneName);
			form.AddField ("test[data_type]", m_dataType.ToString ());
			form.AddField ("test[num_of_answers]", m_numAnswers);
			form.AddField ("test[num_of_incorrect_answers]", m_incorrectAnswers.Count);

			AddBaseStats (form);
			
			WWW www = new WWW (m_url, form);
			
			UserStats.Instance.WaitForRequest ("Game", www);
		}
	}

	public abstract class TimedEvent
	{
		protected DateTime m_start;
		protected DateTime m_end = new DateTime(0);
		
		protected bool m_hasFinished = false;
		
		protected TimedEvent()
		{
			m_start = DateTime.Now;
		}

		protected void AddBaseStats(WWWForm form)
		{
			form.AddField ("test[has_finished]", Convert.ToInt32(m_hasFinished));
			form.AddField ("test[created_at]", GetTrimmedStartTime()); // TODO: Request rename: time_start
			form.AddField ("test[updated_at]", GetTrimmedEndTime()); // TODO: Request rename: time_end
		}
		
		protected void EndEvent()
		{
			if (m_end.Ticks == 0) // m_end can only be set once after initialization
			{
				m_end = DateTime.Now;
			}
		}
		
		protected void SetHasFinishedTrue()
		{
			m_hasFinished = true;
		}

		protected string GetTrimmedStartTime()
		{
			return Trim (String.Format ("start: {0:d} - {1:g}", m_start.Date, m_start.TimeOfDay));
		}
		
		protected string GetTrimmedEndTime()
		{
			return Trim (String.Format ("start: {0:d} - {1:g}", m_end.Date, m_end.TimeOfDay));
		}
		
		string Trim(string s)
		{
			int lastDecimalIndex = s.LastIndexOf (".");
			Debug.Log ("Post - " + s.Substring (0, lastDecimalIndex));
			return s.Substring (0, lastDecimalIndex);
		}
	}

	void AddUserStats(WWWForm form)
	{
		form.AddField ("test[username]", m_userPrefix + "_" + ChooseUser.Instance.GetCurrentUser ());
		form.AddField ("test[email]", m_email);
	}
	
	void WaitForRequest(string eventName, WWW www)
	{
		StartCoroutine (WaitForRequestCo (eventName, www));
	}
	
	public IEnumerator WaitForRequestCo(string eventName, WWW www)
	{
		Debug.Log ("Waiting for request");
		
		yield return www;
		
		// check for errors
		if (www.error == null)
		{
			Debug.Log(String.Format("WWW {0} - OK", eventName));
			Debug.Log("Data: " + www.data);
			Debug.Log("Text: " + www.text);
		} 
		else 
		{
			Debug.Log(String.Format("WWW {0} - ERROR", eventName));
			Debug.Log("Error: "+ www.error);
		}    
	}
}