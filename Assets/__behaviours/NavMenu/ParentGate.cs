using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ParentGate : Singleton<ParentGate> 
{
	public delegate void ParentGateAnswer(bool isCorrect);
	public event ParentGateAnswer OnParentGateAnswer;

	[SerializeField]
	private UICamera m_myCam;
	[SerializeField]
	private TweenOnOffBehaviour m_tweenBehaviour;
	[SerializeField]
	private int m_minQuestionValue = 1;
	[SerializeField]
	private int m_maxQuestionValue = 10;
	[SerializeField]
	private UILabel m_questionLabel;
	[SerializeField]
	private ClickEvent[] m_answerButtons;

	int m_correctAnswer = 0;

	bool m_isShowing = false;
	

	void Awake()
	{
		if(m_myCam != null)
		{
			m_myCam.GetComponent<Camera>().enabled = true;
		}

		foreach(ClickEvent answerLabel in m_answerButtons)
		{
			answerLabel.SingleClicked += OnAnswerClick;
		}
	}

	public void On()
	{
		if(!m_isShowing)
		{
			m_isShowing = true;

			m_tweenBehaviour.On();

			int num1 = UnityEngine.Random.Range(m_minQuestionValue, m_maxQuestionValue + 1);
			int num2 = UnityEngine.Random.Range(m_minQuestionValue, m_maxQuestionValue + 1);

			m_questionLabel.text = String.Format("What is {0} + {1}?", num1, num2);

			m_correctAnswer = num1 + num2;
			HashSet<int> answers = new HashSet<int>();
			while(!answers.Contains(m_correctAnswer))
			{
				answers.Clear();
				for(int i = 0; i < m_answerButtons.Length; ++i)
				{
					answers.Add(UnityEngine.Random.Range(m_minQuestionValue * 2, m_maxQuestionValue * 2));
				}
			}

			int index = 0;
			foreach(int ans in answers)
			{
				if(index < m_answerButtons.Length)
				{
					m_answerButtons[index].GetComponentInChildren<UILabel>().text = ans.ToString();
				}
				++index;
			}

            /*
            UICamera[] allCams = UnityEngine.Object.FindObjectsOfType(typeof(UICamera)) as UICamera[];
            foreach(UICamera cam in allCams)
            {
                if(cam != m_myCam)
                {
                    cam.enabled = false;
                }
            }
            */
		}
	}

	bool CheckAnswer(ClickEvent answerBehaviour)
	{
		try
		{
			return (Convert.ToInt32(answerBehaviour.GetComponentInChildren<UILabel>().text) == m_correctAnswer);
		}
		catch
		{
			return false;
		}
	}

	void OnAnswerClick(ClickEvent answerBehaviour)
	{
        /*
        UICamera[] allCams = UnityEngine.Object.FindObjectsOfType(typeof(UICamera)) as UICamera[];
        foreach(UICamera cam in allCams)
        {
            cam.enabled = true;
        }
        */

        //////D.Log("Answer: " + CheckAnswer(answerBehaviour));

		if(OnParentGateAnswer != null)
		{
			OnParentGateAnswer(CheckAnswer(answerBehaviour));
		}

		m_isShowing = false;
		m_tweenBehaviour.Off();
	}

	public UICamera GetUICam()
	{
		return m_myCam;
	}
}
