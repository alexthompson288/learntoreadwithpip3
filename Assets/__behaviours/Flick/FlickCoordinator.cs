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
    [SerializeField]
    private float m_probabilityLetterIsTarget = 0.5f;
    [SerializeField]
    private ProgressScoreBar m_scoreBar;
    [SerializeField]
    private int m_targetScore = 5;
    [SerializeField]
    private FlickableCatcher m_flickableCatcher;

    int m_score = 0;
	
	List<DataRow> m_dataPool = new List<DataRow>();
	
	DataRow m_targetData = null;

	List<Transform> m_spawnedFlickables = new List<Transform>();
	
	// Use this for initialization
	IEnumerator Start () 
	{
        m_probabilityLetterIsTarget = Mathf.Clamp01(m_probabilityLetterIsTarget);
		m_spawnDelay.x = Mathf.Clamp(m_spawnDelay.x, 0, m_spawnDelay.x);
		m_spawnDelay.y = Mathf.Clamp(m_spawnDelay.y, m_spawnDelay.x, m_spawnDelay.y);

        m_scoreBar.SetStarsTarget(m_targetScore);
		
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_flickableCatcher.OnFlickableEnter += OnFlickableEnter;
		
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
		
        if (m_dataPool.Count > 0)
        {
            ChangeTarget();
            StartCoroutine(SpawnFlickables());
            StartCoroutine(DestroyFlickables());
        }
        else
        {
            StartCoroutine(OnGameComplete());
        }
	}

    void OnFlickableEnter(Collider flickableCollider)
    {
        if (flickableCollider.GetComponent<FlickableWidget>().data == m_targetData)
        {
            m_score++;
            m_scoreBar.SetStarsCompleted(m_score);
            m_scoreBar.SetScore(m_score);

            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");

            if(m_score >= m_targetScore)
            {
                StartCoroutine(OnGameComplete());
            }
            else
            {
                ChangeTarget();
            }
        } 
        else
        {
            SayTarget();
        }

        m_spawnedFlickables.Remove(flickableCollider.transform);
        Destroy(flickableCollider.gameObject);
    }
	
	IEnumerator SpawnFlickables()
	{
		GameObject newFlickable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_flickablePrefab, m_flickableParent);

		m_spawnedFlickables.Add(newFlickable.transform);

		Vector3 pos = newFlickable.transform.position;
		pos.x = Random.Range(m_flickableParent.position.x, m_flickableRightLocation.position.x);
		newFlickable.transform.position = pos;

        DataRow data = m_targetData;
        if (Random.Range(0f, 1f) > m_probabilityLetterIsTarget)
        {
            while(data == m_targetData)
            {
                data = m_dataPool[Random.Range(0, m_dataPool.Count)];
            }
        }

		newFlickable.GetComponent<FlickableWidget>().SetUp(data, m_dataType);

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

    IEnumerator OnGameComplete()
    {
        yield return null;

        PipHelpers.OnGameFinish();
    }
}
