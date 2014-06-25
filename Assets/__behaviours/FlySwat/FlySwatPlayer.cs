using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class FlySwatPlayer : GamePlayer
{
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;
    [SerializeField]
    private DataDisplay m_dataDisplay;
    [SerializeField]
    private List<Spline> m_splines = new List<Spline>();
    [SerializeField]
    private Transform m_spawnParent;
    [SerializeField]
    private Transform m_swatter;
    [SerializeField]
    private Transform m_swatterOff;
    [SerializeField]
    private TrafficLights m_trafficLights;

    List<GameWidget> m_spawnedWidgets = new List<GameWidget>();

    public override void SelectCharacter(int characterIndex)
    {
        Debug.Log("SelectCharacter");
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;
        Debug.Log("m_selectedCharacter: " + m_selectedCharacter);
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }
        FlySwatCoordinator.Instance.CharacterSelected(characterIndex);
    }

    public void ShowDataDisplay(string dataType, DataRow data)
    {
        m_dataDisplay.On(dataType, data);
    }

    public void HideDataDisplay()
    {
        m_dataDisplay.Off();
    }

    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
    }

    // Separate Coroutine for delay so that we can stop the coroutine without stopping all coroutines
    public IEnumerator HideDataDisplayDelay()
    {
        yield return new WaitForSeconds(2);
        m_dataDisplay.Off();
    }

    void Awake()
    {
        m_swatter.position = m_swatterOff.position;
    }

    public void SetUp()
    {
        m_dataDisplay.SetShowPicture(false);
        m_scoreKeeper.SetTargetScore(FlySwatCoordinator.Instance.GetTargetScore());
    }

    IEnumerator ResetSwatter()
    {
        yield return new WaitForSeconds(0.3f);
        //m_swatter.position = m_swatterOff.position;
        iTween.MoveTo(m_swatter.gameObject, m_swatterOff.position, 0.2f);
        yield return null;
    }

    // Start coroutine with string so that we can stop it without stopping all coroutines
    public void StartGame()
    {
        StartCoroutine("SpawnFly");
    }

    public IEnumerator SpawnFly()
    {
        if (m_splines.Count > 0)
        {
            FlySwatCoordinator coordinator = FlySwatCoordinator.Instance; // Temporary variable saves lots of typing
            
            DataRow currentData = coordinator.GetCurrentData();
            DataRow data = currentData;
            
            if (Random.Range(0f, 1f) >= coordinator.GetProbabilityDataIsCurrent())
            {
                while (data == currentData)
                {
                    data = coordinator.GetRandomData();
                }
            }
            
            GameObject newFly = SpawningHelpers.InstantiateUnderWithIdentityTransforms(coordinator.GetFlyPrefab(), m_spawnParent);
            
            GameWidget widget = newFly.GetComponentInChildren<GameWidget>() as GameWidget;
            widget.SetUp(coordinator.GetDataType(), data, false);
            widget.Pressing += OnSwatFly;
            widget.Destroying += OnWidgetDestroy;
            m_spawnedWidgets.Add(widget);
            
            SplineFollower follower = newFly.GetComponent<SplineFollower>() as SplineFollower;
            
            //int splineIndex = Random.Range(0, m_legalSplines.Count);
            //follower.AddSpline(m_legalSplines [splineIndex]);
            //m_legalSplines.RemoveAt(splineIndex);

            Spline spline = m_splines [Random.Range(0, m_splines.Count)];
            follower.AddSpline(spline);
            m_splines.Remove(spline);
            StartCoroutine(AddSpline(spline));
            
            StartCoroutine(follower.On());
            
            
            yield return new WaitForSeconds(Random.Range(coordinator.GetMinSpawnDelay(), coordinator.GetMaxSpawnDelay()));
        }
        else
        {
            yield return null;
        }

        StartCoroutine("SpawnFly");
    }

    IEnumerator AddSpline(Spline spline)
    {
        yield return new WaitForSeconds(2f);

        m_splines.Add(spline);
    }

    void OnSwatFly(GameWidget widget)
    {
        if (widget.data.GetId() == FlySwatCoordinator.Instance.GetCurrentId())
        {
            StartCoroutine(OnCorrect(widget));
        } 
        else
        {
            //WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_EXHALE");
            m_scoreKeeper.UpdateScore(-1);

            FlySwatCoordinator.Instance.PlayAudio();

            StopCoroutine("HideDataDisplayDelay");
            ShowDataDisplay(FlySwatCoordinator.Instance.GetDataType(), FlySwatCoordinator.Instance.GetCurrentData());
            StartCoroutine("HideDataDisplayDelay");

            widget.EnableCollider(false);
            widget.Shake();
            widget.TintGray();
        }
    }

    IEnumerator OnCorrect(GameWidget widget)
    {
        HideDataDisplay();

        StopCoroutine("ResetSwatter");
        iTween.Stop(m_swatter.gameObject);
        m_swatter.transform.position = widget.transform.position;
        StartCoroutine("ResetSwatter");

        //WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");
        WingroveAudio.WingroveRoot.Instance.PostEvent("SPLAT_MUSHROOM");

        widget.GetComponentInChildren<SimpleSpriteAnim>().PlayAnimation("SPLAT");
        widget.GetComponentInParent<SplineFollower>().Stop();

        m_scoreKeeper.UpdateScore();

        if (m_scoreKeeper.HasCompleted())
        {
            StopGame();
            FlySwatCoordinator.Instance.OnPlayerFinish(m_playerIndex);
            yield return StartCoroutine(m_scoreKeeper.On());
            FlySwatCoordinator.Instance.OnWinningPlayerCompleteSequence();
        }

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(DestroyFly(widget));
    }

    void OnWidgetDestroy(GameWidget widget)
    {
        m_spawnedWidgets.Remove(widget);
    }

    IEnumerator DestroyFly(GameWidget widget)
    {
        if (widget != null)
        {
            m_spawnedWidgets.Remove(widget);

            GameObject fly = widget.transform.parent.gameObject;

            yield return StartCoroutine(widget.OffCo());

            Destroy(fly);
        }

        yield break;
    }

    public void StopGame()
    {
        StopCoroutine("SpawnFly");

        for (int i = m_spawnedWidgets.Count - 1; i > -1; --i)
        {
            StartCoroutine(DestroyFly(m_spawnedWidgets[i]));
        }
    }
}
