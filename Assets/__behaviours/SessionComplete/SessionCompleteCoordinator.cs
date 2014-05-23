﻿using UnityEngine;
using System.Collections;
using Wingrove;

public class SessionCompleteCoordinator : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_sessionCompleteButtonPrefab;
    [SerializeField]
    private UIGrid m_grid;
    [SerializeField]
    private string[] m_spriteNames;
    [SerializeField]
    private GameObject m_collectable;
    [SerializeField]
    private UISprite m_collectableBackground;
    [SerializeField]
    private UILabel m_collectableLabel;
    [SerializeField]
    private UITexture m_collectableIcon;
    [SerializeField]
    private TweenOnOffBehaviour m_collector;
    [SerializeField]
    private Transform m_collectorBackpack;
    [SerializeField]
    private Transform m_collectorEndLocation;
    [SerializeField]
    private ClickEvent m_nextButton;

    ThrobGUIElement m_currentThrobBehaviour = null;

	// Use this for initialization
	IEnumerator Start () 
    {
        m_nextButton.OnSingleClick += OnNextButtonClick;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        bool hasSetLabel = false;
        bool hasSetIcon = false;

        if (VoyageInfo.Instance.hasBookmark)
        {
            int sessionId = VoyageInfo.Instance.currentSessionId;
            
            for (int i = 0; i < m_spriteNames.Length; ++i)
            {
                GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_sessionCompleteButtonPrefab, m_grid.transform);
                newButton.GetComponent<ClickEvent>().OnSingleClick += OnButtonClick;
                newButton.GetComponent<ClickEvent>().SetString(m_spriteNames[i]);
                newButton.GetComponentInChildren<UISprite>().spriteName = m_spriteNames[i];
                
                if(VoyageInfo.Instance.GetSessionBackground(sessionId) == m_spriteNames[i])
                {
                    m_currentThrobBehaviour = newButton.GetComponent<ThrobGUIElement>() as ThrobGUIElement;
                    m_currentThrobBehaviour.On();
                    
                    m_collectableBackground.spriteName = m_spriteNames[i];
                }
            }

            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programmodules WHERE id=" + VoyageInfo.Instance.currentModuleId);

            if(dt.Rows.Count > 0 && dt.Rows[0]["modulereward"] != null)
            {
                string moduleReward = dt.Rows[0]["modulereward"].ToString();

                if(moduleReward == "Phonemes")
                {
                    dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE programsession_id=" + sessionId);

                    foreach (DataRow phoneme in dt.Rows)
                    {
                        if (phoneme["is_target_phoneme"] != null && phoneme ["is_target_phoneme"].ToString() == "t")
                        {
                            m_collectableLabel.text = phoneme["phoneme"].ToString();
                            hasSetLabel = true;
                        }
                    }
                }
                else
                {
                    hasSetIcon = true;
                }
            }
        } 

        m_collectableLabel.gameObject.SetActive(hasSetLabel);
        m_collectableIcon.gameObject.SetActive(hasSetIcon);


        m_grid.Reposition();
	}

    void OnNextButtonClick(ClickEvent click)
    {
        StartCoroutine(CompleteGame());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            CharacterPopper popper = Object.FindObjectOfType(typeof(CharacterPopper)) as CharacterPopper;
            popper.PopCharacter(0);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            CharacterPopper popper = Object.FindObjectOfType(typeof(CharacterPopper)) as CharacterPopper;
            popper.PopCharacter(1);
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            CharacterPopper popper = Object.FindObjectOfType(typeof(CharacterPopper)) as CharacterPopper;
            popper.PopCharacter(2);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            CharacterPopper popper = Object.FindObjectOfType(typeof(CharacterPopper)) as CharacterPopper;
            popper.PopCharacter(3);
        }
    }

    IEnumerator CompleteGame()
    {
        m_collector.On();

        float largeScaleDuration = 0.5f;

        //iTween.ScaleTo(m_collectable, Vector3.one * 1.2f, largeScaleDuration);
        iTween.ShakePosition(m_collectable, Vector3.one * 0.1f, m_collector.GetDuration());

        WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");

        yield return new WaitForSeconds(m_collector.GetDuration() + 0.2f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");

        float smallScaleDuration = 0.8f;

        iTween.ScaleTo(m_collectable, Vector3.zero, smallScaleDuration);
        iTween.MoveTo(m_collectable, m_collectorBackpack.position, smallScaleDuration);

        yield return new WaitForSeconds(smallScaleDuration + 0.5f);

        iTween.MoveTo(m_collector.gameObject, m_collectorEndLocation.position, 0.6f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEAR");

        GameManager.Instance.CompleteGame();
    }

    void OnButtonClick(ClickEvent click)
    {
        if (m_currentThrobBehaviour != null)
        {
            m_currentThrobBehaviour.Off();
        }

        m_currentThrobBehaviour = click.GetComponent<ThrobGUIElement>() as ThrobGUIElement;
        m_currentThrobBehaviour.On();

        m_collectableBackground.spriteName = click.GetString();

        if (VoyageInfo.Instance.hasBookmark)
        {
            Debug.Log("Setting Background: " + VoyageInfo.Instance.currentSessionId + " - " + click.GetString());
            VoyageInfo.Instance.AddSessionBackground(VoyageInfo.Instance.currentSessionId, click.GetString());
        }
    }
}
