using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlickCoordinator : MonoBehaviour 
{
	[SerializeField]
	private GameDataBridge.DataType m_dataType;
	[SerializeField]
	private GameObject m_flickablePrefab;
	[SerializeField]
	private Transform m_flickableParent;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private Vector2 m_spawnDelayRange;

	List<DataRow> m_dataPool = new List<DataRow>();

	DataRow m_targetData = null;

	IEnumerator Start()
	{
		// Clamp spawn delay range values
		m_spawnDelayRange.x = Mathf.Clamp (m_spawnDelayRange.x, 0, m_spawnDelayRange.x);
		m_spawnDelayRange.y = Mathf.Clamp (m_spawnDelayRange.y, m_spawnDelayRange.x, m_spawnDelayRange.y);

		yield return StartCoroutine (GameDataBridge.WaitForDatabase());

		switch (m_dataType) 
		{
		case GameDataBridge.DataType.Letters:
			m_dataPool = GameDataBridge.Instance.GetLetters();
			break;
		case GameDataBridge.DataType.Keywords:
			m_dataPool = GameDataBridge.Instance.GetKeywords();
			break;
		case GameDataBridge.DataType.Words:
			m_dataPool = GameDataBridge.Instance.GetWords();
			break;
		}

		StartCoroutine (SpawnFlickables ());
	}

	IEnumerator SpawnFlickables()
	{
		yield return new WaitForSeconds (Random.Range (m_spawnDelayRange.x, m_spawnDelayRange.y));
	}

	void NewTarget()
	{
		m_targetData = m_dataPool [Random.Range (0, m_dataPool.Count)];
		SayTarget ();
	}

	void SayTarget()
	{
		if (m_targetData != null) 
		{
			AudioClip clip = m_dataType == GameDataBridge.DataType.Letters ?
				AudioBankManager.Instance.GetAudioClip(m_targetData["grapheme"].ToString()) :
					LoaderHelpers.LoadAudioForWord (m_targetData["word"].ToString());

			if (clip != null) 
			{
				m_audioSource.clip = clip;
				m_audioSource.Play();
			}
		}
	}
}
