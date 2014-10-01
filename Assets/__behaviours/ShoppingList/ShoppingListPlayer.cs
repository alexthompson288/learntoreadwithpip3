using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Wingrove;

public class ShoppingListPlayer : PlusGamePlayer
{
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private TweenBehaviour m_shoppingList;
    [SerializeField]
    private GameObject m_shoppingListShakeable;
    [SerializeField]
    private ListItem[] m_listItems;

    [System.Serializable]
    class ListItem
    {
        public TriggerTracker m_tracker;
        public UILabel m_label;
    }

    HashSet<DataRow> m_currentData = new HashSet<DataRow>();

    int m_numCorrectAnswers = 0;


    public override void SelectCharacter(int characterIndex)
    {
        base.SelectCharacter (characterIndex);
        ShoppingListCoordinator.Instance.CharacterSelected(characterIndex);
    }

    public void SetCurrentData(HashSet<DataRow> myCurrentData)
    {
        m_currentData = myCurrentData;
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
        m_numCorrectAnswers = 0;

        // Questions
        CollectionHelpers.Shuffle(m_listItems);

        int questionIndex = 0;
        foreach (DataRow question in m_currentData)
        {
            m_listItems[questionIndex].m_label.text = question["word"].ToString();
            ++questionIndex;
        }

        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
        m_shoppingList.On();

        //  Answers;
        HashSet<DataRow> answers = new HashSet<DataRow>();

        foreach (DataRow data in m_currentData)
        {
            answers.Add(data);
        }

        int numAnswers = Mathf.Min(ShoppingListCoordinator.Instance.GetNumAnswers(), m_locators.Length);
        while (answers.Count < numAnswers)
        {
            answers.Add(ShoppingListCoordinator.Instance.GetRandomData());
        }

        CollectionHelpers.Shuffle(m_locators);

        GameObject answerPrefab = ShoppingListCoordinator.Instance.GetAnswerPrefab();
        int answerIndex = 0;
        foreach (DataRow answer in answers)
        {
            GameObject newAnswer = SpawningHelpers.InstantiateUnderWithIdentityTransforms(answerPrefab, m_locators[answerIndex]);

            GameWidget widget = newAnswer.GetComponent<GameWidget>() as GameWidget;
            widget.SetUp("words", answer, DataHelpers.GetPicture(answer), false);
            widget.Unpressed += OnWidgetRelease;

            ++answerIndex;
        }

        yield break;
    }
    
    void OnWidgetRelease(GameWidget widget)
    {
        ListItem[] trackingItems = Array.FindAll(m_listItems, x => x.m_tracker.IsTracking(widget.gameObject));

        if (trackingItems.Length > 0)
        {
            ListItem correctItem = Array.Find(trackingItems, x => x.m_label.text == widget.data["word"].ToString());

            if(correctItem != null)
            {
                ++m_numCorrectAnswers;

                WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");
                widget.TweenToPos(correctItem.m_tracker.transform.position);
                widget.EnableCollider(false);
                widget.transform.parent = m_shoppingList.GetMoveable().transform;

                m_scoreKeeper.UpdateScore(1);

                if(m_numCorrectAnswers >= m_listItems.Length)
                {
                    ShoppingListCoordinator.Instance.OnCompleteSet(this);
                }
            }
            else
            {
                WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");

                Hashtable tweenArgs = new Hashtable();
                tweenArgs.Add("amount", Vector3.one * 25f);
                tweenArgs.Add("islocal", true);
                tweenArgs.Add("time", 0.3f);

                iTween.ShakePosition(m_shoppingListShakeable, tweenArgs);

                m_scoreKeeper.UpdateScore(-1);
                widget.TweenToStartPos();
            }
        }
        else
        {
            widget.TweenToStartPos();
        }
    }
    
    public IEnumerator ClearQuestion()
    {
        GameWidget[] listWidgets = m_shoppingList.GetComponentsInChildren<GameWidget>() as GameWidget[];

        for (int i = 0; i < listWidgets.Length; ++i)
        {
            iTween.Stop(listWidgets[i].gameObject);
        }

        m_shoppingList.Off();
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");

        yield return new WaitForSeconds(m_shoppingList.GetTotalDurationOff());

        for(int i = listWidgets.Length - 1; i > -1; --i)
        {
            listWidgets[i].Off();
        }

        foreach (Transform locator in m_locators)
        {
            GameWidget[] unusedWidgets = locator.GetComponentsInChildren<GameWidget>() as GameWidget[];
            for(int i = unusedWidgets.Length - 1; i > -1; --i)
            {
                unusedWidgets[i].Off();
            }
        }

        yield return new WaitForSeconds(0.75f);

        StartCoroutine(AskQuestion());
    }
    
    void OnScoreKeeperComplete(ScoreKeeper scoreKeeper)
    {
        ShoppingListCoordinator.Instance.CompleteGame();
    }
    
    void OnLevelUp(ScoreKeeper scoreKeeper)
    {
        ShoppingListCoordinator.Instance.OnLevelUp();
    }
}
