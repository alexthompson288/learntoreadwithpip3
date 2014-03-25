using UnityEngine;
using System.Collections;

public class SkipJourneyGame : MonoBehaviour 
{
	void OnClick()
	{
        PipHelpers.OnGameFinish(true, "NewVoyage"); // TODO: Make this sensible. This value is hardcoded because you need to play animations from Voyage, without entering a session.
	}
}
