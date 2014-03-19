using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UserStats : Singleton<UserStats> 
{	
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
#if UNITY_EDITOR
                if(m_current != null)
                {
                    return m_current;
                }
                else // In the editor we might need to spawn a dummy Activity because sometimes run directly from a game scene
                {
                    return new Activity(false);
                }
#else
                return m_current;
#endif
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

		public Activity(bool makeCurrent = true) : base()
		{
			Debug.Log("new Activity()");
			m_scene = Application.loadedLevelName;
			m_sessionIdentifier = Session.OnNewGame(m_scene);

			m_modelName = "activity";
			m_url = "www.learntoreadwithpip.com/activities";

			if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Sets)
			{
				m_setNum = SkillProgressInformation.Instance.GetCurrentLevel();
			}
			else
			{
				m_sectionId = SessionManager.Instance.GetCurrentSectionId();
			}

            if(makeCurrent)
            {
			    m_current = this;
            }
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
			Debug.Log (String.Format ("Activity.EndEvent({0})", completed));

			base.EndEvent (completed);
			
			m_current = null;
		}

		public override void PostData()
		{
			WWWForm form = new WWWForm ();

			form.AddField (m_modelName + "[core_skill]", m_coreSkill);
			form.AddField (m_modelName + "[session_identifier]", m_sessionIdentifier);
			form.AddField (m_modelName + "[scene]", m_scene);
			form.AddField (m_modelName + "[set_num]", m_setNum);
			form.AddField (m_modelName + "[section]", m_sectionId);
			form.AddField (m_modelName + "[num_answers]", m_numAnswers);
			form.AddField (m_modelName + "[phoneme_ids]", ConcatList (m_phonemeIds));
			form.AddField (m_modelName + "[incorrect_phoneme_ids]", ConcatList (m_incorrectPhonemeIds));
			form.AddField (m_modelName + "[word_ids]", ConcatList (m_wordIds));
			form.AddField (m_modelName + "[incorrect_word_ids]", ConcatList (m_incorrectWordIds));
			form.AddField (m_modelName + "[story]", m_storyId);
			form.AddField (m_modelName + "[pip_pad_calls]", ConcatList (m_pipPadCalls));


#if UNITY_EDITOR
			Debug.Log("Activity.PostData()");
			Debug.Log("sessionIdentifier: " + m_sessionIdentifier);
			Debug.Log("scene: " + m_scene);
			Debug.Log("setNum: " + m_setNum);
			Debug.Log("sectionId: " + m_sectionId);
			Debug.Log("numAnswers: " + m_numAnswers);
			Debug.Log("numIncorrectPhonemes: " + m_incorrectPhonemeIds.Count);
			Debug.Log("incorrectPhonemes: " + ConcatList(m_incorrectPhonemeIds));
			Debug.Log("storyId: " + m_storyId);
			Debug.Log("numPipPadCalls: " + m_pipPadCalls.Count);
			Debug.Log("pipPadCalls: " + ConcatList(m_pipPadCalls));
#endif

			base.PostData ("Activity", form);
		}

		// Setters
		public void IncrementNumAnswers()
		{
			Debug.Log ("Activity.IncrementNumAnswers()");
			++m_numAnswers;
		}

		public void AddData(int dataId, GameDataBridge.DataType dataType)
		{
			if (dataType == GameDataBridge.DataType.Letters)
			{
				AddPhoneme(dataId);
			} 
			else
			{
				AddWord(dataId);		
			}
		}

		public void AddData(DataRow data, GameDataBridge.DataType dataType)
		{
			if (dataType == GameDataBridge.DataType.Letters)
			{
				AddPhoneme(data);
			} 
			else
			{
				AddWord(data);		
			}
		}

		public void AddIncorrectData(int dataId, GameDataBridge.DataType dataType)
		{
			if (dataType == GameDataBridge.DataType.Letters)
			{
				AddIncorrectPhoneme(dataId);
			} 
			else
			{
				AddIncorrectWord(dataId);		
			}
		}

		public void AddIncorrectData(DataRow data, GameDataBridge.DataType dataType)
		{
			if (dataType == GameDataBridge.DataType.Letters)
			{
				AddIncorrectPhoneme(data);
			} 
			else
			{
				AddIncorrectWord(data);		
			}
		}

		public void AddPhoneme(int phonemeId)
		{
			m_phonemeIds.Add (phonemeId);
		}

		public void AddPhoneme(DataRow phoneme)
		{
			Debug.Log (String.Format ("Activity.AddPhoneme({0})", phoneme ["phoneme"]));
			m_phonemeIds.Add (Convert.ToInt32(phoneme ["id"]));
		}

		public void AddIncorrectPhoneme(int phonemeId)
		{
			m_incorrectPhonemeIds.Add (phonemeId);
		}

		public void AddIncorrectPhoneme(DataRow phoneme)
		{
			Debug.Log (String.Format ("Activity.AddIncorrectPhoneme({0})", phoneme ["phoneme"]));
			m_incorrectPhonemeIds.Add (Convert.ToInt32(phoneme ["id"]));
		}

		public void AddWord(int wordId)
		{
			m_wordIds.Add (wordId);
		}

		public void AddWord(DataRow word)
		{
			m_wordIds.Add (Convert.ToInt32 (word ["id"]));
		}

		public void AddIncorrectWord(int wordId)
		{
			m_incorrectWordIds.Add (wordId);
		}

		public void AddIncorrectWord(DataRow word)
		{
			m_incorrectWordIds.Add (Convert.ToInt32 (word ["id"]));
		}

		public void SetStoryId(int storyId)
		{
			Debug.Log(String.Format("Activity.SetStoryId({0})", storyId));
			m_storyId = storyId;
		}

		public void AddPipPadCall(int wordId)
		{
			Debug.Log(String.Format("Activity.AddPipPadCall({0})", wordId));
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

		string m_sessionIdentifier = "";
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
			m_sessionId = sessionId;
			m_sessionNum = sessionNum;

			JointConstructor(sessionType);

#if UNITY_EDITOR
			Debug.Log("new Session(): Voyage/Pippisode");
			DebugLog();
#endif
		}

		// Lesson Constructor
		public Session(SessionManager.ST sessionType, string sessionName) : base()
		{
			m_sessionName = sessionName;

			m_letters = LessonInfo.Instance.GetDataIds (LessonInfo.DataType.Letters);
			m_targetLetter = LessonInfo.Instance.GetTargetId (LessonInfo.DataType.Letters);
			
			m_words = LessonInfo.Instance.GetDataIds (LessonInfo.DataType.Words);
			m_targetWord = LessonInfo.Instance.GetTargetId (LessonInfo.DataType.Words);
			
			m_keywords = LessonInfo.Instance.GetDataIds (LessonInfo.DataType.Keywords);
			m_targetKeyword = LessonInfo.Instance.GetTargetId (LessonInfo.DataType.Keywords);

			JointConstructor(sessionType);

#if UNITY_EDITOR
			Debug.Log("new Session(): Lesson");
			DebugLog();
#endif
		}

		// Set anything here that needs to be set in both constructors
		void JointConstructor(SessionManager.ST sessionType) 
		{
			m_sessionType = sessionType;

			m_modelName = "session";
			m_url = "www.learntoreadwithpip.com/sessions";
			BuildSessionIdentifier();

            m_current = this;
		}

