using UnityEngine;
using System.Collections;

public class BootScene : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
        TransitionScreen ts = (TransitionScreen)GameObject.FindObjectOfType(typeof(TransitionScreen));
        ts.ChangeLevel("NewStoryBrowser", false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
