using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaceValuePlayer : GamePlayer
{
    [SerializeField]
    private Timer m_timer;
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;
    [SerializeField]
    private TrafficLights m_trafficLights;
    [SerializeField]
    private PipButton m_goButton;
    [SerializeField]
    private UnitStack[] m_stacks;
    [SerializeField]
    private UILabel[] m_row1Labels;
    [SerializeField]
    private UILabel[] m_row2Labels;

    List<DataRow> m_currentDataPool = new List<DataRow>();

    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
    }

    public void StartGame(bool subscribeToTimer)
    {
        System.Array.Sort(m_row1Labels, CollectionHelpers.LocalRightToLeft);
        System.Array.Sort(m_row2Labels, CollectionHelpers.LocalRightToLeft);

        D.Log("ToyShopPlayer.StartGame()");
        
        m_goButton.Unpressing += OnUnpressGoButton;
        
        System.Array.Sort(m_stacks, CollectionHelpers.LocalLeftToRight);
        
        int[] stackValues = new int[] {1, 10, 100, 1000};
        
        for (int i = 0; i < m_stacks.Length && i < stackValues.Length; ++i)
        {
            m_stacks[i].SetValue(stackValues[i]);
        }
        
        m_timer.SetTimeRemaing(PlaceValueCoordinator.Instance.GetTimeLimit());
        
        if (subscribeToTimer)
        {
            D.Log("Subscribing to timer");
            m_timer.Finished += OnTimerFinish;
        }
        
        m_timer.On();
    }

    void AskQuestion()
    {

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

        }
    }

    void OnTimerFinish(Timer timer)
    {

    }

    public IEnumerator ClearQuestion()
    {
        yield return null;
    }

    public void SetCurrentDataPool(List<DataRow> myCurrentDataPool)
    {
        m_currentDataPool = myCurrentDataPool;
    }

    public IEnumerator CelebrateVictory()
    {
        yield return StartCoroutine(m_scoreKeeper.On());
    }

    public int GetScore()
    {
        return m_scoreKeeper.GetScore();
    }
}
