using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class SwatFliesPlayer : GamePlayer
{
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;
    [SerializeField]
    private DataDisplay m_dataDisplay;
    [SerializeField]
    private Spline[] m_splines;
    [SerializeField]
    private Transform m_spawnParent;
    [SerializeField]
    private Transform m_swatter;
    [SerializeField]
    private Transform m_swatterOff;

    int m_score;

    List<GameWidget> m_spawnedWidgets = new List<GameWidget>();

    public int GetScore()
    {
        return m_score;
    }

    public void ShowDataDisplay(string dataType, DataRow data)
    {
        m_dataDisplay.On(dataType, data);
    }

    public void HideDataDisplay()
    {
        m_dataDisplay.Off();
    }

    // Separate Coroutine for delay so that we can stop the coroutine without stopping all coroutines
    public IEnumerator HideDataDisplayDelay()
    {
        yield return new WaitForSeconds(2);
        m_dataDisplay.Off();
    }

    public void SetUp()
    {
        m_swatter.position = m_swatterOff.position;
        m_dataDisplay.SetShowPicture(false);
        m_scoreKeeper.SetTargetScore(SwatFliesCoordinator.Instance.GetTargetScore());
    }

    IEnumerator ResetSwatter()
    {
        yield return new WaitForSeconds(0.2f);
        m_swatter.position = m_swatterOff.position;
    }

    // Start coroutine with string so that we can stop it without stopping all coroutines
    public void StartGame()
    {
        StartCoroutine("SpawnFly");
    }

    public IEnumerator SpawnFly()
    {
        SwatFliesCoordinator coordinator = SwatFliesCoordinator.Instance; // Temporary variable saves lots of typing

        DataRow currentData = coordinator.GetCurrentData();
        DataRow data = currentData;

        if (Random.Range(0f, 1f) >= coordinator.GetProbabilityDataIsCurrent())
        {
            while(data == currentData)
            {
                data = coordinator.GetRandomData();
            }
        }

        GameObject newFly = SpawningHelpers.InstantiateUnderWithIdentityTransforms(coordinator.GetFlyPrefab(), m_spawnParent);

        GameWidget widget = newFly.GetComponentInChildren<GameWidget>() as GameWidget;
        widget.SetUp(coordinator.GetDataType(), data, false);
        widget.onClick += OnSwatFly;
        m_spawnedWidgets.Add(widget);

        SplineFollower follower = newFly.GetComponent<SplineFollower>() as SplineFollower;
        follower.AddSpline(m_splines[Random.Range(0, m_splines.Length)]);
        StartCoroutine(follower.On());

        yield return new WaitForSeconds(Random.Range(coordinator.GetMinSpawnDelay(), coordinator.GetMaxSpawnDelay()));

        StartCoroutine("SpawnFly");
    }

    void OnSwatFly(GameWidget widget)
    {
        if (widget.data.GetId() == SwatFliesCoordinator.Instance.GetCurrentId())
        {
            StartCoroutine(OnCorrect(widget));
        } 
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_EXHALE");

            SwatFliesCoordinator.Instance.PlayAudio();

            StopCoroutine("HideDataDisplayDelay");
            ShowDataDisplay(SwatFliesCoordinator.Instance.GetDataType(), SwatFliesCoordinator.Instance.GetCurrentData());
            StartCoroutine("HideDataDisplayDelay");

            widget.EnableCollider(false);
            widget.Shake();
            widget.Tint(Color.grey);
        }
    }

    IEnumerator OnCorrect(GameWidget widget)
    {
        HideDataDisplay();

        StopCoroutine("ResetSwatter");
        m_swatter.transform.position = widget.transform.position;
        StartCoroutine("ResetSwatter");

        WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");
        WingroveAudio.WingroveRoot.Instance.PostEvent("SPLAT_MUSHROOM");

        widget.ChangeBackgroundState();

        StartCoroutine(DestroyFly(widget));

        ++m_score;

        m_scoreKeeper.UpdateScore();

        if (m_score >= SwatFliesCoordinator.Instance.GetTargetScore())
        {
            StopGame();
            SwatFliesCoordinator.Instance.OnPlayerFinish(m_playerIndex);
            yield return StartCoroutine(m_scoreKeeper.On());
            SwatFliesCoordinator.Instance.OnWinningPlayerCompleteSequence();
        }

        yield return null;
    }

    IEnumerator DestroyFly(GameWidget widget)
    {
        GameObject fly = widget.transform.parent.gameObject;

        m_spawnedWidgets.Remove(widget);

        yield return StartCoroutine(widget.OffCo());

        Destroy(fly);
    }

    public void StopGame()
    {
        StopCoroutine("SpawnFly");

        Debug.Log("widgets.Count: " + m_spawnedWidgets.Count);
        for (int i = m_spawnedWidgets.Count - 1; i > -1; --i)
        {
            StartCoroutine(DestroyFly(m_spawnedWidgets[i]));
        }
    }
}
