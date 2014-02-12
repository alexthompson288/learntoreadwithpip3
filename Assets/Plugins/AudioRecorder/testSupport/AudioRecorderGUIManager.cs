using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;


[RequireComponent( typeof( AudioSource ) )]
public class AudioRecorderGUIManager : MonoBehaviour
{
	private const string filename = "myAudioFile.wav";
	
	
	void Start()
	{
		if( audio == null )
			gameObject.AddComponent<AudioSource>();
	}
	
	
	void OnGUI()
	{
		float yPos = 5.0f;
		float xPos = 5.0f;
		float width = ( Screen.width >= 800 || Screen.height >= 800 ) ? 320 : 160;
		float height = ( Screen.width >= 800 || Screen.height >= 800 ) ? 60 : 30;
		float heightPlus = height + 10.0f;
		
		
		if( GUI.Button( new Rect( xPos, yPos, width, height ), "Prepare Audio Recorder" ) )
		{
			string error = AudioRecorderBinding.prepareToRecordFile( filename );
			if( error.Length > 0 )
				Debug.Log( "failed to prepare audio recorder: " + error );
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Start Recording" ) )
		{
			bool didRecord = AudioRecorderBinding.record();
			Debug.Log( "audioRecorderRecord: " + didRecord );
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Record for 3 Seconds" ) )
		{
			bool didRecord = AudioRecorderBinding.recordForDuration( 3 );
			Debug.Log( "audioRecorderRecordForDuration: " + didRecord );
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Pause Recording" ) )
		{
			AudioRecorderBinding.pause();
		}

		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Stop Recording" ) )
		{
			AudioRecorderBinding.stop( false );
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Stop and Finish Recording" ) )
		{
			AudioRecorderBinding.stop( true );
		}
		

		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Is Recording" ) )
		{
			bool isRecording = AudioRecorderBinding.isRecording();
			Debug.Log( "AudioRecorder isRecording: " + isRecording );
		}
		

		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Get Current Recording Duration" ) )
		{
			float duration = AudioRecorderBinding.getCurrentTime();
			Debug.Log( "AudioRecorder current duration: " + duration );
		}

		
		// Second column
		xPos = Screen.width - width - 5.0f;
		yPos = 5.0f;
		
		if( GUI.Button( new Rect( xPos, yPos, width, height ), "Play Audio File" ) )
		{
			var file = "file://" + Application.persistentDataPath + "/" + filename;
			
			var onFailure = new Action<string>( error => Debug.Log( error ) );
			var onSuccess = new Action<AudioClip>( clip =>
			{
				audio.clip = clip;
				audio.Play();
			});
			StartCoroutine( AudioRecorderBinding.loadAudioFileAtPath( file, onFailure, onSuccess ) );
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "(alt) Play Audio File" ) )
		{
			var file = "file://" + Application.persistentDataPath + "/" + filename;

			StartCoroutine( AudioRecorderBinding.loadAudioFileAtPath( file, onError, onSuccess ) );
		}
	}
	
	
	public void onError( string error )
	{
		Debug.LogError( "error loading audio file: " + error );
	}
	
	
	public void onSuccess( AudioClip clip )
	{
		audio.clip = clip;
		audio.Play();
	}


}
