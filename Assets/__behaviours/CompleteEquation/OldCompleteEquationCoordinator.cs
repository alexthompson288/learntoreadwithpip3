using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class OldCompleteEquationCoordinator : GameCoordinator 
{
    [SerializeField]
    private GameObject m_answerPrefab;
    [SerializeField]
    private GameObject m_equationPartPrefab;
    [SerializeField]
    private Transform[] m_answerLocators;
    [SerializeField]
    private Transform[] m_equationPartLocators;
    [SerializeField]
    private Transform m_operationLocator;
    [SerializeField]
    private float m_probabilityCurrentIsInteger;
    [SerializeField]
    private int m_numAnswersSpawn = 3;

    List<GameWidget> m_spawnedEquationParts = new List<GameWidget>();
    List<GameWidget> m_spawnedAnswers = new List<GameWidget>();

    List<DataRow> m_operators = new List<DataRow>();

    int m_missingIndex = 0;

    IEnumerator Start()
    {
        m_scoreKeeper.SetTargetScore(m_targetScore);

        m_numAnswersSpawn = Mathf.Min(m_numAnswersSpawn, m_answerLocators.Length);

        System.Array.Sort(m_equationPartLocators, CollectionHelpers.LeftToRight);

        m_probabilityCurrentIsInteger = Mathf.Clamp01(m_probabilityCurrentIsInteger);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetNumbers();

        m_operators = DataHelpers.GetArithmeticOperators();

        if (m_dataPool.Count > 0)
        {
            m_startTime = Time.time;

            AskQuestion();
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
    }

    void AskQuestion()
    {
        DataRow sum = DataHelpers.GetLegalSum(m_dataPool);

        List<DataRow> equationParts = DataHelpers.GetLegalAdditionLHS(sum, m_dataPool);
        equationParts.Add(sum); // Add sum last because it needs to go on RHS and m_equationPartLocators are sorted from left to right

        m_missingIndex = Random.Range(0, equationParts.Count);

        m_currentData = equationParts [m_missingIndex];

        for (int i = 0; i < equationParts.Count && i < m_equationPartLocators.Length; ++i)
        {
            if(i != m_missingIndex)
            {
                GameObject newGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_equationPartPrefab, m_equationPartLocators[i], true);

                GameWidget widget = newGo.GetComponent<GameWidget>() as GameWidget;
                widget.SetUp(equationParts[i]);
                widget.FadeBackground(true, true);
                widget.SetPressedAudio("");
                widget.EnableDrag(false);
                widget.GetComponentInChildren<WobbleGUIElement>().enabled = false;
                widget.Unpressing += OnClickEquationPart;
                m_spawnedEquationParts.Add(widget);
            }
        }

        HashSet<DataRow> answers = new HashSet<DataRow>();
        answers.Add(m_currentData);
        while (answers.Count < m_numAnswersSpawn)
        {
            answers.Add(GetRandomData());
        }

        CollectionHelpers.Shuffle(m_answerLocators);

        int locatorIndex = 0;
        foreach(DataRow answer in answers)
        {
            GameObject newGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_equationPartPrefab, m_answerLocators[locatorIndex], true);
            
            GameWidget widget = newGo.GetComponent<GameWidget>() as GameWidget;
            widget.SetUp(answer);
            widget.Unpressing += OnAnswer;
            m_spawnedAnswers.Add(widget);

            ++locatorIndex;
        }
    }

    void OnAnswer(GameWidget widget)
    {
        if (widget.data == m_currentData)
        {
            widget.TweenToPos(m_equationPartLocators[m_missingIndex].position);

            widget.FadeBackground(true);

            ++m_score;
            m_scoreKeeper.UpdateScore();

            StartCoroutine(ClearQuestion());
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            widget.TweenToStartPos();
            widget.TintGray();
        }
    }

    IEnumerator ClearQuestion()
    {
        yield return new WaitForSeconds(1f);

        CollectionHelpers.DestroyObjects(m_spawnedAnswers, true);
        CollectionHelpers.DestroyObjects(m_spawnedEquationParts, true);

        if (m_scoreKeeper.HasCompleted())
        {
            StartCoroutine(CompleteGame());
        }
        else
        {
            AskQuestion();
        }
    }

    void OnClickEquationPart(GameWidget widget)
    {
        AudioClip clip = LoaderHelpers.LoadAudioForNumber(widget.data);
        if(clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }
    }

    protected override IEnumerator CompleteGame()
    {
        float timeTaken = Time.time - m_startTime;
        
        float twoStarPerQuestion = 8;
        float threeStarPerQuestion = 4;
        
        int stars = ScoreInfo.CalculateTimeStars(timeTaken, twoStarPerQuestion * (float)m_targetScore, threeStarPerQuestion * (float)m_targetScore);
        
        // Game ends when player reaches targetScore
        ScoreInfo.Instance.NewScore(timeTaken, m_targetScore, m_targetScore, stars);

        yield return StartCoroutine(m_scoreKeeper.Celebrate());
        GameManager.Instance.CompleteGame();
    }
}
