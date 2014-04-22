﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BankWordCoordinator : MonoBehaviour 
{
    [SerializeField]
    private ClickEvent m_correctButton;
    [SerializeField]
    private ClickEvent m_incorrectButton;
    [SerializeField]
    private ClickEvent m_leftArrow;
    [SerializeField]
    private ClickEvent m_rightArrow;
    [SerializeField]
    private ClickEvent m_showButtonsButton;
    [SerializeField]
    private ClickEvent m_showPictureButton;
    [SerializeField]
    private ClickEvent m_backToIndexButton;

    bool m_showSounds;

    List<DataRow> m_wordPool = new List<DataRow>();

    int m_currentIndex = 0;

    bool m_pictureActive = false;
    bool m_buttonsActive = false;
    
    void Start()
    {
        BankIndexCoordinator.Instance.OnMoveToShow += RefreshWordPool;

        m_showSounds = GameManager.Instance.dataType != "Keywords";

        m_backToIndexButton.OnSingleClick += OnClickBackToIndex;
        m_correctButton.OnSingleClick += OnClickCorrect;
        m_incorrectButton.OnSingleClick += OnClickIncorrect;
        m_leftArrow.OnSingleClick += OnLeftArrowClick;
        m_rightArrow.OnSingleClick += OnRightArrowClick;
        m_showPictureButton.OnSingleClick += TogglePicture;
        m_showButtonsButton.OnSingleClick += ToggleButtons;
    }

    void RefreshWordPool(DataRow word, string s)
    {
        m_wordPool = DataHelpers.GetWords();

        m_currentIndex = word == null ? 0 : m_wordPool.IndexOf(word);

        StartCoroutine(RefreshPipPad());
    }

    IEnumerator RefreshPipPad()
    {
        Debug.Log("currentIndex: " + m_currentIndex);
        Debug.Log("wordPool.Count: " + m_wordPool.Count);
        if (m_wordPool.Count > 0)
        {
            string word = m_wordPool [m_currentIndex] ["word"].ToString();
            
            Texture2D tex = Resources.Load<Texture2D>("Images/word_images_png_350/_" + word);
            float tweenDuration = 0.3f;
            if (tex != null)
            {
                TweenScale.Begin(m_showPictureButton.gameObject, tweenDuration, Vector3.one);
            } else
            {
                TweenScale.Begin(m_showPictureButton.gameObject, tweenDuration, Vector3.zero);
            }
            
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log("RefreshPipPad()");
            
            PipPadBehaviour.Instance.Show(word);
            
            PipPadBehaviour.Instance.EnableButtons(false);
            PipPadBehaviour.Instance.EnableSayWholeWordButton(false);
            m_buttonsActive = false;
            
            EnableClickEventColliders(true);
        }
    }

    void ToggleButtons(ClickEvent clickBehaviour)
    {
        m_buttonsActive = !m_buttonsActive;
        
        PipPadBehaviour.Instance.EnableButtons(m_buttonsActive);
        PipPadBehaviour.Instance.EnableSayWholeWordButton(m_buttonsActive);
        
        string audioEvent = m_buttonsActive ? "SOMETHING_APPEARS" : "SOMETHING_DISAPPEARS";
        WingroveAudio.WingroveRoot.Instance.PostEvent(audioEvent);
    }
    
    void TogglePicture(ClickEvent clickBehaviour)
    {
        Debug.Log("TogglePicture()");
        
        m_pictureActive = !m_pictureActive;
        
        if(m_pictureActive)
        {
            PipPadBehaviour.Instance.ReShowWordImage();
        }
        else
        {
            PipPadBehaviour.Instance.HideAllBlackboards();
        }
    }

    void OnClickBackToIndex(ClickEvent click)
    {
        BankIndexCoordinator.Instance.RefreshButtons();
        PipPadBehaviour.Instance.Hide();
    }

    void OnClickCorrect(ClickEvent click)
    {
        BankInfo.Instance.NewAnswer(System.Convert.ToInt32(m_wordPool[m_currentIndex]["id"]), true);
        OnArrowClick(1);
    }
    
    void OnClickIncorrect(ClickEvent click)
    {
        BankInfo.Instance.NewAnswer(System.Convert.ToInt32(m_wordPool[m_currentIndex]["id"]), false);
        OnArrowClick(1);
    }
    
    void OnLeftArrowClick(ClickEvent clickBehaviour)
    {
        OnArrowClick(-1);
    }
    
    void OnRightArrowClick(ClickEvent clickBehaviour)
    {
        OnArrowClick(1);
    }
    
    void OnArrowClick(int direction)
    {
        m_currentIndex += direction;
        
        if(m_currentIndex < 0)
        {
            m_currentIndex = m_wordPool.Count - 1;
        }
        else if(m_currentIndex >= m_wordPool.Count)
        {
            m_currentIndex = 0;
        }
        
        EnableClickEventColliders(false);
        
        PipPadBehaviour.Instance.Hide();
        
        PipPadBehaviour.Instance.HideAllBlackboards();
        m_pictureActive = false;
        
        StartCoroutine(RefreshPipPad());
    }

    void EnableClickEventColliders(bool enable)
    {
        m_correctButton.EnableCollider(enable);
        m_incorrectButton.EnableCollider(enable);
        m_leftArrow.EnableCollider(enable);
        m_rightArrow.EnableCollider(enable);
        m_showButtonsButton.EnableCollider(enable);
        m_showPictureButton.EnableCollider(enable);
    }
}