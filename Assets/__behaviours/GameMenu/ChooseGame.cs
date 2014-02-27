using UnityEngine;
using System.Collections;

public class ChooseGame : BuyableGame 
{
	[SerializeField]
	private bool m_isTwoPlayer = false;

	void OnClick()
	{
		if(m_isUnlocked)
		{
			GameMenuCoordinator.Instance.OnChooseGame(m_gameSceneName, m_isTwoPlayer);
		}
		else
		{
			BuyGamesCoordinator.Instance.Show();
		}
	}
}
