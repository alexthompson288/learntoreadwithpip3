using UnityEngine;
using System.Collections;

public class SetFlashcardGame : MonoBehaviour 
{
	[SerializeField]
	private string m_flashcardGame;

	void OnClick()
	{
		FlashcardMapCoordinator.SetFlashcardGame(m_flashcardGame);
	}
}
