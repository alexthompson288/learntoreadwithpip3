using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class GamePlayer : MonoBehaviour 
{
    [SerializeField]
    protected int m_playerIndex;

    protected List<CharacterSelection> m_characterSelections = new List<CharacterSelection>();
    protected int m_selectedCharacter = -1;

    public void RegisterCharacterSelection(CharacterSelection characterSelection)
    {
        m_characterSelections.Add(characterSelection);
    }
    
    public virtual void SelectCharacter(int characterIndex)
    {
        Debug.Log("SelectCharacter");
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;
        Debug.Log("m_selectedCharacter: " + m_selectedCharacter);
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }
        SplatRatGameCoordinator.Instance.CharacterSelected(characterIndex);
    }
    
    public void HideCharacter(int index)
    {
        foreach (CharacterSelection cs in m_characterSelections)
        {
            if (cs.GetCharacterIndex() == index)
            {
                cs.DeactivatePress(false);
            }
        }
    }
    
    public void HideAll()
    {
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(true);
        }
    }
    
    public bool HasSelectedCharacter()
    {
        return (m_selectedCharacter != -1);
    }
}
