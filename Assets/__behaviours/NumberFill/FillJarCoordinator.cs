using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class FillJarCoordinator : GameCoordinator
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

        if (m_dataPool.Count > 0)
        {
            StartCoroutine(AskQuestion());
        }
    }

    void OnTriggerTrackerUpdate(TriggerTracker tracker)
    {
        int numTrackedObjects = m_triggerTracker.GetNumTrackedObjects();
        m_triggerCounter.text = numTrackedObjects.ToString();
        DataAudio.Instance.PlayNumber(numTrackedObjects);
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
        D.Log("Tracking:" + m_triggerTracker.GetNumTrackedObjects());

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

        //WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");

        //yield return new WaitForSeconds(tweenDuration / 2);

        m_bottomCollider.enabled = false;

        //CollectionHelpers.DestroyObjects(m_spawnedRbs);

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
        yield return StartCoroutine(m_scoreKeeper.On());

        GameManager.Instance.CompleteGame();
    }
}

/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class FillJarCoordinator : GameCoordinator
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
    private Transform m_containerLeftOffLocation;
    [SerializeField]
    private Transform m_containerRightOffLocation;
    [SerializeField]
    private Transform[] m_containerLocators;
    [SerializeField]
    private TriggerTracker m_triggerTracker;
    [SerializeField]
    private DestroyRigidbodies[] m_destroyBehaviours;

    Vector3 m_containerOnPos;

    List<GameObject> m_spawnedRbs = new List<GameObject>();

    void Awake()
    {
        foreach (DestroyRigidbodies destroy in m_destroyBehaviours)
        {
            destroy.DestroyingRB += OnRBEnterDestroyArea;
        }

        m_containerOnPos = m_container.transform.position;
        m_container.transform.position = m_containerLeftOffLocation.position;

        m_addButton.SetPipColor(ColorInfo.PipColor.Green);
        m_subtractButton.SetPipColor(ColorInfo.PipColor.Red);
    }

    IEnumerator Start()
    {
        m_scoreKeeper.SetTargetScore(m_targetScore);

        m_addButton.Unpressing += OnPressAdd;
        m_subtractButton.Unpressing += OnPressSubtract;
        m_submitButton.Unpressing += OnPressSubmit;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        m_dataPool = DataHelpers.GetNumbers();

        if (m_dataPool.Count > 0)
        {
            StartCoroutine(AskQuestion());
        }
    }

    IEnumerator StartCoroutine(AskQuestion())
    {
        m_currentData = GetRandomData();

        m_dataDisplay.On(m_currentData);

        int initialNum = Random.Range(0, m_currentData.GetInt("value") + 1);

        CollectionHelpers.Shuffle(m_containerLocators);

        for (int i = 0; i < initialNum && i < m_containerLocators.Length; ++i)
        {
            GameObject newRb = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_rbPrefab, m_containerLocators[i]);
            m_spawnedRbs.Add(newRb);
            newRb.transform.parent = m_container.transform;
        }

        float tweenDuration = 0.5f;

        iTween.MoveTo(m_container, m_containerOnPos, tweenDuration);
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
        
        yield return new WaitForSeconds(tweenDuration);

        PlayShortAudio();
    }

    void OnPressAdd(PipButton button)
    {
        GameObject newRb = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_rbPrefab, m_spawnLocation);
        m_spawnedRbs.Add(newRb);
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
            
            Hashtable tweenArgs = new Hashtable();
            
            float tweenSpeed = 2f;
            
            tweenArgs.Add("position", m_destroyLocation);
            tweenArgs.Add("speed", tweenSpeed);
            tweenArgs.Add("easetype", iTween.EaseType.linear);

            closest.rigidbody.velocity = Vector3.zero;
            closest.rigidbody.isKinematic = true;
            iTween.MoveTo(closest.gameObject, tweenArgs);
            
            yield return new WaitForSeconds(TransformHelpers.GetDuration(closest, m_destroyLocation.position, tweenSpeed));

            iTween.Stop(closest.gameObject);

            closest.rigidbody.isKinematic = false;
            closest.rigidbody.angularVelocity = Vector3.zero;
            closest.rigidbody.AddForce(new Vector3(0, 2, 0), ForceMode.VelocityChange);
        }

        yield break;
    }

    void OnPressSubmit(PipButton button)
    {
        D.Log("Tracking:" + m_triggerTracker.GetNumTrackedObjects());

        if (m_triggerTracker.GetNumTrackedObjects() == m_currentData.GetInt("value"))
        {
            m_scoreKeeper.UpdateScore();

            StartCoroutine(ClearQuestion());
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_EXHALE");
            iTween.ShakePosition(m_container, Vector3.one * 0.2f, 0.35f);
        }
    }

    IEnumerator ClearQuestion()
    {
        float tweenDuration = 0.25f;

        Hashtable tweenArgs = new Hashtable();

        tweenArgs.Add("position", m_containerRightOffLocation);
        tweenArgs.Add("easetype", iTween.EaseType.linear);
        tweenArgs.Add("time", tweenDuration);

        iTween.MoveTo(m_container, tweenArgs);

        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");

        yield return new WaitForSeconds(tweenDuration);

        CollectionHelpers.DestroyObjects(m_spawnedRbs);

        if (m_scoreKeeper.HasCompleted())
        {
            StartCoroutine(CompleteGame());
        }
        else
        {
            m_container.transform.position = m_containerLeftOffLocation.position;
            StartCoroutine(AskQuestion());
        }
    }

    void OnRBEnterDestroyArea(GameObject go)
    {
        m_spawnedRbs.Remove(go);
    }

    protected override IEnumerator CompleteGame()
    {
        yield return StartCoroutine(m_scoreKeeper.On());

        GameManager.Instance.CompleteGame();
    }
}
*/
