﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class CannonCoordinator : Singleton<CannonCoordinator> 
{
    [SerializeField]
    private GameDataBridge.DataType m_dataType;
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
    private Transform m_targetDestroyLocation;
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

        m_scoreBar.SetStarsTarget(m_targetScore);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        switch (m_dataType)
        {
            case GameDataBridge.DataType.Letters:
                m_dataPool = GameDataBridge.Instance.GetLetters();
                break;
            case GameDataBridge.DataType.Words:
                m_dataPool = GameDataBridge.Instance.GetWords();
                break;
            case GameDataBridge.DataType.Keywords:
                m_dataPool = GameDataBridge.Instance.GetKeywords();
                break;
        }

        if (m_dataPool.Count > 0)
        {

            foreach (DataRow data in m_dataPool)
            {
                if (m_dataType == GameDataBridge.DataType.Letters)
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
        } 
        else
        {
            OnGameComplete();
        }
    }

    void Update()
    {
        List<Transform> targetsToDestroy = m_spawnedTargets.FindAll(ShouldDestroyTarget);
        foreach (Transform target in targetsToDestroy)
        {
            m_spawnedTargets.Remove(target);
            Destroy(target.gameObject);
        }
    }

    bool ShouldDestroyTarget(Transform target)
    {
        return (target.position.x > m_targetDestroyLocation.position.x || target.position.y < m_targetDestroyLocation.position.y);
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

        CannonTarget targetBehaviour = newTarget.GetComponent<CannonTarget>() as CannonTarget;

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

    void OnDestroyTarget(CannonTarget target)
    {
        if (m_spawnedTargets.Contains(target.transform))
        {
            m_spawnedTargets.Remove(target.transform);
        }
    }

    void OnTargetHit(CannonTarget target, Collider ball)
    {
        if (target.data == m_currentData)
        {
            target.Explode();
            m_currentData = m_dataPool[Random.Range(0, m_dataPool.Count)];
            PlayShortAudio();

            m_score++;
            m_scoreBar.SetStarsCompleted(m_score);
            m_scoreBar.SetScore(m_score);

            if(m_score >= m_targetScore)
            {
                OnGameComplete();
            }
        } 
        else
        {
            target.ApplyHitForce(ball.transform);
        }
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
        m_castleBehaviour.On();
        yield break;
    }
}
