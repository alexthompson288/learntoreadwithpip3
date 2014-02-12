using UnityEngine;
using System;
using System.Collections.Generic;


// Any methods that Obj-C calls back using UnitySendMessage should be present here
public class AudioRecorderManager : MonoBehaviour
{
	// Event
	public delegate void AudioRecorderEventHandler( string data );
	public static event AudioRecorderEventHandler audioRecorderDidFinish;
	public static event AudioRecorderEventHandler audioRecorderFailed;
	
    void Awake()
    {
		// Set the GameObject name to the class name for easy access from Obj-C
		gameObject.name = this.GetType().ToString();
    }
	
	
	public void audioRecorderDidFinishRecording( string filePath )
	{
		// kick off the event
		if( audioRecorderDidFinish != null )
			audioRecorderDidFinish( filePath );
	}
	
	
	public void audioRecorderEncoderFailed( string error )
	{
		// kick off the event
		if( audioRecorderFailed != null )
			audioRecorderFailed( error );
	}

}