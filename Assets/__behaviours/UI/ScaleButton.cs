using UnityEngine;
using System.Collections;

public class ScaleButton : MonoBehaviour 
{
    /// <summary>
    /// Simple example script of how a button can be scaled visibly when the mouse hovers over it or it gets pressed.
    /// </summary>
    
    [AddComponentMenu("NGUI/Interaction/Button Scale")]
    public Transform[] m_tweenTargets;
    public Vector3 m_hover = Vector3.one;
    public Vector3 m_pressed = new Vector3(0.8f, 0.8f, 0.8f);
    public float m_duration = 0.1f;
    
    Vector3 mScale;
    bool mStarted = false;

    bool HasTweenTargets()
    {
        return m_tweenTargets != null && m_tweenTargets.Length > 0;
    }
    
    void Start ()
    {
        if (!mStarted)
        {
            mStarted = true;
            if (!HasTweenTargets()) 
            {
                m_tweenTargets = new Transform[1];
                m_tweenTargets[0] = transform;
            }
            mScale = m_tweenTargets[0].localScale;
        }
    }
    
    void OnEnable () { if (mStarted) OnHover(UICamera.IsHighlighted(gameObject)); }
    
    void OnDisable ()
    {
        if (mStarted && HasTweenTargets())
        {
            foreach(Transform tweenTarget in m_tweenTargets)
            {
                TweenScale tc = tweenTarget.GetComponent<TweenScale>();
                
                if (tc != null)
                {
                    tc.value = mScale;
                    tc.enabled = false;
                }
            }
        }
    }
    
    void OnPress (bool isPressed)
    {
        if (enabled)
        {
            if (!mStarted) Start();

            foreach(Transform tweenTarget in m_tweenTargets)
            {
                TweenScale.Begin(tweenTarget.gameObject, m_duration, isPressed ? Vector3.Scale(mScale, m_pressed) :
                                 (UICamera.IsHighlighted(gameObject) ? Vector3.Scale(mScale, m_hover) : mScale)).method = UITweener.Method.EaseInOut;
            }
        }
    }
    
    void OnHover (bool isOver)
    {
        if (enabled)
        {
            if (!mStarted) Start();

            foreach(Transform tweenTarget in m_tweenTargets)
            {
                TweenScale.Begin(tweenTarget.gameObject, m_duration, isOver ? Vector3.Scale(mScale, m_hover) : mScale).method = UITweener.Method.EaseInOut;
            }
        }
    }
    
    void OnSelect (bool isSelected)
    {
        if (enabled && (!isSelected || UICamera.currentScheme == UICamera.ControlScheme.Controller))
            OnHover(isSelected);
    }
}
