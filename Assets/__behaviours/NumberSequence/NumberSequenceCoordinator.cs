using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class NumberSequenceCoordinator : GameCoordinator
{
    [SerializeField]
    private GameObject m_numberPrefab;
    [SerializeField]
    private Transform[] m_sequenceLocations;
    [SerializeField]
    private Transform[] m_answerLocators;
    [SerializeField]
    private int m_numToSpawn;
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
    
    int m_currentIndex = 0;

    List<DataRow> m_currentNumberSequence = new List<DataRow>();
    
    List<GameWidget> m_spawnedWidgets = new List<GameWidget>();
    
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
        System.Array.Sort(m_sequenceLocations, CollectionHelpers.LeftToRight);

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
        
        m_numToSpawn = Mathf.Min(m_numToSpawn, m_answerLocators.Length);
        m_numToSpawn = Mathf.Max(m_numToSpawn, m_sequenceLocations.Length);
        
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        
        m_dataPool = DataHelpers.GetNumbers();
        
        m_dataPool.Sort((x, y) => x.GetInt("value").CompareTo(y.GetInt("value")));
        
        if (m_dataPool.Count > 0)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("TRAIN_CHOO_CHOO");
            WingroveAudio.WingroveRoot.Instance.PostEvent("TRAIN_CHUGA_CHUGA");
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
        CollectionHelpers.DestroyObjects(m_spawnedWidgets);

        m_currentIndex = 0;

        m_missingNumberMarker.transform.position = m_sequenceLocations [m_currentIndex].position;

        m_currentNumberSequence.Clear();
        int numberIndex = Random.Range(0, m_dataPool.Count - m_labels.Length);
        while (m_currentNumberSequence.Count < m_numToSpawn)
        {
            m_currentNumberSequence.Add(m_dataPool[numberIndex]);
            ++numberIndex;
        }

        CollectionHelpers.Shuffle(m_answerLocators);
        
        for (int i = 0; i < m_currentNumberSequence.Count && i < m_answerLocators.Length; ++i)
        {
            GameObject newAnswer = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_numberPrefab, m_answerLocators[i]);
            
            GameWidget widget = newAnswer.GetComponent<GameWidget>() as GameWidget;
            
            widget.SetUp(m_currentNumberSequence[i]);
            widget.Unpressing += OnAnswer;
            m_spawnedWidgets.Add(widget);
        }
        
        float trainTweenDuration = 2.5f;
        
        WingroveAudio.WingroveRoot.Instance.PostEvent("TRAIN_CHUGA_CHUGA_STOP");
        
        iTween.MoveTo(m_train, m_trainOnPos, trainTweenDuration);
        
        yield return new WaitForSeconds(trainTweenDuration);
        
        StartCoroutine("SpawnSteam");
    }
    
    void OnAnswer(GameWidget widget)
    {
        if (widget.data.Equals(m_currentNumberSequence[m_currentIndex]))
        {
            widget.GetComponentInChildren<WobbleGUIElement>().enabled = false;

            widget.transform.parent = m_sequenceLocations[m_currentIndex];
            
            widget.TweenToPos(m_sequenceLocations[m_currentIndex].position);
            widget.FadeBackground(true);
            
            iTween.ScaleTo(widget.gameObject, widget.transform.localScale * 0.75f, widget.GetOffDuration());

            foreach(GameWidget spawnedWidget in m_spawnedWidgets)
            {
                spawnedWidget.TintWhite();
                spawnedWidget.ChangeBackgroundState(false);
            }

            ++m_currentIndex;

            if(m_currentIndex >= m_numToSpawn)
            {
                StartCoroutine(OnCorrect());
            }
            else
            {
                m_missingNumberMarker.transform.position = m_sequenceLocations [m_currentIndex].position;
            }
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            widget.Shake();
            widget.TintGray();
        }
    }
    
    IEnumerator OnCorrect()
    {
        StopCoroutine("SpawnSteam");
        m_scoreKeeper.UpdateScore();

        yield return new WaitForSeconds(1.5f);
        
        float trainTweenDuration = 0.75f;
        
        Hashtable tweenArgs = new Hashtable();
        
        tweenArgs.Add("position", m_trainRightOffLocation);
        tweenArgs.Add("time", trainTweenDuration);
        tweenArgs.Add("easetype", iTween.EaseType.easeInQuad);
        
        iTween.MoveTo(m_train, tweenArgs);
        
        WingroveAudio.WingroveRoot.Instance.PostEvent("TRAIN_CHOO_CHOO");
        WingroveAudio.WingroveRoot.Instance.PostEvent("TRAIN_CHUGA_CHUGA");
        
        yield return new WaitForSeconds(trainTweenDuration);
        
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
        WingroveAudio.WingroveRoot.Instance.PostEvent("TRAIN_CHUGA_CHUGA_STOP");
        yield return StartCoroutine(m_scoreKeeper.On());
        GameManager.Instance.CompleteGame();
    }
}
/*
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

        m_dataPool.Sort((x, y) => x.GetInt("value").CompareTo(y.GetInt("value")));

        if (m_dataPool.Count > 0)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("TRAIN_CHOO_CHOO");
            WingroveAudio.WingroveRoot.Instance.PostEvent("TRAIN_CHUGA_CHUGA");
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

        WingroveAudio.WingroveRoot.Instance.PostEvent("TRAIN_CHUGA_CHUGA_STOP");

        iTween.MoveTo(m_train, m_trainOnPos, trainTweenDuration);

        yield return new WaitForSeconds(trainTweenDuration / 2);



        yield return new WaitForSeconds(trainTweenDuration / 2);

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

        WingroveAudio.WingroveRoot.Instance.PostEvent("TRAIN_CHOO_CHOO");
        WingroveAudio.WingroveRoot.Instance.PostEvent("TRAIN_CHUGA_CHUGA");

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
        WingroveAudio.WingroveRoot.Instance.PostEvent("TRAIN_CHUGA_CHUGA_STOP");
        yield return StartCoroutine(m_scoreKeeper.On());
        GameManager.Instance.CompleteGame();
    }
}
*/
