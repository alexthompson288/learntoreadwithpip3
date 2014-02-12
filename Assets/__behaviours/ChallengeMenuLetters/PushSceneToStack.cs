using UnityEngine;
using System.Collections;

// TODO: Deprecate this class and find a better way of making sure that the Challenge Menu stays on the stack or gets put back on the stack when you leave it
public class PushSceneToStack : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		TransitionScreen.Instance.PushToStack("NewChallengeMenuLetters");
	}
}
