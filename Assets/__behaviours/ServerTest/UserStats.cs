using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UserStats : Singleton<UserStats> 
{
	static string m_url = "http://pipperformance.herokuapp.com/tests";
	static string m_email = "bonobo@pip.com";
	static string m_accountUsername = "ParentUser";
	
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
		
		Activity.OnNewScene ();
	}

	static string ConcatList<T>(List<T> list)
	{
		string concat = "";
		
		foreach (T t in list) 
		{
			concat += String.Format("{0}_", t);
		}
		
		concat = concat.TrimEnd (new char[] { '_' });
		
		return concat;
	}

	public class Activity : TimedEvent
	{
		private static Activity m_current;
		public static Activity Current
		{
			get
			{
				return m_current;
			}
		}

		string m_coreSkill = "Reading";

		string m_sessionIdentifier;

		string m_scene;

		int m_setNum = 0;
		int m_sectionId = 0;

		int m_numAnswers = 0;

		List<int> m_phonemeIds = new List<int> ();
		List<int> m_incorrectPhonemeIds = new List<int> ();

		List<int> m_wordIds = new List<int> ();
		List<int> m_incorrectWordIds = new List<int>();

		int m_storyId = 0;
		List<int> m_pipPadCalls = new List<int> ();

		public Activity() : base()
		{
			m_scene = Application.loadedLevelName;
			m_sessionIdentifier = Session.OnNewGame(m_scene);

			if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Sets)
			{
				// TODO: Get m_setNum
			}
			else
			{
				// TODO: Get m_sectionId
			}

			m_current = this;
		}

		public static void OnNewScene()
		{
			if (m_current != null) 
			{
				m_current.EndEvent(false);
			}

			if(GameLinker.Instance.IsSceneGame(Application.loadedLevelName))
			{
				Debug.Log("new Activity: " + Application.loadedLevelName);
				new Activity();
			}
		}

		public override void EndEvent(bool completed)
		{
			base.EndEvent (completed);
			
			m_current = null;
		}

		public override void PostData()
		{
			WWWForm form = new WWWForm ();

			form.AddField ("test[core_skill]", m_coreSkill);
			form.AddField ("test[session_identifier]", m_sessionIdentifier);
			form.AddField ("test[scene]", m_scene);
			form.AddField ("test[set_num]", m_setNum);
			form.AddField ("test[section_id]", m_sectionId);
			form.AddField ("test[num_answers]", m_numAnswers);
			form.AddField ("test[phoneme_ids]", ConcatList (m_phonemeIds));
			form.AddField ("test[incorrect_phoneme_ids]", ConcatList (m_incorrectPhonemeIds));
			form.AddField ("test[word_ids]", ConcatList (m_wordIds));
			form.AddField ("test[incorrect_word_ids]", ConcatList (m_incorrectWordIds));
			form.AddField ("test[story_id]", m_storyId);
			form.AddField ("test[pip_pad_calls]", ConcatList (m_pipPadCalls));

			base.PostData ("Activity", m_url, form);
		}

		// Setters
		public void SetSetNum(int setNum)
		{
			m_setNum = setNum;
		}

		public void SetSectionId(int sectionId)
		{
			m_sectionId = sectionId;
		}

		public void IncrementNumAnswers()
		{
			++m_numAnswers;
		}

		public void AddPhoneme(int phonemeId)
		{
			m_phonemeIds.Add (phonemeId);
		}

		public void AddIncorrectPhoneme(int phonemeId)
		{
			m_incorrectPhonemeIds.Add (phonemeId);
		}

		public void AddWord(int wordId)
		{
			m_wordIds.Add (wordId);
		}

		public void AddIncorrectWordId(int wordId)
		{
			m_wordIds.Add (wordId);
		}

		public void SetStoryId(int storyId)
		{
			m_storyId = storyId;
		}

		public void AddPipPadCall(int wordId)
		{
			m_pipPadCalls.Add (wordId);
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

		string m_sessionIdentifier;
		string m_sessionName = "";
		int m_sessionId = 0;
		int m_sessionNum = 0;

		SessionManager.ST m_sessionType;

		List<string> m_scenes = new List<string> ();

		List<int> m_letters = new List<int> ();
		int m_targetLetter = 0;

		List<int> m_words = new List<int> ();
		int m_targetWord = 0;

		List<int> m_keywords = new List<int>();
		int m_targetKeyword = 0;

		// Voyage/Pippisode Constructor
		public Session(SessionManager.ST sessionType, int sessionId, int sessionNum) : base()
		{
			m_sessionType = sessionType;
			m_sessionId = sessionId;
			m_sessionNum = sessionNum;

			BuildSessionIdentifier(); 

			m_current = this;
		}

		// Lesson Constructor
		public Session(SessionManager.ST sessionType, string sessionName) : base()
		{
			m_sessionType = sessionType;
			m_sessionName = sessionName;

			BuildSessionIdentifier();

			m_letters = LessonInfo.Instance.GetDataIds (LessonInfo.DataType.Letters);
			m_targetLetter = LessonInfo.Instance.GetTargetId (LessonInfo.DataType.Letters);
			
			m_words = LessonInfo.Instance.GetDataIds (LessonInfo.DataType.Words);
			m_targetWord = LessonInfo.Instance.GetTargetId (LessonInfo.DataType.Words);
			
			m_keywords = LessonInfo.Instance.GetDataIds (LessonInfo.DataType.Keywords);
			m_targetKeyword = LessonInfo.Instance.GetTargetId (LessonInfo.DataType.Keywords);

			m_current = this;
		}

		void BuildSessionIdentifier()
		{
			m_sessionIdentifier = String.Format("{0}_{1}_{2}_{3}", new System.Object[] { m_accountUsername, m_sessionId, GetTrimmedStartTime(), m_sessionType.ToString() });
		}

		public static string OnNewGame(string scene)
		{
			string sessionIdentifier = "";

			if (m_current != null) 
			{
				sessionIdentifier = m_current.m_sessionIdentifier;
				m_current.m_scenes.Add(scene);
			}

			return sessionIdentifier;
		}

		public override void EndEvent(bool completed)
		{
			base.EndEvent (completed);
			
			m_current = null;
		}
		
		public override void PostData()
		{
			Debug.Log ("Session.PostData()");
			
			WWWForm form = new WWWForm();

			form.AddField ("test[session_identifier]", m_sessionIdentifier);
			form.AddField ("test[session_name]", m_sessionName);
			form.AddField ("test[session_id]" , m_sessionId);
			form.AddField ("test[session_num]" , m_sessionNum);
			form.AddField ("test[session_type]", m_sessionType.ToString ());
			form.AddField ("test[scenes]", ConcatList (m_scenes));

			form.AddField ("test[phonemes]", ConcatList (m_letters));
			form.AddField ("test[target_phoneme]", m_targetLetter);
			form.AddField ("test[words]", ConcatList (m_words));
			form.AddField ("test[target_word]", m_targetWord);
			form.AddField ("test[keywords]", ConcatList (m_keywords));
			
			base.PostData ("Session", m_url, form);
		}
	}

	void OnSessionComplete()
	{
		if (Session.Current != null) 
		{
			Session.Current.EndEvent(true);
		}
	}
	
	void OnSessionCancel()
	{
		if (Session.Current != null) 
		{
			Session.Current.EndEvent(false);
		}
	}

	/*
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
			CompleteEvent();
		}
		
		private void PostData()
		{
			Debug.Log ("Game.PostData()");
			
			WWWForm form = new WWWForm();
			
			form.AddField ("test[scene_name]" , m_sceneName);
			form.AddField ("test[data_type]", m_dataType.ToString ());
			form.AddField ("test[num_of_answers]", m_numAnswers);
			form.AddField ("test[num_of_incorrect_answers]", m_incorrectAnswers.Count);
			
			AddBaseStats (form);
			
			WWW www = new WWW (m_url, form);
			
			UserStats.Instance.WaitForRequest ("Game", www);
		}
	}
	*/
	
	public abstract class TimedEvent
	{
		protected DateTime m_start;
		protected DateTime m_end = new DateTime(0);
		
		protected bool m_hasCompleted = false;
		
		protected TimedEvent()
		{
			m_start = DateTime.Now;
		}
		
		public virtual void EndEvent(bool completed)
		{
			if (m_end.Ticks == 0) // m_end can only be set once after initialization
			{
				m_end = DateTime.Now;
			}

			if(completed)
			{
				m_hasCompleted = true;
			}

			PostData ();
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

		public abstract void PostData ();

		public virtual void PostData (string eventName, string url, WWWForm form)
		{
			form.AddField ("test[account_username]", m_accountUsername);
			form.AddField ("test[child_name]", ChooseUser.Instance.GetCurrentUser ());
			form.AddField ("test[has_finished]", Convert.ToInt32(m_hasCompleted));
			form.AddField ("test[created_at]", GetTrimmedStartTime());
			form.AddField ("test[updated_at]", GetTrimmedEndTime());

			WWW www = new WWW (url, form);

			UserStats.Instance.WaitForRequest (eventName, www);
		}
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
