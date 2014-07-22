using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class ScoreLights : ScoreKeeper
{
    [SerializeField]
    private GameObject m_lightPrefab;
    [SerializeField]
    private Transform m_lowTransform;
    [SerializeField]
    private Transform m_centreTransform;
    [SerializeField]
    private Transform m_highTransform;
    [SerializeField]
    private Color m_correctColor;
    [SerializeField]
    private Color m_incorrectColor;
    [SerializeField]
    private Color m_unansweredColor;
    [SerializeField]
    private UILabel m_scoreLabel;
    [SerializeField]
    private Vector3 m_lightDelta;

    List<GameObject> m_spawnedLights = new List<GameObject>();

    int m_markerIndex = 0;

    void Awake()
    {
        m_scoreLabel.text = m_score.ToString();
    }

    public override void SetTargetScore(int targetScore)
    {
        base.SetTargetScore(targetScore);

        for (int i = m_spawnedLights.Count - 1; i > -1; --i)
        {
            Destroy(m_spawnedLights[i]);
        }
        m_spawnedLights.Clear();

        Vector3 lightLocalPos = (-m_lightDelta * m_targetScore / 2f) + (m_lightDelta / 2f);

        for (int i = 0; i < targetScore; ++i)
        {
            GameObject newLight = SpawningHelpers.InstantiateUnderWithPrefabTransforms(m_lightPrefab, m_centreTransform);
            newLight.transform.localPosition = lightLocalPos;
            lightLocalPos += m_lightDelta;
            m_spawnedLights.Add(newLight);
        }
    }

    public override void UpdateScore(int delta)
    {
        if (m_markerIndex < m_targetScore)
        {
            m_spawnedLights[m_markerIndex].GetComponentInChildren<UISprite>().spriteName = delta > 0 ? "light_green" : "light_red";
        }

        ++m_markerIndex;

        base.UpdateScore(delta);

        m_scoreLabel.text = m_score.ToString();

        PlayAudio(delta);
    }

    public override IEnumerator On()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_WAHOO");
        WingroveAudio.WingroveRoot.Instance.PostEvent("DING");

        float tweenDuration = 0.3f;

        foreach (GameObject light in m_spawnedLights)
        {
            iTween.ScaleTo(light, Vector3.one * 1.3f, tweenDuration);
            light.GetComponent<RotateConstantly>().enabled = true;
        }

        yield return new WaitForSeconds(2f);
    }

    public override bool HasCompleted()
    {
        return m_numAnswered >= m_targetScore;
    }

#if UNITY_EDITOR
    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            StartCoroutine(On());
        }
        
        if (Input.GetKeyDown(KeyCode.O))
        {
            UpdateScore(-1);
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            UpdateScore(1);
        }
    }
#endif
}
