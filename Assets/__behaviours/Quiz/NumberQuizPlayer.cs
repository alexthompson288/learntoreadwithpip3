using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class NumberQuizPlayer : PlusGamePlayer
{
    [SerializeField]
    private UILabel m_questionLabel;
    [SerializeField]
    private EventRelay[] m_answerRelays;
    [SerializeField]
    private UILabel[] m_answerLabels;
    [SerializeField]
    private UISprite[] m_answerCharacters;
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private GameObject m_locatorParent;
    
    DataRow m_currentData;
    NumberQuizCoordinator.Countable m_currentCountable;
    
    public void SetCurrentData(DataRow myCurrentData)
    {
        m_currentData = myCurrentData;
    }

    public void SetCurrentCountable(NumberQuizCoordinator.Countable myCurrentCountable)
    {
        m_currentCountable = myCurrentCountable;
    }
    
    public override void SelectCharacter(int characterIndex)
    {
        base.SelectCharacter (characterIndex);
        NumberQuizCoordinator.Instance.CharacterSelected(characterIndex);
    }
    
    void Start()
    {
        Array.Sort(m_answerLabels, CollectionHelpers.LocalLeftToRight);
        Array.Sort(m_answerRelays, CollectionHelpers.LocalLeftToRight);
        Array.Sort(m_answerCharacters, CollectionHelpers.LocalLeftToRight);
        
        foreach (UILabel label in m_answerLabels)
        {
            label.transform.localScale = Vector3.zero;
        }
    }
    
    public void StartGame()
    {       
        m_scoreKeeper.SetHealthLostOnIncorrect(10);
        m_scoreKeeper.LevelledUp += OnLevelUp;
        m_scoreKeeper.Completed += OnScoreKeeperComplete;
        m_scoreKeeper.StartTimer();

        m_questionLabel.text = "";
        
        StartCoroutine(AskQuestion());
    }
    
    IEnumerator AskQuestion()
    {   
        float tweenDuration = 0.3f;
        GameObject countablePrefab = NumberQuizCoordinator.Instance.GetCountablePrefab();

        CollectionHelpers.Shuffle(m_locators);

        int target = m_currentData.GetInt("value");

        if (NumberQuizCoordinator.Instance.UseDummyCountables())
        {
            m_questionLabel.text = string.Format("How many {0}?", m_currentCountable.name);

            int numCountables = NumberQuizCoordinator.Instance.GetRandomData(false).GetInt("value");
            while(numCountables < target)
            {
                numCountables = NumberQuizCoordinator.Instance.GetRandomData(false).GetInt("value");
            }

            for (int i = 0; i < numCountables && i < m_locators.Length; ++i)
            {
                GameObject newCountable = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(countablePrefab, m_locators [i]);
                iTween.ScaleFrom(newCountable, Vector3.zero, tweenDuration);
                
                UISprite sprite = newCountable.GetComponent<UISprite>() as UISprite;

                sprite.spriteName = m_currentCountable.spriteName;

                if(i >= target)
                {
                    while(sprite.spriteName == m_currentCountable.spriteName)
                    {
                        sprite.spriteName = NumberQuizCoordinator.Instance.GetRandomCountableSpriteName();
                    }
                }
            }
        } 
        else
        {
            for (int i = 0; i < target && i < m_locators.Length; ++i)
            {
                GameObject newCountable = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(countablePrefab, m_locators [i]);
                iTween.ScaleFrom(newCountable, Vector3.zero, tweenDuration);

                UISprite sprite = newCountable.GetComponent<UISprite>() as UISprite;
                sprite.spriteName = m_currentCountable.spriteName;
            }
        }
        
        HashSet<int> answers = new HashSet<int>();

        answers.Add(target);

        while (answers.Count < m_answerLabels.Length)
        {
            answers.Add(NumberQuizCoordinator.Instance.GetRandomData(false).GetInt("value"));
        }

        // Convert to array because cannot shuffle a hashset
        int[] answerArray = answers.ToArray();
        CollectionHelpers.Shuffle(answerArray);

        for(int i = 0; i < answerArray.Length; ++i)
        {
            m_answerLabels[i].color = Color.white;
            m_answerLabels[i].text = answerArray[i].ToString();
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
            bool isCorrect = m_answerLabels[answerIndex].text == m_currentData.GetInt("value").ToString();
            
            int scoreDelta = isCorrect ? 1 : -1;
            m_scoreKeeper.UpdateScore(scoreDelta);
            
            if (isCorrect)
            {
                m_answerCharacters[answerIndex].spriteName = NGUIHelpers.GetLinkedSpriteName(m_answerCharacters[answerIndex].spriteName);
                NumberQuizCoordinator.Instance.OnCorrectAnswer(this);
            }
            else
            {
                WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
                
                Vector3 shakeAmount = Vector3.one * 0.2f;
                float shakeDuration = 0.25f;
                iTween.ShakePosition(m_locatorParent, shakeAmount, shakeDuration);
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

        foreach (Transform locator in m_locators)
        {
            for(int i = 0; i < locator.childCount; ++i)
            {
                iTween.ScaleTo(locator.GetChild(i).gameObject, Vector3.zero, tweenDuration);
            }
        }
        
        yield return new WaitForSeconds(tweenDuration + 0.5f);

        foreach (Transform locator in m_locators)
        {
            int childCount = locator.childCount;
            for(int i = childCount - 1; i > -1; --i)
            {
                Destroy(locator.GetChild(i).gameObject);
            }
        }
        
        StartCoroutine(AskQuestion());
    }
    
    void OnScoreKeeperComplete(ScoreKeeper scoreKeeper)
    {
        NumberQuizCoordinator.Instance.CompleteGame();
    }
    
    void OnLevelUp(ScoreKeeper scoreKeeper)
    {
        NumberQuizCoordinator.Instance.OnLevelUp();
    }

    public int GetLocatorCount()
    {
        return m_locators.Length;
    }
}
