using UnityEngine;
using System.Collections;

public class PlusGamePlayer : GamePlayer 
{
    [SerializeField]
    protected PlusScoreKeeper m_scoreKeeper;
    [SerializeField]
    protected TrafficLights m_trafficLights;

    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
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

    public override void SelectCharacter(int characterIndex)
    {
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;

        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }
        
        m_scoreKeeper.SetCharacterIcon(characterIndex);
    }
}
