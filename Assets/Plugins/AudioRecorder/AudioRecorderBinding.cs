using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;


// All Objective-C exposed methods should be bound here
public class AudioRecorderBinding
{
    [DllImport("__Internal")]
    private static extern string _audioRecorderPrepareToRecordFile( string filename );
 
	// pass in just the filename, not the full path
    public static string prepareToRecordFile( string filename )
    {
        // Call plugin only when running on real device
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _audioRecorderPrepareToRecordFile( filename );
		return string.Empty;
    }


    [DllImport("__Internal")]
    private static extern bool _audioRecorderRecord();
 
    public static bool record()
    {
        // Call plugin only when running on real device
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _audioRecorderRecord();
		return false;
    }
	
	
	[DllImport("__Internal")]
    private static extern bool _audioRecorderRecordForDuration( float duration );
 
    public static bool recordForDuration( float duration )
    {
        // Call plugin only when running on real device
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _audioRecorderRecordForDuration( duration );
		return false;
    }


    [DllImport("__Internal")]
    private static extern void _audioRecorderPause();
 
    public static void pause()
    {
        // Call plugin only when running on real device
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_audioRecorderPause();
    }


    [DllImport("__Internal")]
    private static extern void _audioRecorderStop( bool finishRecording );
 
	// if finishRecording is true, the audio player will destroy itself after stopping
    public static void stop( bool finishRecording )
    {
        // Call plugin only when running on real device
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_audioRecorderStop( finishRecording );
    }
	
	
    [DllImport("__Internal")]
    private static extern void _audioRecorderEnableMetering( bool shouldEnable );
 
	// Sets if metering is available.  This is required to get average and peak power.  Do not enable
	// metering if you don't intend to use the peak/ave power levels because it eats CPU cycles
    public static void enableMetering( bool shouldEnable )
    {
        // Call plugin only when running on real device
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_audioRecorderEnableMetering( shouldEnable );
    }

	
    [DllImport("__Internal")]
    private static extern float _audioRecorderGetPeakPower();

	// Gets the peak power of the current recording. Between 0 and -160 dB though it can exceed 0 and become
	// positive in some cases.
    public static float getPeakPower()
    {
        // Call plugin only when running on real device
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _audioRecorderGetPeakPower();
		return 0.0f;
    }
	
	
    [DllImport("__Internal")]
    private static extern float _audioRecorderGetAveragePower();

	// Gets the average power of the current recording. Between 0 and -160 dB.
    public static float getAveragePower()
    {
        // Call plugin only when running on real device
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _audioRecorderGetAveragePower();
		return 0.0f;
    }
	
	
    [DllImport("__Internal")]
    private static extern float _audioRecorderGetCurrentTime();

    public static float getCurrentTime()
    {
        // Call plugin only when running on real device
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _audioRecorderGetCurrentTime();
		return 0.0f;
    }


    [DllImport("__Internal")]
    private static extern bool _audioRecorderIsRecording();
 
    public static bool isRecording()
    {
        // Call plugin only when running on real device
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _audioRecorderIsRecording();
		return false;
    }

	
	// loads up the audio file and returns an AudioClip ready for use in the callback
	public static IEnumerator loadAudioFileAtPath( string file, Action<string> onFailure, Action<AudioClip> onSuccess )
	{
		var www = new WWW( file );
		
		yield return www;
		
		if( www.error != null )
		{
			if( onFailure != null )
				onFailure( www.error );
		}

		if( www.audioClip )
		{
			if( onSuccess != null )
				onSuccess( www.GetAudioClip( false ) );
		}
		
		www.Dispose();
	}


}
