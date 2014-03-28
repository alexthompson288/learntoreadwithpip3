using UnityEngine;
using System.Collections;

public class GameHelpers : MonoBehaviour 
{
	public static void OnGameFinish(bool won = true, string setsScene = "NewScoreDanceScene")
	{
        UserStats.Activity.End(true);

		if(Game.session == Game.Session.Single)
		{
			TransitionScreen.Instance.ChangeLevel(setsScene, false);
		}
		else
		{
			SessionManager.Instance.OnGameFinish();
		}
	}

	public static void SetDefaultPlayerVar()
	{
		SessionInformation.Instance.SetNumPlayers(1);
		SessionInformation.Instance.SetWinner(0);
		SessionInformation.Instance.SetPlayerIndex(0, 3);
	}
}
