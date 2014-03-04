using UnityEngine;
using System.Collections;

public class ChooseGame : BuyableGame 
{
	void OnClick()
	{
		if(m_isUnlocked)
		{
			FlurryBinding.logEvent("Choose Game: " + m_gameSceneName, false);
			GameMenuCoordinator.Instance.OnChooseGame(m_gameSceneName, m_isTwoPlayer);
		}
		else
		{
			BuyGamesCoordinator.Instance.Show();
		}
	}
}
