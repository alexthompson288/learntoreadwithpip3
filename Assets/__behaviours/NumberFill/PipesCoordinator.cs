using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class PipesCoordinator : GameCoordinator
{
    [SerializeField]
    private GameObject m_rbPrefab;
    [SerializeField]
    private Transform m_spawnLocation;
    [SerializeField]
    private Transform m_destroyLocation;
    [SerializeField]
    private PipButton m_addButton;
    [SerializeField]
    private PipButton m_subtractButton;
    [SerializeField]
    private PipButton m_submitButton;
    [SerializeField]
    private DataDisplay m_dataDisplay;
    [SerializeField]
    private GameObject m_container;
    [SerializeField]
    private Transform[] m_containerLocators;
    [SerializeField]
    private TriggerTracker m_triggerTracker;
    [SerializeField]
    private GameObject m_baseLeft;
    [SerializeField]
    private GameObject m_baseRight;
    [SerializeField]
    private Collider m_bottomCollider;
    [SerializeField]
    private GameObject m_containerParent;
    [SerializeField]
    private Transform m_submitButtonParent;
    [SerializeField]
    private Transform m_submitButtonParentLocation;
    [SerializeField]
    private UILabel m_triggerCounter;
    [SerializeField]
    private ThrobGUIElement m_throbBehaviour;
    [SerializeField]
    private TweenBehaviour m_moveableGlassTweenBehaviour;

    List<GameObject> m_spawnedRbs = new List<GameObject>();

    void Awake()
    {
        m_addButton.SetPipColor(ColorInfo.PipColor.Green);
        m_subtractButton.SetPipColor(ColorInfo.PipColor.Red);

        m_addButton.SetMustCompletePress(false);
        m_subtractButton.SetMustCompletePress(false);
    }

    IEnumerator Start()
    {
        m_scoreKeeper.SetTargetScore(m_targetScore);

        m_addButton.Unpressing += OnPressAdd;
        m_subtractButton.Unpressing += OnPressSubtract;
        m_submitButton.Unpressing += OnPressSubmit;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        m_dataPool = DataHelpers.GetNumbers();
        m_dataPool = DataHelpers.OnlyLowNumbers(m_dataPool, 20);

        if (m_dataPool.Count > 0)
        {
            m_startTime = Time.time;

            StartCoroutine(AskQuestion());
        }
    }

    void OnTriggerTrackerUpdate(TriggerTracker tracker, Collider other)
    {
        int numTrackedObjects = m_triggerTracker.GetNumTrackedObjects();
        m_triggerCounter.text = numTrackedObjects.ToString();
        DataAudio.Instance.PlayNumber(numTrackedObjects);

        if (numTrackedObjects == m_currentData.GetInt("value"))
        {
            m_throbBehaviour.On();
        }
        else
        {
            m_throbBehaviour.Off();
        }
    }

    void OnRigidbodyDestroy(FillJarFruit rb)
    {
        m_spawnedRbs.Remove(rb.gameObject);
    }

    IEnumerator AskQuestion()
    {
        m_bottomCollider.enabled = true;

        m_currentData = GetRandomData();

        m_dataDisplay.On(m_currentData);


        int initialNum = Random.Range(0, m_currentData.GetInt("value") + 1);

        m_triggerCounter.text = initialNum.ToString();

        CollectionHelpers.Shuffle(m_containerLocators);

        for (int i = 0; i < initialNum && i < m_containerLocators.Length; ++i)
        {
            GameObject newRb = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_rbPrefab, m_containerLocators[i], true);
            m_spawnedRbs.Add(newRb);
            newRb.transform.parent = m_container.transform;
            newRb.GetComponent<FillJarFruit>().Destroying += OnRigidbodyDestroy;
            iTween.ScaleTo(newRb, Vector3.one, 0.2f);
        }

        PlayShortAudio();

        yield return new WaitForSeconds(0.1f);

        m_triggerTracker.Entered += OnTriggerTrackerUpdate;
        m_triggerTracker.Exited += OnTriggerTrackerUpdate;

        if (m_triggerTracker.GetNumTrackedObjects() == m_currentData.GetInt("value"))
        {
            m_throbBehaviour.On();
        }
    }

    void OnPressAdd(PipButton button)
    {
        GameObject newRb = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_rbPrefab, m_spawnLocation);
        m_spawnedRbs.Add(newRb);
        newRb.GetComponent<FillJarFruit>().Destroying += OnRigidbodyDestroy;
        newRb.transform.parent = m_container.transform;
    }

    void OnPressSubtract(PipButton button)
    {
        StartCoroutine(OnPressSubtractCo());
    }

    IEnumerator OnPressSubtractCo()
    {
        Transform closest = TransformHelpers.FindClosest(m_spawnedRbs, m_destroyLocation);
        
        if (closest != null)
        {
            m_spawnedRbs.Remove(closest.gameObject);

            Destroy(closest.GetComponent<FillJarFruit>());

            closest.rigidbody.velocity = Vector3.zero;
            closest.rigidbody.isKinematic = true;

            yield return null;
            
            Hashtable tweenArgs = new Hashtable();
            
            float tweenSpeed = 2f;
            
            tweenArgs.Add("position", m_destroyLocation);
            tweenArgs.Add("speed", tweenSpeed);
            tweenArgs.Add("easetype", iTween.EaseType.linear);

            iTween.MoveTo(closest.gameObject, tweenArgs);
            
            yield return new WaitForSeconds(TransformHelpers.GetDuration(closest, m_destroyLocation.position, tweenSpeed));

            iTween.Stop(closest.gameObject);

            yield return null;

            closest.rigidbody.isKinematic = false;
            closest.rigidbody.angularVelocity = Vector3.zero;
            closest.rigidbody.AddForce(new Vector3(0, 3, 0), ForceMode.VelocityChange);
        }

        yield break;
    }

    void OnPressSubmit(PipButton button)
    {
        ////D.Log("Tracking:" + m_triggerTracker.GetNumTrackedObjects());

        m_throbBehaviour.Off();

        if (m_triggerTracker.GetNumTrackedObjects() == m_currentData.GetInt("value"))
        {
            m_scoreKeeper.UpdateScore();

            StartCoroutine(ClearQuestion());
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_EXHALE");
            iTween.ShakePosition(m_containerParent, Vector3.one * 0.2f, 0.35f);
        }
    }

    IEnumerator UpdateSubmitButtonLocation()
    {
        m_submitButtonParent.transform.position = m_submitButtonParentLocation.transform.position;
        yield return null;
        StartCoroutine("UpdateSubmitButtonLocation");
    }

    IEnumerator ClearQuestion()
    {
        StartCoroutine("UpdateSubmitButtonLocation");
        m_triggerTracker.Entered -= OnTriggerTrackerUpdate;
        m_triggerTracker.Exited -= OnTriggerTrackerUpdate;

        float tweenDuration = 0.2f;

        Hashtable tweenArgs = new Hashtable();

        tweenArgs.Add("rotation", new Vector3(0, 0, 270));
        tweenArgs.Add("easetype", iTween.EaseType.easeInQuad);
        tweenArgs.Add("time", tweenDuration);
        iTween.RotateTo(m_baseLeft, tweenArgs);

        tweenArgs ["rotation"] = new Vector3(0, 0, 90);
        iTween.RotateTo(m_baseRight, tweenArgs);

        m_bottomCollider.enabled = false;

        while (m_triggerTracker.GetNumTrackedObjects() > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        tweenArgs ["rotation"] = Vector3.zero;
        tweenArgs ["easetype"] = iTween.EaseType.easeOutQuad;

        iTween.RotateTo(m_baseLeft, tweenArgs);
        iTween.RotateTo(m_baseRight, tweenArgs);

        yield return new WaitForSeconds(tweenDuration);

        StopCoroutine("UpdateSubmitButtonLocation");

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
        float timeTaken = Time.time - m_startTime;
        
        float twoStarPerQuestion = 18;
        float threeStarPerQuestion = 10;
        
        int stars = ScoreInfo.CalculateTimeStars(timeTaken, twoStarPerQuestion * (float)m_targetScore, threeStarPerQuestion * (float)m_targetScore);
        
        // Game ends when player reaches targetScore
        ScoreInfo.Instance.NewScore(timeTaken, m_targetScore, m_targetScore, stars);

        m_moveableGlassTweenBehaviour.Off();
        m_dataDisplay.Off();

        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(m_scoreKeeper.Celebrate());

        GameManager.Instance.CompleteGame();
    }
}
