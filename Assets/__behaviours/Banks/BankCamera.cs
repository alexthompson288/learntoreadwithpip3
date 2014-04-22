using UnityEngine;
using System.Collections;

public class BankCamera : Singleton<BankCamera> 
{
    [SerializeField]
    private ClickEvent m_toIndexButton;
    [SerializeField]
    private Transform m_index;
    [SerializeField]
    private ClickEvent m_toShowButton;
    [SerializeField]
    private Transform m_show;
    [SerializeField]
    private float m_tweenDuration;

    void Awake()
    {
        if (m_toIndexButton != null)
        {
            m_toIndexButton.OnSingleClick += OnClickMoveToIndex;
        }

        if (m_toShowButton != null)
        {
            m_toShowButton.OnSingleClick += OnClickMoveToShow;
        }
    }

    void OnClickMoveToIndex(ClickEvent click)
    {
        TweenToPos(m_index);
    }

    void OnClickMoveToShow(ClickEvent click)
    {
        TweenToPos(m_show);
    }

    public void MoveToShow()
    {
        TweenToPos(m_show);
    }

	void TweenToPos(Transform tra)
    {
        iTween.MoveTo(gameObject, tra.position, m_tweenDuration);
    }
}
