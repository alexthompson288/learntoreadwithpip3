using UnityEngine;
using System.Collections;

public class PipHelpers : MonoBehaviour 
{
	public static void OnGameFinish(bool won = true)
	{
		if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Voyage)
		{
			JourneyInformation.Instance.OnGameFinish();
		}
		else if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Custom)
		{
			LessonInfo.Instance.OnGameFinish();
		}
		else
		{
			TransitionScreen.Instance.ChangeLevel("NewScoreDanceScene", false);
		}
	}

	public static void SetDefaultPlayerVar()
	{
		SessionInformation.Instance.SetNumPlayers(1);
		SessionInformation.Instance.SetWinner(0);
		SessionInformation.Instance.SetPlayerIndex(0, 3);
	}
}
