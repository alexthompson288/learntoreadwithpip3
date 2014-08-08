using UnityEngine;
using System.Collections;

public class ClockPlayer : GamePlayer 
{
    [SerializeField]
    private ScoreHealth m_scoreKeeper;
    [SerializeField]
    private TrafficLights m_trafficLights;
    [SerializeField]
    private UILabel m_questionLabel;
    [SerializeField]
    private PipButton m_goButton;
    [SerializeField]
    private Clock m_clock;

    DataRow m_currentData;

    public void SetCurrentData(DataRow myCurrentData)
    {
        m_currentData = myCurrentData;
    }

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
        
        m_scoreKeeper.SetCharacterIcon(characterIndex);
        ClockCoordinator.Instance.CharacterSelected(characterIndex);
    }

    public void StartGame(bool subscribeToTimer)
    {
        m_scoreKeeper.SetHealthLostPerSecond(0.5f);

        m_goButton.Unpressing += OnPressGoButton;

        m_scoreKeeper.LevelledUp += OnLevelUp;
        m_scoreKeeper.Completed += OnScoreKeeperComplete;
        m_scoreKeeper.StartTimer();
        
        AskQuestion();
    }

    void AskQuestion()
    {
        D.Log("m_currentData: " + m_currentData);
        D.Log("time: " + m_currentData ["time"]);
        m_questionLabel.text = m_currentData ["time"].ToString();
    }

    void OnPressGoButton(PipButton button)
    {
        if (m_clock.GetDateTime() == System.Convert.ToDateTime(m_currentData ["datetime"]))
        {
            m_scoreKeeper.UpdateScore(1);
            ClockCoordinator.Instance.OnCorrectAnswer(this);
        }
        else
        {
            D.Log("INCORRECT");
            D.Log("Clock: " + m_clock.GetDateTime());
            D.Log("Target: " + System.Convert.ToDateTime(m_currentData ["datetime"]));
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            m_scoreKeeper.UpdateScore(-1);
        }
    }

    public IEnumerator ClearQuestion()
    {
        yield return null;

        AskQuestion();
    }

    void OnScoreKeeperComplete(ScoreKeeper scoreKeeper)
    {
        ClockCoordinator.Instance.CompleteGame();
    }

    void OnLevelUp(ScoreKeeper scoreKeeper)
    {
        ClockCoordinator.Instance.OnLevelUp();
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
