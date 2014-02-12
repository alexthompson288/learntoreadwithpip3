using UnityEngine;
using System.Collections;

public class LettersChallengeMenuAudio : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start ()
	{
		yield return new WaitForSeconds(0.5f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("CHOOSE_GAME");
	}
}
