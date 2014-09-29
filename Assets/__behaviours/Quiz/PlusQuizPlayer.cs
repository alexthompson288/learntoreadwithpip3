using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System;

public class PlusQuizPlayer : PlusGamePlayer 
{
    [SerializeField]
    private GameObject m_questionPictureParent;
    [SerializeField]
    private UITexture m_questionPicture;
    [SerializeField]
    private GameObject m_questionLabelParent;
    [SerializeField]
    private UILabel m_questionLabel;
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
    
    public override void SelectCharacter(int characterIndex)
    {
        base.SelectCharacter (characterIndex);
        PlusQuizCoordinator.Instance.CharacterSelected(characterIndex);
    }
    
    void Start()
    {
        Array.Sort(m_answerLabels, CollectionHelpers.LocalLeftToRight);
        Array.Sort(m_answerRelays, CollectionHelpers.LocalLeftToRight);
        Array.Sort(m_answerCharacters, CollectionHelpers.LocalLeftToRight);

        m_questionLabelParent.transform.localScale = Vector3.zero;
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
        if (m_currentData ["question"] != null)
        {
            m_questionLabel.text = m_currentData["question"].ToString();
            NGUIHelpers.MaxLabelWidth(m_questionLabel, 1600);
        }
        
        Texture2D tex = DataHelpers.GetPicture(m_currentData);
        m_questionPicture.mainTexture = tex;
        m_questionPicture.gameObject.SetActive(tex != null);
        
        float tweenDuration = 0.3f;
        
        iTween.ScaleTo(m_questionPictureParent, Vector3.one, tweenDuration);
        
        //yield return new WaitForSeconds(tweenDuration / 2);

        iTween.ScaleTo(m_questionLabelParent, Vector3.one, tweenDuration);
        
        
        List<string> answerStrings = new List<string>();
        
        if (m_currentData ["dummyanswer1"] != null)
        {
            answerStrings.Add(m_currentData ["dummyanswer1"].ToString());
        }
        
        if (m_currentData ["dummyanswer2"] != null)
        {
            answerStrings.Add(m_currentData ["dummyanswer2"].ToString());
        }
        
        answerStrings.Add(m_currentData ["correctanswer"].ToString());
        
        CollectionHelpers.Shuffle(answerStrings);
        
        for(int i = 0; i < answerStrings.Count && i < m_answerLabels.Length; ++i)
        {
            m_answerLabels[i].color = Color.white;
            m_answerLabels[i].text = answerStrings[i];
            iTween.ScaleTo(m_answerLabels[i].gameObject, Vector3.one, tweenDuration);
        }

        foreach (EventRelay relay in m_answerRelays)
        {
            relay.SingleClicked += OnAnswer;
        }

        foreach (UISprite sprite in m_answerCharacters)
        {
            sprite.spriteName = sprite.spriteName.Substring(0, sprite.spriteName.Length - 1) + "a";
        }

        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");

        yield break;
    }
    
    void OnAnswer(EventRelay relay)
    {
        int answerIndex = Array.IndexOf(m_answerRelays, relay);

        if(answerIndex != -1)
        {
            bool isCorrect = m_answerLabels[answerIndex].text == m_currentData ["correctanswer"].ToString();
            
            int scoreDelta = isCorrect ? 1 : -1;
            m_scoreKeeper.UpdateScore(scoreDelta);
            
            if (isCorrect)
            {
                m_answerCharacters[answerIndex].spriteName = NGUIHelpers.GetLinkedSpriteName(m_answerCharacters[answerIndex].spriteName);
                PlusQuizCoordinator.Instance.OnCorrectAnswer(this);
            }
            else
            {
                WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");

                Vector3 shakeAmount = Vector3.one * 0.2f;
                float shakeDuration = 0.25f;
                iTween.ShakePosition(m_questionLabelParent, shakeAmount, shakeDuration);
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
        
        iTween.ScaleTo(m_questionLabelParent, Vector3.zero, tweenDuration);
        iTween.ScaleTo(m_questionPictureParent, Vector3.zero, tweenDuration);
        
        yield return new WaitForSeconds(tweenDuration + 0.5f);
        
        StartCoroutine(AskQuestion());
    }
    
    void OnScoreKeeperComplete(ScoreKeeper scoreKeeper)
    {
        PlusQuizCoordinator.Instance.CompleteGame();
    }
    
    void OnLevelUp(ScoreKeeper scoreKeeper)
    {
        PlusQuizCoordinator.Instance.OnLevelUp();
    }
}