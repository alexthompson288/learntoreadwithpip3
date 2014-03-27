﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VideoCoordinator : MonoBehaviour 
{
	[SerializeField]
	private MobileMovieTexture m_movieTexture;
	[SerializeField]
	private AudioSource m_audioSource;
    [SerializeField]
    private string m_oggSubdirectory = "StandaloneOggs";
    [SerializeField]
    private string m_mp4Subdirectory = "mp4s";

    static string m_videoName;

    public static void SetVideoName(string videoName)
    {
        m_videoName = videoName;
    }

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		List<DataRow> sentences = GameDataBridge.Instance.GetSectionSentences();
		
		Debug.Log("sentences.Count: " + sentences.Count);

#if UNITY_IPHONE || UNITY_ANDROID
		foreach(DataRow sentence in sentences)
		{
			if(sentence["is_target_sentence"].ToString() == "t")
			{
				Handheld.PlayFullScreenMovie(sentence["text"].ToString() + ".mp4", Color.black, FullScreenMovieControlMode.Full, FullScreenMovieScalingMode.AspectFit);
				break;
			}
		}
#else
        if(Game.session == Game.Session.Single)
        {
            PlayVideo(m_videoName);
            PlayAudio(m_videoName + "_audio");
        }
        else
        {
    		foreach(DataRow sentence in sentences)
    		{
    			if(sentence["is_target_sentence"].ToString() == "t")
    			{
    				PlayVideo(sentence["text"].ToString());
    			}
    			else if(sentence["is_dummy_sentence"].ToString() == "t")
    			{
    				PlayAudio(sentence["text"].ToString());
    			}
    		}
        }
		
		while(m_movieTexture.isPlaying)
		{
			yield return null;
		}
#endif

		//SessionManager.Instance.OnGameFinish();
        PipHelpers.OnGameFinish(true, "NewVoyage");
	}

	void PlayAudio (string filename)
	{
		AudioClip clip = (AudioClip)Resources.Load<AudioClip>(filename);
		
		if(clip != null)
		{
			m_audioSource.clip = clip;
			m_audioSource.Play();
		}
	}

	void PlayVideo (string filename)
	{
		try
		{
            Debug.Log("Execute try");

            Debug.Log("Looking for: " + Application.dataPath + "/StreamingAssets/StandaloneOggs/" + filename + ".ogg");

            if(System.IO.File.Exists(Application.dataPath + "/StreamingAssets/StandaloneOggs/" + filename + ".ogg"))
            {
                Debug.Log("File exists");
                m_movieTexture.SetFilename("Videos/" + filename + ".ogg");
                Debug.Log("Found file");
                m_movieTexture.Play();
                Debug.Log("Played");
            }
            else
            {
                Debug.Log("File not found");
                PipHelpers.OnGameFinish(true, "NewVoyage");
            }
		}
		catch
		{
            Debug.Log("Execute catch");

            PipHelpers.OnGameFinish(true, "NewVoyage");
		}
	}
}
