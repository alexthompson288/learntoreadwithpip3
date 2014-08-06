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
    [SerializeField]
    private UILabel m_rowLabel;
    [SerializeField]
    private UILabel m_columnLabel;

    int m_numRows = 1;
    int m_numColumns = 1;

    DataRow m_currentData = null;

    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
    }

    public override void SelectCharacter(int characterIndex)
    {
        //D.Log("SelectCharacter");
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;
        //D.Log("m_selectedCharacter: " + m_selectedCharacter);
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }

        MultiplicationQuadCoordinator.Instance.CharacterSelected(characterIndex);
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
            m_timer.Finished += OnTimerFinish;
        }
        
        m_timer.On();

        RefreshLines();

        AskQuestion();
    }

    public void SetCurrentData(DataRow myCurrentData)
    {
        m_currentData = myCurrentData;
    }

    void AskQuestion()
    {
        m_dataDisplay.On(m_currentData);
    }

    void OnUnpressColumnButton(PipButton button)
    {
        int lastNumColumns = m_numColumns;

        m_numColumns += button.GetInt();
        m_numColumns = Mathf.Clamp(m_numColumns, 1, MultiplicationQuadCoordinator.Instance.GetMaxNumLines());

        if (m_numColumns != lastNumColumns)
        {
            RefreshLines();
        }
    }

    void OnUnpressRowButton(PipButton button)
    {
        int lastNumRows = m_numRows;

        m_numRows += button.GetInt();
        m_numRows = Mathf.Clamp(m_numRows, 1, MultiplicationQuadCoordinator.Instance.GetMaxNumLines());

        if (m_numRows != lastNumRows)
        {
            RefreshLines();
        }
    }

    void RefreshLines()
    {
        m_grid.maxPerLine = m_numRows;

        int delta = (m_numRows * m_numColumns) - m_grid.transform.childCount;

        if (delta > 0)
        {
            GameObject pointPrefab = MultiplicationQuadCoordinator.Instance.GetPointPrefab();

            for(int i = 0; i < delta; ++i)
            {
                Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(pointPrefab, m_grid.transform);
            }
        }
        else if (delta < 0)
        {
            int numToDestroy = Mathf.Min(Mathf.Abs(delta), m_grid.transform.childCount);
            for(int i = 0; i < numToDestroy; ++i)
            {
                GameObject pointToDestroy = m_grid.transform.GetChild(m_grid.transform.childCount - i - 1).gameObject;
                Destroy(pointToDestroy);
            }
        }

        m_grid.Reposition();


        m_rowLabel.text = m_numRows.ToString();
        m_columnLabel.text = m_numColumns.ToString();
    }

    public IEnumerator ClearQuestion ()
    {
        m_dataDisplay.Off();

        /*
        int numToDestroy = m_grid.transform.childCount;
        for (int i = 0; i < numToDestroy; ++i)
        {
            GameObject pointToDestroy = m_grid.transform.GetChild(i).gameObject;
            Destroy(pointToDestroy);
        }
        */
        m_numRows = 1;
        m_numColumns = 1;
        RefreshLines();

        yield return new WaitForSeconds(0.25f);

        AskQuestion();
    }

    void OnUnpressGoButton (PipButton button)
    {
        if (m_grid.transform.childCount == m_currentData.GetInt("value"))
        {
            m_scoreKeeper.UpdateScore(1);
            MultiplicationQuadCoordinator.Instance.OnCorrectAnswer(this);
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
        }
    }

    void OnTimerFinish (Timer timer)
    {
        MultiplicationQuadCoordinator.Instance.CompleteGame();
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
