using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class CatapultCountingCoordinator : GameCoordinator
{
    [SerializeField]
    private GameObject m_targetPrefab;
    [SerializeField]
    private DataDisplay m_dataDisplay;
    [SerializeField]
    private Transform m_container;
    [SerializeField]
    private Transform m_containerLeftOffLocation;
    [SerializeField]
    private Transform m_containerRightOffLocation;
    [SerializeField]
    private Transform[] m_containerLocators;
    [SerializeField]
    private TriggerTracker m_triggerTracker;
    [SerializeField]
    private Transform m_targetSpawnParent;

    float m_targetSpawnDelay;

    Vector3 m_containerOnPos;

    List<GameObject> m_spawnedTargets = new List<GameObject>();

    void Awake()
    {
        m_containerOnPos = m_container.position;
        m_container.position = m_containerLeftOffLocation.position;

        m_triggerTracker.Entered += OnTargetEnterTrigger;
    }

	// Use this for initialization
	IEnumerator Start () 
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetNumbers();

        StartCoroutine(SpawnTargets());
        StartCoroutine(AskQuestion());
	}

    IEnumerator SpawnTargets()
    {
        SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_targetPrefab, m_targetSpawnParent);
        yield return new WaitForSeconds(m_targetSpawnDelay);
        StartCoroutine(SpawnTargets());
    }

    IEnumerator AskQuestion()
    {
        m_container.transform.position = m_containerLeftOffLocation.position;

        float tweenDuration = 0.3f;
        iTween.MoveTo(m_container.gameObject, m_containerOnPos, tweenDuration);

        m_currentData = GetRandomData();
        m_dataDisplay.On(m_currentData);

        yield break;
    }

    void OnTargetEnterTrigger(TriggerTracker tracker)
    {
        PlayShortAudio(m_dataPool.Find(x => x.GetInt("value") == m_triggerTracker.GetNumTrackedObjects()));

        if (m_triggerTracker.GetNumTrackedObjects() >= m_currentData.GetInt("value"))
        {
            m_scoreKeeper.UpdateScore();
            StartCoroutine(ClearQuestion());
        }
    }

    IEnumerator ClearQuestion()
    {
        m_dataDisplay.Off();
        float tweenDuration = 0.25f;

        Hashtable tweenArgs = new Hashtable();

        tweenArgs.Add("position", m_containerRightOffLocation.position);
        tweenArgs.Add("time", tweenDuration);
        tweenArgs.Add("easetype", iTween.EaseType.linear);

        iTween.MoveTo(m_container.gameObject, tweenArgs);

        yield return new WaitForSeconds(tweenDuration);

        if (m_scoreKeeper.HasCompleted())
        {
            StartCoroutine(CompleteGame());
        }
        else
        {
            StartCoroutine(AskQuestion());
        }
    }

    IEnumerator CompleteGame()
    {
        yield return StartCoroutine(m_scoreKeeper.On());
        GameManager.Instance.CompleteGame();
    }
}
