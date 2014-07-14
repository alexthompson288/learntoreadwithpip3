using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class NumberSequenceCoordinator : GameCoordinator
{
    [SerializeField]
    private GameObject m_numberPrefab;
    [SerializeField]
    private Transform[] m_answerLocators;
    [SerializeField]
    private int m_numAnswers;
    [SerializeField]
    private GameObject m_train;
    [SerializeField]
    private Transform m_trainLeftOffLocation;
    [SerializeField]
    private Transform m_trainRightOffLocation;
    [SerializeField]
    private UISprite m_missingNumberMarker;
    [SerializeField]
    private Transform m_steamSpawnParent;
    [SerializeField]
    private Transform m_steamParent;
    [SerializeField]
    private GameObject m_steamPrefab;
    
    int m_highestNumber = 0;
    
    UILabel m_currentLabel;
    
    List<GameWidget> m_spawnedAnswers = new List<GameWidget>();
    
    bool m_hasAnsweredIncorrectly = false;

    UILabel[] m_labels;

    Vector3 m_trainOnPos;

    IEnumerator SpawnSteam()
    {
        GameObject newSteam = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_steamPrefab, m_steamSpawnParent, true);
        newSteam.transform.parent = m_steamParent;
        StartCoroutine(newSteam.GetComponent<TrainSteam>().On());
        yield return new WaitForSeconds(1f);
        StartCoroutine("SpawnSteam");
    }

    IEnumerator Start()
    {
        m_trainOnPos = m_train.transform.position;
        m_train.transform.position = m_trainLeftOffLocation.position;

        GameObject[] labelGos = GameObject.FindGameObjectsWithTag("Label");
        m_labels = new UILabel[labelGos.Length];
        for (int i = 0; i < labelGos.Length; ++i)
        {
            m_labels[i] = labelGos[i].GetComponent<UILabel>() as UILabel;
        }

        System.Array.Sort(m_labels, CollectionHelpers.LeftToRight);

        m_scoreKeeper.SetTargetScore(m_targetScore);
        
        m_numAnswers = Mathf.Min(m_numAnswers, m_answerLocators.Length);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        
        m_dataPool = DataHelpers.GetNumbers();

        if (m_dataPool.Count > 0)
        {
            yield return StartCoroutine(TransitionScreen.WaitForScreenExit());
            StartCoroutine(AskQuestion());
        }
        else
        {
            StartCoroutine(CompleteGame());
        }
    }
    
    IEnumerator AskQuestion()
    {
        int index = Random.Range(0, m_dataPool.Count - m_labels.Length);
        
        List<DataRow> numberSequence = new List<DataRow>();
        
        while (numberSequence.Count < m_labels.Length)
        {
            numberSequence.Add(m_dataPool[index]);
            ++index;
        }
        
        for (int i = 0; i < numberSequence.Count && i < m_labels.Length; ++i)
        {
            m_labels[i].gameObject.SetActive(true);
            m_labels[i].text = numberSequence[i]["value"].ToString();
        }

        int currentNumberIndex = Random.Range(0, m_labels.Length);

        m_currentData = numberSequence [currentNumberIndex];

        m_currentLabel = m_labels [currentNumberIndex];
        m_currentLabel.gameObject.SetActive(false);

        m_missingNumberMarker.alpha = 1;
        m_missingNumberMarker.transform.position = m_currentLabel.transform.position;

        HashSet<DataRow> answers = new HashSet<DataRow>();
        answers.Add(m_currentData);
        
        while (answers.Count < m_numAnswers)
        {
            answers.Add(GetRandomData());
        }
        
        CollectionHelpers.Shuffle(m_answerLocators);
        
        int locatorIndex = 0;
        foreach (DataRow answer in answers)
        {
            GameObject newAnswer = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_numberPrefab, m_answerLocators[locatorIndex]);
            
            GameWidget widget = newAnswer.GetComponent<GameWidget>() as GameWidget;
            
            widget.SetUp(answer);
            widget.Unpressing += OnAnswer;
            m_spawnedAnswers.Add(widget);
            
            ++locatorIndex;
        }

        float trainTweenDuration = 2.5f;

        iTween.MoveTo(m_train, m_trainOnPos, trainTweenDuration);

        yield return new WaitForSeconds(trainTweenDuration);

        StartCoroutine("SpawnSteam");
    }
    
    void OnAnswer(GameWidget widget)
    {
        if (widget.data ["value"].ToString() == m_currentLabel.text)
        {
            StartCoroutine(OnCorrect(widget));
        }
        else
        {
            widget.Shake();
            widget.TintGray();
        }
    }
    
    IEnumerator OnCorrect(GameWidget widget)
    {
        StopCoroutine("SpawnSteam");
        m_scoreKeeper.UpdateScore();

        m_spawnedAnswers.Remove(widget);
        CollectionHelpers.DestroyObjects(m_spawnedAnswers, true);

        widget.transform.parent = m_currentLabel.transform.parent;

        float widgetTweenDuration = widget.GetOffDuration();

        TweenAlpha.Begin(m_missingNumberMarker.gameObject, widgetTweenDuration, 0);

        widget.TweenToPos(m_currentLabel.transform.position);
        widget.FadeBackground(true);

        iTween.ScaleTo(widget.gameObject, widget.transform.localScale * 0.75f, widgetTweenDuration);

        yield return new WaitForSeconds(widgetTweenDuration + 1f);

        float trainTweenDuration = 0.75f;

        Hashtable tweenArgs = new Hashtable();

        tweenArgs.Add("position", m_trainRightOffLocation);
        tweenArgs.Add("time", trainTweenDuration);
        tweenArgs.Add("easetype", iTween.EaseType.easeInQuad);

        iTween.MoveTo(m_train, tweenArgs);

        yield return new WaitForSeconds(trainTweenDuration);

        widget.Off();

        yield return new WaitForSeconds(0.75f);

        
        m_train.transform.position = m_trainLeftOffLocation.position;

        if (m_scoreKeeper.HasCompleted())
        {
            StartCoroutine(CompleteGame());
        }
        else
        {
            StartCoroutine(AskQuestion());
        }
    }

    protected override IEnumerator CompleteGame()
    {
        yield return StartCoroutine(m_scoreKeeper.On());
        GameManager.Instance.CompleteGame();
    }
}
