using UnityEngine;
using System.Collections;

public class SetBubbleGame : MonoBehaviour 
{
	[SerializeField]
	private string m_bubbleGame;

	void OnClick()
	{
		BubbleMapCoordinator.SetBubbleGame(m_bubbleGame);
	}
}
