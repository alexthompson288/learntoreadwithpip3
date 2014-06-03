using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class NumberFillCoordinator : GameCoordinator
{
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private DataDisplay m_display;
    [SerializeField]
    private GameObject m_widgetPrefab;
    [SerializeField]
    private GameObject m_widgetHolderPrefab;
    [SerializeField]
    private Transform m_widgetHolderLocation;
    [SerializeField]
    private PipButton m_submitAnswerButton;

    GameWidgetHolder m_currentWidgetHolder = null;

    List<GameWidget> m_unheldWidgets = new List<GameWidget>();

    int m_maxValue = 0;

	// Use this for initialization
	IEnumerator Start () 
    {
        m_submitAnswerButton.Unpressing += OnSubmitAnswer;
        m_scoreKeeper.SetTargetScore(m_targetScore);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetNumbers();

        foreach (DataRow number in m_dataPool)
        {
            if(System.Convert.ToInt32(number["value"]) > m_maxValue)
            {
                m_maxValue = System.Convert.ToInt32(number["value"]);
            }
        }

        AskQuestion();
	}

    Transform FindEmptyLocator()
    {
        CollectionHelpers.Shuffle(m_locators);

        foreach (Transform locator in m_locators)
        {
            if(locator.childCount == 0)
            {
                return locator;
            }
        }

        return m_locators[0];
    }


    void AskQuestion()
    {
        m_currentData = GetRandomData();

        int totalNumSpawn = Random.Range(System.Convert.ToInt32(m_currentData["value"]), m_maxValue + 1);
        
        int numWidgetsAlreadyHeld = Random.Range(0, totalNumSpawn + 1);
        
        m_currentWidgetHolder = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_widgetHolderPrefab, m_widgetHolderLocation).GetComponent<GameWidgetHolder>() as GameWidgetHolder;

        List<GameWidget> heldWidgets = m_currentWidgetHolder.SpawnWidgets(numWidgetsAlreadyHeld);
        foreach (GameWidget widget in heldWidgets)
        {
            widget.SetUpBackground();
            widget.onAll += OnWidgetInteract;
        }
        
        CollectionHelpers.Shuffle(m_locators);
        
        for(int i = 0; i < totalNumSpawn - numWidgetsAlreadyHeld; ++i)
        {
            GameObject newGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_widgetPrefab, m_locators[i]);
            
            GameWidget widget = newGo.GetComponent<GameWidget>() as GameWidget;
            widget.SetUpBackground();
            widget.onAll += OnWidgetInteract;
            m_unheldWidgets.Add(widget);
        }

        iTween.ScaleFrom(m_currentWidgetHolder.gameObject, Vector3.zero, 0.3f);

        m_display.On("numbers", m_currentData);
    }

    void OnWidgetInteract(GameWidget widget)
    {
        if (m_unheldWidgets.Contains(widget))
        {
            m_currentWidgetHolder.AddWidget(widget);
            m_unheldWidgets.Remove(widget);
        } 
        else
        {
            m_currentWidgetHolder.RemoveWidget(widget);
            
            Transform emptyLocator = FindEmptyLocator();
            
            widget.transform.parent = emptyLocator;
            iTween.MoveTo(widget.gameObject, emptyLocator.position, 0.5f);
            
            m_unheldWidgets.Add(widget);
        }

        AudioClip clip = LoaderHelpers.LoadAudioForNumber(m_currentWidgetHolder.heldWidgetCount);
        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }

        Resources.UnloadUnusedAssets();
    }

    void OnSubmitAnswer(PipButton button)
    {
        if (m_currentWidgetHolder.heldWidgetCount == System.Convert.ToInt32(m_currentData ["value"]))
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");
            
            ++m_score;
            m_scoreKeeper.UpdateScore(1);
            
            StartCoroutine(ClearQuestion());
        } 
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            
            m_currentWidgetHolder.Shake();
        }
    }

    IEnumerator ClearQuestion()
    {
        m_display.Off();
        StartCoroutine(m_currentWidgetHolder.Off());

        CollectionHelpers.DestroyObjects(m_unheldWidgets, true);

        yield return new WaitForSeconds(0.5f);

        if (m_score < m_targetScore)
        {
            AskQuestion();
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
    }
}
