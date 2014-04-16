using UnityEngine;
using System.Collections;

public class ChooseGame : BuyableGame 
{
	void OnClick()
	{
		if(m_isUnlocked)
		{
#if UNITY_IPHONE
			System.Collections.Generic.Dictionary<string, string> ep = new System.Collections.Generic.Dictionary<string, string>();
			ep.Add("GameSceneName", m_gameSceneName);
			FlurryBinding.logEventWithParameters("GameMenu - Game", ep, false);
#endif

			OldGameMenuCoordinator.Instance.OnChooseGame(m_gameSceneName, m_isTwoPlayer);
		}
		else
		{
			BuyGamesCoordinator.Instance.Show();
		}
	}
}
