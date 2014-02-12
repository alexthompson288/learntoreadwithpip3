using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class LetterPictureGamePlayer : GamePlayer {
	
    [SerializeField]
    private ProgressScoreBar m_scoreBar;
    [SerializeField]
    private GameObject m_letterButtonPrefab;
    [SerializeField]
    private GameObject[] m_locators;
    [SerializeField]
    private ImageBlackboard m_imageBlackboard;
    [SerializeField]
    private Transform m_wordsOffTransform;
    [SerializeField]
    private Transform m_locatorOffCorrect;
    [SerializeField]
    private LivesDisplay m_livesDisplay;
    [SerializeField]
    private int m_targetScore = 6;
    [SerializeField]
    private GameObject m_retryPrompt;
    [SerializeField]
    private int m_playerIndex;

    private int m_selectedCharacter = -1;
    private int m_score = 0;
    private int m_numLives = 3;

    List<DataRow> m_remainingWords = null;
    List<DataRow> m_allWords = null;

    List<CharacterSelection> m_characterSelections = new List<CharacterSelection>();
    List<LetterSelectionButton> m_selectionButtons = new List<LetterSelectionButton>();
	
	
	List<DataRow> m_lettersPool = new List<DataRow>();
	Dictionary<DataRow, Texture2D> m_phonemeImages = new Dictionary<DataRow, Texture2D>();
	
	public void SetUp(List<DataRow> lettersPool, Dictionary<DataRow, Texture2D> phonemeImages)
	{
		m_lettersPool = lettersPool;
		m_phonemeImages = phonemeImages;
	}
	
    public override void RegisterCharacterSelection(CharacterSelection characterSelection)
    {
        m_characterSelections.Add(characterSelection);
    }

    public override void SelectCharacter(int characterIndex)
    {
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;
        m_livesDisplay.SetLifeIcon(m_selectedCharacter);
        m_livesDisplay.SetLives(m_numLives);
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }
        LetterPictureGameCoordinator.Instance.CharacterSelected(characterIndex);
    }

    public void HideCharacter(int index)
    {
        foreach (CharacterSelection cs in m_characterSelections)
        {
            if (cs.GetCharacterIndex() == index)
            {
                cs.DeactivatePress(false);
            }
        }
    }

    public void HideAll()
    {
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(true);
        }
    }

    public bool HasSelectedCharacter()
    {
        return (m_selectedCharacter != -1);
    }

    void ShowNextQuestion()
    {
        DataRow selectedQuestion = m_remainingWords[Random.Range(0, m_remainingWords.Count)];

        HashSet<DataRow> allFour = new HashSet<DataRow>();
        allFour.Add(selectedQuestion);
        while (allFour.Count < 4)
        {
            DataRow newWord = m_allWords[Random.Range(0, m_allWords.Count)];
            bool okToAdd = true;
            foreach (DataRow dr in allFour)
            {
                if (dr["word"].ToString() == newWord["word"].ToString())
                {
                    okToAdd = false;
                }
            }
            if (okToAdd)
            {
                allFour.Add(newWord);
            }
        }

        List<GameObject> newLocatorList = new List<GameObject>();
        newLocatorList.AddRange(m_locators);
        foreach (DataRow wordRow in allFour)
        {
            GameObject locator = newLocatorList[Random.Range(0, newLocatorList.Count)];

            GameObject spawnedWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterButtonPrefab, locator.transform);
            spawnedWord.GetComponent<LetterSelectionButton>().SetUp(wordRow == selectedQuestion,
                wordRow["word"].ToString(), this, m_wordsOffTransform);

            m_selectionButtons.Add(spawnedWord.GetComponent<LetterSelectionButton>());
            newLocatorList.Remove(locator);
        }

        Texture2D wordImage = null;
        if ((selectedQuestion["image"] != null) && (!string.IsNullOrEmpty(selectedQuestion["image"].ToString())))
        {
            wordImage = (Texture2D)Resources.Load("Images/word_images_png_350/_" + selectedQuestion["image"].ToString());
        }
        else
        {
            wordImage = (Texture2D)Resources.Load("Images/word_images_png_350/_" + selectedQuestion["word"].ToString());
        }

        m_imageBlackboard.ShowImage(wordImage, null, null, null);

        m_remainingWords.Remove(selectedQuestion);

    }

    public void StartGame()
    {
        m_allWords = LetterPictureGameCoordinator.Instance.GetWordList();
        m_remainingWords = new List<DataRow>();
        m_remainingWords.AddRange(m_allWords);
        m_scoreBar.SetStarsTarget(m_targetScore);
        
        ShowNextQuestion();
    }

    public void StopGame()
    {
        StopAllCoroutines();
        foreach (LetterSelectionButton wsb in m_selectionButtons)
        {
            wsb.Remove();
        }
        m_imageBlackboard.Hide();
        m_selectionButtons.Clear();
    }

    IEnumerator DelayForNextWord()
    {
        yield return new WaitForSeconds(0.5f);

        ShowNextQuestion();
    }

    public void WordClicked(bool correct, LetterSelectionButton incoming)
    {
        if (correct)
        {
            m_score++;
            m_scoreBar.SetScore(m_score);
            m_scoreBar.SetStarsCompleted(m_score);
            
            foreach (LetterSelectionButton wsb in m_selectionButtons)
            {
                if (incoming == wsb)
                {
                    wsb.RemoveCorrect(m_locatorOffCorrect);
                }
                else
                {
                    wsb.Remove();
                }
            }

            m_imageBlackboard.Hide();
            m_selectionButtons.Clear();
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");
            WingroveAudio.WingroveRoot.Instance.PostEvent("SFX_SPARKLE");

            if (m_score != m_targetScore)
            {
                StartCoroutine(DelayForNextWord());
            }
        }
        else
        {
            // remove lives
            m_numLives--;
            m_livesDisplay.SetLives(m_numLives);
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            WingroveAudio.WingroveRoot.Instance.PostEvent("NEGATIVE_HIT");
        }

    }

    public bool HasFinished()
    {
        return ((m_numLives == 0) || (m_score == m_targetScore));
    }

    public bool HasWon()
    {
        return (m_score == m_targetScore);
    }

    public void ShowRetryPrompt()
    {
        StopGame();
        m_retryPrompt.SetActive(true);
    }
}
