﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class BankLettersCoordinator : MonoBehaviour 
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
    private ClickEvent m_backToIndexButton;
    [SerializeField]
    private ClickEvent m_soundButton;
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private UITexture m_mnemonicTexture;
    [SerializeField]
    private AudioSource m_audioSource;
    
    List<DataRow> m_phonemePool = new List<DataRow>();
    List<string> m_letterPool = new List<string>();
    
    int m_currentIndex = 0;
    
    bool m_pictureActive = false;
    bool m_buttonsActive = false;

    bool m_isAlphabet;
    
    void Start()
    {
#if UNITY_EDITOR
        m_isAlphabet = GameManager.Instance.dataType == "alphabet" || System.String.IsNullOrEmpty(GameManager.Instance.dataType);
#else
        m_isAlphabet = GameManager.Instance.dataType == "alphabet";
#endif

        BankIndexCoordinator.Instance.OnMoveToShow += Refresh;

        if (m_isAlphabet)
        {
            for (char i = 'a'; i <= 'z'; ++i)
            {
                m_letterPool.Add(i.ToString());
            }
        } 

        m_mnemonicTexture.gameObject.SetActive(!m_isAlphabet);

        m_soundButton.OnSingleClick += OnClickSoundButton;
        m_backToIndexButton.OnSingleClick += OnClickBackToIndex;
        m_correctButton.OnSingleClick += OnClickCorrect;
        m_incorrectButton.OnSingleClick += OnClickIncorrect;
        m_leftArrow.OnSingleClick += OnLeftArrowClick;
        m_rightArrow.OnSingleClick += OnRightArrowClick;
    }
    
    void Refresh(DataRow phoneme, string s)
    {
        if (m_isAlphabet)
        {
            m_currentIndex = System.String.IsNullOrEmpty(s) ? 0 : m_letterPool.IndexOf(s);
        } 
        else
        {
            m_phonemePool = DataHelpers.GetLetters();
            m_currentIndex = phoneme == null ? 0 : m_phonemePool.IndexOf(phoneme);
        }

        ShowNew();
    }

    void ShowNew()
    {
        Debug.Log("ShowNew()");

        Debug.Log("count: " + m_phonemePool.Count);
        Debug.Log("currentIndex: " + m_currentIndex);

        string labelText = m_isAlphabet ? m_letterPool [m_currentIndex] : m_phonemePool [m_currentIndex] ["phoneme"].ToString();
        m_label.text = labelText;

        // Mnemonic Picture
        if (!m_isAlphabet)
        {
            Texture2D tex = Resources.Load<Texture2D>(System.String.Format("Images/mnemonics_images_png_250/{0}_{1}",
                                                      m_phonemePool[m_currentIndex]["phoneme"],
                                                      m_phonemePool[m_currentIndex]["mneumonic"].ToString().Replace(" ", "_")));

            m_mnemonicTexture.gameObject.SetActive(tex != null);
            m_mnemonicTexture.mainTexture = tex;
        }
    }

    void OnClickSoundButton(ClickEvent click)
    {
        AudioClip clip = m_isAlphabet ? AudioBankManager.Instance.GetAudioClip("lettername_" + m_letterPool[m_currentIndex]) : LoaderHelpers.LoadMnemonic(m_phonemePool[m_currentIndex]);

        if(clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }
    }
    
    void OnClickBackToIndex(ClickEvent click)
    {
        BankIndexCoordinator.Instance.RefreshButtons();
    }
    
    void OnClickCorrect(ClickEvent click)
    {
        if (m_isAlphabet)
        {
            BankInfo.Instance.NewAnswer(m_letterPool[m_currentIndex], true);
        } 
        else
        {
            BankInfo.Instance.NewAnswer(System.Convert.ToInt32(m_phonemePool [m_currentIndex] ["id"]), true);
        }

        OnArrowClick(1);
    }
    
    void OnClickIncorrect(ClickEvent click)
    {
        if (m_isAlphabet)
        {
            BankInfo.Instance.NewAnswer(m_letterPool[m_currentIndex], false);
        } 
        else
        {
            BankInfo.Instance.NewAnswer(System.Convert.ToInt32(m_phonemePool [m_currentIndex] ["id"]), false);
        }

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

        int collectionCount = m_isAlphabet ? m_letterPool.Count : m_phonemePool.Count;

        if(m_currentIndex < 0)
        {
            m_currentIndex = collectionCount - 1;
        }
        else if(m_currentIndex >= collectionCount)
        {
            m_currentIndex = 0;
        }
        
        //EnableClickEventColliders(false);

        ShowNew();
    }
    
    void EnableClickEventColliders(bool enable)
    {
        m_correctButton.EnableCollider(enable);
        m_incorrectButton.EnableCollider(enable);
        m_leftArrow.EnableCollider(enable);
        m_rightArrow.EnableCollider(enable);
        m_soundButton.EnableCollider(enable);
    }
}