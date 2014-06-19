﻿using UnityEngine;
using System.Collections;

public class AnimManager : MonoBehaviour 
{
    [SerializeField]
    private bool m_startRandomOn = true;
    [SerializeField]
    private SimpleSpriteAnim m_spriteAnim;
    [SerializeField]
    private string[] m_randomAnimNames = new string [] { "ON" };
    [SerializeField]
    private Vector2 m_randomDelayRange = new Vector2(3f, 6f);

    void Start()
    {
        if (m_spriteAnim == null)
        {
            m_spriteAnim = GetComponent<SimpleSpriteAnim>() as SimpleSpriteAnim;
        }

        if (m_spriteAnim != null && m_startRandomOn)
        {
            StartCoroutine(PlayRandom());
        }
    }

    public void PlayAnimation(string animName)
    {
        if (m_spriteAnim.HasAnim(animName))
        {
            StopRandom();

            m_spriteAnim.AnimFinished += OnNamedFinish;

            m_spriteAnim.PlayAnimation(animName);
        }
    }

    void OnNamedFinish(string animName)
    {
        m_spriteAnim.AnimFinished -= OnNamedFinish;

        StartCoroutine(PlayRandom());
    }

	void OnRandomFinish(string animName)
    {
        // Subscribe/Unsubscribe from event every time so that this is not accidentally called if the SimpleSpriteAnim finishes a looping static 1-frame animation
        m_spriteAnim.AnimFinished -= OnRandomFinish;

        StartCoroutine(PlayRandom());
    }

    public void StartRandom()
    {
        StartCoroutine(PlayRandom());
    }

    IEnumerator PlayRandom()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(m_randomDelayRange.x, m_randomDelayRange.y));

        // Subscribe/Unsubscribe from event every time so that this is not accidentally called if the SimpleSpriteAnim finishes a looping static 1-frame animation
        if (m_randomAnimNames.Length > 0)
        {
            m_spriteAnim.AnimFinished += OnRandomFinish;
            m_spriteAnim.PlayAnimation(m_randomAnimNames [Random.Range(0, m_randomAnimNames.Length)]);
        }
    }

    public void StopRandom()
    {
        if (m_spriteAnim != null)
        {
            m_spriteAnim.AnimFinished -= OnRandomFinish;
            StopAllCoroutines();
        }
    }
}