﻿using UnityEngine;
using System.Collections;

public class PipButton : MonoBehaviour 
{
    public delegate void PressEventHandler (PipButton button);
    public event PressEventHandler Pressing;
    public event PressEventHandler Unpressing;
    public event PressEventHandler Unpressed;
    
    
    // Shared
    [SerializeField]
    private UISprite m_pressableButton;
    [SerializeField]
    private float m_pressDuration = 0.3f;
    [SerializeField]
    private string m_pressedAudio = "BUTTON_PRESS";
    [SerializeField]
    private string m_unpressedAudio = "BUTTON_UNPRESS";
    [SerializeField]
    private ColorInfo.PipColor m_pipColor = ColorInfo.PipColor.White;
    [SerializeField]
    private UIWidget[] m_additionalColoredWidgets;
    [SerializeField]
    private SimpleSpriteAnim m_sheenAnimation;
    [SerializeField]
    private SimpleSpriteAnim m_pressAnimation;
    
    bool m_isTransitioning;
    
    public ColorInfo.PipColor pipColor
    {
        get
        {
            return m_pipColor;
        }
    }
    
    
    // PositionChange
    [SerializeField]
    private bool m_changePosition;
    [SerializeField]
    private Transform m_pressedLocation;
    [SerializeField]
    private Transform m_positionFollower;
    [SerializeField]
    private Transform m_followerLocation;
    [SerializeField]
    private GameObject m_emptyPrefab;
    [SerializeField]
    private iTween.EaseType m_positionEaseType = iTween.EaseType.linear;
    
    Transform m_unpressedLocation;
    Hashtable m_positionTweenArgs = new Hashtable();
    
    
    // SpriteChange
    [SerializeField]
    private bool m_changeSprite;
    [SerializeField]
    private bool m_resetSprite;
    [SerializeField]
    private string m_pressedSpriteName;
    
    string m_unpressedSpriteName;
    
    
    // ColorChange
    [SerializeField]
    private bool m_changeColor;
    [SerializeField]
    private Color m_pressedColor = Color.white;
    
    Color m_unpressedColor;
    
    
    void Awake()
    {
        if (m_pressableButton != null)
        {
            Color col = ColorInfo.GetColor(m_pipColor);
            m_pressableButton.color = col;
            
            foreach (UIWidget widget in m_additionalColoredWidgets)
            {
                widget.color = col;
            }
            
            if (m_changePosition)
            {
                GameObject unpressedLocationGo = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, transform);
                m_unpressedLocation = unpressedLocationGo.transform;
                m_unpressedLocation.position = m_pressableButton.transform.position;
                
                m_positionTweenArgs.Add("time", m_pressDuration);
                m_positionTweenArgs.Add("easetype", m_positionEaseType);
            }
            
            if (m_changeSprite)
            {
                m_unpressedSpriteName = m_pressableButton.spriteName;
                
                if (System.String.IsNullOrEmpty(m_pressedSpriteName))
                {
                    m_pressedSpriteName = DataHelpers.GetLinkedSpriteName(m_unpressedSpriteName);
                }
            }
            
            if (m_changeColor)
            {
                m_unpressedColor = m_pressableButton.color;
                
                // If unpressedColor hasn't been set then we want to set it to be a bit darker than the pressed color
                if (m_pressedColor == Color.white)
                {
                    m_unpressedColor = m_pressedColor;
                }
                
                if (Mathf.Approximately(m_pressedColor.r, m_unpressedColor.r) && Mathf.Approximately(m_pressedColor.g, m_unpressedColor.g) && Mathf.Approximately(m_pressedColor.b, m_unpressedColor.b))
                {
                    m_pressedColor = m_unpressedColor;
                    
                    for (int i = 0; i < 3; ++i) // Only change 0(r), 1(g), 2(b). 3 is alpha.
                    {
                        m_pressedColor [i] = Mathf.Clamp01(m_pressedColor [i] - 0.2f);
                    }
                }
            }
            
            if (m_sheenAnimation != null)
            {
                m_sheenAnimation.OnAnimFinish += OnSheenFinish;
                m_sheenAnimation.PlayAnimation("ON");
                StartCoroutine(PlaySheen());
            }
        }
    }
    
    void OnSheenFinish(string animName)
    {
        StartCoroutine(PlaySheen());
    }
    
    IEnumerator PlaySheen()
    {
        yield return new WaitForSeconds(Random.Range(3f, 9f));
        m_sheenAnimation.PlayAnimation("ON");
    }
    
    IEnumerator UpdateFollowerPosition()
    {
        m_positionFollower.position = m_followerLocation.position;
        yield return null;
        StartCoroutine("UpdateFollowerPosition");
    }
    
    void OnPress(bool pressed)
    {
        if (pressed)
        {
            StartCoroutine(Press());
        } 
        else
        {
            StartCoroutine(Unpress());
        }
    }
    
    IEnumerator Press()
    {
        while (m_isTransitioning)
        {
            yield return null;
        }
        
        if (Pressing != null)
        {
            Pressing(this);
        }
        
        m_isTransitioning = true;
        collider.enabled = false;
        
        WingroveAudio.WingroveRoot.Instance.PostEvent(m_pressedAudio);
        
        if (m_changePosition)
        {
            if (m_positionFollower != null)
            {
                StartCoroutine("UpdateFollowerPosition");
            }

            m_positionTweenArgs ["position"] = m_pressedLocation;
            iTween.MoveTo(m_pressableButton.gameObject, m_positionTweenArgs);
        }
        
        if (m_changeSprite)
        {
            m_pressableButton.spriteName = m_pressedSpriteName;
        }
        
        if (m_changeColor)
        {
            m_pressableButton.color = m_pressedColor;
        }
        
        if (m_pressAnimation != null)
        {
            m_pressAnimation.PlayAnimation("ON");
        }
        
        yield return new WaitForSeconds(m_pressDuration);
        
        collider.enabled = true;
        m_isTransitioning = false;
    }
    
    IEnumerator Unpress()
    {
        while (m_isTransitioning)
        {
            yield return null;
        }
        
        if (Unpressing != null)
        {
            Unpressing(this);
        }
        
        m_isTransitioning = true;
        collider.enabled = false;
        
        WingroveAudio.WingroveRoot.Instance.PostEvent(m_unpressedAudio);
        
        if (m_changePosition)
        {
            m_positionTweenArgs ["position"] = m_unpressedLocation;
            iTween.MoveTo(m_pressableButton.gameObject, m_positionTweenArgs);
        }
        
        if (m_changeSprite && m_resetSprite)
        {
            m_pressableButton.spriteName = m_unpressedSpriteName;
        }
        
        if (m_changeColor)
        {
            m_pressableButton.color = m_unpressedColor;
        }
        
        yield return new WaitForSeconds(m_pressDuration);
        
        if (Unpressed != null)
        {
            Unpressed(this);
        }

        if (m_changePosition && m_positionFollower != null)
        {
            Debug.Log("Stopping Coroutine");
            StopCoroutine("UpdateFollowerPosition");
        }
        
        collider.enabled = true;
        m_isTransitioning = false;
    }
}