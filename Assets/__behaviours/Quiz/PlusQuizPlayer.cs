﻿using UnityEngine;
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
    
    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
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

/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

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
    private GameObject m_answerLabelPrefab;
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private GameObject m_questionPictureShakable;
    [SerializeField]
    private GameObject m_questionLabelShakable;

    List<GameWidget> m_spawnedAnswers = new List<GameWidget>();

	DataRow m_currentData;

    Vector3 m_pictureOnPos;
    Vector3 m_pictureBeforePos;
    
    Vector3 m_labelOnPos;
    Vector3 m_labelBeforePos;
    
    float m_widgetOffDistance = 2000;
    	
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
		PlusQuizCoordinator.Instance.CharacterSelected(characterIndex);
	}

    IEnumerator Start()
    {
        yield return null; // Wait a frame so that ScreenSetUp Start method executes first

        m_pictureOnPos = m_questionPictureParent.transform.localPosition;
        m_pictureBeforePos = new Vector3(m_pictureOnPos.x - m_widgetOffDistance, m_pictureOnPos.y, m_pictureOnPos.z);
        m_questionPictureParent.transform.localPosition = m_pictureBeforePos;
        
        m_labelOnPos = m_questionLabelParent.transform.localPosition;
        m_labelBeforePos = new Vector3(m_labelOnPos.x - m_widgetOffDistance, m_labelOnPos.y, m_labelOnPos.z);
        m_questionLabelParent.transform.localPosition = m_labelBeforePos;
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
        m_questionPictureParent.transform.localPosition = m_pictureBeforePos;
        m_questionLabelParent.transform.localPosition = m_labelBeforePos;

        if (m_currentData ["question"] != null)
        {
            m_questionLabel.text = m_currentData["question"].ToString();
            NGUIHelpers.MaxLabelWidth(m_questionLabel, 1600);
        }
        
        Texture2D tex = DataHelpers.GetPicture(m_currentData);
        m_questionPicture.mainTexture = tex;
        m_questionPicture.gameObject.SetActive(tex != null);
        
        
        float tweenDuration = 0.5f;

        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("position", m_pictureOnPos);
        tweenArgs.Add("time", tweenDuration);
        tweenArgs.Add("islocal", true);
        
        //iTween.MoveTo(m_questionPictureParent, m_pictureOnPos, tweenDuration);
        iTween.MoveTo(m_questionPictureParent, tweenArgs);
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
        
        yield return new WaitForSeconds(tweenDuration / 2);

        tweenArgs ["position"] = m_labelOnPos;
        
        //iTween.MoveTo(m_questionLabelParent, m_labelOnPos, tweenDuration);
        iTween.MoveTo(m_questionLabelParent, tweenArgs);
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
        
        yield return new WaitForSeconds(tweenDuration / 2);
        
        
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
        
        for(int i = 0; i < answerStrings.Count && i < m_locators.Length; ++i)
        {
            GameObject newAnswer = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_answerLabelPrefab, m_locators[i]);
            GameWidget answerBehaviour = newAnswer.GetComponent<GameWidget>() as GameWidget;
            answerBehaviour.SetUp(answerStrings[i], true);
            answerBehaviour.Unpressed += OnAnswer;
            answerBehaviour.EnableDrag(false);
            m_spawnedAnswers.Add(answerBehaviour);
        }
	}

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            foreach(GameWidget widget in m_spawnedAnswers)
            {
                if(widget.labelText == m_currentData ["correctanswer"].ToString())
                {
                    OnAnswer(widget);
                    break;
                }
            }
        }
    }
#endif

    void OnAnswer(GameWidget widget)
    {
        bool isCorrect = (widget == null || widget.labelText == m_currentData ["correctanswer"].ToString());
        
        int scoreDelta = isCorrect ? 1 : -1;
        m_scoreKeeper.UpdateScore(scoreDelta);
        
        if (isCorrect)
        {
            PlusQuizCoordinator.Instance.OnCorrectAnswer(this);
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            Vector3 shakeAmount = Vector3.one * 0.2f;
            float shakeDuration = 0.25f;
            iTween.ShakePosition(m_questionLabelShakable, shakeAmount, shakeDuration);
            iTween.ShakePosition(m_questionPictureShakable, shakeAmount, shakeDuration);
            widget.Shake();
            widget.TintGray();
            widget.EnableCollider(false);
        }
    }
	
	public IEnumerator ClearQuestion()
	{
        for (int i = m_spawnedAnswers.Count - 1; i > -1; --i)
        {
            m_spawnedAnswers [i].Off();
        }
        
        m_spawnedAnswers.Clear();
        
        float tweenDuration = 0.5f;

        Hashtable tweenArgs = new Hashtable();

        tweenArgs.Add("position", new Vector3(m_pictureOnPos.x + m_widgetOffDistance, m_pictureOnPos.y, m_pictureOnPos.z));
        tweenArgs.Add("time", tweenDuration);
        tweenArgs.Add("islocal", true);
        
        //Vector3 pictureOffPos = new Vector3(m_pictureOnPos.x + m_widgetOffDistance, m_pictureOnPos.y, m_pictureOnPos.z);
        //iTween.MoveTo(m_questionPictureParent, pictureOffPos, tweenDuration);
        iTween.MoveTo(m_questionPictureParent, tweenArgs);
        
        //Vector3 labelOffPos = new Vector3(m_labelOnPos.x + m_widgetOffDistance, m_labelOnPos.y, m_labelOnPos.z);
        //iTween.MoveTo(m_questionLabelParent, labelOffPos, tweenDuration);
        tweenArgs["position"] = new Vector3(m_labelOnPos.x + m_widgetOffDistance, m_labelOnPos.y, m_labelOnPos.z);
        iTween.MoveTo(m_questionLabelParent, tweenArgs);

        yield return new WaitForSeconds(tweenDuration + 0.1f);
		
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
*/
