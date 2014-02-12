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

		if(sentences.Count > 0)
		{
			Debug.Log("id: " + sentences[0]["id"].ToString());

			string filename = sentences[0]["text"].ToString();

			AudioClip clip = (AudioClip)Resources.Load<AudioClip>(filename + "_audio");

			if(clip != null)
			{
				m_audioSource.clip = clip;
				m_audioSource.Play();
			}

			m_movieTexture.SetFilename("Videos/" + filename + ".ogg");

			try
			{
				m_movieTexture.Play();
			}
			catch
			{
				JourneyInformation.Instance.OnGameFinish();
			}
		}

		while(m_movieTexture.isPlaying)
		{
			yield return null;
		}

		JourneyInformation.Instance.OnGameFinish();
	}
}
