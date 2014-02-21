using UnityEngine;
using System.Collections;

public class ChooseGame : MonoBehaviour 
{
	[SerializeField]
	private string m_gameSceneName;
	[SerializeField]
	private bool m_isTwoPlayer = false;

	void OnClick()
	{
		GameMenuCoordinator.Instance.OnChooseGame(m_gameSceneName, m_isTwoPlayer);
	}
}
