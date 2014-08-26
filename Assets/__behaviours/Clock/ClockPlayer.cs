using UnityEngine;
using System.Collections;
using System;

public class ClockPlayer : GamePlayer 
{
    [SerializeField]
    private PlusScoreKeeper m_scoreKeeper;
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
        ////D.Log("SelectCharacter");
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;
        ////D.Log("m_selectedCharacter: " + m_selectedCharacter);
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }
        
        m_scoreKeeper.SetCharacterIcon(characterIndex);
        ClockCoordinator.Instance.CharacterSelected(characterIndex);
    }

    public void StartGame()
    {
        m_scoreKeeper.SetHealthLostPerSecond(1f);

        m_goButton.Unpressing += OnPressGoButton;

        m_scoreKeeper.LevelledUp += OnLevelUp;
        m_scoreKeeper.Completed += OnScoreKeeperComplete;
        m_scoreKeeper.StartTimer();
        
        AskQuestion();
    }

    void AskQuestion()
    {
        //D.Log("m_currentData: " + m_currentData);
        //D.Log("time: " + m_currentData ["time"]);
        m_questionLabel.text = m_currentData ["time"].ToString();
    }

    void OnPressGoButton(PipButton button)
    {
        DateTime currentTime = Convert.ToDateTime(m_currentData ["datetime"]);
        DateTime clockTime = m_clock.GetDateTime();
        int cushion = ClockCoordinator.Instance.GetCushion();

        if(DateTime.Compare(clockTime.AddMinutes(cushion), currentTime) >= 0 && DateTime.Compare(clockTime.AddMinutes(-cushion), currentTime) <= 0)
        {
            //D.Log("CORRECT");
            //D.Log("Clock: " + clockTime);
            //D.Log("Current: " + currentTime);
            m_scoreKeeper.UpdateScore(1);
            ClockCoordinator.Instance.OnCorrectAnswer(this);
        }
        else
        {
            //D.Log("INCORRECT");
            //D.Log("Clock: " + clockTime);
            //D.Log("Current: " + currentTime);
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
        if (SessionInformation.Instance.GetNumPlayers() == 2)
        {
            yield return new WaitForSeconds(0.8f);
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
