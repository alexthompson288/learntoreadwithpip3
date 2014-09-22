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

        string dummy1 = m_currentData ["dummyword1"].ToString();
        if (!string.IsNullOrEmpty(dummy1))
        {
            answerStrings.Add(dummy1);
        }

        string dummy2 = m_currentData ["dummyword2"].ToString();
        if (!string.IsNullOrEmpty(dummy2))
        {
            answerStrings.Add(dummy2);
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
/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorrectWordPlayer : PlusGamePlayer 
{
    [SerializeField]
    private GameObject m_questionParent;
    [SerializeField]
    private UITexture m_questionImage;
    [SerializeField]
    private GameObject m_questionImageParent;
    [SerializeField]
    private GameObject[] m_questionLabels;
    [SerializeField]
    private ClickEvent m_yesButton;
    [SerializeField]
    private ClickEvent m_noButton;
    [SerializeField]
    private Transform m_questionLabelLocationOn;
    [SerializeField]
    private Transform m_questionLabelLocationLeftOff;
    [SerializeField]
    private Transform m_questionLabelLocationRightOff;

    List<GameObject> m_spawnedQuestionText = new List<GameObject>();

    DataRow m_currentData = null;

    bool m_hasAnswered = false;
    
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
        base.SelectCharacter(characterIndex);

        CorrectWordCoordinator.Instance.CharacterSelected(characterIndex);
    }

    public void StartGame()
    {
        m_yesButton.SingleClicked += OnAnswer;
        m_noButton.SingleClicked += OnAnswer;
        
        m_scoreKeeper.SetHealthLostPerSecond(1f);
        
        m_scoreKeeper.LevelledUp += OnLevelUp;
        m_scoreKeeper.Completed += OnScoreKeeperComplete;
        m_scoreKeeper.StartTimer();
        
        m_questionParent.GetComponent<TweenOnOffBehaviour>().On();
        
        AskQuestion();    
    }

    void AskQuestion()
    {
        m_questionLabels[0].transform.localPosition = m_questionLabelLocationOn.localPosition;
        m_questionLabels[1].transform.localPosition = m_questionLabelLocationLeftOff.localPosition;

        Texture2D tex = DataHelpers.GetPicture(m_currentData);
        m_questionImage.mainTexture = tex;

        string word = m_currentData ["word"].ToString();

        UILabel label1 = m_questionLabels [1].GetComponentInChildren<UILabel>() as UILabel;
        label1.GetComponentInChildren<UILabel>().text = word;
        m_questionLabels[1].GetComponentInChildren<ShowPipPadForWord>().SetUp(word, NGUIHelpers.GetLabelSize3(label1), true);

        Color[] colors = new Color[2];

        if (Random.Range(0f, 1f) > CorrectWordCoordinator.Instance.GetProbabilityYesIsCorrect())
        {
            string dummy1 = CorrectWordCoordinator.Instance.GetDummyAttribute1();
            string dummy2 = CorrectWordCoordinator.Instance.GetDummyAttribute2();

            string dummyAttribute = Random.Range(0, 2) == 0 ? dummy1 : dummy2;
            if (m_currentData [dummyAttribute] == null)
            {
                dummyAttribute = dummyAttribute == dummy1 ? dummy2 : dummy1;
            }

            word = m_currentData [dummyAttribute].ToString();

            colors [0] = CorrectWordCoordinator.Instance.GetIncorrectColor();
            colors [1] = CorrectWordCoordinator.Instance.GetCorrectColor();
        } 
        else
        {
            colors [0] = CorrectWordCoordinator.Instance.GetCorrectColor();
            colors [1] = CorrectWordCoordinator.Instance.GetIncorrectColor();
        }

        for (int i = 0; i < m_questionLabels.Length && i < colors.Length; ++i)
        {
            UISprite highlight = m_questionLabels[i].GetComponentInChildren<UISprite>() as UISprite;
            highlight.color = colors[i];
            highlight.enabled = false;
        }
            
        UILabel label0 = m_questionLabels [0].GetComponentInChildren<UILabel>() as UILabel;
        label0.text = word;
        m_questionLabels[0].GetComponentInChildren<ShowPipPadForWord>().SetUp(word, NGUIHelpers.GetLabelSize3(label0), true);

        TweenQuestionParents(Vector3.one);
    }

    void OnAnswer(ClickEvent click)
    {
        if (!m_hasAnswered)
        {
            m_hasAnswered = true;
            StartCoroutine(OnAnswerCo(click));
        }
   }

    IEnumerator OnAnswerCo(ClickEvent click)
    {
        for (int i = 0; i < m_questionLabels.Length; ++i)
        {
            m_questionLabels[i].GetComponentInChildren<UISprite>().enabled = true;
        }

        bool answerIsYes = click.GetInt() == 1; 
        bool yesIsCorrect = m_questionLabels[0].GetComponentInChildren<UILabel>().text == m_currentData ["word"].ToString(); // The current attribute is the last one in m_remainingAttributes
        bool isCorrect = yesIsCorrect == answerIsYes;
        
        if (isCorrect)
        {
            m_scoreKeeper.UpdateScore(1);
        } 
        else
        {
            m_scoreKeeper.UpdateScore(-1);
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");

            float shakeDuration = 0.3f;
            iTween.ShakePosition(m_questionParent, Vector3.one * 0.2f, shakeDuration);
            yield return new WaitForSeconds(shakeDuration);
        }

        if (!yesIsCorrect)
        {
            float tweenDuration = CorrectWordCoordinator.Instance.GetQuestionTweenDuration();
            iTween.MoveTo(m_questionLabels[1], m_questionLabelLocationOn.position, tweenDuration);
            yield return new WaitForSeconds(tweenDuration / 3);
            iTween.MoveTo(m_questionLabels[0], m_questionLabelLocationRightOff.position, tweenDuration);
        }

        float delay = 1f;

        if (isCorrect && !answerIsYes)
        {
            delay = 1.5f;
        }
        else if(!isCorrect)
        {
            delay = 2f;
        }

        yield return new WaitForSeconds(delay);
        
        CorrectWordCoordinator.Instance.OnAnswer(this);
    }
    
    void TweenQuestionParents(Vector3 newScale)
    {
        iTween.ScaleTo(m_questionImageParent, newScale, CorrectWordCoordinator.Instance.GetQuestionTweenDuration());

        string audioString = Mathf.Approximately(newScale.x, 0) ? "SOMETHING_APPEAR" : "SOMETHING_DISAPPEAR";
        WingroveAudio.WingroveRoot.Instance.PostEvent(audioString);
    }
    
    public IEnumerator ClearQuestion()
    {
        m_hasAnswered = false;

        TweenQuestionParents(Vector3.zero);
        
        yield return new WaitForSeconds(CorrectWordCoordinator.Instance.GetQuestionTweenDuration());
        
        AskQuestion();
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
*/
