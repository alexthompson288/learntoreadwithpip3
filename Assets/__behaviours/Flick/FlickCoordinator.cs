using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class FlickCoordinator : MonoBehaviour 
{
	[SerializeField]
	private GameDataBridge.DataType m_dataType;
	[SerializeField]
	private GameObject m_flickablePrefab;
	[SerializeField]
	private Transform m_flickableParent;
	[SerializeField]
	private Transform m_flickableRightLocation;
	[SerializeField]
	private Transform m_flickableDestroyLocation;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private Vector2 m_spawnDelay;
	
	List<DataRow> m_dataPool = new List<DataRow>();
	
	DataRow m_targetData = null;

	List<Transform> m_spawnedFlickables = new List<Transform>();
	
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

		Debug.Log("m_dataPool.Count: " + m_dataPool.Count);
		foreach(DataRow data in m_dataPool)
		{
			Debug.Log(data["phoneme"].ToString());
		}
		
		ChangeTarget();
		StartCoroutine(SpawnFlickables());
		StartCoroutine(DestroyFlickables());
	}
	
	IEnumerator SpawnFlickables()
	{
		GameObject newFlickable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_flickablePrefab, m_flickableParent);

		m_spawnedFlickables.Add(newFlickable.transform);

		Vector3 pos = newFlickable.transform.position;
		pos.x = Random.Range(m_flickableParent.position.x, m_flickableRightLocation.position.x);
		newFlickable.transform.position = pos;

		newFlickable.GetComponent<FlickableWidget>().SetUp(m_dataPool[Random.Range(0, m_dataPool.Count)], m_dataType);

		yield return new WaitForSeconds(Random.Range(m_spawnDelay.x, m_spawnDelay.y));
		StartCoroutine(SpawnFlickables());
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

	IEnumerator DestroyFlickables()
	{
		List<Transform> toDestroy = m_spawnedFlickables.FindAll(IsBelowDestroyLocation);

		foreach(Transform flickable in toDestroy)
		{
			m_spawnedFlickables.Remove(flickable);
			Destroy(flickable.gameObject);
		}

		yield return new WaitForSeconds(1f);
		StartCoroutine(DestroyFlickables());
	}

	bool IsBelowDestroyLocation(Transform tra)
	{
		return tra.position.y < m_flickableDestroyLocation.position.y;
	}
}
