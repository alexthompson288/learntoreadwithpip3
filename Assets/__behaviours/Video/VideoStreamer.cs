using UnityEngine;
using System.Collections;
using System.IO;

public class VideoStreamer : Singleton<VideoStreamer> 
{
	[SerializeField]
	private bool m_playFromStreamingAssetsOnStart;

    Color m_bgColor = Color.black;
    FullScreenMovieControlMode m_controlMode = FullScreenMovieControlMode.Full;
    FullScreenMovieScalingMode m_scalingMode = FullScreenMovieScalingMode.AspectFit;

	string m_url = "https://s3-eu-west-1.amazonaws.com/pipvideotest";

	string m_testFileName = "pip_and_diego_the_dancer.mp4";

	void Start()
	{
		Debug.Log ("VideoStreamer.Start()");

		if (m_playFromStreamingAssetsOnStart) 
		{
			PlayFromStreamingAssets("local_" + m_testFileName);
		}
	}

	public string GetTestFileName()
	{
		return m_testFileName;
	}

	public static IEnumerator WaitForInstance()
	{
		while (VideoStreamer.Instance == null) 
		{
			yield return null;
		}
	}

	public void PlayFromStreamingAssets(string fileName)
	{
		Debug.Log ("VideoStreamer.PlayFromStreamingAssets()");

		Debug.Log ("fileName: " + fileName);

		Debug.Log ("PLAY STREAMINGASSETS MOVIE");
		
		Handheld.PlayFullScreenMovie (fileName, m_bgColor, m_controlMode, m_scalingMode);
		
		Debug.Log ("FINISHED STREAMINGASSETS MOVIE");
	}
    
    public IEnumerator DownloadAndPlayVideo (string fileName) 
    {
		Debug.Log ("VideoStreamer.DownloadAndPlayVideo()");

        UICamera[] uiCams = Object.FindObjectsOfType(typeof(UICamera)) as UICamera[];

        foreach (UICamera cam in uiCams)
        {
            cam.enabled = false;
        }

        if (fileName [0] != '/')
        {
            fileName = "/" + fileName;
        }

        string videoFile = Application.persistentDataPath + fileName;
        string playVideoFile = videoFile;

#if UNITY_IPHONE
        playVideoFile = "file://" + videoFile;
#elif UNITY_ANDROID
		Debug.Log("LOG ANDROID");
		//Debug.Log("Use Streaming Assets Path");
		//videoFile = Application.streamingAssetsPath + fileName;
		//playVideoFile = videoFile;
#endif

		Debug.Log ("LOG VAR");
		Debug.Log ("fileName: " + fileName);
		Debug.Log ("videoFile: " + videoFile);
		Debug.Log ("playVideoFile: " + playVideoFile);

		if (!File.Exists (videoFile)) 
		{
			Debug.Log ("CREATE WWW");
			WWW www = new WWW (m_url + fileName);
			Debug.Log ("WAIT");
			yield return www;
			Debug.Log ("FINISHED DOWNLOAD");
			if (www != null && www.isDone && www.error == null) 
			{
				Debug.Log ("WRITING");
				FileStream stream = new FileStream (videoFile, FileMode.Create);
				stream.Write (www.bytes, 0, www.bytes.Length);
				stream.Close ();
			} 
			else 
			{
				Debug.Log ("CANNOT WRITE");
				Debug.Log ("www != null: " + www != null);
				Debug.Log ("www.isDone: " + www.isDone);
				Debug.Log ("www.error == null: " + www.error == null);
			}
		} 
		else 
		{
			Debug.Log ("FOUND LOCAL");
		}

		Debug.Log ("PLAY DOWNLOAD MOVIE");
		
		Handheld.PlayFullScreenMovie (playVideoFile, m_bgColor, m_controlMode, m_scalingMode);
		
		Debug.Log ("FINISHED DOWNLOAD MOVIE");
		

	    foreach (UICamera cam in uiCams)
	    {
	        cam.enabled = true;
	    }
    }
}
