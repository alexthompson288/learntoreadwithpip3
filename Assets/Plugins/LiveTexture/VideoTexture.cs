using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;


#if UNITY_IPHONE
public class VideoTexture
{
	// If true, the Texture2D will be automatically destroyed for you and all resources freed. If false, it is your responsibility to call
	// stop to destory the Texture2D.
	public bool autoDestructOnCompletion = true;
	
	// The video texture. This is valid immediately after creating a VideoTexture instance
	public Texture2D texture { get; private set; }
	
	// Fired when a video texture begins playing
	public Action videoDidStartEvent;
	
	// Fired when a video texture is done playing
	public Action videoDidFinishEvent;
	
	private string _instanceId;
	
	
	[DllImport("__Internal")]
    private static extern void _arStartVideoTexturePlayback( string instanceId, string filename, int textureId, bool shouldLoop, float startTime, bool playAudio );
	
	[DllImport("__Internal")]
    private static extern void _arPauseVideoTexturePlayback( string instanceId );
	
	[DllImport("__Internal")]
    private static extern void _arUnpauseVideoTexturePlayback( string instanceId );
	
	[DllImport("__Internal")]
    private static extern void _arStopVideoTexturePlayback( string instanceId );
	
	[DllImport("__Internal")]
    private static extern void _arSetGainForTextureInstance( string instanceId, float gain );
	
	
	// Constructor. used to create a video texture
	public VideoTexture( string filename, int width, int height, bool shouldLoop = false, float startTime = 0, bool playAudio = false )
	{
		_instanceId = Prime31.Utils.randomString();
		
        // Create texture that will be updated in the plugin code
		texture = new Texture2D( width, height, TextureFormat.ARGB32, false );
    
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_arStartVideoTexturePlayback( _instanceId, filename, texture.GetNativeTextureID(), shouldLoop, startTime, playAudio );
		
		VideoTextureManager.registerInstance( _instanceId, this );
	}
	
	
	// Pauses the video texture
    public void pause()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_arPauseVideoTexturePlayback( _instanceId );
    }

    
	// Unpauses the video texture
    public void unpause()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_arUnpauseVideoTexturePlayback( _instanceId );
    }


	// Stops and releases the video texture player.  The Texture2D will be destroyed automatically for you
    public void stop()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			_arStopVideoTexturePlayback( _instanceId );
			cleanup();
		}
    }
	
	
	// Sets the gain for the instance clamped from 0 to 1
	public void setGain( float gain )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_arSetGainForTextureInstance( _instanceId, Mathf.Clamp01( gain ) );
	}
	
	
	private void cleanup()
	{
		UnityEngine.Object.Destroy( texture );
		texture = null;
		VideoTextureManager.deRegisterInstance( _instanceId );
	}
	
	
	// called internally
	public void onStarted()
	{
		if( videoDidStartEvent != null )
			videoDidStartEvent();
	}
	
	
	// called internally
	public void onComplete()
	{
		if( videoDidFinishEvent != null )
			videoDidFinishEvent();

		// if we are set to autodestruct do so
		if( autoDestructOnCompletion )
			stop();
	}

}
#endif