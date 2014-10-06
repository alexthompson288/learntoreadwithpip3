using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CountingPlayer : PlusGamePlayer
{
    [SerializeField]
    private UILabel m_questionLabel;
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private EventRelay m_enterButton;

    List<Countable> m_spawnedCountables = new List<Countable>();
    
    DataRow m_currentData;
    CountingCoordinator.Countable m_currentCountableInfo;
    
    public void SetCurrentData(DataRow myCurrentData)
    {
        m_currentData = myCurrentData;
    }
    
    public void SetCurrentCountableInfo(CountingCoordinator.Countable myCurrentCountable)
    {
        m_currentCountableInfo = myCurrentCountable;
    }
    
    public override void SelectCharacter(int characterIndex)
    {
        base.SelectCharacter (characterIndex);
        CountingCoordinator.Instance.CharacterSelected(characterIndex);
    }

    void Awake()
    {
        m_questionLabel.alpha = 0;
    }
    
    public void StartGame()
    {     
        m_enterButton.SingleClicked += OnAnswer;

        m_scoreKeeper.LevelledUp += OnLevelUp;
        m_scoreKeeper.Completed += OnScoreKeeperComplete;
        m_scoreKeeper.StartTimer();
        
        m_questionLabel.text = "";
        
        StartCoroutine(AskQuestion());
    }
    
    IEnumerator AskQuestion()
    {   
        m_questionLabel.text = string.Format("Pick {0} {1}", m_currentData.GetInt("value"), m_currentCountableInfo.name);
        TweenAlpha.Begin(m_questionLabel.gameObject, 0.25f, 1f);

        GameObject countablePrefab = CountingCoordinator.Instance.GetCountablePrefab();

        int target = m_currentData.GetInt("value");
        int numCountables = CountingCoordinator.Instance.GetRandomData(false).GetInt("value");
        while (numCountables < target)
        {
            numCountables = CountingCoordinator.Instance.GetRandomData(false).GetInt("value");
        }

        CollectionHelpers.Shuffle(m_locators);

        for(int i = 0; i < numCountables; ++i)
        {
            GameObject newCountable = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(countablePrefab, m_locators[i]);
            Countable countableBehaviour = newCountable.GetComponent<Countable>() as Countable;
            countableBehaviour.SetUp(m_currentCountableInfo.spriteName);
            m_spawnedCountables.Add(countableBehaviour);
        }
        
        yield break;
    }
    
    void OnAnswer(EventRelay relay)
    {
        bool isCorrect = m_spawnedCountables.FindAll(x => x.isSelected).Count == m_currentData.GetInt("value");

        int scoreDelta = isCorrect ? 1 : -1;
        m_scoreKeeper.UpdateScore(scoreDelta);
        
        if (isCorrect)
        {
            CountingCoordinator.Instance.OnCorrectAnswer(this);
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");

            Hashtable tweenArgs = new Hashtable();
            tweenArgs.Add("islocal", true);
            tweenArgs.Add("amount", Vector3.one * 20);
            tweenArgs.Add("time", 0.5f);

            foreach(Countable countable in m_spawnedCountables)
            {
                iTween.ShakePosition(countable.gameObject, tweenArgs);
            }
        }
    }
    
    public IEnumerator ClearQuestion()
    {
        CollectionHelpers.DestroyObjects(m_spawnedCountables, true);
        TweenAlpha.Begin(m_questionLabel.gameObject, 0.25f, 0f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEAR");

        yield return new WaitForSeconds(0.4f);
        
        StartCoroutine(AskQuestion());
    }
    
    void OnScoreKeeperComplete(ScoreKeeper scoreKeeper)
    {
        CountingCoordinator.Instance.CompleteGame();
    }
    
    void OnLevelUp(ScoreKeeper scoreKeeper)
    {
        CountingCoordinator.Instance.OnLevelUp();
    }
    
    public int GetLocatorCount()
    {
        return m_locators.Length;
    }
}
