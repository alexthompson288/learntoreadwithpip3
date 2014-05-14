using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System.Linq;

public class MakeSentenceCoordinator : GameCoordinator 
{
    [SerializeField]
    private List<Transform> m_locators;
    [SerializeField]
    private GameObject m_wordPrefab;
    [SerializeField]
    private GameObject m_emptyPrefab;
    [SerializeField]
    private Transform m_textPosition;
    [SerializeField]
    private UITexture m_sentenceImage;

    List<GameWidget> m_spawnedWords = new List<GameWidget>();
    List<Transform> m_spawnedWordLocations = new List<Transform>();

    List<string> m_remainingWords = new List<string>();

    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetSentences();

        m_dataPool.RemoveAll(x => DataHelpers.GetSentenceWords(x).Length > 5);

        ClampTargetScore();

        Debug.Log("dataPool.Count: " + m_dataPool.Count);

        if (m_dataPool.Count > 0)
        {
            AskQuestion();
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
    }

    void AskQuestion()
    {
        m_currentData = GetRandomData();
        m_dataPool.Remove(m_currentData);

        m_remainingWords = DataHelpers.GetSentenceWords(m_currentData).ToList();

        List<Transform> locators = new List<Transform>();
        locators.AddRange(m_locators);
        CollectionHelpers.Shuffle(locators);

        float length = 0;
        float height = 0;
        //float maxWidth = 0;

        for (int i = 0; i < m_remainingWords.Count && i < locators.Count; ++i)
        {
            GameObject newWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordPrefab, locators[i]);

            // Add component so pressing word calls PipPad
            newWord.AddComponent<ShowPipPadForWord>();

            // Set up GameWidget
            GameWidget wordBehaviour = newWord.GetComponent<GameWidget>() as GameWidget;
            wordBehaviour.SetUp(m_remainingWords[i], true);
            wordBehaviour.onAll += OnWordClick;
            m_spawnedWords.Add(wordBehaviour);

            // Position wordLocation
            GameObject newWordLocation = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_textPosition);
            newWordLocation.transform.localPosition = new Vector3(length, height, 0);
            length += wordBehaviour.backgroundWidth;
            //maxWidth = Mathf.Max(maxWidth, length);

            SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordPrefab, newWordLocation.transform);
        }

        m_textPosition.localPosition = new Vector3(-length / 2, m_textPosition.localPosition.y, m_textPosition.localPosition.z);
    }

    void OnWordClick(GameWidget widget)
    {
        if (widget.labelText == m_remainingWords [0])
        {

        } 
        else
        {
            widget.TweenToStartPos();
        }
    }

    protected override IEnumerator CompleteGame()
    {
        yield return new WaitForSeconds(1f);
        //GameManager.Instance.CompleteGame();
    }
}
