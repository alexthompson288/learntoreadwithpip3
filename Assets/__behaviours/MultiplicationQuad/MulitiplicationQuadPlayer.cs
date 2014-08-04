using UnityEngine;
using System.Collections;

public class MulitiplicationQuadPlayer : GamePlayer
{
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;
    [SerializeField]
    private DataDisplay m_dataDisplay;
    [SerializeField]
    private TrafficLights m_trafficLights;
    [SerializeField]
    private Timer m_timer;
    [SerializeField]
    private PipButton m_goButton;
    [SerializeField]
    private UIGrid m_grid;
    [SerializeField]
    private PipButton[] m_rowButtons;
    [SerializeField]
    private PipButton[] m_columnButtons;

    int m_numRows;
    int m_numColumns;

    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
    }

    public void StartGame(bool subscribeToTimer)
    {
        m_goButton.Unpressing += OnUnpressGoButton;

        for (int i = 0; i < m_rowButtons.Length; ++i)
        {
            m_rowButtons[i].Unpressing += OnUnpressRowButton;
        }

        for (int i = 0; i < m_columnButtons.Length; ++i)
        {
            m_columnButtons[i].Unpressing += OnUnpressColumnButton;
        }
        
        m_timer.SetTimeRemaing(MultiplicationQuadCoordinator.Instance.GetTimeLimit());
        
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

    void OnUnpressColumnButton(PipButton button)
    {
        int lastNumColumns = m_numColumns;

        m_numColumns += button.GetInt();
        m_numColumns = Mathf.Clamp(m_numColumns, 0, MultiplicationQuadCoordinator.Instance.GetMaxNumLines());

        if (m_numColumns != lastNumColumns)
        {
            RefreshLines();
        }
    }

    void OnUnpressRowButton(PipButton button)
    {
        int lastNumRows = m_numRows;

        m_numRows += button.GetInt();
        m_numRows = Mathf.Clamp(m_numRows, 0, MultiplicationQuadCoordinator.Instance.GetMaxNumLines());

        if (m_numRows != lastNumRows)
        {
            RefreshLines();
        }
    }

    void RefreshLines()
    {
        m_grid.maxPerLine = m_numRows;

        int numToSpawn = (m_numRows * m_numColumns) - m_grid.transform.childCount;

        if (numToSpawn > 0 && m_grid.transform)
        {
            GameObject pointPrefab = MultiplicationQuadCoordinator.Instance.GetPointPrefab();

            for(int i = 0; i < numToSpawn; ++i)
            {
                Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(pointPrefab, m_grid.transform);
            }
        }
        else if (numToSpawn < 0)
        {
            while(m_grid.transform.childCount > numToSpawn && m_grid.transform.childCount > 0)
            {
                GameObject pointToDestroy = m_grid.transform.GetChild(0);
                Destroy(pointToDestroy);
            }
        }

        m_grid.Reposition();
    }

    public void ClearQuestion()
    {

    }

    void OnUnpressGoButton(PipButton button)
    {

    }

    void OnTimerFinish(Timer timer)
    {
    }
}
