using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class CompleteEquationPlayer : GamePlayer
{
    [SerializeField]
    private ScoreHealth m_scoreKeeper;
    [SerializeField]
    private TrafficLights m_trafficLights;
    [SerializeField]
    private Transform[] m_answerLocators;
    [SerializeField]
    private Transform[] m_equationPartLocators;
    [SerializeField]
    private TweenBehaviour m_equationMoveable;
    [SerializeField]
    private GameObject m_conveyorBelt;
    [SerializeField]
    private UISprite m_light;
    [SerializeField]
    private RotateConstantly[] m_conveyorWheels;

    float m_conveyorBeltOffsetX = 3036;

    List<GameWidget> m_spawnedEquationParts = new List<GameWidget>();
    List<GameWidget> m_spawnedAnswers = new List<GameWidget>();
    
    List<DataRow> m_equationParts = new List<DataRow>();
    int m_missingIndex = 0;
    DataRow m_currentData = null;

    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
    }

    public override void SelectCharacter(int characterIndex)
    {
        ////////D.Log("SelectCharacter");
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;
        ////////D.Log("m_selectedCharacter: " + m_selectedCharacter);
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }

        m_scoreKeeper.SetCharacterIcon(characterIndex);
        CompleteEquationCoordinator.Instance.CharacterSelected(characterIndex);
    }

    public void SetEquation(CompleteEquationCoordinator.Equation equation)
    {
        m_equationParts = equation.m_equationParts;
        m_missingIndex = equation.m_missingIndex;
    }

    void EnableWheels(bool enable)
    {
        foreach (RotateConstantly wheel in m_conveyorWheels)
        {
            wheel.enabled = enable;
        }
    }

    void Awake()
    {
        EnableWheels(false);
    }
    
    public void StartGame(bool subscribeToTimer)
    {
        System.Array.Sort(m_equationPartLocators, CollectionHelpers.LocalLeftToRight);

        m_scoreKeeper.LevelledUp += OnLevelUp;
        m_scoreKeeper.Completed += OnScoreKeeperComplete;
        m_scoreKeeper.StartTimer();

        AskQuestion();
    }

    void ChangeLightSprite(string suffix)
    {
        StopCoroutine("ResetLightSprite");
        m_light.spriteName = "light_" + suffix;
    }

    IEnumerator ResetLightSprite()
    {
        yield return new WaitForSeconds(0.8f);
        m_light.spriteName = "light_yellow";
    }
    
    void AskQuestion()
    {
        ChangeLightSprite("yellow");

        m_currentData = m_equationParts [m_missingIndex];

        GameObject equationPartPrefab = CompleteEquationCoordinator.Instance.GetEquationPartPrefab();

        for (int i = 0; i < m_equationParts.Count && i < m_equationPartLocators.Length; ++i)
        {
            if(i != m_missingIndex)
            {
                GameObject newGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(equationPartPrefab, m_equationPartLocators[i], true);
                
                GameWidget widget = newGo.GetComponent<GameWidget>() as GameWidget;
                widget.SetUp(m_equationParts[i]);
                widget.FadeBackground(true, true);
                widget.SetPressedAudio("");
                widget.EnableDrag(false);
                widget.GetComponentInChildren<WobbleGUIElement>().enabled = false;
                widget.Unpressing += OnClickEquationPart;
                m_spawnedEquationParts.Add(widget);
            }
        }

        int numAnswersToSpawn = Mathf.Min(m_answerLocators.Length, CompleteEquationCoordinator.Instance.GetNumAnswersToSpawn());

        HashSet<DataRow> answers = new HashSet<DataRow>();
        answers.Add(m_currentData);
        while (answers.Count < numAnswersToSpawn)
        {
            answers.Add(CompleteEquationCoordinator.Instance.GetRandomData());
        }

        GameObject answerPrefab = CompleteEquationCoordinator.Instance.GetAnswerPrefab();
        
        CollectionHelpers.Shuffle(m_answerLocators);
        int locatorIndex = 0;

        foreach(DataRow answer in answers)
        {
            GameObject newGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(answerPrefab, m_answerLocators[locatorIndex], true);
            
            GameWidget widget = newGo.GetComponent<GameWidget>() as GameWidget;
            widget.SetUp(answer);
            widget.Unpressing += OnAnswer;
            m_spawnedAnswers.Add(widget);
            
            ++locatorIndex;
        }

        StartCoroutine(MoveConveyorBelt(m_conveyorBeltOffsetX));
    }

    IEnumerator MoveConveyorBelt(float localPosX)
    {
        EnableWheels(true);

        float tweenDuration = 0.4f;

        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("position", new Vector3(localPosX, m_conveyorBelt.transform.localPosition.y, m_conveyorBelt.transform.localPosition.z));
        tweenArgs.Add("islocal", true);
        tweenArgs.Add("time", tweenDuration);
        tweenArgs.Add("easetype", iTween.EaseType.easeOutQuad);

        iTween.MoveTo(m_conveyorBelt, tweenArgs);

        yield return new WaitForSeconds(tweenDuration);

        EnableWheels(false);
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            GameWidget widget = m_spawnedAnswers.Find(x => x.data == m_currentData);

            if(widget != null)
            {
                OnAnswer(widget);
            }
        }
    }
