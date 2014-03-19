using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class PictureGamePlayer : GamePlayer 
{
    [SerializeField]
    private ProgressScoreBar m_scoreBar;
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
    private int m_playerIndex;
    [SerializeField]
    private int[] m_wordsToSpawnForDifficulty;

    private int m_selectedCharacter = -1;
    private int m_score = 0;
    private int m_numLives = 3;

    List<DataRow> m_remainingWords = null;
    List<DataRow> m_allWords = null;

    List<CharacterSelection> m_characterSelections = new List<CharacterSelection>();
    List<WordSelectionButton> m_selectionButtons = new List<WordSelectionButton>();

    AudioClip m_wordAudio;

    DataRow m_currentWordData = null;
    string m_currentWord;

	int m_maxSpawn = 0;

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
        PictureGameCoordinator.Instance.CharacterSelected(characterIndex);
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

    public void SetTimer(float timerT)
    {
        m_scoreBar.SetTimer(timerT);
    }

    void ShowNextQuestion()
    {
		Debug.Log("Player ShowNextQuestion()");

		Texture2D selectedTex = null;
		DataRow selectedQuestion = null;

		while(selectedTex == null)
		{
			selectedQuestion = m_remainingWords[Random.Range(0, m_remainingWords.Count)];
			selectedTex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + selectedQuestion["word"].ToString());
		}

        m_currentWordData = selectedQuestion;

        UserStats.Activity.Current.AddWord(m_currentWordData);

        Resources.UnloadUnusedAssets();
        AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(selectedQuestion["word"].ToString());
        m_wordAudio = loadedAudio;

        HashSet<DataRow> allFour = new HashSet<DataRow>();
        allFour.Add(selectedQuestion);
        //while (allFour.Count < m_wordsToSpawnForDifficulty[SessionInformation.Instance.GetDifficulty()])
		while (allFour.Count < m_maxSpawn)
		{
            DataRow newWord = m_allWords[Random.Range(0, m_allWords.Count)];
            bool okToAdd = true;
            foreach (DataRow dr in allFour)
            {
				Texture2D dummyTex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + newWord["word"].ToString());;
                if (dr["word"].ToString() == newWord["word"].ToString() || dummyTex == null)
                {
                    okToAdd = false;
                }
				Resources.UnloadUnusedAssets();
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

            GameObject spawnedWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordButtonPrefab, locator.transform);
            spawnedWord.GetComponent<WordSelectionButton>().SetUp(wordRow == selectedQuestion,
                wordRow["word"].ToString(), this, m_wordsOffTransform);


            m_selectionButtons.Add(spawnedWord.GetComponent<WordSelectionButton>());
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

        m_imageBlackboard.ShowImage(wordImage, null, null, selectedQuestion["word"].ToString());

        if (SessionInformation.Instance.GetNumPlayers() != 2)
        {
            GetComponent<AudioSource>().clip = m_wordAudio;
            GetComponent<AudioSource>().Play();
        }

        m_remainingWords.Remove(selectedQuestion);
        m_currentWord = selectedQuestion["word"].ToString();
		Resources.UnloadUnusedAssets();
    }

    public void StartGame()
    {
		Debug.Log("Player StartGame()");
        m_allWords = PictureGameCoordinator.Instance.GetWordList();
		Debug.Log("m_allWords.Count: " + m_allWords.Count);
        m_remainingWords = new List<DataRow>();
        m_remainingWords.AddRange(m_allWords);
		Debug.Log("m_remainingWords.Count: " + m_remainingWords.Count);

		if(m_targetScore > m_allWords.Count)
		{
			m_targetScore = m_allWords.Count;
		}

		for(int i = m_allWords.Count - 1; i > -1; --i)
		{
			Debug.Log("word: " + m_allWords[i]["word"].ToString());
			Texture2D tex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + m_allWords[i]["word"].ToString());
			
			Debug.Log("tex: " + tex);
			if(tex != null)
			{
				++m_maxSpawn;
			}
			else
			{
				if(m_remainingWords.Contains(m_allWords[i]))
				{
					m_remainingWords.Remove(m_allWords[i]);
				}

				m_allWords.RemoveAt(i);
			}
		}

		/*
		foreach(DataRow word in m_allWords)
		{
			Debug.Log("word: " + word["word"].ToString());
			Texture2D tex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + word["word"].ToString());

			Debug.Log("tex: " + tex);
			if(tex != null)
			{
				++m_maxSpawn;
			}
			else
			{
				if(m_remainingWords.Contains(word))
				{
					m_remainingWords.Remove(word);
				}
				if(m_allWords.Contains(word))
				{
					m_allWords.Remove(word);
				}
			}
			Resources.UnloadUnusedAssets();
		}
		*/

		Resources.UnloadUnusedAssets();

		if(m_maxSpawn > m_wordsToSpawnForDifficulty[SessionInformation.Instance.GetDifficulty()])
		{
			m_maxSpawn = m_wordsToSpawnForDifficulty[SessionInformation.Instance.GetDifficulty()];
		}

		if(m_maxSpawn > m_allWords.Count)
		{
			m_maxSpawn = m_allWords.Count;
		}

		if(m_allWords.Count == 0)
		{
			PipHelpers.SetDefaultPlayerVar();
			PipHelpers.OnGameFinish();
		}

		Debug.Log("m_maxSpawn: " + m_maxSpawn);

        m_scoreBar.SetStarsTarget(m_targetScore);
        
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


    IEnumerator WordClickedCoroutine(bool correct, WordSelectionButton incoming)
    {
        UserStats.Activity.Current.IncrementNumAnswers();

        if (correct)
        {
            m_score++;
            m_scoreBar.SetScore(m_score);
            m_scoreBar.SetStarsCompleted(m_score);

            foreach (WordSelectionButton wsb in m_selectionButtons)
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

            if (SessionInformation.Instance.GetNumPlayers() != 2)
            {
                GetComponent<AudioSource>().clip = m_wordAudio;
                GetComponent<AudioSource>().Play();
            }

            yield return new WaitForSeconds(1.5f);

            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");
            WingroveAudio.WingroveRoot.Instance.PostEvent("SFX_SPARKLE");

            if (m_score != m_targetScore && m_remainingWords.Count > 0)
            {
                yield return new WaitForSeconds(1.0f);

                ShowNextQuestion();
            }
			else if(m_remainingWords.Count == 0)
			{
				PipHelpers.SetDefaultPlayerVar();
				PipHelpers.OnGameFinish();
			}
        }
        else
        {
            UserStats.Activity.Current.AddIncorrectWord(m_currentWordData);

            // remove lives
            m_numLives--;
            m_livesDisplay.SetLives(m_numLives);
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            WingroveAudio.WingroveRoot.Instance.PostEvent("NEGATIVE_HIT");
            if (SessionInformation.Instance.GetNumPlayers() != 2)
            {
                //PipPadBehaviour.Instance.Show(m_currentWord);
                PipPadBehaviour.Instance.SayAll(1.5f);
            }
        }
    }


    public void WordClicked(bool correct, WordSelectionButton incoming)
    {
        StartCoroutine(WordClickedCoroutine(correct, incoming));


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
