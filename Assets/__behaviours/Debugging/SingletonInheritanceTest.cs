using UnityEngine;
using System.Collections;

public class SingletonInheritanceTest : MonoBehaviour 
{
	// Use this for initialization
	IEnumerator Start () 
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        //CorrectCaptionCoordinator.Instance.LogSelf();
	}
}
