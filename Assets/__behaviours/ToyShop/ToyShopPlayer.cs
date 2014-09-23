using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class ToyShopPlayer : GamePlayer 
{
    [SerializeField]
    private ScoreHealth m_scoreKeeper;
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private TrafficLights m_trafficLights;
    [SerializeField]
    private UnitStack[] m_coinStacks;
    [SerializeField]
    private EventRelay m_submitButton;
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
    [SerializeField]
    private UILabel m_currentStackLabel;

    int m_numSweetTypes = 8;

    int m_spriteDepthAdjust = 10;

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

        m_scoreKeeper.SetCharacterIcon(characterIndex);
        ToyShopCoordinator.Instance.CharacterSelected(characterIndex);
    }

    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
    }

    void Update()
    {
        int totalStackedValue = 0;
        foreach (UnitStack stack in m_coinStacks)
        {
            totalStackedValue += stack.GetStackedValue();
        }
  
        m_currentStackLabel.text = totalStackedValue.ToString();

        if (m_currentToy != null)
        {
            m_priceTag.position = m_currentToy.transform.position;
        }
    }

    public void StartGame(bool subscribeToTimer)
    {
        ////D.Log("ToyShopPlayer.StartGame()");
        if (m_coinStackTweenBehaviour != null)
        {
            m_coinStackTweenBehaviour.On();
        }

        m_submitButton.SingleClicked += OnClickSubmitButton;

        System.Array.Sort(m_coinStacks, CollectionHelpers.LocalLeftToRight);

        int[] coinValues = ToyShopCoordinator.Instance.GetCoinValues();

        for (int i = 0; i < m_coinStacks.Length && i < coinValues.Length; ++i)
        {
            m_coinStacks[i].SetValue(coinValues[i]);
        }

        m_scoreKeeper.SetHealthLostPerSecond(1f);

        m_scoreKeeper.StartTimer();
        m_scoreKeeper.LevelledUp += OnLevelUp;
        m_scoreKeeper.Completed += OnScoreKeeperComplete;

        SpawnToys();
    }

    void SpawnToys()
    {
        GameObject toyPrefab = ToyShopCoordinator.Instance.GetToyPrefab();
        int numToSpawn = Mathf.Min(ToyShopCoordinator.Instance.GetNumToysToSpawn(), m_locators.Length);

        for(int i = 0; i < numToSpawn; ++i)
        {
            /*
            GameObject newToy = SpawningHelpers.InstantiateUnderWithIdentityTransforms(toyPrefab, m_locators[i], true);
            GameWidget widgetBehaviour = newToy.GetComponent<GameWidget>() as GameWidget;
            widgetBehaviour.EnableDrag(false);
            widgetBehaviour.SetUpBackground();
            widgetBehaviour.Unpressing += OnToySelect;
            newToy.GetComponentInChildren<UISprite>().MakePixelPerfect();
            m_spawnedToys.Add(newToy);
            */
            SpawnToy(m_locators[i]);
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

        m_currentToy.GetComponentInChildren<UISprite>().depth -= m_spriteDepthAdjust;

        m_canChooseToy = true;

        m_currentToy = null;
    }
    
    void OnToySelect(GameWidget widget)
    {
        if (m_currentToy != null)
        {
            OnToyDeselect(m_currentToy.GetComponent<GameWidget>());
        }

        if (m_canChooseToy)
        {
            m_canChooseToy = false;

            widget.Unpressing -= OnToySelect;
            widget.Unpressing += OnToyDeselect;

            m_currentToy = widget.gameObject;

            m_currentToy.GetComponentInChildren<UISprite>().depth += m_spriteDepthAdjust;

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

            /*
            if (priceString.Length > 2)
            {
                priceString = priceString.Insert(priceString.Length - 2, ".");
                priceString = priceString.Insert(0, "£");
            }
            else
            {
                priceString = priceString + "p";
            }
            */

            m_priceLabel.text = priceString;
            iTween.ScaleTo(m_priceTag.gameObject, Vector3.one, tweenDuration);
            //m_priceTagTweenBehaviour.On();
            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
        }
    }

    void SpawnToy(Transform parent)
    {
        GameObject toyPrefab = ToyShopCoordinator.Instance.GetToyPrefab();

        GameObject newToy = SpawningHelpers.InstantiateUnderWithIdentityTransforms(toyPrefab, parent, true);
        GameWidget widgetBehaviour = newToy.GetComponent<GameWidget>() as GameWidget;
        widgetBehaviour.EnableDrag(false);
        widgetBehaviour.SetUpBackground();
        widgetBehaviour.Unpressing += OnToySelect;


        string spriteName = string.Format("sweet{0}", Random.Range(1, m_numSweetTypes + 1));

        while (m_spawnedToys.Find(x => x.GetComponentInChildren<UISprite>().spriteName == spriteName))
        {
            spriteName = string.Format("sweet{0}", Random.Range(1, m_numSweetTypes + 1));
        }

        newToy.GetComponentInChildren<UISprite>().spriteName = spriteName;

        newToy.GetComponentInChildren<UISprite>().MakePixelPerfect();

        m_spawnedToys.Add(newToy);
    }

    void OnClickSubmitButton(EventRelay relay)
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

            SpawnToy(m_currentToy.transform.parent);

            m_currentToy.transform.parent = m_shoppingTrolley;
            
            float tweenDuration = 0.25f;

            m_currentToy.GetComponent<GameWidget>().TweenToPos(m_shoppingTrolley.position);
            //iTween.ScaleTo(m_currentToy, Vector3.one * 0.5f, tweenDuration);
            iTween.ScaleTo(m_currentToy, Vector3.zero, tweenDuration);
            StartCoroutine(DestroyToy(tweenDuration + 0.1f, m_currentToy));

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
            m_scoreKeeper.UpdateScore(-1);
        }
    }

    IEnumerator DestroyToy(float delay, GameObject go)
    {
        yield return new WaitForSeconds(delay);
        Destroy(go);
    }

    public IEnumerator CelebrateVictory()
    {
        if (SessionInformation.Instance.GetNumPlayers() == 2)
        {
            yield return new WaitForSeconds(0.8f);
            CelebrationCoordinator.Instance.DisplayVictoryLabels(m_playerIndex);
            CelebrationCoordinator.Instance.PopCharacter(m_selectedCharacter, true);
            yield return new WaitForSeconds(2f);
        }

        yield return StartCoroutine(m_scoreKeeper.Celebrate());
    }

    void OnLevelUp(ScoreKeeper scoreKeeper)
    {
        ToyShopCoordinator.Instance.OnLevelUp();
    }

    void OnScoreKeeperComplete(ScoreKeeper scoreKeeper)
    {
        ////D.Log("ToyShopPlayer.OnTimerFinish()");
        ToyShopCoordinator.Instance.CompleteGame();
    }

    public int GetScore()
    {
        return m_scoreKeeper.GetScore();
    }
}