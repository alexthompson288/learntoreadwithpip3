using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParentGate : Singleton<ParentGate> 
{
    public delegate void ParentGateAnswer(bool isCorrect);
    public event ParentGateAnswer Answered;

    public delegate void ParentGateDismiss();
    public event ParentGateDismiss Dismissed;

    [SerializeField]
    private TweenBehaviour m_tweenBehaviour;
    [SerializeField]
    private int m_minQuestionValue = 1;
    [SerializeField]
    private int m_maxQuestionValue = 10;
    [SerializeField]
    private UILabel m_questionLabel;
    [SerializeField]
    private EventRelay[] m_answerButtons;
    [SerializeField]
    private EventRelay m_backCollider;

    int m_correctAnswer;

    void Awake()
    {
        m_tweenBehaviour.gameObject.SetActive(true);

        m_backCollider.SingleClicked += OnClickBackCollider;

        foreach (EventRelay button in m_answerButtons)
        {
            button.SingleClicked += OnClickAnswer;
        }
    }

    public void On()
    {
        if(!m_tweenBehaviour.isOn)
        {
            m_tweenBehaviour.On();
            
            int num1 = UnityEngine.Random.Range(m_minQuestionValue, m_maxQuestionValue + 1);
            int num2 = UnityEngine.Random.Range(m_minQuestionValue, m_maxQuestionValue + 1);
            
            m_questionLabel.text = string.Format("What is {0} + {1}?", num1, num2);
            
            m_correctAnswer = num1 + num2;

            //D.Log("correctAnswer: " + m_correctAnswer);

            HashSet<int> answers = new HashSet<int>();

            answers.Add(m_correctAnswer);

            while(answers.Count < m_answerButtons.Length)
            {
                answers.Add(UnityEngine.Random.Range(m_minQuestionValue * 2, m_maxQuestionValue * 2));
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
        }
    }

    bool CheckAnswer(EventRelay relay)
    {
        try
        {
            return (System.Convert.ToInt32(relay.GetComponentInChildren<UILabel>().text) == m_correctAnswer);
        }
        catch
        {
            return false;
        }
    }

    void OnClickAnswer(EventRelay relay)
    {
        m_tweenBehaviour.Off();

        if(Answered != null)
        {
            Answered(CheckAnswer(relay));
        }
    }

    void OnClickBackCollider(EventRelay relay)
    {
        m_tweenBehaviour.Off();
        if (Dismissed != null)
        {
            Dismissed();
        }
    }
}
