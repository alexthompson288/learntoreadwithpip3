using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class VideoCoordinator : MonoBehaviour 
{
    [SerializeField]
    private MobileMovieTexture m_movieTexture;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private string m_oggSubdirectory = "StandaloneOgg";
    [SerializeField]
    private string m_mp4Subdirectory = "mp4";

    bool m_videoError = false;

    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        GameManager.Instance.CompleteGame();
    }

    /*
    // Use this for initialization
    IEnumerator Start () 
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        
        List<DataRow> sentences = DataHelpers.GetSentences();
        
        Debug.Log("sentences.Count: " + sentences.Count);
        
        #if UNITY_IPHONE || UNITY_ANDROID
        foreach(DataRow sentence in sentences)
        {
            if(sentence["is_target_sentence"].ToString() == "t")
            {
                Handheld.PlayFullScreenMovie(String.Format("{0}/{1}.mp4", m_mp4Subdirectory, sentence["text"]),
                                             Color.black, FullScreenMovieControlMode.Full, FullScreenMovieScalingMode.AspectFit);
                break;
            }
        }
        #else
        foreach(DataRow sentence in sentences)
        {
            if(sentence["is_dummy_sentence"].ToString() == "t")
            {
                PlayAudio(sentence["text"].ToString());
            }
            else if(sentence["is_target_sentence"].ToString() == "t")
            {
                PlayVideo(sentence["text"].ToString());
            }
        }

        
        while(m_movieTexture.isPlaying && !m_videoError)
        {
            yield return null;
        }
        #endif
        
        GameManager.Instance.CompleteGame(true, "NewVoyage");
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
            
            string streamingRelativePath = String.Format("{0}/{1}.ogg", m_oggSubdirectory, filename);
            
            if(System.IO.File.Exists(String.Format("{0}/StreamingAssets/{1}", Application.dataPath, streamingRelativePath)))
            {
                Debug.Log("File exists");
                m_movieTexture.SetFilename(streamingRelativePath);
                Debug.Log("Found file");
                m_movieTexture.Play();
                Debug.Log("Played");
            }
            else
            {
                Debug.Log("File not found: " + String.Format("{0}/StreamingAssets/{1}", Application.dataPath, streamingRelativePath));
                m_videoError = true;
            }
        }
        catch
        {
            Debug.Log("Execute catch");
            m_videoError = true;
        }
    }
    */
}