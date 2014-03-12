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
	private Vector2 m_spawnDelay;

	List<DataRow> m_dataPool = new List<DataRow>();

	DataRow m_targetData = null;

	// Use this for initialization
	IEnumerator Start () 
	{
		m_spawnDelay.x = Mathf.Clamp(m_spawnDelay.x, 0, m_spawnDelay.x);
		m_spawnDelay.y = Mathf.Clamp(m_spawnDelay.y, m_spawnDelay.x, m_spawnDelay.y);

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

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

		ChangeTarget();
		StartCoroutine(SpawnFlickables());
	}

	IEnumerator SpawnFlickables()
	{
		yield return new WaitForSeconds(Random.Range(m_spawnDelay.x, m_spawnDelay.y));
	}

	void ChangeTarget()
	{
		m_targetData = m_dataPool[Random.Range(0, m_dataPool.Count)];
		SayTarget();
	}

	void SayTarget()
	{
		AudioClip clip = m_dataType == GameDataBridge.DataType.Letters ? AudioBankManager.Instance.GetAudioClip(m_targetData["grapheme"].ToString()) : LoaderHelpers.LoadAudioForWord(m_targetData["word"].ToString());

		if(clip != null)
		{
			m_audioSource.clip = clip;
			m_audioSource.Play();
		}
	}
}
