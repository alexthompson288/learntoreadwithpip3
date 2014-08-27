using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaceValuePlayer : GamePlayer
{
    [SerializeField]
    private Timer m_timer;
    [SerializeField]
    private ScoreHealth m_scoreKeeper;
    [SerializeField]
    private TrafficLights m_trafficLights;
    [SerializeField]
    private PipButton m_goButton;
    [SerializeField]
    private UnitStack[] m_stacks;
    [SerializeField]
    private GameObject[] m_questionLabelParents;
    [SerializeField]
    private GameObject m_additionLabel;

    List<DataRow> m_currentDataPool = new List<DataRow>();

    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
    }

    public void StartGame(bool subscribeToTimer)
    {
        // Sort m_numberLabels from top to bottom
        System.Array.Sort(m_questionLabelParents, CollectionHelpers.LocalTopToBottom);

        ////D.Log("PlaceValuePlayer.StartGame()");
        
        m_goButton.Unpressing += OnUnpressGoButton;
        
        System.Array.Sort(m_stacks, CollectionHelpers.LocalRightToLeft);
        
        int[] stackValues = new int[] {1, 10, 100, 1000};
        
        for (int i = 0; i < m_stacks.Length && i < stackValues.Length; ++i)
        {
            m_stacks[i].SetValue(stackValues[i]);
        }

        m_scoreKeeper.StartTimer();
        m_scoreKeeper.Completed += OnScoreKeeperComplete;

        /*
        m_timer.SetTimeRemaing(PlaceValueCoordinator.Instance.GetTimeLimit());
        
        if (subscribeToTimer)
        {
            ////D.Log("Subscribing to timer");
            m_timer.Finished += OnTimerFinish;
        }
        
        m_timer.On();
        */

        AskQuestion();
    }

    void AskQuestion()
    {
        m_additionLabel.SetActive(m_currentDataPool.Count > 1);

        for(int i = 0;i < m_questionLabelParents.Length; ++i)
        {
            if(i < m_currentDataPool.Count)
            {
                m_questionLabelParents[i].SetActive(true);

                string value = m_currentDataPool[i]["value"].ToString();
                value = StringHelpers.Reverse(value);

                UILabel[] labels = m_questionLabelParents[i].GetComponentsInChildren<UILabel>() as UILabel[];
                System.Array.Sort(labels, CollectionHelpers.LocalRightToLeft);

                for(int j = 0;j < labels.Length; ++j)
                {
                    labels[j].enabled = j < value.Length;

                    if(j < value.Length)
                    {
                        labels[j].text = value[j].ToString();
                    }
                }
            }
            else
            {
                m_questionLabelParents[i].SetActive(false);
            }
        }
    }

    void OnUnpressGoButton(PipButton button)
    {
        int totalStackValue = 0;
        foreach (UnitStack stack in m_stacks)
        {
            totalStackValue += stack.GetStackedValue();
        }

        if (totalStackValue == PlaceValueCoordinator.Instance.GetDataTotal(m_currentDataPool))
        {
            m_scoreKeeper.UpdateScore(1);
            PlaceValueCoordinator.Instance.OnCorrectAnswer(this);
        }
        else
        {
            m_scoreKeeper.UpdateScore(-1);
            //WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
        }
    }

    void OnScoreKeeperComplete(ScoreKeeper keeper)
    {
        PlaceValueCoordinator.Instance.CompleteGame();
    }

    void OnTimerFinish(Timer timer)
    {
        PlaceValueCoordinator.Instance.CompleteGame();
    }

    public IEnumerator ClearQuestion()
    {
        foreach (UnitStack stack in m_stacks)
        {
            stack.ClearStack();
        }

        yield return new WaitForSeconds(0.2f);

        AskQuestion();
    }

    public void SetCurrentDataPool(List<DataRow> myCurrentDataPool)
    {
        m_currentDataPool = myCurrentDataPool;
    }

    public IEnumerator CelebrateVictory()
    {
        yield return StartCoroutine(m_scoreKeeper.Celebrate());
    }

    public int GetScore()
    {
        return m_scoreKeeper.GetScore();
    }
}