#endif

    void OnAnswer(GameWidget widget)
    {
        if (widget.data == m_currentData)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("DING");
            ChangeLightSprite("green");

            widget.transform.parent = m_equationPartLocators[m_missingIndex];
            widget.TweenToPos(m_equationPartLocators[m_missingIndex].position);
            
            widget.FadeBackground(true);

            m_scoreKeeper.UpdateScore(1);
            
            CompleteEquationCoordinator.Instance.OnCorrectAnswer(this);
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            m_scoreKeeper.UpdateScore(-1);
            widget.TweenToStartPos();
            widget.TintGray();

            ChangeLightSprite("red");
            StartCoroutine(ResetLightSprite());
        }
    }

    void OnClickEquationPart(GameWidget widget)
    {
        if (SessionInformation.Instance.GetNumPlayers() == 1)
        {
            CompleteEquationCoordinator.Instance.PlayAudio(widget.data);
        }
    }

    public IEnumerator ClearQuestion()
    {
        StartCoroutine(MoveConveyorBelt(m_conveyorBeltOffsetX * 2));

        yield return new WaitForSeconds(1f);

        Vector3 conveyorLocalPos = m_conveyorBelt.transform.localPosition;
        conveyorLocalPos.x = 0;
        m_conveyorBelt.transform.localPosition = conveyorLocalPos;
        
        CollectionHelpers.DestroyObjects(m_spawnedAnswers, true);
        CollectionHelpers.DestroyObjects(m_spawnedEquationParts, true);

        if (!CompleteEquationCoordinator.Instance.HasCompleted())
        {
            AskQuestion();
        }
    }

    public void ClearGame()
    {
        CollectionHelpers.DestroyObjects(m_spawnedAnswers, true);
        m_equationMoveable.Off();
    }

    public IEnumerator CelebrateVictory()
    {
        if (SessionInformation.Instance.GetNumPlayers() == 2)
        {
            yield return new WaitForSeconds(0.8f);
            WingroveAudio.WingroveRoot.Instance.PostEvent(string.Format("PLAYER_{0}_WIN", m_selectedCharacter));
            CelebrationCoordinator.Instance.DisplayVictoryLabels(m_playerIndex);
            CelebrationCoordinator.Instance.PopCharacter(m_selectedCharacter, true);
            yield return new WaitForSeconds(1.5f);
        }

        yield return StartCoroutine(m_scoreKeeper.Celebrate());
    }

    void OnLevelUp(ScoreKeeper scoreKeeper)
    {
        CompleteEquationCoordinator.Instance.OnLevelUp();
    }

    void OnScoreKeeperComplete(ScoreKeeper scoreKeeper)
    {
        CompleteEquationCoordinator.Instance.CompleteGame();
    }

    public int GetScore()
    {
        return m_scoreKeeper.GetScore();
    }
}
