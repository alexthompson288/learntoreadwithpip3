using UnityEngine;
using System.Collections;

public class SkipJourneyGame : MonoBehaviour 
{
	void OnClick()
	{
		SessionManager.Instance.OnGameFinish();
	}
}
