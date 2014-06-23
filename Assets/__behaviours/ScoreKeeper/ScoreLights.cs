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

        if (m_targetScore % 2 == 1)
        {
            //lightLocalPos += (m_lightDelta / 2f);
        }

        for (int i = 0; i < targetScore; ++i)
        {
            GameObject newLight = SpawningHelpers.InstantiateUnderWithPrefabTransforms(m_lightPrefab, m_centreTransform);
            newLight.transform.localPosition = lightLocalPos;
            lightLocalPos += m_lightDelta;
            m_spawnedLights.Add(newLight);
        }

        /*
        int divisor = m_targetScore > 1 ? m_targetScore - 1 : m_targetScore;
        Vector3 delta = (m_highTransform.transform.localPosition - m_lowTransform.transform.localPosition) / divisor;

        Debug.Log("delta: " + delta);

        for (int index = 0; index < targetScore; ++index)
        {
            GameObject newLight = SpawningHelpers.InstantiateUnderWithPrefabTransforms(m_lightPrefab, m_lowTransform);
            newLight.transform.localPosition = delta * index;
            m_spawnedLights.Add(newLight);
        }
        */
    }


    public override void UpdateScore(int delta)
    {
        if (m_markerIndex < m_targetScore)
        {
            m_spawnedLights[m_markerIndex].GetComponentInChildren<UISprite>().spriteName = delta == 0 ? "light_red" : "light_green";
        }

        ++m_markerIndex;

        base.UpdateScore(delta);

        m_scoreLabel.text = m_score.ToString();

        PlayAudio(delta);
    }

    public override bool HasCompleted()
    {
        return m_numAnswered >= m_targetScore;
    }
}
