﻿using UnityEngine;
using System.Collections;

public class AtoBPressButton : MonoBehaviour {

    [SerializeField]
    private UISprite m_switchSprite;
    [SerializeField]
    private bool m_bToC = false;
    [SerializeField]
    private bool m_manualOnly = false;

	// Use this for initialization
	void Start () {

        string spriteName = m_switchSprite.spriteName;
	}

    void OnPress(bool isDown)
    {
        if (!m_manualOnly)
        {
            if (isDown)
            {
                m_switchSprite.spriteName = m_switchSprite.spriteName.Substring(0, m_switchSprite.spriteName.Length - 1) + (m_bToC ? "c" : "b");
            }
            else
            {
                m_switchSprite.spriteName = m_switchSprite.spriteName.Substring(0, m_switchSprite.spriteName.Length - 1) + (m_bToC ? "b" : "a");
            }
        }
    }

    public void ManualActivate(bool isDown)
    {
        if (isDown)
        {
            m_switchSprite.spriteName = m_switchSprite.spriteName.Substring(0, m_switchSprite.spriteName.Length - 1) + (m_bToC ? "c" : "b");
        }
        else
        {
            m_switchSprite.spriteName = m_switchSprite.spriteName.Substring(0, m_switchSprite.spriteName.Length - 1) + (m_bToC ? "b" : "a");
        }
    }
}