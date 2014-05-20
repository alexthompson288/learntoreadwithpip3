using UnityEngine;
using System.Collections;
using System.IO;

public class VideoStreamer : Singleton<VideoStreamer> 
{
    Color m_bgColor = Color.black;
    FullScreenMovieControlMode m_controlMode = FullScreenMovieControlMode.Full;
    FullScreenMovieScalingMode m_scalingMode = FullScreenMovieScalingMode.AspectFit;

    string m_url = "http://www.learntoreadwithpip.com";

    
    public IEnumerator DownloadAndPlayVideo (string fileName) 
    {
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

#endif
        
        WWW www = new WWW(m_url + fileName);
        yield return www;
        
        if (www != null && www.isDone && www.error == null) 
        {
            FileStream stream = new FileStream(videoFile, FileMode.Create);
            stream.Write(www.bytes, 0, www.bytes.Length);
            stream.Close();
        }
        
        Handheld.PlayFullScreenMovie(playVideoFile, m_bgColor, m_controlMode, m_scalingMode);

        foreach (UICamera cam in uiCams)
        {
            cam.enabled = true;
        }
    }
}
