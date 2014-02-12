using UnityEngine;
using System.Collections;


public class AudioRecorderEventListener : MonoBehaviour
{
	void Start()
	{
		// listen for events
		AudioRecorderManager.audioRecorderDidFinish += audioRecorderDidFinish;
		AudioRecorderManager.audioRecorderFailed += audioRecorderFailed;
	}
	
	
	public void audioRecorderDidFinish( string filePath )
	{
		Debug.Log( "audioRecorderDidFinish event: " + filePath );
		
		// Playback is not supported in Unity of files from the web or docs directory yet so you
		// have to use the native audio player to play them
	}

	
	public void audioRecorderFailed( string error )
	{
		Debug.Log( "audioRecorderFailed event: " + error );
	}
}
