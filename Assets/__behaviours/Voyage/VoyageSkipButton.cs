using UnityEngine;
using System.Collections;

public class VoyageSkipButton : MonoBehaviour 
{
	void OnClick()
	{
        GameManager.Instance.CompleteGame(true, "NewVoyage"); // TODO: Make this sensible. This value is hardcoded because you need to play animations from Voyage, without entering a session.
	}
}
