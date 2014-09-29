using UnityEngine;
using System.Collections;

public class MultiplicationQuadPlayer : GamePlayer
{
    [SerializeField]
    private ScoreHealth m_scoreKeeper;
    [SerializeField]
    private TrafficLights m_trafficLights;
    [SerializeField]
    private EventRelay m_submitButton;
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
    [SerializeField]
    private UILabel m_targetLabel;
    [SerializeField]
    private UISprite m_targetSplodge;

    int m_numRows = 1;
    int m_numColumns = 1;

    DataRow m_currentData = null;

    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
    }

    public override void SelectCharacter(int characterIndex)
    {
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }

        m_scoreKeeper.SetCharacterIcon(characterIndex);
        MultiplicationQuadCoordinator.Instance.CharacterSelected(characterIndex);
    }

    public void StartGame(bool subscribeToTimer)
    {
        m_submitButton.SingleClicked += OnClickGoButton;

        for (int i = 0; i < m_rowButtons.Length; ++i)
        {
            m_rowButtons[i].Unpressing += OnUnpressRowButton;
        }

        for (int i = 0; i < m_columnButtons.Length; ++i)
        {
            m_columnButtons[i].Unpressing += OnUnpressColumnButton;
        }
        
        m_scoreKeeper.LevelledUp += OnLevelUp;
        m_scoreKeeper.Completed += OnScoreKeeperComplete;
        m_scoreKeeper.StartTimer();

        RefreshLines();

        AskQuestion();
    }

    public void SetCurrentData(DataRow myCurrentData)
    {
        m_currentData = myCurrentData;
    }

    void AskQuestion()
    {
        m_targetLabel.text = m_currentData.GetInt("value").ToString();
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

    void ChangeSplodgeColor(ColorInfo.PipColor splodgeColor)
    {
        StopCoroutine("ChangeSplodgeColorCo");
        m_targetSplodge.color = ColorInfo.GetColor(splodgeColor);
        StartCoroutine("ChangeSplodgeColorCo");
    }

    IEnumerator ChangeSplodgeColorCo()
    {
        yield return new WaitForSeconds(0.75f);
        m_targetSplodge.color = Color.white;
    }

    public IEnumerator ClearQuestion ()
    {
        m_numRows = 1;
        m_numColumns = 1;
        RefreshLines();

        yield return new WaitForSeconds(0.8f);

        AskQuestion();
    }

    void OnClickGoButton (EventRelay relay)
    {
        if (m_grid.transform.childCount == m_currentData.GetInt("value"))
        {
            ChangeSplodgeColor(ColorInfo.PipColor.LightGreen);
            m_scoreKeeper.UpdateScore(1);
            MultiplicationQuadCoordinator.Instance.OnCorrectAnswer(this);
        }
        else
        {
            ChangeSplodgeColor(ColorInfo.PipColor.LightRed);
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            m_scoreKeeper.UpdateScore(-1);
        }
    }

    void OnLevelUp(ScoreKeeper scoreKeeper)
    {
        MultiplicationQuadCoordinator.Instance.OnLevelUp();
    }
    
    void OnScoreKeeperComplete(ScoreKeeper scoreKeeper)
    {
        MultiplicationQuadCoordinator.Instance.CompleteGame();
    }

    public IEnumerator CelebrateVictory()
    {
        if (SessionInformation.Instance.GetNumPlayers() == 2)
        {
            yield return new WaitForSeconds(0.8f);
            WingroveAudio.WingroveRoot.Instance.PostEvent(string.Format("PLAYER_{0}_WIN", m_selectedCharacter));
            CelebrationCoordinator.Instance.DisplayVictoryLabels(m_playerIndex);
            CelebrationCoordinator.Instance.PopCharacter(m_selectedCharacter, true);
            yield return new WaitForSeconds(2f);
        }

        yield return StartCoroutine(m_scoreKeeper.Celebrate());
    }

    public int GetScore()
    {
        return m_scoreKeeper.GetScore();
    }
}
