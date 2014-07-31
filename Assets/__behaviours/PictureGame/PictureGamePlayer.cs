using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class PictureGamePlayer : GamePlayer 
{
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;
    [SerializeField]
    private GameObject m_wordButtonPrefab;
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
    private int[] m_wordsToSpawnForDifficulty;

    //private int m_numLives = 3;

    List<DataRow> m_remainingWords = new List<DataRow>();
    List<DataRow> m_wordPool = new List<DataRow>();

    List<WordSelectionButton> m_selectionButtons = new List<WordSelectionButton>();

    AudioClip m_wordAudio;

    DataRow m_currentWordData = null;
    string m_currentWord;

	int m_maxSpawn = 2;

    public override void SelectCharacter(int characterIndex)
    {
        //D.Log("SelectCharacter");
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;
        //D.Log("m_selectedCharacter: " + m_selectedCharacter);
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }
        PictureGameCoordinator.Instance.CharacterSelected(characterIndex);
    }

    void ShowNextQuestion()
    {
        //D.Log("Player ShowNextQuestion()");
        
        if (m_remainingWords.Count == 0)
        {
            m_remainingWords.AddRange(m_wordPool);
        }
        
        
        m_currentWordData = m_remainingWords[Random.Range(0, m_remainingWords.Count)];

        m_wordAudio = DataHelpers.GetShortAudio(m_currentWordData);
        
        HashSet<DataRow> allFour = new HashSet<DataRow>();
        allFour.Add(m_currentWordData);
        while (allFour.Count < m_maxSpawn)
        {
            allFour.Add(m_wordPool[Random.Range(0, m_wordPool.Count)]);
        }
        
        List<GameObject> newLocatorList = new List<GameObject>();
        newLocatorList.AddRange(m_locators);
        foreach (DataRow wordRow in allFour)
        {
            GameObject locator = newLocatorList[Random.Range(0, newLocatorList.Count)];
            
            GameObject spawnedWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordButtonPrefab, locator.transform);
            spawnedWord.GetComponent<WordSelectionButton>().SetUp(wordRow == m_currentWordData,
                                                                  wordRow["word"].ToString(), this, m_wordsOffTransform);
            
            
            m_selectionButtons.Add(spawnedWord.GetComponent<WordSelectionButton>());
            newLocatorList.Remove(locator);
        }

        m_imageBlackboard.ShowImage(DataHelpers.GetPicture(m_currentWordData), null, null, m_currentWordData["word"].ToString());
        
        if (SessionInformation.Instance.GetNumPlayers() != 2)
        {
            GetComponent<AudioSource>().clip = m_wordAudio;
            GetComponent<AudioSource>().Play();
        }
        
        m_remainingWords.Remove(m_currentWordData);
        m_currentWord = m_currentWordData["word"].ToString();
        Resources.UnloadUnusedAssets();
    }

    public void StartGame()
    {
        m_wordPool = PictureGameCoordinator.Instance.GetWordList();
        m_wordPool = DataHelpers.OnlyPictureData(m_wordPool);
        m_remainingWords.AddRange(m_wordPool);

        //D.Log("wordPool.Count: " + m_wordPool.Count);

		Resources.UnloadUnusedAssets();

        m_maxSpawn = Mathf.Min(m_maxSpawn, m_wordPool.Count);
        //m_maxSpawn = Mathf.Min(m_maxSpawn, m_wordsToSpawnForDifficulty[SessionInformation.Instance.GetDifficulty()]);

		if(m_wordPool.Count == 0)
		{
			SessionInformation.SetDefaultPlayerVar();
			GameManager.Instance.CompleteGame();
		}

		//D.Log("m_maxSpawn: " + m_maxSpawn);

        m_targetScore = Mathf.Min(m_targetScore, m_remainingWords.Count);

        m_scoreKeeper.SetTargetScore(m_targetScore);
        
        ShowNextQuestion();
    }

    public void StopGame()
    {
        StopAllCoroutines();
        foreach (WordSelectionButton wsb in m_selectionButtons)
        {
            wsb.Remove();
        }
        m_imageBlackboard.Hide();
        m_selectionButtons.Clear();
    }

    IEnumerator ClearQuestion()
    {
        PipPadBehaviour.Instance.Hiding -= OnPipPadHide;

        foreach (WordSelectionButton wsb in m_selectionButtons)
        {
            if (wsb.IsCorrect())
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
        
        if (SessionInformation.Instance.GetNumPlayers() != 2)
        {
            GetComponent<AudioSource>().clip = m_wordAudio;
            GetComponent<AudioSource>().Play();
        }
        
        yield return new WaitForSeconds(1.5f);

        //WingroveAudio.WingroveRoot.Instance.PostEvent("SFX_SPARKLE");
        
        if (!m_scoreKeeper.HasCompleted() && m_remainingWords.Count > 0)
        {
            yield return new WaitForSeconds(1.0f);
            
            ShowNextQuestion();
        }
        /*
        else if(m_remainingWords.Count == 0)
        {
            SessionInformation.SetDefaultPlayerVar();

            yield return StartCoroutine(m_scoreKeeper.On());

            GameManager.Instance.CompleteGame();
        }
        */

        yield break;
    }

    void OnPipPadHide()
    {
        StartCoroutine(ClearQuestion());
    }


    public void WordClicked(bool correct, WordSelectionButton incoming)
    {
        StartCoroutine(WordClickedCo(correct, incoming));
    }

    IEnumerator WordClickedCo(bool correct, WordSelectionButton incoming)
    {
        //UserStats.Activity.IncrementNumAnswers();
        
        int scoreDelta = correct ? 1 : -1;
        m_scoreKeeper.UpdateScore(scoreDelta);
        
        if (correct)
        {
            StartCoroutine(ClearQuestion());
        }
        else
        {
            //UserStats.Activity.AddIncorrectWord(m_currentWordData);
            
            // remove lives
            //m_numLives--;
            //m_livesDisplay.SetLives(m_numLives);
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            WingroveAudio.WingroveRoot.Instance.PostEvent("NEGATIVE_HIT");

            yield return new WaitForSeconds(0.75f);

            if (SessionInformation.Instance.GetNumPlayers() != 2)
            {
                PipPadBehaviour.Instance.Hiding += OnPipPadHide;
                PipPadBehaviour.Instance.Show(m_currentWord);
                PipPadBehaviour.Instance.SayAll(1.5f);
            }
        }
    }

    public IEnumerator ActivateScoreKeeper()
    {
        yield return StartCoroutine(m_scoreKeeper.On());
        //D.Log("Player returning");
    }

    public bool HasFinished()
    {
        return m_scoreKeeper.HasCompleted();
    }

    public bool HasWon()
    {
        return m_scoreKeeper.GetScore() == m_targetScore;
    }

    public void ShowRetryPrompt()
    {
        StopGame();
        m_retryPrompt.SetActive(true);
    }
}
