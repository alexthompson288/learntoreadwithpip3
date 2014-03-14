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


	void OnLevelWasLoaded(int level)
	{
		Debug.Log ("UserStats.OnLevelWasLoaded()");

		Game.OnNewScene ();
	}

	public abstract class TimedEvent
	{
		protected DateTime m_start;
		protected DateTime m_end;

		protected bool m_hasFinished = false;

		protected TimedEvent()
		{
			Debug.Log("new TimedEvent()");
			m_start = DateTime.Now;
		}

		protected void OnEventEnd()
		{
			m_end = DateTime.Now;
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

	public class Story : TimedEvent // TODO
	{
		// Title
		// Id
	}

	public class Session : TimedEvent
	{
		static Session m_current;

		List<Game> m_games = new List<Game> ();
	}

	public class Game : TimedEvent
	{
		static Game m_current = null;

		string m_sceneName;
		GameDataBridge.DataType m_dataType;

		int m_numAnswers = 0;
		List<IncorrectAnswer> m_incorrectAnswers = new List<IncorrectAnswer>();

		public static void OnNewScene()
		{
			Debug.Log ("Game.OnNewScene()");

			if (m_current != null) 
			{
				m_current.OnEventEnd();
				m_current.PostData();
			}

			Game currentGame = null;

			if(SessionManager.Instance.IsGame(Application.loadedLevelName))
			{
				currentGame = new Game();
			}

			m_current = currentGame;
		}

		private Game() : base()
		{
			Debug.Log ("new Game()");

			m_sceneName = Application.loadedLevelName;
			m_dataType = GameDataBridge.Instance.dataType;
		}

		public static void OnAnswer()
		{
			Debug.Log ("Game.OnAnswer()");

			if (m_current != null) 
			{
				++m_current.m_numAnswers;
			}
		}

		public static void OnIncorrect(DataRow answer, DataRow correct)
		{
			Debug.Log ("Game.OnIncorrect()");

			if (m_current != null)
			{
				string attribute = GameDataBridge.Instance.GetAttribute (m_current.m_dataType);

				m_current.m_incorrectAnswers.Add(new IncorrectAnswer(answer[attribute].ToString(),
				                                                     Convert.ToInt32(answer["id"]),
				                                                     correct[attribute].ToString(),
				                                                     Convert.ToInt32(correct["id"])));
			}

		}

		public static void OnIncorrect(string answer, int answerId, string correct, int correctId)
		{
			if (m_current != null) 
			{
				m_current.m_incorrectAnswers.Add (new IncorrectAnswer (answer, answerId, correct, correctId));
			}
		}

		public static void FinishGame()
		{
			if (m_current != null) 
			{
				m_current.SetHasFinishedTrue();
			}
		}

		private void PostData()
		{
			Debug.Log ("Game.PostData()");

			WWWForm form = new WWWForm();
			form.AddField ("test[username]", m_userPrefix + "_" + ChooseUser.Instance.GetCurrentUser ());
			form.AddField ("test[email]", m_email);
			form.AddField ("test[scene_name]" , m_sceneName);
			form.AddField ("test[data_type]", m_dataType.ToString ());
			form.AddField ("test[num_of_answers]", m_numAnswers);
			form.AddField ("test[num_of_incorrect_answers]", m_incorrectAnswers.Count);
			form.AddField ("test[has_finished]", Convert.ToInt32(m_hasFinished));
			form.AddField ("test[created_at]", GetTrimmedStartTime());
			form.AddField ("test[updated_at]", GetTrimmedEndTime());

			WWW www = new WWW (m_url, form);

			UserStats.Instance.WaitForRequest ("Game", www);
		}
	}

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
