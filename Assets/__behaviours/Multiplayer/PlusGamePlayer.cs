using UnityEngine;
using System.Collections;

public class PlusGamePlayer : GamePlayer 
{
    [SerializeField]
    protected PlusScoreKeeper m_scoreKeeper;
    [SerializeField]
    protected TrafficLights m_trafficLights;

    public IEnumerator CelebrateVictory()
    {
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
