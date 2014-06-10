﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class ScoreLights : ScoreKeeper
{
    [SerializeField]
    private GameObject m_scoreQuizMarkerPrefab;
    [SerializeField]
    private Transform m_lowTransform;
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

    List<GameObject> m_spawnedMarkers = new List<GameObject>();

    int m_markerIndex = 0;

    void Awake()
    {
        m_scoreLabel.text = m_score.ToString();
    }

    public override void SetTargetScore(int targetScore)
    {
        base.SetTargetScore(targetScore);

        for (int i = m_spawnedMarkers.Count - 1; i > -1; --i)
        {
            Destroy(m_spawnedMarkers[i]);
        }
        m_spawnedMarkers.Clear();

        int divisor = m_targetScore > 1 ? m_targetScore - 1 : m_targetScore;
        Vector3 delta = (m_highTransform.transform.localPosition - m_lowTransform.transform.localPosition) / divisor;

        for (int index = 0; index < targetScore; ++index)
        {
            GameObject newMarker = SpawningHelpers.InstantiateUnderWithPrefabTransforms(m_scoreQuizMarkerPrefab, m_lowTransform);
            newMarker.transform.localPosition = delta * index;
            //newMarker.GetComponentInChildren<UISprite>().color = m_unansweredColor;
            m_spawnedMarkers.Add(newMarker);
        }
    }

    public override void UpdateScore(int delta)
    {

        if (m_markerIndex < m_targetScore)
        {
            //Color col = delta == 0 ? m_incorrectColor : m_correctColor;
            //TweenColor.Begin(m_spawnedMarkers[m_markerIndex].gameObject, 0.3f, col);
            m_spawnedMarkers[m_markerIndex].GetComponentInChildren<UISprite>().spriteName = delta == 0 ? "light_red" : "light_green";

            PlayAudio(delta);
        }

        ++m_markerIndex;

        base.UpdateScore(delta);

        m_scoreLabel.text = m_score.ToString();
    }
}
