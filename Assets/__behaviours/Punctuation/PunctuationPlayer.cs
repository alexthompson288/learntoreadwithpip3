using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wingrove;

public class PunctuationPlayer : PlusGamePlayer
{
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private Transform m_textPosition;
    [SerializeField]
    private UILabel m_scaleLabel;
    [SerializeField]
    private UISprite m_highlight;

    List<GameWidget> m_spawnedAnswers = new List<GameWidget>();
    List<GameObject> m_spawnedText = new List<GameObject>();
    
    DataRow m_currentData;
    GameObject m_currentText;

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
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }
        
        m_scoreKeeper.SetCharacterIcon(characterIndex);
        PunctuationCoordinator.Instance.CharacterSelected(characterIndex);
    }
    
    public void StartGame()
    {
        m_scoreKeeper.SetHealthLostPerSecond(1f);

        m_scoreKeeper.LevelledUp += OnLevelUp;
        m_scoreKeeper.Completed += OnScoreKeeperComplete;
        m_scoreKeeper.StartTimer();
        
        AskQuestion();
    }
    
    void AskQuestion()
    {
        m_scaleLabel.transform.localScale = Vector3.one;
        
        float maxWidth = 900;
        float maxHeight = 220;
        
        string originalText = m_currentData ["good_sentence"].ToString().Replace("\\n", "\n").Replace("\n", "").Replace("  ", " ");
        
        string textToDisplay = originalText;
        
        while (true)
        {
            int lineStartIndex = 0;
            int checkLength = 1;
            for (int i = 0; i < textToDisplay.Length && lineStartIndex + checkLength < textToDisplay.Length; ++i)
            {
                if (NGUIHelpers.GetLabelWidth(m_scaleLabel, textToDisplay.Substring(lineStartIndex, checkLength)) > maxWidth)
                {
                    // Find empty char before lineStartIndex + checkLength
                    while (textToDisplay[lineStartIndex + checkLength] != ' ')
                    {
                        --checkLength;
                    }
                    
                    while(checkLength > -1)
                    {
                        if(textToDisplay[lineStartIndex + checkLength] == ' ')
                        {
                            ++checkLength; // Increment checkLength so that the empty character is at the end of the current line instead of the start of the new one
                            break;
                        }
                        else
                        {
                            --checkLength;
                        }
                    }

                    textToDisplay = textToDisplay.Insert(lineStartIndex + checkLength, "\n");
                    lineStartIndex = lineStartIndex + checkLength;

                    checkLength = 1;
                } 
                else
                {
                    ++checkLength;
                }
            }
            
            if(NGUIHelpers.GetLabelHeight(m_scaleLabel, textToDisplay) < maxHeight)
            {
                break;
            }
            else
            {
                m_scaleLabel.transform.localScale *= 0.99f;
                textToDisplay = originalText;
            }
        }
        
        m_textPosition.transform.localScale = m_scaleLabel.transform.localScale;
        
        m_scaleLabel.text = textToDisplay;

        string[] punctuation = PunctuationCoordinator.Instance.GetPunctuation();
        List<string> validPunctuation = new List<string>();
        foreach (string punc in punctuation)
        {
            if(textToDisplay.Contains(punc.ToString()))
            {
                validPunctuation.Add(punc);
            }
        }

        string[] lines = textToDisplay.Split('\n');
        
        float length = 0;
        float height = 0;
        float widestLineWidth = 0;

        GameObject textPrefab = PunctuationCoordinator.Instance.GetTextPrefab();
        
        foreach (string line in lines)
        {
            List<string> lineWords = line.Split(new char[]{' '}, System.StringSplitOptions.RemoveEmptyEntries).ToList();

            FurtherSplit(lineWords, validPunctuation);

            bool hadValidWord = false;
            foreach (string newWord in lineWords)
            {
                if (!string.IsNullOrEmpty(newWord) && newWord != " ")
                {
                    hadValidWord = true;
                    GameObject newWordInstance = SpawningHelpers.InstantiateUnderWithIdentityTransforms(textPrefab, m_textPosition);
                    
                    m_spawnedText.Add(newWordInstance);

                    UILabel label = newWordInstance.GetComponent<UILabel>() as UILabel;
                    label.text = newWord + " ";
                    newWordInstance.transform.localPosition = new Vector3(length, height, 0);
                    Vector3 wordSize = NGUIHelpers.GetLabelSize3(label);
                    length += wordSize.x;
                    widestLineWidth = Mathf.Max(widestLineWidth, length);

                    newWordInstance.GetComponent<ShowPipPadForWord>().SetUp(newWord, wordSize, true);
                }
            }
            if (hadValidWord)
            {
                length = 0;
                height -= 100;
            }
        }

        string currentPunctuation = validPunctuation [Random.Range(0, validPunctuation.Count)];
        List<GameObject> matchingText = m_spawnedText.FindAll(x => x.GetComponent<UILabel>().text.Replace(" ", "").Equals(currentPunctuation));

        foreach (GameObject text in m_spawnedText)
        {
            string labelText = text.GetComponent<UILabel>().text.Replace(" ", "");
        }

        m_currentText = matchingText [Random.Range(0, matchingText.Count)];

        UILabel currentLabel = m_currentText.GetComponent<UILabel>() as UILabel;
        m_highlight.transform.position = m_currentText.transform.position;
        m_highlight.height = currentLabel.height;
        m_highlight.width = Mathf.RoundToInt(NGUIHelpers.GetLabelWidth(currentLabel));
        currentLabel.enabled = false;


        HashSet<string> answers = new HashSet<string>();
        answers.Add(currentPunctuation);

        int numAnswersToSpawn = Mathf.Max(PunctuationCoordinator.Instance.GetNumAnswersToSpawn(), m_locators.Length);
        while (answers.Count < numAnswersToSpawn)
        {
            answers.Add(punctuation[Random.Range(0, punctuation.Length)]);
        }

        GameObject answerPrefab = PunctuationCoordinator.Instance.GetAnswerPrefab();

        CollectionHelpers.Shuffle(m_locators);
        int locatorIndex = 0;

        foreach (string answer in answers)
        {
            GameObject newAnswer = SpawningHelpers.InstantiateUnderWithIdentityTransforms(answerPrefab, m_locators[locatorIndex]);

            GameWidget widget = newAnswer.GetComponent<GameWidget>() as GameWidget;
            widget.SetUp(answer.ToString(), false);
            widget.EnableDrag(false);
            widget.Unpressing += OnAnswer;
            m_spawnedAnswers.Add(widget);

            UILabel widgetLabel = widget.GetComponentInChildren<UILabel>() as UILabel;
            //widgetLabel.transform.localPosition = new Vector3(0, NGUIHelpers.GetLabelHeight(widgetLabel) / 2.5f, 0);
            widgetLabel.transform.localPosition = new Vector3(0, 30, 0);

            ++locatorIndex;
        }
    }

    void FurtherSplit(List<string> words, List<string> punctuation)
    {
        bool hasSplit = false;
        for (int i = 0; i < words.Count; ++i)
        {
            for(int j = 0; j < punctuation.Count; ++j)
            {
                if(words[i].Contains(punctuation[j]) && !words[i].Equals(punctuation[j]))
                {
                    words[i] = words[i].Replace(punctuation[j], "");
                    words.Insert(i + 1, punctuation[j]);
                    hasSplit = true;
                    break; 
                }
            }
            if(hasSplit)
            {
                break;
            }
        }

        if(hasSplit)
        {
            FurtherSplit(words, punctuation);
        }
    }

    void OnAnswer(GameWidget widget)
    {
        widget.GetComponentInChildren<WobbleGUIElement>().enabled = false;

        widget.ChangeBackgroundState(true);
        WingroveAudio.WingroveRoot.Instance.PostEvent("SPLAT_MUSHROOM");

        if (widget.labelText.Replace(" ", "") == m_currentText.GetComponent<UILabel>().text.Replace(" ", ""))
        {
            m_scoreKeeper.UpdateScore(1);

            m_currentText.GetComponent<UILabel>().pivot = UIWidget.Pivot.Left;
            Vector3 localOffset = new Vector3(20, -20, 0);
            m_currentText.transform.localPosition = m_currentText.transform.localPosition + localOffset;
            widget.TweenToPos(m_currentText.transform.position);

            widget.FadeBackground(true);
            PunctuationCoordinator.Instance.OnCorrectAnswer(this);
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            m_scoreKeeper.UpdateScore(-1);
            widget.TintGray();
            widget.EnableCollider(false);
        }
    }
    
    public IEnumerator ClearQuestion()
    {
        yield return new WaitForSeconds(1f);

        CollectionHelpers.DestroyObjects(m_spawnedAnswers, true);
        CollectionHelpers.DestroyObjects(m_spawnedText, false);
        
        AskQuestion();
    }
    
    void OnScoreKeeperComplete(ScoreKeeper scoreKeeper)
    {
        PunctuationCoordinator.Instance.CompleteGame();
    }
    
    void OnLevelUp(ScoreKeeper scoreKeeper)
    {
        PunctuationCoordinator.Instance.OnLevelUp();
    }
}