#if UNITY_EDITOR
		void DebugLog()
		{
			Debug.Log ("sessionIdentifier: " + m_sessionIdentifier);
			Debug.Log ("sessionType: " + m_sessionType);
			Debug.Log ("sessionName: " + m_sessionName);
			Debug.Log ("sessionId: " + m_sessionId);
			Debug.Log ("sessionNum: " + m_sessionNum);
		}
#endif

		void BuildSessionIdentifier()
		{
			m_sessionIdentifier = String.Format("{0}_{1}_{2}_{3}", new System.Object[] { UserInfo.Instance.accountUsername, m_sessionId, GetTrimmedStartTime(), m_sessionType.ToString() });
		}

		public static string OnNewGame(string scene)
		{
			Debug.Log ("Session.OnNewGame()");

			string sessionIdentifier = "";

			if (m_current != null) 
			{
				Debug.Log ("Linking activity to session: " + m_current.m_sessionIdentifier);
				sessionIdentifier = m_current.m_sessionIdentifier;
				m_current.m_scenes.Add (scene);
			} 
			else 
			{
				Debug.Log("No session to link activity");	
			}

			return sessionIdentifier;
		}

		public override void EndEvent(bool completed)
		{
			Debug.Log(String.Format("Session.EndEvent({0})", completed));
			base.EndEvent (completed);
			
			m_current = null;
		}
		
		public override void PostData()
		{
			WWWForm form = new WWWForm();

			form.AddField (m_modelName + "[session_identifier]", m_sessionIdentifier);
			form.AddField (m_modelName + "[session_name]", m_sessionName);
			form.AddField (m_modelName + "[session_id]" , m_sessionId);
			form.AddField (m_modelName + "[session_num]" , m_sessionNum);
			form.AddField (m_modelName + "[session_type]", m_sessionType.ToString ());
			form.AddField (m_modelName + "[scenes]", ConcatList (m_scenes));

			form.AddField (m_modelName + "[phoneme_ids]", ConcatList (m_letters));
			form.AddField (m_modelName + "[target_phoneme_id]", m_targetLetter);
			form.AddField (m_modelName + "[word_ids]", ConcatList (m_words));
			form.AddField (m_modelName + "[target_word_id]", m_targetWord);
			form.AddField (m_modelName + "[keyword_ids]", ConcatList (m_keywords));
			form.AddField (m_modelName + "[target_keyword_id]", m_targetKeyword);

#if UNITY_EDITOR
			Debug.Log ("Session.PostData()");
			Debug.Log("sessionIdentifier: " + m_sessionIdentifier);
			Debug.Log("sessionName: " + m_sessionName);
			Debug.Log("sessionId: " + m_sessionId);
			Debug.Log("sessionNum: " + m_sessionNum);
			Debug.Log("sessionType: " + m_sessionType);
			Debug.Log("scenes: " + ConcatList(m_scenes));
#endif
			
			base.PostData ("Session", form);
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

	public abstract class TimedEvent
	{
		protected DateTime m_start;
		protected DateTime m_end = new DateTime(0);
		
		protected bool m_hasCompleted = false;

		protected string m_url;
		protected string m_modelName;
		
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
			return Trim (String.Format ("{0:d} - {1:g}", m_start.Date, m_start.TimeOfDay));
		}
		
		protected string GetTrimmedEndTime()
		{
			return Trim (String.Format ("{0:d} - {1:g}", m_end.Date, m_end.TimeOfDay));
		}
		
		string Trim(string s)
		{
			int lastDecimalIndex = s.LastIndexOf (".");
			return s.Substring (0, lastDecimalIndex);
		}

		public abstract void PostData ();

		public virtual void PostData (string eventName, WWWForm form)
		{
			Debug.Log ("base.PostData(): " + m_modelName);

			form.AddField (m_modelName + "[account_username]", UserInfo.Instance.accountUsername);
			form.AddField (m_modelName + "[child_name]", UserInfo.Instance.childName);
			form.AddField (m_modelName + "[has_completed]", Convert.ToInt32(m_hasCompleted));
			form.AddField (m_modelName + "[created_at]", GetTrimmedStartTime());
			form.AddField (m_modelName + "[updated_at]", GetTrimmedEndTime());
                   

#if UNITY_EDITOR
			Debug.Log("accountUsername: " + UserInfo.Instance.accountUsername);
			Debug.Log("childName: " + UserInfo.Instance.childName);
			Debug.Log("hasCompleted: " + m_hasCompleted);
			Debug.Log("start: " + GetTrimmedStartTime());
			Debug.Log("end: " + GetTrimmedEndTime());

            Debug.Log("url: " + m_url);
            Debug.Log("tableName: " + m_modelName);
#endif

			WWW www = new WWW (m_url, form);

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
