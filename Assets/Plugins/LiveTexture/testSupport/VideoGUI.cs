using UnityEngine;
using System.Collections.Generic;
using System;


public class VideoGUI : MonoBehaviour
{
	public GameObject targetOne; // the objects to apply the video texture to
	public GameObject targetTwo;
    
	
	#warning "For Demo Purposes Only. Please remove the testSupport folder before shipping. Audio support for video textures is currenty in beta. It is very resource intensive so depending on what else is happening in your game you may experience stutters during playback"

#if UNITY_IPHONE
	
	private VideoTexture _videoTexture;
	private VideoTexture _videoTextureTwo;

	
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
		
		
		if( GUI.Button( new Rect( xPos, yPos, width, height ), "Start Video Texture" ) )
		{
			// create the video texture
			_videoTexture = new VideoTexture( "vid.m4v", 640, 360, true );
			
			// apply the texture to a material and set the UVs
	        targetOne.renderer.sharedMaterial.mainTexture = _videoTexture.texture;
	        LiveTextureBinding.updateMaterialUVScaleForTexture( targetOne.renderer.sharedMaterial, _videoTexture.texture );
			
			// add some event handlers
			_videoTexture.videoDidStartEvent = () =>
			{
				Debug.Log( "Video one started" );
			};
			_videoTexture.videoDidFinishEvent = () =>
			{
				// when the video finishes if we are not set to loop this instance is no longer valid
				Debug.Log( "Video one finished" );
			};
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Pause" ) )
		{
			// null check in case Stop was pressed which will kill the VideoTexture
			if( _videoTexture != null )
				_videoTexture.pause();
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Unpause" ) )
		{
			// null check in case Stop was pressed which will kill the VideoTexture
			if( _videoTexture != null )
				_videoTexture.unpause();
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Stop" ) )
		{
			// null check in case Stop was pressed which will kill the VideoTexture
			if( _videoTexture != null )
			{
				_videoTexture.stop();
				_videoTexture = null;
			}
		}
		

		xPos = Screen.width - width - 5.0f;
		yPos = Screen.height - height - 5.0f;
		if( GUI.Button( new Rect( xPos, yPos, width, height ), "Back" ) )
		{
			// if the video texture is still playing kill it
			OnApplicationQuit();
	        
	        Application.LoadLevel( "CameraTestScene" );
		}
		
		
		if( GUI.Button( new Rect( 5, yPos, width, height ), "Play Unity Audio Clip" ) )
		{
			GetComponent<AudioSource>().Play();
		}

		
		
		// Second video texture
		yPos = 5.0f;
		xPos = Screen.width - width - 5;
		if( GUI.Button( new Rect( xPos, yPos, width, height ), "Start Video Texture Two" ) )
		{
			// create the video texture
			_videoTextureTwo = new VideoTexture( "vid2.m4v", 480, 264, false, 0, true );
			
			// apply the texture to a material and set the UVs
	        targetTwo.renderer.sharedMaterial.mainTexture = _videoTextureTwo.texture;
	        LiveTextureBinding.updateMaterialUVScaleForTexture( targetTwo.renderer.sharedMaterial, _videoTextureTwo.texture );
			
			// add some event handlers
			_videoTextureTwo.videoDidStartEvent = () =>
			{
				Debug.Log( "Video two started" );
			};
			_videoTextureTwo.videoDidFinishEvent = () =>
			{
				// when the video finishes if we are not set to loop this instance is no longer valid and this texture is not set to loop
				_videoTextureTwo = null;
				Debug.Log( "Video two finished" );
			};
		}
		
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Pause" ) )
		{
			// null check in case Stop was pressed which will kill the VideoTexture
			if( _videoTextureTwo != null )
				_videoTextureTwo.pause();
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Unpause" ) )
		{
			// null check in case Stop was pressed which will kill the VideoTexture
			if( _videoTextureTwo != null )
				_videoTextureTwo.unpause();
		}
		
		
		if( GUI.Button( new Rect( xPos, yPos += heightPlus, width, height ), "Stop" ) )
		{
			// null check in case Stop was pressed which will kill the VideoTexture
			if( _videoTextureTwo != null )
			{
				_videoTextureTwo.stop();
				_videoTextureTwo = null;
			}
		}
	}
	
	
	void OnApplicationQuit()
	{
        // if the video texture is still playing kill it
		if( _videoTexture != null )
		{
			_videoTexture.stop();
			_videoTexture = null;
		}
		
		if( _videoTextureTwo != null )
		{
			_videoTextureTwo.stop();
			_videoTextureTwo = null;
		}
	}
	
	
	void OnApplicationPause( bool paused )
	{
		if( paused )
		{
	        // if the video texture is still playing kill it
			if( _videoTexture != null )
			{
				_videoTexture.stop();
				_videoTexture = null;
			}
			
			if( _videoTextureTwo != null )
			{
				_videoTextureTwo.stop();
				_videoTextureTwo = null;
			}
		}
	}
	
#endif
	
}
