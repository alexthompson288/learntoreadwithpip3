using UnityEngine;
using System.Collections;

public class ClockPlayer : GamePlayer 
{
    [SerializeField]
    private ScoreHealth m_scoreKeeper;
    [SerializeField]
    private TrafficLights m_trafficLights;

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

    public void StartGame()
    {

    }
}
