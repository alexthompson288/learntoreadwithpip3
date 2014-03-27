using UnityEngine;
using System.Collections;

public class PipHelpers : MonoBehaviour 
{
	public static void OnGameFinish(bool won = true, string setsScene = "NewScoreDanceScene")
	{
        if (UserStats.Activity.Current != null)
        {
            UserStats.Activity.Current.EndEvent(true);
        }

		if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Sets)
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
