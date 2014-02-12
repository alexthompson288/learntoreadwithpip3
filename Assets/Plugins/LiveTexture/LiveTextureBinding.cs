using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;



#if UNITY_IPHONE
public enum LTCapturePreset
{
	Size192x144, // same as low
	Size640x480,
	Size1280x720
}

public enum LTExposureMode
{
	Locked = 0,
	AutoExpose = 1,
	ContinuousAutoExposure = 2,
}

public enum LTFocusMode
{
	Locked = 0,
	AutoFocus = 1,
	ContinuousAutoFocus = 2,
}


public class LiveTextureBinding
{
	#region Camera methods
	
    [DllImport("__Internal")]
    private static extern bool _arIsCaptureAvailable();
 
	// Checks to see if the device has a supported camera and the proper iOS version
    public static bool isCaptureAvailable()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _arIsCaptureAvailable();
		return false;
    }
    
    
    [DllImport("__Internal")]
    private static extern void _arStartCameraCapture( bool useFrontCameraIfAvailable, int capturePreset, int textureId );

	// Starts the camera capture and returns a Texture2D that will have the camera output as it's content
    public static Texture2D startCameraCapture( bool useFrontCameraIfAvailable, LTCapturePreset capturePreset )
    {
		// force lower presets for devices that cant handle higher as a safety net
		if( useFrontCameraIfAvailable && capturePreset == LTCapturePreset.Size1280x720 )
			capturePreset = LTCapturePreset.Size640x480;
		
		if( iPhone.generation == iPhoneGeneration.iPhone3G )
			capturePreset = LTCapturePreset.Size192x144;

		var isMediumResDevice = ( iPhone.generation == iPhoneGeneration.iPad1Gen || iPhone.generation == iPhoneGeneration.iPhone3GS );
		if( capturePreset == LTCapturePreset.Size1280x720 && isMediumResDevice )
			capturePreset = LTCapturePreset.Size640x480;
		
    	int width = 0, height = 0;
		switch( capturePreset )
		{
			case LTCapturePreset.Size192x144:
				width = 192;
				height = 144;
				break;
			case LTCapturePreset.Size640x480:
				width = 640;
				height = 480;
				break;
			case LTCapturePreset.Size1280x720:
				width = 1280;
				height = 720;
				break;
		}

        // Create texture that will be updated in the plugin code
		Texture2D texture = new Texture2D( width, height, TextureFormat.ARGB32, false );
		
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_arStartCameraCapture( useFrontCameraIfAvailable, (int)capturePreset, texture.GetNativeTextureID() );
		
		return texture;
    }
    
    
    [DllImport("__Internal")]
    private static extern void _arStopCameraCapture();
 
	// Stops the camera capture
    public static void stopCameraCapture()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_arStopCameraCapture();
    }
	
	
	// Updates the materials UV offset to accommodate the texture being placed into the next biggest power of 2 container
	public static void updateMaterialUVScaleForTexture( Material material, Texture2D texture )
	{
		Vector2 textureOffset = new Vector2( (float)texture.width / (float)nearestPowerOfTwo( texture.width ), (float)texture.height / (float)nearestPowerOfTwo( texture.height ) );
		material.mainTextureScale = textureOffset;
	}
	
	
	private static int nearestPowerOfTwo( float number )
	{
		int n = 1;
		
		while( n < number )
			n <<= 1;

		return n;
	}
	
	
    [DllImport("__Internal")]
    private static extern void _arSetExposureMode( int exposureMode );
 
	// Sets the exposure mode.  Capture must be started for this to work!
    public static void setExposureMode( LTExposureMode mode )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_arSetExposureMode( (int)mode );
    }
    
    
    [DllImport("__Internal")]
    private static extern void _arSetFocusMode( int focusMode );
 
	// Sets the focus mode.  Capture must be started for this to work!
    public static void setFocusMode( LTFocusMode mode )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_arSetFocusMode( (int)mode );
    }

	#endregion
}
#endif