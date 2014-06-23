using UnityEngine;
using System.Collections;

public class AudioTest : MonoBehaviour 
{
#if UNITY_EDITOR	
	// Update is called once per frame
	void Update () 
    {
	    if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Calling SPARKLE_2");
            WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
        }
	}
#endif
}
