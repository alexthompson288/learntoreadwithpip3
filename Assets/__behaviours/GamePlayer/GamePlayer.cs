using UnityEngine;
using System.Collections;

public abstract class GamePlayer : MonoBehaviour 
{
	public abstract void RegisterCharacterSelection(CharacterSelection characterSelection);
	public abstract void SelectCharacter(int characterIndex);
}
