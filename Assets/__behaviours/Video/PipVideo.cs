using UnityEngine;
using System.Collections;
using System.IO;

public class PipVideo : Singleton<PipVideo> 
{
    Color m_bgColor = Color.black;

#if UNITY_IPHONE || UNITY_ANDROID
    FullScreenMovieControlMode m_controlMode = FullScreenMovieControlMode.Full;
    FullScreenMovieScalingMode m_scalingMode = FullScreenMovieScalingMode.AspectFit;
#endif

    string m_url = "https://s3-eu-west-1.amazonaws.com/pipvideotest";

    //string m_testFileName = "pip_and_diego_the_dancer.mp4";
    string m_testFileName = "intro_rhyming_2048.mp4";

    public void TestLocal()
    {
        Debug.Log("TEST LOCAL");
        Debug.Log("Playing local");
#if UNITY_IPHONE || UNITY_ANROID
        Handheld.PlayFullScreenMovie("Pipisodes/mp4/" + m_testFileName);
#endif
        Debug.Log("Finished local");
    }

    public void TestDownload()
    {
        Debug.Log("TEST DOWNLOAD");
        StartCoroutine(DownloadAndPlayVideo(m_testFileName));
    }
    
    public IEnumerator DownloadAndPlayVideo (string fileName) 
    {
        Debug.Log("DISABLE UI");
        UICamera[] uiCams = Object.FindObjectsOfType(typeof(UICamera)) as UICamera[];

        foreach (UICamera cam in uiCams)
        {
            cam.enabled = false;
        }

        if (fileName [0] != '/')
        {
            Debug.Log("Adding '/' to file name");
            fileName = "/" + fileName;
        }

        string videoFile = Application.persistentDataPath + fileName;
        string playVideoFile = videoFile;

#if UNITY_IPHONE
        playVideoFile = "file://" + videoFile;
#elif UNITY_ANDROID

#endif

        Debug.Log("LOG VAR");
        Debug.Log("fileName: " + fileName);
        Debug.Log("videoFile: " + videoFile);
        Debug.Log("playVideoFile: " + playVideoFile);
        Debug.Log("url: " + m_url + fileName);

        Debug.Log("WAIT");
        WWW www = new WWW(m_url + fileName);
        yield return www;
        
        Debug.Log("DOWNLOADED");
        
        if (www != null && www.isDone && www.error == null)
        {
            Debug.Log("WRITING");
            FileStream stream = new FileStream(videoFile, FileMode.Create);
            stream.Write(www.bytes, 0, www.bytes.Length);
            stream.Close();
        } else
        {
            Debug.Log("DOWNLOAD FAILED");
            Debug.Log("www != null: " + www != null);
            Debug.Log("www.isDone: " + www.isDone);
            Debug.Log("www.error == null: " + www.error == null);
            
            if(www.error != null)
            {
                Debug.Log("error: " + www.error);
            }
        }

        Debug.Log("PLAY");
#if UNITY_IPHONE || UNITY_ANDROID
        Handheld.PlayFullScreenMovie(playVideoFile, m_bgColor, m_controlMode, m_scalingMode);
#endif

        Debug.Log("PLAYED");

        Debug.Log("ENABLE UI");
        foreach (UICamera cam in uiCams)
        {
            cam.enabled = true;
        }
    }
}
