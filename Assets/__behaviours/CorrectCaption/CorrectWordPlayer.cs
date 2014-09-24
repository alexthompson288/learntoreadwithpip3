using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System;

public class CorrectWordPlayer : PlusGamePlayer 
{
    [SerializeField]
    private GameObject m_questionPictureParent;
    [SerializeField]
    private UITexture m_questionPicture;
    [SerializeField]
    private EventRelay[] m_answerRelays;
    [SerializeField]
    private UILabel[] m_answerLabels;
    [SerializeField]
    private UISprite[] m_answerCharacters;
    
    DataRow m_currentData;
    
    
    public void SetCurrentData(DataRow myCurrentData)
    {
        m_currentData = myCurrentData;
    }
    
    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
    }
    
    public override void SelectCharacter(int characterIndex)
    {
        base.SelectCharacter (characterIndex);
        CorrectWordCoordinator.Instance.CharacterSelected(characterIndex);
    }
    
    void Start()
    {
        Array.Sort(m_answerLabels, CollectionHelpers.LocalLeftToRight);
        Array.Sort(m_answerRelays, CollectionHelpers.LocalLeftToRight);
        Array.Sort(m_answerCharacters, CollectionHelpers.LocalLeftToRight);
        
        m_questionPictureParent.transform.localScale = Vector3.zero;
        
        foreach (UILabel label in m_answerLabels)
        {
            label.transform.localScale = Vector3.zero;
        }
    }
    
    public void StartGame()
    {       
        m_scoreKeeper.LevelledUp += OnLevelUp;
        m_scoreKeeper.Completed += OnScoreKeeperComplete;
        m_scoreKeeper.StartTimer();
        
        StartCoroutine(AskQuestion());
    }
    
    IEnumerator AskQuestion()
    {   
        Texture2D tex = DataHelpers.GetPicture(m_currentData);
        m_questionPicture.mainTexture = tex;
        m_questionPicture.gameObject.SetActive(tex != null);
        
        float tweenDuration = 0.3f;
        
        iTween.ScaleTo(m_questionPictureParent, Vector3.one, tweenDuration);

        List<string> answerStrings = new List<string>();
        
        // Add answer strings
        answerStrings.Add(m_currentData["word"].ToString());

        if (m_currentData [CorrectWordCoordinator.Instance.GetDummyAttribute1()] != null)
        {
            //D.Log("dummy1: " + m_currentData [CorrectWordCoordinator.Instance.GetDummyAttribute1()].ToString());
            answerStrings.Add(m_currentData [CorrectWordCoordinator.Instance.GetDummyAttribute1()].ToString());
        }

        if (m_currentData [CorrectWordCoordinator.Instance.GetDummyAttribute2()] != null)
        {
            //D.Log("dummy2: " + m_currentData [CorrectWordCoordinator.Instance.GetDummyAttribute2()].ToString());
            answerStrings.Add(m_currentData [CorrectWordCoordinator.Instance.GetDummyAttribute2()].ToString());
        }
        
        CollectionHelpers.Shuffle(answerStrings);
        
        for(int i = 0; i < m_answerLabels.Length; ++i)
        {
            m_answerCharacters[i].spriteName = m_answerCharacters[i].spriteName.Substring(0, m_answerCharacters[i].spriteName.Length - 1) + "a";

            if(i < answerStrings.Count)
            {
                m_answerRelays[i].SingleClicked += OnAnswer;
                
                m_answerLabels[i].color = Color.white;
                m_answerLabels[i].text = answerStrings[i];
                iTween.ScaleTo(m_answerLabels[i].gameObject, Vector3.one, tweenDuration);
            }
        }
        
        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");
        
        yield break;
    }
    
    void OnAnswer(EventRelay relay)
    {
        int answerIndex = Array.IndexOf(m_answerRelays, relay);
        
        if(answerIndex != -1)
        {
            bool isCorrect = m_answerLabels[answerIndex].text == m_currentData ["word"].ToString();
            
            int scoreDelta = isCorrect ? 1 : -1;
            m_scoreKeeper.UpdateScore(scoreDelta);
            
            if (isCorrect)
            {
                m_answerCharacters[answerIndex].spriteName = NGUIHelpers.GetLinkedSpriteName(m_answerCharacters[answerIndex].spriteName);
                CorrectWordCoordinator.Instance.OnCorrectAnswer(this);
            }
            else
            {
                WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
                
                Vector3 shakeAmount = Vector3.one * 0.2f;
                float shakeDuration = 0.25f;
                iTween.ShakePosition(m_questionPictureParent, shakeAmount, shakeDuration);
                iTween.ShakePosition(m_answerLabels[answerIndex].gameObject, shakeAmount, shakeDuration);
                
                m_answerLabels[answerIndex].color = Color.grey;
                
                relay.SingleClicked -= OnAnswer;
            }
        }
    }
    
    public IEnumerator ClearQuestion()
    {
        foreach (EventRelay relay in m_answerRelays)
        {
            relay.SingleClicked -= OnAnswer;
        }
        
        float tweenDuration = 0.3f;
        
        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");
        
        foreach (UILabel label in m_answerLabels)
        {
            iTween.ScaleTo(label.gameObject, Vector3.zero, tweenDuration);
        }
        
        Hashtable tweenArgs = new Hashtable();
        
        iTween.ScaleTo(m_questionPictureParent, Vector3.zero, tweenDuration);
        
        yield return new WaitForSeconds(tweenDuration + 0.5f);
        
        StartCoroutine(AskQuestion());
    }
    
    void OnScoreKeeperComplete(ScoreKeeper scoreKeeper)
    {
        CorrectWordCoordinator.Instance.CompleteGame();
    }
    
    void OnLevelUp(ScoreKeeper scoreKeeper)
    {
        CorrectWordCoordinator.Instance.OnLevelUp();
    }
}