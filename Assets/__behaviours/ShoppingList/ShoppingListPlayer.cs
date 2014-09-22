using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Wingrove;

public class ShoppingListPlayer : PlusGamePlayer
{
    [SerializeField]
    private ListItem[] m_listItems;
    [SerializeField]
    private Transform[] m_locators;

    class ListItem
    {
        public TriggerTracker m_tracker;
        public UILabel m_label;
        public UITexture m_texture;
    }

    HashSet<DataRow> m_currentData = new HashSet<DataRow>();

    List<GameWidget> m_spawnedDraggables = new List<GameWidget>();

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

        CollectionHelpers.Shuffle(m_listItems);

        int questionIndex = 0;
        foreach (DataRow question in m_currentData)
        {
            m_listItems[questionIndex].m_label.text = question["word"].ToString();
            //m_listItems[questionIndex].m_texture = DataHelpers.GetPicture(question);
            ++questionIndex;
        }

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
            widget.SetUp(answer);
            widget.Unpressed += OnWidgetRelease;

            ++questionIndex;
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

                m_scoreKeeper.UpdateScore(1);

                if(m_numCorrectAnswers >= m_listItems.Length)
                {
                    ShoppingListCoordinator.Instance.OnCompleteSet(this);
                }
            }
            else
            {
                WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
                m_scoreKeeper.UpdateScore(-1);
            }
        }
        else
        {
            widget.TweenToStartPos();
        }
    }
    
    public IEnumerator ClearQuestion()
    {


        yield return null;
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
