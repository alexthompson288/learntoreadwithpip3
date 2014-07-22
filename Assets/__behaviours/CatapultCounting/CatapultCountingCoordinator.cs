using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class CatapultCountingCoordinator : GameCoordinator
{
    [SerializeField]
    private CatapultBehaviour m_catapult;
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
    private TriggerTracker m_triggerTracker;
    [SerializeField]
    private Transform m_targetSpawnParent;
    [SerializeField]
    private UILabel m_countingLabel;

    Vector3 m_containerOnPos;

    List<GameObject> m_trackedTargets = new List<GameObject>();

    void Awake()
    {
        m_scoreKeeper.SetTargetScore(m_targetScore);

        m_catapult.MoveToMultiplayerLocation(0);

        m_containerOnPos = m_container.position;
        m_container.position = m_containerLeftOffLocation.position;

        m_triggerTracker.Entered += OnTargetEnterTrigger;
    }

	// Use this for initialization
	IEnumerator Start () 
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetNumbers();

        m_dataPool = DataHelpers.OnlyLowNumbers(m_dataPool, 15);

        StartCoroutine(SpawnTargets());
        StartCoroutine(AskQuestion());
	}

    IEnumerator SpawnTargets()
    {
        SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_targetPrefab, m_targetSpawnParent);
        yield return new WaitForSeconds(0.75f);
        StartCoroutine(SpawnTargets());
    }

    IEnumerator AskQuestion()
    {
        m_container.transform.position = m_containerLeftOffLocation.position;
        float tweenDuration = 0.8f;
        iTween.MoveTo(m_container.gameObject, m_containerOnPos, tweenDuration);
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");

        m_currentData = GetRandomData();

        m_dataDisplay.On(m_currentData);

        m_countingLabel.text = "0";

        yield break;
    }

    void OnTargetEnterTrigger(TriggerTracker tracker, Collider other)
    {
        int numTrackedTargets = m_triggerTracker.GetNumTrackedObjects();

        m_countingLabel.text = numTrackedTargets.ToString();

        other.transform.parent = m_container;
        m_trackedTargets.Add(other.gameObject);

        PlayShortAudio(m_dataPool.Find(x => x.GetInt("value") == numTrackedTargets));

        if (numTrackedTargets >= m_currentData.GetInt("value"))
        {
            m_scoreKeeper.UpdateScore();
            StartCoroutine(ClearQuestion());
        }
    }

    IEnumerator ClearQuestion()
    {
        yield return new WaitForSeconds(1.2f);

        m_dataDisplay.Off();
        float tweenDuration = 0.25f;

        Hashtable tweenArgs = new Hashtable();

        tweenArgs.Add("position", m_containerRightOffLocation.position);
        tweenArgs.Add("time", tweenDuration);
        tweenArgs.Add("easetype", iTween.EaseType.linear);

        iTween.MoveTo(m_container.gameObject, tweenArgs);
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");

        yield return new WaitForSeconds(tweenDuration);

        CollectionHelpers.DestroyObjects(m_trackedTargets);

        yield return new WaitForSeconds(0.5f);

        if (m_scoreKeeper.HasCompleted())
        {
            StartCoroutine(CompleteGame());
        }
        else
        {
            StartCoroutine(AskQuestion());
        }
    }

    protected override IEnumerator CompleteGame()
    {
        m_catapult.Off();

        float timeTaken = Time.time - m_startTime;
        
        float twoStarPerQuestion = 12;
        float threeStarPerQuestion = 6;
        
        int stars = ScoreInfo.CalculateTimeStars(timeTaken, twoStarPerQuestion * (float)m_targetScore, threeStarPerQuestion * (float)m_targetScore);
        
        // Game ends when player reaches targetScore
        ScoreInfo.Instance.NewScore(timeTaken, m_targetScore, m_targetScore, stars);

        yield return StartCoroutine(m_scoreKeeper.On());
        GameManager.Instance.CompleteGame();
    }
}
