﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class ToyShopPlayer : GamePlayer 
{
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private Timer m_timer;
    [SerializeField]
    private TrafficLights m_trafficLights;
    [SerializeField]
    private UnitStack[] m_coinStacks;
    [SerializeField]
    private PipButton m_goButton;
    [SerializeField]
    private TweenOnOffBehaviour m_priceTagTweenBehaviour;
    [SerializeField]
    private Transform m_priceTag;
    [SerializeField]
    private UILabel m_priceLabel;
    [SerializeField]
    private Transform m_shoppingTrolley;
    [SerializeField]
    private Transform m_toyBuyLocation;
    [SerializeField]
    private TweenOnOffBehaviour m_coinStackTweenBehaviour;

    GameObject m_currentToy = null;

    List<GameObject> m_spawnedToys = new List<GameObject>();

    bool m_canChooseToy = true;

    int m_currentPrice;

    public override void SelectCharacter(int characterIndex)
    {
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;

        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }

        ToyShopCoordinator.Instance.CharacterSelected(characterIndex);
    }

    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
    }

    void Update()
    {
        if (m_currentToy != null)
        {
            m_priceTag.position = m_currentToy.transform.position;
        }
    }

    public void StartGame(bool subscribeToTimer)
    {
        D.Log("ToyShopPlayer.StartGame()");
        m_coinStackTweenBehaviour.On();

        m_goButton.Unpressing += OnUnpressGoButton;

        System.Array.Sort(m_coinStacks, CollectionHelpers.LocalLeftToRight);

        int[] coinValues = new int[] {1, 2, 5, 10, 20, 50, 100};

        for (int i = 0; i < m_coinStacks.Length && i < coinValues.Length; ++i)
        {
            m_coinStacks[i].SetValue(coinValues[i]);
        }

        m_timer.SetTimeRemaing(ToyShopCoordinator.Instance.GetTimeLimit());

        if (subscribeToTimer)
        {
            D.Log("Subscribing to timer");
            m_timer.Finished += OnTimerFinish;
        }

        m_timer.On();

        SpawnToys();
    }

    void SpawnToys()
    {
        GameObject toyPrefab = ToyShopCoordinator.Instance.GetToyPrefab();
        int numToSpawn = Mathf.Min(ToyShopCoordinator.Instance.GetNumToysToSpawn(), m_locators.Length);

        for(int i = 0; i < numToSpawn; ++i)
        {
            GameObject newToy = SpawningHelpers.InstantiateUnderWithIdentityTransforms(toyPrefab, m_locators[i], true);
            GameWidget widgetBehaviour = newToy.GetComponent<GameWidget>() as GameWidget;
            widgetBehaviour.EnableDrag(false);
            widgetBehaviour.SetUpBackground();
            widgetBehaviour.Unpressing += OnToySelect;
            m_spawnedToys.Add(newToy);
        }
    }

    void OnToyDeselect(GameWidget widget)
    {
        iTween.ScaleTo(m_priceTag.gameObject, Vector3.zero, 0.25f);

        iTween.ScaleTo(m_currentToy, Vector3.one, 0.25f);

        widget.TweenToPos(widget.transform.parent.position);

        widget.Unpressing -= OnToyDeselect;
        widget.Unpressing += OnToySelect;

        foreach (GameObject toy in m_spawnedToys)
        {
            toy.GetComponentInChildren<WobbleGUIElement>().enabled = true;
        }

        m_canChooseToy = true;

        m_currentToy = null;
    }
    
    void OnToySelect(GameWidget widget)
    {
        if (m_canChooseToy)
        {
            m_canChooseToy = false;

            widget.Unpressing -= OnToySelect;
            widget.Unpressing += OnToyDeselect;

            m_currentToy = widget.gameObject;

            m_spawnedToys.Remove(m_currentToy);

            foreach(GameObject toy in m_spawnedToys)
            {
                toy.GetComponentInChildren<WobbleGUIElement>().enabled = false;
            }

            float tweenDuration = 0.25f;
            iTween.ScaleTo(m_currentToy, Vector3.one * 2.5f, tweenDuration);
            iTween.MoveTo(m_currentToy, m_toyBuyLocation.position, tweenDuration);

            m_currentPrice = ToyShopCoordinator.Instance.GetRandomValue();
            string priceString = m_currentPrice.ToString();

            if (priceString.Length > 2)
            {
                priceString = priceString.Insert(priceString.Length - 2, ".");
                priceString = priceString.Insert(0, "£");
            }
            else
            {
                priceString = priceString + "p";
            }

            m_priceLabel.text = priceString;
            iTween.ScaleTo(m_priceTag.gameObject, Vector3.one, tweenDuration);
            //m_priceTagTweenBehaviour.On();
            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
        }
    }

    void OnUnpressGoButton(PipButton button)
    {
        int amountPaid = 0;
        foreach (UnitStack stack in m_coinStacks)
        {
            amountPaid += stack.GetStackedValue();
        }

        if (amountPaid == m_currentPrice)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_PAD_APPEAR");

            m_scoreKeeper.UpdateScore(1);

            foreach(GameObject toy in m_spawnedToys)
            {
                toy.GetComponentInChildren<WobbleGUIElement>().enabled = true;
            }

            Transform emptyLocator = m_currentToy.transform.parent;
            GameObject toyPrefab = ToyShopCoordinator.Instance.GetToyPrefab();

            GameObject newToy = SpawningHelpers.InstantiateUnderWithIdentityTransforms(toyPrefab, emptyLocator, true);
            GameWidget widgetBehaviour = newToy.GetComponent<GameWidget>() as GameWidget;
            widgetBehaviour.EnableDrag(false);
            widgetBehaviour.SetUpBackground();
            widgetBehaviour.Unpressing += OnToySelect;
            m_spawnedToys.Add(newToy);

            m_currentToy.transform.parent = m_shoppingTrolley;
            
            float tweenDuration = 0.25f;

            m_currentToy.GetComponent<GameWidget>().TweenToPos(m_shoppingTrolley.position);
            iTween.ScaleTo(m_currentToy, Vector3.one * 0.5f, tweenDuration);

            //m_priceTagTweenBehaviour.Off();
            iTween.ScaleTo(m_priceTag.gameObject, Vector3.zero, 0.25f);
            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");

            if(m_spawnedToys.Count == 0)
            {
                SpawnToys();
            }

            foreach(UnitStack stack in m_coinStacks)
            {
                stack.ClearStack();
            }

            m_canChooseToy = true;
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            //m_currentToy.GetComponent<GameWidget>().TweenToStartPos();
        }
    }

    public IEnumerator CelebrateVictory()
    {
        yield return StartCoroutine(m_scoreKeeper.On());
    }

    void OnTimerFinish(Timer timer)
    {
        D.Log("ToyShopPlayer.OnTimerFinish()");
        ToyShopCoordinator.Instance.CompleteGame();
    }

    public int GetScore()
    {
        return m_scoreKeeper.GetScore();
    }
}