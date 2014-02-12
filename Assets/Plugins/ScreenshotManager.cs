#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;

public class ScreenshotManager : MonoBehaviour {
	
	[DllImport("__Internal")]
    private static extern bool saveToGallery( string path );
	
	public static IEnumerator Save(string fileName, string albumName = "MyScreenshots")
	{
		string date = System.DateTime.Now.ToString("dd-MM-yy");
		
		ScreenshotManager.ScreenShotNumber++;
		
		string screenshotFilename = fileName + "_" + ScreenshotManager.ScreenShotNumber + "_" + date + ".png";
		
		Debug.Log("Save screenshot " + screenshotFilename); 
		
		#if UNITY_IPHONE
		
			if(Application.platform == RuntimePlatform.IPhonePlayer) 
			{
				Debug.Log("iOS platform detected");
				
				string iosPath = Application.persistentDataPath + "/" + screenshotFilename;
		
				Application.CaptureScreenshot(screenshotFilename);
				
				while(true) 
				{
					bool photoSaved = saveToGallery( iosPath );
					
					if(photoSaved) return false;
					
					yield return new WaitForSeconds(.5f);
				}
			
			} else {
			
				Application.CaptureScreenshot(screenshotFilename);
			
			}
			
		#elif UNITY_ANDROID	
				
				if(Application.platform == RuntimePlatform.Android) 
				{
					Debug.Log("Android platform detected");
					
					string androidPath = "/../../../../DCIM/" + albumName + "/" + screenshotFilename;
					string path = Application.persistentDataPath + androidPath;
					string pathonly = Path.GetDirectoryName(path);
					Directory.CreateDirectory(pathonly);
					Application.CaptureScreenshot(androidPath);
					
					AndroidJavaClass obj = new AndroidJavaClass("com.ryanwebb.androidscreenshot.MainActivity");
					
					while(true) 
					{
						bool photoSaved = obj.CallStatic<bool>("scanMedia", path);
				
						if(photoSaved) return false;
							
						yield return new WaitForSeconds(.5f);
					}
			
				} else {
			
					Application.CaptureScreenshot(screenshotFilename);
			
				}
		#else
			
			while(true) 
			{
				yield return new WaitForSeconds(.5f);
		
				Debug.Log("Screenshots only available in iOS/Android mode!");

                yield break;
			}
		
		#endif
	}
	
	
	public static IEnumerator SaveExisting(string filePath)
	{
		Debug.Log("Save existing file to gallery " + filePath);

		#if UNITY_IPHONE
		
			if(Application.platform == RuntimePlatform.IPhonePlayer) 
			{
				Debug.Log("iOS platform detected");
				
				while(true) 
				{
					bool photoSaved = saveToGallery( filePath );
					
					if(photoSaved) return false;
					
					yield return new WaitForSeconds(.5f);
				}
			
			}
			
		#elif UNITY_ANDROID	
				
			if(Application.platform == RuntimePlatform.Android) 
			{
				Debug.Log("Android platform detected");

				AndroidJavaClass obj = new AndroidJavaClass("com.ryanwebb.androidscreenshot.MainActivity");
					
				while(true) 
				{
					bool photoSaved = obj.CallStatic<bool>("scanMedia", filePath);
				
					if(photoSaved) return false;
							
					yield return new WaitForSeconds(.5f);
				}
			
			}
		
		#else
			
			while(true) 
			{
				yield return new WaitForSeconds(.5f);
		
				Debug.Log("Save existing file only available in iOS/Android mode!");

                yield break;
			}
		
		#endif
	}
	
	
	public static int ScreenShotNumber 
	{
		set { PlayerPrefs.SetInt("screenShotNumber", value); }
	
		get { return PlayerPrefs.GetInt("screenShotNumber"); }
	}
}
