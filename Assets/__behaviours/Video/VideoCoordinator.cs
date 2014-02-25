using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VideoCoordinator : MonoBehaviour 
{
	[SerializeField]
	private MobileMovieTexture m_movieTexture;
	[SerializeField]
	private AudioSource m_audioSource;

	//string m_videoFilename = "Videos/ogg_1024_2_blending.ogg";
	//string m_videoFilename = "Videos/BunnyDance.ogg";

	//string m_audioFilename = "blending_audio";

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
		
		while(m_movieTexture.isPlaying)
		{
			yield return null;
		}
#endif

		SessionManager.Instance.OnGameFinish();
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
		m_movieTexture.SetFilename("Videos/" + filename + ".ogg");
		
		try
		{
			m_movieTexture.Play();
		}
		catch
		{
			SessionManager.Instance.OnGameFinish();
		}
	}
}
