using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlusCorrectWordPlayer : PlusGamePlayer 
{
    [SerializeField]
    private GameObject m_questionParent;
    [SerializeField]
    private UITexture m_questionImage;
    [SerializeField]
    private GameObject m_questionImageParent;
    [SerializeField]
    private GameObject[] m_questionLabels;
    [SerializeField]
    private ClickEvent m_yesButton;
    [SerializeField]
    private ClickEvent m_noButton;
    [SerializeField]
    private Transform m_questionLabelLocationOn;
    [SerializeField]
    private Transform m_questionLabelLocationLeftOff;
    [SerializeField]
    private Transform m_questionLabelLocationRightOff;

    List<GameObject> m_spawnedQuestionText = new List<GameObject>();

    DataRow m_currentData = null;

    bool m_hasAnswered = false;
    
    public void SetCurrentData(DataRow myCurrentData)
    {
        m_currentData = myCurrentData;
    }
    
    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
    }
    
    public override void SelectCharacter(int characterIndex)
    {
        base.SelectCharacter(characterIndex);

        PlusCorrectWordCoordinator.Instance.CharacterSelected(characterIndex);
    }

    public void StartGame()
    {
        m_yesButton.SingleClicked += OnAnswer;
        m_noButton.SingleClicked += OnAnswer;
        
        m_scoreKeeper.SetHealthLostPerSecond(1f);
        
        m_scoreKeeper.LevelledUp += OnLevelUp;
        m_scoreKeeper.Completed += OnScoreKeeperComplete;
        m_scoreKeeper.StartTimer();
        
        m_questionParent.GetComponent<TweenOnOffBehaviour>().On();
        
        AskQuestion();    
    }

    void AskQuestion()
    {
        m_questionLabels[0].transform.localPosition = m_questionLabelLocationOn.localPosition;
        m_questionLabels[1].transform.localPosition = m_questionLabelLocationLeftOff.localPosition;

        Texture2D tex = DataHelpers.GetPicture(m_currentData);
        m_questionImage.mainTexture = tex;

        string word = m_currentData ["word"].ToString();

        UILabel label1 = m_questionLabels [1].GetComponentInChildren<UILabel>() as UILabel;
        label1.GetComponentInChildren<UILabel>().text = word;
        m_questionLabels[1].GetComponentInChildren<ShowPipPadForWord>().SetUp(word, NGUIHelpers.GetLabelSize3(label1), true);

        Color[] colors = new Color[2];

        if (Random.Range(0f, 1f) > PlusCorrectWordCoordinator.Instance.GetProbabilityYesIsCorrect())
        {
            string dummy1 = PlusCorrectWordCoordinator.Instance.GetDummyAttribute1();
            string dummy2 = PlusCorrectWordCoordinator.Instance.GetDummyAttribute2();

            string dummyAttribute = Random.Range(0, 2) == 0 ? dummy1 : dummy2;
            if (m_currentData [dummyAttribute] == null)
            {
                dummyAttribute = dummyAttribute == dummy1 ? dummy2 : dummy1;
            }

            word = m_currentData [dummyAttribute].ToString();

            colors [0] = PlusCorrectWordCoordinator.Instance.GetIncorrectColor();
            colors [1] = PlusCorrectWordCoordinator.Instance.GetCorrectColor();
        } 
        else
        {
            colors [0] = PlusCorrectWordCoordinator.Instance.GetCorrectColor();
            colors [1] = PlusCorrectWordCoordinator.Instance.GetIncorrectColor();
        }

        for (int i = 0; i < m_questionLabels.Length && i < colors.Length; ++i)
        {
            UISprite highlight = m_questionLabels[i].GetComponentInChildren<UISprite>() as UISprite;
            highlight.color = colors[i];
            highlight.enabled = false;
        }
            
        UILabel label0 = m_questionLabels [0].GetComponentInChildren<UILabel>() as UILabel;
        label0.text = word;
        m_questionLabels[0].GetComponentInChildren<ShowPipPadForWord>().SetUp(word, NGUIHelpers.GetLabelSize3(label0), true);

        TweenQuestionParents(Vector3.one);
    }

    void OnAnswer(ClickEvent click)
    {
        if (!m_hasAnswered)
        {
            m_hasAnswered = true;
            StartCoroutine(OnAnswerCo(click));
        }
   }

    IEnumerator OnAnswerCo(ClickEvent click)
    {
        for (int i = 0; i < m_questionLabels.Length; ++i)
        {
            m_questionLabels[i].GetComponentInChildren<UISprite>().enabled = true;
        }

        bool answerIsYes = click.GetInt() == 1; 
        bool yesIsCorrect = m_questionLabels[0].GetComponentInChildren<UILabel>().text == m_currentData ["word"].ToString(); // The current attribute is the last one in m_remainingAttributes
        bool isCorrect = yesIsCorrect == answerIsYes;
        
        if (isCorrect)
        {
            m_scoreKeeper.UpdateScore(1);
        } 
        else
        {
            m_scoreKeeper.UpdateScore(-1);
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");

            float shakeDuration = 0.3f;
            iTween.ShakePosition(m_questionParent, Vector3.one * 0.2f, shakeDuration);
            yield return new WaitForSeconds(shakeDuration);
        }

        if (!yesIsCorrect)
        {
            float tweenDuration = PlusCorrectWordCoordinator.Instance.GetQuestionTweenDuration();
            iTween.MoveTo(m_questionLabels[1], m_questionLabelLocationOn.position, tweenDuration);
            yield return new WaitForSeconds(tweenDuration / 3);
            iTween.MoveTo(m_questionLabels[0], m_questionLabelLocationRightOff.position, tweenDuration);
        }

        float delay = 1f;

        if (isCorrect && !answerIsYes)
        {
            delay = 1.5f;
        }
        else if(!isCorrect)
        {
            delay = 2f;
        }

        yield return new WaitForSeconds(delay);
        
        PlusCorrectWordCoordinator.Instance.OnAnswer(this);
    }
    
    void TweenQuestionParents(Vector3 newScale)
    {
        iTween.ScaleTo(m_questionImageParent, newScale, PlusCorrectWordCoordinator.Instance.GetQuestionTweenDuration());

        string audioString = Mathf.Approximately(newScale.x, 0) ? "SOMETHING_APPEAR" : "SOMETHING_DISAPPEAR";
        WingroveAudio.WingroveRoot.Instance.PostEvent(audioString);
    }
    
    public IEnumerator ClearQuestion()
    {
        m_hasAnswered = false;

        TweenQuestionParents(Vector3.zero);
        
        yield return new WaitForSeconds(PlusCorrectWordCoordinator.Instance.GetQuestionTweenDuration());
        
        AskQuestion();
    }

    void OnScoreKeeperComplete(ScoreKeeper scoreKeeper)
    {
        PlusCorrectWordCoordinator.Instance.CompleteGame();
    }
    
    void OnLevelUp(ScoreKeeper scoreKeeper)
    {
        PlusCorrectWordCoordinator.Instance.OnLevelUp();
    }
}
