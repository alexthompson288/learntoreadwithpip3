using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class OldCatapultCoordinator : MonoBehaviour
{
    [SerializeField]
    private string m_dataType;
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private GameObject m_cannonTargetPrefab;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private int m_initialSpawnNum = 3;
    [SerializeField]
    private Vector2 m_timeBetweenSpawn = new Vector2(4f, 8f);
    [SerializeField]
    private float m_probabilityTargetIsCurrent = 0.5f;
    [SerializeField]
    private Transform m_minTargetDestroyLocation;
    [SerializeField]
    private Transform m_maxTargetDestroyLocation;
    [SerializeField]
    private int m_targetScore = 5;
    [SerializeField]
    private ProgressScoreBar m_scoreBar;
    [SerializeField]
    private CastleBehaviour m_castleBehaviour;

    int m_score = 0;

    List<DataRow> m_dataPool = new List<DataRow>();
    DataRow m_currentData;

    Dictionary<DataRow, AudioClip> m_shortAudio = new Dictionary<DataRow, AudioClip>();
    Dictionary<DataRow, AudioClip> m_longAudio = new Dictionary<DataRow, AudioClip>();

    List<Transform> m_spawnedTargets = new List<Transform>();

	IEnumerator Start () 
    {
        m_probabilityTargetIsCurrent = Mathf.Clamp01(m_probabilityTargetIsCurrent);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        switch (m_dataType)
        {
            case "phonemes":
                m_dataPool = DataHelpers.GetPhonemes();
                break;
            case "words":
                m_dataPool = DataHelpers.GetWords();
                break;
            case "keywords":
                m_dataPool = DataHelpers.GetKeywords();
                break;
        }

        if (m_dataPool.Count > 0)
        {
            foreach (DataRow data in m_dataPool)
            {
                if (m_dataType == "phonemes")
                {
                    m_shortAudio [data] = AudioBankManager.Instance.GetAudioClip(data ["grapheme"].ToString());
                    m_longAudio [data] = LoaderHelpers.LoadMnemonic(data);
                } else
                {
                    m_shortAudio [data] = LoaderHelpers.LoadAudioForWord(data);
                }
            }

            m_currentData = m_dataPool [Random.Range(0, m_dataPool.Count)];

            PlayShortAudio();

            for (int i = 0; i < m_initialSpawnNum && i < m_locators.Length; ++i)
            {
                bool makeRecursive = (i == 0);
                StartCoroutine(SpawnTarget(makeRecursive));
            }

            StartCoroutine(DestroyTargets());
        } 
        else
        {
            OnGameComplete();
        }
    }

    IEnumerator DestroyTargets()
    {
        List<Transform> targetsToDestroy = m_spawnedTargets.FindAll(ShouldDestroyTarget);
        foreach (Transform target in targetsToDestroy)
        {
            m_spawnedTargets.Remove(target);
            Destroy(target.gameObject);
        }

        yield return new WaitForSeconds(3f);
        StartCoroutine(DestroyTargets());
    }

    bool ShouldDestroyTarget(Transform target)
    {
        return (target.position.x > m_maxTargetDestroyLocation.position.x ||  target.position.y > m_maxTargetDestroyLocation.position.y 
                || target.position.y < m_minTargetDestroyLocation.position.y || target.position.y < m_minTargetDestroyLocation.position.y);
    }

    IEnumerator SpawnTarget(bool makeRecursive)
    {
        Transform locator = m_locators [Random.Range(0, m_locators.Length)];

        GameObject newTarget = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_cannonTargetPrefab, locator);

        DataRow targetData = m_currentData;
        if (Random.Range(0f, 1f) > m_probabilityTargetIsCurrent)
        {
            while (targetData == m_currentData)
            {
                targetData = m_dataPool [Random.Range(0, m_dataPool.Count)];
            }
        } 

        CatapultTarget targetBehaviour = newTarget.GetComponent<CatapultTarget>() as CatapultTarget;

        targetBehaviour.SetUp(targetData, m_dataType);
        targetBehaviour.OnTargetHit += OnTargetHit;
        targetBehaviour.OnDestroyGo += OnDestroyTarget;

        m_spawnedTargets.Add(newTarget.transform);

        yield return new WaitForSeconds(Random.Range(m_timeBetweenSpawn.x, m_timeBetweenSpawn.y) + 0.5f);

        if (makeRecursive)
        {
            StartCoroutine(SpawnTarget(true));
        }
    }

    void OnDestroyTarget(CatapultTarget target)
    {
        if (m_spawnedTargets.Contains(target.transform))
        {
            m_spawnedTargets.Remove(target.transform);
        }
    }

    void OnTargetHit(CatapultTarget target, Collider ball)
    {
        if (target.data == m_currentData)
        {
            target.Explode();
            m_currentData = m_dataPool[Random.Range(0, m_dataPool.Count)];
            PlayShortAudio();

            m_score++;

            if(m_score >= m_targetScore)
            {
                OnGameComplete();
            }
        } 
        else
        {
            target.ApplyHitForce(ball.transform);
        }

        ball.GetComponent<CatapultAmmo>().Explode();
    }

    void PlayLongAudio()
    {
        m_audioSource.clip = m_longAudio [m_currentData];
        if (m_audioSource.clip != null)
        {
            m_audioSource.Play();
        } 
        else
        {
            PlayShortAudio();
        }
    }

    void PlayShortAudio()
    {
        m_audioSource.clip = m_shortAudio [m_currentData];
        if (m_audioSource.clip != null)
        {
            m_audioSource.Play();
        }
    }

    void OnGameComplete()
    {
        StopAllCoroutines();
        StartCoroutine(OnGameCompleteCo());
    }

    IEnumerator OnGameCompleteCo()
    {
        yield return null;

        if (m_castleBehaviour.gameObject.activeInHierarchy)
        {
            m_castleBehaviour.On();
        }
    }

    void OnGUI()
    {
        GUILayout.Label("Score: " + m_score);
    }
}
