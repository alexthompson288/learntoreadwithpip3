using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;


public class CameraGUI : MonoBehaviour
{
	public GameObject target; // the object to apply the camera texture to
    private Texture2D texture; // local reference of the texture
    
#if UNITY_IPHONE
	void Start()
	{
		Screen.orientation = ScreenOrientation.LandscapeLeft;
	}
	
    
	void OnGUI()
	{
		float yPos = 5.0f;
		float xPos = 5.0f;
		float width = ( Screen.width >= 960 || Screen.height >= 960 ) ? 320 : 160;
		float height = ( Screen.width >= 960 || Screen.height >= 960 ) ? 80 : 40;
		float heightPlus = height + 10.0f;
	

		if( GUI.Button( new Rect( xPos, yPos, width, height ), "Start Capture (low)" ) )
		{
			// start the camera capture and use the returned texture
	        texture = LiveTextureBinding.startCameraCapture( false, LTCapturePreset.Size192x144 );
	        target.renderer.sharedMaterial.mainTexture = texture;
	        LiveTextureBinding.updateMaterialUVScaleForTexture( target.renderer.sharedMaterial, texture );
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Start Capture (high)" ) )
		{
	        texture = LiveTextureBinding.startCameraCapture( false, LTCapturePreset.Size1280x720 );
	        target.renderer.sharedMaterial.mainTexture = texture;
	        LiveTextureBinding.updateMaterialUVScaleForTexture( target.renderer.sharedMaterial, texture );
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Start Capture (front, low)" ) )
		{
	        texture = LiveTextureBinding.startCameraCapture( true, LTCapturePreset.Size192x144 );
	        target.renderer.sharedMaterial.mainTexture = texture;
	        LiveTextureBinding.updateMaterialUVScaleForTexture( target.renderer.sharedMaterial, texture );
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Start Capture (front, high)" ) )
		{
	        texture = LiveTextureBinding.startCameraCapture( true, LTCapturePreset.Size640x480 );
	        target.renderer.sharedMaterial.mainTexture = texture;
	        LiveTextureBinding.updateMaterialUVScaleForTexture( target.renderer.sharedMaterial, texture );
		}
		

		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Stop Capture" ) )
		{
	        LiveTextureBinding.stopCameraCapture();
	        Destroy( texture );
	        texture = null;
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Set Exposure Mode" ) )
		{
			LiveTextureBinding.setExposureMode( LTExposureMode.ContinuousAutoExposure );
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Set Focus Mode" ) )
		{
			LiveTextureBinding.setFocusMode( LTFocusMode.ContinuousAutoFocus );
		}

		
		xPos = Screen.width - width - 5.0f;
		yPos = Screen.height - height - 5.0f;
		if( GUI.Button( new Rect( xPos, yPos, width, height ), "Next" ) )
		{
	        LiveTextureBinding.stopCameraCapture();
	        Destroy( texture );
	        texture = null;
	        
	        Application.LoadLevel( "VideoTestScene" );
		}
	}
	
	
	void OnApplicationQuit()
	{
        LiveTextureBinding.stopCameraCapture();
        Destroy( texture );
        texture = null;
	}
	
	
	void OnApplicationPause( bool paused )
	{
		if( paused )
		{
	        LiveTextureBinding.stopCameraCapture();
	        Destroy( texture );
	        texture = null;
		}
	}
#endif

}
