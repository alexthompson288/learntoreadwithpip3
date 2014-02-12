using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UserStoriesStats : Singleton<UserStoriesStats>
{
	Dictionary<string, int> m_pipPadCalls = new Dictionary<string, int>();
	List<Question> m_correctAnswers = new List<Question>();
	Dictionary<Question, int> m_incorrectAnswers = new Dictionary<Question, int>();

	public class Question : IEquatable<Question>
	{
		int m_num;
		string m_question;

		public Question(int num, string question)
		{
			m_num = num;
			m_question = question;
		}

		public int GetNum()
		{
			return m_num;
		}

		public string GetQuestion()
		{
			return m_question;
		}

		public bool Equals(Question other)
		{
			if(other == null)
			{
				return false;
			}

			return (m_num == other.GetNum());
		}
	}

	public void ClearStats()
	{
		m_pipPadCalls.Clear();
		m_correctAnswers.Clear();
		m_incorrectAnswers.Clear();
	}

	public void ClearPipPadCalls()
	{
		m_pipPadCalls.Clear();
	}

	public void ClearAnswers()
	{
		m_correctAnswers.Clear();
		m_incorrectAnswers.Clear();
	}

	public void OnPipPadCall(string word)
	{
		if(m_pipPadCalls.ContainsKey(word))
		{
			++m_pipPadCalls[word];
		}
		else
		{
			m_pipPadCalls.Add(word, 1);
		}
	}

	public void OnCorrectAnswer(int num, string sentence)
	{
		Question question = new Question(num, sentence);

		if(!m_incorrectAnswers.ContainsKey(question))
		{
			m_correctAnswers.Add(question);
		}
	}

	public void OnIncorrectAnswer(int num, string sentence)
	{
		Question question = new Question(num, sentence);

		if(!m_incorrectAnswers.ContainsKey(question))
		{
			m_incorrectAnswers.Add(question, 1);
		}
		else
		{
			++m_incorrectAnswers[question];
		}
	}

	public Dictionary<string, int> GetPipPadCalls()
	{
		return m_pipPadCalls;
	}

	public int GetCorrectCount()
	{
		return m_correctAnswers.Count;
	}

	public int GetIncorrectCount()
	{
		return m_incorrectAnswers.Count;
	}
}
