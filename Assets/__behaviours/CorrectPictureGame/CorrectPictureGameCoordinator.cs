using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorrectPictureGameCoordinator : Singleton<CorrectPictureGameCoordinator>
{

    [SerializeField]
    private int m_sectionId;
    [SerializeField]
    private int m_scoreTarget = 6;
    [SerializeField]
    private ProgressScoreBar m_progressScoreBar;
    [SerializeField]
    private CharacterPopper m_characterToPop;
    [SerializeField]
    private CharacterPopper m_characterToPopTroll;
    [SerializeField]
    private int[] m_wordsForDifficulty;

    private List<DataRow> m_wordSelection;

    private List<string> m_remainingWords = new List<string>();
    private List<string> m_allWords = new List<string>();

    int m_score = 0;
    bool m_gotIncorrect = false;

	int m_maxSpawn = 0;

    DataRow m_currentWordData = null;

    // Use this for initialization
    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		m_wordSelection = DataHelpers.GetWords();

        m_progressScoreBar.SetStarsTarget(m_scoreTarget);

		for(int i = m_wordSelection.Count - 1; i > -1; --i)
		{
			Texture2D tex = null;
			if(m_wordSelection[i]["image"] != null)
			{
				tex =(Texture2D)Resources.Load("Images/word_images_png_350/_" + m_wordSelection[i]["image"].ToString());
			}
			if(tex == null)
			{
				tex =(Texture2D)Resources.Load("Images/word_images_png_350/_" + m_wordSelection[i]["word"].ToString());
			}

			if(tex != null)
			{
				m_remainingWords.Add(m_wordSelection[i]["word"].ToString());
				m_allWords.Add(m_wordSelection[i]["word"].ToString());
				++m_maxSpawn;
			}
			else
			{
				m_wordSelection.Remove(m_wordSelection[i]);
			}
			
			Resources.UnloadUnusedAssets();
		}

		D.Log("Difficulty Spawn: " + m_wordsForDifficulty[SessionInformation.Instance.GetDifficulty()]);
		if(m_maxSpawn > m_wordsForDifficulty[SessionInformation.Instance.GetDifficulty()])
		{
			m_maxSpawn = m_wordsForDifficulty[SessionInformation.Instance.GetDifficulty()];
		}

		if(m_maxSpawn < 2)
		{
			m_maxSpawn = 2;
		}

		Resources.UnloadUnusedAssets();


		if(m_wordSelection.Count >= m_maxSpawn)
		{
        	StartCoroutine(ShowNextQuestion());
		}
		else
		{
			FinishGame();
		}
    }

    IEnumerator ShowNextQuestion()
    {
        yield return new WaitForSeconds(1.5f);

        m_gotIncorrect = false;
        string selectedWord = null;
        Texture2D targetTex = null;
		
        while(targetTex == null)
        {
            selectedWord = m_remainingWords[Random.Range(0, m_remainingWords.Count)];
            targetTex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + selectedWord);
        }

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words WHERE word='" + selectedWord + "'");

        if (dt.Rows.Count > 0)
        {
            m_currentWordData = dt.Rows[0];
            //UserStats.Activity.AddWord(m_currentWordData);
        }

        List<string> otherWordList = new List<string>();

        while (otherWordList.Count < m_maxSpawn - 1)
        {			
            string otherWord = m_allWords[Random.Range(0, m_allWords.Count)];
			
            Texture2D tex = null;

            while (otherWord == selectedWord || tex == null)
            {
                otherWord = m_allWords[Random.Range(0, m_allWords.Count)];
                tex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + otherWord);
            }

            if ( !otherWordList.Contains(otherWord) )
            {
                otherWordList.Add(otherWord);
            }
        }

        PipPadBehaviour.Instance.Show(selectedWord);

        if (m_maxSpawn == 2)
        {
            if (Random.Range(0, 10) > 5)
            {
                PipPadBehaviour.Instance.ShowMultipleBlackboards(selectedWord, otherWordList[0], null);
            }
            else
            {
                PipPadBehaviour.Instance.ShowMultipleBlackboards(otherWordList[0], selectedWord, null);
            }
        }
        else
        {
            int correctIndex = Random.Range(0, 2);

            if (correctIndex == 0)
            {
                PipPadBehaviour.Instance.ShowMultipleBlackboards(selectedWord, otherWordList[0], otherWordList[1]);
            }
            else if (correctIndex == 1)
            {
                PipPadBehaviour.Instance.ShowMultipleBlackboards(otherWordList[0], selectedWord, otherWordList[1]);
            }
            else
            {
                PipPadBehaviour.Instance.ShowMultipleBlackboards(otherWordList[0], otherWordList[1], selectedWord);
            }
        }

        yield break;
    }

    IEnumerator OnCorrectClick()
    {
        if (!m_gotIncorrect)
        {
            m_score++;
            PipPadBehaviour.Instance.SayWholeWord();
            yield return new WaitForSeconds(1.5f);
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");
            m_characterToPop.PopCharacter();
        }
        else
        {
            PipPadBehaviour.Instance.SayAll(0.5f);
            yield return new WaitForSeconds(4.0f);
        }
        PipPadBehaviour.Instance.Hide();

        m_progressScoreBar.SetStarsCompleted(m_score);
        m_progressScoreBar.SetScore(m_score);
        if (m_score < m_scoreTarget)
        {
            StartCoroutine(ShowNextQuestion());
        }
        else
        {
			FinishGame();
        }
    }

	void FinishGame()
	{
		SessionInformation.SetDefaultPlayerVar();
		GameManager.Instance.CompleteGame();
	}

    public void WordClicked(int index, ImageBlackboard clickedBlackboard)
    {
        //UserStats.Activity.IncrementNumAnswers();
        if(m_currentWordData["word"].ToString().ToLower() == clickedBlackboard.GetImageName().Replace("_", "").ToLower())
        {
            StopAllCoroutines();
            StartCoroutine(OnCorrectClick());
        }
        else
        {
            if(m_currentWordData != null)
            {
                //UserStats.Activity.AddIncorrectWord(m_currentWordData);
            }

            m_gotIncorrect = true;
            m_characterToPopTroll.PopCharacter();
            clickedBlackboard.ShakeFade();
            StopAllCoroutines();
            StartCoroutine(StartIncorrect());
        }
    }

    IEnumerator StartIncorrect()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_MINOR");
        yield return new WaitForSeconds(0.5f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("PRESS_BLUE_BUTTONS");

        yield return new WaitForSeconds(1.0f);
    }
}
